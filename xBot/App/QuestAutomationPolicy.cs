namespace xBot.App
{
    public enum QuestCompletionAction
    {
        None,
        ReturnTown,
        RunScript
    }

    /// <summary>Pure quest completion decisions, kept separate for regression tests.</summary>
    public static class QuestAutomationPolicy
    {
        public static bool IsReadyToTurnIn(byte questState)
        {
            // vSRO QuestStatus: 2=completed/not supplied, 8=user-completed/not supplied.
            return questState == 2 || questState == 8;
        }

        public static bool ShouldQueue(bool enabled, QuestCompletionAction action,
            byte questState, bool alreadyHandled)
        {
            return enabled && action != QuestCompletionAction.None
                && IsReadyToTurnIn(questState) && !alreadyHandled;
        }

        public static string CompletionKey(uint questId, byte questState)
        {
            return questId + ":" + questState;
        }
    }
}
