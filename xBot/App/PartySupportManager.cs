using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Party;

namespace xBot.App
{
    public static class PartySupportManager
    {
        public static bool PartyHealEnabled { get; set; } = true;
        public static byte PartyHealHPPercent { get; set; } = 70;
        public static bool PartyAutoRessEnabled { get; set; } = true;
        public static bool PartyCureEnabled { get; set; } = true;
        public static bool PartyBuffsEnabled { get; set; } = true;

        private static DateTime lastSupportActionUtc = DateTime.MinValue;
        private static readonly object supportLock = new object();

        public static void RunTick()
        {
            if (InfoManager.Character == null || !InfoManager.inGame || !Bot.Get.isBotting)
                return;

            if (InfoManager.Party == null || InfoManager.Party.Members == null || InfoManager.Party.Members.Count <= 1)
                return;

            if (InfoManager.Character.LifeStateType == SRModel.LifeState.Dead)
                return;

            // Throttle party support actions to prevent packet spam
            if ((DateTime.UtcNow - lastSupportActionUtc).TotalMilliseconds < 1500)
                return;

            if (!Monitor.TryEnter(supportLock))
                return;

            try
            {
                SRCoord myPos = InfoManager.Character.GetRealtimePosition();
                if (myPos == null)
                    return;

                // 1. Check for Dead Party Members to Resurrect
                if (PartyAutoRessEnabled)
                {
                    for (int i = 0; i < InfoManager.Party.Members.Count; i++)
                    {
                        SRPartyMember member = InfoManager.Party.Members.GetAt(i);
                        if (member == null || member.Name == InfoManager.Character.Name)
                            continue;

                        if (member.HPPercent == 0)
                        {
                            SRPlayer livePlayer = InfoManager.Players.Find(p => p != null && p.Name == member.Name);
                            if (livePlayer != null && livePlayer.Position != null && myPos.DistanceTo(livePlayer.Position) <= 25.0)
                            {
                                SRSkill ressSkill = FindPartyRessSkill();
                                if (ressSkill != null && ressSkill.isCastingEnabled)
                                {
                                    Window.Get?.LogProcess($"Party Support: Resurrecting [{member.Name}] with {ressSkill.Name}...");
                                    Bot.Get.CheckWeaponSwitch(ressSkill);
                                    PacketBuilder.CastSkill(ressSkill.ID, livePlayer.UniqueID);
                                    lastSupportActionUtc = DateTime.UtcNow;
                                    Thread.Sleep(Math.Max(500, ressSkill.CastingTime + 200));
                                    Bot.Get.EnsureMainWeapon();
                                    return;
                                }
                            }
                        }
                    }
                }

                // 2. Check for Low HP Party Members to Heal
                if (PartyHealEnabled)
                {
                    for (int i = 0; i < InfoManager.Party.Members.Count; i++)
                    {
                        SRPartyMember member = InfoManager.Party.Members.GetAt(i);
                        if (member == null || member.Name == InfoManager.Character.Name)
                            continue;

                        if (PartyPolicy.ShouldHealPartyMember(member.HPPercent, PartyHealHPPercent))
                        {
                            SRPlayer livePlayer = InfoManager.Players.Find(p => p != null && p.Name == member.Name);
                            if (livePlayer != null && livePlayer.Position != null && myPos.DistanceTo(livePlayer.Position) <= 35.0)
                            {
                                SRSkill healSkill = FindPartyHealSkill();
                                if (healSkill != null && healSkill.isCastingEnabled)
                                {
                                    Window.Get?.LogProcess($"Party Support: Healing [{member.Name}] ({member.HPPercent}% HP) with {healSkill.Name}...");
                                    Bot.Get.CheckWeaponSwitch(healSkill);
                                    PacketBuilder.CastSkill(healSkill.ID, livePlayer.UniqueID);
                                    lastSupportActionUtc = DateTime.UtcNow;
                                    Thread.Sleep(Math.Max(500, healSkill.CastingTime + 200));
                                    Bot.Get.EnsureMainWeapon();
                                    return;
                                }
                            }
                        }
                    }
                }

                // 3. Check Cure Bad Status
                if (PartyCureEnabled)
                {
                    SRSkill cureSkill = FindPartyCureSkill();
                    if (cureSkill != null && cureSkill.isCastingEnabled && (DateTime.UtcNow - lastSupportActionUtc).TotalSeconds > 15)
                    {
                        Bot.Get.CheckWeaponSwitch(cureSkill);
                        PacketBuilder.CastSkill(cureSkill.ID, 0);
                        lastSupportActionUtc = DateTime.UtcNow;
                        Thread.Sleep(Math.Max(400, cureSkill.CastingTime + 100));
                        Bot.Get.EnsureMainWeapon();
                        return;
                    }
                }
            }
            finally
            {
                Monitor.Exit(supportLock);
            }
        }

        private static SRSkill FindPartyHealSkill()
        {
            if (InfoManager.Character == null || InfoManager.Character.Skills == null)
                return null;

            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                SRSkill skill = InfoManager.Character.Skills.GetAt(i);
                if (skill != null && skill.Enabled && skill.isCastingEnabled && PartyPolicy.IsClericHealingSkill(skill.ServerName))
                {
                    return skill;
                }
            }
            return null;
        }

        private static SRSkill FindPartyRessSkill()
        {
            if (InfoManager.Character == null || InfoManager.Character.Skills == null)
                return null;

            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                SRSkill skill = InfoManager.Character.Skills.GetAt(i);
                if (skill != null && skill.Enabled && skill.isCastingEnabled && PartyPolicy.IsClericRessSkill(skill.ServerName))
                {
                    return skill;
                }
            }
            return null;
        }

        private static SRSkill FindPartyCureSkill()
        {
            if (InfoManager.Character == null || InfoManager.Character.Skills == null)
                return null;

            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                SRSkill skill = InfoManager.Character.Skills.GetAt(i);
                if (skill != null && skill.Enabled && skill.isCastingEnabled && PartyPolicy.IsClericCureSkill(skill.ServerName))
                {
                    return skill;
                }
            }
            return null;
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["PartyHealEnabled"] = PartyHealEnabled;
            json["PartyHealHPPercent"] = PartyHealHPPercent;
            json["PartyAutoRessEnabled"] = PartyAutoRessEnabled;
            json["PartyCureEnabled"] = PartyCureEnabled;
            json["PartyBuffsEnabled"] = PartyBuffsEnabled;
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null) return;
            if (json.ContainsKey("PartyHealEnabled")) PartyHealEnabled = (bool)json["PartyHealEnabled"];
            if (json.ContainsKey("PartyHealHPPercent")) PartyHealHPPercent = (byte)json["PartyHealHPPercent"];
            if (json.ContainsKey("PartyAutoRessEnabled")) PartyAutoRessEnabled = (bool)json["PartyAutoRessEnabled"];
            if (json.ContainsKey("PartyCureEnabled")) PartyCureEnabled = (bool)json["PartyCureEnabled"];
            if (json.ContainsKey("PartyBuffsEnabled")) PartyBuffsEnabled = (bool)json["PartyBuffsEnabled"];
        }
    }
}
