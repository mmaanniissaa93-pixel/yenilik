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
        private static readonly string ProfilesDirectory = Path.Combine("Config", "Profiles");
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
            var list = new List<string>();
            try
            {
                if (File.Exists(Path.Combine("Config", "Default.json"))) list.Add("Default");
                if (!Directory.Exists("Config")) return list;
                if (Directory.Exists(ProfilesDirectory))
                {
                    foreach (string f in Directory.GetFiles(ProfilesDirectory, "*.json"))
                    {
                        string n = Path.GetFileNameWithoutExtension(f);
                        if (!list.Contains(n, StringComparer.OrdinalIgnoreCase)) list.Add(n);
                    }
                }
                // Keep old root-level profiles selectable for compatibility. New profile
                // mutations are confined to ProfilesDirectory so character files stay safe.
                foreach (string f in Directory.GetFiles("Config", "*.json"))
                {
                    string n = Path.GetFileNameWithoutExtension(f);
                    if (!string.Equals(n, "Default", StringComparison.OrdinalIgnoreCase)
                        && !list.Contains(n, StringComparer.OrdinalIgnoreCase)) list.Add(n);
                }
            }
            catch { }
            list.Sort(StringComparer.OrdinalIgnoreCase);
            return list;
        }

        private static bool TryNormalizeProfileName(string profileName, out string clean, out string error)
        {
            clean = (profileName ?? "").Trim();
            error = "";
            if (clean.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                clean = clean.Substring(0, clean.Length - 5);
            if (clean.Length == 0 || clean == "." || clean == ".."
                || clean.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                || clean.IndexOf(Path.DirectorySeparatorChar) >= 0
                || clean.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
            {
                error = "Geçersiz profil adı.";
                return false;
            }
            return true;
        }

        private static string ManagedProfilePath(string name)
        {
            return Path.GetFullPath(Path.Combine(ProfilesDirectory, name + ".json"));
        }

        private static string LegacyProfilePath(string name)
        {
            return Path.GetFullPath(Path.Combine("Config", name + ".json"));
        }

        public static bool TryResolveProfilePath(string profileName, out string path, out string error)
        {
            path = null;
            string clean;
            if (!TryNormalizeProfileName(profileName, out clean, out error)) return false;
            try
            {
                if (string.Equals(clean, "Default", StringComparison.OrdinalIgnoreCase))
                    path = LegacyProfilePath("Default");
                else
                {
                    string managed = ManagedProfilePath(clean);
                    string legacy = LegacyProfilePath(clean);
                    path = File.Exists(managed) ? managed : legacy;
                }
                if (!File.Exists(path))
                {
                    error = "Profil bulunamadı: " + clean;
                    path = null;
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                path = null;
                return false;
            }
        }

        public static bool CreateProfile(string name, out string error)
        {
            error = "";
            string clean;
            if (!TryNormalizeProfileName(name, out clean, out error)) return false;
            if (string.Equals(clean, "Default", StringComparison.OrdinalIgnoreCase))
            { error = "Default ayrılmış bir profil adıdır."; return false; }
            try
            {
                if (!Directory.Exists(ProfilesDirectory)) Directory.CreateDirectory(ProfilesDirectory);
                string path = ManagedProfilePath(clean);
                if (File.Exists(path)) { error = "Profil zaten var: " + clean; return false; }
                if (File.Exists(LegacyProfilePath(clean)))
                { error = "Aynı adda eski bir Config dosyası var. Var olan dosyanın üzerine yazılmayacak; başka ad seçin."; return false; }
                if (Game.InfoManager.inGame)
                {
                    Settings.FlushPendingSaves();
                    if (!Settings.LastCharacterSaveSucceeded)
                    { error = "Önce açık karakter ayarları kaydedilemedi."; return false; }
                }
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
                string oldClean, newClean;
                if (!TryNormalizeProfileName(oldName, out oldClean, out error)
                    || !TryNormalizeProfileName(newName, out newClean, out error)) return false;
                if (string.Equals(oldClean, "Default", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(newClean, "Default", StringComparison.OrdinalIgnoreCase))
                { error = "Default profili yeniden adlandırılamaz."; return false; }
                if (Game.InfoManager.inGame)
                {
                    Settings.FlushPendingSaves();
                    if (!Settings.LastCharacterSaveSucceeded)
                    { error = "Profil yeniden adlandırılmadan önce açık karakter ayarları kaydedilemedi."; return false; }
                }
                string a = ManagedProfilePath(oldClean);
                string b = ManagedProfilePath(newClean);
                if (!File.Exists(a))
                {
                    error = File.Exists(LegacyProfilePath(oldClean))
                        ? "Eski Config dosyaları profil düğmeleriyle yeniden adlandırılamaz; yeni bir profil oluşturun."
                        : "Profil bulunamadı: " + oldName;
                    return false;
                }
                if (File.Exists(b)) { error = "Hedef profil zaten var: " + newName; return false; }
                if (File.Exists(LegacyProfilePath(newClean)))
                { error = "Hedef adda eski bir Config dosyası var; başka ad seçin."; return false; }
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
                string clean;
                if (!TryNormalizeProfileName(name, out clean, out error)) return false;
                if (string.Equals(clean, "Default", StringComparison.OrdinalIgnoreCase))
                { error = "Default profili silinemez."; return false; }
                if (string.Equals(Settings.LoadedCharacterProfile, clean, StringComparison.OrdinalIgnoreCase))
                { error = "Etkin profil silinemez; önce başka bir profil yükleyin."; return false; }
                string p = ManagedProfilePath(clean);
                if (!File.Exists(p))
                {
                    error = File.Exists(LegacyProfilePath(clean))
                        ? "Eski Config dosyaları profil düğmeleriyle silinemez."
                        : "Profil bulunamadı: " + name;
                    return false;
                }
                File.Delete(p);
                return true;
            }
            catch (Exception ex) { error = ex.Message; return false; }
        }

        private static string CurrentCharacterProfilePath()
        {
            try
            {
                string path = Settings.LoadedCharacterProfilePath;
                if (!string.IsNullOrEmpty(path) && File.Exists(path)) return path;
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
                if (!Game.InfoManager.inGame || string.IsNullOrWhiteSpace(Game.DataManager.SilkroadName)
                    || string.IsNullOrWhiteSpace(Game.InfoManager.ServerName) || string.IsNullOrWhiteSpace(Game.InfoManager.CharName))
                    return "";
                string silkroad = Game.DataManager.SilkroadName;
                string server = Game.InfoManager.ServerName;
                string character = Game.InfoManager.CharName;
                foreach (char c in Path.GetInvalidFileNameChars())
                {
                    silkroad = silkroad.Replace(c, '_');
                    server = server.Replace(c, '_');
                    character = character.Replace(c, '_');
                }
                string current = Path.Combine("Config", silkroad + "_" + server + "_" + character + ".json");
                return File.Exists(current) ? current : "";
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
