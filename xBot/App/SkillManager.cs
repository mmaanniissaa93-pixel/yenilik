using System;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Common;

namespace xBot.App
{
    public static class SkillManager
    {
        public static bool NoAttackMode { get; set; } = false;
        public static string SelectedImbue { get; set; } = "None"; // None, Fire, Cold, Lightning
        public static bool UseDevilSpirit { get; set; } = false;
        public static bool InOrderCombo { get; set; } = true;

        public static bool HasActiveImbue()
        {
            if (InfoManager.Character == null || InfoManager.Character.Buffs == null)
                return false;

            for (byte i = 0; i < InfoManager.Character.Buffs.Count; i++)
            {
                var buff = InfoManager.Character.Buffs.GetAt(i);
                if (buff != null && !string.IsNullOrEmpty(buff.ServerName))
                {
                    string name = buff.ServerName.ToUpperInvariant();
                    if (name.Contains("_GIGONGTA_") || name.Contains("_FIRE_ENCHANT") || name.Contains("_COLD_ENCHANT") || name.Contains("_LIGHT_ENCHANT"))
                        return true;
                }
            }
            return false;
        }

        public static void EnsureImbueActive()
        {
            if (SelectedImbue == "None" || HasActiveImbue())
                return;

            if (InfoManager.Character == null || InfoManager.Character.Skills == null)
                return;

            string searchTag = "";
            switch (SelectedImbue.ToUpperInvariant())
            {
                case "FIRE":
                    searchTag = "_FIRE";
                    break;
                case "COLD":
                    searchTag = "_COLD";
                    break;
                case "LIGHTNING":
                    searchTag = "_LIGHT";
                    break;
            }

            if (string.IsNullOrEmpty(searchTag))
                return;

            SRSkill imbueSkill = null;
            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                var s = InfoManager.Character.Skills.GetAt(i);
                if (s != null && !string.IsNullOrEmpty(s.ServerName) && s.ServerName.Contains(searchTag) && (s.ServerName.Contains("_GIGONGTA_") || s.ServerName.Contains("_ENCHANT")))
                {
                    if (imbueSkill == null || s.Level > imbueSkill.Level)
                        imbueSkill = s;
                }
            }

            if (imbueSkill != null)
            {
                PacketBuilder.CastSkill(imbueSkill.ID, 0);
            }
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
            json["UseDevilSpirit"] = UseDevilSpirit;
            json["InOrderCombo"] = InOrderCombo;
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null)
                return;

            if (json.ContainsKey("NoAttackMode")) NoAttackMode = (bool)json["NoAttackMode"];
            if (json.ContainsKey("SelectedImbue")) SelectedImbue = (string)json["SelectedImbue"];
            if (json.ContainsKey("UseDevilSpirit")) UseDevilSpirit = (bool)json["UseDevilSpirit"];
            if (json.ContainsKey("InOrderCombo")) InOrderCombo = (bool)json["InOrderCombo"];
        }
    }
}
