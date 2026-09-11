using System;
using System.Threading;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Navigation;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;
using xBot.Network;
using SecurityAPI;

namespace xBot.App
{
    public static class ProtectionManager
    {
        // Skill-based Healing & Recovery
        public static bool UseSkillHP { get; set; } = false;
        public static byte SkillHPPercent { get; set; } = 50;
        public static bool UseSkillMP { get; set; } = false;
        public static byte SkillMPPercent { get; set; } = 40;
        public static bool UseSkillBadStatus { get; set; } = false;

        // Pet Protection
        public static bool RevivePet { get; set; } = true;
        public static bool AutoSummonPet { get; set; } = true;

        // Scroll & Return Protection
        public static bool UseReturnScrolls { get; set; } = true;
        public static bool UseReverseOnDeath { get; set; } = false;
        public static bool UseReverseAfterTown { get; set; } = false;
        public static bool UseSpeedDrugs { get; set; } = false;
        public static bool SpeedDrugsOnlyInScript { get; set; } = false;
        public static bool UseRepairHammer { get; set; } = false;

        // Pet Return Triggers
        public static bool ReturnOutOfPetRecoveryKits { get; set; } = false;
        public static bool ReturnOutOfPetRevive { get; set; } = false;
        public static bool ReturnOutOfPetFeed { get; set; } = false;
        public static bool ReturnOutOfPetAbnormalPill { get; set; } = false;
        public static bool ReturnOutOfTransportRecoveryKits { get; set; } = false;

        // Back to Town Triggers
        public static bool ReturnDeadWithDelay { get; set; } = false;
        public static int DeadDelaySeconds { get; set; } = 10;
        public static bool StopBotInTown { get; set; } = false;
        public static bool ReturnNoArrows { get; set; } = false;
        public static bool ReturnFullInventory { get; set; } = false;
        public static bool ReturnFullPetInventory { get; set; } = false;
        public static bool ReturnHPLow { get; set; } = false;
        public static int HPLowThreshold { get; set; } = 15;
        public static bool ReturnMPLow { get; set; } = false;
        public static int MPLowThreshold { get; set; } = 15;
        public static bool ReturnDurabilityLow { get; set; } = false;
        public static int DurabilityLowThreshold { get; set; } = 5;
        public static bool ReturnLevelUp { get; set; } = false;

        // Time-based Return Triggers
        public static bool ReturnNextHourEnabled { get; set; } = false;
        public static int ReturnNextHourMinutes { get; set; } = 10;
        public static bool ReturnEveryEnabled { get; set; } = false;
        public static int ReturnEveryMinutes { get; set; } = 1440;
        public static bool ReturnAtTimeEnabled { get; set; } = false;
        public static string ReturnAtTimeValue { get; set; } = "00:00";
        public static bool ReturnAtTimeStopBot { get; set; } = false;
        public static bool ReturnDisconnectMinutesEnabled { get; set; } = false;
        public static int ReturnDisconnectMinutes { get; set; } = 300;
        public static bool ReturnNotAttackedEnabled { get; set; } = false;
        public static int ReturnNotAttackedMinutes { get; set; } = 10;
        public static bool ReturnUniqueSpawn { get; set; } = false;

        private static readonly object RuntimeLock = new object();
        private static DateTime lastTownReturnCheck = DateTime.MinValue;
        private static DateTime nextSkillHealingUtc = DateTime.MinValue;
        private static DateTime nextSkillManaUtc = DateTime.MinValue;
        private static DateTime nextSkillCureUtc = DateTime.MinValue;
        private static DateTime nextPetProtectionUtc = DateTime.MinValue;
        private static DateTime lastPetSummonAttemptUtc = DateTime.MinValue;
        private static DateTime characterDeadSinceUtc = DateTime.MinValue;
        private static DateTime lastReturnAtTimeUtc = DateTime.MinValue;
        private static DateTime lastReturnEveryUtc = DateTime.MinValue;
        private static DateTime lastReturnNextHourUtc = DateTime.MinValue;
        private static bool stopAfterReturn;
        private static bool levelUpPending;

        /// <summary>
        /// Runs every protection rule from the joined-game loop.
        /// Event-driven checks remain available for fast HP/MP updates, while
        /// this tick covers rules that do not have a packet event of their own.
        /// </summary>
        public static void RunTick()
        {
            if (InfoManager.Character == null || !InfoManager.inGame || !Bot.Get.isBotting)
                return;

            if (!Monitor.TryEnter(RuntimeLock))
                return;

            try
            {
                if (InfoManager.Character.LifeStateType == SRModel.LifeState.Dead)
                {
                    CheckTownReturnTriggers();
                    return;
                }

                CheckSkillHealing();
                CheckSkillMana();
                CheckSkillCure();
                CheckPetProtection();
                CheckTownReturnTriggers();
            }
            finally
            {
                Monitor.Exit(RuntimeLock);
            }
        }

        /// <summary>
        /// Resets transient protection state when the game session ends.
        /// </summary>
        public static void ResetRuntimeState()
        {
            lock (RuntimeLock)
            {
                lastTownReturnCheck = DateTime.MinValue;
                nextSkillHealingUtc = DateTime.MinValue;
                nextSkillManaUtc = DateTime.MinValue;
                nextSkillCureUtc = DateTime.MinValue;
                nextPetProtectionUtc = DateTime.MinValue;
                lastPetSummonAttemptUtc = DateTime.MinValue;
                characterDeadSinceUtc = DateTime.MinValue;
                stopAfterReturn = false;
                levelUpPending = false;
            }
        }

        public static void NotifyCharacterDead()
        {
            lock (RuntimeLock)
            {
                if (characterDeadSinceUtc == DateTime.MinValue)
                    characterDeadSinceUtc = DateTime.UtcNow;
            }
        }

        public static void NotifyLevelUp()
        {
            lock (RuntimeLock)
            {
                levelUpPending = true;
            }
        }

        private static bool IsActionDue(ref DateTime nextAllowedUtc, int delayMilliseconds)
        {
            DateTime now = DateTime.UtcNow;
            if (now < nextAllowedUtc)
                return false;

            nextAllowedUtc = now.AddMilliseconds(delayMilliseconds);
            return true;
        }

        public static void CheckSkillHealing()
        {
            if (!UseSkillHP || InfoManager.Character == null || InfoManager.Character.Skills == null)
                return;

            if (InfoManager.Character.GetHPPercent() <= SkillHPPercent
                && IsActionDue(ref nextSkillHealingUtc, 1000))
            {
                SRSkill healSkill = null;
                for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
                {
                    var s = InfoManager.Character.Skills.GetAt(i);
                    if (s != null && s.isCastingEnabled && !string.IsNullOrEmpty(s.ServerName)
                        && (s.ServerName.IndexOf("HEAL", StringComparison.OrdinalIgnoreCase) >= 0
                            || s.ServerName.IndexOf("RECOVERY_DIV", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        if (healSkill == null || s.Level > healSkill.Level)
                            healSkill = s;
                    }
                }

                if (healSkill != null)
                {
                    PacketBuilder.CastSkill(healSkill.ID, InfoManager.Character.UniqueID);
                }
            }
        }

        public static void CheckSkillMana()
        {
            if (!UseSkillMP || InfoManager.Character == null || InfoManager.Character.Skills == null)
                return;

            if (InfoManager.Character.GetMPPercent() <= SkillMPPercent
                && IsActionDue(ref nextSkillManaUtc, 1000))
            {
                SRSkill mpSkill = null;
                for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
                {
                    var s = InfoManager.Character.Skills.GetAt(i);
                    if (s != null && s.isCastingEnabled && !string.IsNullOrEmpty(s.ServerName)
                        && (s.ServerName.IndexOf("MANA_CYCLE", StringComparison.OrdinalIgnoreCase) >= 0
                            || s.ServerName.IndexOf("MANA_ORBIT", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        if (mpSkill == null || s.Level > mpSkill.Level)
                            mpSkill = s;
                    }
                }

                if (mpSkill != null)
                {
                    PacketBuilder.CastSkill(mpSkill.ID, InfoManager.Character.UniqueID);
                }
            }
        }

        public static void CheckSkillCure()
        {
            if (!UseSkillBadStatus || InfoManager.Character == null || InfoManager.Character.Skills == null)
                return;

            if (InfoManager.Character.BadStatusFlags != SRModel.BadStatus.None
                && IsActionDue(ref nextSkillCureUtc, 1000))
            {
                SRSkill cureSkill = null;
                for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
                {
                    var s = InfoManager.Character.Skills.GetAt(i);
                    if (s != null && s.isCastingEnabled && !string.IsNullOrEmpty(s.ServerName)
                        && (s.ServerName.IndexOf("PURIFY", StringComparison.OrdinalIgnoreCase) >= 0
                            || s.ServerName.IndexOf("HOLY_SPELL", StringComparison.OrdinalIgnoreCase) >= 0
                            || s.ServerName.IndexOf("CURE", StringComparison.OrdinalIgnoreCase) >= 0))
                    {
                        if (cureSkill == null || s.Level > cureSkill.Level)
                            cureSkill = s;
                    }
                }

                if (cureSkill != null)
                {
                    PacketBuilder.CastSkill(cureSkill.ID, InfoManager.Character.UniqueID);
                }
            }
        }

        public static void CheckPetProtection()
        {
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null
                || InfoManager.MyPets == null || !InfoManager.inGame
                || !IsActionDue(ref nextPetProtectionUtc, 1000))
                return;

            // 1. Revive dead pet
            if (RevivePet)
            {
                var deadPet = InfoManager.MyPets.Find(p => p != null && p.LifeStateType == SRModel.LifeState.Dead);
                if (deadPet != null)
                {
                    byte slot = 0;
                    if (Bot.Get.FindItem(3, 1, 8, ref slot, "REVIVAL") || Bot.Get.FindItem(3, 1, 8, ref slot, "GRASS"))
                    {
                        PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot, deadPet.UniqueID);
                        Window.Get?.Log("Protection: Used Revival Scroll on Pet!");
                        return;
                    }
                }
            }

            // 2. Auto summon pet if none is summoned (with 15s throttle to prevent packet flooding)
            if (AutoSummonPet && InfoManager.MyPets.Count == 0 && InfoManager.inGame)
            {
                DateTime nowUtc = DateTime.UtcNow;
                if ((nowUtc - lastPetSummonAttemptUtc).TotalSeconds >= 15)
                {
                    byte slot = 0;
                    if (Bot.Get.FindItem(3, 1, 8, ref slot, "_SUMMON") || Bot.Get.FindItem(3, 1, 8, ref slot, "_PET_"))
                    {
                        lastPetSummonAttemptUtc = nowUtc;
                        PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot);
                        Window.Get?.Log("Protection: Auto summoned Pet!");
                    }
                }
            }
        }

        public static bool CheckTownReturnTriggers()
        {
            if (InfoManager.Character == null || !InfoManager.inGame)
                return false;

            if (!Bot.Get.isBotting)
                return false;

            if (!Monitor.TryEnter(RuntimeLock))
                return false;

            try
            {
                DateTime now = DateTime.UtcNow;
                bool isDead = InfoManager.Character.LifeStateType == SRModel.LifeState.Dead;

                if (isDead)
                {
                    if (characterDeadSinceUtc == DateTime.MinValue)
                        characterDeadSinceUtc = now;
                }
                else
                {
                    characterDeadSinceUtc = DateTime.MinValue;
                }

                ProtectionPolicyInput input = BuildPolicyInput(now);
                ProtectionPolicyOptions options = GetPolicyOptions();
                ProtectionDecision decision = ProtectionPolicy.Evaluate(input, options);

                // Stopping after a successful return must not wait for the 3-second
                // trigger throttle; the character may already be in town by then.
                if (decision == ProtectionDecision.StopBotInTown)
                {
                    stopAfterReturn = false;
                    Window.Get?.Log("Protection: Returned to town; bot stopped.");
                    Bot.Get.Stop();
                    return true;
                }

                if ((now - lastTownReturnCheck).TotalSeconds < 3)
                    return false;

                lastTownReturnCheck = now;

                if (input.DurabilityLow && UseRepairHammer)
                {
                    if (TryUseRepairHammer())
                    {
                        input.DurabilityLow = HasLowDurability(InfoManager.Character.Inventory);
                        decision = ProtectionPolicy.Evaluate(input, options);
                    }
                }

                if (decision == ProtectionDecision.ReturnToTown)
                    return TryReturnToTown(GetReturnReason(input));

                // Time-based return triggers
                if (ReturnAtTimeEnabled && !string.IsNullOrEmpty(ReturnAtTimeValue))
                {
                    string nowStr = DateTime.Now.ToString("HH:mm");
                    if (nowStr == ReturnAtTimeValue && (now - lastReturnAtTimeUtc).TotalSeconds > 70)
                    {
                        lastReturnAtTimeUtc = now;
                        if (ReturnAtTimeStopBot) stopAfterReturn = true;
                        return TryReturnToTown("Configured time reached (" + ReturnAtTimeValue + ").");
                    }
                }

                if (ReturnEveryEnabled && ReturnEveryMinutes > 0)
                {
                    if (lastReturnEveryUtc == DateTime.MinValue) lastReturnEveryUtc = now;
                    else if ((now - lastReturnEveryUtc).TotalMinutes >= ReturnEveryMinutes)
                    {
                        lastReturnEveryUtc = now;
                        return TryReturnToTown("Periodic interval elapsed (" + ReturnEveryMinutes + " min).");
                    }
                }

                if (ReturnNextHourEnabled && ReturnNextHourMinutes > 0)
                {
                    int minToHour = 60 - DateTime.Now.Minute;
                    if (minToHour <= ReturnNextHourMinutes && (now - lastReturnNextHourUtc).TotalSeconds > 120)
                    {
                        lastReturnNextHourUtc = now;
                        return TryReturnToTown("Approaching next hour (" + minToHour + " min left).");
                    }
                }

                if (IsConfiguredTriggerActive(input, options) ||
                    (!input.IsAlive && options.ReturnDeadWithDelay && input.DeadDelayElapsed))
                {
                    Window.Get?.LogProcess("Protection: A return condition is active, but no Return Scroll was found.", Window.ProcessState.Warning);
                }

                return false;
            }
            finally
            {
                Monitor.Exit(RuntimeLock);
            }
        }

        private static ProtectionPolicyOptions GetPolicyOptions()
        {
            return new ProtectionPolicyOptions
            {
                UseReturnScrolls = UseReturnScrolls,
                ReturnDeadWithDelay = ReturnDeadWithDelay,
                StopBotInTown = StopBotInTown,
                ReturnNoArrows = ReturnNoArrows,
                ReturnFullInventory = ReturnFullInventory,
                ReturnFullPetInventory = ReturnFullPetInventory,
                ReturnHPLow = ReturnHPLow,
                ReturnMPLow = ReturnMPLow,
                ReturnDurabilityLow = ReturnDurabilityLow,
                ReturnLevelUp = ReturnLevelUp,
                ReturnOutOfPetRecoveryKits = ReturnOutOfPetRecoveryKits,
                ReturnOutOfPetRevive = ReturnOutOfPetRevive,
                ReturnOutOfPetFeed = ReturnOutOfPetFeed,
                ReturnOutOfPetAbnormalPill = ReturnOutOfPetAbnormalPill,
                ReturnOutOfTransportRecoveryKits = ReturnOutOfTransportRecoveryKits
            };
        }

        private static ProtectionPolicyInput BuildPolicyInput(DateTime now)
        {
            xList<SRItem> inventory = InfoManager.Character.Inventory;
            return new ProtectionPolicyInput
            {
                IsInGame = InfoManager.inGame,
                IsBotting = Bot.Get.isBotting,
                IsInTown = TownManager.Get.IsNearTown(InfoManager.Character.GetRealtimePosition()),
                IsAlive = InfoManager.Character.LifeStateType == SRModel.LifeState.Alive,
                StopAfterReturn = stopAfterReturn,
                DeadDelayElapsed = characterDeadSinceUtc != DateTime.MinValue &&
                    (now - characterDeadSinceUtc).TotalSeconds >= Math.Max(0, DeadDelaySeconds),
                HasReturnScroll = HasReturnScroll(inventory),
                NoArrows = HasNoArrows(inventory),
                FullInventory = IsInventoryFull(inventory),
                FullPetInventory = IsPetInventoryFull(),
                HPLow = CountInventoryQuantity(inventory, 3, 1, 1) <= HPLowThreshold,
                MPLow = CountInventoryQuantity(inventory, 3, 1, 2) <= MPLowThreshold,
                DurabilityLow = HasLowDurability(inventory),
                LevelUpPending = levelUpPending,
                OutOfPetRecoveryKits = ReturnOutOfPetRecoveryKits && HasNoPetItem(inventory, "RECOVERY"),
                OutOfPetRevive = ReturnOutOfPetRevive && HasNoPetItem(inventory, "REVIVAL", "GRASS"),
                OutOfPetFeed = ReturnOutOfPetFeed && HasNoPetItem(inventory, "FEED", "FOOD"),
                OutOfPetAbnormalPill = ReturnOutOfPetAbnormalPill && HasNoPetItem(inventory, "PILL", "CURE"),
                OutOfTransportRecoveryKits = ReturnOutOfTransportRecoveryKits && HasNoPetItem(inventory, "TRANSPORT", "RECOVERY")
            };
        }

        private static bool IsConfiguredTriggerActive(ProtectionPolicyInput input, ProtectionPolicyOptions options)
        {
            return ProtectionPolicy.HasReturnTrigger(input, options);
        }

        private static string GetReturnReason(ProtectionPolicyInput input)
        {
            if (!input.IsAlive && ReturnDeadWithDelay)
                return "Character is dead and the configured delay elapsed.";
            if (ReturnLevelUp && input.LevelUpPending)
                return "Level-up protection triggered.";
            if (ReturnFullPetInventory && input.FullPetInventory)
                return "Pet inventory is full.";
            if (ReturnFullInventory && input.FullInventory)
                return "Inventory is full.";
            if (ReturnNoArrows && input.NoArrows)
                return "No arrows or bolts remain.";
            if (ReturnHPLow && input.HPLow)
                return "HP potions are low (" + HPLowThreshold + " threshold).";
            if (ReturnMPLow && input.MPLow)
                return "MP potions are low (" + MPLowThreshold + " threshold).";
            if (ReturnDurabilityLow && input.DurabilityLow)
                return "Equipment durability is low.";
            if (ReturnOutOfPetRecoveryKits && input.OutOfPetRecoveryKits)
                return "Out of pet recovery kits.";
            if (ReturnOutOfPetRevive && input.OutOfPetRevive)
                return "Out of pet revival items.";
            if (ReturnOutOfPetFeed && input.OutOfPetFeed)
                return "Out of pet feed items.";
            if (ReturnOutOfPetAbnormalPill && input.OutOfPetAbnormalPill)
                return "Out of pet abnormal state potions.";
            if (ReturnOutOfTransportRecoveryKits && input.OutOfTransportRecoveryKits)
                return "Out of transport recovery kits.";
            return "A protection condition was met.";
        }

        private static bool TryReturnToTown(string reason)
        {
            if (Bot.Get.UseReturnScroll())
            {
                stopAfterReturn = StopBotInTown;
                levelUpPending = false;
                Window.Get?.Log("Protection: " + reason + " Returning to town...");
                return true;
            }

            Window.Get?.LogProcess("Protection: " + reason + " but no Return Scroll was found.", Window.ProcessState.Warning);
            return false;
        }

        private static bool HasReturnScroll(xList<SRItem> inventory)
        {
            if (!UseReturnScrolls || inventory == null)
                return false;

            for (byte i = 13; i < inventory.Capacity; i++)
            {
                SRItem item = inventory[i];
                if (item == null || !item.isType(3, 3, 1))
                    continue;

                switch (item.ServerName)
                {
                    case "ITEM_ETC_SCROLL_RETURN_01":
                    case "ITEM_ETC_SCROLL_RETURN_02":
                    case "ITEM_ETC_SCROLL_RETURN_03":
                    case "ITEM_ETC_SCROLL_RETURN_NEWBIE_01":
                    case "ITEM_ETC_E041225_SANTA_WINGS":
                    case "ITEM_MALL_RETURN_SCROLL_HIGH_SPEED":
                    case "ITEM_EVENT_RETURN_SCROLL_HIGH_SPEED":
                        return true;
                }
            }

            return false;
        }

        private static bool HasNoPetItem(xList<SRItem> inventory, params string[] keywords)
        {
            if (inventory == null) return true;
            for (byte i = 13; i < inventory.Capacity; i++)
            {
                var item = inventory[i];
                if (item == null || item.ID2 != 3) continue;
                string sName = item.ServerName ?? "";
                for (int k = 0; k < keywords.Length; k++)
                {
                    if (sName.IndexOf(keywords[k], StringComparison.OrdinalIgnoreCase) >= 0)
                        return false;
                }
            }
            return true;
        }

        private static bool HasNoArrows(xList<SRItem> inventory)
        {
            if (inventory == null)
                return false;

            SRTypes.Weapon weapon = Bot.Get.GetMyWeaponType();
            if (weapon != SRTypes.Weapon.Bow && weapon != SRTypes.Weapon.Crossbow)
                return false;

            SRItem equippedAmmo = inventory.Capacity > 7 ? inventory[7] : null;
            if (equippedAmmo != null && equippedAmmo.Quantity > 0)
                return false;

            byte slot = 0;
            return !Bot.Get.FindItem(3, 1, 7, ref slot);
        }

        private static bool IsInventoryFull(xList<SRItem> inventory)
        {
            if (inventory == null || inventory.Capacity <= 13)
                return true;

            for (byte i = 13; i < inventory.Capacity; i++)
            {
                if (inventory[i] == null)
                    return false;
            }

            return true;
        }

        private static bool IsPetInventoryFull()
        {
            if (InfoManager.MyPets == null)
                return false;

            for (int i = 0; i < InfoManager.MyPets.Count; i++)
            {
                SRCoService pet = InfoManager.MyPets.GetAt(i);
                if (pet == null || !pet.isPickPet() || pet.Inventory == null || pet.Inventory.Capacity == 0)
                    continue;

                for (int slot = 0; slot < pet.Inventory.Capacity; slot++)
                {
                    if (pet.Inventory[slot] == null)
                        return false;
                }

                return true;
            }

            return false;
        }

        private static int CountInventoryQuantity(xList<SRItem> inventory, byte id2, byte id3, byte id4)
        {
            if (inventory == null)
                return 0;

            int quantity = 0;
            for (byte i = 13; i < inventory.Capacity; i++)
            {
                SRItem item = inventory[i];
                if (item != null && item.isType(id2, id3, id4))
                    quantity += item.Quantity;
            }

            return quantity;
        }

        private static bool HasLowDurability(xList<SRItem> inventory)
        {
            if (inventory == null)
                return false;

            // Slots 0..7 are armor, weapon and shield. Avatar, job and
            // accessories do not participate in durability protection.
            for (byte i = 0; i <= 7 && i < inventory.Capacity; i++)
            {
                SREquipable equip = inventory[i] as SREquipable;
                if (equip != null && !equip.isAvatar() && !equip.isJob()
                    && equip.ItemType != SREquipable.Equipable.AccesoriesCH
                    && equip.ItemType != SREquipable.Equipable.AccesoriesEU
                    && equip.Durability > 0 && equip.Durability <= DurabilityLowThreshold)
                    return true;
            }

            return false;
        }

        public static bool TryUseRepairHammer()
        {
            try
            {
                var chr = InfoManager.Character;
                if (chr == null || chr.Inventory == null) return false;
                for (byte s = 13; s < chr.Inventory.Capacity; s++)
                {
                    var it = chr.Inventory[s];
                    if (it == null) continue;
                    string sn = it.ServerName ?? "";
                    string name = it.Name ?? "";
                    if (sn.IndexOf("REPAIR_HAMMER", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        sn.IndexOf("HAMMER", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("Repair Hammer", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("Tamir", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Window.Get?.Log($"Protection: Repair Hammer kullanılıyor [{it.Name}]...");
                        bool ok = PacketBuilder.UseItem(it, s);
                        Thread.Sleep(500);
                        return ok;
                    }
                }
            }
            catch { }
            return false;
        }

        public static bool TryUseReverseReturnScroll(byte targetPoint = 2)
        {
            try
            {
                var chr = InfoManager.Character;
                if (chr == null || chr.Inventory == null) return false;
                for (byte s = 13; s < chr.Inventory.Capacity; s++)
                {
                    var it = chr.Inventory[s];
                    if (it == null) continue;
                    string sn = it.ServerName ?? "";
                    string name = it.Name ?? "";
                    if (sn.IndexOf("REVERSE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("Reverse", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        Window.Get?.Log($"Reverse Scroll: Kullanılıyor [{it.Name}] (hedef={targetPoint})...");
                        bool ok = PacketBuilder.UseItem(it, s);
                        Thread.Sleep(800);
                        try
                        {
                            Packet p = new Packet(Agent.Opcode.CLIENT_TELEPORT_USE_REQUEST);
                            p.WriteByte(targetPoint);
                            Bot.Get.Proxy.Agent.InjectToServer(p);
                        }
                        catch { }
                        return ok;
                    }
                }
            }
            catch { }
            return false;
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["UseSkillHP"] = UseSkillHP;
            json["SkillHPPercent"] = SkillHPPercent;
            json["UseSkillMP"] = UseSkillMP;
            json["SkillMPPercent"] = SkillMPPercent;
            json["UseSkillBadStatus"] = UseSkillBadStatus;
            json["RevivePet"] = RevivePet;
            json["AutoSummonPet"] = AutoSummonPet;
            json["ReturnDeadWithDelay"] = ReturnDeadWithDelay;
            json["DeadDelaySeconds"] = DeadDelaySeconds;
            json["StopBotInTown"] = StopBotInTown;
            json["ReturnNoArrows"] = ReturnNoArrows;
            json["ReturnFullInventory"] = ReturnFullInventory;
            json["ReturnFullPetInventory"] = ReturnFullPetInventory;
            json["ReturnHPLow"] = ReturnHPLow;
            json["HPLowThreshold"] = HPLowThreshold;
            json["ReturnMPLow"] = ReturnMPLow;
            json["MPLowThreshold"] = MPLowThreshold;
            json["UseReturnScrolls"] = UseReturnScrolls;
            json["UseReverseOnDeath"] = UseReverseOnDeath;
            json["UseReverseAfterTown"] = UseReverseAfterTown;
            json["UseSpeedDrugs"] = UseSpeedDrugs;
            json["SpeedDrugsOnlyInScript"] = SpeedDrugsOnlyInScript;
            json["UseRepairHammer"] = UseRepairHammer;
            json["ReturnOutOfPetRecoveryKits"] = ReturnOutOfPetRecoveryKits;
            json["ReturnOutOfPetRevive"] = ReturnOutOfPetRevive;
            json["ReturnOutOfPetFeed"] = ReturnOutOfPetFeed;
            json["ReturnOutOfPetAbnormalPill"] = ReturnOutOfPetAbnormalPill;
            json["ReturnOutOfTransportRecoveryKits"] = ReturnOutOfTransportRecoveryKits;
            json["ReturnLevelUp"] = ReturnLevelUp;
            json["ReturnNextHourEnabled"] = ReturnNextHourEnabled;
            json["ReturnNextHourMinutes"] = ReturnNextHourMinutes;
            json["ReturnEveryEnabled"] = ReturnEveryEnabled;
            json["ReturnEveryMinutes"] = ReturnEveryMinutes;
            json["ReturnAtTimeEnabled"] = ReturnAtTimeEnabled;
            json["ReturnAtTimeValue"] = ReturnAtTimeValue;
            json["ReturnAtTimeStopBot"] = ReturnAtTimeStopBot;
            json["ReturnDisconnectMinutesEnabled"] = ReturnDisconnectMinutesEnabled;
            json["ReturnDisconnectMinutes"] = ReturnDisconnectMinutes;
            json["ReturnNotAttackedEnabled"] = ReturnNotAttackedEnabled;
            json["ReturnNotAttackedMinutes"] = ReturnNotAttackedMinutes;
            json["ReturnUniqueSpawn"] = ReturnUniqueSpawn;
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null)
                return;

            if (json.ContainsKey("UseSkillHP")) UseSkillHP = (bool)json["UseSkillHP"];
            if (json.ContainsKey("SkillHPPercent")) SkillHPPercent = (byte)json["SkillHPPercent"];
            if (json.ContainsKey("UseSkillMP")) UseSkillMP = (bool)json["UseSkillMP"];
            if (json.ContainsKey("SkillMPPercent")) SkillMPPercent = (byte)json["SkillMPPercent"];
            if (json.ContainsKey("UseSkillBadStatus")) UseSkillBadStatus = (bool)json["UseSkillBadStatus"];
            if (json.ContainsKey("RevivePet")) RevivePet = (bool)json["RevivePet"];
            if (json.ContainsKey("AutoSummonPet")) AutoSummonPet = (bool)json["AutoSummonPet"];
            if (json.ContainsKey("UseReturnScrolls")) UseReturnScrolls = (bool)json["UseReturnScrolls"];
            if (json.ContainsKey("UseReverseOnDeath")) UseReverseOnDeath = (bool)json["UseReverseOnDeath"];
            if (json.ContainsKey("UseReverseAfterTown")) UseReverseAfterTown = (bool)json["UseReverseAfterTown"];
            if (json.ContainsKey("UseSpeedDrugs")) UseSpeedDrugs = (bool)json["UseSpeedDrugs"];
            if (json.ContainsKey("SpeedDrugsOnlyInScript")) SpeedDrugsOnlyInScript = (bool)json["SpeedDrugsOnlyInScript"];
            if (json.ContainsKey("UseRepairHammer")) UseRepairHammer = (bool)json["UseRepairHammer"];
            if (json.ContainsKey("ReturnOutOfPetRecoveryKits")) ReturnOutOfPetRecoveryKits = (bool)json["ReturnOutOfPetRecoveryKits"];
            if (json.ContainsKey("ReturnOutOfPetRevive")) ReturnOutOfPetRevive = (bool)json["ReturnOutOfPetRevive"];
            if (json.ContainsKey("ReturnOutOfPetFeed")) ReturnOutOfPetFeed = (bool)json["ReturnOutOfPetFeed"];
            if (json.ContainsKey("ReturnOutOfPetAbnormalPill")) ReturnOutOfPetAbnormalPill = (bool)json["ReturnOutOfPetAbnormalPill"];
            if (json.ContainsKey("ReturnOutOfTransportRecoveryKits")) ReturnOutOfTransportRecoveryKits = (bool)json["ReturnOutOfTransportRecoveryKits"];
            if (json.ContainsKey("ReturnDeadWithDelay")) ReturnDeadWithDelay = (bool)json["ReturnDeadWithDelay"];
            if (json.ContainsKey("DeadDelaySeconds")) DeadDelaySeconds = (int)json["DeadDelaySeconds"];
            if (json.ContainsKey("StopBotInTown")) StopBotInTown = (bool)json["StopBotInTown"];
            if (json.ContainsKey("ReturnNoArrows")) ReturnNoArrows = (bool)json["ReturnNoArrows"];
            if (json.ContainsKey("ReturnFullInventory")) ReturnFullInventory = (bool)json["ReturnFullInventory"];
            if (json.ContainsKey("ReturnFullPetInventory")) ReturnFullPetInventory = (bool)json["ReturnFullPetInventory"];
            if (json.ContainsKey("ReturnHPLow")) ReturnHPLow = (bool)json["ReturnHPLow"];
            if (json.ContainsKey("HPLowThreshold")) HPLowThreshold = (int)json["HPLowThreshold"];
            if (json.ContainsKey("ReturnMPLow")) ReturnMPLow = (bool)json["ReturnMPLow"];
            if (json.ContainsKey("MPLowThreshold")) MPLowThreshold = (int)json["MPLowThreshold"];
            if (json.ContainsKey("ReturnDurabilityLow")) ReturnDurabilityLow = (bool)json["ReturnDurabilityLow"];
            if (json.ContainsKey("DurabilityLowThreshold")) DurabilityLowThreshold = (int)json["DurabilityLowThreshold"];
            if (json.ContainsKey("ReturnLevelUp")) ReturnLevelUp = (bool)json["ReturnLevelUp"];
            if (json.ContainsKey("ReturnNextHourEnabled")) ReturnNextHourEnabled = (bool)json["ReturnNextHourEnabled"];
            if (json.ContainsKey("ReturnNextHourMinutes")) ReturnNextHourMinutes = (int)json["ReturnNextHourMinutes"];
            if (json.ContainsKey("ReturnEveryEnabled")) ReturnEveryEnabled = (bool)json["ReturnEveryEnabled"];
            if (json.ContainsKey("ReturnEveryMinutes")) ReturnEveryMinutes = (int)json["ReturnEveryMinutes"];
            if (json.ContainsKey("ReturnAtTimeEnabled")) ReturnAtTimeEnabled = (bool)json["ReturnAtTimeEnabled"];
            if (json.ContainsKey("ReturnAtTimeValue")) ReturnAtTimeValue = (string)json["ReturnAtTimeValue"];
            if (json.ContainsKey("ReturnAtTimeStopBot")) ReturnAtTimeStopBot = (bool)json["ReturnAtTimeStopBot"];
            if (json.ContainsKey("ReturnDisconnectMinutesEnabled")) ReturnDisconnectMinutesEnabled = (bool)json["ReturnDisconnectMinutesEnabled"];
            if (json.ContainsKey("ReturnDisconnectMinutes")) ReturnDisconnectMinutes = (int)json["ReturnDisconnectMinutes"];
            if (json.ContainsKey("ReturnNotAttackedEnabled")) ReturnNotAttackedEnabled = (bool)json["ReturnNotAttackedEnabled"];
            if (json.ContainsKey("ReturnNotAttackedMinutes")) ReturnNotAttackedMinutes = (int)json["ReturnNotAttackedMinutes"];
            if (json.ContainsKey("ReturnUniqueSpawn")) ReturnUniqueSpawn = (bool)json["ReturnUniqueSpawn"];
        }
    }
}
