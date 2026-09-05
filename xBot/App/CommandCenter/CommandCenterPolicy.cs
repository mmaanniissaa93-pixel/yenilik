using System;
using System.Collections.Generic;

namespace xBot.App.CommandCenter
{
    public enum EmoteType : byte
    {
        Hi = 0,
        Greeting = 1,
        Rush = 2,
        Joy = 3,
        No = 4,
        Yes = 5,
        Smile = 6
    }

    public static class CommandCenterPolicy
    {
        public static readonly string[] ValidCommands = new[]
        {
            "none",
            "buff",
            "area",
            "here",
            "show",
            "start",
            "stop"
        };

        public static string ResolveDefaultCommandForEmote(EmoteType type)
        {
            switch (type)
            {
                case EmoteType.No:
                    return "stop";
                case EmoteType.Joy:
                    return "none";
                case EmoteType.Rush:
                    return "area";
                case EmoteType.Yes:
                    return "start";
                case EmoteType.Greeting:
                    return "area";
                case EmoteType.Smile:
                    return "show";
                case EmoteType.Hi:
                    return "none";
                default:
                    return "none";
            }
        }

        public static bool IsValidCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return false;

            string normalized = command.Trim().ToLowerInvariant();
            for (int i = 0; i < ValidCommands.Length; i++)
            {
                if (ValidCommands[i] == normalized)
                    return true;
            }
            return false;
        }

        public static string ResolveAssignedEmoteCommand(EmoteType type, IDictionary<string, string> mappings)
        {
            string emoteName = type.ToString();
            if (mappings != null && mappings.TryGetValue(emoteName, out string assigned))
            {
                if (!string.IsNullOrWhiteSpace(assigned) && IsValidCommand(assigned))
                    return assigned.Trim().ToLowerInvariant();
            }
            return ResolveDefaultCommandForEmote(type);
        }

        public static bool ParseChatCommand(string message, out string commandKey)
        {
            commandKey = null;
            if (string.IsNullOrWhiteSpace(message))
                return false;

            string trimmed = message.Trim();
            if (!trimmed.StartsWith("\\"))
                return false;

            string cmd = trimmed.Substring(1).Trim().ToLowerInvariant();
            if (IsValidCommand(cmd) && cmd != "none")
            {
                commandKey = cmd;
                return true;
            }

            return false;
        }
    }
}
