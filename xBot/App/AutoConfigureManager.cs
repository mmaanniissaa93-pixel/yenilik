using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace xBot.App
{
    /// <summary>
    /// phBot Auto Configure işlevinin xBot karşılığı (clean-room).
    /// UI girdilerini (Çince build/silah, Euro primary/secondary, profil,
    /// town-öncesi ve shared-pick bayrakları) alıp StatPointManager,
    /// SkillManager ve karakter profil dosyalarına uygular.
    /// </summary>
    public static class AutoConfigureManager
    {
        public static bool AutoConfigureBeforeTownLoop { get; set; } = false;
        public static bool UseSharedPickFilter { get; set; } = false;
        public static string LastAppliedBuild { get; private set; } = "";
        public static List<string> LastAppliedMasteries { get; private set; } = new List<string>();
        public static List<string> LastAppliedSkills { get; private set; } = new List<string>();

        public static void ApplyBeforeTownLoop()
        {
            if (!AutoConfigureBeforeTownLoop || string.IsNullOrEmpty(LastAppliedBuild)) return;
            string[] parts = LastAppliedBuild.Split(new[] { ' ', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return;
            if (parts[0] == "Chinese") ApplyChinese(parts[1], parts[2]);
            else if (parts[0] == "European") ApplyEuropean(parts[1], parts[2]);
        }

        private static string SharedFilterPath()
        {
            string name = (Game.DataManager.SilkroadName ?? "Default") + "_" + (Game.InfoManager.ServerName ?? "Default");
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return Path.Combine("Config", "SharedPick", name + ".json");
        }

        public static void SaveSharedPickFilter()
        {
            if (!UseSharedPickFilter) return;
            string temporary = null;
            try
            {
                string path = SharedFilterPath();
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var data = new JObject { ["ItemFilterManager"] = ItemFilterManager.ToJson(), ["EzFilterManager"] = EzFilterManager.ToJson() };
                temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
                File.WriteAllText(temporary, data.ToString());
                if (File.Exists(path)) File.Replace(temporary, path, null);
                else File.Move(temporary, path);
            }
            catch (Exception ex) { Window.Get?.Log("Shared pick filter could not be saved: " + ex.Message); }
            finally
            {
                if (temporary != null)
                    try { if (File.Exists(temporary)) File.Delete(temporary); } catch (IOException) { } catch (UnauthorizedAccessException) { }
            }
        }

        public static void LoadSharedPickFilter()
        {
            if (!UseSharedPickFilter) return;
            string path = SharedFilterPath();
            if (!File.Exists(path)) { SaveSharedPickFilter(); return; }
            try
            {
                var data = JObject.Parse(File.ReadAllText(path));
                if (data["ItemFilterManager"] is JObject filter) ItemFilterManager.FromJson(SharedPickFilterPolicy.Merge(ItemFilterManager.ToJson(), filter));
                if (data["EzFilterManager"] is JObject ez) EzFilterManager.FromJson(SharedPickFilterPolicy.MergeCategories(EzFilterManager.ToJson(), ez));
            }
            catch (Exception ex) { Window.Get?.Log("Shared pick filter could not be loaded: " + ex.Message); }
        }

        public static string ApplyChinese(string build, string weapon)
        {
            var r = AutoConfigurePolicy.ForChinese(build, weapon);
            ApplyResult("Chinese " + build + "/" + weapon, r);
            return Describe(r);
        }

        public static string ApplyEuropean(string primary, string secondary)
        {
            var r = AutoConfigurePolicy.ForEuropean(primary, secondary);
            ApplyResult("European " + primary + "/" + secondary, r);
            return Describe(r);
        }

        private static void ApplyResult(string label, AutoConfigurePolicy.Result r)
        {
            LastAppliedBuild = label;
            LastAppliedMasteries = new List<string>(r.Masteries);
            LastAppliedSkills = new List<string>(r.Skills);
            try { StatPointManager.TargetSTR = r.TargetSTR; } catch { }
            try { StatPointManager.TargetINT = r.TargetINT; } catch { }
            try { StatPointManager.AutoDistributeEnabled = (r.TargetSTR + r.TargetINT) > 0; } catch { }
            try { SkillManager.SelectedImbue = SkillManager.NormalizeImbueSelection(r.Imbue); } catch { }
            try { SkillManager.InOrderCombo = true; } catch { }
            try { Window.Get?.Log("Otomatik yapılandırma uygulandı [" + label + "]: " + r.Masteries.Count + " mastery, " + r.Skills.Count + " skill."); } catch { }
        }

        private static string Describe(AutoConfigurePolicy.Result r)
        {
            return string.Format("{0} mastery, {1} skill (STR {2}/INT {3})",
                r.Masteries.Count, r.Skills.Count, r.TargetSTR, r.TargetINT);
        }

        public static IList<string> ListProfiles()
        {
            var list = new List<string> { "Varsayılan" };
            try
            {
                if (!Directory.Exists("Config")) return list;
                foreach (string f in Directory.GetFiles("Config", "*.json"))
                {
                    string n = Path.GetFileNameWithoutExtension(f);
                    if (string.Equals(n, "Default", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!list.Contains(n)) list.Add(n);
                }
            }
            catch { }
            return list;
        }

        public static bool CreateProfile(string name, out string error)
        {
            error = "";
            string clean = (name ?? "").Trim();
            if (clean.Length == 0) { error = "Profil adı boş olamaz."; return false; }
            if (clean.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) { error = "Geçersiz profil adı."; return false; }
            try
            {
                if (!Directory.Exists("Config")) Directory.CreateDirectory("Config");
                string path = Path.Combine("Config", clean + ".json");
                if (File.Exists(path)) { error = "Profil zaten var: " + clean; return false; }
                Settings.SaveCharacterSettings();
                string current = CurrentCharacterConfigPath();
                if (!string.IsNullOrEmpty(current) && File.Exists(current))
                    File.Copy(current, path);
                else
                    File.WriteAllText(path, new JObject { ["Profile"] = clean }.ToString());
                return true;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        public static bool RenameProfile(string oldName, string newName, out string error)
        {
            error = "";
            try
            {
                string a = Path.Combine("Config", (oldName ?? "").Trim() + ".json");
                string b = Path.Combine("Config", (newName ?? "").Trim() + ".json");
                if (!File.Exists(a)) { error = "Profil bulunamadı: " + oldName; return false; }
                if (File.Exists(b)) { error = "Hedef profil zaten var: " + newName; return false; }
                File.Move(a, b);
                return true;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        public static bool DeleteProfile(string name, out string error)
        {
            error = "";
            try
            {
                string p = Path.Combine("Config", (name ?? "").Trim() + ".json");
                if (!File.Exists(p)) { error = "Profil bulunamadı: " + name; return false; }
                File.Delete(p);
                return true;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        private static string CurrentCharacterProfilePath()
        {
            try
            {
                string loaded = Settings.LoadedCharacterProfile;
                if (!string.IsNullOrEmpty(loaded))
                {
                    string p = Path.Combine("Config", loaded + ".json");
                    if (File.Exists(p)) return p;
                }
            }
            catch { }
            return "";
        }

        private static string CurrentCharacterConfigPath()
        {
            string hit = CurrentCharacterProfilePath();
            if (!string.IsNullOrEmpty(hit)) return hit;
            try
            {
                if (!Directory.Exists("Config")) return "";
                var files = Directory.GetFiles("Config", "*.json")
                    .Where(f => !string.Equals(Path.GetFileName(f), "Default.json", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(f => new FileInfo(f).LastWriteTimeUtc)
                    .ToArray();
                return files.Length > 0 ? files[0] : "";
            }
            catch { return ""; }
        }

        public static JObject ToJson()
        {
            var j = new JObject();
            j["AutoConfigureBeforeTownLoop"] = AutoConfigureBeforeTownLoop;
            j["UseSharedPickFilter"] = UseSharedPickFilter;
            j["LastAppliedBuild"] = LastAppliedBuild ?? "";
            j["LastAppliedMasteries"] = new JArray(LastAppliedMasteries.ToArray());
            j["LastAppliedSkills"] = new JArray(LastAppliedSkills.ToArray());
            return j;
        }

        public static void FromJson(JObject j)
        {
            if (j == null) return;
            try { if (j["AutoConfigureBeforeTownLoop"] != null) AutoConfigureBeforeTownLoop = (bool)j["AutoConfigureBeforeTownLoop"]; } catch { }
            try { if (j["UseSharedPickFilter"] != null) UseSharedPickFilter = (bool)j["UseSharedPickFilter"]; } catch { }
            try { if (j["LastAppliedBuild"] != null) LastAppliedBuild = (string)j["LastAppliedBuild"]; } catch { }
        }
    }
}
