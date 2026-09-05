using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Item;

namespace xBot.App
{
    /// <summary>
    /// Service that coordinates Auto Alchemy (+ Basma) upgrading for equipment.
    /// </summary>
    public static class AlchemyManager
    {
        public static bool IsRunning { get; private set; } = false;
        public static byte TargetSlot { get; set; } = 13;
        public static byte TargetPlus { get; set; } = 3;
        public static bool UseLuckyPowder { get; set; } = true;
        public static int MaxAttempts { get; set; } = 50;
        public static int CurrentAttempts { get; private set; } = 0;
        public static int SuccessCount { get; private set; } = 0;
        public static int FailCount { get; private set; } = 0;
        public static int DelayMs { get; set; } = 2000;
        public static string LastStatus { get; private set; } = "Beklemede / Idle";

        public static event Action OnStatusChanged;
        public static event Action<string> OnLogMessage;

        private static DateTime lastAttemptUtc = DateTime.MinValue;
        private static readonly object alchemyLock = new object();

        /// <summary>
        /// Resets attempt and result counters.
        /// </summary>
        public static void ResetCounters()
        {
            CurrentAttempts = 0;
            SuccessCount = 0;
            FailCount = 0;
            LastStatus = "Sayaçlar sıfırlandı / Reset";
            OnStatusChanged?.Invoke();
        }

        /// <summary>
        /// Starts the auto alchemy upgrading process for a specific inventory slot and target plus.
        /// </summary>
        public static void Start(byte slot, byte targetPlus, bool usePowder = true, int maxAttempts = 50, int delayMs = 2000)
        {
            TargetSlot = slot;
            TargetPlus = targetPlus;
            UseLuckyPowder = usePowder;
            MaxAttempts = maxAttempts;
            DelayMs = Math.Max(500, delayMs);
            CurrentAttempts = 0;
            SuccessCount = 0;
            FailCount = 0;
            IsRunning = true;
            lastAttemptUtc = DateTime.MinValue;
            LastStatus = $"Başlatıldı (+{targetPlus} hedefleniyor)";

            string msg = $"[Auto Alchemy] Started for slot {slot} (Target: +{targetPlus}, Powder: {usePowder}, Max Attempts: {maxAttempts}).";
            Window.Get?.Log(msg);
            OnLogMessage?.Invoke(msg);
            OnStatusChanged?.Invoke();

            if (alchemyThread == null || !alchemyThread.IsAlive)
            {
                alchemyThread = new Thread(AlchemyLoop) { IsBackground = true };
                alchemyThread.Start();
            }
        }

        private static Thread alchemyThread;

        private static void AlchemyLoop()
        {
            while (IsRunning)
            {
                RunTick();
                Thread.Sleep(Math.Max(500, DelayMs));
            }
        }

        /// <summary>
        /// Stops the auto alchemy upgrading process.
        /// </summary>
        public static void Stop()
        {
            if (IsRunning)
            {
                IsRunning = false;
                LastStatus = "Durduruldu / Stopped";
                string msg = "[Auto Alchemy] Stopped.";
                Window.Get?.Log(msg);
                OnLogMessage?.Invoke(msg);
                OnStatusChanged?.Invoke();
            }
        }

        /// <summary>
        /// Executes a single alchemy cycle if conditions are met.
        /// </summary>
        public static void RunTick()
        {
            if (!IsRunning || !InfoManager.inGame || InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return;

            if ((DateTime.UtcNow - lastAttemptUtc).TotalMilliseconds < DelayMs)
                return;

            if (!Monitor.TryEnter(alchemyLock))
                return;

            try
            {
                if (TargetSlot >= InfoManager.Character.Inventory.Capacity)
                {
                    Window.Get?.Log($"[Auto Alchemy] Invalid target slot ({TargetSlot}). Stopping.");
                    Stop();
                    return;
                }

                SRItem rawItem = InfoManager.Character.Inventory[TargetSlot];
                if (rawItem == null || !(rawItem is SREquipable item))
                {
                    Window.Get?.Log($"[Auto Alchemy] No upgradeable equipment found in slot {TargetSlot}. Stopping.");
                    Stop();
                    return;
                }

                // Check if target plus is reached
                if (item.Plus >= TargetPlus)
                {
                    Window.Get?.Log($"[Auto Alchemy] Target +{TargetPlus} reached on [{item.Name}]! (Current: +{item.Plus}). Stopping.");
                    Stop();
                    return;
                }

                // Check attempts limit
                if (MaxAttempts > 0 && CurrentAttempts >= MaxAttempts)
                {
                    Window.Get?.Log($"[Auto Alchemy] Maximum attempts limit ({MaxAttempts}) reached. Stopping.");
                    Stop();
                    return;
                }

                ElixirType requiredType = AlchemyPolicy.GetRequiredElixirType(item.ID3);
                if (requiredType == ElixirType.None)
                {
                    Window.Get?.Log($"[Auto Alchemy] Item [{item.Name}] cannot be enhanced with Elixirs. Stopping.");
                    Stop();
                    return;
                }

                byte degree = AlchemyPolicy.CalculateDegree(item.LevelRequired);

                // Find matching Elixir in inventory
                int elixirSlot = FindMatchingElixirSlot(requiredType);
                if (elixirSlot == -1)
                {
                    Window.Get?.Log($"[Auto Alchemy] No matching Elixir ({requiredType}) found in inventory. Stopping.");
                    Stop();
                    return;
                }

                // Find matching Lucky Powder if enabled
                byte powderSlot = 0xFF;
                if (UseLuckyPowder)
                {
                    int foundPowder = FindMatchingPowderSlot(degree);
                    if (foundPowder != -1)
                    {
                        powderSlot = (byte)foundPowder;
                    }
                }

                CurrentAttempts++;
                lastAttemptUtc = DateTime.UtcNow;

                string powderLog = powderSlot != 0xFF ? $" + Lucky Powder D{degree}" : "";
                Window.Get?.Log($"[Auto Alchemy] Attempt #{CurrentAttempts}: Fusing [{item.Name} (+{item.Plus}) -> Target +{TargetPlus}] with Elixir {requiredType}{powderLog}...");

                PacketBuilder.FuseItem(TargetSlot, (byte)elixirSlot, powderSlot);
            }
            finally
            {
                Monitor.Exit(alchemyLock);
            }
        }

        /// <summary>
        /// Called when the server responds with an alchemy result (0xB150).
        /// </summary>
        public static void OnAlchemyResult(byte resultByte)
        {
            if (AlchemyPolicy.ParseAlchemyResponse(resultByte, out bool isSuccess, out string description))
            {
                if (isSuccess)
                {
                    SuccessCount++;
                    LastStatus = "Başarılı! (+ Basıldı)";
                }
                else
                {
                    FailCount++;
                    LastStatus = resultByte == 3 ? "Eşya kırıldı / Destroyed!" : "Başarısız / Failed";
                }

                string msg = $"[Auto Alchemy] Result: {description} (Başarılı: {SuccessCount}, Başarısız: {FailCount})";
                Window.Get?.Log(msg);
                OnLogMessage?.Invoke(msg);
                OnStatusChanged?.Invoke();

                if (!isSuccess && resultByte == 3)
                {
                    // Item was destroyed
                    Stop();
                }
            }
        }

        public class UpgradeableItemInfo
        {
            public byte Slot { get; set; }
            public string Name { get; set; }
            public byte Plus { get; set; }
            public byte Degree { get; set; }
            public ElixirType RequiredElixir { get; set; }

            public override string ToString()
            {
                return $"[Slot {Slot}] {Name} (+{Plus}) (D{Degree})";
            }
        }

        public static List<UpgradeableItemInfo> GetUpgradeableInventoryItems()
        {
            List<UpgradeableItemInfo> list = new List<UpgradeableItemInfo>();
            var inv = InfoManager.Character?.Inventory;
            if (inv == null) return list;

            for (byte i = 13; i < inv.Capacity; i++)
            {
                SRItem raw = inv[i];
                if (raw != null && raw is SREquipable eq)
                {
                    ElixirType elix = AlchemyPolicy.GetRequiredElixirType(eq.ID3);
                    if (elix != ElixirType.None)
                    {
                        list.Add(new UpgradeableItemInfo
                        {
                            Slot = i,
                            Name = eq.Name,
                            Plus = eq.Plus,
                            Degree = AlchemyPolicy.CalculateDegree(eq.LevelRequired),
                            RequiredElixir = elix
                        });
                    }
                }
            }
            return list;
        }

        public static int CountElixirs(ElixirType type)
        {
            var inv = InfoManager.Character?.Inventory;
            if (inv == null) return 0;
            int count = 0;
            for (byte i = 13; i < inv.Capacity; i++)
            {
                SRItem raw = inv[i];
                if (raw != null && raw.isEtc() && AlchemyPolicy.IsElixirMatching(raw.ServerName, type))
                {
                    count += raw.Quantity;
                }
            }
            return count;
        }

        public static int CountLuckyPowder(byte degree)
        {
            var inv = InfoManager.Character?.Inventory;
            if (inv == null) return 0;
            int count = 0;
            for (byte i = 13; i < inv.Capacity; i++)
            {
                SRItem raw = inv[i];
                if (raw != null && raw.isEtc() && AlchemyPolicy.IsLuckyPowderMatching(raw.ServerName, degree))
                {
                    count += raw.Quantity;
                }
            }
            return count;
        }

        private static int FindMatchingElixirSlot(ElixirType requiredType)
        {
            var inv = InfoManager.Character?.Inventory;
            if (inv == null) return -1;

            for (byte i = 13; i < inv.Capacity; i++)
            {
                SRItem item = inv[i];
                if (item != null && item.isEtc() && AlchemyPolicy.IsElixirMatching(item.ServerName, requiredType))
                {
                    return i;
                }
            }
            return -1;
        }

        private static int FindMatchingPowderSlot(byte degree)
        {
            var inv = InfoManager.Character?.Inventory;
            if (inv == null) return -1;

            for (byte i = 13; i < inv.Capacity; i++)
            {
                SRItem item = inv[i];
                if (item != null && item.isEtc() && AlchemyPolicy.IsLuckyPowderMatching(item.ServerName, degree))
                {
                    return i;
                }
            }
            return -1;
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["TargetSlot"] = TargetSlot;
            json["TargetPlus"] = TargetPlus;
            json["UseLuckyPowder"] = UseLuckyPowder;
            json["MaxAttempts"] = MaxAttempts;
            json["DelayMs"] = DelayMs;
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null) return;
            if (json.ContainsKey("TargetSlot")) TargetSlot = (byte)json["TargetSlot"];
            if (json.ContainsKey("TargetPlus")) TargetPlus = (byte)json["TargetPlus"];
            if (json.ContainsKey("UseLuckyPowder")) UseLuckyPowder = (bool)json["UseLuckyPowder"];
            if (json.ContainsKey("MaxAttempts")) MaxAttempts = (int)json["MaxAttempts"];
            if (json.ContainsKey("DelayMs")) DelayMs = (int)json["DelayMs"];
        }
    }
}
