namespace xBot.App
{
    public enum ItemFilterAction
    {
        Ignore,
        Pickup,
        Sell,
        Store
    }

    public sealed class ItemFilterInput
    {
        public string ItemName { get; set; }
        public string ServerName { get; set; }
        public bool IsEquipable { get; set; }
        public bool IsGold { get; set; }
        public bool IsElixirOrStone { get; set; }
        public bool IsSox { get; set; }
        public int Degree { get; set; }
        public bool IsChina { get; set; }
        public bool IsEurope { get; set; }
        public bool IsMale { get; set; }
        public bool IsFemale { get; set; }
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
        public bool Sell { get; set; } = false;
        public bool Store { get; set; } = false;
    }

    public static class ItemFilterPolicy
    {
        public static bool ShouldPickup(ItemFilterInput input, ItemFilterOptions options, ItemFilterRule rule)
        {
            if (input == null)
                return false;

            if (rule != null)
                return rule.Pickup;

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

        public static bool ShouldSell(ItemFilterInput input, ItemFilterRule rule)
        {
            return input != null && rule != null && rule.Sell;
        }

        public static bool ShouldStore(ItemFilterInput input, ItemFilterRule rule)
        {
            if (input == null)
                return false;
            if (rule != null)
                return rule.Store;

            return input.IsElixirOrStone || input.IsSox;
        }
    }
}
