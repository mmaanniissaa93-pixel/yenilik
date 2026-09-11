using Newtonsoft.Json.Linq;

namespace xBot.App
{
    /// <summary>
    /// Training > Area sekmesindeki "Alana Dönüş" bölümünün ayarları.
    /// Karakter eğitim alanının dışındayken dönüş yolculuğunun nasıl
    /// yapılacağını belirler (ThreadBotting dönüş kolu tarafından okunur).
    /// </summary>
    public static class ReturnToAreaPolicy
    {
        /// <summary>Dönüş öncesi binek çağır (vehicle/transport summon scroll).</summary>
        public static bool UseMount { get; set; } = true;
        /// <summary>Dönüş öncesi listedeki buff'ları tazele.</summary>
        public static bool CastBuffs { get; set; } = true;
        /// <summary>Dönüş öncesi hız eşyası kullan (SPEED içerip RETURN içermeyen).</summary>
        public static bool UseSpeedDrug { get; set; } = true;
        /// <summary>Yürüyüş scriptini tersten uygula (alan &lt;-&gt; şehir yönü).</summary>
        public static bool ReverseRoute { get; set; } = false;
        /// <summary>Şehir döngüsünü kullan (Town &gt; EnableTownLoop ile senkron).</summary>
        public static bool TownCycling { get; set; } = true;
        public static bool SkipTownScript { get; set; } = false;
        public static bool ContinueTownScript { get; set; } = true;
        public static bool ReturnIfScriptStuck { get; set; } = false;
        public static bool AvoidStatueOfJustice { get; set; } = false;
        public static bool DontWalkAroundTrainingArea { get; set; } = false;
        public static int ScriptWalkDelay { get; set; } = 0;
        public static int StuckGoBackSeconds { get; set; } = 0;
        public static int StuckReturnSeconds { get; set; } = 0;

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["UseMount"] = UseMount;
            json["CastBuffs"] = CastBuffs;
            json["UseSpeedDrug"] = UseSpeedDrug;
            json["ReverseRoute"] = ReverseRoute;
            json["TownCycling"] = TownCycling;
            json["SkipTownScript"] = SkipTownScript;
            json["ContinueTownScript"] = ContinueTownScript;
            json["ReturnIfScriptStuck"] = ReturnIfScriptStuck;
            json["AvoidStatueOfJustice"] = AvoidStatueOfJustice;
            json["DontWalkAroundTrainingArea"] = DontWalkAroundTrainingArea;
            json["ScriptWalkDelay"] = ScriptWalkDelay;
            json["StuckGoBackSeconds"] = StuckGoBackSeconds;
            json["StuckReturnSeconds"] = StuckReturnSeconds;
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null)
                return;
            if (json.ContainsKey("UseMount")) UseMount = (bool)json["UseMount"];
            if (json.ContainsKey("CastBuffs")) CastBuffs = (bool)json["CastBuffs"];
            if (json.ContainsKey("UseSpeedDrug")) UseSpeedDrug = (bool)json["UseSpeedDrug"];
            if (json.ContainsKey("ReverseRoute")) ReverseRoute = (bool)json["ReverseRoute"];
            if (json.ContainsKey("TownCycling")) TownCycling = (bool)json["TownCycling"];
            if (json.ContainsKey("SkipTownScript")) SkipTownScript = (bool)json["SkipTownScript"];
            if (json.ContainsKey("ContinueTownScript")) ContinueTownScript = (bool)json["ContinueTownScript"];
            if (json.ContainsKey("ReturnIfScriptStuck")) ReturnIfScriptStuck = (bool)json["ReturnIfScriptStuck"];
            if (json.ContainsKey("AvoidStatueOfJustice")) AvoidStatueOfJustice = (bool)json["AvoidStatueOfJustice"];
            if (json.ContainsKey("DontWalkAroundTrainingArea")) DontWalkAroundTrainingArea = (bool)json["DontWalkAroundTrainingArea"];
            if (json.ContainsKey("ScriptWalkDelay")) ScriptWalkDelay = (int)json["ScriptWalkDelay"];
            if (json.ContainsKey("StuckGoBackSeconds")) StuckGoBackSeconds = (int)json["StuckGoBackSeconds"];
            if (json.ContainsKey("StuckReturnSeconds")) StuckReturnSeconds = (int)json["StuckReturnSeconds"];
        }
    }
}
