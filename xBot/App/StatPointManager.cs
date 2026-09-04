using System;
using Newtonsoft.Json.Linq;
using xBot.Game;

namespace xBot.App
{
    public static class StatPointManager
    {
        public static bool AutoDistributeEnabled { get; set; } = false;
        public static int TargetSTR { get; set; } = 0;
        public static int TargetINT { get; set; } = 3;

        public static void CheckAndDistribute()
        {
            if (!AutoDistributeEnabled || InfoManager.Character == null)
                return;

            ushort availablePoints = InfoManager.Character.StatPoints;
            if (availablePoints == 0)
                return;

            int totalRatio = TargetSTR + TargetINT;
            if (totalRatio <= 0)
                return;

            Window.Get?.Log("Stat Distribution: " + availablePoints + " stat point(s) available. Allocating automatically...");

            // If pure INT
            if (TargetSTR == 0 && TargetINT > 0)
            {
                for (int i = 0; i < availablePoints; i++)
                {
                    PacketBuilder.AddStatPointINT();
                    System.Threading.Thread.Sleep(100);
                }
                return;
            }

            // If pure STR
            if (TargetINT == 0 && TargetSTR > 0)
            {
                for (int i = 0; i < availablePoints; i++)
                {
                    PacketBuilder.AddStatPointSTR();
                    System.Threading.Thread.Sleep(100);
                }
                return;
            }

            // Hybrid ratio distribution
            while (availablePoints > 0)
            {
                int strBatch = Math.Min(TargetSTR, (int)availablePoints);
                for (int s = 0; s < strBatch; s++)
                {
                    PacketBuilder.AddStatPointSTR();
                    availablePoints--;
                    System.Threading.Thread.Sleep(100);
                }

                int intBatch = Math.Min(TargetINT, (int)availablePoints);
                for (int i = 0; i < intBatch; i++)
                {
                    PacketBuilder.AddStatPointINT();
                    availablePoints--;
                    System.Threading.Thread.Sleep(100);
                }
            }
        }

        public static JObject ToJson()
        {
            JObject json = new JObject();
            json["AutoDistributeEnabled"] = AutoDistributeEnabled;
            json["TargetSTR"] = TargetSTR;
            json["TargetINT"] = TargetINT;
            return json;
        }

        public static void FromJson(JObject json)
        {
            if (json == null)
                return;

            if (json.ContainsKey("AutoDistributeEnabled")) AutoDistributeEnabled = (bool)json["AutoDistributeEnabled"];
            if (json.ContainsKey("TargetSTR")) TargetSTR = (int)json["TargetSTR"];
            if (json.ContainsKey("TargetINT")) TargetINT = (int)json["TargetINT"];
        }
    }
}
