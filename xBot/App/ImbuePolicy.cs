using System;

namespace xBot.App
{
    /// <summary>
    /// Recognizes Chinese imbue skill/buff names across the common SRO
    /// server-name variants (GIGONGTA, ENCHANT and IMBU prefixes).
    /// </summary>
    public static class ImbuePolicy
    {
        public static bool IsChineseImbueSkill(string serverName)
        {
            return GetElement(serverName) != "None";
        }

        public static string GetElement(string serverName)
        {
            if (string.IsNullOrWhiteSpace(serverName))
                return "None";

            string name = serverName.ToUpperInvariant();
            bool hasImbueMarker = name.Contains("GIGONGTA")
                || name.Contains("ENCHANT")
                || name.Contains("IMBU");
            if (!hasImbueMarker)
                return "None";

            if (ContainsElement(name, "FIRE"))
                return "Fire";
            if (ContainsElement(name, "COLD"))
                return "Cold";
            if (ContainsElement(name, "LIGHTNING") || ContainsElement(name, "LIGHT"))
                return "Lightning";

            return "None";
        }

        public static bool IsImbueSkill(string serverName, string element)
        {
            if (string.IsNullOrWhiteSpace(serverName) || !IsSupportedElement(element))
                return false;

            return string.Equals(GetElement(serverName), element, StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsActiveImbue(string serverName, string element)
        {
            if (!IsSupportedElement(element) || string.IsNullOrWhiteSpace(serverName))
                return false;

            string name = serverName.ToUpperInvariant();
            string token = GetElementToken(element);
            bool hasElement = ContainsElement(name, token);
            bool hasImbueMarker = name.Contains("GIGONGTA")
                || name.Contains("ENCHANT")
                || name.Contains("IMBU")
                || name.Contains("BUFF");

            return hasElement && hasImbueMarker;
        }

        private static bool IsSupportedElement(string element)
        {
            return string.Equals(element, "Fire", StringComparison.OrdinalIgnoreCase)
                || string.Equals(element, "Cold", StringComparison.OrdinalIgnoreCase)
                || string.Equals(element, "Lightning", StringComparison.OrdinalIgnoreCase);
        }

        private static string GetElementToken(string element)
        {
            if (string.Equals(element, "Lightning", StringComparison.OrdinalIgnoreCase))
                return "LIGHTNING";
            return element.ToUpperInvariant();
        }

        private static bool ContainsElement(string name, string token)
        {
            if (token == "LIGHTNING")
            {
                return name.Contains("_LIGHTNING")
                    || name.Contains("LIGHTNING_")
                    || name.Contains("_LIGHT_")
                    || name.Contains("LIGHT_");
            }

            return name.Contains("_" + token)
                || name.Contains(token + "_");
        }
    }
}
