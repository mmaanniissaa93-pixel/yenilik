using System;
using System.Collections.Generic;

namespace xBot.App
{
    /// <summary>
    /// Auto Configure karar tabloları (clean-room).
    /// Kaynak: gözlenen phBot davranışı — Config/Velora_*.json Auto Mastery + Stat Points
    /// çıktıları ve TRSRO.db3 skills/mastery tablolarındaki isimler. phBot binary
    /// kodu kopyalanmadı; sadece girdi->çıktı eşlemesi yeniden ifade edildi.
    /// </summary>
    public static class AutoConfigurePolicy
    {
        public sealed class Result
        {
            public List<string> Masteries = new List<string>();
            public List<string> Skills = new List<string>();
            public int TargetSTR;
            public int TargetINT;
            public string Imbue = "None";
        }

        public static Result ForChinese(string build, string weapon)
        {
            var r = new Result();
            string b = (build ?? "Str").Trim().ToLowerInvariant();
            string w = (weapon ?? "Bicheon").Trim();
            bool isStr = b.StartsWith("str");
            bool isInt = !isStr && b.StartsWith("int");
            if (!isStr && !isInt) { isStr = true; } // Hybrid varsayılanı Str'e düşür

            r.TargetSTR = isStr ? 3 : 0;
            r.TargetINT = isStr ? 0 : 3;
            r.Masteries.Add(NormalizeChineseWeapon(w));
            // Bicheon skillgroup adları TRSRO.db3 skillgroup tablosundan gözlendi:
            // Smash (attack), Chain (attack), Shield (buff), Passive (buff), Geomgi.
            if (string.Equals(w, "Bicheon", StringComparison.OrdinalIgnoreCase))
            {
                r.Skills.AddRange(new[] { "Vurup Ezme", "Illuzyon Zinciri", "Kale Kalkani", "Kalkan Korumasi", "Ruh Pala Kesigi" });
                r.Imbue = isStr ? "None" : "Fire";
            }
            else if (string.Equals(w, "Heuksal", StringComparison.OrdinalIgnoreCase))
            {
                r.Skills.AddRange(new[] { "Heuksal Saldiri", "Heuksal Buff" });
                r.Imbue = isStr ? "None" : "Lightning";
            }
            else // Pacheon
            {
                r.Skills.AddRange(new[] { "Pacheon Saldiri", "Pacheon Buff" });
                r.Imbue = "None";
            }
            if (!isStr)
                r.Masteries.Add("Force");
            return r;
        }

        public static Result ForEuropean(string primary, string secondary)
        {
            var r = new Result();
            string p = NormalizeEuroWeapon(primary);
            string s = NormalizeEuroWeapon(secondary);
            if (!string.IsNullOrEmpty(p)) r.Masteries.Add(p);
            if (!string.IsNullOrEmpty(s) && !string.Equals(s, p, StringComparison.OrdinalIgnoreCase))
                r.Masteries.Add(s);

            // Aşağıdaki listeler Config/Velora_*.Varsayılan.json Auto Mastery
            // Skill dizilerinden birebir alındı (gözlenen çıktı).
            switch (p.ToLowerInvariant())
            {
                case "wizard":
                    r.TargetSTR = 0; r.TargetINT = 3;
                    r.Skills.AddRange(new[] { "Earth Spirit", "Cold Spirit", "Fire Spirit", "Lightning Spirit", "Frozen Spear", "Earth Quake", "Earth Fence", "Charged Squall", "Aerial Teleport", "Meteor", "Fire Bolt", "Blizzard" });
                    break;
                case "bard":
                    r.TargetSTR = 0; r.TargetINT = 0;
                    r.Skills.AddRange(new[] { "Mana Switch", "Moving March", "Swing March", "Guard Tambour", "Mana Tambour", "Hit March", "Clout March", "Noise", "Mana Orbit", "Dancing of Wizardry" });
                    break;
                case "cleric":
                    r.TargetSTR = 0; r.TargetINT = 0;
                    r.Skills.AddRange(new[] { "Grad Reverse", "Faith", "Healing Orbit", "Bless Spell", "Holy Recovery Division", "Force Blessing", "Force Deity", "Mental Blessing", "Mental Deity", "Body Blessing", "Body Deity", "Soul Blessing", "Soul Deity", "Holy Spell", "Charity", "Glory Armor", "Favor Armor", "Healing Cycle" });
                    break;
                case "warrior":
                    r.TargetSTR = 3; r.TargetINT = 0;
                    r.Skills.AddRange(new[] { "Taunting Target", "Howling Shout", "Vital Increase", "Descry", "Iron Skin", "Mana Skin", "Sprint Assault", "Pain Quota", "Physical Fence", "Magical Fence", "Protect", "Physical Screen", "Ultimate Screen", "Morale Screen" });
                    break;
                case "warlock":
                    r.TargetSTR = 0; r.TargetINT = 3;
                    r.Skills.AddRange(new[] { "Dark Seed", "Blaze", "Dark Blaze", "Toxin", "Toxin Invasion", "Decayed", "Dark Decayed", "Curse Breath", "Dark Breath", "Physical Raze", "Physical Ravage", "Medical Raze", "Magical Ravage", "Combat Raze", "Combat Ravage", "Courage Raze", "Courage Ravage" });
                    break;
                case "rogue":
                case "dagger":
                    r.TargetSTR = 3; r.TargetINT = 0;
                    r.Skills.AddRange(new string[0]);
                    break;
                default:
                    r.TargetSTR = 0; r.TargetINT = 0;
                    break;
            }
            return r;
        }

        public static string NormalizeChineseWeapon(string w)
        {
            if (string.Equals(w, "Heuksal", StringComparison.OrdinalIgnoreCase)) return "Heuksal";
            if (string.Equals(w, "Pacheon", StringComparison.OrdinalIgnoreCase)) return "Pacheon";
            return "Bicheon";
        }

        public static string NormalizeEuroWeapon(string w)
        {
            if (string.IsNullOrWhiteSpace(w) || string.Equals(w, "None", StringComparison.OrdinalIgnoreCase)) return "";
            string t = w.Trim().ToLowerInvariant();
            if (t.Contains("dagger") || t.Contains("rogue")) return "Rogue";
            if (t.Contains("wizard")) return "Wizard";
            if (t.Contains("warlock")) return "Warlock";
            if (t.Contains("bard")) return "Bard";
            if (t.Contains("cleric")) return "Cleric";
            if (t.Contains("warrior")) return "Warrior";
            if (t.Contains("crossbow")) return "Rogue";
            if (t.Contains("sword") || t.Contains("axe")) return "Warrior";
            return w.Trim();
        }
    }
}
