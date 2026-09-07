using System.Collections.Generic;

namespace xBot.App
{
    /// <summary>
    /// Options sekmesindeki 13 seçenek + eşikler (phBot "Options" sekmesi).
    /// </summary>
    public sealed class PickFilterOptions
    {
        public bool PickItemsFirst { get; set; } = false;
        public bool UsePickPet { get; set; } = true;
        public bool PickOthersItems { get; set; } = false;
        public bool PickPartyItems { get; set; } = false;
        public bool DontPickItems { get; set; } = false;
        public bool AllowSellAll { get; set; } = false;
        public bool OnlyPickRareBlue { get; set; } = false;
        public bool OnlyStoreRareBlue { get; set; } = false;
        public bool PickArrowsBolts { get; set; } = false;
        public int ArrowBoltAmount { get; set; } = 200;
        public bool OnlyStoreSpecificBlues { get; set; } = false;
        public bool PickWithCharIfPetGoneFull { get; set; } = true;
        public bool DontMovePetItemsExceptStoreSell { get; set; } = true;
        public bool OnlyStorePlusEnabled { get; set; } = false;
        public int OnlyStorePlus { get; set; } = 0;
        public bool NoSellPlusEnabled { get; set; } = false;
        public int NoSellPlus { get; set; } = 0;
        public bool PickEvenWhenFull { get; set; } = false;
    }

    /// <summary>
    /// Blues listesindeki tek satır: mavi özellik + store bayrağı.
    /// </summary>
    public sealed class BlueAttributeRule
    {
        public string ServerName { get; set; } = "";
        public string Name { get; set; } = "";
        public bool Store { get; set; } = false;
    }

    /// <summary>
    /// Store Gold sekmesi.
    /// </summary>
    public sealed class StoreGoldOptions
    {
        public bool Enabled { get; set; } = false;
        public ulong GoldKeepAmount { get; set; } = 1000000;
        public bool TakeGoldFromStorage { get; set; } = false;
        public bool TakeGoldFromGuildStorage { get; set; } = false;
        public bool StoreGoldInStorage { get; set; } = false;
        public ulong StoreGoldMax { get; set; } = 0; // 0 = limitsiz
        public bool StoreGoldInGuildStorage { get; set; } = false;
        public ulong StoreGoldGuildMax { get; set; } = 0; // 0 = limitsiz
    }

    /// <summary>
    /// Dismantle sekmesi.
    /// </summary>
    public sealed class DismantleOptions
    {
        public HashSet<int> Degrees { get; set; } = new HashSet<int>();
        public bool WhiteItems { get; set; } = false;
        public bool PlussedBelowEnabled { get; set; } = false;
        public int PlussedBelow { get; set; } = 3;
        public bool BlueItems { get; set; } = false;
        public bool RareItems { get; set; } = false;
        public bool BeforeBlacksmith { get; set; } = false;
        public bool BeforeGrocery { get; set; } = false;
        public bool BeforeHerbalist { get; set; } = false;
        public bool BeforeStorage { get; set; } = false;
        public bool BeforeGuildStorage { get; set; } = false;
    }
}
