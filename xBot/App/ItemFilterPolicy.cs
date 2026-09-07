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
        public System.Collections.Generic.List<string> BlueNames { get; set; }
        public bool IsArrowBolt { get; set; }
        public int Degree { get; set; }
        public bool IsChina { get; set; }
        public bool IsEurope { get; set; }
        public bool IsMale { get; set; }
        public bool IsFemale { get; set; }
        /// <summary>0 = sahipsiz/serbest, 1 = bana ait, 2 = parti üyesine ait, 3 = yabancı oyuncuya ait.</summary>
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

            if (pick != null && pick.DontPickItems)
                return false;

            // Sahiplik: yabancı oyuncunun eşyası / parti eşyası.
            if (pick != null)
            {
                if (input.OwnerKind == 3 && !pick.PickOthersItems)
                    return false;
                if (input.OwnerKind == 2 && !pick.PickPartyItems)
                    return false;
            }

            // Kural varsa Pick sütunu KARAKTERİN yürüyeceği eşyadır (pet açık
            // olsa bile). Pet işine ShouldUsePet karar verir — iki sütun bağımsızdır.
            if (rule != null)
                return rule.Pickup;

            if (pick != null && pick.OnlyPickRareBlue && !input.IsRare && !input.IsBlue && !input.IsSox)
                return false;

            if (input.IsGold || input.IsElixirOrStone || !input.IsEquipable)
                return true;

            if (options == null)
                return true;

            if (input.Degree > 0 && (input.Degree < options.MinDegree || input.Degree > options.MaxDegree))
                return false;
            if (options.OnlySox && !input.IsSox)
                return false;
            if (input.IsChina && !options.FilterChina)
                return false;
            if (input.IsEurope && !options.FilterEurope)
                return false;
            if (input.IsMale && !options.FilterMale)
                return false;
            if (input.IsFemale && !options.FilterFemale)
                return false;

            return true;
        }

        // Eski imza ile uyumluluk.
        public static bool ShouldPickup(ItemFilterInput input, ItemFilterOptions options, ItemFilterRule rule)
        {
            return ShouldPickup(input, options, rule, null);
        }

        /// <summary>
        /// Eşya pet ile mi toplansın? Varsayılan KARAKTER toplar; sadece kuralda
        /// Pet=Yes yazan eşyayı pet toplar (phBot davranışı).
        /// </summary>
        public static bool ShouldUsePet(ItemFilterInput input, ItemFilterRule rule, PickFilterOptions pick, bool petAvailable, bool petFull)
        {
            if (!petAvailable)
                return false;
            if (pick != null && !pick.UsePickPet)
                return false;
            // Kural yoksa pet toplamasın — karakter toplar.
            if (rule == null)
                return false;
            if (!rule.Pet)
                return false;
            if (petFull)
                return false;
            return true;
        }

        public static bool ShouldSell(ItemFilterInput input, ItemFilterRule rule, PickFilterOptions pick)
        {
            if (input == null)
                return false;
            // SOX asla satılmaz.
            if (input.IsSox)
                return false;
            if (pick != null && pick.NoSellPlusEnabled && input.Plus >= pick.NoSellPlus)
                return false;
            if (rule != null)
                return rule.Sell;
            // Kural yoksa: sadece "hepsini sat" modu beyaz eşyayı satar.
            if (pick != null && pick.AllowSellAll && input.IsEquipable && !input.IsRare && !input.IsBlue)
                return true;
            return false;
        }

        // Eski imza ile uyumluluk.
        public static bool ShouldSell(ItemFilterInput input, ItemFilterRule rule)
        {
            return ShouldSell(input, rule, null);
        }

        public static bool ShouldStore(ItemFilterInput input, ItemFilterRule rule, PickFilterOptions pick, System.Collections.Generic.IDictionary<string, BlueAttributeRule> blues)
        {
            if (input == null)
                return false;
            if (pick != null && pick.OnlyStoreRareBlue && !input.IsRare && !input.IsBlue && !input.IsSox)
                return false;
            if (pick != null && pick.OnlyStorePlusEnabled && input.IsEquipable && input.Plus < pick.OnlyStorePlus)
                return false;
            if (rule != null)
            {
                if (rule.Store)
                    return true;
                // Kural var ama Store=No: sadece mavi-özellik kuralı kurtarabilir.
                return ShouldStoreByBlue(input, pick, blues);
            }

            if (ShouldStoreByBlue(input, pick, blues))
                return true;

            return input.IsElixirOrStone || input.IsSox;
        }

        // Eski imza ile uyumluluk.
        public static bool ShouldStore(ItemFilterInput input, ItemFilterRule rule)
        {
            return ShouldStore(input, rule, null, null);
        }

        private static bool ShouldStoreByBlue(ItemFilterInput input, PickFilterOptions pick, System.Collections.Generic.IDictionary<string, BlueAttributeRule> blues)
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

        public static bool ShouldStoreGuild(ItemFilterInput input, ItemFilterRule rule)
        {
            return input != null && rule != null && rule.StoreGuild;
        }

        /// <summary>
        /// Dismantle adayı mı? (Motor paketi yoksa çağıran taraf loglar/atlar.)
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
