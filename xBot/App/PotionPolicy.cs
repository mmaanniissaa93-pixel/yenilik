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

        /// <summary>
        /// Item Mall potu mu? ServerName locale-bağımsızdır ("MALL" içerir);
        /// display adı yedeğidir (X-large potlar mall'dandır).
        /// </summary>
        public static bool IsMallPotion(string serverName, string displayName)
        {
            if (!string.IsNullOrEmpty(serverName)
                && serverName.IndexOf("MALL", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            if (!string.IsNullOrEmpty(displayName)
                && displayName.IndexOf("X-large", StringComparison.OrdinalIgnoreCase) >= 0)
                return true;
            return false;
        }

        /// <summary>
        /// Usage çözümü: öğrenilmiş değer varsa o; yoksa tek sayıda reject sonrası
        /// bit0 çevrilmiş alternatif denenir (Sevar 0x..ED vs 0x..EC). Tutan varyant
        /// kalıcı öğrenilir, sayaç sıfırlanır.
        /// </summary>
        public static ushort ResolveUsageWithFallback(ushort computed, int rejectCount, bool hasLearned, ushort learned)
        {
            if (hasLearned)
                return learned;
            if ((rejectCount & 1) == 1)
                return (ushort)(computed ^ 0x0001);
            return computed;
        }

        /// <summary>
        /// Pot karşılaştırma: normal pot her zaman mall'dan önce; sonra güçlü
        /// (yüksek LevelRequired) olan; sonra büyük model ID. Negatif = a önce kullanılmalı.
        /// </summary>
        public static int ComparePotions(bool aMall, int aLevelRequired, uint aId,
                                         bool bMall, int bLevelRequired, uint bId)
        {
            if (aMall != bMall)
                return aMall ? 1 : -1;
            if (aLevelRequired != bLevelRequired)
                return bLevelRequired.CompareTo(aLevelRequired);
            if (aId != bId)
                return bId.CompareTo(aId);
            return 0;
        }
    }
}
