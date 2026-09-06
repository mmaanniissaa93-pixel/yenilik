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

            SRSkill pendingSkill = null;
            uint pendingTarget = 0;
            int pendingSleep = 0;
            string pendingLog = null;
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
                                    pendingSkill = ressSkill; pendingTarget = livePlayer.UniqueID;
                                    pendingSleep = Math.Max(500, ressSkill.CastingTime + 200);
                                    pendingLog = $"Party Support: Resurrecting [{member.Name}] with {ressSkill.Name}...";
                                    lastSupportActionUtc = DateTime.UtcNow;
                                    break;
                                }
                            }
                        }
                    }
                }

                // 2. Check for Low HP Party Members to Heal
                if (pendingSkill == null && PartyHealEnabled)
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
                                    pendingSkill = healSkill; pendingTarget = livePlayer.UniqueID;
                                    pendingSleep = Math.Max(500, healSkill.CastingTime + 200);
                                    pendingLog = $"Party Support: Healing [{member.Name}] ({member.HPPercent}% HP) with {healSkill.Name}...";
                                    lastSupportActionUtc = DateTime.UtcNow;
                                    break;
                                }
                            }
                        }
                    }
                }

                // 3. Check Cure Bad Status (sadece gerçekten kötü status varsa)
                if (pendingSkill == null && PartyCureEnabled)
                {
                    SRSkill cureSkill = FindPartyCureSkill();
                    bool needCure = InfoManager.Character.BadStatusFlags != SRModel.BadStatus.None;
                    if (!needCure && InfoManager.Party.Members != null)
                    {
                        for (int i = 0; i < InfoManager.Party.Members.Count; i++)
                        {
                            var m = InfoManager.Party.Members.GetAt(i);
                            if (m == null || m.Name == InfoManager.Character.Name) continue;
                            var lp = InfoManager.Players.Find(p => p != null && p.Name == m.Name);
                            if (lp != null && lp.BadStatusFlags != SRModel.BadStatus.None
                                && lp.Position != null && InfoManager.Character.GetRealtimePosition() != null
                                && InfoManager.Character.GetRealtimePosition().DistanceTo(lp.Position) <= 35.0)
                            { needCure = true; break; }
                        }
                    }
                    if (needCure && cureSkill != null && cureSkill.isCastingEnabled && (DateTime.UtcNow - lastSupportActionUtc).TotalSeconds > 15)
                    {
                        pendingSkill = cureSkill; pendingTarget = 0;
                        pendingSleep = Math.Max(400, cureSkill.CastingTime + 100);
                        pendingLog = "Party Support: Curing bad status...";
                        lastSupportActionUtc = DateTime.UtcNow;
                    }
                }
            }
            finally
            {
                Monitor.Exit(supportLock);
            }

            // Lock DIŞINDA cast+sleep: lock içeride Sleep yok, support tepkisi 2sn kitlenmez
            if (pendingSkill != null)
            {
                if (pendingLog != null) Window.Get?.LogProcess(pendingLog);
                Bot.Get.CheckWeaponSwitch(pendingSkill);
                PacketBuilder.CastSkill(pendingSkill.ID, pendingTarget);
                Thread.Sleep(pendingSleep);
                Bot.Get.EnsureMainWeapon();
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
