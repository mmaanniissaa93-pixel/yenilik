using System;
using Newtonsoft.Json.Linq;

namespace xBot.App
{
    public static class PetPolicy
    {
        #region Saldırı Peti (Attack Pet) Ayarları
        public static bool UseAttackPet { get; set; } = false;
        public static bool DontAttackMonsters { get; set; } = false;
        public static bool AutoSummonAttackPet { get; set; } = false;
        public static bool OnlyInTown { get; set; } = false;
        public static bool OnlyAtTrainingArea { get; set; } = false;
        public static bool AutoReviveAttackPet { get; set; } = false;
        public static int AutoReviveMaxCount { get; set; } = 0; // 0 = Sınırsız
        public static bool OnlyIfHpPotionPresent { get; set; } = false;
        public static bool ReturnTownWhenAttackPetDies { get; set; } = false;
        public static bool ReturnTownOnlyIfNoRevive { get; set; } = false;
        public static bool ProtectAttackPet { get; set; } = false;
        public static bool AutoRecallStuckAttackPet { get; set; } = false;
        public static int AttackRadius { get; set; } = 50;
        #endregion

        #region Fellow Pet Ayarları
        public static bool UseFellowUniqueSkills { get; set; } = false;
        public static bool UsePotionOfGrowth { get; set; } = false;
        public static bool UseFellowReadySkills { get; set; } = false;
        public static bool UseFellowSpRecallInTown { get; set; } = false;
        #endregion

        #region Runtime Takip Değişkenleri
        public static int CurrentReviveCount { get; set; } = 0;
        public static DateTime LastRecallTime { get; set; } = DateTime.MinValue;
        public static DateTime LastSummonAttemptTime { get; set; } = DateTime.MinValue;
        public static DateTime LastSpRecallTime { get; set; } = DateTime.MinValue;
        public static DateTime LastGrowthPotionTime { get; set; } = DateTime.MinValue;
        public static DateTime LastStuckTime { get; set; } = DateTime.MinValue;
        public static Game.Objects.Common.SRCoord LastKnownPetPosition { get; set; } = null;
        public static int StuckCounter { get; set; } = 0;

        public static void ResetRuntimeState()
        {
            CurrentReviveCount = 0;
            StuckCounter = 0;
            LastKnownPetPosition = null;
            LastRecallTime = DateTime.MinValue;
            LastSummonAttemptTime = DateTime.MinValue;
            LastSpRecallTime = DateTime.MinValue;
            LastGrowthPotionTime = DateTime.MinValue;
            LastStuckTime = DateTime.MinValue;
        }

        public static bool CanSummon(bool inTown, bool atTrainingArea, bool hasHpPots)
        {
            if (!AutoSummonAttackPet) return false;
            if (OnlyInTown && !inTown) return false;
            if (OnlyAtTrainingArea && !atTrainingArea) return false;
            if (OnlyIfHpPotionPresent && !hasHpPots) return false;
            if ((DateTime.UtcNow - LastSummonAttemptTime).TotalSeconds < 5) return false;
            return true;
        }

        public static bool CanRevive()
        {
            if (!AutoReviveAttackPet) return false;
            if (AutoReviveMaxCount > 0 && CurrentReviveCount >= AutoReviveMaxCount) return false;
            return true;
        }
        #endregion

        #region JSON Serileştirme
        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["UseAttackPet"] = UseAttackPet;
            json["DontAttackMonsters"] = DontAttackMonsters;
            json["AutoSummonAttackPet"] = AutoSummonAttackPet;
            json["OnlyInTown"] = OnlyInTown;
            json["OnlyAtTrainingArea"] = OnlyAtTrainingArea;
            json["AutoReviveAttackPet"] = AutoReviveAttackPet;
            json["AutoReviveMaxCount"] = AutoReviveMaxCount;
            json["OnlyIfHpPotionPresent"] = OnlyIfHpPotionPresent;
            json["ReturnTownWhenAttackPetDies"] = ReturnTownWhenAttackPetDies;
            json["ReturnTownOnlyIfNoRevive"] = ReturnTownOnlyIfNoRevive;
            json["ProtectAttackPet"] = ProtectAttackPet;
            json["AutoRecallStuckAttackPet"] = AutoRecallStuckAttackPet;
            json["AttackRadius"] = AttackRadius;

            json["UseFellowUniqueSkills"] = UseFellowUniqueSkills;
            json["UsePotionOfGrowth"] = UsePotionOfGrowth;
            json["UseFellowReadySkills"] = UseFellowReadySkills;
            json["UseFellowSpRecallInTown"] = UseFellowSpRecallInTown;

            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null) return;
            try
            {
                if (json["UseAttackPet"] != null) UseAttackPet = (bool)json["UseAttackPet"];
                if (json["DontAttackMonsters"] != null) DontAttackMonsters = (bool)json["DontAttackMonsters"];
                if (json["AutoSummonAttackPet"] != null) AutoSummonAttackPet = (bool)json["AutoSummonAttackPet"];
                if (json["OnlyInTown"] != null) OnlyInTown = (bool)json["OnlyInTown"];
                if (json["OnlyAtTrainingArea"] != null) OnlyAtTrainingArea = (bool)json["OnlyAtTrainingArea"];
                if (json["AutoReviveAttackPet"] != null) AutoReviveAttackPet = (bool)json["AutoReviveAttackPet"];
                if (json["AutoReviveMaxCount"] != null) AutoReviveMaxCount = (int)json["AutoReviveMaxCount"];
                if (json["OnlyIfHpPotionPresent"] != null) OnlyIfHpPotionPresent = (bool)json["OnlyIfHpPotionPresent"];
                if (json["ReturnTownWhenAttackPetDies"] != null) ReturnTownWhenAttackPetDies = (bool)json["ReturnTownWhenAttackPetDies"];
                if (json["ReturnTownOnlyIfNoRevive"] != null) ReturnTownOnlyIfNoRevive = (bool)json["ReturnTownOnlyIfNoRevive"];
                if (json["ProtectAttackPet"] != null) ProtectAttackPet = (bool)json["ProtectAttackPet"];
                if (json["AutoRecallStuckAttackPet"] != null) AutoRecallStuckAttackPet = (bool)json["AutoRecallStuckAttackPet"];
                if (json["AttackRadius"] != null) AttackRadius = (int)json["AttackRadius"];

                if (json["UseFellowUniqueSkills"] != null) UseFellowUniqueSkills = (bool)json["UseFellowUniqueSkills"];
                if (json["UsePotionOfGrowth"] != null) UsePotionOfGrowth = (bool)json["UsePotionOfGrowth"];
                if (json["UseFellowReadySkills"] != null) UseFellowReadySkills = (bool)json["UseFellowReadySkills"];
                if (json["UseFellowSpRecallInTown"] != null) UseFellowSpRecallInTown = (bool)json["UseFellowSpRecallInTown"];
            }
            catch { }
        }
        #endregion
    }
}
