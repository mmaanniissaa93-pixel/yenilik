namespace xBot.App
{
    /// <summary>
    /// Small, deterministic decisions for the attack-skill loop.
    /// </summary>
    public static class SkillPolicy
    {
        public static bool ShouldUseFallback(bool targetAlive, bool configuredSkillCastConfirmed)
        {
            return targetAlive && !configuredSkillCastConfirmed;
        }

        public static bool ShouldContinueCombo(bool castConfirmed, bool castInOrder)
        {
            return castConfirmed && castInOrder;
        }
    }
}
