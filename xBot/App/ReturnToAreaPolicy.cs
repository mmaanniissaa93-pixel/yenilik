using Newtonsoft.Json.Linq;
using xBot.Game.Objects.Common;

namespace xBot.App
{
    /// <summary>
    /// Training > Area sekmesindeki "Alana Dönüş" ve Training > Komut sekmesindeki ayarlar.
    /// Karakter eğitim alanının dışındayken ve script koşarken botun nasıl davranacağını belirler.
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
        public static bool DontWalkAroundTrainingArea { get; set; } = false;

        // --- Kasılma Alanı > Komut (Script) sekmesindeki 11 seçenek ---
        /// <summary>1. Şehir döngüsünü atla</summary>
        public static bool SkipTownScript { get; set; } = false;
        /// <summary>2. Şehir döngüsüne devam et</summary>
        public static bool ContinueTownScript { get; set; } = true;
        /// <summary>3. Yolda takılırsa şehre dön</summary>
        public static bool ReturnIfScriptStuck { get; set; } = false;
        /// <summary>4. Komutta Statue of Justice'den kaç</summary>
        public static bool AvoidStatueOfJustice { get; set; } = false;
        /// <summary>5. Yürüme gecikmesi aktif mi?</summary>
        public static bool EnableScriptWalkDelay { get; set; } = false;
        /// <summary>5. Yürüme gecikmesi (ms)</summary>
        public static int ScriptWalkDelay { get; set; } = 1000;
        /// <summary>6. Karakter takılırsa bir koordinat geri dön</summary>
        public static bool GoBackCoordIfStuck { get; set; } = true;
        /// <summary>6. Karakter takılırsa bir koordinat geri dön süresi (s)</summary>
        public static int GoBackCoordSeconds { get; set; } = 15;
        /// <summary>7. Şu süre boyunca takılı kalırsa şehre dön aktif mi?</summary>
        public static bool ReturnIfStuckAfterSecondsEnabled { get; set; } = false;
        /// <summary>7. Şu süre boyunca takılı kalırsa şehre dön süresi (s)</summary>
        public static int ReturnIfStuckSeconds { get; set; } = 90;
        /// <summary>8. Kasma alanına giderken fellow'a bin</summary>
        public static bool RideFellowPet { get; set; } = false;
        /// <summary>9. Mağaralarda yeniden bin</summary>
        public static bool RemountInCaves { get; set; } = false;
        /// <summary>10. Son return kullanılan yere dönmek için şehirdeki Teleport NPC'lerini kullan</summary>
        public static bool UseTownTeleportForLastRecall { get; set; } = false;
        /// <summary>11. Öldüğün yere dönmek için şehirdeki Teleport NPC'lerini kullan</summary>
        public static bool UseTownTeleportForLastDeath { get; set; } = false;

        // Geriye dönük uyumluluk alanları:
        public static int StuckGoBackSeconds { get => GoBackCoordSeconds; set => GoBackCoordSeconds = value; }
        public static int StuckReturnSeconds { get => ReturnIfStuckSeconds; set => ReturnIfStuckSeconds = value; }

        // Runtime konum takibi:
        public static SRCoord LastRecallPosition { get; set; }
        public static SRCoord LastDeathPosition { get; set; }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["UseMount"] = UseMount;
            json["CastBuffs"] = CastBuffs;
            json["UseSpeedDrug"] = UseSpeedDrug;
            json["ReverseRoute"] = ReverseRoute;
            json["TownCycling"] = TownCycling;
            json["DontWalkAroundTrainingArea"] = DontWalkAroundTrainingArea;

            json["SkipTownScript"] = SkipTownScript;
            json["ContinueTownScript"] = ContinueTownScript;
            json["ReturnIfScriptStuck"] = ReturnIfScriptStuck;
            json["AvoidStatueOfJustice"] = AvoidStatueOfJustice;
            json["EnableScriptWalkDelay"] = EnableScriptWalkDelay;
            json["ScriptWalkDelay"] = ScriptWalkDelay;
            json["GoBackCoordIfStuck"] = GoBackCoordIfStuck;
            json["GoBackCoordSeconds"] = GoBackCoordSeconds;
            json["ReturnIfStuckAfterSecondsEnabled"] = ReturnIfStuckAfterSecondsEnabled;
            json["ReturnIfStuckSeconds"] = ReturnIfStuckSeconds;
            json["RideFellowPet"] = RideFellowPet;
            json["RemountInCaves"] = RemountInCaves;
            json["UseTownTeleportForLastRecall"] = UseTownTeleportForLastRecall;
            json["UseTownTeleportForLastDeath"] = UseTownTeleportForLastDeath;

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
            if (json.ContainsKey("DontWalkAroundTrainingArea")) DontWalkAroundTrainingArea = (bool)json["DontWalkAroundTrainingArea"];

            if (json.ContainsKey("SkipTownScript")) SkipTownScript = (bool)json["SkipTownScript"];
            if (json.ContainsKey("ContinueTownScript")) ContinueTownScript = (bool)json["ContinueTownScript"];
            if (json.ContainsKey("ReturnIfScriptStuck")) ReturnIfScriptStuck = (bool)json["ReturnIfScriptStuck"];
            if (json.ContainsKey("AvoidStatueOfJustice")) AvoidStatueOfJustice = (bool)json["AvoidStatueOfJustice"];
            if (json.ContainsKey("EnableScriptWalkDelay")) EnableScriptWalkDelay = (bool)json["EnableScriptWalkDelay"];
            if (json.ContainsKey("ScriptWalkDelay")) ScriptWalkDelay = (int)json["ScriptWalkDelay"];
            if (json.ContainsKey("GoBackCoordIfStuck")) GoBackCoordIfStuck = (bool)json["GoBackCoordIfStuck"];
            if (json.ContainsKey("GoBackCoordSeconds")) GoBackCoordSeconds = (int)json["GoBackCoordSeconds"];
            if (json.ContainsKey("ReturnIfStuckAfterSecondsEnabled")) ReturnIfStuckAfterSecondsEnabled = (bool)json["ReturnIfStuckAfterSecondsEnabled"];
            if (json.ContainsKey("ReturnIfStuckSeconds")) ReturnIfStuckSeconds = (int)json["ReturnIfStuckSeconds"];
            if (json.ContainsKey("RideFellowPet")) RideFellowPet = (bool)json["RideFellowPet"];
            if (json.ContainsKey("RemountInCaves")) RemountInCaves = (bool)json["RemountInCaves"];
            if (json.ContainsKey("UseTownTeleportForLastRecall")) UseTownTeleportForLastRecall = (bool)json["UseTownTeleportForLastRecall"];
            if (json.ContainsKey("UseTownTeleportForLastDeath")) UseTownTeleportForLastDeath = (bool)json["UseTownTeleportForLastDeath"];

            if (json.ContainsKey("StuckGoBackSeconds") && !json.ContainsKey("GoBackCoordSeconds")) GoBackCoordSeconds = (int)json["StuckGoBackSeconds"];
            if (json.ContainsKey("StuckReturnSeconds") && !json.ContainsKey("ReturnIfStuckSeconds")) ReturnIfStuckSeconds = (int)json["StuckReturnSeconds"];
        }
    }
}
