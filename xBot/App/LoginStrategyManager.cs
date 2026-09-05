using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using xBot.Game.Objects.Common;

namespace xBot.App
{
    public enum CharacterSelectionStrategy
    {
        FirstFound,
        HighestLevel
    }

    public static class LoginStrategyManager
    {
        public static bool AutomatedLogin { get; set; } = true;
        public static bool StaticCaptcha { get; set; } = false;
        public static string StaticCaptchaCode { get; set; } = "";
        public static int LoginDelaySeconds { get; set; } = 5;
        public static int WaitAfterDCMinutes { get; set; } = 2;
        public static bool AutoStartBot { get; set; } = false;
        public static bool AutoHideClient { get; set; } = false;
        public static bool StayConnected { get; set; } = true; // Failover
        public static CharacterSelectionStrategy Strategy { get; set; } = CharacterSelectionStrategy.FirstFound;
        public static bool AutoEnterSecondaryPasscode { get; set; } = true;
        public static string SecondaryPasscode { get; set; } = "";

        public static SRCharSelection SelectCharacter(List<SRCharSelection> characterList)
        {
            if (characterList == null || characterList.Count == 0)
                return null;

            var availableChars = characterList.Where(c => !c.isDeleting).ToList();
            if (availableChars.Count == 0)
                return null;

            if (Strategy == CharacterSelectionStrategy.HighestLevel)
            {
                return availableChars
                    .OrderByDescending(c => c.Level)
                    .ThenByDescending(c => c.Exp)
                    .FirstOrDefault();
            }

            return availableChars.FirstOrDefault();
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["AutomatedLogin"] = AutomatedLogin;
            json["StaticCaptcha"] = StaticCaptcha;
            json["StaticCaptchaCode"] = StaticCaptchaCode;
            json["LoginDelaySeconds"] = LoginDelaySeconds;
            json["WaitAfterDCMinutes"] = WaitAfterDCMinutes;
            json["AutoStartBot"] = AutoStartBot;
            json["AutoHideClient"] = AutoHideClient;
            json["StayConnected"] = StayConnected;
            json["Strategy"] = (int)Strategy;
            json["AutoEnterSecondaryPasscode"] = AutoEnterSecondaryPasscode;
            json["SecondaryPasscode"] = SecondaryPasscode;
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null)
                return;

            if (json.ContainsKey("AutomatedLogin")) AutomatedLogin = (bool)json["AutomatedLogin"];
            if (json.ContainsKey("StaticCaptcha")) StaticCaptcha = (bool)json["StaticCaptcha"];
            if (json.ContainsKey("StaticCaptchaCode")) StaticCaptchaCode = (string)json["StaticCaptchaCode"];
            if (json.ContainsKey("LoginDelaySeconds")) LoginDelaySeconds = (int)json["LoginDelaySeconds"];
            if (json.ContainsKey("WaitAfterDCMinutes")) WaitAfterDCMinutes = (int)json["WaitAfterDCMinutes"];
            if (json.ContainsKey("AutoStartBot")) AutoStartBot = (bool)json["AutoStartBot"];
            if (json.ContainsKey("AutoHideClient")) AutoHideClient = (bool)json["AutoHideClient"];
            if (json.ContainsKey("StayConnected")) StayConnected = (bool)json["StayConnected"];
            if (json.ContainsKey("Strategy")) Strategy = (CharacterSelectionStrategy)(int)json["Strategy"];
            if (json.ContainsKey("AutoEnterSecondaryPasscode")) AutoEnterSecondaryPasscode = (bool)json["AutoEnterSecondaryPasscode"];
            if (json.ContainsKey("SecondaryPasscode")) SecondaryPasscode = (string)json["SecondaryPasscode"];
        }
    }
}
