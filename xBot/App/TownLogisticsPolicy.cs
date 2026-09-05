using System;

namespace xBot.App
{
    public enum AmmoType
    {
        None,
        Arrow,
        Bolt
    }

    /// <summary>
    /// Pure town logistics decisions shared by the runtime and scenario tests.
    /// Keeping these checks free of UI/game state makes town routines predictable and testable.
    /// </summary>
    public static class TownLogisticsPolicy
    {
        public static bool IsRangedWeapon(int weaponType)
        {
            // 6: Bow, 12: Crossbow (matching SRTypes.Weapon values)
            return weaponType == 6 || weaponType == 12;
        }

        public static AmmoType GetAmmoType(int weaponType)
        {
            if (weaponType == 6) return AmmoType.Arrow;
            if (weaponType == 12) return AmmoType.Bolt;
            return AmmoType.None;
        }

        public static byte GetAmmoShopSlot(AmmoType ammoType)
        {
            return ammoType == AmmoType.Bolt ? (byte)1 : (byte)0;
        }

        public static bool ShouldBuyAmmo(int weaponType, int currentAmmo, int minThreshold = 1000)
        {
            if (!IsRangedWeapon(weaponType))
                return false;

            return currentAmmo < minThreshold;
        }

        public static int CalculateMissingScrolls(int currentScrolls, int targetScrolls = 5)
        {
            return Math.Max(0, targetScrolls - currentScrolls);
        }

        public static bool ShouldUnloadPetItem(ItemFilterInput itemInput, ItemFilterRule explicitRule = null)
        {
            return ItemFilterPolicy.ShouldStore(itemInput, explicitRule);
        }
    }
}
