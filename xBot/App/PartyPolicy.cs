using System;

namespace xBot.App
{
    /// <summary>
    /// Pure decisions for party support, healing, resurrect, and weapon revert.
    /// Free of UI or network dependencies for reliable unit testing.
    /// </summary>
    public static class PartyPolicy
    {
        public static bool ShouldHealPartyMember(byte currentHpPercent, byte thresholdPercent)
        {
            if (currentHpPercent == 0 || thresholdPercent == 0)
                return false;

            return currentHpPercent <= thresholdPercent;
        }

        public static bool ShouldResurrectMember(byte currentHpPercent, bool isNearby)
        {
            return currentHpPercent == 0 && isNearby;
        }

        public static bool ShouldAcceptRess(bool acceptEnabled, bool partyOnly, bool requesterInParty)
        {
            if (!acceptEnabled)
                return false;

            if (partyOnly && !requesterInParty)
                return false;

            return true;
        }

        public static bool ShouldRevertWeapon(int currentWeapon, int mainWeapon)
        {
            if (mainWeapon <= 0)
                return false;

            return currentWeapon > 0 && currentWeapon != mainWeapon;
        }

        public static bool IsClericHealingSkill(string serverName)
        {
            if (string.IsNullOrEmpty(serverName))
                return false;

            string upper = serverName.ToUpperInvariant();
            return upper.Contains("CLERIC") && (
                upper.Contains("HEAL") ||
                upper.Contains("RECOVERY") ||
                upper.Contains("DIVISION") ||
                upper.Contains("ORBIT") ||
                upper.Contains("GROUP") ||
                upper.Contains("CYCLE"));
        }

        public static bool IsClericRessSkill(string serverName)
        {
            if (string.IsNullOrEmpty(serverName))
                return false;

            string upper = serverName.ToUpperInvariant();
            return upper.Contains("CLERIC") && (
                upper.Contains("RESURRECTION") ||
                upper.Contains("REBIRTH") ||
                upper.Contains("GRAND_REBIRTH"));
        }

        public static bool IsClericCureSkill(string serverName)
        {
            if (string.IsNullOrEmpty(serverName))
                return false;

            string upper = serverName.ToUpperInvariant();
            return upper.Contains("CLERIC") && (
                upper.Contains("PURIFY") ||
                upper.Contains("INTEGRITY") ||
                upper.Contains("CURE"));
        }

        public static bool IsPartyBuffSkill(string serverName)
        {
            if (string.IsNullOrEmpty(serverName))
                return false;

            string upper = serverName.ToUpperInvariant();
            return (upper.Contains("BARD") && (upper.Contains("SPEED") || upper.Contains("GUARD") || upper.Contains("MANA") || upper.Contains("DANCE")))
                || (upper.Contains("WARRIOR") && (upper.Contains("PAIN") || upper.Contains("FENCE") || upper.Contains("SCREEN")))
                || (upper.Contains("CLERIC") && (upper.Contains("BLESS") || upper.Contains("BODY_ARMOR") || upper.Contains("SOUL_ARMOR")));
        }
    }
}
