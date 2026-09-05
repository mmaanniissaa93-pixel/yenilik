using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Common;
using xBot.Network;
using SecurityAPI;

namespace xBot.App.CommandCenter
{
    public class CommandOption
    {
        public string Key { get; }
        public string DisplayName { get; }

        public CommandOption(string key, string displayName)
        {
            Key = key;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }

    public class EmoteDefinition
    {
        public EmoteType Type { get; }
        public string Name { get; }
        public string DefaultCommand { get; }
        public string IconName { get; }

        public EmoteDefinition(EmoteType type, string name, string defaultCommand, string iconName)
        {
            Type = type;
            Name = name;
            DefaultCommand = defaultCommand;
            IconName = iconName;
        }
    }

    public class ChatCommandDefinition
    {
        public string Trigger { get; }
        public string CommandKey { get; }
        public string Description { get; }

        public ChatCommandDefinition(string trigger, string commandKey, string description)
        {
            Trigger = trigger;
            CommandKey = commandKey;
            Description = description;
        }
    }

    public static class CommandCenterManager
    {
        public static bool Enabled { get; set; } = true;

        public static readonly List<CommandOption> AvailableCommands = new List<CommandOption>
        {
            new CommandOption("none", "No action"),
            new CommandOption("buff", "Cast all buffs"),
            new CommandOption("area", "Set the training area"),
            new CommandOption("here", "Set training area and start bot"),
            new CommandOption("show", "Show the bot window"),
            new CommandOption("start", "Start the bot"),
            new CommandOption("stop", "Stop the bot")
        };

        public static readonly List<EmoteDefinition> Emotes = new List<EmoteDefinition>
        {
            new EmoteDefinition(EmoteType.No, "No", "stop", "no.png"),
            new EmoteDefinition(EmoteType.Joy, "Joy", "none", "joy.png"),
            new EmoteDefinition(EmoteType.Rush, "Rush", "area", "rush.png"),
            new EmoteDefinition(EmoteType.Yes, "Yes", "start", "yes.png"),
            new EmoteDefinition(EmoteType.Greeting, "Greeting", "area", "greeting.png"),
            new EmoteDefinition(EmoteType.Smile, "Smile", "show", "smile.png"),
            new EmoteDefinition(EmoteType.Hi, "Hi", "none", "hi.png")
        };

        public static readonly List<ChatCommandDefinition> ChatCommands = new List<ChatCommandDefinition>
        {
            new ChatCommandDefinition("\\area", "area", "Set the training area"),
            new ChatCommandDefinition("\\buff", "buff", "Cast all buffs"),
            new ChatCommandDefinition("\\show", "show", "Show the bot window"),
            new ChatCommandDefinition("\\start", "start", "Start the bot"),
            new ChatCommandDefinition("\\here", "here", "Set training area and start bot"),
            new ChatCommandDefinition("\\stop", "stop", "Stop the bot")
        };

        private static readonly Dictionary<string, string> _assignedEmoteActions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        static CommandCenterManager()
        {
            ResetToDefaults();
        }

        public static void ResetToDefaults()
        {
            _assignedEmoteActions.Clear();
            foreach (var emote in Emotes)
            {
                _assignedEmoteActions[emote.Name] = emote.DefaultCommand;
            }
        }

        public static string GetAssignedCommand(string emoteName)
        {
            if (_assignedEmoteActions.TryGetValue(emoteName, out string cmd))
                return cmd;

            foreach (var def in Emotes)
            {
                if (string.Equals(def.Name, emoteName, StringComparison.OrdinalIgnoreCase))
                    return def.DefaultCommand;
            }
            return "none";
        }

        public static void SetAssignedCommand(string emoteName, string commandKey)
        {
            _assignedEmoteActions[emoteName] = commandKey ?? "none";
        }

        public static Dictionary<string, string> GetAllAssignedCommands()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var kv in _assignedEmoteActions)
                dict[kv.Key] = kv.Value;
            return dict;
        }

        public static void SendInGameNotice(string message)
        {
            try
            {
                if (Bot.Get?.Proxy?.Agent != null && !Bot.Get.Proxy.ClientlessMode)
                {
                    Packet p = new Packet(Agent.Opcode.SERVER_CHAT_UPDATE);
                    p.WriteByte(SRTypes.Chat.Notice);
                    p.WriteAscii(message);
                    Bot.Get.Proxy.Agent.InjectToClient(p);
                }
            }
            catch
            {
                // Ignore notice injection errors
            }
        }

        public static void OnEmoteUsed(byte emoteByte)
        {
            if (!Enabled)
                return;

            if (Enum.IsDefined(typeof(EmoteType), emoteByte))
            {
                EmoteType type = (EmoteType)emoteByte;
                string emoteName = type.ToString();
                string command = GetAssignedCommand(emoteName);

                if (!string.IsNullOrEmpty(command) && !string.Equals(command, "none", StringComparison.OrdinalIgnoreCase))
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        ExecuteCommand(command);
                    });
                }
            }
        }

        public static bool HandleChatCommand(string message)
        {
            if (!Enabled || string.IsNullOrWhiteSpace(message))
                return false;

            string trimmed = message.Trim();
            if (!trimmed.StartsWith("\\"))
                return false;

            string cmdName = trimmed.Substring(1).Trim().ToLowerInvariant();
            foreach (var chatDef in ChatCommands)
            {
                string triggerName = chatDef.Trigger.TrimStart('\\').ToLowerInvariant();
                if (cmdName == triggerName)
                {
                    ThreadPool.QueueUserWorkItem(_ =>
                    {
                        ExecuteCommand(chatDef.CommandKey);
                    });
                    return true;
                }
            }

            return false;
        }

        public static bool ExecuteCommand(string commandKey, bool silent = false)
        {
            if (string.IsNullOrWhiteSpace(commandKey) || string.Equals(commandKey, "none", StringComparison.OrdinalIgnoreCase))
                return true;

            Window w = Window.Get;
            string key = commandKey.Trim().ToLowerInvariant();

            switch (key)
            {
                case "start":
                    if (!silent)
                        SendInGameNotice("[xBot] Starting bot");
                    w?.Log("[Command Center] Starting bot...");
                    Bot.Get?.Start();
                    return true;

                case "stop":
                    if (!silent)
                        SendInGameNotice("[xBot] Stopping bot");
                    w?.Log("[Command Center] Stopping bot...");
                    Bot.Get?.Stop();
                    return true;

                case "area":
                    return ExecuteSetTrainingArea(silent);

                case "here":
                    if (!silent)
                        SendInGameNotice("[xBot] Starting bot at current location");
                    bool areaOk = ExecuteSetTrainingArea(silent: true);
                    Thread.Sleep(250);
                    Bot.Get?.Start();
                    return areaOk;

                case "buff":
                    if (!silent)
                        SendInGameNotice("[xBot] Casting all buffs");
                    w?.Log("[Command Center] Casting all buffs...");
                    w?.CastAllBuffs();
                    return true;

                case "show":
                    if (!silent)
                        SendInGameNotice("[xBot] Showing bot window");
                    w?.Log("[Command Center] Bringing bot window to front...");
                    if (w != null)
                    {
                        WinAPI.InvokeIfRequired(w, () =>
                        {
                            if (w.WindowState == FormWindowState.Minimized)
                                w.WindowState = FormWindowState.Normal;
                            w.Show();
                            w.Activate();
                            w.BringToFront();
                        });
                    }
                    return true;
            }

            return false;
        }

        private static bool ExecuteSetTrainingArea(bool silent)
        {
            Window w = Window.Get;
            if (InfoManager.Character == null)
            {
                w?.Log("[Command Center] Character not loaded, cannot set training area.");
                return false;
            }

            SRCoord currentPos = InfoManager.Character.GetRealtimePosition();
            if (currentPos == null)
            {
                w?.Log("[Command Center] Current position is unavailable.");
                return false;
            }

            if (!silent)
                SendInGameNotice($"[xBot] Setting training area to X={currentPos.X:0.0} Y={currentPos.Y:0.0} R=50");

            w?.Log($"[Command Center] Training area updated to Region={currentPos.Region}, X={currentPos.X}, Y={currentPos.Y}, Radius=50");

            if (w != null)
            {
                WinAPI.InvokeIfRequired(w, () =>
                {
                    w.TrainingArea_SetFromCurrent(currentPos);
                });
            }

            return true;
        }

        public static void LoadSettings(JObject json)
        {
            if (json == null || !json.ContainsKey("CommandCenter"))
                return;

            try
            {
                JObject cc = (JObject)json["CommandCenter"];
                if (cc.ContainsKey("Enabled"))
                    Enabled = (bool)cc["Enabled"];

                if (cc.ContainsKey("EmoteActions") && cc["EmoteActions"] is JObject actionsObj)
                {
                    foreach (var prop in actionsObj.Properties())
                    {
                        SetAssignedCommand(prop.Name, prop.Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Window.Get?.Log("[Command Center] Error loading settings: " + ex.Message);
            }
        }

        public static void SaveSettings(JObject json)
        {
            if (json == null) return;

            try
            {
                JObject cc = new JObject
                {
                    ["Enabled"] = Enabled
                };

                JObject actions = new JObject();
                foreach (var emote in Emotes)
                {
                    actions[emote.Name] = GetAssignedCommand(emote.Name);
                }
                cc["EmoteActions"] = actions;

                json["CommandCenter"] = cc;
            }
            catch (Exception ex)
            {
                Window.Get?.Log("[Command Center] Error saving settings: " + ex.Message);
            }
        }

        public static Image GetEmoteIcon(string iconName)
        {
            try
            {
                string asmLoc = "";
                try { asmLoc = typeof(CommandCenterManager).Assembly.Location; } catch { }
                string asmDir = !string.IsNullOrEmpty(asmLoc) ? Path.GetDirectoryName(asmLoc) : "";
                string baseDir = AppDomain.CurrentDomain.BaseDirectory ?? "";
                string currentDir = Directory.GetCurrentDirectory();

                string[] candidateDirs = new[]
                {
                    Path.Combine(asmDir, "Resources", "Emotes"),
                    Path.Combine(baseDir, "Resources", "Emotes"),
                    Path.Combine(currentDir, "Resources", "Emotes"),
                    Path.Combine(currentDir, "xBot", "Resources", "Emotes"),
                    @"c:\Users\auguu\Desktop\xBot-WinForms\xBot\Resources\Emotes",
                    Path.Combine(baseDir, "..", "..", "xBot", "Resources", "Emotes"),
                    Path.Combine(baseDir, "..", "xBot", "Resources", "Emotes")
                };

                foreach (string dir in candidateDirs)
                {
                    if (string.IsNullOrEmpty(dir)) continue;
                    string fullPath = Path.Combine(dir, iconName);
                    if (File.Exists(fullPath))
                    {
                        using (var temp = Image.FromFile(fullPath))
                        {
                            return new Bitmap(temp);
                        }
                    }
                }
            }
            catch
            {
                // Fallback
            }
            return new Bitmap(32, 32);
        }
    }
}
