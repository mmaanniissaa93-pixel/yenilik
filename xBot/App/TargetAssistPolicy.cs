using System;
using System.Collections.Generic;
using System.Linq;

namespace xBot.App
{
    public enum TargetAssistRoleMode
    {
        Civil = 0,
        Thief = 1,
        HunterTrader = 2
    }

    public class TargetCandidateData
    {
        public uint UniqueId { get; set; }
        public string Name { get; set; } = string.Empty;
        public double Distance { get; set; }
        public string GuildName { get; set; } = string.Empty;
        public bool IsDead { get; set; }
        public bool HasSnowShield { get; set; }
        public bool HasBloodyStorm { get; set; }
        public bool HasJobMode { get; set; }
        public byte JobType { get; set; } // 0: None, 1: Trader, 2: Thief, 3: Hunter
    }

    public class TargetAssistSettings
    {
        public bool Enabled { get; set; } = false;
        public double MaxRange { get; set; } = 40.0;
        public TargetAssistRoleMode RoleMode { get; set; } = TargetAssistRoleMode.Civil;
        public string TargetCycleKey { get; set; } = "Oem3";
        public bool IncludeDeadTargets { get; set; } = false;
        public bool IgnoreBloodyStormTargets { get; set; } = false;
        public bool IgnoreSnowShieldTargets { get; set; } = true;
        public bool OnlyCustomPlayers { get; set; } = false;
        public HashSet<string> IgnoredGuilds { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        public HashSet<string> CustomPlayers { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }

    public static class TargetAssistPolicy
    {
        public const uint EffectTransferParam = 1701213281; // AUTO_TRANSFER ("efta")

        public static bool IsValidTarget(TargetCandidateData candidate, TargetAssistSettings settings, uint selfUniqueId)
        {
            if (candidate == null || settings == null)
                return false;

            if (candidate.UniqueId == 0 || candidate.UniqueId == selfUniqueId)
                return false;

            if (string.IsNullOrWhiteSpace(candidate.Name))
                return false;

            if (!settings.IncludeDeadTargets && candidate.IsDead)
                return false;

            if (candidate.Distance > settings.MaxRange)
                return false;

            if (settings.IgnoreSnowShieldTargets && candidate.HasSnowShield)
                return false;

            if (settings.IgnoreBloodyStormTargets && candidate.HasBloodyStorm)
                return false;

            if (IsIgnoredGuild(candidate.GuildName, settings.IgnoredGuilds))
                return false;

            if (settings.OnlyCustomPlayers && !IsCustomPlayer(candidate.Name, settings.CustomPlayers))
                return false;

            if (!MatchesRoleMode(candidate, settings.RoleMode))
                return false;

            return true;
        }

        public static bool IsIgnoredGuild(string guildName, HashSet<string> ignoredGuilds)
        {
            if (string.IsNullOrWhiteSpace(guildName) || ignoredGuilds == null || ignoredGuilds.Count == 0)
                return false;

            return ignoredGuilds.Contains(guildName.Trim());
        }

        public static bool IsCustomPlayer(string playerName, HashSet<string> customPlayers)
        {
            if (string.IsNullOrWhiteSpace(playerName) || customPlayers == null || customPlayers.Count == 0)
                return false;

            return customPlayers.Contains(playerName.Trim());
        }

        public static bool MatchesRoleMode(TargetCandidateData candidate, TargetAssistRoleMode roleMode)
        {
            if (candidate == null) return false;

            switch (roleMode)
            {
                case TargetAssistRoleMode.Thief:
                    return candidate.HasJobMode && (candidate.JobType == 1 || candidate.JobType == 3);
                case TargetAssistRoleMode.HunterTrader:
                    return candidate.HasJobMode && candidate.JobType == 2;
                case TargetAssistRoleMode.Civil:
                default:
                    return true;
            }
        }

        public static TargetCandidateData ResolveNextTarget(IReadOnlyList<TargetCandidateData> orderedCandidates, uint currentSelectedId)
        {
            if (orderedCandidates == null || orderedCandidates.Count == 0)
                return null;

            if (currentSelectedId == 0)
                return orderedCandidates[0];

            int selectedIndex = -1;
            for (int i = 0; i < orderedCandidates.Count; i++)
            {
                if (orderedCandidates[i].UniqueId == currentSelectedId)
                {
                    selectedIndex = i;
                    break;
                }
            }

            if (selectedIndex < 0)
                return orderedCandidates[0];

            int nextIndex = (selectedIndex + 1) % orderedCandidates.Count;
            return orderedCandidates[nextIndex];
        }

        public static bool HasSnowShield(string buffServerName, uint effectParam = 0)
        {
            if (effectParam == EffectTransferParam)
                return true;

            if (!string.IsNullOrWhiteSpace(buffServerName) && buffServerName.IndexOf("COLD_SHIELD", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;

            return false;
        }

        public static bool HasBloodyStorm(string buffServerName)
        {
            if (string.IsNullOrWhiteSpace(buffServerName))
                return false;

            return buffServerName.IndexOf("FANSTORM", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   buffServerName.IndexOf("FAN_STORM", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static string NormalizeKeyName(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return "Oem3";

            return key.Trim();
        }

        public static string FormatCandidateStatus(int count, string nearestName, double nearestDistance)
        {
            if (count <= 0)
                return "No target candidates in range.";

            string distStr = nearestDistance >= 0
                ? string.Format(System.Globalization.CultureInfo.InvariantCulture, " ({0:0.0}m)", nearestDistance)
                : string.Empty;
            return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Candidates: {0}  |  Nearest: {1}{2}", count, nearestName, distStr);
        }
    }
}
