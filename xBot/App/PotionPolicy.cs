using System;

namespace xBot.App
{
    public static class PotionPolicy
    {
        public const int EuropeanPotionCooldownMs = 15000;
        public const int ChinesePotionCooldownMs = 1000;
        public const int VigorPotionCooldownMs = 15000;
        public const int PillCooldownMs = 12000;

        public const int DefaultSlotBlockSeconds = 5;
        public const int DefaultBlackoutSeconds = 2;
        public const int CooldownRejectBlackoutMs = 500;

        public const ushort ErrorCodeCooldownActive = 0x185B;

        public static bool IsEuropean(string serverName)
        {
            return !string.IsNullOrEmpty(serverName) && serverName.StartsWith("CHAR_EU_", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsChinese(string serverName)
        {
            return !string.IsNullOrEmpty(serverName) && serverName.StartsWith("CHAR_CH_", StringComparison.OrdinalIgnoreCase);
        }

        public static int GetPotionCooldownMs(bool isEuropean)
        {
            return isEuropean ? EuropeanPotionCooldownMs : ChinesePotionCooldownMs;
        }

        public static bool ShouldBlockSlot(ushort errorCode)
        {
            // 0x185B is normal potion cooldown remaining on the server. Never block the slot.
            return errorCode != ErrorCodeCooldownActive;
        }

        public static int GetSlotBlockSeconds(ushort errorCode)
        {
            if (errorCode == ErrorCodeCooldownActive)
                return 0;
            return DefaultSlotBlockSeconds;
        }

        public static int GetBlackoutMs(ushort errorCode)
        {
            if (errorCode == ErrorCodeCooldownActive)
                return CooldownRejectBlackoutMs;
            return DefaultBlackoutSeconds * 1000;
        }
    }
}
