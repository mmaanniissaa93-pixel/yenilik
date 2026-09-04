namespace xBot.App
{
    public enum ProtectionDecision
    {
        None,
        ReturnToTown,
        StopBotInTown
    }

    /// <summary>
    /// Runtime-independent input used to evaluate protection decisions.
    /// Keeping this separate from game objects makes the safety rules testable
    /// without opening a client or connecting to a server.
    /// </summary>
    public sealed class ProtectionPolicyInput
    {
        public bool IsInGame { get; set; }
        public bool IsBotting { get; set; }
        public bool IsAlive { get; set; }
        public bool IsInTown { get; set; }
        public bool StopAfterReturn { get; set; }
        public bool DeadDelayElapsed { get; set; }
        public bool HasReturnScroll { get; set; }
        public bool NoArrows { get; set; }
        public bool FullInventory { get; set; }
        public bool FullPetInventory { get; set; }
        public bool HPLow { get; set; }
        public bool MPLow { get; set; }
        public bool DurabilityLow { get; set; }
        public bool LevelUpPending { get; set; }
    }

    public sealed class ProtectionPolicyOptions
    {
        public bool ReturnDeadWithDelay { get; set; }
        public bool StopBotInTown { get; set; }
        public bool ReturnNoArrows { get; set; }
        public bool ReturnFullInventory { get; set; }
        public bool ReturnFullPetInventory { get; set; }
        public bool ReturnHPLow { get; set; }
        public bool ReturnMPLow { get; set; }
        public bool ReturnDurabilityLow { get; set; }
        public bool ReturnLevelUp { get; set; }
    }

    public static class ProtectionPolicy
    {
        public static bool HasReturnTrigger(ProtectionPolicyInput input, ProtectionPolicyOptions options)
        {
            if (input == null || options == null || !input.IsAlive)
                return false;

            return (options.ReturnNoArrows && input.NoArrows)
                || (options.ReturnFullInventory && input.FullInventory)
                || (options.ReturnFullPetInventory && input.FullPetInventory)
                || (options.ReturnHPLow && input.HPLow)
                || (options.ReturnMPLow && input.MPLow)
                || (options.ReturnDurabilityLow && input.DurabilityLow)
                || (options.ReturnLevelUp && input.LevelUpPending);
        }

        public static ProtectionDecision Evaluate(ProtectionPolicyInput input, ProtectionPolicyOptions options)
        {
            if (input == null || options == null || !input.IsInGame || !input.IsBotting)
                return ProtectionDecision.None;

            if (!input.IsAlive)
            {
                if (options.ReturnDeadWithDelay && input.DeadDelayElapsed && input.HasReturnScroll)
                    return ProtectionDecision.ReturnToTown;

                return ProtectionDecision.None;
            }

            if (options.StopBotInTown && input.IsInTown && input.StopAfterReturn)
                return ProtectionDecision.StopBotInTown;

            if (HasReturnTrigger(input, options) && input.HasReturnScroll)
                return ProtectionDecision.ReturnToTown;

            return ProtectionDecision.None;
        }
    }
}
