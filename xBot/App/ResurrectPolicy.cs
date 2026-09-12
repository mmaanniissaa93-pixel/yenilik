using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace xBot.App
{
    public static class ResurrectPolicy
    {
        public static List<string> ResurrectRegexList { get; } = new List<string>();
        public static int ResurrectRange { get; set; } = 30;
        public static int ResurrectDelayMs { get; set; } = 10000;
        public static bool ResurrectAllParty { get; set; } = false;
        public static int RetryLimit { get; set; } = 0;
        public static bool AcceptFromOthers { get; set; } = false;
        public static bool AcceptPartyOnly { get; set; } = false;
        public static List<string> SelectedResSkills { get; } = new List<string>();

        public static bool MatchesRegex(string playerName)
        {
            if (string.IsNullOrEmpty(playerName)) return false;
            lock (ResurrectRegexList)
            {
                if (ResurrectRegexList.Count == 0) return false;
                for (int i = 0; i < ResurrectRegexList.Count; i++)
                {
                    string pattern = ResurrectRegexList[i];
                    if (string.IsNullOrEmpty(pattern)) continue;
                    try
                    {
                        if (Regex.IsMatch(playerName, pattern, RegexOptions.IgnoreCase))
                            return true;
                    }
                    catch
                    {
                        if (playerName.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                    }
                }
            }
            return false;
        }

        public static bool ShouldResurrect(string targetName, bool isParty, double distance, int retryCount)
        {
            if (distance > ResurrectRange)
                return false;

            if (RetryLimit > 0 && retryCount >= RetryLimit)
                return false;

            if (isParty && ResurrectAllParty)
                return true;

            return MatchesRegex(targetName);
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["ResurrectRange"] = ResurrectRange;
            json["ResurrectDelayMs"] = ResurrectDelayMs;
            json["ResurrectAllParty"] = ResurrectAllParty;
            json["RetryLimit"] = RetryLimit;
            json["AcceptFromOthers"] = AcceptFromOthers;
            json["AcceptPartyOnly"] = AcceptPartyOnly;

            JArray regexArr = new JArray();
            lock (ResurrectRegexList)
            {
                foreach (var r in ResurrectRegexList) regexArr.Add(r);
            }
            json["ResurrectRegexList"] = regexArr;

            JArray skillsArr = new JArray();
            lock (SelectedResSkills)
            {
                foreach (var s in SelectedResSkills) skillsArr.Add(s);
            }
            json["SelectedResSkills"] = skillsArr;

            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null) return;
            if (json.ContainsKey("ResurrectRange")) ResurrectRange = (int)json["ResurrectRange"];
            if (json.ContainsKey("ResurrectDelayMs")) ResurrectDelayMs = (int)json["ResurrectDelayMs"];
            if (json.ContainsKey("ResurrectAllParty")) ResurrectAllParty = (bool)json["ResurrectAllParty"];
            if (json.ContainsKey("RetryLimit")) RetryLimit = (int)json["RetryLimit"];
            if (json.ContainsKey("AcceptFromOthers")) AcceptFromOthers = (bool)json["AcceptFromOthers"];
            if (json.ContainsKey("AcceptPartyOnly")) AcceptPartyOnly = (bool)json["AcceptPartyOnly"];

            if (json.ContainsKey("ResurrectRegexList") && json["ResurrectRegexList"] is JArray rArr)
            {
                lock (ResurrectRegexList)
                {
                    ResurrectRegexList.Clear();
                    foreach (var token in rArr) ResurrectRegexList.Add(token.ToString());
                }
            }

            if (json.ContainsKey("SelectedResSkills") && json["SelectedResSkills"] is JArray sArr)
            {
                lock (SelectedResSkills)
                {
                    SelectedResSkills.Clear();
                    foreach (var token in sArr) SelectedResSkills.Add(token.ToString());
                }
            }
        }
    }
}
