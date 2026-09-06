using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using static xBot.Game.Objects.Common.SRTypes;

namespace xBot.Game.Objects.Common
{
    public class SRSkill
    {
        private Stopwatch m_CooldownTimer;
        public uint ID { get; set; }
        public string ServerName { get; set; }
        public string Name { get; set; }
        public uint GroupID { get; set; }
        public string GroupName { get; set; }
        public uint Cooldown { get; set; }
        public uint DurationMax { get; set; }
        public int CastingTime { get; set; }
        public uint MPUsage { get; set; }
        public bool isTargetRequired { get; }
        public byte Level { get; set; }
        public string Params { get; set; }
        public string Icon { get; set; }
        public Weapon RequiredWeaponPrimary { get; set; }
        public Weapon RequiredWeaponSecondary { get; set; }
        public List<Params_ItemRequired> RequiredItems { get; } = new List<Params_ItemRequired>();
        public bool Enabled { get; set; }
        public bool isCastingEnabled
        {
            get
            {
                if (m_CooldownTimer != null)
                {
                    if (m_CooldownTimer.ElapsedMilliseconds < Cooldown)
                        return false;
                    m_CooldownTimer.Stop();
                }
                return true;
            }
        }
        public SRSkill(uint ID)
        {
            NameValueCollection data = DataManager.GetSkillData(ID);

            this.ID = ID;
            if (data != null)
            {
                ServerName = data["servername"] ?? "";
                Name = data["name"] ?? ServerName;
                uint u; int itmp; byte b;
                GroupID = uint.TryParse(data["group_id"], out u) ? u : 0;
                GroupName = data["group_name"] ?? "";
                Cooldown = uint.TryParse(data["cooldown"], out u) ? u : 0;
                DurationMax = uint.TryParse(data["duration"], out u) ? u : 0;
                CastingTime = int.TryParse(data["casttime"], out itmp) ? itmp : 0;
                MPUsage = uint.TryParse(data["mana"], out u) ? u : 0;
                isTargetRequired = data["target_required"] == "1";
                Level = byte.TryParse(data["level"], out b) ? b : (byte)0;
                Params = data["params"] ?? "";
                Icon = data["icon"] ?? "";
                RequiredWeaponPrimary = (Weapon)(byte.TryParse(data["weapon_first"], out b) ? b : 0);
                RequiredWeaponSecondary = (Weapon)(byte.TryParse(data["weapon_second"], out b) ? b : 0);
                // Extract item required
                if (!string.IsNullOrEmpty(Params))
                {
                    var paramList = Params.Split('|');
                    for (int i = 0; i < paramList.Length; i++)
                    {
                        if (paramList[i] == "" + (uint)Game.Params.Type.ITEM_REQUIRED && i + 2 < paramList.Length)
                        {
                            byte tid3, tid4;
                            byte.TryParse(paramList[i + 1], out tid3);
                            byte.TryParse(paramList[i + 2], out tid4);
                            RequiredItems.Add(new Params_ItemRequired() { TID3 = tid3, TID4 = tid4 });
                            i += 2;
                        }
                    }
                }
            }
            else
            {
                ServerName = "SKILL_UNKNOWN_" + ID;
                Name = ServerName;
                GroupID = 0;
                GroupName = "";
                Cooldown = 0;
                DurationMax = 0;
                CastingTime = 0;
                MPUsage = 0;
                isTargetRequired = false;
                Level = 0;
                Params = "";
                Icon = "";
                RequiredWeaponPrimary = Weapon.None;
                RequiredWeaponSecondary = Weapon.None;
            }
        }
        public SRSkill(string ServerName)
        {
            NameValueCollection data = DataManager.GetSkillData(ServerName);

            this.ServerName = ServerName;
            if (data == null)
            {
                ID = 0;
                Name = ServerName;
                GroupID = 0;
                GroupName = "";
                Cooldown = 0;
                DurationMax = 0;
                CastingTime = 0;
                MPUsage = 0;
                isTargetRequired = false;
                Level = 0;
                Params = "";
                Icon = "";
                RequiredWeaponPrimary = Weapon.None;
                RequiredWeaponSecondary = Weapon.None;
                return;
            }
            uint u; int itmp; byte b;
            ID = uint.TryParse(data["id"], out u) ? u : 0;
            Name = data["name"] ?? ServerName;
            GroupID = uint.TryParse(data["group_id"], out u) ? u : 0;
            GroupName = data["group_name"] ?? "";
            Cooldown = uint.TryParse(data["cooldown"], out u) ? u : 0;
            DurationMax = uint.TryParse(data["duration"], out u) ? u : 0;
            CastingTime = int.TryParse(data["casttime"], out itmp) ? itmp : 0;
            MPUsage = uint.TryParse(data["mana"], out u) ? u : 0;
            isTargetRequired = data["target_required"] == "1";
            Level = byte.TryParse(data["level"], out b) ? b : (byte)0;
            Params = data["params"] ?? "";
            Icon = data["icon"] ?? "";
            RequiredWeaponPrimary = (Weapon)(byte.TryParse(data["weapon_first"], out b) ? b : 0);
            RequiredWeaponSecondary = (Weapon)(byte.TryParse(data["weapon_second"], out b) ? b : 0);
            // Extract item required
            if (!string.IsNullOrEmpty(Params))
            {
                var paramList = Params.Split('|');
                for (int i = 0; i + 2 < paramList.Length; i++)
                {
                    if (paramList[i] == "" + (uint)Game.Params.Type.ITEM_REQUIRED)
                    {
                        byte t3, t4;
                        byte.TryParse(paramList[i + 1], out t3);
                        byte.TryParse(paramList[i + 2], out t4);
                        RequiredItems.Add(new Params_ItemRequired() { TID3 = t3, TID4 = t4 });
                        i += 2;
                    }
                }
            }
        }

        public void StartCooldown()
        {
            if (m_CooldownTimer == null)
                m_CooldownTimer = Stopwatch.StartNew();
            else
                m_CooldownTimer.Restart();
        }
        public bool isAttackingSkill()
        {
            if (MPUsage > 0 && DurationMax == 0)
                return true;
            return false;
        }
        /// <summary>
        /// Sureli, cast edilebilir buff skili (koruma, bless, imbue, Devil Spirit...).
        /// </summary>
        public bool isBuffSkill()
        {
            if (MPUsage > 0 && DurationMax > 0)
                return true;
            return false;
        }
        /// <summary>
        /// Bot tarafindan kullanilabilir skill: attack veya buff. MP harcamayan
        /// pasif skill'ler (MPUsage==0) elenir. DB'de olmayanlar siniflanamaz.
        /// </summary>
        public bool isUsableSkill()
        {
            if (IsDatabaseMissing())
                return true; // siniflanamiyor: eleme, kullanici karar versin
            return isAttackingSkill() || isBuffSkill();
        }
        /// <summary>
        /// DB'de karşılığı yoksa (SKILL_UNKNOWN_*) isimle gelir. Bu durumda
        /// isAttackingSkill() hep false döner ve UI eklemeyi sessizce engeller.
        /// </summary>
        public bool IsDatabaseMissing()
        {
            return string.IsNullOrEmpty(ServerName) || ServerName.StartsWith("SKILL_UNKNOWN_");
        }
        


        public class Params_ItemRequired
        {
            public byte TID3;
            public byte TID4;
        }
    }
}
