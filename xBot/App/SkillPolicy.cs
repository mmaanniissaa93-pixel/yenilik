using System;

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

        /// <summary>
        /// Sıralı kombo seçimi: cursor konumundan başla, hazır (cooldown'da
        /// olmayan, MP'si yeten, aktif) olan ilk skill'in index'ini dön.
        /// Hazır olmayan atlanır, liste biterse başa sarılır. Hiçbiri hazır
        /// değilse -1 döner ve cursor değişmez (bekleme/fallback yolu).
        /// </summary>
        public static int SelectNextReadyIndex(int count, Func<int, bool> isReady, ref int cursor)
        {
            if (count <= 0 || isReady == null)
                return -1;
            if (cursor < 0 || cursor >= count)
                cursor = 0;

            for (int n = 0; n < count; n++)
            {
                int idx = (cursor + n) % count;
                bool ready;
                try
                {
                    ready = isReady(idx);
                }
                catch
                {
                    ready = false;
                }
                if (ready)
                {
                    cursor = (idx + 1) % count;
                    return idx;
                }
            }
            return -1;
        }
    }
}
