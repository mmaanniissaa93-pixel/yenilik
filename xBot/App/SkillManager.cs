using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Common;

namespace xBot.App
{
    public static class SkillManager
    {
        public static bool NoAttackMode { get; set; } = false;
        // Kept for backwards-compatible settings; the active choice is the skill ID.
        public static string SelectedImbue { get; set; } = "None";
        public static uint SelectedImbueSkillId { get; set; }
        public static bool UseDevilSpirit { get; set; } = false;
        public static bool InOrderCombo { get; set; } = true;

        public static string LastCastStatus { get; private set; } = "Hazır";
        public static string LastCastSkill { get; private set; } = "-";
        public static int ConsecutiveCastFailures { get; private set; }
        public static DateTime LastCastAt { get; private set; }
        private static DateTime LastImbueAttemptAt { get; set; }

        public sealed class ImbueSkillOption
        {
            public uint SkillId { get; private set; }
            public string Element { get; private set; }
            public byte Level { get; private set; }
            public string DisplayName { get; private set; }

            public ImbueSkillOption(SRSkill skill, string element)
            {
                SkillId = skill.ID;
                Element = element;
                Level = skill.Level;
                string skillName = string.IsNullOrWhiteSpace(skill.Name) ? skill.ServerName : skill.Name;
                DisplayName = string.Format("{0} - Lv.{1} ({2})", skillName, skill.Level, element);
            }

            public override string ToString()
            {
                return DisplayName;
            }
        }

        public static List<ImbueSkillOption> GetAvailableImbueSkills()
        {
            List<ImbueSkillOption> result = new List<ImbueSkillOption>();
            if (InfoManager.Character == null || InfoManager.Character.Skills == null)
                return result;

            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                SRSkill skill = InfoManager.Character.Skills.GetAt(i);
                string element = skill == null ? "None" : ImbuePolicy.GetElement(skill.ServerName);
                if (skill != null && skill.Enabled && element != "None")
                    result.Add(new ImbueSkillOption(skill, element));
            }

            return result
                .OrderByDescending(option => option.Level)
                .ThenBy(option => option.Element)
                .ThenBy(option => option.DisplayName)
                .ToList();
        }

        public static bool HasActiveImbue()
        {
            return HasActiveImbue(GetSelectedImbueElement());
        }

        public static bool HasActiveImbue(string element)
        {
            if (InfoManager.Character == null || InfoManager.Character.Buffs == null)
                return false;

            for (byte i = 0; i < InfoManager.Character.Buffs.Count; i++)
            {
                var buff = InfoManager.Character.Buffs.GetAt(i);
                if (buff != null && !string.IsNullOrEmpty(buff.ServerName))
                {
                    if (ImbuePolicy.IsActiveImbue(buff.ServerName, element))
                        return true;
                }
            }
            return false;
        }

        public static void EnsureImbueActive()
        {
            string selectedElement = GetSelectedImbueElement();
            if (selectedElement == "None" || HasActiveImbue(selectedElement))
                return;

            if (InfoManager.Character == null || InfoManager.Character.Skills == null)
                return;

            // Prevent a missing/late server response from causing an imbue
            // packet every tick. The skill cooldown remains authoritative
            // after the server confirms the cast.
            if ((DateTime.Now - LastImbueAttemptAt).TotalMilliseconds < 1000)
                return;

            SRSkill imbueSkill = null;
            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                var s = InfoManager.Character.Skills.GetAt(i);
                if (s != null && s.Enabled && s.isCastingEnabled
                    && ImbuePolicy.IsImbueSkill(s.ServerName, selectedElement)
                    && (SelectedImbueSkillId == 0 || s.ID == SelectedImbueSkillId))
                {
                    if (imbueSkill == null || s.Level > imbueSkill.Level)
                        imbueSkill = s;
                }
            }

            if (imbueSkill != null)
            {
                LastImbueAttemptAt = DateTime.Now;
                PacketBuilder.CastSkill(imbueSkill.ID, 0);
            }
        }

        private static string GetSelectedImbueElement()
        {
            if (SelectedImbueSkillId != 0 && InfoManager.Character != null && InfoManager.Character.Skills != null)
            {
                for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
                {
                    SRSkill skill = InfoManager.Character.Skills.GetAt(i);
                    if (skill != null && skill.ID == SelectedImbueSkillId)
                        return ImbuePolicy.GetElement(skill.ServerName);
                }
            }

            return NormalizeImbueSelection(SelectedImbue);
        }

        public static string NormalizeImbueSelection(string element)
        {
            if (string.Equals(element, "LIGHT", StringComparison.OrdinalIgnoreCase)
                || string.Equals(element, "LIGHTNING", StringComparison.OrdinalIgnoreCase))
                return "Lightning";
            if (string.Equals(element, "FIRE", StringComparison.OrdinalIgnoreCase))
                return "Fire";
            if (string.Equals(element, "COLD", StringComparison.OrdinalIgnoreCase))
                return "Cold";
            return "None";
        }

        public static void CheckDevilSpirit()
        {
            if (!UseDevilSpirit || InfoManager.Character == null || InfoManager.Character.Skills == null)
                return;

            // Check if devil spirit buff is already active
            for (byte i = 0; i < InfoManager.Character.Buffs.Count; i++)
            {
                var b = InfoManager.Character.Buffs.GetAt(i);
                if (b != null && b.ServerName != null && b.ServerName.Contains("DEVIL"))
                    return;
            }

            SRSkill devilSkill = null;
            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                var s = InfoManager.Character.Skills.GetAt(i);
                if (s != null && !string.IsNullOrEmpty(s.ServerName) && s.ServerName.Contains("DEVIL"))
                {
                    devilSkill = s;
                    break;
                }
            }

            if (devilSkill != null)
            {
                PacketBuilder.CastSkill(devilSkill.ID, 0);
            }
        }

        /// <summary>
        /// Creates a safe basic attack when the configured attack list is
        /// empty, on cooldown, or requires an unavailable weapon.
        /// Silkroad Common Attack is an Action (opcode 0x7074 action 1) with ID 1,
        /// never a database animation skill ID.
        /// </summary>
        public static SRSkill GetFallbackAttack(SRTypes.Weapon weapon)
        {
            SRSkill fallback = new SRSkill(1u);
            fallback.Name = "Common Attack";
            fallback.CastingTime = 800;
            fallback.MPUsage = 0;
            fallback.RequiredWeaponPrimary = SRTypes.Weapon.None;
            fallback.RequiredWeaponSecondary = SRTypes.Weapon.None;
            return fallback;
        }

        public static void RecordCastSuccess(SRSkill skill)
        {
            ConsecutiveCastFailures = 0;
            LastCastSkill = skill == null ? "-" : skill.Name;
            LastCastStatus = "Başarılı";
            LastCastAt = DateTime.Now;
            NotifyStatusChanged();
        }

        public static void RecordCastFailure(SRSkill skill, string reason)
        {
            ConsecutiveCastFailures++;
            LastCastSkill = skill == null ? "-" : skill.Name;
            LastCastStatus = string.IsNullOrEmpty(reason) ? "Başarısız" : reason;
            LastCastAt = DateTime.Now;
            NotifyStatusChanged();
        }

        private static void NotifyStatusChanged()
        {
            try
            {
                Window.Get.UpdateSkillRuntimeStatus();
            }
            catch
            {
                // UI status must never interrupt the bot loop.
            }
        }

        public static void MoveSelectedItemUp(ListView lstv)
        {
            if (lstv == null || lstv.SelectedItems.Count == 0)
                return;

            ListViewItem selected = lstv.SelectedItems[0];
            int index = selected.Index;
            if (index > 0)
            {
                lstv.BeginUpdate();
                lstv.Items.RemoveAt(index);
                lstv.Items.Insert(index - 1, selected);
                selected.Selected = true;
                lstv.EndUpdate();
            }
        }

        public static void MoveSelectedItemDown(ListView lstv)
        {
            if (lstv == null || lstv.SelectedItems.Count == 0)
                return;

            ListViewItem selected = lstv.SelectedItems[0];
            int index = selected.Index;
            if (index < lstv.Items.Count - 1)
            {
                lstv.BeginUpdate();
                lstv.Items.RemoveAt(index);
                lstv.Items.Insert(index + 1, selected);
                selected.Selected = true;
                lstv.EndUpdate();
            }
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["NoAttackMode"] = NoAttackMode;
            json["SelectedImbue"] = SelectedImbue;
            json["SelectedImbueSkillId"] = SelectedImbueSkillId;
            json["UseDevilSpirit"] = UseDevilSpirit;
            json["InOrderCombo"] = InOrderCombo;
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null)
                return;

            if (json.ContainsKey("NoAttackMode")) NoAttackMode = (bool)json["NoAttackMode"];
            if (json.ContainsKey("SelectedImbue")) SelectedImbue = NormalizeImbueSelection((string)json["SelectedImbue"]);
            if (json.ContainsKey("SelectedImbueSkillId")) SelectedImbueSkillId = (uint)json["SelectedImbueSkillId"];
            if (json.ContainsKey("UseDevilSpirit")) UseDevilSpirit = (bool)json["UseDevilSpirit"];
            if (json.ContainsKey("InOrderCombo")) InOrderCombo = (bool)json["InOrderCombo"];
        }
    }
}
