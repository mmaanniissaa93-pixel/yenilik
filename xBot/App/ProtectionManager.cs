using System;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;

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

        private static DateTime lastTownReturnCheck = DateTime.MinValue;

        public static void CheckSkillHealing()
        {
            if (!UseSkillHP || InfoManager.Character == null || InfoManager.Character.Skills == null)
                return;

            if (InfoManager.Character.GetHPPercent() <= SkillHPPercent)
            {
                SRSkill healSkill = null;
                for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
                {
                    var s = InfoManager.Character.Skills.GetAt(i);
                    if (s != null && !string.IsNullOrEmpty(s.ServerName) && (s.ServerName.Contains("HEAL") || s.ServerName.Contains("RECOVERY_DIV")))
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

            if (InfoManager.Character.GetMPPercent() <= SkillMPPercent)
            {
                SRSkill mpSkill = null;
                for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
                {
                    var s = InfoManager.Character.Skills.GetAt(i);
                    if (s != null && !string.IsNullOrEmpty(s.ServerName) && (s.ServerName.Contains("MANA_CYCLE") || s.ServerName.Contains("MANA_ORBIT")))
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

            if (InfoManager.Character.BadStatusFlags != SRModel.BadStatus.None)
            {
                SRSkill cureSkill = null;
                for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
                {
                    var s = InfoManager.Character.Skills.GetAt(i);
                    if (s != null && !string.IsNullOrEmpty(s.ServerName) && (s.ServerName.Contains("PURIFY") || s.ServerName.Contains("HOLY_SPELL") || s.ServerName.Contains("CURE")))
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
            if (InfoManager.Character == null || InfoManager.MyPets == null)
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

            // 2. Auto summon pet if none is summoned
            if (AutoSummonPet && InfoManager.MyPets.Count == 0 && InfoManager.inGame)
            {
                byte slot = 0;
                if (Bot.Get.FindItem(3, 1, 8, ref slot, "_SUMMON") || Bot.Get.FindItem(3, 1, 8, ref slot, "_PET_"))
                {
                    PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot);
                    Window.Get?.Log("Protection: Auto summoned Pet!");
                }
            }
        }

        public static bool CheckTownReturnTriggers()
        {
            if (InfoManager.Character == null || !InfoManager.inGame)
                return false;

            // Only run town return triggers when the bot is actively running
            if (!Bot.Get.isBotting)
                return false;

            // Throttle checks to once every 3 seconds
            if ((DateTime.UtcNow - lastTownReturnCheck).TotalSeconds < 3)
                return false;

            lastTownReturnCheck = DateTime.UtcNow;

            // Check if character is alive
            if (InfoManager.Character.LifeStateType != SRModel.LifeState.Alive)
                return false;

            var inventory = InfoManager.Character.Inventory;
            if (inventory == null)
                return false;

            // 1. Full inventory check
            if (ReturnFullInventory)
            {
                int freeSlots = 0;
                for (byte i = 13; i < inventory.Capacity; i++)
                {
                    if (inventory[i] == null)
                        freeSlots++;
                }

                if (freeSlots == 0)
                {
                    Window.Get?.Log("Town Trigger: Inventory full! Returning to town...");
                    Bot.Get.UseReturnScroll();
                    return true;
                }
            }

            // 2. No arrows or bolts left
            if (ReturnNoArrows)
            {
                var weapon = Bot.Get.GetMyWeaponType();
                if (weapon == SRTypes.Weapon.Bow || weapon == SRTypes.Weapon.Crossbow)
                {
                    var ammoItem = inventory[7];
                    if (ammoItem == null || ammoItem.Quantity == 0)
                    {
                        byte dummySlot = 0;
                        bool hasInBag = Bot.Get.FindItem(3, 1, 7, ref dummySlot);
                        if (!hasInBag)
                        {
                            Window.Get?.Log("Town Trigger: No arrows/bolts left! Returning to town...");
                            Bot.Get.UseReturnScroll();
                            return true;
                        }
                    }
                }
            }

            // 3. HP / MP Potions low
            if (ReturnHPLow || ReturnMPLow)
            {
                int hpCount = 0;
                int mpCount = 0;
                for (byte i = 13; i < inventory.Capacity; i++)
                {
                    var item = inventory[i];
                    if (item != null)
                    {
                        if (item.isType(3, 1, 1)) hpCount += item.Quantity;
                        else if (item.isType(3, 1, 2)) mpCount += item.Quantity;
                    }
                }

                if (ReturnHPLow && hpCount <= HPLowThreshold)
                {
                    Window.Get?.Log("Town Trigger: HP potions low (" + hpCount + ")! Returning to town...");
                    Bot.Get.UseReturnScroll();
                    return true;
                }

                if (ReturnMPLow && mpCount <= MPLowThreshold)
                {
                    Window.Get?.Log("Town Trigger: MP potions low (" + mpCount + ")! Returning to town...");
                    Bot.Get.UseReturnScroll();
                    return true;
                }
            }

            // 4. Equipment durability low
            if (ReturnDurabilityLow)
            {
                // In Silkroad: slots 0..5 are armor, slot 6 is weapon, slot 7 is shield.
                // Slots 8 (job/avatar), 9..12 (accessories: earring, necklace, rings) DO NOT HAVE DURABILITY!
                for (byte i = 0; i <= 7; i++)
                {
                    var equip = inventory[i] as SREquipable;
                    if (equip != null && !equip.isAvatar() && !equip.isJob()
                        && equip.ItemType != SREquipable.Equipable.AccesoriesCH
                        && equip.ItemType != SREquipable.Equipable.AccesoriesEU
                        && equip.Durability > 0)
                    {
                        if (equip.Durability <= DurabilityLowThreshold)
                        {
                            Window.Get?.Log("Town Trigger: Equipment durability critical (" + equip.Durability + ") on " + equip.Name + "! Returning to town...");
                            Bot.Get.UseReturnScroll();
                            return true;
                        }
                    }
                }
            }

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
            json["ReturnDurabilityLow"] = ReturnDurabilityLow;
            json["DurabilityLowThreshold"] = DurabilityLowThreshold;
            json["ReturnLevelUp"] = ReturnLevelUp;
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
        }
    }
}
