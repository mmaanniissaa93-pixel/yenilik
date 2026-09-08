using System;

namespace xBot.App
{
    [Flags]
    public enum TownNpcActions
    {
        None = 0,
        Sell = 1,
        Repair = 2,
        BuyPotions = 4,
        BuyAmmo = 8
    }

    /// <summary>
    /// Legacy BUY satırlarını NPC türünün gerçek şehir işlevlerine bağlar.
    /// Böylece alış hedefi sıfır olsa bile demirci tamiri gibi bağımsız işler atlanmaz.
    /// </summary>
    public static class TownNpcActionPolicy
    {
        public static TownNpcActions ForNpcCode(string npcCode)
        {
            string code = (npcCode ?? string.Empty).ToUpperInvariant();
            TownNpcActions actions = TownNpcActions.Sell;

            if (code.Contains("SMITH") || code.Contains("BLACKSMITH") || code.Contains("JUPITER"))
                actions |= TownNpcActions.Repair;
            if (code.Contains("POTION") || code.Contains("HERBALIST") || code.Contains("PHARMACY") || code.Contains("JUPITER"))
                actions |= TownNpcActions.BuyPotions;
            if (code.Contains("GROCERY"))
                actions |= TownNpcActions.BuyAmmo;

            return actions;
        }
    }
}
