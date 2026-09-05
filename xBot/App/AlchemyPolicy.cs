using System;

namespace xBot.App
{
    public enum ElixirType : byte
    {
        None = 0,
        Weapon = 1,
        Shield = 2,
        Protector = 3,
        Accessory = 4
    }

    /// <summary>
    /// Pure decision and matching engine for Silkroad Auto Alchemy (+ Basma).
    /// Free of UI or network dependencies for complete unit testability.
    /// </summary>
    public static class AlchemyPolicy
    {
        public const ushort CLIENT_ALCHEMY = 0x7150;
        public const ushort CLIENT_ALCHEMY_STONE = 0x7151;
        public const ushort SERVER_ALCHEMY_RESPONSE = 0xB150;

        /// <summary>
        /// Calculates the Silkroad item Degree (1 to 14+) based on required item level.
        /// </summary>
        public static byte CalculateDegree(byte itemLevel)
        {
            if (itemLevel <= 7) return 1;
            if (itemLevel <= 15) return 2;
            if (itemLevel <= 23) return 3;
            if (itemLevel <= 31) return 4;
            if (itemLevel <= 41) return 5;
            if (itemLevel <= 51) return 6;
            if (itemLevel <= 63) return 7;
            if (itemLevel <= 75) return 8;
            if (itemLevel <= 89) return 9;
            if (itemLevel <= 100) return 10;
            if (itemLevel <= 110) return 11;
            if (itemLevel <= 120) return 12;
            if (itemLevel <= 130) return 13;
            return 14;
        }

        /// <summary>
        /// Resolves the required Elixir type based on equipment item type or tid3.
        /// In Silkroad: tid3: 6 = Weapon, 4 = Shield, 1 = Armor/Protector/Clothes, 12 = Accessory.
        /// </summary>
        public static ElixirType GetRequiredElixirType(byte id3)
        {
            switch (id3)
            {
                case 6: // Weapon
                    return ElixirType.Weapon;
                case 4: // Shield
                    return ElixirType.Shield;
                case 1: // Armor / Garment / Protector
                    return ElixirType.Protector;
                case 12: // Accessory (Ring, Necklace, Earring)
                    return ElixirType.Accessory;
                default:
                    return ElixirType.None;
            }
        }

        /// <summary>
        /// Checks if an Elixir item matches the required equipment type.
        /// </summary>
        public static bool IsElixirMatching(string serverName, ElixirType requiredType)
        {
            if (string.IsNullOrWhiteSpace(serverName) || requiredType == ElixirType.None)
                return false;

            string upper = serverName.ToUpperInvariant();
            if (!upper.Contains("ARCHEMY_POTION") && !upper.Contains("ELIXIR"))
                return false;

            switch (requiredType)
            {
                case ElixirType.Weapon:
                    return upper.Contains("WEAPON");
                case ElixirType.Shield:
                    return upper.Contains("SHIELD");
                case ElixirType.Protector:
                    return upper.Contains("ARMOR") || upper.Contains("GUARD") || upper.Contains("PROTECTOR");
                case ElixirType.Accessory:
                    return upper.Contains("ACCESSORY");
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if Lucky Powder matches the target equipment Degree.
        /// </summary>
        public static bool IsLuckyPowderMatching(string serverName, byte itemDegree)
        {
            if (string.IsNullOrWhiteSpace(serverName) || itemDegree == 0)
                return false;

            string upper = serverName.ToUpperInvariant();
            if (!upper.Contains("REINFORCE_RECIPE") && !upper.Contains("POWDER"))
                return false;

            string degreeStr = itemDegree.ToString("D2"); // e.g. "01", "08", "11"
            return upper.EndsWith(degreeStr) || upper.Contains("_" + degreeStr) || upper.Contains(itemDegree.ToString());
        }

        /// <summary>
        /// Determines whether auto alchemy fusing should continue.
        /// </summary>
        public static bool ShouldContinueFusing(byte currentPlus, byte targetPlus, int availableElixirs, int attemptsMade, int maxAttempts)
        {
            // Reached or exceeded target plus
            if (currentPlus >= targetPlus)
                return false;

            // Out of materials
            if (availableElixirs <= 0)
                return false;

            // Exceeded attempt safety limit
            if (maxAttempts > 0 && attemptsMade >= maxAttempts)
                return false;

            return true;
        }

        /// <summary>
        /// Parses the alchemy result byte from server response 0xB150.
        /// </summary>
        public static bool ParseAlchemyResponse(byte resultByte, out bool isSuccess, out string description)
        {
            switch (resultByte)
            {
                case 1:
                    isSuccess = true;
                    description = "Alchemy succeeded! (+1)";
                    return true;
                case 2:
                    isSuccess = false;
                    description = "Alchemy failed! (+ value reset or reduced)";
                    return true;
                case 3:
                    isSuccess = false;
                    description = "Item destroyed during alchemy!";
                    return true;
                default:
                    isSuccess = false;
                    description = $"Alchemy error or unknown result ({resultByte})";
                    return false;
            }
        }
    }
}
