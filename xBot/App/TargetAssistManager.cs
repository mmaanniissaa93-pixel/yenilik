using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;

namespace xBot.App
{
    public static class TargetAssistManager
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private static readonly TargetAssistSettings _settings = new TargetAssistSettings();
        private static bool _initialized = false;
        private static Timer _tickTimer;
        private static bool _keyWasDown = false;
        private static int _lastRetargetTick = 0;
        private static uint _lastSelectedTargetId = 0;
        private static int s_running;

        public static TargetAssistSettings Settings => _settings;

        public static bool Enabled
        {
            get => _settings.Enabled;
            set => _settings.Enabled = value;
        }

        public static double MaxRange
        {
            get => _settings.MaxRange;
            set => _settings.MaxRange = Math.Max(5.0, Math.Min(400.0, value));
        }

        public static TargetAssistRoleMode RoleMode
        {
            get => _settings.RoleMode;
            set => _settings.RoleMode = value;
        }

        public static string TargetCycleKey
        {
            get => _settings.TargetCycleKey;
            set => _settings.TargetCycleKey = TargetAssistPolicy.NormalizeKeyName(value);
        }

        public static bool IncludeDeadTargets
        {
            get => _settings.IncludeDeadTargets;
            set => _settings.IncludeDeadTargets = value;
        }

        public static bool IgnoreSnowShieldTargets
        {
            get => _settings.IgnoreSnowShieldTargets;
            set => _settings.IgnoreSnowShieldTargets = value;
        }

        public static bool IgnoreBloodyStormTargets
        {
            get => _settings.IgnoreBloodyStormTargets;
            set => _settings.IgnoreBloodyStormTargets = value;
        }

        public static bool OnlyCustomPlayers
        {
            get => _settings.OnlyCustomPlayers;
            set => _settings.OnlyCustomPlayers = value;
        }

        public static HashSet<string> IgnoredGuilds => _settings.IgnoredGuilds;
        public static HashSet<string> CustomPlayers => _settings.CustomPlayers;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;

            _tickTimer = new Timer();
            _tickTimer.Interval = 100; // 10Hz hotkey polling yeterli (40ms CPU israfıydı)
            _tickTimer.Tick += (s, e) => RunTick();
            _tickTimer.Start();
        }

        public static void RunTick()
        {
            // Timer + Bot.IA çift çağrısına karşı reentrancy koruması (tek kaynak: timer)
            if (System.Threading.Interlocked.Exchange(ref s_running, 1) == 1)
                return;
            try
            {
                RunTickInner();
            }
            finally
            {
                System.Threading.Interlocked.Exchange(ref s_running, 0);
            }
        }
        private static void RunTickInner()
        {
            if (!_settings.Enabled)
            {
                _keyWasDown = false;
                return;
            }

            if (InfoManager.Character == null || InfoManager.Character.LifeStateType != SRModel.LifeState.Alive)
            {
                _keyWasDown = false;
                return;
            }

            Keys key = ParseKey(_settings.TargetCycleKey);
            short keyState = GetAsyncKeyState((int)key);
            bool isKeyDown = (keyState & 0x8000) != 0;

            if (!isKeyDown)
            {
                _keyWasDown = false;
                return;
            }

            if (_keyWasDown)
                return;

            _keyWasDown = true;

            int now = Environment.TickCount;
            if (Math.Abs(now - _lastRetargetTick) < 350)
                return;

            _lastRetargetTick = now;
            CycleTarget();
        }

        public static bool CycleTarget()
        {
            try
            {
                if (InfoManager.Character == null) return false;

                var candidates = CollectCandidates();
                if (candidates.Count == 0)
                    return false;

                var nextTarget = TargetAssistPolicy.ResolveNextTarget(candidates, _lastSelectedTargetId);
                if (nextTarget == null)
                    return false;

                _lastSelectedTargetId = nextTarget.UniqueId;
                PacketBuilder.SelectEntity(nextTarget.UniqueId);

                Window.Get?.LogProcess(string.Format("[TargetAssist] Hedef seçildi: {0} ({1:0.0}m)", nextTarget.Name, nextTarget.Distance));
                return true;
            }
            catch (Exception ex)
            {
                Window.Get?.Log("[TargetAssist Error] " + ex.Message);
                return false;
            }
        }

        public static List<TargetCandidateData> CollectCandidates()
        {
            var list = new List<TargetCandidateData>();
            if (InfoManager.Character == null || InfoManager.Players == null)
                return list;

            uint selfId = InfoManager.Character.UniqueID;
            SRCoord selfPos = InfoManager.Character.GetRealtimePosition();
            if (selfPos == null) return list;

            int count = InfoManager.Players.Count;
            for (int i = 0; i < count; i++)
            {
                SRPlayer player = InfoManager.Players.GetAt(i);
                if (player == null || player.UniqueID == selfId)
                    continue;

                SRCoord playerPos = player.GetRealtimePosition();
                double dist = playerPos != null ? selfPos.DistanceTo(playerPos) : double.MaxValue;

                bool isDead = player.LifeStateType == SRModel.LifeState.Dead;
                bool hasSnow = false;
                bool hasBloody = false;

                if (player.Buffs != null)
                {
                    int buffCount = player.Buffs.Count;
                    for (int b = 0; b < buffCount; b++)
                    {
                        SRBuff buff = player.Buffs.GetAt(b);
                        if (buff == null) continue;

                        if (!hasSnow && (buff.hasAutoTransferEffect() || TargetAssistPolicy.HasSnowShield(buff.ServerName)))
                            hasSnow = true;

                        if (!hasBloody && TargetAssistPolicy.HasBloodyStorm(buff.ServerName))
                            hasBloody = true;
                    }
                }

                var cand = new TargetCandidateData
                {
                    UniqueId = player.UniqueID,
                    Name = player.Name ?? string.Empty,
                    Distance = dist,
                    GuildName = player.GuildName ?? string.Empty,
                    IsDead = isDead,
                    HasSnowShield = hasSnow,
                    HasBloodyStorm = hasBloody,
                    HasJobMode = player.hasJobMode(),
                    JobType = (byte)player.JobType
                };

                if (TargetAssistPolicy.IsValidTarget(cand, _settings, selfId))
                {
                    list.Add(cand);
                }
            }

            list.Sort((a, b) => a.Distance.CompareTo(b.Distance));
            return list;
        }

        public static (int count, string nearestName, double nearestDistance) GetStatusInfo()
        {
            var candidates = CollectCandidates();
            if (candidates.Count == 0)
                return (0, string.Empty, -1);

            return (candidates.Count, candidates[0].Name, candidates[0].Distance);
        }

        private static Keys ParseKey(string keyName)
        {
            if (string.IsNullOrWhiteSpace(keyName))
                return Keys.Oem3;

            if (Enum.TryParse<Keys>(keyName.Trim(), true, out Keys parsed) && parsed != Keys.None)
                return parsed;

            return Keys.Oem3;
        }

        public static JObject ToJson()
        {
            var obj = new JObject
            {
                ["Enabled"] = _settings.Enabled,
                ["MaxRange"] = _settings.MaxRange,
                ["RoleMode"] = _settings.RoleMode.ToString(),
                ["TargetCycleKey"] = _settings.TargetCycleKey,
                ["IncludeDeadTargets"] = _settings.IncludeDeadTargets,
                ["IgnoreSnowShieldTargets"] = _settings.IgnoreSnowShieldTargets,
                ["IgnoreBloodyStormTargets"] = _settings.IgnoreBloodyStormTargets,
                ["OnlyCustomPlayers"] = _settings.OnlyCustomPlayers
            };

            var guilds = new JArray();
            foreach (var g in _settings.IgnoredGuilds)
                guilds.Add(g);
            obj["IgnoredGuilds"] = guilds;

            var players = new JArray();
            foreach (var p in _settings.CustomPlayers)
                players.Add(p);
            obj["CustomPlayers"] = players;

            return obj;
        }

        public static void FromJson(JObject obj)
        {
            if (obj == null) return;

            if (obj.TryGetValue("Enabled", out JToken enabledToken))
                _settings.Enabled = enabledToken.Value<bool>();

            if (obj.TryGetValue("MaxRange", out JToken maxRangeToken))
                _settings.MaxRange = Math.Max(5.0, Math.Min(400.0, maxRangeToken.Value<double>()));

            if (obj.TryGetValue("RoleMode", out JToken roleToken))
            {
                string roleStr = roleToken.Value<string>();
                if (Enum.TryParse<TargetAssistRoleMode>(roleStr, true, out var parsedRole))
                    _settings.RoleMode = parsedRole;
            }

            if (obj.TryGetValue("TargetCycleKey", out JToken keyToken))
                _settings.TargetCycleKey = TargetAssistPolicy.NormalizeKeyName(keyToken.Value<string>());

            if (obj.TryGetValue("IncludeDeadTargets", out JToken deadToken))
                _settings.IncludeDeadTargets = deadToken.Value<bool>();

            if (obj.TryGetValue("IgnoreSnowShieldTargets", out JToken snowToken))
                _settings.IgnoreSnowShieldTargets = snowToken.Value<bool>();

            if (obj.TryGetValue("IgnoreBloodyStormTargets", out JToken bloodyToken))
                _settings.IgnoreBloodyStormTargets = bloodyToken.Value<bool>();

            if (obj.TryGetValue("OnlyCustomPlayers", out JToken customToken))
                _settings.OnlyCustomPlayers = customToken.Value<bool>();

            if (obj.TryGetValue("IgnoredGuilds", out JToken guildsToken) && guildsToken is JArray guildsArr)
            {
                _settings.IgnoredGuilds.Clear();
                foreach (var g in guildsArr)
                {
                    string str = g.Value<string>();
                    if (!string.IsNullOrWhiteSpace(str))
                        _settings.IgnoredGuilds.Add(str.Trim());
                }
            }

            if (obj.TryGetValue("CustomPlayers", out JToken playersToken) && playersToken is JArray playersArr)
            {
                _settings.CustomPlayers.Clear();
                foreach (var p in playersArr)
                {
                    string str = p.Value<string>();
                    if (!string.IsNullOrWhiteSpace(str))
                        _settings.CustomPlayers.Add(str.Trim());
                }
            }
        }
    }
}
