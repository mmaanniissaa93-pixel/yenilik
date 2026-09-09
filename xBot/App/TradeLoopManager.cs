using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;

namespace xBot.App
{
    public enum TradeAmountMode { Fill, Quantity, Star }
    public enum TradeMountMode { StayMounted, StayOff, Remount }

    public sealed class TradeRouteDefinition
    {
        public string StartTown { get; set; } = "";
        public string EndTown { get; set; } = "";
        public string ScriptPath { get; set; } = "";
        public string TransportName { get; set; } = "";
        public string ItemName { get; set; } = "";
        public TradeAmountMode AmountMode { get; set; } = TradeAmountMode.Fill;
        public int Amount { get; set; }
    }

    /// <summary>
    /// phBot benzeri, yönlü route listesini oldtrade script komutlarıyla çalıştırır.
    /// Normal bot döngüsü yerine Bot.ThreadBotting içinde tek sahibi vardır.
    /// </summary>
    public static class TradeLoopManager
    {
        private static readonly object Sync = new object();
        private static readonly List<TradeRouteDefinition> Routes = new List<TradeRouteDefinition>();
        private static volatile bool startRequested;
        private static volatile bool running;

        public static int RepeatCount { get; set; } = 1;
        public static bool UseReturnScroll { get; set; }
        public static bool TerminateTransportAtEnd { get; set; } = true;
        public static bool SkipTownLoop { get; set; } = true;
        public static TradeMountMode MountMode { get; set; } = TradeMountMode.Remount;
        public static bool AttackSpawnedThieves { get; set; }
        public static int AttackRadius { get; set; } = 30;
        public static string Status { get; private set; } = "Hazır";
        public static bool IsRunning { get { return running; } }
        public static bool StartRequested { get { return startRequested; } }

        public static IList<TradeRouteDefinition> GetRoutes()
        {
            lock (Sync) return new List<TradeRouteDefinition>(Routes).AsReadOnly();
        }

        public static void AddRoute(TradeRouteDefinition route)
        {
            if (route == null) throw new ArgumentNullException("route");
            lock (Sync) Routes.Add(route);
        }

        public static void RemoveRouteAt(int index)
        {
            lock (Sync) if (index >= 0 && index < Routes.Count) Routes.RemoveAt(index);
        }

        public static bool RequestStart(out string error)
        {
            error = null;
            IList<TradeRouteDefinition> routes = GetRoutes();
            if (routes.Count == 0) { error = "En az bir trade rotası ekleyin."; return false; }
            foreach (TradeRouteDefinition route in routes)
            {
                if (string.IsNullOrWhiteSpace(route.StartTown) || string.IsNullOrWhiteSpace(route.EndTown))
                { error = "Her rotada başlangıç ve bitiş şehri gerekli."; return false; }
                if (string.IsNullOrWhiteSpace(route.ScriptPath) || !File.Exists(route.ScriptPath))
                { error = "Rota scripti bulunamadı: " + route.ScriptPath; return false; }
                if (route.AmountMode == TradeAmountMode.Star)
                { error = "1-5 yıldız eşikleri henüz doğrulanmadı; Fill veya Quantity seçin."; return false; }
                if (route.AmountMode == TradeAmountMode.Quantity && route.Amount <= 5)
                { error = "Kesin miktar 5'ten büyük olmalı; 1-5 phBot'ta yıldız anlamına gelir."; return false; }
            }
            for (int i = 0; i + 1 < routes.Count; i++)
            {
                if (!string.Equals(routes[i].EndTown, routes[i + 1].StartTown, StringComparison.OrdinalIgnoreCase))
                { error = "Rota zinciri kopuk: " + routes[i].EndTown + " → " + routes[i + 1].StartTown; return false; }
            }
            if (RepeatCount > 1 && !UseReturnScroll
                && !string.Equals(routes[routes.Count - 1].EndTown, routes[0].StartTown, StringComparison.OrdinalIgnoreCase))
            { error = "Tekrarlanan döngü son şehirden ilk şehre dönmeli veya return scroll açık olmalı."; return false; }
            startRequested = true;
            Status = "Başlatılıyor";
            return true;
        }

        public static void CancelRequest()
        {
            startRequested = false;
            if (!running) Status = "Durduruldu";
        }

        internal static void Run(Bot bot)
        {
            if (!startRequested || bot == null) return;
            running = true;
            try
            {
                IList<TradeRouteDefinition> routes = GetRoutes();
                int repeats = Math.Max(1, RepeatCount);
                for (int loop = 0; loop < repeats && bot.isBotting && startRequested; loop++)
                {
                    for (int r = 0; r < routes.Count && bot.isBotting && startRequested; r++)
                    {
                        TradeRouteDefinition route = routes[r];
                        Status = string.Format("{0} → {1} ({2}/{3})", route.StartTown, route.EndTown, loop + 1, repeats);
                        Script script = BuildRouteScript(route);
                        script.Run(0);
                    }
                    if (UseReturnScroll && bot.isBotting && startRequested)
                    {
                        if (TerminateTransportAtEnd) TryTerminateEmptyTransport(bot);
                        Script returnScript = new Script();
                        returnScript.Add("use,returnscroll");
                        returnScript.Run(0);
                        WaitForReturnTeleport(bot);
                    }
                }
                if (TerminateTransportAtEnd && !UseReturnScroll && bot.isBotting && startRequested)
                    TryTerminateEmptyTransport(bot);
                Status = startRequested ? "Trade döngüsü tamamlandı" : "Durduruldu";
            }
            catch (Exception ex)
            {
                Status = "Hata: " + ex.Message;
                Window.Get?.Log("[Trade Loop] " + ex.Message, Theme.LogLevel.Warning);
            }
            finally
            {
                running = false;
                startRequested = false;
            }
        }

        private static Script BuildRouteScript(TradeRouteDefinition route)
        {
            Script script = new Script();
            script.Add("oldtrade,spawn" + (string.IsNullOrWhiteSpace(route.TransportName) ? "" : "," + route.TransportName));
            int amount = route.AmountMode == TradeAmountMode.Fill ? 0 : route.Amount;
            script.Add("oldtrade,buy," + amount + (string.IsNullOrWhiteSpace(route.ItemName) ? "" : "," + route.ItemName));
            if (MountMode == TradeMountMode.StayOff)
                script.Add("dismount");
            else
                script.Add("mount,transport");

            foreach (string line in File.ReadAllLines(route.ScriptPath))
            {
                string trimmed = (line ?? "").Trim();
                if (MountMode == TradeMountMode.Remount
                    && (trimmed.StartsWith("move", StringComparison.OrdinalIgnoreCase)
                        || trimmed.StartsWith("walk", StringComparison.OrdinalIgnoreCase)))
                    script.Add("mount,transport");
                script.Add(line);
            }
            script.Add("oldtrade,sell");
            return script;
        }

        private static void TryTerminateEmptyTransport(Bot bot)
        {
            SRCoService transport = InfoManager.MyPets.Find(pet => pet != null && pet.isTransport());
            if (transport != null && transport.Inventory != null)
            {
                for (int i = 0; i < transport.Inventory.Capacity; i++)
                {
                    SRItem item = transport.Inventory[i];
                    if (item != null && item.ID2 == 3 && item.ID3 == 8)
                    {
                        Window.Get?.Log("[Trade Loop] Taşıtta satılmamış specialty goods var; malları düşürmemek için terminate atlandı.", Theme.LogLevel.Warning);
                        return;
                    }
                }
            }
            Script finish = new Script();
            finish.Add("terminate,transport");
            finish.Run(0);
        }

        private static void WaitForReturnTeleport(Bot bot)
        {
            bool started = false;
            for (int i = 0; i < 300 && bot.isBotting && startRequested; i++)
            {
                if (InfoManager.inTeleport) started = true;
                if (started && !InfoManager.inTeleport && InfoManager.inGame)
                {
                    Thread.Sleep(1000);
                    return;
                }
                Thread.Sleep(100);
            }
            if (!started)
                Window.Get?.Log("[Trade Loop] Return teleport başlamadı; sonraki tekrar güvenlik için durduruldu.", Theme.LogLevel.Warning);
            if (!started) startRequested = false;
        }

        public static JObject ToJson()
        {
            JObject root = new JObject
            {
                ["RepeatCount"] = RepeatCount,
                ["UseReturnScroll"] = UseReturnScroll,
                ["TerminateTransportAtEnd"] = TerminateTransportAtEnd,
                ["SkipTownLoop"] = SkipTownLoop,
                ["MountMode"] = MountMode.ToString(),
                ["AttackSpawnedThieves"] = AttackSpawnedThieves,
                ["AttackRadius"] = AttackRadius
            };
            JArray routes = new JArray();
            foreach (TradeRouteDefinition route in GetRoutes())
            {
                routes.Add(new JObject
                {
                    ["StartTown"] = route.StartTown,
                    ["EndTown"] = route.EndTown,
                    ["ScriptPath"] = route.ScriptPath,
                    ["TransportName"] = route.TransportName,
                    ["ItemName"] = route.ItemName,
                    ["AmountMode"] = route.AmountMode.ToString(),
                    ["Amount"] = route.Amount
                });
            }
            root["Routes"] = routes;
            return root;
        }

        public static void FromJson(JObject root)
        {
            lock (Sync) Routes.Clear();
            if (root == null) return;
            RepeatCount = ReadInt(root, "RepeatCount", 1, 1, 999);
            UseReturnScroll = ReadBool(root, "UseReturnScroll", false);
            TerminateTransportAtEnd = ReadBool(root, "TerminateTransportAtEnd", true);
            SkipTownLoop = ReadBool(root, "SkipTownLoop", true);
            AttackSpawnedThieves = ReadBool(root, "AttackSpawnedThieves", false);
            AttackRadius = ReadInt(root, "AttackRadius", 30, 5, 100);
            TradeMountMode mount;
            if (Enum.TryParse((string)root["MountMode"], true, out mount)) MountMode = mount;
            JArray routes = root["Routes"] as JArray;
            if (routes == null) return;
            foreach (JObject item in routes)
            {
                TradeAmountMode mode;
                if (!Enum.TryParse((string)item["AmountMode"], true, out mode)) mode = TradeAmountMode.Fill;
                AddRoute(new TradeRouteDefinition
                {
                    StartTown = (string)item["StartTown"] ?? "",
                    EndTown = (string)item["EndTown"] ?? "",
                    ScriptPath = (string)item["ScriptPath"] ?? "",
                    TransportName = (string)item["TransportName"] ?? "",
                    ItemName = (string)item["ItemName"] ?? "",
                    AmountMode = mode,
                    Amount = ReadInt(item, "Amount", 0, 0, int.MaxValue)
                });
            }
        }

        private static bool ReadBool(JObject root, string key, bool fallback)
        { return root[key] == null ? fallback : (bool)root[key]; }
        private static int ReadInt(JObject root, string key, int fallback, int min, int max)
        {
            int value = root[key] == null ? fallback : (int)root[key];
            return Math.Max(min, Math.Min(max, value));
        }
    }
}
