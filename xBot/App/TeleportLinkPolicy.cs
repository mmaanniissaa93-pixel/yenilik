using System;
using System.Collections.Generic;

namespace xBot.App
{
    /// <summary>
    /// Sürekli başarısız olan teleport köprülerinin geçici kara listesi.
    /// Saf karar mantığıdır (UI/oyun durumu yok); TeleportManager delege eder,
    /// senaryo testleri doğrudan bunu çalıştırır.
    /// </summary>
    public static class TeleportLinkPolicy
    {
        public const int MaxLinkFailures = 3;
        public static readonly TimeSpan BlacklistDuration = TimeSpan.FromMinutes(10);

        private static readonly object s_lock = new object();
        private static readonly Dictionary<string, int> s_failCount = new Dictionary<string, int>();
        private static readonly Dictionary<string, DateTime> s_blacklist = new Dictionary<string, DateTime>();

        public static string Key(uint sourceId, uint destinationId)
        {
            return sourceId + "->" + destinationId;
        }

        public static bool IsBlacklisted(uint sourceId, uint destinationId, DateTime now)
        {
            lock (s_lock)
            {
                DateTime until;
                if (s_blacklist.TryGetValue(Key(sourceId, destinationId), out until))
                {
                    if (now < until)
                        return true;
                    s_blacklist.Remove(Key(sourceId, destinationId));
                }
                return false;
            }
        }

        /// <summary>Başarısızlığı işler; bu çağrı kara listeye soktuysa true döner.</summary>
        public static bool NoteFailure(uint sourceId, uint destinationId, DateTime now)
        {
            lock (s_lock)
            {
                string key = Key(sourceId, destinationId);
                int n = 0;
                s_failCount.TryGetValue(key, out n);
                n++;
                s_failCount[key] = n;
                if (n >= MaxLinkFailures && !s_blacklist.ContainsKey(key))
                {
                    s_blacklist[key] = now.Add(BlacklistDuration);
                    return true;
                }
                return false;
            }
        }

        public static void NoteSuccess(uint sourceId, uint destinationId)
        {
            lock (s_lock)
            {
                string key = Key(sourceId, destinationId);
                s_failCount.Remove(key);
                s_blacklist.Remove(key);
            }
        }

        public static void Reset()
        {
            lock (s_lock)
            {
                s_failCount.Clear();
                s_blacklist.Clear();
            }
        }
    }
}
