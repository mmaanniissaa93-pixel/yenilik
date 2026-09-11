using System;
using Newtonsoft.Json.Linq;

namespace xBot.App
{
    /// <summary>
    /// Kasılma Alanı > Seçenekler (Training Area > Options) sekmesindeki 20 politikanın
    /// merkezi durum ve karar mantığı. Bot motorunun (Bot.IA) kasılma alanındaki davranışlarını,
    /// eşya kuşanma, kutu/çiçek/zerk/pandora kullanımını ve sınır koruma kurallarını yönetir.
    /// </summary>
    public static class TrainingOptionsPolicy
    {
        // Sol Kolon
        // 1. Kasılma alanında dolaşma
        public static bool DontWalkAroundTrainingArea
        {
            get => ReturnToAreaPolicy.DontWalkAroundTrainingArea;
            set => ReturnToAreaPolicy.DontWalkAroundTrainingArea = value;
        }

        // 2. Treasure Boxes kullan
        public static bool UseTreasureBoxes { get; set; } = false;

        // 3. Paskalya Yumurtası Etkinlik NPClerini kullan
        public static bool UseEasterEggEventNpcs { get; set; } = false;

        // 4. Daha yüksek eşyaları otomatik kuşan
        public static bool AutoEquipBetterItems { get; set; } = false;

        // 5. Kasılma alanında çiçek kullan
        public static bool SummonFlowersInTrainingArea { get; set; } = false;

        // 6. Repair Hammer kullan
        public static bool UseRepairHammer
        {
            get => ProtectionManager.UseRepairHammer;
            set => ProtectionManager.UseRepairHammer = value;
        }

        // 7. Berserker regeneration (zerk potu) kullan
        public static bool UseZerkPotion
        {
            get => CombatAIEngine.UseZerkPotion;
            set => CombatAIEngine.UseZerkPotion = value;
        }

        // 8. Energy of Life potu kullan
        public static bool UseEnergyOfLife
        {
            get => CombatAIEngine.UseEnergyOfLife;
            set => CombatAIEngine.UseEnergyOfLife = value;
        }

        // 9. Energy of Life zerki doldur
        public static bool UseEnergyOfLifeForZerk { get; set; } = false;

        // 10. Öldüğün zaman reverse kullan
        public static bool UseReverseOnDeath
        {
            get => ProtectionManager.UseReverseOnDeath;
            set => ProtectionManager.UseReverseOnDeath = value;
        }

        // 11. Şehre döndükten sonra reverse kullan
        public static bool UseReverseAfterTown
        {
            get => ProtectionManager.UseReverseAfterTown;
            set => ProtectionManager.UseReverseAfterTown = value;
        }

        // 12. Kasılma alanında Pandora box veya monster scroll kullan
        public static bool UsePandoraBoxOrMonsterScroll { get; set; } = false;

        // 13. Tekrar kullanmadan önce tüm Strong mobların ölmesini bekle
        public static bool WaitForStrongMobsBeforeSummon { get; set; } = false;

        // 14. Hızlı Koşma Kullan (Speed Drug)
        public static bool UseSpeedDrugs
        {
            get => ProtectionManager.UseSpeedDrugs || ReturnToAreaPolicy.UseSpeedDrug;
            set
            {
                ProtectionManager.UseSpeedDrugs = value;
                ReturnToAreaPolicy.UseSpeedDrug = value;
            }
        }

        // 15. Sadece alana giderken hızlı koşma kullan
        public static bool SpeedDrugsOnlyInScript
        {
            get => ProtectionManager.SpeedDrugsOnlyInScript;
            set => ProtectionManager.SpeedDrugsOnlyInScript = value;
        }

        // Sağ Kolon
        // 16. Training alanında kal
        public static bool StayInTrainingArea { get; set; } = false;

        // 17. Statue of Justice mobuna saldır
        public static bool AttackStatueOfJustice { get; set; } = false;

        // 18. At çıkarma (At/Binek çağırmayı engelle)
        public static bool DoNotSpawnMount { get; set; } = false;

        // 19. Düşük MP'de çekil
        public static bool WithdrawOnLowMp { get; set; } = false;
        public static int LowMpPercent { get; set; } = 0;

        // 20. Pandora sayısını koru
        public static bool PreservePandora { get; set; } = false;
        public static int PreservePandoraCount { get; set; } = 3;

        public static JObject ToJson()
        {
            var json = new JObject();
            json["DontWalkAroundTrainingArea"] = DontWalkAroundTrainingArea;
            json["UseTreasureBoxes"] = UseTreasureBoxes;
            json["UseEasterEggEventNpcs"] = UseEasterEggEventNpcs;
            json["AutoEquipBetterItems"] = AutoEquipBetterItems;
            json["SummonFlowersInTrainingArea"] = SummonFlowersInTrainingArea;
            json["UseRepairHammer"] = UseRepairHammer;
            json["UseZerkPotion"] = UseZerkPotion;
            json["UseEnergyOfLife"] = UseEnergyOfLife;
            json["UseEnergyOfLifeForZerk"] = UseEnergyOfLifeForZerk;
            json["UseReverseOnDeath"] = UseReverseOnDeath;
            json["UseReverseAfterTown"] = UseReverseAfterTown;
            json["UsePandoraBoxOrMonsterScroll"] = UsePandoraBoxOrMonsterScroll;
            json["WaitForStrongMobsBeforeSummon"] = WaitForStrongMobsBeforeSummon;
            json["UseSpeedDrugs"] = UseSpeedDrugs;
            json["SpeedDrugsOnlyInScript"] = SpeedDrugsOnlyInScript;
            json["StayInTrainingArea"] = StayInTrainingArea;
            json["AttackStatueOfJustice"] = AttackStatueOfJustice;
            json["DoNotSpawnMount"] = DoNotSpawnMount;
            json["WithdrawOnLowMp"] = WithdrawOnLowMp;
            json["LowMpPercent"] = LowMpPercent;
            json["PreservePandora"] = PreservePandora;
            json["PreservePandoraCount"] = PreservePandoraCount;
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null) return;
            if (json.ContainsKey("DontWalkAroundTrainingArea"))
                DontWalkAroundTrainingArea = (bool)json["DontWalkAroundTrainingArea"];
            if (json.ContainsKey("UseTreasureBoxes"))
                UseTreasureBoxes = (bool)json["UseTreasureBoxes"];
            if (json.ContainsKey("UseEasterEggEventNpcs"))
                UseEasterEggEventNpcs = (bool)json["UseEasterEggEventNpcs"];
            if (json.ContainsKey("AutoEquipBetterItems"))
                AutoEquipBetterItems = (bool)json["AutoEquipBetterItems"];
            if (json.ContainsKey("SummonFlowersInTrainingArea"))
                SummonFlowersInTrainingArea = (bool)json["SummonFlowersInTrainingArea"];
            if (json.ContainsKey("UseRepairHammer"))
                UseRepairHammer = (bool)json["UseRepairHammer"];
            if (json.ContainsKey("UseZerkPotion"))
                UseZerkPotion = (bool)json["UseZerkPotion"];
            if (json.ContainsKey("UseEnergyOfLife"))
                UseEnergyOfLife = (bool)json["UseEnergyOfLife"];
            if (json.ContainsKey("UseEnergyOfLifeForZerk"))
                UseEnergyOfLifeForZerk = (bool)json["UseEnergyOfLifeForZerk"];
            if (json.ContainsKey("UseReverseOnDeath"))
                UseReverseOnDeath = (bool)json["UseReverseOnDeath"];
            if (json.ContainsKey("UseReverseAfterTown"))
                UseReverseAfterTown = (bool)json["UseReverseAfterTown"];
            if (json.ContainsKey("UsePandoraBoxOrMonsterScroll"))
                UsePandoraBoxOrMonsterScroll = (bool)json["UsePandoraBoxOrMonsterScroll"];
            if (json.ContainsKey("WaitForStrongMobsBeforeSummon"))
                WaitForStrongMobsBeforeSummon = (bool)json["WaitForStrongMobsBeforeSummon"];
            if (json.ContainsKey("UseSpeedDrugs"))
                UseSpeedDrugs = (bool)json["UseSpeedDrugs"];
            if (json.ContainsKey("SpeedDrugsOnlyInScript"))
                SpeedDrugsOnlyInScript = (bool)json["SpeedDrugsOnlyInScript"];
            if (json.ContainsKey("StayInTrainingArea"))
                StayInTrainingArea = (bool)json["StayInTrainingArea"];
            if (json.ContainsKey("AttackStatueOfJustice"))
                AttackStatueOfJustice = (bool)json["AttackStatueOfJustice"];
            if (json.ContainsKey("DoNotSpawnMount"))
                DoNotSpawnMount = (bool)json["DoNotSpawnMount"];
            if (json.ContainsKey("WithdrawOnLowMp"))
                WithdrawOnLowMp = (bool)json["WithdrawOnLowMp"];
            if (json.ContainsKey("LowMpPercent"))
                LowMpPercent = (int)json["LowMpPercent"];
            if (json.ContainsKey("PreservePandora"))
                PreservePandora = (bool)json["PreservePandora"];
            if (json.ContainsKey("PreservePandoraCount"))
                PreservePandoraCount = (int)json["PreservePandoraCount"];
        }
    }
}
