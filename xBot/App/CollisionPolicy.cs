using System;
using Newtonsoft.Json.Linq;
using xBot.Game.Navigation;

namespace xBot.App
{
    /// <summary>
    /// Kasılma Alanı > Çarpışma (Training Area > Collision) sekmesindeki 8 politikanın
    /// merkezi durum ve karar mantığı. Bot motorunun hedef yaklaşımı, engel aşma manevraları,
    /// eşya toplama esnasında rota belirleme ve şehirler arası ışınlanma seçimlerini yönetir.
    /// </summary>
    public static class CollisionPolicy
    {
        // 1. Kasılma alanında engel tespit etmeyi aktifleştir
        public static bool EnableCollisionInTrainingArea { get; set; } = true;

        // 2. Engellerin etrafından dolaş
        public static bool NavigateAroundObstacles { get; set; } = true;

        // 3. Eşyaları toplarken engellerin etrafından dolaş
        public static bool NavigateAroundObstaclesWhilePicking { get; set; } = true;

        // 4. Samarkand'a ışınlanmayı devre dışı bırak
        public static bool DisableTeleportSamarkand { get; set; } = false;

        // 5. Alexandria'ya ışınlanmayı devre dışı bırak
        public static bool DisableTeleportAlexandria { get; set; } = false;

        // 6. Guide/Advice NPCs Devre dışı bırak
        public static bool DisableGuideAdviceNPCs { get; set; } = false;

        // 7. Taklamakan'ı devre dışı bırak
        public static bool DisableTaklamakan { get; set; } = false;

        // 8. Işınlanma seviyesini yoksay
        public static bool IgnoreTeleportLevel { get; set; } = false;

        public static JObject ToJson()
        {
            var json = new JObject();
            json["EnableCollisionInTrainingArea"] = EnableCollisionInTrainingArea;
            json["NavigateAroundObstacles"] = NavigateAroundObstacles;
            json["NavigateAroundObstaclesWhilePicking"] = NavigateAroundObstaclesWhilePicking;
            json["DisableTeleportSamarkand"] = DisableTeleportSamarkand;
            json["DisableTeleportAlexandria"] = DisableTeleportAlexandria;
            json["DisableGuideAdviceNPCs"] = DisableGuideAdviceNPCs;
            json["DisableTaklamakan"] = DisableTaklamakan;
            json["IgnoreTeleportLevel"] = IgnoreTeleportLevel;
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null) return;
            if (json.ContainsKey("EnableCollisionInTrainingArea"))
                EnableCollisionInTrainingArea = (bool)json["EnableCollisionInTrainingArea"];
            if (json.ContainsKey("NavigateAroundObstacles"))
                NavigateAroundObstacles = (bool)json["NavigateAroundObstacles"];
            if (json.ContainsKey("NavigateAroundObstaclesWhilePicking"))
                NavigateAroundObstaclesWhilePicking = (bool)json["NavigateAroundObstaclesWhilePicking"];
            if (json.ContainsKey("DisableTeleportSamarkand"))
                DisableTeleportSamarkand = (bool)json["DisableTeleportSamarkand"];
            if (json.ContainsKey("DisableTeleportAlexandria"))
                DisableTeleportAlexandria = (bool)json["DisableTeleportAlexandria"];
            if (json.ContainsKey("DisableGuideAdviceNPCs"))
                DisableGuideAdviceNPCs = (bool)json["DisableGuideAdviceNPCs"];
            if (json.ContainsKey("DisableTaklamakan"))
                DisableTaklamakan = (bool)json["DisableTaklamakan"];
            if (json.ContainsKey("IgnoreTeleportLevel"))
                IgnoreTeleportLevel = (bool)json["IgnoreTeleportLevel"];
        }

        /// <summary>
        /// Bir teleport/ferry köprüsünün Çarpışma sekmesi kurallarına göre uygun olup olmadığını denetler.
        /// </summary>
        public static bool IsLinkAllowed(TeleportLinkInfo link, int characterLevel = 0)
        {
            if (link == null) return false;

            string src = link.SourceName ?? "";
            string dst = link.DestinationName ?? "";

            // 4. Samarkand filtresi
            if (DisableTeleportSamarkand)
            {
                if (link.SourceId == 25 || link.DestinationId == 25 ||
                    src.IndexOf("Samarkand", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dst.IndexOf("Samarkand", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
            }

            // 5. Alexandria filtresi
            if (DisableTeleportAlexandria)
            {
                if (link.SourceId == 175 || link.SourceId == 176 ||
                    link.DestinationId == 175 || link.DestinationId == 176 ||
                    src.IndexOf("Alexandria", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dst.IndexOf("Alexandria", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
            }

            // 6. Guide/Advice NPCs filtresi
            if (DisableGuideAdviceNPCs)
            {
                if (src.IndexOf("Guide", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    src.IndexOf("Advice", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    src.IndexOf("Rehber", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    src.IndexOf("Tavsiye", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dst.IndexOf("Guide", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dst.IndexOf("Advice", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dst.IndexOf("Rehber", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dst.IndexOf("Tavsiye", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
            }

            // 7. Taklamakan filtresi
            if (DisableTaklamakan)
            {
                if (src.IndexOf("Taklamakan", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    dst.IndexOf("Taklamakan", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
            }

            // 8. Seviye kısıtı (IgnoreTeleportLevel false ise kontrol edilir)
            if (!IgnoreTeleportLevel && characterLevel > 0)
            {
                int minLvl = GetMinLevelForDestination(link.DestinationId, dst);
                if (characterLevel < minLvl)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool IsTaklamakanRegion(int regionId)
        {
            byte secX = (byte)(regionId & 0xFF);
            byte secY = (byte)((regionId >> 8) & 0xFF);
            return (secX >= 16 && secX <= 25 && secY >= 25 && secY <= 33);
        }

        public static int GetMinLevelForDestination(uint destId, string destName)
        {
            if (destId == 175 || destId == 176 || (destName != null && destName.IndexOf("Alexandria", StringComparison.OrdinalIgnoreCase) >= 0))
                return 100;
            if (destId == 25 || (destName != null && destName.IndexOf("Samarkand", StringComparison.OrdinalIgnoreCase) >= 0))
                return 20;
            if (destId == 5 || (destName != null && destName.IndexOf("Hotan", StringComparison.OrdinalIgnoreCase) >= 0))
                return 20;
            if (destId == 2 || (destName != null && destName.IndexOf("Donwhang", StringComparison.OrdinalIgnoreCase) >= 0))
                return 20;
            if (destName != null && destName.IndexOf("Jupiter", StringComparison.OrdinalIgnoreCase) >= 0)
                return 110;
            return 1;
        }
    }
}
