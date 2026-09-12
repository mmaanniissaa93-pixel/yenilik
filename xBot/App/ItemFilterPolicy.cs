using System;
using System.Collections.Generic;

namespace xBot.App
{
    public enum ItemFilterAction
    {
        Ignore,
        Pickup,
        Sell,
        Store
    }

    public enum ItemFilterMatchType
    {
        Exact,      // Exact string match (current behavior)
        Wildcard,   // Glob-style wildcard (* = any chars, ? = single char)
        Regex       // Regular expression
    }

    public sealed class ItemFilterInput
    {
        public string ItemName { get; set; }
        public string ServerName { get; set; }
        public bool IsEquipable { get; set; }
        public bool IsGold { get; set; }
        public bool IsElixirOrStone { get; set; }
        public bool IsSox { get; set; }
        public bool IsRare { get; set; }
        public bool IsBlue { get; set; }
        public int Plus { get; set; }
        public List<string> BlueNames { get; set; }
        public bool IsArrowBolt { get; set; }
        public int Degree { get; set; }
        public bool IsChina { get; set; }
        public bool IsEurope { get; set; }
        public bool IsMale { get; set; }
        public bool IsFemale { get; set; }
        public byte ID2 { get; set; }
        public byte ID3 { get; set; }
        public byte ID4 { get; set; }
        public EzFilterCategory? EzCategory { get; set; }
        /// <summary>0 = sahipsiz/serbest, 1 = bana ait, 2 = parti Ã¼yesine ait, 3 = yabancÄ± oyuncuya ait.</summary>
        public int OwnerKind { get; set; }
    }

    public sealed class ItemFilterOptions
    {
        public int MinDegree { get; set; }
        public int MaxDegree { get; set; }
        public bool OnlySox { get; set; }
        public bool FilterChina { get; set; }
        public bool FilterEurope { get; set; }
        public bool FilterMale { get; set; }
        public bool FilterFemale { get; set; }
    }

    public class ItemFilterRule
    {
        public string ItemName { get; set; } = "";
        public bool Pickup { get; set; } = true;
        public bool Pet { get; set; } = false;
        public bool Sell { get; set; } = false;
        public bool Store { get; set; } = false;
        public bool StoreGuild { get; set; } = false;
        public bool TakeStorage { get; set; } = false;
        public bool TakeGuildStorage { get; set; } = false;

        // Pattern matching support
        public ItemFilterMatchType MatchType { get; set; } = ItemFilterMatchType.Exact;
        public string Pattern { get; set; } = ""; // Used for Wildcard/Regex modes
    }

    public static class ItemFilterPolicy
    {
        public static bool ShouldPickup(ItemFilterInput input, ItemFilterOptions options, ItemFilterRule rule, PickFilterOptions pick)
        {
            if (input == null)
                return false;

            if (pick != null && !pick.Enabled)
                return false;

            if (pick != null && pick.DontPickItems)
                return false;

            // Gri barda eşyaları toplama: sahipsiz değilse ve bana/partime ait değilse alma
            if (pick != null && pick.DontPickGreyBar && input.OwnerKind == 3)
                return false;

            // Sahiplik: yabancı oyuncunun eşyası / parti eşyası.
            if (pick != null)
            {
                if (input.OwnerKind == 3 && !pick.PickOthersItems)
                    return false;
                if (input.OwnerKind == 2 && !pick.PickPartyItems)
                    return false;
            }

            // 1. Doğrudan Eşya Kuralı (Toplama Filtresi tablosundaki kural)
            if (rule != null)
                return rule.Pickup;

            // 2. Ez Filter Kategori Kuralı
            if (input.EzCategory.HasValue)
            {
                var ezRule = EzFilterManager.GetRule(input.EzCategory.Value);
                if (ezRule != null && ezRule.Pick)
                {
                    // Ekipman ise ve "Sadece SOX ya da Blue eşyaları topla" açıksa filtrele
                    if (pick != null && pick.OnlyPickRareBlue && input.IsEquipable && !input.IsRare && !input.IsBlue && !input.IsSox)
                        return false;

                    return true;
                }
            }

            // 3. "Sadece SOX ya da Blue eşyaları topla"
            if (pick != null && pick.OnlyPickRareBlue && (input.IsRare || input.IsBlue || input.IsSox))
                return true;

            // Mavi-özellik modu
            if (pick != null && pick.OnlyPickSpecificBlues && input.IsEquipable && (input.IsRare || input.IsBlue || input.IsSox))
                return true;

            if (input.IsGold || input.IsElixirOrStone)
                return true;

            return false;
        }

        // Eski imza ile uyumluluk.
        public static bool ShouldPickup(ItemFilterInput input, ItemFilterOptions options, ItemFilterRule rule)
        {
            return ShouldPickup(input, options, rule, null);
        }

        /// <summary>
        /// EÅŸya pet ile mi toplansÄ±n?
        /// </summary>
        public static bool ShouldUsePet(ItemFilterInput input, ItemFilterRule rule, PickFilterOptions pick, bool petAvailable, bool petFull)
        {
            if (!petAvailable)
                return false;
            if (pick != null && !pick.Enabled)
                return false;
            if (pick != null && pick.DontPickItems)
                return false;
            if (pick != null && !pick.UsePickPet)
                return false;
            if (petFull)
                return false;

            // Gri barda eşyaları toplama
            if (pick != null && pick.DontPickGreyBar && input != null && input.OwnerKind == 3)
                return false;

            if (pick != null && input != null)
            {
                if (input.OwnerKind == 3 && !pick.PickOthersItems)
                    return false;
                if (input.OwnerKind == 2 && !pick.PickPartyItems)
                    return false;
            }

            // 1. DoÄŸrudan EÅŸya KuralÄ± (Toplama Filtresi tablosundaki Pet sÃ¼tunu)
            if (rule != null)
                return rule.Pet;

            // 2. Ez Filter Kategori KuralÄ± (pet pick)
            if (input != null && input.EzCategory.HasValue)
            {
                var ezRule = EzFilterManager.GetRule(input.EzCategory.Value);
                if (ezRule != null && ezRule.PetPick)
                {
                    if (pick != null && pick.OnlyPickRareBlue && input.IsEquipable && !input.IsRare && !input.IsBlue && !input.IsSox)
                        return false;

                    return true;
                }
            }

            // 3. SOX/Blue toplama
            if (pick != null && pick.OnlyPickRareBlue && input != null && (input.IsRare || input.IsBlue || input.IsSox))
                return true;

            return false;
        }

        public static Func<IDictionary<string, BlueAttributeRule>> GetBluesProvider { get; set; }

        public static bool ShouldSell(ItemFilterInput input, ItemFilterRule rule, PickFilterOptions pick)
        {
            if (input == null)
                return false;
            // SOX asla satılmaz.
            if (input.IsSox)
                return false;
            if (pick != null && pick.NoSellPlusEnabled && input.Plus >= pick.NoSellPlus)
                return false;

            // 1. Doğrudan Eşya Kuralı
            if (rule != null)
                return rule.Sell;

            // 2. Seçili blue'lu eşyaları sat
            if (pick != null && pick.SellSelectedBlues && ShouldSellByBlue(input, GetBluesProvider?.Invoke()))
                return true;

            // 3. Ez Filter Kategori KuralÄ±
            if (input.EzCategory.HasValue)
            {
                var ezRule = EzFilterManager.GetRule(input.EzCategory.Value);
                if (ezRule != null && ezRule.Sell)
                    return true;
            }

            // 4. Kural yoksa: sadece "TÃ¼m eÅŸya tÃ¼rlerini satmaya izin ver" modu beyaz eÅŸyayÄ± satar.
            if (pick != null && pick.AllowSellAll && input.IsEquipable && !input.IsRare && !input.IsBlue)
                return true;

            return false;
        }

        // Eski imza ile uyumluluk.
        public static bool ShouldSell(ItemFilterInput input, ItemFilterRule rule)
        {
            return ShouldSell(input, rule, null);
        }

        public static bool ShouldStore(ItemFilterInput input, ItemFilterRule rule, PickFilterOptions pick, IDictionary<string, BlueAttributeRule> blues)
        {
            if (input == null)
                return false;
            if (pick != null && pick.OnlyStoreRareBlue && !input.IsRare && !input.IsBlue && !input.IsSox)
                return false;
            if (pick != null && pick.OnlyStorePlusEnabled && input.IsEquipable && input.Plus < pick.OnlyStorePlus)
                return false;

            // 1. DoÄŸrudan kural
            if (rule != null)
            {
                if (rule.Store)
                    return true;
                return ShouldStoreByBlue(input, pick, blues);
            }

            // 2. Mavi-Ã¶zellik kontrolÃ¼
            if (ShouldStoreByBlue(input, pick, blues))
                return true;

            // 3. Ez Filter Kategori KuralÄ±
            if (input.EzCategory.HasValue)
            {
                var ezRule = EzFilterManager.GetRule(input.EzCategory.Value);
                if (ezRule != null && ezRule.Store)
                    return true;
            }

            // 4. Varsayılan olarak Elixir, Stone veya SOX depolanır
            if (input.IsElixirOrStone || input.IsSox)
                return true;

            if (pick != null && pick.OnlyStoreRareBlue && (input.IsSox || input.IsRare || input.IsBlue))
                return true;

            return false;
        }

        // Eski imza ile uyumluluk.
        public static bool ShouldStore(ItemFilterInput input, ItemFilterRule rule)
        {
            return ShouldStore(input, rule, null, null);
        }

        private static bool ShouldStoreByBlue(ItemFilterInput input, PickFilterOptions pick, IDictionary<string, BlueAttributeRule> blues)
        {
            if (pick == null || !pick.OnlyStoreSpecificBlues || blues == null || blues.Count == 0)
                return false;
            if (input == null || !input.IsBlue || input.BlueNames == null)
                return false;
            foreach (string blue in input.BlueNames)
            {
                if (string.IsNullOrEmpty(blue))
                    continue;
                BlueAttributeRule rule;
                if (blues.TryGetValue(blue, out rule) && rule != null && rule.Store)
                    return true;
            }
            return false;
        }

        private static bool ShouldSellByBlue(ItemFilterInput input, IDictionary<string, BlueAttributeRule> blues)
        {
            if (blues == null || blues.Count == 0 || input == null || !input.IsBlue || input.BlueNames == null)
                return false;
            foreach (string blue in input.BlueNames)
            {
                if (string.IsNullOrEmpty(blue))
                    continue;
                BlueAttributeRule rule;
                if (blues.TryGetValue(blue, out rule) && rule != null && rule.Sell)
                    return true;
            }
            return false;
        }

        public static bool ShouldStoreGuild(ItemFilterInput input, ItemFilterRule rule)
        {
            if (input == null)
                return false;

            if (rule != null)
                return rule.StoreGuild;

            if (input.EzCategory.HasValue)
            {
                var ezRule = EzFilterManager.GetRule(input.EzCategory.Value);
                if (ezRule != null && ezRule.GuildStore)
                    return true;
            }

            return false;
        }

        public static bool ShouldTakeStorage(ItemFilterInput input, ItemFilterRule rule)
        {
            return input != null && rule != null && rule.TakeStorage;
        }

        public static bool ShouldTakeGuildStorage(ItemFilterInput input, ItemFilterRule rule)
        {
            return input != null && rule != null && rule.TakeGuildStorage;
        }

        /// <summary>
        /// Dismantle adayÄ± mÄ±?
        /// </summary>
        public static bool ShouldDismantle(ItemFilterInput input, DismantleOptions dismantle)
        {
            if (input == null || dismantle == null || !input.IsEquipable || input.IsSox)
                return false;
            if (dismantle.Degrees != null && dismantle.Degrees.Count > 0)
            {
                if (input.Degree <= 0 || !dismantle.Degrees.Contains(input.Degree))
                    return false;
            }
            bool white = !input.IsRare && !input.IsBlue;
            if (white && dismantle.WhiteItems)
                return true;
            if (dismantle.PlussedBelowEnabled && input.Plus < dismantle.PlussedBelow)
                return true;
            if (input.IsBlue && !input.IsRare && dismantle.BlueItems)
                return true;
            if (input.IsRare && dismantle.RareItems)
                return true;
            return false;
        }
    }
}