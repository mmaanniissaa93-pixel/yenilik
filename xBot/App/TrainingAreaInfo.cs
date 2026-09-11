using System;

namespace xBot.App
{
    /// <summary>
    /// Kasılma Alanı veri modeli: Adı, Dosya, Menzil, Toplama Menzili, Tipi ve koordinatları tutar.
    /// </summary>
    public class TrainingAreaInfo
    {
        public string Name { get; set; } = "Yeni Kasılma Alanı";
        public ushort Region { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int Z { get; set; }
        public int Radius { get; set; } = 50;
        public int PickRadius { get; set; } = 50;
        public string ScriptPath { get; set; } = "";
        public string Type { get; set; } = "Menzil";

        public TrainingAreaInfo() { }

        public TrainingAreaInfo(string name, ushort region, int x, int y, int z, int radius = 50, int pickRadius = 50, string scriptPath = "", string type = "Menzil")
        {
            Name = string.IsNullOrWhiteSpace(name) ? "Yeni Kasılma Alanı" : name.Trim();
            Region = region;
            X = x;
            Y = y;
            Z = z;
            Radius = radius > 0 ? radius : 50;
            PickRadius = pickRadius > 0 ? pickRadius : 50;
            ScriptPath = scriptPath ?? "";
            Type = string.IsNullOrWhiteSpace(type) ? "Menzil" : type.Trim();
        }
    }
}
