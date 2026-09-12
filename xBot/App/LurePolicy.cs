using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace xBot.App
{
    public static class LurePolicy
    {
        public static bool WalkBackDistEnabled { get; set; } = false;
        public static int WalkBackDist { get; set; } = 0;
        public static bool LureSkillEnabled { get; set; } = false;
        public static string LureSkillName { get; set; } = "";
        public static bool BuffAtLureEnd { get; set; } = false;

        public static bool StopDeadPartyEnabled { get; set; } = false;
        public static int StopDeadPartyCount { get; set; } = 0;

        public static bool StopTownLessEnabled { get; set; } = false;
        public static int StopTownLessCount { get; set; } = 0;

        public static bool StopGiantPartyEnabled { get; set; } = false;
        public static int StopGiantPartyCount { get; set; } = 0;

        public static bool StopMobCountEnabled { get; set; } = false;
        public static int StopMobCount { get; set; } = 0;

        public static bool AttackLimitEnabled { get; set; } = false;
        public static int AttackMobLimit { get; set; } = 999;

        public static int DelayEdgeMs { get; set; } = 5000;
        public static int DelayCenterMs { get; set; } = 5000;
        public static int DelayHalfMs { get; set; } = 2000;

        public static bool UseScript { get; set; } = false;
        public static string ScriptPath { get; set; } = "";

        public static bool RandomWalk { get; set; } = false;
        public static bool WalkToSpawns { get; set; } = false;
        public static bool SmartWalk { get; set; } = false;
        public static bool StopIfPartyAway { get; set; } = false;
        public static int PartyMemberFarAwayDistance { get; set; } = 50;
        public static List<string> PartyNearWhitelist { get; } = new List<string>();
        public static bool BuffAfterScriptCommand { get; set; } = false;

        public static bool ShouldPauseLure(int deadPartyCount, int giantPartyCount, int areaMobCount, bool partyNear)
        {
            if (StopDeadPartyEnabled && StopDeadPartyCount > 0 && deadPartyCount >= StopDeadPartyCount)
                return true;

            if (StopGiantPartyEnabled && StopGiantPartyCount > 0 && giantPartyCount >= StopGiantPartyCount)
                return true;

            if (StopMobCountEnabled && StopMobCount > 0 && areaMobCount >= StopMobCount)
                return true;

            if (StopIfPartyAway && !partyNear)
                return true;

            return false;
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["WalkBackDistEnabled"] = WalkBackDistEnabled;
            json["WalkBackDist"] = WalkBackDist;
            json["LureSkillEnabled"] = LureSkillEnabled;
            json["LureSkillName"] = LureSkillName ?? "";
            json["BuffAtLureEnd"] = BuffAtLureEnd;

            json["StopDeadPartyEnabled"] = StopDeadPartyEnabled;
            json["StopDeadPartyCount"] = StopDeadPartyCount;
            json["StopTownLessEnabled"] = StopTownLessEnabled;
            json["StopTownLessCount"] = StopTownLessCount;
            json["StopGiantPartyEnabled"] = StopGiantPartyEnabled;
            json["StopGiantPartyCount"] = StopGiantPartyCount;
            json["StopMobCountEnabled"] = StopMobCountEnabled;
            json["StopMobCount"] = StopMobCount;

            json["AttackLimitEnabled"] = AttackLimitEnabled;
            json["AttackMobLimit"] = AttackMobLimit;
            json["DelayEdgeMs"] = DelayEdgeMs;
            json["DelayCenterMs"] = DelayCenterMs;
            json["DelayHalfMs"] = DelayHalfMs;

            json["UseScript"] = UseScript;
            json["ScriptPath"] = ScriptPath ?? "";

            json["RandomWalk"] = RandomWalk;
            json["WalkToSpawns"] = WalkToSpawns;
            json["SmartWalk"] = SmartWalk;
            json["StopIfPartyAway"] = StopIfPartyAway;
            json["PartyMemberFarAwayDistance"] = PartyMemberFarAwayDistance;
            json["BuffAfterScriptCommand"] = BuffAfterScriptCommand;

            JArray pArr = new JArray();
            lock (PartyNearWhitelist)
            {
                foreach (var p in PartyNearWhitelist) pArr.Add(p);
            }
            json["PartyNearWhitelist"] = pArr;

            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null) return;
            if (json.ContainsKey("WalkBackDistEnabled")) WalkBackDistEnabled = (bool)json["WalkBackDistEnabled"];
            if (json.ContainsKey("WalkBackDist")) WalkBackDist = (int)json["WalkBackDist"];
            if (json.ContainsKey("LureSkillEnabled")) LureSkillEnabled = (bool)json["LureSkillEnabled"];
            if (json.ContainsKey("LureSkillName")) LureSkillName = (string)json["LureSkillName"];
            if (json.ContainsKey("BuffAtLureEnd")) BuffAtLureEnd = (bool)json["BuffAtLureEnd"];

            if (json.ContainsKey("StopDeadPartyEnabled")) StopDeadPartyEnabled = (bool)json["StopDeadPartyEnabled"];
            if (json.ContainsKey("StopDeadPartyCount")) StopDeadPartyCount = (int)json["StopDeadPartyCount"];
            if (json.ContainsKey("StopTownLessEnabled")) StopTownLessEnabled = (bool)json["StopTownLessEnabled"];
            if (json.ContainsKey("StopTownLessCount")) StopTownLessCount = (int)json["StopTownLessCount"];
            if (json.ContainsKey("StopGiantPartyEnabled")) StopGiantPartyEnabled = (bool)json["StopGiantPartyEnabled"];
            if (json.ContainsKey("StopGiantPartyCount")) StopGiantPartyCount = (int)json["StopGiantPartyCount"];
            if (json.ContainsKey("StopMobCountEnabled")) StopMobCountEnabled = (bool)json["StopMobCountEnabled"];
            if (json.ContainsKey("StopMobCount")) StopMobCount = (int)json["StopMobCount"];

            if (json.ContainsKey("AttackLimitEnabled")) AttackLimitEnabled = (bool)json["AttackLimitEnabled"];
            if (json.ContainsKey("AttackMobLimit")) AttackMobLimit = (int)json["AttackMobLimit"];
            if (json.ContainsKey("DelayEdgeMs")) DelayEdgeMs = (int)json["DelayEdgeMs"];
            if (json.ContainsKey("DelayCenterMs")) DelayCenterMs = (int)json["DelayCenterMs"];
            if (json.ContainsKey("DelayHalfMs")) DelayHalfMs = (int)json["DelayHalfMs"];

            if (json.ContainsKey("UseScript")) UseScript = (bool)json["UseScript"];
            if (json.ContainsKey("ScriptPath")) ScriptPath = (string)json["ScriptPath"];

            if (json.ContainsKey("RandomWalk")) RandomWalk = (bool)json["RandomWalk"];
            if (json.ContainsKey("WalkToSpawns")) WalkToSpawns = (bool)json["WalkToSpawns"];
            if (json.ContainsKey("SmartWalk")) SmartWalk = (bool)json["SmartWalk"];
            if (json.ContainsKey("StopIfPartyAway")) StopIfPartyAway = (bool)json["StopIfPartyAway"];
            if (json.ContainsKey("PartyMemberFarAwayDistance")) PartyMemberFarAwayDistance = (int)json["PartyMemberFarAwayDistance"];
            if (json.ContainsKey("BuffAfterScriptCommand")) BuffAfterScriptCommand = (bool)json["BuffAfterScriptCommand"];

            if (json.ContainsKey("PartyNearWhitelist") && json["PartyNearWhitelist"] is JArray arr)
            {
                lock (PartyNearWhitelist)
                {
                    PartyNearWhitelist.Clear();
                    foreach (var token in arr) PartyNearWhitelist.Add(token.ToString());
                }
            }
        }
    }
}
