using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using xBot.Game;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;
using xBot.Game.Navigation;

namespace xBot.App
{
    public partial class Bot
    {
        public bool isBotting { get { return tBotting != null; } }
        /// <summary>
        /// Thread controlling all botting actions.
        /// </summary>
        Thread tBotting;
        Script currentScript;
        private volatile bool m_stopBottingRequested;
        private System.Threading.CancellationTokenSource m_botCts;
        // Cached mob list for training area to avoid FindAll allocation each tick
        private List<SRMob> m_cachedMobsInRange = new List<SRMob>();
        private bool m_mobsCacheDirty = true;
        private SRCoord m_lastTrainingPosition;
        private int m_lastTrainingRadius;
        // Town scriptleri loop şeklinde bitince (son nokta ilk noktaya yakın) aynı
        // tick'te tekrar town algılanıp sonsuz döngüye girilmemesi için guard.
        // SADECE son döngünün bittiği yerde (+80m) ve 90sn içinde duruluyorsa atlanır;
        // bot yeniden başlatılsa bile şehirdeyse döngü HER ZAMAN yapılır.
        private DateTime m_lastTownLoopFinished = DateTime.MinValue;
        private SRCoord m_lastTownLoopEndPos = null;
        private DateTime m_lastPartyAutoTick = DateTime.MinValue;
        private DateTime m_lastPartyMatchReform = DateTime.MinValue;
        private Thread tPetLooting = null;
        private volatile bool m_stopPetLootingRequested = false;
        private readonly Dictionary<uint, DateTime> _petCommandedDrops = new Dictionary<uint, DateTime>();
        private readonly Dictionary<uint, DateTime> _unreachableMobs = new Dictionary<uint, DateTime>();

        /// <summary>
        /// Asenkron pet toplama iş parçacığını başlatır (Bot startlı olmasa bile çalışır).
        /// </summary>
        public void StartPetLooting()
        {
            try
            {
                m_stopPetLootingRequested = false;
                if (tPetLooting != null && tPetLooting.IsAlive)
                    return;

                tPetLooting = new Thread(this.ThreadPetLooting)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Normal
                };
                tPetLooting.Start();
            }
            catch { }
        }

        /// <summary>
        /// Pet toplama iş parçacığını durdurur (Client bağlantısı kesildiğinde çağrılır).
        /// </summary>
        public void StopPetLooting()
        {
            try
            {
                m_stopPetLootingRequested = true;
                Thread tp = tPetLooting;
                tPetLooting = null;
                try { if (tp != null && tp.IsAlive && tp != Thread.CurrentThread) tp.Join(500); } catch { }
                lock (_petCommandedDrops) { _petCommandedDrops.Clear(); }
            }
            catch { }
        }
        /// <summary>
        /// Kesilebilir bekleme: Stop() çağrılırsa erken döner (uzun Sleep'lerin bloklamasını önler).
        /// </summary>
        private bool SleepInterruptible(int ms)
        {
            if (ms <= 0) return !m_stopBottingRequested;
            int waited = 0;
            while (waited < ms)
            {
                if (m_stopBottingRequested) return false;
                var cts = m_botCts;
                if (cts != null && cts.IsCancellationRequested) return false;
                int slice = System.Math.Min(50, ms - waited);
                try
                {
                    if (cts != null)
                        cts.Token.WaitHandle.WaitOne(slice);
                    else
                        Thread.Sleep(slice);
                }
                catch { Thread.Sleep(slice); }
                waited += slice;
            }
            return !m_stopBottingRequested;
        }

        #region (Handle everything about botting)
        /// <summary>
        /// Start botting.
        /// </summary>
        public void Start()
        {
            if (InfoManager.inGame && !isBotting)
            {
                m_stopBottingRequested = false;
                try { m_botCts?.Dispose(); } catch { }
                m_botCts = new System.Threading.CancellationTokenSource();
                tBotting = new Thread(this.ThreadBotting);
                tBotting.IsBackground = true;
                tBotting.Priority = ThreadPriority.Normal;
                tBotting.Start();

                StartPetLooting();
                // ...
                Window w = Window.Get;
                w.Log("Starting bot");
                // Update GUI
                WinAPI.InvokeIfRequired(w.btnBotStart, () => {
                    w.btnBotStart.ForeColor = Color.Lime;
                    w.ToolTips.SetToolTip(w.btnBotStart, "Stop Bot");
                });
            }
        }
        /// <summary>
        /// Stop botting.
        /// </summary>
        public void Stop()
        {
            if (isBotting)
            {
                m_stopBottingRequested = true;
                QuestAutomationManager.Suspend();
                try { m_botCts?.Cancel(); } catch { }
                Thread t = tBotting;
                tBotting = null;
                // Bloklanan döngüye en fazla 2sn süre tanı, UI'yi kilitleme
                try { if (t != null && t.IsAlive && t != Thread.CurrentThread) t.Join(2000); } catch { }
                // ...
                Window w = Window.Get;
                w.LogProcess("Bot stopped");

                w.Log("Stopping bot");
                // Update GUI
                WinAPI.InvokeIfRequired(w.btnBotStart, () => {
                    w.btnBotStart.ForeColor = Color.Red;
                    w.ToolTips.SetToolTip(w.btnBotStart, "Start Bot");
                });
                // ...

            }
        }
        private void ThreadBotting()
        {
			if (TradeLoopManager.StartRequested)
			{
				if (!TradeLoopManager.SkipTownLoop)
				{
					try
					{
						SRCoord tradeStart = InfoManager.Character.GetRealtimePosition();
						Script preparation = Script.GetTownScriptForRegion(tradeStart.Region)
							?? Script.GetNearestTownScript(tradeStart, 150);
						if (preparation != null)
						{
							Window.Get.Log("Trade Loop: normal town script çalıştırılıyor [" + preparation.FileName + "]...");
							preparation.Run(0);
						}
						else Window.Get.Log("Trade Loop: normal town script bulunamadı; trade rotasına geçiliyor.", Theme.LogLevel.Warning);
					}
					catch (Exception ex) { Window.Get.Log("Trade Loop town hazırlığı: " + ex.Message, Theme.LogLevel.Warning); }
				}
				TradeLoopManager.Run(this);
				Stop();
				return;
			}
            // 1.1 TOWN ?
            // 1.1.1 Do TOWN
            // 1.1.2 Wait at starting script

            // 1.2 TRAINING AREA ?
            // 1.2.1 Kill Mobs

            // 1.3 OUTSIDE TOWN AND OUTSIDE TRAINING AREA ? 
            // 1.3.1 Find the walking nearest point
            // 1.3.1.1 Follow it
            // 1.3.1.2 Go to 1.2
            // 1.3.2 Use return scroll

            Window w = Window.Get;
            while (!m_stopBottingRequested && tBotting != null)
            {
                // Quest packet thread only queues completion work. Execute it here
                // so movement/scripts keep a single bot-thread owner.
                QuestAutomationManager.ObserveActiveQuests();
                if (QuestAutomationManager.TryExecutePending(this))
                {
                    SleepInterruptible(500);
                    continue;
                }
                // Checking where am I ?
                w.LogProcess("Checking current location...");
                SRCoord myPosition = InfoManager.Character.GetRealtimePosition();
                currentScript = null;
                // Döngü daha yeni bitti VE hâlâ bittiği yerde duruluyorsa tekrar girme
                // (yoksa loop-script kendini sonsuz tetikler). Onun dışında şehirde
                // yakalanırsa HER ZAMAN town döngüsü yapılır.
                bool townJustFinished = false;
                try
                {
                    townJustFinished = (DateTime.Now - m_lastTownLoopFinished).TotalSeconds < 90
                        && m_lastTownLoopEndPos != null && myPosition != null
                        && myPosition.DistanceTo(m_lastTownLoopEndPos) < 80.0;
                }
                catch { }
                if (!townJustFinished)
                {
                    // Town cycling kapalıysa şehir scripti aranmaz — log spam'i
                    // de olmaz, direkt kasılma alanına gidilir. İki anahtardan
                    // HERHANGİ biri açıksa döngü vardır (senkron edge-case dahil).
                    bool townCycling = true;
                    try { townCycling = ReturnToAreaPolicy.TownCycling; } catch { }
                    try { if (w.Town_cbxEnableTownLoop != null && w.Town_cbxEnableTownLoop.Checked) townCycling = true; } catch { }
                    if (ReturnToAreaPolicy.SkipTownScript)
                    {
                        townCycling = false;
                    }
                    if (!townCycling)
                    {
                        currentScript = null;
                    }
                    else try
                    {
                        // 1) Bölge eşleşmesi: şehirde doğmuşsa noktalara uzak olsa da yakala
                        //    (dosya adı = region, örn. 25000.rbs).
                        currentScript = Script.GetTownScriptForRegion(myPosition.Region);
                        if (currentScript != null)
                            w.Log($"Town Script: bölge eşleşmesi [{myPosition.Region}] -> [{currentScript.FileName}]");
                        else
                        {
                            // 2) Yakınlık eşleşmesi (şehir büyük, 150m tolerans).
                            currentScript = Script.GetNearestTownScript(myPosition, 150);
                            if (currentScript == null)
                            {
                                double nearest = Script.GetNearestTownMoveDistance(myPosition);
                                string nearestTxt = nearest < 0 ? "yok (script klasörü boş?)" : ((int)nearest + "m");
                                w.Log($"Town Script: eşleşme yok (region={myPosition.Region}, en yakın nokta={nearestTxt}). Town/ klasöründe {myPosition.Region}.rbs var mı?");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        w.Log("Town Script algılama hatası: " + ex.Message + " — training rotasına geçiliyor.");
                        currentScript = null;
                    }
                }
                else
                {
                    w.Log("Town Script: döngü yeni bitti, aynı noktadayız — training rotasına çıkılıyor.");
                }
                if (currentScript != null)
                {
                    // I'm near town loop script
                    TownLoop(currentScript);
                }
                else
                {
                    // Checking training area
                    SRCoord trainingPosition = w.TrainingArea_GetPosition();
                    if (trainingPosition == null)
                    {
                        w.Log("Training area it's not activated");
                        Stop();
                        return;
                    }
                    else
                    {
                        // Check if I'm inside training area
                        int trainingRadius = w.TrainingArea_GetRadius();
                        double distanceToArea = myPosition.DistanceTo(trainingPosition);
                        w.LogProcess($"Pos: ({(int)myPosition.PosX},{(int)myPosition.PosY}) -> Slot: ({(int)trainingPosition.PosX},{(int)trainingPosition.PosY}) [{(int)distanceToArea}m, r={trainingRadius}m]");
                        if (distanceToArea <= trainingRadius)
                        {
                            AttackLoop();
                        }
                        else
                        {
                            // Training Options: Reverse scroll kontrolü (Şehirden sonra veya ölüm sonrası alana anında ışınlan)
                            if ((TrainingOptionsPolicy.UseReverseAfterTown || TrainingOptionsPolicy.UseReverseOnDeath) && distanceToArea > 80.0)
                            {
                                byte targetPoint = TrainingOptionsPolicy.UseReverseOnDeath ? (byte)1 : (byte)2;
                                if (ProtectionManager.TryUseReverseReturnScroll(targetPoint))
                                {
                                    w.LogProcess("Reverse scroll kullanıldı, alana ışınlanma bekleniyor...");
                                    SleepInterruptible(3000);
                                    continue;
                                }
                            }

                            // 10. Son return kullanılan yere dönmek için şehirdeki Teleport NPC'lerini kullan
                            if (ReturnToAreaPolicy.UseTownTeleportForLastRecall && ReturnToAreaPolicy.LastRecallPosition != null && TownManager.Get.IsNearTown(myPosition))
                            {
                                var link = TeleportManager.Get.FindBestLink(myPosition, ReturnToAreaPolicy.LastRecallPosition);
                                if (link != null && CollisionPolicy.IsLinkAllowed(link, (int)(InfoManager.Character?.Level ?? 0)))
                                {
                                    w.LogProcess($"Alana Dönüş: Son return konumuna gitmek için teleport kullanılıyor [{link.SourceName} -> {link.DestinationName}]...");
                                    if (ExecuteTeleportTransition(link))
                                    {
                                        ReturnToAreaPolicy.LastRecallPosition = null;
                                        continue;
                                    }
                                }
                            }

                            // 11. Öldüğün yere dönmek için şehirdeki Teleport NPC'lerini kullan
                            if (ReturnToAreaPolicy.UseTownTeleportForLastDeath && ReturnToAreaPolicy.LastDeathPosition != null && TownManager.Get.IsNearTown(myPosition))
                            {
                                var link = TeleportManager.Get.FindBestLink(myPosition, ReturnToAreaPolicy.LastDeathPosition);
                                if (link != null && CollisionPolicy.IsLinkAllowed(link, (int)(InfoManager.Character?.Level ?? 0)))
                                {
                                    w.LogProcess($"Alana Dönüş: Öldüğün yere gitmek için teleport kullanılıyor [{link.SourceName} -> {link.DestinationName}]...");
                                    if (ExecuteTeleportTransition(link))
                                    {
                                        ReturnToAreaPolicy.LastDeathPosition = null;
                                        continue;
                                    }
                                }
                            }

                            // Alana Dönüş hazırlığı: buff tazele + hız eşyası + binek
                            // (45sn geçiş korumalı, spam yapmaz).
                            PrepareReturnTrip();
                            // 1. First try: Auto NavMesh / Multi-hop Ferry & Teleport route
                            if (NavigationManager.Get.IsAvailable)
                            {
                                w.LogProcess("NavMesh: Calculating route to training area...");
                                NavigationRoute navRoute = NavigationManager.Get.FindCompoundRoute(myPosition, trainingPosition);
                                if (navRoute != null && !navRoute.IsEmpty)
                                {
                                    w.Log($"NavMesh: Executing route with {navRoute.Segments.Count} segment(s) ({navRoute.TotalWaypointsCount} waypoints)...");
                                    bool routeAborted = false;

                                    for (int s = 0; s < navRoute.Segments.Count && isBotting; s++)
                                    {
                                        var segment = navRoute.Segments[s];
                                        if (segment.Type == RouteSegmentType.Walk)
                                        {
                                            for (int p = 0; p < segment.Waypoints.Count && isBotting; p++)
                                            {
                                                if (myPosition.DistanceTo(trainingPosition) <= trainingRadius)
                                                    break; // Arrived at training area

                                                if (ReturnToAreaPolicy.RemountInCaves && (myPosition.inDungeon() || SRCoord.inDungeon(myPosition.Region)))
                                                {
                                                    var chr = InfoManager.Character;
                                                    if (chr != null && !chr.isRiding)
                                                    {
                                                        if (ReturnToAreaPolicy.RideFellowPet)
                                                            TryRideFellowPet();
                                                        else if (ReturnToAreaPolicy.UseMount && !TrainingOptionsPolicy.DoNotSpawnMount)
                                                            TrySummonMount();
                                                    }
                                                }

                                                w.LogProcess($"NavMesh walk [{p + 1}/{segment.Waypoints.Count}]");
                                                WaitMovement(segment.Waypoints[p], 12);
                                                myPosition = InfoManager.Character.GetRealtimePosition();
                                            }
                                        }
                                        else if (segment.Type == RouteSegmentType.Teleport)
                                        {
                                            if (!ExecuteTeleportTransition(segment.TeleportLink))
                                            {
                                                w.LogProcess("Teleport transition failed. Retrying route...", Window.ProcessState.Warning);
                                                routeAborted = true;
                                                break;
                                            }
                                            myPosition = InfoManager.Character.GetRealtimePosition();
                                        }
                                    }

                                    if (!routeAborted)
                                        continue;
                                }
                                else
                                {
                                    w.LogProcess("NavMesh: No route found (FindCompoundRoute returned null/empty).", Window.ProcessState.Warning);
                                }
                            }
                            else
                            {
                                w.LogProcess("NavMesh: Not available (navdata not indexed).", Window.ProcessState.Warning);
                            }

                            // 2. Fallback: User movement script
                            string scriptPath = w.TrainingArea_GetScript();
                            if (!string.IsNullOrEmpty(scriptPath) && File.Exists(scriptPath))
                            {
                                w.LogProcess("Fallback: Using user movement script.");
                                w.LogProcess("Loading training script...");
                                currentScript = new Script(scriptPath);
                                int nearIndex = currentScript.GetNearMovement(myPosition, 80);
                                if (ReturnToAreaPolicy.ReverseRoute)
                                {
                                    if (nearIndex != -1)
                                    {
                                        w.Log("Reverse route: script sondan başa, adım " + (nearIndex + 1) + "ten geriye...");
                                        currentScript.RunReversed(nearIndex);
                                    }
                                    else
                                    {
                                        w.Log("Reverse route: script sondan başlıyor...");
                                        currentScript.RunReversed();
                                    }
                                }
                                else if (nearIndex != -1)
                                {
                                    w.Log("Resuming script from step " + (nearIndex + 1));
                                    currentScript.Run(nearIndex);
                                }
                                else
                                {
                                    w.Log("Starting script from beginning...");
                                    currentScript.Run(0);
                                }

// Script bittikten sonra tekrar kontrol
                                Thread.Sleep(1000);
                            }
                            else
                            {
                                w.LogProcess("Fallback: No movement script configured, using direct walk.", Window.ProcessState.Warning);
                                // 3. Fallback: Direct walk to training position with collision avoidance
                            // Calculate attempts based on distance (more attempts for farther distances)
                            double walkDistance = myPosition.DistanceTo(trainingPosition);
                            // UZUN MESAFE GUARD: NavMesh yoksa/rota yoksa 120m+ düz yürüyüş dağ/duvar
                            // arasından geçemez (örn. Spider Forest -> Ancient Remains ~800m, arada
                            // Karakoram dağları var). Kör yürüyüş yerinde sağa-sola sekip takılır.
                            const double MaxDirectWalkDistance = 120.0;
                            if (walkDistance > MaxDirectWalkDistance)
                            {
                                w.Log($"Training area {walkDistance:F0}m uzakta. NavMesh rotası ve walk script yoksa düz yürüyüşle gidilemez (dağ/duvar takılır).");
                                w.Log("Çözüm 1: 'navdata' klasörünü xBot.exe yanına kopyalayın ve botu yeniden başlatın (NavMesh rota bulur).");
                                w.Log("Çözüm 2: Training sekmesine walk script (.txt) ekleyin (örn. Karakoram Entrance geçidi üzerinden).");
                                w.Log("Çözüm 3: Karakteri training alanına yakın bir yere (120m içine) götürüp botu orada başlatın.");
                                w.Log("Cannot reach training area (too far for direct walk). Stopped.");
                                Stop();
                                return;
                            }
                            int maxAttempts = Math.Max(15, (int)(walkDistance / 5.0) + 5); // ~1 attempt per 5m + buffer
                            if (maxAttempts > 40) maxAttempts = 40; // Duvara 90sn vurmayı engelle
                            w.Log($"Walking towards training area ({walkDistance:F0}m) with collision avoidance (max {maxAttempts} attempts)...");
                            if (!WaitMovement(trainingPosition, maxAttempts))
                            {
                                w.Log("Cannot reach training area. Stopped.");
                                Stop();
                                return;
                            }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// FindLiveNpc bazen sadece "koordinata 35m yakın" diye alakasız NPC döndürür.
        /// Yanlış NPC'ye Repair/OpenStorage paketi private server'da bağlantıyı kestirir,
        /// o yüzden paket öncesi ID veya isim doğrulaması şart.
        /// </summary>
        private bool IsVerifiedTownNpc(TownServiceInfo service, SREntity npc, Window w, string action)
        {
            if (service == null || npc == null)
                return false;
            bool idOk = service.NpcId != 0 && npc.ID == service.NpcId;
            bool nameOk = !string.IsNullOrEmpty(npc.Name) && !string.IsNullOrEmpty(service.NpcName)
                && npc.Name.IndexOf(service.NpcName, StringComparison.OrdinalIgnoreCase) >= 0;
            if (idOk || nameOk)
                return true;
            w.Log($"{action}: [{npc.Name}] beklenen [{service.NpcName}] değil (ID {npc.ID}/{service.NpcId}) — kick riski yüzünden atlanıyor.");
            return false;
        }
        private void TownLoop(Script town)
        {
            Window w = Window.Get;
            if (ReturnToAreaPolicy.SkipTownScript)
            {
                w.LogProcess("Town Loop: 'Şehir döngüsünü atla' aktif, şehir döngüsü atlanıyor.");
                return;
            }
            // Town cycling iki yerden işaretlenebilir (Town sekmesi + Alana Dönüş
            // kartı). İkisi senkron ama desenkron edge-case'de de: HERHANGİ biri
            // açıksa şehir döngüsü yapılır — bot şehirde başlayınca çalışır.
            bool townOn = false;
            try { townOn = ReturnToAreaPolicy.TownCycling; } catch { }
            try { if (w.Town_cbxEnableTownLoop != null && w.Town_cbxEnableTownLoop.Checked) townOn = true; } catch { }
            if (!townOn)
            {
                w.LogProcess("Town Loop is disabled in Town settings.");
                return;
            }

            w.Log("Town Loop: Initiating town logistic sequence...");

            SRCoord myPosition = InfoManager.Character.GetRealtimePosition();
            if (myPosition == null)
                return;

            // NOT: Şehir işlerinin tamamını town scripti yapar (move/store/buy/repair).
            // UI tabanlı servis lojistiği (TownManager koordinatları) bu server'daki
            // özel NPC'lerle uyuşmadığı için KALDIRILDI — yanlış NPC'ye paket atıp
            // kick yediriyordu. Script adımları (Script.cs) kendi doğrulamasıyla çalışır.
            if (town == null)
            {
                // Pot bitince dönüş gibi scriptsiz çağrılarda konuma göre çöz.
                try
                {
                    town = Script.GetTownScriptForRegion(myPosition.Region)
                        ?? Script.GetNearestTownScript(myPosition, 150);
                    if (town != null)
                        w.Log($"Town Script: çözüldü [{town.FileName}]");
                }
                catch { town = null; }
            }
            if (town != null)
            {
                w.Log("Running town script [" + town.FileName + "] (script-only mode)...");
                town.Run(0);
            }
            else
            {
                w.Log("Town Loop: town script bulunamadı, atlanıyor.", Theme.LogLevel.Warning);
            }

			if (!InfoManager.inGame || !isBotting || m_stopBottingRequested
				|| Proxy == null || !Proxy.isRunning)
				return;

			// Script tüm dükkânları kapattıktan sonra bağlamsız satış paketi
			// gönderilmez. Cephane alımı kendi doğrulanmış dükkân oturumunu açar.
			ExecuteAutoBuyAmmo();

			if (!InfoManager.inGame || !isBotting || m_stopBottingRequested
				|| Proxy == null || !Proxy.isRunning)
				return;

            w.Log("Town Loop: Logistics routine completed. Returning to training area...");
            try
            {
                SRCoord tp = w.TrainingArea_GetPosition();
                if (tp != null)
                {
                    SRCoord mp = InfoManager.Character.GetRealtimePosition();
                    w.Log($"Town Loop: Route to training area ({(int)tp.PosX},{(int)tp.PosY}) from ({(int)mp.PosX},{(int)mp.PosY}, {mp.DistanceTo(tp):F0}m)...");
                    // ReturnNavMesh onayı: şehir çıkışında eğitim alanına NavMesh
                    // rotasıyla (teleport/ferry dahil) dön, kapalıysa ana döngünün
                    // düz yürüyüşüne bırak.
                    bool useNavMesh = false;
                    try { w.Town_cbxReturnNavMesh.InvokeIfRequired(() => { useNavMesh = w.Town_cbxReturnNavMesh.Checked; }); } catch { }
                    if (useNavMesh && NavigationManager.Get.IsAvailable && mp != null)
                    {
                        try
                        {
                            NavigationRoute navRoute = NavigationManager.Get.FindCompoundRoute(mp, tp);
                            if (navRoute != null && !navRoute.IsEmpty)
                            {
                                w.Log($"Town Loop: NavMesh dönüş rotası ({navRoute.Segments.Count} segment) uygulanıyor...");
                                foreach (var segment in navRoute.Segments)
                                {
                                    if (!isBotting || m_stopBottingRequested) break;
                                    if (segment.Type == RouteSegmentType.Walk)
                                    {
                                        foreach (var wp in segment.Waypoints)
                                        {
                                            if (!isBotting || m_stopBottingRequested) break;
                                            SRCoord cur = InfoManager.Character.GetRealtimePosition();
                                            if (cur != null && cur.DistanceTo(tp) <= w.TrainingArea_GetRadius())
                                                break;
                                            WaitMovement(wp, 12);
                                        }
                                    }
                                    else if (segment.Type == RouteSegmentType.Teleport)
                                    {
                                        if (!ExecuteTeleportTransition(segment.TeleportLink))
                                            break;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            w.Log("Town Loop: NavMesh dönüş hatası: " + ex.Message + " — normal rotaya geçiliyor.");
                        }
                    }
                }
            }
            catch { }
            m_lastTownLoopFinished = DateTime.Now;
            try { m_lastTownLoopEndPos = InfoManager.Character.GetRealtimePosition(); } catch { m_lastTownLoopEndPos = null; }
            SleepInterruptible(1500);
        }
        private void AttackLoop()
        {
            Window w = Window.Get;

            SRCoord myPosition, trainingPosition;
            int trainingRadius;

            bool doMovement = false;
            while (!m_stopBottingRequested && isBotting)
            {
                // Check attacking params
                trainingPosition = w.TrainingArea_GetPosition();
                if (trainingPosition == null)
                {
                    w.Log("Training area it's not activated");
                    Stop();
                    return;
                }
                myPosition = InfoManager.Character.GetRealtimePosition();
                trainingRadius = w.TrainingArea_GetRadius();

                // Check movement
                if (doMovement)
                {
                    // Avoid getting far away from training area
                    if (myPosition.DistanceTo(trainingPosition) - trainingRadius < 50)
                    {
                        // Default time walking
                        int timeTraveling = 3000;
                        // Try to make a training movement
                        if (w.Training_cbxWalkToCenter.Checked)
                        {
                            if (!myPosition.Equals(trainingPosition, 3.0))
                            {
                                if (CollisionPolicy.EnableCollisionInTrainingArea && CollisionPolicy.NavigateAroundObstacles)
                                {
                                    w.LogProcess("Merkeze yürünüyor (engel algılama devrede)...");
                                    ApproachTargetWithCollision(trainingPosition, 3.0, null);
                                }
                                else
                                {
                                    timeTraveling = myPosition.TimeTo(trainingPosition, InfoManager.Character.GetMovementSpeed());
                                    MoveTo(trainingPosition);
                                    w.LogProcess("Walking to center (" + timeTraveling + "ms)...");
                                    WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged, InfoManager.MonitorBuffRemoved }, Math.Min(timeTraveling, 3000));
                                }
                            }
                        }
                        else
                        {
                            // Kasılma alanında dolaşma kontrolü
                            if (TrainingOptionsPolicy.DontWalkAroundTrainingArea)
                            {
                                if (myPosition.DistanceTo(trainingPosition) > trainingRadius)
                                {
                                    MoveTo(trainingPosition);
                                    w.LogProcess("Kasılma alanında dolaşma: Sınıra ulaşıldı, merkeze dönülüyor...");
                                    WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged }, 1500);
                                }
                                else
                                {
                                    WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged }, 1000);
                                }
                            }
                            // Avoid blind random walk across terrain / sectors. If drifted, return to center.
                            else if (myPosition.DistanceTo(trainingPosition) > 15.0)
                            {
                                if (CollisionPolicy.EnableCollisionInTrainingArea && CollisionPolicy.NavigateAroundObstacles)
                                {
                                    w.LogProcess("Merkeze dönülüyor (engel algılama devrede)...");
                                    ApproachTargetWithCollision(trainingPosition, 5.0, null);
                                }
                                else
                                {
                                    timeTraveling = myPosition.TimeTo(trainingPosition, InfoManager.Character.GetMovementSpeed());
                                    MoveTo(trainingPosition);
                                    w.LogProcess("Returning towards center (" + timeTraveling + "ms)...");
                                    WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged, InfoManager.MonitorBuffRemoved }, Math.Min(timeTraveling, 3000));
                                }
                            }
                            else
                            {
                                // Already in center, wait safely for mobs to spawn
                                WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged }, 1000);
                            }
                        }
                        doMovement = false;
                    }
                    else
                    {
                        // Too far away from training area
                        w.Log("Attacking stopped, too far away from training area");
                        w.LogProcess("Far away from training area");
                        return;
                    }
                }

                // Check buffs
                BuffLoop();

                // Check party support (Heal, Ress, Cure)
                PartySupportManager.RunTick();

                // Party otomasyonu botting sırasında da sürsün (önceden sadece
                // login'de 1 kez çalışıyordu): davet + lider yoksa ayrılma.
                CheckPartyAutoTick();

                // Check Target Assist hotkey cycle (timer tek kaynak; çift tetik önlemek için buradan çağrılmaz)
                // TargetAssistManager.RunTick();

                if (trainingRadius > 0)
                {
                    // No attack mode check (support / lure / buffer only)
                    if (SkillManager.NoAttackMode)
                    {
                        w.LogProcess("Support mode: Buffing & Following only");
                        Thread.Sleep(400);
                        continue;
                    }

                    // Ensure Imbue is active before combat
                    SkillManager.EnsureImbueActive();

                    // Lure döngüsü kontrolü
                    if (CheckLureTick(w, trainingPosition, trainingRadius))
                    {
                        continue;
                    }

                    // Attacking
                    // Update cached mob list if training area changed or cache is dirty
                    if (m_mobsCacheDirty || !trainingPosition.Equals(m_lastTrainingPosition) || trainingRadius != m_lastTrainingRadius)
                    {
                        m_cachedMobsInRange.Clear();
                        foreach (var m in InfoManager.Mobs.Snapshot())
                        {
                            if (m != null && trainingPosition.DistanceTo(m.GetRealtimePosition()) <= trainingRadius)
                            {
                                m_cachedMobsInRange.Add(m);
                            }
                        }
                        m_lastTrainingPosition = trainingPosition;
                        m_lastTrainingRadius = trainingRadius;
                        m_mobsCacheDirty = false;
                    }
                    List<SRMob> mobs = m_cachedMobsInRange;

                    // Devil Spirit: Check activation conditions
                    SkillManager.CheckDevilSpirit(mobs);

                    // Combat AI: Check Berserker activation (canonical state: CombatAIEngine.IsBerserkEnabled)
                    if (CombatAIEngine.IsBerserkEnabled)
                    {
                        CheckBerserker(mobs);
                    }

                    // Combat AI: Emergency Panic Escape (low HP & no pots)
                    if (w.Combat_cbxPanicEscape == null || w.Combat_cbxPanicEscape.Checked)
                    {
                        if (CheckPanicEscape())
                            return;
                    }

                    // Combat AI: Check if we need to return to town (no pots / full bag / durability)
                    if (w.Town_cbxEnableTownLoop == null || w.Town_cbxEnableTownLoop.Checked)
                    {
                        if (ProtectionManager.CheckTownReturnTriggers())
                        {
                            TownLoop(null);
                            return;
                        }
                    }

                    // Pick Filter: "Pick items first" — yerde toplanacak eşya varsa
                    // yeni mob seçmeden önce topla.
                    if (ItemFilterManager.Pick.PickItemsFirst && HasLootableDrops())
                    {
                        w.LogProcess("Pick items first: looting before next attack...");
                        LootDrops(trainingPosition, trainingRadius);
                        continue;
                    }

                    // Training Options: Düşük MP'de çekil
                    if (TrainingOptionsPolicy.WithdrawOnLowMp && InfoManager.Character != null)
                    {
                        int mpPct = InfoManager.Character.GetMPPercent();
                        if (mpPct <= TrainingOptionsPolicy.LowMpPercent)
                        {
                            w.LogProcess($"Düşük MP ({mpPct}% <= {TrainingOptionsPolicy.LowMpPercent}%): Güvenli merkeze çekiliniyor...");
                            if (myPosition.DistanceTo(trainingPosition) > 5.0)
                                MoveTo(trainingPosition);
                            Thread.Sleep(1000);
                            continue;
                        }
                    }

                    // Training Options: Kasılma alanında periyodik kontroller
                    CheckPandoraAndMonsterScrolls(mobs);
                    CheckFlowerSummon();
                    CheckTreasureBoxes();
                    CheckAutoEquipBetterItems();

                    // Pet Motoru: Oto çağırma, canlandırma, koruma, stuck auto-recall
                    if (CheckPetEngineTick(trainingPosition, trainingRadius))
                        return;

                    if (TrainingOptionsPolicy.UseSpeedDrugs && !TrainingOptionsPolicy.SpeedDrugsOnlyInScript)
                    {
                        if (!HasSpeedBuffActive())
                            TryUseSpeedDrug();
                    }

                    SRMob mob = GetMobFiltered(mobs, trainingPosition, trainingRadius);
                    if (mob == null)
                    {
                        // No mob to attack
                        w.LogProcess("No mobs around to attack");
                        LootDrops(trainingPosition, trainingRadius);
                        WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged }, 1000);
                        if (TrainingOptionsPolicy.DontWalkAroundTrainingArea)
                        {
                            if (myPosition.DistanceTo(trainingPosition) > trainingRadius + 5.0)
                                doMovement = true;
                            else
                                doMovement = false;
                        }
                        else if (myPosition.DistanceTo(trainingPosition) > 20.0)
                            doMovement = true;
                        else
                            doMovement = false;
                        continue;
                    }
                    else
                    {
                        // Combat AI: Ranged Kiting check (Disabled by default, only executes when explicitly checked)
                        if (w.Combat_cbxKiting != null && w.Combat_cbxKiting.Checked)
                        {
                            ExecuteKiting(mob, myPosition);
                        }

                        // Check distance to target mob before attacking
                        SRTypes.Weapon myWeapon = GetMyWeaponType();
                        SRSkill[] initialSkillshots = w.Skills_GetSkillShots(mob.MobType);
                        double maxAttackRange = GetEffectiveAttackRange(initialSkillshots, myWeapon);
                        SRCoord mobPosition = mob.GetRealtimePosition();
                        myPosition = InfoManager.Character.GetRealtimePosition();
                        double distanceToMob = myPosition.DistanceTo(mobPosition);

                        if (distanceToMob > maxAttackRange)
                        {
                            w.LogProcess($"Approaching {mob.Name} ({distanceToMob:F1}m)...");
                            bool reached = ApproachTargetWithCollision(mobPosition, maxAttackRange, mob);
                            if (!reached)
                            {
                                continue;
                            }
                        }

                        if (!EnsureCombatTargetSelected(mob))
                            continue;

                        int currentSkillIndex = 0;
                        int noSkillAttempts = 0;
                        var obstacleRecovery = new CombatObstacleRecovery();

                            while (isBotting && IsLiveCombatTarget(mob))
                            {
                                if (SkillManager.NoAttackMode)
                                    break;

                                // Training Options: Training alanında kal
                                myPosition = InfoManager.Character.GetRealtimePosition();
                                if (TrainingOptionsPolicy.StayInTrainingArea && trainingRadius > 0 && myPosition.DistanceTo(trainingPosition) > trainingRadius)
                                {
                                    w.LogProcess("Training alanında kal: Karakter sınır dışına taştı, hedeften vazgeçilip alana dönülüyor...");
                                    MoveTo(trainingPosition);
                                    break;
                                }

                                // Training Options: Düşük MP'de savaşı kes ve çekil
                                if (TrainingOptionsPolicy.WithdrawOnLowMp && InfoManager.Character != null)
                                {
                                    int mpPct = InfoManager.Character.GetMPPercent();
                                    if (mpPct <= TrainingOptionsPolicy.LowMpPercent)
                                    {
                                        w.LogProcess($"Düşük MP ({mpPct}% <= {TrainingOptionsPolicy.LowMpPercent}%): Savaş kesilip geri çekiliniyor...");
                                        MoveTo(trainingPosition);
                                        break;
                                    }
                                }

                                // NOT: Imbue/Devil zaten mob başında (dış döngüde) kontrol ediliyor;
                                // her vuruşta tekrar taramak iç döngüde mikro-takılma yapıyordu.
                                // Distance check to target
                                myWeapon = GetMyWeaponType();
                                SRSkill[] skillshots = w.Skills_GetSkillShots(mob.MobType);
                                maxAttackRange = GetEffectiveAttackRange(skillshots, myWeapon);
                                mobPosition = mob.GetRealtimePosition();
                                myPosition = InfoManager.Character.GetRealtimePosition();
                                distanceToMob = myPosition.DistanceTo(mobPosition);

                                if (distanceToMob > maxAttackRange)
                                {
                                    bool reached = ApproachTargetWithCollision(mobPosition, maxAttackRange, mob);
                                    if (!reached)
                                        break;
                                    continue;
                                }

                                SRSkill skillToCast = null;
                                uint currentMP = InfoManager.Character != null ? InfoManager.Character.MP : 0;
                                bool hasConfiguredSkills = HasAnyConfiguredSkill(skillshots);

                                if (skillshots != null && skillshots.Length > 0)
                                {
                                    if (SkillManager.InOrderCombo)
                                    {
                                        // Sıralı kombo: imleçten başla, beklemede (cooldown) ya da
                                        // MP'si yetmeyen skill'i beklemeden atlayıp altındakini
                                        // kullan, liste biterse başa sar.
                                        int remaining = skillshots.Length;
                                        while (remaining-- > 0)
                                        {
                                            int picked = SkillPolicy.SelectNextReadyIndex(
                                                skillshots.Length,
                                                idx =>
                                                {
                                                    SRSkill candidate = skillshots[idx];
                                                    if (candidate == null)
                                                        return false;
                                                    if (candidate.ID != 1)
                                                    {
                                                        if (!candidate.Enabled && !candidate.isUsableSkill())
                                                            return false;
                                                        if (currentMP > 0 && candidate.MPUsage > currentMP)
                                                            return false;
                                                    }
                                                    return candidate.isCastingEnabled;
                                                },
                                                ref currentSkillIndex);
                                            if (picked < 0)
                                                break;
                                            if (TryPrepareAttackSkill(skillshots[picked], w))
                                            {
                                                skillToCast = skillshots[picked];
                                                break;
                                            }
                                            // Silah uygun değilse aynı turda altındaki skill'e devam et.
                                        }
                                    }
                                    else
                                    {
                                        // Priority mode: first check non-basic attack skills
                                        for (int k = 0; k < skillshots.Length; k++)
                                        {
                                            SRSkill candidate = skillshots[k];
                                            if (candidate != null && candidate.ID != 1 && (candidate.Enabled || candidate.isUsableSkill()) && candidate.isCastingEnabled
                                                && (currentMP == 0 || candidate.MPUsage <= currentMP))
                                            {
                                                if (TryPrepareAttackSkill(candidate, w))
                                                {
                                                    skillToCast = candidate;
                                                    break;
                                                }
                                            }
                                        }

                                        // If class skills are on cooldown, check if Common Attack is in list
                                        if (skillToCast == null)
                                        {
                                            for (int k = 0; k < skillshots.Length; k++)
                                            {
                                                SRSkill candidate = skillshots[k];
                                                if (candidate != null && candidate.ID == 1 && candidate.isCastingEnabled)
                                                {
                                                    skillToCast = candidate;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (skillToCast == null)
                                {
                                    // Oyuncu listede saldırı skili belirttiyse, skiller cooldown'da diye ASLA otomatik vuruşa (Common Attack) düşme!
                                    // Silkroad skilleri birkaç saniye içinde döner; kısa bekle ve bir sonraki turda ilk açılan skili anında bas.
                                    if (hasConfiguredSkills)
                                    {
                                        if (currentMP > 0 || noSkillAttempts < 40)
                                        {
                                            noSkillAttempts++;
                                            Thread.Sleep(30);
                                            continue;
                                        }
                                    }

                                    noSkillAttempts++;
                                    if (noSkillAttempts >= 2 && SkillPolicy.ShouldUseFallback(InfoManager.Mobs.ContainsKey(mob.UniqueID), false))
                                    {
                                        if (TryFallbackAttack(mob, w, false))
                                            obstacleRecovery.OnSuccess();
                                        else if (!RecoverFailedCombatAttack(mob, obstacleRecovery,
                                            $"Common Attack (0x{InfoManager.LastSkillCastErrorCode:X4})"))
                                            break;
                                        noSkillAttempts = 0;
                                    }
                                    else
                                    {
                                        Thread.Sleep(40);
                                    }
                                    continue;
                                }

                                noSkillAttempts = 0;

                                if (skillToCast.ID == 1)
                                {
                                    skillToCast = SkillManager.GetFallbackAttack(GetMyWeaponType());
                                }

                                if (!IsLiveCombatTarget(mob))
                                    break;

                                // Seçilen skilin menzil kontrolü (uzak büyü 15m iken yakın dövüş skili 3.5m-4.5m isteyebilir)
                                double requiredSkillRange = GetSkillAttackRange(skillToCast, myWeapon);
                                mobPosition = mob.GetRealtimePosition();
                                myPosition = InfoManager.Character.GetRealtimePosition();
                                distanceToMob = myPosition.DistanceTo(mobPosition);
                                if (distanceToMob > requiredSkillRange)
                                {
                                    bool reached = ApproachTargetWithCollision(mobPosition, requiredSkillRange, mob);
                                    if (!reached)
                                        break;
                                }

                                w.LogProcess("Casting skill " + skillToCast.Name + " (" + skillToCast.CastingTime + "ms)...");

                                if (!EnsureCombatTargetSelected(mob))
                                    break;

                                InfoManager.LastSkillCastSuccess = false;
                                InfoManager.LastSkillCastErrorCode = 0;
                                InfoManager.MonitorSkillCast.Reset();

                                PacketBuilder.AttackTarget(mob.UniqueID, skillToCast.ID);

                                bool confirmed = WaitForCombatCast(mob);
                                bool treatAsSuccess = confirmed && InfoManager.LastSkillCastSuccess;
                                // Damage from another player or a DOT is not our cast acknowledgement.

                                if (treatAsSuccess)
                                {
                                    obstacleRecovery.OnSuccess();
                                    SkillManager.RecordCastSuccess(skillToCast);
                                    try { skillToCast.StartCooldown(); } catch { }

                                    // Smooth pipeline: tam cast süresi kadar kör bekleme; sıradaki
                                    // skill hazır olur olmaz devam et. Üst üste reddediliyorsa
                                    // server erken ateşe izin vermiyor demektir, tam bekle.
                                    int castTime = Math.Max(0, skillToCast.CastingTime);
                                    int fullWait = Math.Max(350, castTime);
                                    int minGap = SkillManager.ConsecutiveCastFailures >= 2
                                        ? fullWait
                                        : Math.Max(200, (castTime * 2) / 3);
                                    int elapsed = 0;
                                    while (elapsed < fullWait && isBotting)
                                    {
                                        if (!IsLiveCombatTarget(mob))
                                            break;
                                        if (elapsed >= minGap && IsAnyAttackSkillReady(skillshots))
                                            break;
                                        Thread.Sleep(30);
                                        elapsed += 30;
                                    }

                                    // PhBot SlowerAttackMode: vuruşlar arası ek gecikme
                                    if (CombatAIEngine.SlowerAttackMode)
                                    {
                                        Thread.Sleep(300);
                                    }

                                    // PhBot SwitchMonsterAfterDot: canavara DOT (kanama/zehir/yanma) bulaştıysa diğerine geç
                                    if (CombatAIEngine.SwitchMonsterAfterDot && mob != null && mob.BadStatusFlags != SRModel.BadStatus.None)
                                    {
                                        w.LogProcess($"PhBot: Switching monster after DOT on [{mob.Name}]");
                                        break;
                                    }
                                }
                                else
                                {
                                    string reason = !confirmed ? "Zaman aşımı"
                                        : (InfoManager.LastSkillCastErrorCode == 0 ? "Sunucu reddetti"
                                            : $"Reddedildi (0x{InfoManager.LastSkillCastErrorCode:X4})");
                                    if (!IsLiveCombatTarget(mob))
                                        break;
                                    SkillManager.RecordCastFailure(skillToCast, reason);
                                    if (!RecoverFailedCombatAttack(mob, obstacleRecovery, reason))
                                        break;
                                    Thread.Sleep(60);
                                }
                            }

                            // Mob öldükten sonra drop topla
                            LootDrops(trainingPosition, trainingRadius);
                    }
                }
            }
        }

        /// <summary>
        /// Pipeline bekleyiş için hafif hazır-skill taraması (silah değiştirmez,
        /// paket göndermez; sadece sıradaki vuruşa geçilebilir mi diye bakar).
        /// </summary>
        private bool IsAnyAttackSkillReady(SRSkill[] skillshots)
        {
            if (skillshots == null || skillshots.Length == 0)
                return false;
            uint currentMP = 0;
            try { currentMP = InfoManager.Character != null ? InfoManager.Character.MP : 0; } catch { }
            for (int i = 0; i < skillshots.Length; i++)
            {
                SRSkill s = skillshots[i];
                if (s == null || !s.isCastingEnabled)
                    continue;
                if (s.ID != 1 && !s.Enabled && !s.isUsableSkill())
                    continue;
                if (s.ID != 1 && currentMP > 0 && s.MPUsage > currentMP)
                    continue;
                return true;
            }
            return false;
        }

        private bool TryPrepareAttackSkill(SRSkill skill, Window w)
        {
            if (skill == null || InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return false;

            SRTypes.Weapon primaryWeapon = skill.RequiredWeaponPrimary;
            SRTypes.Weapon secondaryWeapon = skill.RequiredWeaponSecondary;
            if (skill.ID == 1 || (primaryWeapon == SRTypes.Weapon.None && secondaryWeapon == SRTypes.Weapon.None))
                return true;

            SRTypes.Weapon currentWeapon = GetMyWeaponType();
            if (currentWeapon == primaryWeapon || currentWeapon == secondaryWeapon)
                return true;

            SRTypes.Weapon requiredWeapon = primaryWeapon != SRTypes.Weapon.None
                ? primaryWeapon
                : secondaryWeapon;

            w.LogProcess("Checking weapon required (" + requiredWeapon + ")...");
            int inventorySlot = InfoManager.Character.Inventory.FindIndex(
                item => item != null && item.ID2 == 1 && item.ID3 == 6
                    && (item.ID4 == (byte)primaryWeapon || item.ID4 == (byte)secondaryWeapon), 13, 16);
            if (inventorySlot == -1)
            {
                w.LogProcess("Weapon required not found (" + requiredWeapon + ")...");
                return false;
            }

            w.LogProcess("Changing weapon (" + currentWeapon + ")...");
            byte attempts = 5;
            while (currentWeapon != requiredWeapon && attempts > 0)
            {
                PacketBuilder.MoveItem((byte)inventorySlot, 6, SRTypes.InventoryItemMovement.InventoryToInventory);
                attempts--;
                InfoManager.MonitorWeaponChanged.WaitOne(250);
                currentWeapon = GetMyWeaponType();
            }

            if (currentWeapon != requiredWeapon)
            {
                w.LogProcess("Weapon changing failed!");
                return false;
            }

            return true;
        }

        private static bool IsLiveCombatTarget(SRMob mob)
        {
            return mob != null && CombatPolicy.IsLiveTarget(
                InfoManager.Mobs.ContainsKey(mob.UniqueID), mob.LifeStateType == SRModel.LifeState.Dead);
        }

        private void DeferCombatTarget(SRMob mob, string reason)
        {
            if (!IsLiveCombatTarget(mob)) return;
            _unreachableMobs[mob.UniqueID] = DateTime.UtcNow.AddSeconds(3);
            Window.Get.Log($"[Combat] {mob.Name} (UID {mob.UniqueID}) geçici olarak atlandı: {reason}");
        }

        private bool EnsureCombatTargetSelected(SRMob mob)
        {
            if (!isBotting || !IsLiveCombatTarget(mob)) return false;
            if (InfoManager.SelectedEntityUniqueID != mob.UniqueID)
            {
                // A previous selection signal must not acknowledge this request.
                InfoManager.MonitorEntitySelected.Reset();
                PacketBuilder.SelectEntity(mob.UniqueID);
                var wait = System.Diagnostics.Stopwatch.StartNew();
                while (isBotting && IsLiveCombatTarget(mob)
                    && InfoManager.SelectedEntityUniqueID != mob.UniqueID && wait.ElapsedMilliseconds < 750)
                    InfoManager.MonitorEntitySelected.WaitOne(50);
            }

            bool ready = isBotting && CombatPolicy.CanSendAttack(IsLiveCombatTarget(mob),
                mob.UniqueID, InfoManager.SelectedEntityUniqueID);
            if (!ready) DeferCombatTarget(mob, "Hedef seçimi onaylanmadı");
            return ready;
        }

        private bool TryFallbackAttack(SRMob mob, Window w, bool deferOnFailure = true)
        {
            if (!EnsureCombatTargetSelected(mob))
                return false;

            SRSkill fallback = SkillManager.GetFallbackAttack(GetMyWeaponType());
            if (fallback == null)
                return false;

            w.LogProcess("Casting Common Attack fallback...");
            InfoManager.LastSkillCastSuccess = false;
            InfoManager.LastSkillCastErrorCode = 0;
            InfoManager.MonitorSkillCast.Reset();

            PacketBuilder.AttackTarget(mob.UniqueID, 1u);
            if (WaitForCombatCast(mob))
            {
                if (InfoManager.LastSkillCastSuccess)
                {
                    SkillManager.RecordCastSuccess(fallback);
                    int elapsed = 0;
                    int animTime = Math.Max(400, fallback.CastingTime);
                    while (elapsed < animTime && isBotting)
                    {
                        if (!IsLiveCombatTarget(mob))
                            break;
                        Thread.Sleep(50);
                        elapsed += 50;
                    }
                    return true;
                }
            }

            if (!IsLiveCombatTarget(mob)) return false;
            SkillManager.RecordCastFailure(fallback, "Common Attack yanıt vermedi");
            if (deferOnFailure)
                DeferCombatTarget(mob, $"Common Attack reddedildi/zaman aşımı (0x{InfoManager.LastSkillCastErrorCode:X4})");
            Thread.Sleep(80);
            return false;
        }

        private bool WaitForCombatCast(SRMob mob)
        {
            var wait = System.Diagnostics.Stopwatch.StartNew();
            do
            {
                if (InfoManager.MonitorSkillCast.WaitOne(50)) return true;
            }
            while (isBotting && IsLiveCombatTarget(mob) && wait.ElapsedMilliseconds < 1200);
            return false;
        }

        private LocalObstacleNavigator CreateCombatNavigator(SRMob mob)
        {
            SRCoord center = Window.Get.TrainingArea_GetPosition();
            int radius = Window.Get.TrainingArea_GetRadius();
            return new LocalObstacleNavigator(
                () => InfoManager.Character?.GetRealtimePosition(),
                point => {
                    if (InfoManager.inGame && InfoManager.Character != null
                        && InfoManager.Character.LifeStateType != SRModel.LifeState.Dead)
                        MoveTo(point);
                },
                Thread.Sleep,
                () => isBotting && !m_stopBottingRequested && InfoManager.inGame
                    && InfoManager.Character != null
                    && InfoManager.Character.LifeStateType != SRModel.LifeState.Dead
                    && (mob == null || IsLiveCombatTarget(mob)),
                point => !TrainingOptionsPolicy.StayInTrainingArea || center == null || radius <= 0
                    || center.DistanceTo(point) <= radius);
        }

        private bool RecoverFailedCombatAttack(SRMob mob, CombatObstacleRecovery recovery, string reason)
        {
            if (!isBotting || !IsLiveCombatTarget(mob)
                || InfoManager.SelectedEntityUniqueID != mob.UniqueID) return false;
            bool enabled = CollisionPolicy.EnableCollisionInTrainingArea && CollisionPolicy.NavigateAroundObstacles;
            bool retry = recovery.OnFailure(enabled, attempt => {
                if (!isBotting || !IsLiveCombatTarget(mob)) return false;
                Window.Get.Log($"[Combat] {mob.Name}: saldırı ilerlemiyor ({reason}); engel için yan geçiş {attempt + 1}/{LocalObstacleNavigator.MaxDetours}.");
                return CreateCombatNavigator(mob).TryDetour(mob.GetRealtimePosition(), attempt);
            });
            if (!retry)
            {
                DeferCombatTarget(mob, reason);
                if (enabled && IsLiveCombatTarget(mob))
                    _unreachableMobs[mob.UniqueID] = DateTime.UtcNow.AddSeconds(15);
            }
            return retry;
        }

        private bool ApproachTargetWithCollision(SRCoord targetPosition, double stopRange, SRMob targetMob)
        {
            SRCoord myPosition = InfoManager.Character?.GetRealtimePosition();
            if (myPosition == null || targetPosition == null) return false;
            if (myPosition.DistanceTo(targetPosition) <= stopRange) return true;

            if (CombatAIEngine.UseTeleportSkills && myPosition.DistanceTo(targetPosition) > 12.0)
                TryCastTeleportSkill();

            var navigator = CreateCombatNavigator(targetMob);
            if (navigator.MoveTo(targetPosition, stopRange)) return true;
            if (!CollisionPolicy.EnableCollisionInTrainingArea) return false;

            if (CollisionPolicy.NavigateAroundObstacles)
            {
                for (int attempt = 0; attempt < LocalObstacleNavigator.MaxDetours && isBotting; attempt++)
                {
                    if (targetMob != null)
                    {
                        if (!IsLiveCombatTarget(targetMob)) return false;
                        targetPosition = targetMob.GetRealtimePosition();
                    }
                    Window.Get.LogProcess($"Engel için yan geçiş deneniyor ({attempt + 1}/{LocalObstacleNavigator.MaxDetours})...");
                    if (!navigator.TryDetour(targetPosition, attempt)) continue;
                    if (targetMob != null) targetPosition = targetMob.GetRealtimePosition();
                    if (navigator.MoveTo(targetPosition, stopRange)) return true;
                }
            }

            if (IsLiveCombatTarget(targetMob))
            {
                _unreachableMobs[targetMob.UniqueID] = DateTime.UtcNow.AddSeconds(15);
                Window.Get.Log($"[Combat] {targetMob.Name}: engel aşılamadı, başka hedefe geçiliyor.");
            }
            return false;
        }

        private void TryCastTeleportSkill()
        {
            if (InfoManager.Character == null || InfoManager.Character.Skills == null) return;
            for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
            {
                var sk = InfoManager.Character.Skills.GetAt(i);
                if (sk != null && sk.Enabled && sk.isCastingEnabled)
                {
                    string sName = sk.ServerName ?? "";
                    if (sName.IndexOf("TELEPORT", StringComparison.OrdinalIgnoreCase) >= 0
                        || sName.IndexOf("MOVING_MARCH", StringComparison.OrdinalIgnoreCase) >= 0
                        || sName.IndexOf("PHANTOM", StringComparison.OrdinalIgnoreCase) >= 0
                        || sName.IndexOf("GHOST_WALK", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        PacketBuilder.CastSkill(sk.ID);
                        Thread.Sleep(200);
                        break;
                    }
                }
            }
        }

        private bool CheckLureTick(Window w, SRCoord trainingPosition, int trainingRadius)
        {
            if (!LurePolicy.WalkBackDistEnabled && !LurePolicy.LureSkillEnabled && !LurePolicy.UseScript)
                return false;

            // Check Stop conditions
            int deadParty = 0;
            if (InfoManager.Party?.Members != null)
            {
                for (int i = 0; i < InfoManager.Party.Members.Count; i++)
                {
                    var m = InfoManager.Party.Members.GetAt(i);
                    if (m != null && m.HPPercent == 0) deadParty++;
                }
            }

            int giantPartyCount = 0;
            int areaMobCount = 0;
            for (int i = 0; i < InfoManager.Mobs.Count; i++)
            {
                var m = InfoManager.Mobs.GetAt(i);
                if (m == null) continue;
                if (trainingPosition != null && trainingRadius > 0 && trainingPosition.DistanceTo(m.GetRealtimePosition()) <= trainingRadius)
                {
                    areaMobCount++;
                    if (m.MobType == SRMob.Mob.PartyGiant) giantPartyCount++;
                }
            }

            bool partyNear = true;
            if (LurePolicy.StopIfPartyAway && InfoManager.Party?.Members != null)
            {
                for (int i = 0; i < InfoManager.Party.Members.Count; i++)
                {
                    var m = InfoManager.Party.Members.GetAt(i);
                    if (m == null || m.Name == InfoManager.Character.Name) continue;
                    if (LurePolicy.PartyNearWhitelist.Count > 0 && !LurePolicy.PartyNearWhitelist.Contains(m.Name)) continue;
                    var lp = InfoManager.Players.Find(p => p != null && p.Name == m.Name);
                    if (lp == null || lp.Position == null || (trainingPosition != null && trainingPosition.DistanceTo(lp.Position) > (trainingRadius + 45.0)))
                    {
                        partyNear = false;
                        break;
                    }
                }
            }

            if (LurePolicy.ShouldPauseLure(deadParty, giantPartyCount, areaMobCount, partyNear))
            {
                w.LogProcess("Lure: Durdurma koşulu sağlandı (ölü parti / mob sayısı / parti uzak), lure bekletiliyor.");
                Thread.Sleep(800);
                return true;
            }

            // Cast Lure skill if enabled
            if (LurePolicy.LureSkillEnabled && !string.IsNullOrEmpty(LurePolicy.LureSkillName))
            {
                SRSkill lSkill = null;
                if (InfoManager.Character?.Skills != null)
                {
                    for (int i = 0; i < InfoManager.Character.Skills.Count; i++)
                    {
                        var s = InfoManager.Character.Skills.GetAt(i);
                        if (s != null && s.Enabled && s.isCastingEnabled && (s.Name == LurePolicy.LureSkillName || s.ServerName == LurePolicy.LureSkillName))
                        {
                            lSkill = s;
                            break;
                        }
                    }
                }

                if (lSkill != null)
                {
                    for (int i = 0; i < InfoManager.Mobs.Count; i++)
                    {
                        var m = InfoManager.Mobs.GetAt(i);
                        if (m == null) continue;
                        if (m.TargetUniqueID != InfoManager.Character.UniqueID && trainingPosition != null && trainingPosition.DistanceTo(m.GetRealtimePosition()) <= trainingRadius)
                        {
                            w.LogProcess($"Lure: Casting [{lSkill.Name}] on [{m.Name}]...");
                            PacketBuilder.AttackTarget(m.UniqueID, lSkill.ID);
                            Thread.Sleep(Math.Max(350, lSkill.CastingTime + 100));
                            break;
                        }
                    }
                }
            }

            if (LurePolicy.BuffAtLureEnd)
            {
                BuffLoop();
            }

            return false;
        }

        private SRMob GetMobFiltered(List<SRMob> mobs, SRCoord trainingPosition, int trainingRadius)
        {
            if (mobs == null || mobs.Count == 0)
                return null;

            if (_unreachableMobs.Count > 0)
            {
                DateTime now = DateTime.UtcNow;
                var expired = new List<uint>();
                foreach (var kvp in _unreachableMobs)
                {
                    if (now >= kvp.Value) expired.Add(kvp.Key);
                }
                for (int e = 0; e < expired.Count; e++)
                    _unreachableMobs.Remove(expired[e]);
            }

            SRMob bestMob = null;
            double bestScore = double.MinValue;
            SRCoord myPosition = InfoManager.Character.GetRealtimePosition();

            Window w = Window.Get;
            bool enablePriority = (w.Combat_cbxMobPriority == null || w.Combat_cbxMobPriority.Checked);

            for (int j = 0; j < mobs.Count; j++)
            {
                SRMob m = mobs[j];
                if (!IsLiveCombatTarget(m)) continue;

                // Çarpışma / Engel nedeniyle ulaşılamayan canavarlar geçici süreyle atlanır
                if (_unreachableMobs.ContainsKey(m.UniqueID))
                    continue;

                // Check if user allowed targeting this mob type
                bool allowed = true;
                if (w.Combat_cbxTargetGeneral != null)
                {
                    switch (m.MobType)
                    {
                        case SRMob.Mob.General:
                            allowed = w.Combat_cbxTargetGeneral.Checked;
                            break;
                        case SRMob.Mob.Champion:
                            allowed = w.Combat_cbxTargetChampion.Checked;
                            break;
                        case SRMob.Mob.Giant:
                            allowed = w.Combat_cbxTargetGiant.Checked;
                            break;
                        case SRMob.Mob.PartyGeneral:
                        case SRMob.Mob.PartyChampion:
                        case SRMob.Mob.PartyGiant:
                            allowed = w.Combat_cbxTargetParty.Checked;
                            break;
                        case SRMob.Mob.Elite:
                            allowed = w.Combat_cbxTargetElite.Checked;
                            break;
                        case SRMob.Mob.Unique:
                            allowed = w.Combat_cbxTargetUnique.Checked;
                            break;
                    }
                }

                SRCoord mobPosition = m.GetRealtimePosition();
                bool withinTrainingArea = trainingPosition == null || trainingRadius <= 0
                    || trainingPosition.DistanceTo(mobPosition) <= trainingRadius;

                // Training alanında kal: alan dışındaki moblara asla yönelme
                if (TrainingOptionsPolicy.StayInTrainingArea && !withinTrainingArea)
                    continue;

                // Statue of Justice: onay verilmemişse saldırılmaz
                bool isStatueOfJustice = (m.Name != null && (m.Name.IndexOf("Statue of Justice", StringComparison.OrdinalIgnoreCase) >= 0 || m.Name.IndexOf("Adalet", StringComparison.OrdinalIgnoreCase) >= 0))
                    || (m.ServerName != null && m.ServerName.IndexOf("STATUE_OF_JUSTICE", StringComparison.OrdinalIgnoreCase) >= 0);
                if (isStatueOfJustice && !TrainingOptionsPolicy.AttackStatueOfJustice)
                    continue;

                // Canavar Tercihleri: Yoksay (Ignore) kontrolü
                var prefEntry = CombatAIEngine.FindPreference(m);
                if (prefEntry != null && prefEntry.Preference == MonsterPreferenceType.Ignore)
                {
                    bool isQuestAttack = CombatAIEngine.AllowAttackingQuestMobs && m.TargetUniqueID == InfoManager.Character.UniqueID;
                    if (!isQuestAttack)
                        continue;
                }

                if (!CombatPolicy.CanTarget(new CombatTargetInput
                {
                    AllowedByType = allowed,
                    Avoided = (prefEntry != null && prefEntry.Preference == MonsterPreferenceType.Avoid) || CombatAIEngine.ShouldAvoid(m.MobType),
                    IsDimensionPillar = CombatAIEngine.IsDimensionPillar(m),
                    IgnoreDimensionPillars = CombatAIEngine.IgnoreDimensionPillars,
                    DoNotFollowMobs = CombatAIEngine.DoNotFollowMobs,
                    WithinTrainingArea = withinTrainingArea
                }))
                    continue;

                // PhBot KillSteal: kapalıysa başka oyuncunun saldırdığı mobları atla (parti dışı)
                if (!CombatAIEngine.KillSteal && m.TargetUniqueID != 0 && m.TargetUniqueID != InfoManager.Character.UniqueID)
                {
                    bool isPartyTarget = false;
                    if (InfoManager.Party?.Members != null)
                    {
                        SREntity targetEnt = InfoManager.GetEntity(m.TargetUniqueID);
                        if (targetEnt != null)
                        {
                            for (int pmi = 0; pmi < InfoManager.Party.Members.Count; pmi++)
                            {
                                var pm = InfoManager.Party.Members.GetAt(pmi);
                                if (pm != null && pm.Name == targetEnt.Name) { isPartyTarget = true; break; }
                            }
                        }
                    }
                    if (!isPartyTarget) continue;
                }

                double dist = mobPosition.DistanceTo(myPosition);

                // If priority is disabled and user has no custom Monster Preferences, strictly choose the nearest mob (karakter ayırt etmez)
                bool hasCustomPreferences = CombatAIEngine.MonsterPreferences != null && CombatAIEngine.MonsterPreferences.Count > 0;
                if (!enablePriority && !hasCustomPreferences)
                {
                    double distScore = -dist;
                    if (distScore > bestScore)
                    {
                        bestScore = distScore;
                        bestMob = m;
                    }
                    continue;
                }

                // Base priority score based on Silkroad mob danger
                double score = 10.0;
                if (enablePriority)
                {
                    switch (m.MobType)
                    {
                        case SRMob.Mob.Unique:
                            score = 250.0;
                            break;
                        case SRMob.Mob.Elite:
                            score = 180.0;
                            break;
                        case SRMob.Mob.PartyGiant:
                            score = 140.0;
                            break;
                        case SRMob.Mob.Giant:
                            score = 100.0;
                            break;
                        case SRMob.Mob.PartyChampion:
                            score = 75.0;
                            break;
                        case SRMob.Mob.Champion:
                            score = 50.0;
                            break;
                        case SRMob.Mob.PartyGeneral:
                            score = 30.0;
                            break;
                        default:
                            score = 10.0;
                            break;
                    }
                }

                // PhBot ProtectParty: parti üyesine vuran canavara en yüksek öncelik
                if (CombatAIEngine.ProtectParty && m.TargetUniqueID != 0)
                {
                    bool isAttackingParty = false;
                    if (InfoManager.Party?.Members != null)
                    {
                        SREntity targetEnt = InfoManager.GetEntity(m.TargetUniqueID);
                        if (targetEnt != null)
                        {
                            for (int pmi = 0; pmi < InfoManager.Party.Members.Count; pmi++)
                            {
                                var pm = InfoManager.Party.Members.GetAt(pmi);
                                if (pm != null && pm.Name == targetEnt.Name)
                                {
                                    if (string.IsNullOrEmpty(CombatAIEngine.ProtectPartyTarget) || pm.Name.IndexOf(CombatAIEngine.ProtectPartyTarget, StringComparison.OrdinalIgnoreCase) >= 0)
                                    {
                                        isAttackingParty = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    if (isAttackingParty)
                    {
                        score += 8000.0;
                    }
                }

                // PetPolicy ProtectAttackPet: Saldırı petine saldıran canavara en yüksek öncelik
                if (PetPolicy.ProtectAttackPet && m.TargetUniqueID != 0)
                {
                    var atkPet = InfoManager.MyPets?.Find(p => p != null && p.isAttackPet());
                    if (atkPet != null && m.TargetUniqueID == atkPet.UniqueID)
                    {
                        score += 9000.0; // Peti korumak için en yüksek öncelik
                    }
                }

                // PhBot AttackLowerFirst: düşük seviyeli canavarlara öncelik ver
                if (CombatAIEngine.AttackLowerFirst && m.Level > 0)
                {
                    score += (140 - m.Level) * 4.0;
                }

                // Canavar Tercihleri: Tercih Et (öncelik listenin başına göre) & Uzak Dur
                if (prefEntry != null)
                {
                    if (prefEntry.Preference == MonsterPreferenceType.Prefer)
                    {
                        int prefIdx = CombatAIEngine.GetPreferenceIndex(m);
                        int listBonus = (prefIdx >= 0) ? (CombatAIEngine.MonsterPreferences.Count - prefIdx) * 1000 : 500;
                        score += 2000.0 + listBonus;
                    }
                    else if (prefEntry.Preference == MonsterPreferenceType.Avoid)
                    {
                        score -= 5000.0;
                    }
                }
                else if (CombatAIEngine.IsPreferred(m.MobType))
                {
                    score += 200.0;
                }

                // CombatAI: AttackWeakerFirst — boost mobs with lower HP ratio
                if (CombatAIEngine.AttackWeakerFirst && m.HPMax > 0)
                {
                    double hpRatio = (double)m.HP / m.HPMax;
                    score += (1.0 - hpRatio) * 80.0;
                }

                // If mob is close and in attacking range, prioritize it
                if (dist <= 6.0)
                {
                    score += 35.0;
                }

                // Distance penalty: prefer closer enemies to minimize running around
                score -= (dist * 1.5);

                if (score > bestScore)
                {
                    bestScore = score;
                    bestMob = m;
                }
            }

            return bestMob;
        }

        private static DateTime s_lastBerserkAttemptUtc = DateTime.MinValue;

        private void CheckBerserker(List<SRMob> nearbyMobs)
        {
            if (InfoManager.Character == null)
                return;

            // Only trigger if Berserker bar is 100% full (5 points)
            if (InfoManager.Character.BerserkPoints < 5)
            {
                if (InfoManager.Character.GameStateType != SRModel.GameState.Berserk)
                {
                    if (TrainingOptionsPolicy.UseZerkPotion)
                        TryUseZerkPotion();
                    if (TrainingOptionsPolicy.UseEnergyOfLifeForZerk)
                        TryUseEnergyOfLife(true);
                }
                return;
            }

            // Don't trigger if already in Berserk mode (0x30BF sets GameStateType to Berserk = 1)
            if (InfoManager.Character.GameStateType == SRModel.GameState.Berserk)
                return;

            if ((DateTime.UtcNow - s_lastBerserkAttemptUtc).TotalSeconds < 3.0)
                return;

            double hpPercent = InfoManager.Character.HPMax > 0
                ? ((double)InfoManager.Character.HP / InfoManager.Character.HPMax * 100.0)
                : 100.0;

            // Delegate to CombatAIEngine for configurable berserk triggers
            bool shouldActivate = CombatAIEngine.CheckBerserkTrigger(nearbyMobs, hpPercent);

            // Fallback: HP low (< 45%) and in combat
            if (!shouldActivate && hpPercent < 45.0 && nearbyMobs != null && nearbyMobs.Count > 0)
                shouldActivate = true;

            if (shouldActivate)
            {
                s_lastBerserkAttemptUtc = DateTime.UtcNow;
                System.Diagnostics.Debug.WriteLine("Combat AI: Berserker trigger! Activating Berserk mode!");
                PacketBuilder.ActivateBerserk();
                Thread.Sleep(400);
            }
        }

        private static DateTime s_lastZerkPotAttemptUtc = DateTime.MinValue;
        private bool TryUseZerkPotion()
        {
            try
            {
                if ((DateTime.UtcNow - s_lastZerkPotAttemptUtc).TotalSeconds < 5.0)
                    return false;
                s_lastZerkPotAttemptUtc = DateTime.UtcNow;

                var chr = InfoManager.Character;
                if (chr == null || chr.Inventory == null) return false;
                for (byte s = 13; s < chr.Inventory.Capacity; s++)
                {
                    var it = chr.Inventory[s];
                    if (it == null) continue;
                    string sn = it.ServerName ?? "";
                    string name = it.Name ?? "";
                    if (sn.IndexOf("ZERK", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        sn.IndexOf("BERSERK", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("Berserker regeneration", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("Zerk", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Berserker: Zerk potu kullanılıyor [{it.Name}]...");
                        return PacketBuilder.UseItem(it, s);
                    }
                }
            }
            catch { }
            return false;
        }

        private static DateTime s_lastEnergyOfLifeAttemptUtc = DateTime.MinValue;
        private bool TryUseEnergyOfLife(bool forZerk = false)
        {
            try
            {
                if ((DateTime.UtcNow - s_lastEnergyOfLifeAttemptUtc).TotalSeconds < 5.0)
                    return false;
                s_lastEnergyOfLifeAttemptUtc = DateTime.UtcNow;

                var chr = InfoManager.Character;
                if (chr == null || chr.Inventory == null) return false;
                for (byte s = 13; s < chr.Inventory.Capacity; s++)
                {
                    var it = chr.Inventory[s];
                    if (it == null) continue;
                    string sn = it.ServerName ?? "";
                    string name = it.Name ?? "";
                    if (sn.IndexOf("ENERGY_OF_LIFE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        name.IndexOf("Energy of Life", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"Energy of Life: {(forZerk ? "Zerk doldurmak için" : "HP için")} kullanılıyor [{it.Name}]...");
                        return PacketBuilder.UseItem(it, s);
                    }
                }
            }
            catch { }
            return false;
        }

        private static DateTime s_lastSummonScrollCheckUtc = DateTime.MinValue;
        private void CheckPandoraAndMonsterScrolls(List<SRMob> nearbyMobs)
        {
            if (!TrainingOptionsPolicy.UsePandoraBoxOrMonsterScroll)
                return;

            if ((DateTime.UtcNow - s_lastSummonScrollCheckUtc).TotalSeconds < 10.0)
                return;
            s_lastSummonScrollCheckUtc = DateTime.UtcNow;

            // Check if strong mobs alive
            if (TrainingOptionsPolicy.WaitForStrongMobsBeforeSummon && nearbyMobs != null)
            {
                bool hasStrong = false;
                for (int i = 0; i < nearbyMobs.Count; i++)
                {
                    var m = nearbyMobs[i];
                    if (m != null && (m.MobType == SRMob.Mob.Champion || m.MobType == SRMob.Mob.Giant ||
                                      m.MobType == SRMob.Mob.PartyChampion || m.MobType == SRMob.Mob.PartyGiant ||
                                      m.MobType == SRMob.Mob.Elite || m.MobType == SRMob.Mob.Unique))
                    {
                        hasStrong = true;
                        break;
                    }
                }
                if (hasStrong)
                    return;
            }

            var chr = InfoManager.Character;
            if (chr == null || chr.Inventory == null)
                return;

            int pandoraCount = 0;
            byte pandoraSlot = 0;
            byte otherSummonSlot = 0;

            for (byte s = 13; s < chr.Inventory.Capacity; s++)
            {
                var it = chr.Inventory[s];
                if (it == null) continue;
                string sn = it.ServerName ?? "";
                string name = it.Name ?? "";

                bool isPandora = sn.IndexOf("PANDORA", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                 name.IndexOf("Pandora", StringComparison.OrdinalIgnoreCase) >= 0;
                bool isMonsterScroll = sn.IndexOf("SUMMON", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       sn.IndexOf("MONSTER_SCROLL", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       name.IndexOf("Monster Summon", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                       name.IndexOf("Canavar Çağırma", StringComparison.OrdinalIgnoreCase) >= 0;

                if (isPandora)
                {
                    pandoraCount += it.Quantity > 0 ? it.Quantity : 1;
                    if (pandoraSlot == 0) pandoraSlot = s;
                }
                else if (isMonsterScroll && otherSummonSlot == 0)
                {
                    otherSummonSlot = s;
                }
            }

            if (otherSummonSlot > 0)
            {
                var it = chr.Inventory[otherSummonSlot];
                System.Diagnostics.Debug.WriteLine($"Kasılma Alanı: Canavar çağırma scroll'u kullanılıyor [{it.Name}]...");
                PacketBuilder.UseItem(it, otherSummonSlot);
                Thread.Sleep(500);
                return;
            }

            if (pandoraSlot > 0)
            {
                if (TrainingOptionsPolicy.PreservePandora && pandoraCount <= TrainingOptionsPolicy.PreservePandoraCount)
                    return;

                var it = chr.Inventory[pandoraSlot];
                System.Diagnostics.Debug.WriteLine($"Kasılma Alanı: Pandora's Box açılıyor [{it.Name}] (Kalan: {pandoraCount})...");
                PacketBuilder.UseItem(it, pandoraSlot);
                Thread.Sleep(500);
            }
        }

        private static DateTime s_lastFlowerCheckUtc = DateTime.MinValue;
        private void CheckFlowerSummon()
        {
            if (!TrainingOptionsPolicy.SummonFlowersInTrainingArea)
                return;

            if ((DateTime.UtcNow - s_lastFlowerCheckUtc).TotalSeconds < 15.0)
                return;
            s_lastFlowerCheckUtc = DateTime.UtcNow;

            var chr = InfoManager.Character;
            if (chr == null || chr.Inventory == null)
                return;

            if (chr.Buffs != null)
            {
                for (byte b = 0; b < chr.Buffs.Count; b++)
                {
                    var buff = chr.Buffs.GetAt(b);
                    if (buff == null) continue;
                    string bname = buff.Name ?? "";
                    string bsn = buff.ServerName ?? "";
                    if (bname.IndexOf("Flower", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        bname.IndexOf("Çiçek", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        bsn.IndexOf("FLOWER", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return;
                    }
                }
            }

            for (byte s = 13; s < chr.Inventory.Capacity; s++)
            {
                var it = chr.Inventory[s];
                if (it == null) continue;
                string sn = it.ServerName ?? "";
                string name = it.Name ?? "";
                if (sn.IndexOf("FLOWER", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("Flower", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("Çiçek", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Kasılma Alanı: Çiçek kullanılıyor [{it.Name}]...");
                    PacketBuilder.UseItem(it, s);
                    Thread.Sleep(500);
                    return;
                }
            }
        }

        private static DateTime s_lastTreasureBoxCheckUtc = DateTime.MinValue;
        private void CheckTreasureBoxes()
        {
            if (!TrainingOptionsPolicy.UseTreasureBoxes)
                return;

            if ((DateTime.UtcNow - s_lastTreasureBoxCheckUtc).TotalSeconds < 15.0)
                return;
            s_lastTreasureBoxCheckUtc = DateTime.UtcNow;

            var chr = InfoManager.Character;
            if (chr == null || chr.Inventory == null)
                return;

            for (byte s = 13; s < chr.Inventory.Capacity; s++)
            {
                var it = chr.Inventory[s];
                if (it == null) continue;
                string sn = it.ServerName ?? "";
                string name = it.Name ?? "";
                if (sn.IndexOf("TREASURE", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    sn.IndexOf("MAGIC_BOX", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("Treasure", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("Hazine", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"Kasılma Alanı: Treasure Box kullanılıyor [{it.Name}]...");
                    PacketBuilder.UseItem(it, s);
                    Thread.Sleep(500);
                    return;
                }
            }
        }

        private static DateTime s_lastAutoEquipCheckUtc = DateTime.MinValue;
        private void CheckAutoEquipBetterItems()
        {
            if (!TrainingOptionsPolicy.AutoEquipBetterItems)
                return;

            if ((DateTime.UtcNow - s_lastAutoEquipCheckUtc).TotalSeconds < 8.0)
                return;
            s_lastAutoEquipCheckUtc = DateTime.UtcNow;

            var chr = InfoManager.Character;
            if (chr == null || chr.Inventory == null)
                return;

            byte charLevel = chr.Level;
            bool isMale = chr.ServerName != null && chr.ServerName.Contains("_M_");
            bool isFemale = chr.ServerName != null && chr.ServerName.Contains("_W_");
            var charRace = chr.GetRace();

            for (byte s = 13; s < chr.Inventory.Capacity; s++)
            {
                var it = chr.Inventory[s];
                if (it == null || !it.isEquipable()) continue;

                var eq = it as SREquipable ?? new SREquipable(it);
                if (eq.LevelRequired > charLevel) continue;

                var itemRace = eq.GetRace();
                if (itemRace != SRTypes.Race.Unknown && itemRace != charRace) continue;

                var genre = eq.GetGenre();
                if (genre == SREquipable.Genre.Male && !isMale && isFemale) continue;
                if (genre == SREquipable.Genre.Female && !isFemale && isMale) continue;

                int targetSlot = -1;
                switch (eq.ItemType)
                {
                    case SREquipable.Equipable.Garment:
                    case SREquipable.Equipable.Protector:
                    case SREquipable.Equipable.Armor:
                    case SREquipable.Equipable.Robe:
                    case SREquipable.Equipable.LightArmor:
                    case SREquipable.Equipable.HeavyArmor:
                        switch ((SRTypes.SetPart)eq.ID4)
                        {
                            case SRTypes.SetPart.Head: targetSlot = 0; break;
                            case SRTypes.SetPart.Chest: targetSlot = 1; break;
                            case SRTypes.SetPart.Shoulders: targetSlot = 2; break;
                            case SRTypes.SetPart.Gloves: targetSlot = 3; break;
                            case SRTypes.SetPart.Pants: targetSlot = 4; break;
                            case SRTypes.SetPart.Boots: targetSlot = 5; break;
                        }
                        break;
                    case SREquipable.Equipable.Weapon:
                        var curWpn = chr.Inventory[6] as SREquipable;
                        if (curWpn == null || curWpn.ID4 == eq.ID4)
                            targetSlot = 6;
                        break;
                    case SREquipable.Equipable.Shield:
                        targetSlot = 7;
                        break;
                    case SREquipable.Equipable.AccesoriesCH:
                    case SREquipable.Equipable.AccesoriesEU:
                        switch ((SRTypes.AccesoriesPart)eq.ID4)
                        {
                            case SRTypes.AccesoriesPart.Earring: targetSlot = 9; break;
                            case SRTypes.AccesoriesPart.Necklace: targetSlot = 10; break;
                            case SRTypes.AccesoriesPart.Ring:
                                targetSlot = chr.Inventory[11] == null ? 11 : 12;
                                break;
                        }
                        break;
                }

                if (targetSlot < 0 || targetSlot >= 13) continue;

                var currentItem = chr.Inventory[targetSlot] as SREquipable;
                byte eqDegree = AlchemyPolicy.CalculateDegree(eq.LevelRequired);
                byte curDegree = currentItem != null ? AlchemyPolicy.CalculateDegree(currentItem.LevelRequired) : (byte)0;

                bool isBetter = false;
                if (currentItem == null)
                {
                    isBetter = true;
                }
                else if (eqDegree > curDegree)
                {
                    isBetter = true;
                }
                else if (eqDegree == curDegree)
                {
                    if (eq.GetRarity() > currentItem.GetRarity())
                        isBetter = true;
                    else if (eq.GetRarity() == currentItem.GetRarity() && eq.Plus > currentItem.Plus)
                        isBetter = true;
                }

                if (isBetter)
                {
                    System.Diagnostics.Debug.WriteLine($"Otomatik Kuşanma: Daha iyi eşya bulundu [{eq.Name}] -> Slot {targetSlot}'a kuşanılıyor...");
                    EquipItem(s);
                    Thread.Sleep(500);
                    return;
                }
            }
        }

        #region Pet Motoru Entegrasyonu
        private bool CheckPetEngineTick(SRCoord trainingPosition, int trainingRadius)
        {
            Window w = Window.Get;
            if (InfoManager.Character == null) return false;
            SRCoord myPosition = InfoManager.Character.GetRealtimePosition();

            // 1. Saldırı Peti Otomasyonu
            if (PetPolicy.UseAttackPet)
            {
                var atkPet = InfoManager.MyPets?.Find(p => p != null && p.isAttackPet() && !Script.IsLikelyFellow(p));
                if (atkPet == null)
                    atkPet = InfoManager.MyPets?.Find(p => p != null && p.isAttackPet());

                bool inTown = myPosition != null && TownManager.Get.IsNearTown(myPosition);
                bool atTrainingArea = trainingPosition != null && myPosition != null && myPosition.DistanceTo(trainingPosition) <= trainingRadius;

                // Pet çağrılı mı kontrol et
                if (atkPet == null)
                {
                    // Otomatik çağırma
                    bool hasHpPots = HasPetHpPotions();
                    if (PetPolicy.CanSummon(inTown, atTrainingArea, hasHpPots))
                    {
                        TryAutoSummonAttackPet(w);
                    }
                }
                else
                {
                    // Pet çağrılı ise durumunu kontrol et (Canlı / Ölü / Takılı)
                    if (atkPet.HP == 0)
                    {
                        // Pet ölmüş: Canlandırma dene
                        if (PetPolicy.CanRevive())
                        {
                            bool revived = TryAutoReviveAttackPet(atkPet, w);
                            if (!revived && PetPolicy.ReturnTownWhenAttackPetDies)
                            {
                                w.LogProcess("Saldırı Peti öldü ve canlandırılamadı ('Saldırı peti ölünce şehre dön' aktif). Şehre dönülüyor...", Window.ProcessState.Warning);
                                UseReturnScroll();
                                return true;
                            }
                        }
                        else if (PetPolicy.ReturnTownWhenAttackPetDies)
                        {
                            w.LogProcess("Saldırı Peti öldü (canlandırma limiti doldu). Şehre dönülüyor...", Window.ProcessState.Warning);
                            UseReturnScroll();
                            return true;
                        }
                    }
                    else
                    {
                        // Pet canlı: Takılma (Stuck) ve Auto-Recall kontrolü
                        if (PetPolicy.AutoRecallStuckAttackPet && myPosition != null)
                        {
                            SRCoord petPos = atkPet.GetRealtimePosition();
                            if (petPos != null)
                            {
                                double dist = myPosition.DistanceTo(petPos);
                                if (dist > 35.0 || (dist > 15.0 && PetPolicy.LastKnownPetPosition != null && petPos.DistanceTo(PetPolicy.LastKnownPetPosition) < 0.6))
                                {
                                    if ((DateTime.UtcNow - PetPolicy.LastRecallTime).TotalSeconds >= 8)
                                    {
                                        PetPolicy.LastRecallTime = DateTime.UtcNow;
                                        PacketBuilder.MoveTo(myPosition, atkPet.UniqueID);
                                        w.LogProcess($"Saldırı Peti takıldı/uzaklaştı ({dist:F1}m). Auto-recall yapılıyor...");
                                    }
                                }
                                PetPolicy.LastKnownPetPosition = petPos;
                            }
                        }
                    }
                }
            }

            // 2. Fellow Pet Otomasyonu
            var fellow = InfoManager.MyPets?.Find(p => p != null && Script.IsLikelyFellow(p));
            if (fellow != null)
            {
                // Potion of Growth
                if (PetPolicy.UsePotionOfGrowth && (DateTime.UtcNow - PetPolicy.LastGrowthPotionTime).TotalSeconds >= 30)
                {
                    TryUsePotionOfGrowth(fellow);
                }
            }

            return false;
        }

        private bool TryAutoSummonAttackPet(Window w)
        {
            var chr = InfoManager.Character;
            if (chr == null || chr.Inventory == null) return false;

            for (byte s = 13; s < chr.Inventory.Capacity; s++)
            {
                var item = chr.Inventory[s];
                if (item == null || string.IsNullOrEmpty(item.ServerName)) continue;
                string sn = item.ServerName.ToUpperInvariant();

                // Saldırı peti çağırma eşyası (WOLF, BEAR, JAGUAR, FOX, BIRD, ITEM_COS_P)
                if ((item.ID2 == 1 && (item.ID3 == 7 || item.ID3 == 8) && !sn.Contains("GROWTH") && !sn.Contains("POTION"))
                    || sn.Contains("COS_P_WOLF") || sn.Contains("COS_P_BEAR") || sn.Contains("COS_P_FOX")
                    || sn.Contains("COS_P_JAGUAR") || sn.Contains("COS_P_BIRD") || sn.Contains("COS_P_TIGER")
                    || (sn.StartsWith("ITEM_COS_P_") && !sn.Contains("GROWTH") && !sn.Contains("POTION") && !sn.Contains("RABBIT") && !sn.Contains("MONKEY") && !sn.Contains("SQUIRREL") && !sn.Contains("CAT") && !sn.Contains("PIG")))
                {
                    PetPolicy.LastSummonAttemptTime = DateTime.UtcNow;
                    w.LogProcess($"Saldırı Peti: Çağrılıyor [{item.Name ?? item.ServerName}]...");
                    return PacketBuilder.UseItem(item, s);
                }
            }
            return false;
        }

        private bool TryAutoReviveAttackPet(SRCoService atkPet, Window w)
        {
            var chr = InfoManager.Character;
            if (chr == null || chr.Inventory == null || atkPet == null) return false;

            for (byte s = 13; s < chr.Inventory.Capacity; s++)
            {
                var item = chr.Inventory[s];
                if (item == null || string.IsNullOrEmpty(item.ServerName)) continue;
                string sn = item.ServerName.ToUpperInvariant();

                if (sn.Contains("REBIRTH") || sn.Contains("REVIVE") || sn.Contains("GRASS_OF_LIFE") || sn.Contains("RESURRECT"))
                {
                    PetPolicy.CurrentReviveCount++;
                    w.LogProcess($"Saldırı Peti: Canlandırılıyor [{item.Name ?? item.ServerName}] (Adet #{PetPolicy.CurrentReviveCount})...");
                    return PacketBuilder.UseItem(item, s, atkPet.UniqueID);
                }
            }
            return false;
        }

        private bool HasPetHpPotions()
        {
            var chr = InfoManager.Character;
            if (chr == null || chr.Inventory == null) return false;

            for (byte s = 13; s < chr.Inventory.Capacity; s++)
            {
                var item = chr.Inventory[s];
                if (item == null || string.IsNullOrEmpty(item.ServerName)) continue;
                string sn = item.ServerName.ToUpperInvariant();
                if (sn.Contains("RECOVERY") || sn.Contains("POTION_COS") || sn.Contains("HP_RECOVERY"))
                    return true;
            }
            return false;
        }

        private bool TryUsePotionOfGrowth(SRCoService fellow)
        {
            var chr = InfoManager.Character;
            if (chr == null || chr.Inventory == null || fellow == null) return false;

            for (byte s = 13; s < chr.Inventory.Capacity; s++)
            {
                var item = chr.Inventory[s];
                if (item == null || string.IsNullOrEmpty(item.ServerName)) continue;
                string sn = item.ServerName.ToUpperInvariant();
                if (sn.Contains("GROWTH") || sn.Contains("EXP_POTION"))
                {
                    PetPolicy.LastGrowthPotionTime = DateTime.UtcNow;
                    Window.Get?.LogProcess($"Fellow Pet: Potion of Growth kullanılıyor [{item.Name}]...");
                    return PacketBuilder.UseItem(item, s, fellow.UniqueID);
                }
            }
            return false;
        }
        #endregion

        private bool HasSpeedBuffActive()
        {
            var chr = InfoManager.Character;
            if (chr == null || chr.Buffs == null) return false;
            for (byte b = 0; b < chr.Buffs.Count; b++)
            {
                var buff = chr.Buffs.GetAt(b);
                if (buff == null) continue;
                string sn = buff.ServerName ?? "";
                string name = buff.Name ?? "";
                if (sn.IndexOf("SPEED", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("Speed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    name.IndexOf("Hız", StringComparison.OrdinalIgnoreCase) >= 0)
                    return true;
            }
            return false;
        }

        private double GetWeaponAttackRange(SRTypes.Weapon weapon)
        {
            switch (weapon)
            {
                case SRTypes.Weapon.Bow:
                    return 15.0;
                case SRTypes.Weapon.Crossbow:
                    return 14.0;
                case SRTypes.Weapon.TwoHandStaff:
                case SRTypes.Weapon.Warlock:
                case SRTypes.Weapon.Harp:
                case SRTypes.Weapon.Cleric:
                    return 14.0;
                case SRTypes.Weapon.Spear:
                case SRTypes.Weapon.Glaive:
                case SRTypes.Weapon.TwoHandSword:
                case SRTypes.Weapon.DualAxes:
                    return 4.5;
                default:
                    return 3.5;
            }
        }

        private static bool HasAnyConfiguredSkill(SRSkill[] skillshots)
        {
            if (skillshots == null || skillshots.Length == 0)
                return false;
            for (int i = 0; i < skillshots.Length; i++)
            {
                SRSkill s = skillshots[i];
                if (s != null && s.ID > 1)
                    return true;
            }
            return false;
        }

        private double GetSkillAttackRange(SRSkill skill, SRTypes.Weapon weapon)
        {
            if (skill == null || skill.ID == 1)
                return GetWeaponAttackRange(weapon);

            string sn = (skill.ServerName ?? "").ToUpperInvariant();

            // 1. Silaha bağlı skiller: Skilin birincil silah şartı varsa, menzili o silahın menzilini aşamaz (özel menzilli skiller hariç)
            if (skill.RequiredWeaponPrimary != SRTypes.Weapon.None)
            {
                // Bicheon Kılıç Dalgası (Flying Dragon) - 15m
                if (sn.Contains("_SWORD_FLY") || sn.Contains("_SWORD_SWORD"))
                    return 15.0;

                // Bow skilleri - 15m
                if (skill.RequiredWeaponPrimary == SRTypes.Weapon.Bow || sn.Contains("_BOW_"))
                    return 15.0;

                // Crossbow skilleri - 14m
                if (skill.RequiredWeaponPrimary == SRTypes.Weapon.Crossbow || sn.Contains("_CROSSBOW_"))
                    return 14.0;

                // Avrupa Büyü Silahları (Staff, Warlock rod, Harp, Cleric rod) - 14m
                if (skill.RequiredWeaponPrimary == SRTypes.Weapon.TwoHandStaff ||
                    skill.RequiredWeaponPrimary == SRTypes.Weapon.Warlock ||
                    skill.RequiredWeaponPrimary == SRTypes.Weapon.Harp ||
                    skill.RequiredWeaponPrimary == SRTypes.Weapon.Cleric)
                    return 14.0;

                // Yakın dövüş silah skilleri (Spear, Glaive, Sword, Blade, TwoHand, DualAxes, Daggers)
                return GetWeaponAttackRange(skill.RequiredWeaponPrimary);
            }

            // 2. Silah gerektirmeyen gerçek Çin Büyü Nukeleri (Cold, Lightning, Fire)
            if (sn.Contains("_COLD_GUNG") || sn.Contains("_COLD_WAVE") || sn.Contains("_COLD_CHUN") ||
                sn.Contains("_LIGHTNING_GUNG") || sn.Contains("_LIGHTNING_THUNDER") || sn.Contains("_LIGHTNING_SHOCK") ||
                sn.Contains("_FIRE_GUNG") || sn.Contains("_FIRE_WAVE"))
            {
                return 15.0;
            }

            // Avrupa Büyü Skilleri (Wizard, Warlock, Bard, Cleric)
            if (sn.Contains("_WIZARD_") || sn.Contains("_WARLOCK_") || sn.Contains("_BARD_") || sn.Contains("_CLERIC_"))
            {
                return 14.0;
            }

            // Bow & Crossbow
            if (sn.Contains("_BOW_") || sn.Contains("_CROSSBOW_"))
            {
                return 15.0;
            }

            // Bicheon Sword wave
            if (sn.Contains("_SWORD_FLY") || sn.Contains("_SWORD_SWORD"))
            {
                return 15.0;
            }

            return GetWeaponAttackRange(weapon);
        }

        private double GetEffectiveAttackRange(SRSkill[] skillshots, SRTypes.Weapon weapon)
        {
            double baseRange = GetWeaponAttackRange(weapon);
            if (skillshots == null || skillshots.Length == 0)
                return baseRange;

            double maxRange = baseRange;
            for (int i = 0; i < skillshots.Length; i++)
            {
                SRSkill s = skillshots[i];
                if (s == null || s.ID == 1) continue;
                if (!s.Enabled && !s.isUsableSkill()) continue;
                if (!s.isCastingEnabled) continue;

                double r = GetSkillAttackRange(s, weapon);
                if (r > maxRange)
                    maxRange = r;
            }
            return maxRange;
        }

        private void ExecuteKiting(SRMob mob, SRCoord myPosition)
        {
            if (mob == null || myPosition == null)
                return;

            SRTypes.Weapon weapon = GetMyWeaponType();
            bool isRanged = (weapon == SRTypes.Weapon.Bow || 
                             weapon == SRTypes.Weapon.Crossbow || 
                             weapon == SRTypes.Weapon.TwoHandStaff || 
                             weapon == SRTypes.Weapon.Warlock);

            if (!isRanged)
                return;

            SRCoord mobPos = mob.GetRealtimePosition();
            double dist = myPosition.DistanceTo(mobPos);

            // If mob gets closer than 4 meters, kite backwards
            if (dist < 4.0 && dist > 0.1)
            {
                double dx = myPosition.PosX - mobPos.PosX;
                double dy = myPosition.PosY - mobPos.PosY;
                double len = Math.Sqrt(dx * dx + dy * dy);
                if (len > 0.001)
                {
                    double stepDist = 6.0;
                    SRCoord kitePos = new SRCoord(myPosition.PosX + (dx / len) * stepDist, myPosition.PosY + (dy / len) * stepDist);
                    Window.Get?.LogProcess("Combat AI: Kiting back from mob...");
                    MoveTo(kitePos);
                    Thread.Sleep(500);
                }
            }
        }

        private bool CheckPanicEscape()
        {
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return false;

            byte hpSlot = 0;
            bool hasHpPot = FindItem(3, 1, 1, ref hpSlot);
            double hpPercent = InfoManager.Character.HPMax > 0 ? ((double)InfoManager.Character.HP / InfoManager.Character.HPMax * 100.0) : 100.0;

            // HP critical (< 22%) and no HP pots left
            if (hpPercent < 22.0 && !hasHpPot)
            {
                byte scrollSlot = 0;
                if (FindItem(3, 3, 1, ref scrollSlot) || FindItem(3, 3, 2, ref scrollSlot) || FindItem(3, 3, 3, ref scrollSlot))
                {
                    Window.Get?.Log("Combat AI: EMERGENCY! HP critical and no HP potions! Using Return Scroll...");
                    SRItem scrollItem = InfoManager.Character.Inventory[scrollSlot];
                    PacketBuilder.UseItem(scrollItem, scrollSlot);
                    SleepInterruptible(4000);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Party otomasyonunu botting döngüsünde periyodik çalıştırır.
        /// (Önceden CheckAutoParty/CheckPartyLeaving/MatchAutoReform sadece
        /// login'de 1 kez koşuyordu; davet/reform beklentisi tick'te yoktu.)
        /// </summary>
        private void CheckPartyAutoTick()
        {
            try
            {
                if (!InfoManager.inGame)
                    return;
                Window w = Window.Get;
                if (w == null)
                    return;
                DateTime now = DateTime.Now;
                if ((now - m_lastPartyAutoTick).TotalSeconds < 10.0)
                    return;
                m_lastPartyAutoTick = now;
                try { CheckPartyLeaving(); } catch { }
                try { CheckAutoParty(); } catch { }
                bool autoReform = false;
                try { w.Party_cbxMatchAutoReform.InvokeIfRequired(() => { autoReform = w.Party_cbxMatchAutoReform.Checked; }); } catch { }
                if (autoReform && (now - m_lastPartyMatchReform).TotalMinutes >= 1.0)
                {
                    m_lastPartyMatchReform = now;
                    try { CheckPartyMatchAutoReform(); } catch { }
                }
            }
            catch { }
        }

        public bool ExecuteStorageDeposit(uint npcUniqueID)
        {
            Window w = Window.Get;
            if (InfoManager.Character == null || InfoManager.Character.Storage == null)
                return false;

            var inv = InfoManager.Character.Inventory;
            var storage = InfoManager.Character.Storage;
            bool completed = true;

            for (byte slot = 13; slot < inv.Capacity && isBotting; slot++)
            {
                var item = inv[slot];
                if (item == null)
                    continue;

				if (ItemFilterManager.ShouldStore(item))
                {
                    int emptyStorageSlot = -1;
                    for (int s = 0; s < storage.Capacity; s++)
                    {
                        if (storage[s] == null)
                        {
                            emptyStorageSlot = s;
                            break;
                        }
                    }

                    if (emptyStorageSlot != -1)
                    {
                        w.Log($"Depositing [{item.Name}] x{item.Quantity} (inv:{slot} -> stor:{emptyStorageSlot})...");
                        InfoManager.MonitorInventoryMovement.Reset();
                        PacketBuilder.MoveStorageItem(slot, (byte)emptyStorageSlot,
                            SRTypes.InventoryItemMovement.InventoryToStorage, npcUniqueID);
                        // Server yankısı (0xB034) gelmezse paket reddedilmiş demektir:
                        // üst üste göndermek kick yedirir, o yüzden dur.
                        if (!WaitForInventoryMovementAck(2000))
                        {
                            w.Log($"STORE: [{item.Name}] server tarafından kabul edilmedi (yankı yok) — depozit durduruldu.");
                            completed = false;
                            break;
                        }
                        Thread.Sleep(350);
                    }
                    else
                    {
                        w.LogProcess("Town Loop: Storage is full!", Window.ProcessState.Warning);
                        completed = false;
                        break;
                    }
                }
            }

            // Unload and store items from active Pick Pet
            if (completed && InfoManager.MyPets != null)
            {
                SRCoService pickPet = InfoManager.MyPets.Find(p => p != null && p.isPickPet() && p.Inventory != null);
                if (pickPet != null && pickPet.Inventory != null)
                {
                    for (byte pSlot = 0; pSlot < pickPet.Inventory.Capacity && isBotting; pSlot++)
                    {
                        var pItem = pickPet.Inventory[pSlot];
                        if (pItem == null)
                            continue;

                        if (ItemFilterManager.ShouldStore(pItem))
                        {
                            int emptyStorageSlot = -1;
                            for (int s = 0; s < storage.Capacity; s++)
                            {
                                if (storage[s] == null)
                                {
                                    emptyStorageSlot = s;
                                    break;
                                }
                            }

                            if (emptyStorageSlot == -1)
                            {
                                w.LogProcess("Town Loop: Storage is full for pet items!", Window.ProcessState.Warning);
                                completed = false;
                                break;
                            }

                            // Find empty slot in character inventory to intermediate pet to storage
                            int emptyCharSlot = inv.FindIndex(i => i == null, 13);
                            if (emptyCharSlot != -1)
                            {
                                w.Log($"Transferring [{pItem.Name}] from pet slot {pSlot} to inventory...");
                                InfoManager.MonitorInventoryMovement.Reset();
                                PacketBuilder.MoveItem(pSlot, (byte)emptyCharSlot, SRTypes.InventoryItemMovement.PetToInventory, pickPet.UniqueID);
                                if (!WaitForInventoryMovementAck(2000))
                                {
                                    w.Log($"STORE: pet transferi kabul edilmedi — depozit durduruldu.");
                                    completed = false;
                                    break;
                                }
                                Thread.Sleep(350);

                                w.Log($"Depositing [{pItem.Name}] x{pItem.Quantity} (inv:{emptyCharSlot} -> stor:{emptyStorageSlot})...");
                                InfoManager.MonitorInventoryMovement.Reset();
                                PacketBuilder.MoveStorageItem((byte)emptyCharSlot, (byte)emptyStorageSlot,
                                    SRTypes.InventoryItemMovement.InventoryToStorage, npcUniqueID);
                                if (!WaitForInventoryMovementAck(2000))
                                {
                                    w.Log($"STORE: [{pItem.Name}] server tarafından kabul edilmedi (yankı yok) — depozit durduruldu.");
                                    completed = false;
                                    break;
                                }
                                Thread.Sleep(350);
                            }
                            else
                            {
                                w.LogProcess("Town Loop: Character inventory full while transferring pet items.", Window.ProcessState.Warning);
                                completed = false;
                                break;
                            }
                        }
                    }
                }
            }

            ReportUnhandledFilterItems(w);
            return completed && isBotting && InfoManager.inGame && Proxy != null && Proxy.isRunning;
        }

		/// <summary>
		/// Trade rotasındaki her waypoint sonrası taşıt mesafesini ve core'da
		/// doğrulanan TypeID4=2 spawn thief NPC'lerini kontrol eder.
		/// </summary>
		internal void HandleTradeWaypoint()
		{
			if (!TradeLoopManager.IsRunning || !isBotting || InfoManager.Character == null)
				return;
			SRCoService transport = InfoManager.MyPets.Find(pet => pet != null && pet.isTransport());
			if (transport == null) return;
			try
			{
				double transportDistance = InfoManager.Character.GetRealtimePosition().DistanceTo(transport.GetRealtimePosition());
				if (transportDistance > 25.0)
				{
					Window.Get.LogProcess($"Trade: taşıt geride kaldı ({transportDistance:F0}m), geri dönülüyor...");
					WaitMovement(transport.GetRealtimePosition(), 8);
				}
			}
			catch { }

			if (!TradeLoopManager.AttackSpawnedThieves || TradeLoopManager.MountMode == TradeMountMode.StayMounted)
				return;
			SRMob firstThief = FindTradeThiefNearTransport(transport);
			if (firstThief == null) return;
			if (InfoManager.Character.isRiding)
			{
				while (InfoManager.MonitorPetMountResponse.WaitOne(0)) { }
				PacketBuilder.SetPetMounted(transport.UniqueID, false);
				for (int i = 0; i < 25 && isBotting && InfoManager.Character.isRiding; i++)
					InfoManager.MonitorPetMountResponse.WaitOne(100);
				if (InfoManager.Character.isRiding)
				{
					Window.Get.Log("Trade: thief saldırısı için taşıttan inilemedi.", Theme.LogLevel.Warning);
					return;
				}
			}
			int guard = 12;
			while (guard-- > 0 && isBotting && TradeLoopManager.IsRunning)
			{
				SRMob thief = FindTradeThiefNearTransport(transport);
				if (thief == null) break;
				double best = transport.GetRealtimePosition().DistanceTo(thief.GetRealtimePosition());
				Window.Get.LogProcess($"Trade: spawn thief hedefleniyor [{thief.Name}] ({best:F0}m)...");
				try
				{
					double playerDistance = InfoManager.Character.GetRealtimePosition().DistanceTo(thief.GetRealtimePosition());
					if (playerDistance > GetWeaponAttackRange(GetMyWeaponType()))
						WaitMovement(thief.GetRealtimePosition(), 5);
				}
				catch { }
				PacketBuilder.SelectEntity(thief.UniqueID);
				InfoManager.MonitorEntitySelected.WaitOne(100);
				int attackGuard = 40;
				while (attackGuard-- > 0 && isBotting && InfoManager.Mobs.ContainsKey(thief.UniqueID))
				{
					if (!TryFallbackAttack(thief, Window.Get)) Thread.Sleep(250);
				}
				if (InfoManager.Mobs.ContainsKey(thief.UniqueID))
				{
					Window.Get.Log("Trade: thief saldırı zaman aşımı; rota devam ediyor.", Theme.LogLevel.Warning);
					break;
				}
			}
			if (TradeLoopManager.MountMode == TradeMountMode.Remount && isBotting
				&& !InfoManager.Character.isRiding && InfoManager.MyPets.ContainsKey(transport.UniqueID))
			{
				while (InfoManager.MonitorPetMountResponse.WaitOne(0)) { }
				PacketBuilder.SetPetMounted(transport.UniqueID, true);
				for (int i = 0; i < 25 && isBotting && !InfoManager.Character.isRiding; i++)
					InfoManager.MonitorPetMountResponse.WaitOne(100);
			}
		}

		private static SRMob FindTradeThiefNearTransport(SRCoService transport)
		{
			SRMob thief = null;
			double best = TradeLoopManager.AttackRadius;
			foreach (SRMob mob in InfoManager.Mobs.Snapshot())
			{
				if (mob == null || mob.ID4 != 2 || mob.HP == 0) continue;
				double distance;
				try { distance = transport.GetRealtimePosition().DistanceTo(mob.GetRealtimePosition()); }
				catch { continue; }
				if (distance <= best) { best = distance; thief = mob; }
			}
			return thief;
		}

		private bool WaitForInventoryMovementAck(int timeoutMilliseconds)
		{
			int waited = 0;
			while (waited < timeoutMilliseconds && isBotting && InfoManager.inGame
				&& Proxy != null && Proxy.isRunning)
			{
				int slice = Math.Min(100, timeoutMilliseconds - waited);
				if (InfoManager.MonitorInventoryMovement.WaitOne(slice))
					return true;
				waited += slice;
			}
			return false;
		}

		private int CountEmptyInventorySlots()
		{
			if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
				return 0;
			int count = 0;
			var inventory = InfoManager.Character.Inventory;
			for (int slot = 13; slot < inventory.Capacity; slot++)
				if (inventory[slot] == null) count++;
			return count;
		}

		public void ExecuteStorageTake(uint npcUniqueID)
		{
			Window w = Window.Get;
			if (InfoManager.Character == null || InfoManager.Character.Inventory == null
				|| InfoManager.Character.Storage == null)
				return;
			var inventory = InfoManager.Character.Inventory;
			var storage = InfoManager.Character.Storage;
			int keepEmpty = Math.Max(0, ItemFilterManager.Pick.StorageTakeKeepEmptySlots);
			for (int storageSlot = 0; storageSlot < storage.Capacity && isBotting; storageSlot++)
			{
				SRItem item = storage[storageSlot];
				if (item == null || !ItemFilterManager.ShouldTakeStorage(item))
					continue;
				if (CountEmptyInventorySlots() <= keepEmpty)
				{
					w.LogProcess($"Storage Take: envanterde {keepEmpty} boş slot korunuyor.");
					break;
				}
				int inventorySlot = inventory.FindIndex(value => value == null, 13);
				if (inventorySlot < 0)
					break;
				w.Log($"Storage Take: [{item.Name}] x{item.Quantity} (storage:{storageSlot} -> inv:{inventorySlot})...");
				InfoManager.MonitorInventoryMovement.Reset();
				PacketBuilder.MoveStorageItem((byte)storageSlot, (byte)inventorySlot,
					SRTypes.InventoryItemMovement.StorageToInventory, npcUniqueID);
				if (!WaitForInventoryMovementAck(2500))
				{
					w.LogProcess("Storage Take: hareket onayı gelmedi; işlem durduruldu.", Window.ProcessState.Warning);
					break;
				}
				if (!SleepInterruptible(400)) break;
			}
		}

		public void ExecuteGuildStorageDeposit(uint npcUniqueID)
		{
			Window w = Window.Get;
			if (!InfoManager.inGuild || InfoManager.Guild == null || InfoManager.Guild.Storage == null
				|| InfoManager.Character == null || InfoManager.Character.Inventory == null)
				return;

			var inventory = InfoManager.Character.Inventory;
			var storage = InfoManager.Guild.Storage;
			for (byte slot = 13; slot < inventory.Capacity && isBotting; slot++)
			{
				SRItem item = inventory[slot];
				if (item == null || !ItemFilterManager.ShouldStoreGuild(item))
					continue;
				int emptySlot = storage.FindIndex(value => value == null, 0);
				if (emptySlot < 0)
				{
					w.LogProcess("Guild Storage is full!", Window.ProcessState.Warning);
					break;
				}
				w.Log($"Guild Storage: [{item.Name}] x{item.Quantity} (inv:{slot} -> guild:{emptySlot})...");
				InfoManager.MonitorInventoryMovement.Reset();
				PacketBuilder.MoveStorageItem(slot, (byte)emptySlot, SRTypes.InventoryItemMovement.InventoryToGuild,
					npcUniqueID);
				if (!WaitForInventoryMovementAck(2500))
				{
					w.LogProcess("Guild Storage: hareket onayı gelmedi; işlem güvenli biçimde durduruldu.", Window.ProcessState.Warning);
					break;
				}
				if (!SleepInterruptible(400))
					break;
			}
		}

		public void ExecuteGuildStorageTake(uint npcUniqueID)
		{
			Window w = Window.Get;
			if (!InfoManager.inGuild || InfoManager.Guild == null || InfoManager.Guild.Storage == null
				|| InfoManager.Character == null || InfoManager.Character.Inventory == null)
				return;
			var inventory = InfoManager.Character.Inventory;
			var storage = InfoManager.Guild.Storage;
			int keepEmpty = Math.Max(0, ItemFilterManager.Pick.StorageTakeKeepEmptySlots);
			for (int storageSlot = 0; storageSlot < storage.Capacity && isBotting; storageSlot++)
			{
				SRItem item = storage[storageSlot];
				if (item == null || !ItemFilterManager.ShouldTakeGuildStorage(item))
					continue;
				if (CountEmptyInventorySlots() <= keepEmpty)
				{
					w.LogProcess($"Guild Storage Take: envanterde {keepEmpty} boş slot korunuyor.");
					break;
				}
				int inventorySlot = inventory.FindIndex(value => value == null, 13);
				if (inventorySlot < 0) break;
				w.Log($"Guild Storage Take: [{item.Name}] x{item.Quantity} (guild:{storageSlot} -> inv:{inventorySlot})...");
				InfoManager.MonitorInventoryMovement.Reset();
				PacketBuilder.MoveStorageItem((byte)storageSlot, (byte)inventorySlot,
					SRTypes.InventoryItemMovement.GuildToInventory, npcUniqueID);
				if (!WaitForInventoryMovementAck(2500))
				{
					w.LogProcess("Guild Storage Take: hareket onayı gelmedi; işlem durduruldu.", Window.ProcessState.Warning);
					break;
				}
				if (!SleepInterruptible(400)) break;
			}
		}

		public void ExecuteGuildStorageGold()
		{
			Window w = Window.Get;
			var options = ItemFilterManager.StoreGold;
			if (options == null || (!options.Enabled && !options.HasEnabledAction) || InfoManager.Character == null
				|| InfoManager.Guild == null || !isBotting)
				return;
			try
			{
				ulong inventoryGold = InfoManager.Character.Gold;
				ulong guildGold = InfoManager.Guild.StorageGold;
				if (options.StoreGoldInGuildStorage && inventoryGold > options.GoldKeepAmount)
				{
					ulong amount = inventoryGold - options.GoldKeepAmount;
					if (options.StoreGoldGuildMax > 0)
					{
						ulong available = guildGold >= options.StoreGoldGuildMax
							? 0 : options.StoreGoldGuildMax - guildGold;
						if (amount > available)
							amount = available;
					}
					if (amount > 0)
					{
						w.Log($"Guild Storage: {amount} gold depolanıyor...");
						InfoManager.MonitorInventoryMovement.Reset();
						PacketBuilder.MoveGold(SRTypes.InventoryItemMovement.InventoryGoldToGuild, amount);
						if (!WaitForInventoryMovementAck(2500))
							return;
						SleepInterruptible(400);
					}
				}

				if (options.TakeGoldFromGuildStorage)
				{
					inventoryGold = InfoManager.Character.Gold;
					guildGold = InfoManager.Guild.StorageGold;
					if (inventoryGold < options.GoldKeepAmount && guildGold > 0)
					{
						ulong need = options.GoldKeepAmount - inventoryGold;
						ulong amount = need < guildGold ? need : guildGold;
						w.Log($"Guild Storage: {amount} gold alınıyor...");
						InfoManager.MonitorInventoryMovement.Reset();
						PacketBuilder.MoveGold(SRTypes.InventoryItemMovement.GuildGoldToInventory, amount);
						if (!WaitForInventoryMovementAck(2500))
							return;
						SleepInterruptible(400);
					}
				}
			}
			catch (Exception ex)
			{
				w.LogProcess("Guild Storage Gold hatası: " + ex.Message, Window.ProcessState.Warning);
			}
		}

        /// <summary>
        /// Town scriptte henüz ilgili servis adımına ulaşmamış StoreGuild öğelerini
        /// ve paket desteği bulunmayan dismantle adaylarını raporlar.
        /// </summary>
        private void ReportUnhandledFilterItems(Window w)
        {
            if (!isBotting || InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return;
            try
            {
                int guildCount = 0, dismantleCount = 0;
                var inv = InfoManager.Character.Inventory;
                for (int i = 13; i < inv.Capacity; i++)
                {
                    var item = inv[i];
                    if (item == null)
                        continue;
                    try
                    {
                        if (ItemFilterManager.ShouldStoreGuild(item))
                            guildCount++;
                        else if (ItemFilterManager.ShouldDismantle(item))
                            dismantleCount++;
                    }
                    catch { }
                }
				if (guildCount > 0)
					w.Log($"StoreGuild: {guildCount} eşya DoGuildStorage adımını bekliyor.");
                if (dismantleCount > 0)
                    w.Log($"Dismantle: {dismantleCount} eşya söküm istiyor (bu sürümde desteklenmiyor, çantada bırakıldı).");
            }
            catch { }
        }

        /// <summary>
        /// Store Gold sekmesi: depo penceresi AÇIKKEN çağrılmalı.
        /// </summary>
        public void ExecuteStoreGold()
        {
            Window w = Window.Get;
            var opt = ItemFilterManager.StoreGold;
            if (opt == null || (!opt.Enabled && !opt.HasEnabledAction))
                return;
            if (InfoManager.Character == null || !isBotting)
                return;
            try
            {
                ulong invGold = InfoManager.Character.Gold;
                ulong storGold = InfoManager.Character.StorageGold;
				w.Log($"Store Gold: envanter={invGold}, depo={storGold}, korunacak={opt.GoldKeepAmount}, depo-maksimum={opt.StoreGoldMax}.");

                // 1. Depoya altın koy (keep üstü).
                if (opt.ShouldStoreInPersonalStorage && invGold > opt.GoldKeepAmount)
                {
                    ulong amount = invGold - opt.GoldKeepAmount;
                    // Ayrı "Store gold" aksiyonu seçilmediyse yanındaki maksimum
                    // alanı uygulanmaz; Gold keep amount bütün fazlayı yatırır.
                    if (opt.StoreGoldInStorage && opt.StoreGoldMax > 0 && storGold + amount > opt.StoreGoldMax)
                    {
                        if (storGold >= opt.StoreGoldMax)
                            amount = 0;
                        else
                            amount = opt.StoreGoldMax - storGold;
                    }
                    if (amount > 0)
                    {
                        w.Log($"Store Gold: depoya {amount} altın konuyor...");
                        InfoManager.MonitorInventoryMovement.Reset();
                        PacketBuilder.MoveGold(SRTypes.InventoryItemMovement.InventoryGoldToStorage, amount);
                        if (!WaitForInventoryMovementAck(2000))
                            return;
                        Thread.Sleep(400);
                    }
					else
					{
						w.Log($"Store Gold: depo maksimumuna ulaşıldığı için işlem yok ({storGold} / {opt.StoreGoldMax}).");
					}
                }
                else if (opt.ShouldStoreInPersonalStorage)
				{
					w.Log($"Store Gold: envanter altını korunacak miktarı aşmadığı için işlem yok ({invGold} <= {opt.GoldKeepAmount}).");
				}

                // 2. Depodan altın al (keep altı).
                if (opt.TakeGoldFromStorage)
                {
                    invGold = InfoManager.Character.Gold;
                    storGold = InfoManager.Character.StorageGold;
                    if (invGold < opt.GoldKeepAmount && storGold > 0)
                    {
                        ulong need = opt.GoldKeepAmount - invGold;
                        ulong amount = need < storGold ? need : storGold;
                        if (amount > 0)
                        {
                            w.Log($"Store Gold: depodan {amount} altın alınıyor...");
                            InfoManager.MonitorInventoryMovement.Reset();
                            PacketBuilder.MoveGold(SRTypes.InventoryItemMovement.StorageGoldToInventory, amount);
                            if (!WaitForInventoryMovementAck(2000))
                                return;
                            Thread.Sleep(400);
                        }
                    }
                }

                if (opt.TakeGoldFromGuildStorage || opt.StoreGoldInGuildStorage)
                    w.Log("Store Gold: guild deposu altını bu sürümde desteklenmiyor, atlandı.");
            }
            catch (Exception ex)
            {
                w.LogProcess("Store Gold hatası: " + ex.Message, Window.ProcessState.Warning);
            }
        }

        public void ExecuteSellTrash()
        {
            Window w = Window.Get;
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return;

            var inv = InfoManager.Character.Inventory;

            for (byte slot = 13; slot < inv.Capacity && isBotting; slot++)
            {
                var item = inv[slot];
                if (item == null)
                    continue;

                // Sell white/normal armor, weapon or shield
				bool isDefaultTrash = item is SREquipable eq
					&& eq.GetRarity() == SREquipable.Rarity.None
					&& !eq.isJob() && !eq.isAvatar();

				// "Do not sell items with plus >=" koruması (kural + varsayılan satışı da kapsar).
				if (ItemFilterManager.Pick.NoSellPlusEnabled && item is SREquipable eqPlus && eqPlus.Plus >= ItemFilterManager.Pick.NoSellPlus)
					continue;

				// Explicit sell rules apply to every inventory item. Store has
				// precedence when both flags are enabled for the same rule.
				if (!ItemFilterManager.ShouldStore(item)
					&& (ItemFilterManager.ShouldSell(item) || isDefaultTrash))
                {
                    w.LogProcess($"Selling trash [{item.Name}] to NPC...");
                    PacketBuilder.MoveItem(slot, 0, SRTypes.InventoryItemMovement.InventoryToShop, item.Quantity);
                    Thread.Sleep(300);
                }
            }
        }
        private void BuffLoop()
        {
            Window w = Window.Get;
            SRSkill[] buffs = w.Skills_GetBuffs(SRMob.Mob.General);
            if (buffs == null || buffs.Length == 0)
                return;

            for (int i = 0; i < buffs.Length; i++)
            {
                SRSkill buff = buffs[i];
                if (buff == null || !buff.isCastingEnabled)
                    continue;

                // Karakterde bu buff zaten aktif mi kontrol et
                bool hasBuff = false;
                if (InfoManager.Character != null && InfoManager.Character.Buffs != null)
                {
                    hasBuff = InfoManager.Character.Buffs.ContainsKey(buff.GroupID);
                }

                if (!hasBuff)
                {
                    w.LogProcess("Casting buff: " + buff.Name + " (" + buff.CastingTime + "ms)...");
                    CheckWeaponSwitch(buff);
                    PacketBuilder.CastSkill(buff.ID);
                    Thread.Sleep(Math.Max(500, buff.CastingTime + 100));
                }
            }

            // Revert back to primary attack weapon if weapon was switched for buffs
            EnsureMainWeapon();

            // Devil Spirit: Check activation conditions during buff cycle
            if (SkillManager.IsDevilSpiritEnabled)
            {
                SkillManager.CheckDevilSpirit(m_cachedMobsInRange);
            }
        }

        /// <summary>
        /// Pick Filter: yerdeki toplanabilir eşya var mı? (Pick items first için hızlı tarama)
        /// Karakter listesi (Pick) VEYA pet listesi (Pet) doluysa true.
        /// </summary>
        private bool HasLootableDrops()
        {
            try
            {
                if (InfoManager.Character == null)
                    return false;
                if (!ItemFilterManager.IsFilterActive)
                    return false;
                SRCoord myPos = InfoManager.Character.GetRealtimePosition();
                if (myPos == null)
                    return false;
                for (int i = 0; i < InfoManager.Entities.Count; i++)
                {
                    SREntity entity = null;
                    try { entity = InfoManager.Entities.GetAt(i); } catch { continue; }
                    if (entity is SRDrop drop)
                    {
                        try
                        {
                            if (drop.GetRealtimePosition().DistanceTo(myPos) > 30.0)
                                continue;
                            if (ItemFilterManager.ShouldPickup(drop))
                                return true;
                            if (IsPetWanted(drop))
                                return true;
                        }
                        catch { }
                    }
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Bu damla pet listesinde mi? (Pet=Yes ve pet müsait/dolu değil)
        /// </summary>
        private bool IsPetWanted(SRDrop drop)
        {
            try
            {
                if (drop == null || !ItemFilterManager.Pick.UsePickPet)
                    return false;
                SRCoService pet = null;
                try { pet = InfoManager.MyPets.Find(p => p.isPickPet()); } catch { }
                if (pet == null)
                    return false;
                bool full = false;
                try
                {
                    full = pet.Inventory == null
                        || pet.Inventory.FindIndex(item => item == null, 0) == -1;
                }
                catch { full = false; }
                return ItemFilterManager.ShouldUsePet(drop, true, full);
            }
            catch { return false; }
        }

        /// <summary>
        /// Asenkron Pet Toplama Döngüsü.
        /// Karakter savaşırken, skill basarken veya hareket ederken pet'in
        /// yerdeki eşyaları gecikmesiz, anında ve bağımsız olarak toplamasını sağlar.
        /// </summary>
        private void ThreadPetLooting()
        {
            while (!m_stopPetLootingRequested)
            {
                try
                {
                    if (!InfoManager.inGame || InfoManager.Character == null)
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    if (!ItemFilterManager.Pick.UsePickPet || !ItemFilterManager.IsFilterActive || ItemFilterManager.Pick.DontPickItems)
                    {
                        Thread.Sleep(300);
                        continue;
                    }

                    SRCoService pickPet = null;
                    try { pickPet = InfoManager.MyPets.Find(p => p != null && p.isPickPet()); } catch { }
                    if (pickPet == null)
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    bool petFull = false;
                    try
                    {
                        petFull = pickPet.Inventory == null
                            || pickPet.Inventory.FindIndex(item => item == null, 0) == -1;
                    }
                    catch { petFull = false; }

                    if (petFull)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }

                    SRCoord center = InfoManager.Character.GetRealtimePosition();
                    if (center == null)
                    {
                        Thread.Sleep(200);
                        continue;
                    }

                    int pickRadius = 45;
                    try
                    {
                        if (Window.Get != null)
                            pickRadius = Window.Get.TrainingArea_GetPickRadius();
                    }
                    catch { }
                    if (pickRadius < 30) pickRadius = 45;

                    DateTime now = DateTime.UtcNow;

                    // 4 saniyeden eski veya artık çevrede olmayan komutları hafızadan düşür
                    lock (_petCommandedDrops)
                    {
                        List<uint> cleanup = null;
                        foreach (var kvp in _petCommandedDrops)
                        {
                            if ((now - kvp.Value).TotalSeconds > 3.0 || !InfoManager.isEntityNear(kvp.Key))
                            {
                                if (cleanup == null) cleanup = new List<uint>();
                                cleanup.Add(kvp.Key);
                            }
                        }
                        if (cleanup != null)
                        {
                            for (int c = 0; c < cleanup.Count; c++)
                                _petCommandedDrops.Remove(cleanup[c]);
                        }
                    }

                    // Yerdeki dropları tara
                    List<SRDrop> petDrops = new List<SRDrop>();
                    for (int i = 0; i < InfoManager.Entities.Count; i++)
                    {
                        SREntity entity = null;
                        try { entity = InfoManager.Entities.GetAt(i); } catch { continue; }
                        if (entity is SRDrop drop)
                        {
                            try
                            {
                                if (!InfoManager.isEntityNear(drop.UniqueID))
                                    continue;

                                double dist = drop.GetRealtimePosition().DistanceTo(center);
                                if (dist <= pickRadius)
                                {
                                    if (ItemFilterManager.ShouldUsePet(drop, true, petFull))
                                    {
                                        lock (_petCommandedDrops)
                                        {
                                            if (!_petCommandedDrops.ContainsKey(drop.UniqueID))
                                                petDrops.Add(drop);
                                        }
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    if (petDrops.Count > 0)
                    {
                        // En yakından başlayarak pet'e komut ver
                        petDrops.Sort((a, b) => a.GetRealtimePosition().DistanceTo(center).CompareTo(b.GetRealtimePosition().DistanceTo(center)));

                        for (int d = 0; d < petDrops.Count; d++)
                        {
                            if (m_stopPetLootingRequested) break;
                            SRDrop drop = petDrops[d];
                            if (!InfoManager.isEntityNear(drop.UniqueID)) continue;

                            lock (_petCommandedDrops)
                            {
                                _petCommandedDrops[drop.UniqueID] = DateTime.UtcNow;
                            }

                            PacketBuilder.PickUpItem(drop.UniqueID, pickPet.UniqueID);
                            Thread.Sleep(100);
                        }
                    }
                }
                catch { }

                Thread.Sleep(100);
            }
        }

        private void LootDrops(SRCoord trainingPosition, int trainingRadius)
        {
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return;

            if (!ItemFilterManager.IsFilterActive || ItemFilterManager.Pick.DontPickItems)
                return;

            // Çantada boş yer var mı? (13. slottan itibaren)
            int emptySlot = InfoManager.Character.Inventory.FindIndex(i => i == null, 13);
            if (emptySlot == -1 && !ItemFilterManager.Pick.PickEvenWhenFull)
                return; // Envanter dolu

            SRCoord myPos = InfoManager.Character.GetRealtimePosition();
            if (myPos == null) return;

            bool wantPet = ItemFilterManager.Pick.UsePickPet;
            SRCoService pickPet = wantPet ? InfoManager.MyPets.Find(p => p.isPickPet()) : null;
            uint petId = pickPet != null ? pickPet.UniqueID : 0;
            bool petFull = false;
            if (pickPet != null)
            {
                try
                {
                    petFull = pickPet.Inventory == null
                        || pickPet.Inventory.FindIndex(item => item == null, 0) == -1;
                }
                catch { petFull = false; }
            }

            int pickRadius = Window.Get != null ? Window.Get.TrainingArea_GetPickRadius() : 45;
            List<SRDrop> drops = new List<SRDrop>();

            for (int i = 0; i < InfoManager.Entities.Count; i++)
            {
                SREntity entity = null;
                try { entity = InfoManager.Entities.GetAt(i); } catch { continue; }
                if (entity is SRDrop drop)
                {
                    try
                    {
                        if (!InfoManager.isEntityNear(drop.UniqueID))
                            continue;

                        double dist = drop.GetRealtimePosition().DistanceTo(myPos);
                        if (dist <= pickRadius)
                        {
                            bool charWantsDrop = ItemFilterManager.ShouldPickup(drop);
                            bool petWantsDrop = petId != 0 && ItemFilterManager.ShouldUsePet(drop, true, petFull);
                            if (!charWantsDrop && !petWantsDrop)
                                continue;

                            // Ok/mermi kotası dolduysa yerden alma.
                            if (drop.ID2 == 3 && drop.ID3 == 4 && ItemFilterManager.Pick.PickArrowsBolts)
                            {
                                int have = CountItemTotalQuantity(3, 4, 0);
                                if (have >= ItemFilterManager.Pick.ArrowBoltAmount)
                                    continue;
                            }

                            drops.Add(drop);
                        }
                    }
                    catch { }
                }
            }

            if (drops.Count == 0)
                return;

            // En yakından uzağa sırala
            drops.Sort((a, b) => a.GetRealtimePosition().DistanceTo(myPos).CompareTo(b.GetRealtimePosition().DistanceTo(myPos)));

            for (int d = 0; d < drops.Count && d < 15; d++)
            {
                if (!isBotting || m_stopBottingRequested) break;
                SRDrop drop = drops[d];
                if (!InfoManager.isEntityNear(drop.UniqueID))
                    continue;

                var dropRule = ItemFilterManager.GetRule(drop.Name) ?? ItemFilterManager.GetRule(drop.ServerName);
                bool petOnly = dropRule != null && dropRule.Pet && !dropRule.Pickup;
                bool petWants = petId != 0 && ItemFilterManager.ShouldUsePet(drop, true, petFull);
                bool charWants = ItemFilterManager.ShouldPickup(drop);
                if (petOnly && !petWants && ItemFilterManager.Pick.PickWithCharIfPetGoneFull)
                    charWants = true;

                // Pet toplayabiliyorsa pet'e komut ver
                if (petWants)
                {
                    lock (_petCommandedDrops)
                    {
                        _petCommandedDrops[drop.UniqueID] = DateTime.UtcNow;
                    }
                    PacketBuilder.PickUpItem(drop.UniqueID, petId);
                }

                // Karakter yalnızca pet alamıyorsa veya pet yoksa/doluysa yürüyüp toplasın
                // (Pet toplayacaksa karakterin savaşı durdurup yürümesine gerek yok)
                bool needCharMove = charWants && (!petWants || pickPet == null || petFull);
                if (needCharMove)
                {
                    if (!InfoManager.isEntityNear(drop.UniqueID))
                        continue;
                    double dist = myPos.DistanceTo(drop.GetRealtimePosition());
                    if (dist > 3.0)
                    {
                        if (CollisionPolicy.NavigateAroundObstaclesWhilePicking)
                        {
                            bool reached = ApproachTargetWithCollision(drop.GetRealtimePosition(), 3.0, null);
                            if (!reached && CollisionPolicy.EnableCollisionInTrainingArea)
                            {
                                Window.Get?.LogProcess($"Eşyaya ({drop.Name}) giderken engel aşılamadı, atlanıyor...");
                                continue;
                            }
                        }
                        else
                        {
                            MoveTo(drop.GetRealtimePosition());
                            Thread.Sleep(200);
                        }
                    }
                    PacketBuilder.PickUpItem(drop.UniqueID);
                    Thread.Sleep(150);
                }
            }
        }

        private int CountItemTotalQuantity(byte tid2, byte tid3, byte tid4 = 0)
        {
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return 0;

            int total = 0;
            var inv = InfoManager.Character.Inventory;
            for (int i = 13; i < inv.Capacity; i++)
            {
                var item = inv[i];
                if (item != null && item.ID1 == 3 && item.ID2 == tid2 && item.ID3 == tid3 && (tid4 == 0 || item.ID4 == tid4))
                {
                    total += item.Quantity;
                }
            }
            return total;
        }

        public void ExecuteAutoBuyPotions(SREntity potionNpc)
        {
            Window w = Window.Get;
            if (w == null || w.Town_cbxAutoBuy == null || !w.Town_cbxAutoBuy.Checked || potionNpc == null)
                return;

            // 1. HP Potion
            int currentHp = CountItemTotalQuantity(3, 1, 1);
            int targetHp = 150;
            w.Town_nudHpAmount.InvokeIfRequired(() => { targetHp = (int)w.Town_nudHpAmount.Value; });
            int missingHp = targetHp - currentHp;

            int hpIndex = 3; // default Large
            w.Town_cmbxHpType.InvokeIfRequired(() => { hpIndex = w.Town_cmbxHpType.SelectedIndex; });
            if (hpIndex < 0 || hpIndex > 4) hpIndex = 3;

            if (missingHp > 0)
            {
                byte slotInShop = (byte)hpIndex;
                if (!TryBuyVerifiedShopItem(potionNpc, 0, slotInShop, (ushort)missingHp, 3, 1, 1, "HP potion"))
                    return;
                Thread.Sleep(600);
            }

            // 2. MP Potion
            int currentMp = CountItemTotalQuantity(3, 1, 2);
            int targetMp = 150;
            w.Town_nudMpAmount.InvokeIfRequired(() => { targetMp = (int)w.Town_nudMpAmount.Value; });
            int missingMp = targetMp - currentMp;

            int mpIndex = 3; // default Large
            w.Town_cmbxMpType.InvokeIfRequired(() => { mpIndex = w.Town_cmbxMpType.SelectedIndex; });
            if (mpIndex < 0 || mpIndex > 4) mpIndex = 3;

            if (missingMp > 0)
            {
                byte slotInShop = (byte)(5 + mpIndex);
                if (!TryBuyVerifiedShopItem(potionNpc, 0, slotInShop, (ushort)missingMp, 3, 1, 2, "MP potion"))
                    return;
                Thread.Sleep(600);
            }

            // 3. Universal Pills
            bool buyPills = false;
            w.Town_cbxBuyPills.InvokeIfRequired(() => { buyPills = w.Town_cbxBuyPills.Checked; });
            if (buyPills)
            {
                int currentPills = CountItemTotalQuantity(3, 2, 1);
                int targetPills = 50;
                int missingPills = targetPills - currentPills;
                if (missingPills > 0)
                {
                    if (!TryBuyVerifiedShopItem(potionNpc, 0, 13, (ushort)missingPills, 3, 2, 1, "Universal Pills"))
                        return;
                    Thread.Sleep(600);
                }
            }

            // 4. Return Scrolls
            int currentScrolls = CountReturnScrolls();
            int targetScrolls = 5;
            int missingScrolls = targetScrolls - currentScrolls;
            if (missingScrolls > 0)
            {
                if (!TryBuyVerifiedShopItem(potionNpc, 0, 14, (ushort)missingScrolls, 3, 3, 1, "Return Scroll"))
                    return;
                Thread.Sleep(600);
            }
        }

		private bool TryBuyVerifiedShopItem(SREntity npc, byte tab, byte slot, ushort quantity,
			byte expectedId2, byte expectedId3, byte expectedId4, string description)
		{
			Window w = Window.Get;
			if (npc == null || quantity == 0 || !InfoManager.inGame || !isBotting
				|| Proxy == null || !Proxy.isRunning)
				return false;

			SRItem shopItem = DataManager.GetItemFromShop(npc.ServerName, tab, slot);
			if (shopItem == null || !shopItem.isType(expectedId2, expectedId3, expectedId4))
			{
				string actual = shopItem == null ? "boş" : shopItem.ServerName;
				w.LogProcess($"Auto Buy: {description} için [{npc.Name}] tab {tab} / slot {slot} doğrulanamadı ({actual}); riskli paket gönderilmedi.", Window.ProcessState.Warning);
				return false;
			}

			w.LogProcess($"Auto Buy: Purchasing {quantity} {description} from {npc.Name}...");
			PacketBuilder.BuyItemFromShop(tab, slot, quantity, npc.UniqueID);
			return true;
		}

        private int CountReturnScrolls()
        {
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return 0;

            int total = 0;
            var inv = InfoManager.Character.Inventory;
            for (int i = 13; i < inv.Capacity; i++)
            {
                var item = inv[i];
                if (item != null && item.isType(3, 3, 1))
                {
                    total += item.Quantity;
                }
            }
            return total;
        }

        private int CountEquippedAndInventoryAmmo()
        {
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return 0;

            var inv = InfoManager.Character.Inventory;
            int total = 0;
            // Slot 7 is secondary / ammo slot
            if (inv.Capacity > 7 && inv[7] != null && inv[7].isType(3, 1, 7))
            {
                total += inv[7].Quantity;
            }
            for (int i = 13; i < inv.Capacity; i++)
            {
                var item = inv[i];
                if (item != null && item.isType(3, 1, 7))
                {
                    total += item.Quantity;
                }
            }
            return total;
        }

        public void ExecuteAutoBuyAmmo()
        {
            Window w = Window.Get;
            if (InfoManager.Character == null)
                return;

            SRTypes.Weapon weapon = GetMyWeaponType();
            // Ölü policy canlandırıldı: eşik/slot kararı TownLogisticsPolicy'den.
            int currentAmmo = CountEquippedAndInventoryAmmo();
            if (!TownLogisticsPolicy.ShouldBuyAmmo((int)weapon, currentAmmo))
                return;
            AmmoType ammoType = TownLogisticsPolicy.GetAmmoType((int)weapon);
            if (ammoType == AmmoType.None)
                return;

            SRCoord myPosition = InfoManager.Character.GetRealtimePosition();
            TownServiceInfo grocery = TownManager.Get.FindNearestService(myPosition, TownServiceType.GroceryMerchant);
            if (grocery == null)
                return;

            w.LogProcess($"Town Loop: Walking to [{grocery.NpcName}] for ammunition...");
            List<SRCoord> pathToGrocery = NavigationManager.Get.FindPath(myPosition, grocery.Coord);
            if (pathToGrocery != null && pathToGrocery.Count > 0)
            {
                for (int i = 0; i < pathToGrocery.Count && isBotting; i++)
                    WaitMovement(pathToGrocery[i], 8);
            }
            else
            {
                WaitMovement(grocery.Coord, 8);
            }

            SREntity groceryNpc = TownManager.Get.FindLiveNpc(grocery);
            if (groceryNpc != null && IsVerifiedTownNpc(grocery, groceryNpc, w, "Ammo BUY"))
            {
                if (!WaitSelectEntity(groceryNpc.UniqueID, 8, 250, "Selecting Grocery Merchant..."))
                    return;
                if (!Script.OpenNpcDialog(groceryNpc, "Ammo BUY", w, this))
                    return;

				ExecuteAutoBuyAmmoFromOpenNpc(groceryNpc);
				try
				{
					if (InfoManager.inGame && Proxy != null && Proxy.isRunning)
						PacketBuilder.CloseNPC(groceryNpc.UniqueID);
				}
				catch { }
            }
        }

		/// <summary>
		/// DoGroceryTrader ve klasik town loop için, doğrulanmış ve açık NPC
		/// oturumunda cephane alımını ortak uygular.
		/// </summary>
		public void ExecuteAutoBuyAmmoFromOpenNpc(SREntity groceryNpc)
		{
			Window w = Window.Get;
			if (groceryNpc == null || InfoManager.Character == null || !InfoManager.inGame || !isBotting)
				return;
			SRTypes.Weapon weapon = GetMyWeaponType();
			int currentAmmo = CountEquippedAndInventoryAmmo();
			if (!TownLogisticsPolicy.ShouldBuyAmmo((int)weapon, currentAmmo))
				return;
			AmmoType ammoType = TownLogisticsPolicy.GetAmmoType((int)weapon);
			if (ammoType == AmmoType.None)
				return;

			byte shopSlot = TownLogisticsPolicy.GetAmmoShopSlot(ammoType);
			string ammoName = ammoType == AmmoType.Arrow ? "Arrows" : "Bolts";
			SRItem shopItem = DataManager.GetItemFromShop(groceryNpc.ServerName, 0, shopSlot);
			if (shopItem == null || shopItem.ID2 != 3 || shopItem.ID3 != 1)
			{
				string actual = shopItem == null ? "boş" : shopItem.ServerName;
				w.LogProcess($"Ammo BUY: [{groceryNpc.Name}] tab 0 / slot {shopSlot} doğrulanamadı ({actual}); paket gönderilmedi.", Window.ProcessState.Warning);
				return;
			}

			w.LogProcess($"Auto Buy: Purchasing {ammoName} from {groceryNpc.Name}...");
			PacketBuilder.BuyItemFromShop(0, shopSlot, 1, groceryNpc.UniqueID);
			if (!SleepInterruptible(600) || !InfoManager.inGame || Proxy == null || !Proxy.isRunning)
				return;
			PacketBuilder.BuyItemFromShop(0, shopSlot, 1, groceryNpc.UniqueID);
			if (!SleepInterruptible(600))
				return;

			var inv = InfoManager.Character.Inventory;
			if (inv != null && inv.Capacity > 7 && (inv[7] == null || inv[7].Quantity == 0))
			{
				byte invSlot = 0;
				if (FindItem(3, 1, 7, ref invSlot))
				{
					w.LogProcess("Auto Equip: Equipping ammunition to slot 7...");
					PacketBuilder.MoveItem(invSlot, 7, SRTypes.InventoryItemMovement.InventoryToInventory);
					SleepInterruptible(500);
				}
			}
		}

        private void WalkLoop()
        {

        }

        private DateTime m_lastReturnPrep = DateTime.MinValue;
        /// <summary>
        /// Alana Dönüş hazırlığı (Area &gt; Alana Dönüş kartı).
        /// Eğitim alanına yürüyüş başlamadan önce buff tazeleme, hız eşyası
        /// ve binek çağırma yapar. 45sn geçiş korumalıdır.
        /// </summary>
        private void PrepareReturnTrip()
        {
            try
            {
                if ((DateTime.Now - m_lastReturnPrep).TotalSeconds < 45.0)
                    return;
                m_lastReturnPrep = DateTime.Now;
                Window w = Window.Get;
                if (ReturnToAreaPolicy.CastBuffs)
                {
                    try { w.CastAllBuffs(); } catch { }
                }
                if (ReturnToAreaPolicy.UseSpeedDrug || TrainingOptionsPolicy.UseSpeedDrugs)
                    TryUseSpeedDrug();
                if (ReturnToAreaPolicy.RideFellowPet)
                    TryRideFellowPet();
                else if (ReturnToAreaPolicy.UseMount && !TrainingOptionsPolicy.DoNotSpawnMount)
                    TrySummonMount();
            }
            catch { }
        }

        public bool TryRideFellowPet()
        {
            try
            {
                var chr = InfoManager.Character;
                if (chr == null || chr.isRiding)
                    return false;

                var fellow = Script.FindFellowMountCandidate();
                if (fellow != null)
                {
                    Window.Get?.LogProcess($"Alana Dönüş: fellow pete biniliyor [{fellow.Name}]...");
                    PacketBuilder.SetPetMounted(fellow.UniqueID, true);
                    return true;
                }

                if (chr.Inventory != null)
                {
                    for (byte s = 13; s < chr.Inventory.Capacity; s++)
                    {
                        var item = chr.Inventory[s];
                        if (item != null && item.ServerName != null &&
                            (item.ServerName.IndexOf("FELLOW", StringComparison.OrdinalIgnoreCase) >= 0
                             || item.ServerName.IndexOf("GROWTH", StringComparison.OrdinalIgnoreCase) >= 0
                             || item.ServerName.IndexOf("COS_P", StringComparison.OrdinalIgnoreCase) >= 0))
                        {
                            Window.Get?.LogProcess($"Alana Dönüş: fellow pet çağrılıyor [{item.Name}]...");
                            return PacketBuilder.UseItem(item, s);
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        public bool ReturnTripFellowNow()
        {
            try { return TryRideFellowPet(); } catch { return false; }
        }
        /// <summary>
        /// Hız eşyası kullanır: ServerName içinde SPEED geçip RETURN geçmeyen
        /// kullanılabilir eşya (dönüş scroll'ları bilerek hariç tutulur).
        /// </summary>
        private bool TryUseSpeedDrug()
        {
            try
            {
                var chr = InfoManager.Character;
                if (chr == null || chr.Inventory == null)
                    return false;
                var inv = chr.Inventory;
                for (byte s = 13; s < inv.Capacity; s++)
                {
                    var it = inv[s];
                    if (it == null || it.ID2 != 3)
                        continue;
                    string sn = it.ServerName ?? "";
                    if (sn.IndexOf("SPEED", StringComparison.OrdinalIgnoreCase) < 0)
                        continue;
                    if (sn.IndexOf("RETURN", StringComparison.OrdinalIgnoreCase) >= 0)
                        continue;
                    if (IsItemBlockedFromUse(it))
                        continue;
                    Window.Get?.LogProcess("Alana Dönüş: hız eşyası kullanılıyor [" + it.Name + "]...");
                    return PacketBuilder.UseItem(it, s);
                }
            }
            catch { }
            return false;
        }
        /// <summary>
        /// UI toggle'ları için anında-uygulama girişleri (tick sistemiyle aynı
        /// backend: PrepareReturnTrip'in kullandığı metotlar).
        /// </summary>
        public void ReturnTripBuffsNow()
        {
            try { Window.Get?.CastAllBuffs(); } catch { }
        }
        public bool ReturnTripSpeedNow()
        {
            try { return TryUseSpeedDrug(); } catch { return false; }
        }
        public bool ReturnTripMountNow()
        {
            try { return TrySummonMount(); } catch { return false; }
        }
        /// <summary>
        /// Binek çağırır: zaten biniliyorsa ya da at/transport dışarıdaysa
        /// dokunmaz, yoksa vehicle/transport summon scroll basar.
        /// </summary>
        private bool TrySummonMount()
        {
            try
            {
                if (TrainingOptionsPolicy.DoNotSpawnMount)
                    return false;

                var chr = InfoManager.Character;
                if (chr == null || chr.isRiding)
                    return false;
                try
                {
                    var pet = InfoManager.MyPets.Find(p => p.isHorse() || p.isTransport());
                    if (pet != null)
                        return false;
                }
                catch { }
                byte slot = 0;
                if (Bot.Get.FindItem(3, 3, 2, ref slot))
                {
                    var it = chr.Inventory[slot];
                    Window.Get?.LogProcess("Alana Dönüş: binek çağrılıyor [" + (it != null ? it.Name : slot.ToString()) + "]...");
                    return PacketBuilder.UseItem(it, slot);
                }
            }
            catch { }
            return false;
        }

        /// <summary>
        /// Akıllı hareket ve çarpışma/takılma kurtarma (Anti-Stuck Obstacle Avoidance) motoru.
        /// </summary>
        public bool WaitMovement(SRCoord position, int maxAttempts)
        {
            int attemps = 0;
            SRCoord myPosition;
            SRCoord lastPosition = null;
            DateTime? stuckSinceUtc = null;
            int stuckCounter = 0;
            bool avoidToRight = true;
            int bypassCount = 0; // Ard arda kaç kez bypass yapıldı (büyüyen adım için)
            double startDist = -1;
            double bestDist = double.MaxValue;

            while (isBotting)
            {
                myPosition = InfoManager.Character.GetRealtimePosition();

                // Hedefe tolerans dahilinde (<= 3 metre) ulaşıldı mı?
                if (myPosition.Equals(position, 3.0))
                {
                    return true;
                }

                if (attemps >= maxAttempts)
                {
                    return false;
                }
                attemps++;

                double curDist = myPosition.DistanceTo(position);
                if (startDist < 0) startDist = curDist;
                if (curDist < bestDist) bestDist = curDist;

                // İlerleme watchdog: 10 denemede en iyi mesafe 3m'den az kısalmadıysa duvar/dağ
                // önündeyiz demektir; 168 deneme boyunca yerinde sekme, erken çık.
                if (attemps % 10 == 0 && (startDist - bestDist) < 3.0 && startDist > 15.0)
                {
                    Window.Get.LogProcess("No progress towards target (blocked by wall/mountain). Aborting walk.", Window.ProcessState.Warning);
                    return false;
                }

                // Çarpışma / Takılma Algılayıcı (Stuck Detection)
                if (lastPosition != null && myPosition.DistanceTo(lastPosition) < 0.6)
                {
                    if (stuckSinceUtc == null) stuckSinceUtc = DateTime.UtcNow;
                    double stuckSec = (DateTime.UtcNow - stuckSinceUtc.Value).TotalSeconds;

                    if (ReturnToAreaPolicy.ReturnIfStuckAfterSecondsEnabled && ReturnToAreaPolicy.ReturnIfStuckSeconds > 0 && stuckSec >= ReturnToAreaPolicy.ReturnIfStuckSeconds)
                    {
                        Window.Get?.LogProcess($"WaitMovement: {ReturnToAreaPolicy.ReturnIfStuckSeconds}s takılı kalındı ('Şu süre boyunca takılı kalırsa şehre dön' aktif). Şehre dönülüyor...", Window.ProcessState.Warning);
                        UseReturnScroll();
                        return false;
                    }

                    if (ReturnToAreaPolicy.GoBackCoordIfStuck && ReturnToAreaPolicy.GoBackCoordSeconds > 0 && stuckSec >= ReturnToAreaPolicy.GoBackCoordSeconds)
                    {
                        Window.Get?.LogProcess($"WaitMovement: {ReturnToAreaPolicy.GoBackCoordSeconds}s takılı kalındı ('Karakter takılırsa bir koordinat geri dön' aktif).");
                        return false;
                    }

                    stuckCounter++;
                    if (stuckCounter >= 2)
                    {
                        // 2 denemede ilerleyemedi -> Önünde engel/duvar var!
                        Window.Get.LogProcess("Collision detected! Executing obstacle bypass maneuver...");
                        bypassCount++;

                        // Hedefe doğru olan vektör
                        double dx = position.PosX - myPosition.PosX;
                        double dy = position.PosY - myPosition.PosY;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        if (dist < 0.001) dist = 1.0;

                        // Normalleştirilmiş birim vektör (ileri yön)
                        double nx = dx / dist;
                        double ny = dy / dist;

                        // 90 derece teğet dik vektör (sağa veya sola)
                        double perpX = avoidToRight ? -ny : ny;
                        double perpY = avoidToRight ? nx : -nx;
                        // NOT: her seferinde yön değiştirme ping-pong yapar (A->B->A).
                        // Aynı yönde 2 bypass dene, sonra değiştir.
                        if (bypassCount % 2 == 0)
                            avoidToRight = !avoidToRight;

                        // Öne + yana yay çizerek engelin etrafından dolaş:
                        // saf yana adım geri döndürür, ileri bileşen ilerletir.
                        // Adım her takılmada büyür (4.5m -> 6.5m -> 8.5m ... max 12m).
                        double avoidStep = Math.Min(12.0, 4.5 + (bypassCount - 1) * 2.0);
                        double forwardStep = 3.0;
                        double bx = myPosition.PosX + perpX * avoidStep + nx * forwardStep;
                        double by = myPosition.PosY + perpY * avoidStep + ny * forwardStep;
                        SRCoord bypassCoord;
                        if (myPosition.inDungeon())
                            bypassCoord = new SRCoord(bx, by, myPosition.Region, myPosition.Z);
                        else
                            bypassCoord = new SRCoord(bx, by);

                        MoveTo(bypassCoord);
                        Thread.Sleep(900);
                        stuckCounter = 0;
                        lastPosition = InfoManager.Character.GetRealtimePosition();
                        continue;
                    }
                }
                else
                {
                    stuckCounter = 0;
                    stuckSinceUtc = null;
                    // Hareket varsa bypass serisini sıfırla (yönü koru)
                    if (lastPosition != null && myPosition.DistanceTo(lastPosition) > 2.0)
                        bypassCount = 0;
                }
                lastPosition = myPosition;

                if (ReturnToAreaPolicy.AvoidStatueOfJustice)
                {
                    try
                    {
                        var statue = InfoManager.Mobs.Snapshot().Find(m => m != null &&
                            ((m.Name != null && (m.Name.IndexOf("Statue of Justice", StringComparison.OrdinalIgnoreCase) >= 0 || m.Name.IndexOf("Adalet", StringComparison.OrdinalIgnoreCase) >= 0))
                            || (m.ServerName != null && m.ServerName.IndexOf("STATUE_OF_JUSTICE", StringComparison.OrdinalIgnoreCase) >= 0)));
                        if (statue != null)
                        {
                            var sPos = statue.GetRealtimePosition();
                            if (sPos != null && myPosition.DistanceTo(sPos) < 20.0)
                            {
                                double edx = myPosition.PosX - sPos.PosX;
                                double edy = myPosition.PosY - sPos.PosY;
                                double len = Math.Sqrt(edx * edx + edy * edy);
                                if (len < 0.1) { edx = 1; len = 1; }
                                SRCoord evade = new SRCoord(myPosition.PosX + (edx / len) * 15.0, myPosition.PosY + (edy / len) * 15.0);
                                Window.Get?.LogProcess("WaitMovement: Statue of Justice algılandı! Güvenli mesafeye kaçılıyor...");
                                MoveTo(evade);
                                Thread.Sleep(600);
                            }
                        }
                    }
                    catch { }
                }

                // Hedefe doğru yürü
                int timeWalking = myPosition.TimeTo(position, InfoManager.Character.GetSpeed());
                MoveTo(position);
                int waitTime = Math.Min(1200, Math.Max(300, timeWalking / 2));
                Window.Get.LogProcess("Walking towards waypoint (" + (int)myPosition.DistanceTo(position) + "m remaining)...");
                Thread.Sleep(waitTime);
            }
            return false;
        }
        public bool WaitSelectEntity(uint uniqueID, int maxAttempts, int delay, string logProcess = "")
        {
            int attemps = 0;
            while (InfoManager.SelectedEntityUniqueID != uniqueID)
            {
                // Check if entity is near
                if (!InfoManager.isEntityNear(uniqueID))
                    return false;

                if (attemps >= maxAttempts)
                    return false;
                else
                    attemps++;
                // Selecting
                PacketBuilder.SelectEntity(uniqueID);
                if (logProcess != "")
                    Window.Get.LogProcess(logProcess);
                InfoManager.MonitorEntitySelected.WaitOne(delay);
            }
            return true;
        }

        // Son denemede yanlış çıkan teleport adayı UID'leri (oturum içi).
        private static readonly Dictionary<uint, DateTime> s_badTeleportUids = new Dictionary<uint, DateTime>();
        private static readonly object s_badTeleportLock = new object();

        private static bool IsBadTeleportUid(uint uid)
        {
            lock (s_badTeleportLock)
            {
                DateTime until;
                if (s_badTeleportUids.TryGetValue(uid, out until))
                {
                    if (DateTime.Now < until)
                        return true;
                    s_badTeleportUids.Remove(uid);
                }
                return false;
            }
        }

        private static void MarkBadTeleportUid(uint uid)
        {
            lock (s_badTeleportLock)
            {
                s_badTeleportUids[uid] = DateTime.Now.AddMinutes(10);
            }
        }

        /// <summary>
        /// İsme/ServerName'e bakarak gerçek ışınlanma NPC'sini tanır
        /// (Dimensional Gate, Teleport Gatekeeper, Ferry Ticket Seller vb.).
        /// Şehir kapıcıları normalde guide-tipi NPC'dir (ID3==2), o yüzden
        /// körü körüne "guide = kötü" denemez — bakkal/depo ile kapı
        /// bu isim testiyle ayrılır.
        /// </summary>
        private static bool IsTeleportLookingNpc(SREntity e)
        {
            try
            {
                string name = (e.Name ?? "").ToUpperInvariant();
                string sn = (e.ServerName ?? "").ToUpperInvariant();
                string combined = name + " " + sn;
                if (combined.Contains("TELEPORT") || combined.Contains("GATEKEEPER")
                    || combined.Contains("DIMENSIONAL") || combined.Contains("DIMENSION")
                    || combined.Contains("FERRY") || combined.Contains("TICKET SELLER")
                    || combined.Contains("GATE") || combined.Contains("PORTAL"))
                    return true;
            }
            catch { }
            return false;
        }

        private static bool IsPlayerOpenedPortal(SRTeleport tp)
        {
            try
            {
                if (tp == null)
                    return false;
                // Oyuncunun açtığı dimensional hole: sahibi vardır, şehir kapısı değildir.
                if (tp.PortalType == SRTeleport.Portal.Dimensional
                    && (tp.OwnerUniqueID != 0 || !string.IsNullOrEmpty(tp.OwnerName)))
                    return true;
            }
            catch { }
            return false;
        }

        /// <summary>
        /// NPC/kapı dibine girmeden etkileşim mesafesinde durma noktası:
        /// oyuncudan hedefe bakınca hedefin standDist metre gerisi.
        /// (Dimensional Gate ve NPC'lerin collision'ı var — tam üstüne
        /// yürümek 3-4sn takılma yapar.)
        /// </summary>
        private static SRCoord StandPointNear(SRCoord from, SRCoord target, double standDist)
        {
            try
            {
                if (from == null || target == null)
                    return target;
                double dx = from.PosX - target.PosX;
                double dy = from.PosY - target.PosY;
                double d = System.Math.Sqrt(dx * dx + dy * dy);
                if (d < 0.001 || d <= standDist)
                    return from;
                double k = standDist / d;
                return new SRCoord(target.PosX + dx * k, target.PosY + dy * k);
            }
            catch { return target; }
        }

        /// <summary>
        /// Aday skorlar: ModelID eşleşmesi ve hedefi seçeneklerinde barındıran
        /// gerçek teleport kapısı en üstte; ışınlanma NPC'si olmayan guide'lar
        /// (bakkal, depo, Magic POP Guide vb.) güçlü şekilde elenir; 80m
        /// içindeki ilk NPC körü körüne alınmaz.
        /// </summary>
        private static int ScoreTeleportCandidate(SREntity e, TeleportLinkInfo link, double dist)
        {
            int score = 0;
            if (e.ID == link.NpcId)
                score += 1000;
            SRTeleport tp = e as SRTeleport;
            if (tp != null)
            {
                // Oyuncunun açtığı geçici portallar şehir kapısı değildir.
                if (IsPlayerOpenedPortal(tp))
                    return -2000;
                score += 300;
                try
                {
                    if (tp.TeleportOptions != null)
                    {
                        foreach (var opt in tp.TeleportOptions)
                        {
                            if (opt == null)
                                continue;
                            if (opt.ID == link.DestinationId)
                            {
                                score += 500;
                                break;
                            }
                            if (!string.IsNullOrEmpty(opt.Name) && !string.IsNullOrEmpty(link.DestinationName)
                                && opt.Name.IndexOf(link.DestinationName, StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                score += 500;
                                break;
                            }
                        }
                    }
                }
                catch { }
            }
            string name = e.Name ?? "";
            string sn = e.ServerName ?? "";
            if (!string.IsNullOrEmpty(link.SourceName)
                && name.IndexOf(link.SourceName, StringComparison.OrdinalIgnoreCase) >= 0)
                score += 200;
            // Kanonik kapı adı (DB + harita verisinde "Dimensional Gate").
            if (name.IndexOf("Dimensional Gate", StringComparison.OrdinalIgnoreCase) >= 0)
                score += 100;
            if (sn.IndexOf("TELEPORT", StringComparison.OrdinalIgnoreCase) >= 0
                || sn.IndexOf("PORTAL", StringComparison.OrdinalIgnoreCase) >= 0
                || sn.IndexOf("FERRY", StringComparison.OrdinalIgnoreCase) >= 0
                || sn.IndexOf("GATE", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("Ferry Ticket", StringComparison.OrdinalIgnoreCase) >= 0)
                score += 150;
            SRNpc npc = e as SRNpc;
            if (npc != null)
            {
                if (IsTeleportLookingNpc(e))
                    score += 400;
                else if (npc.isGuide())
                    score -= 600;
            }
            score -= (int)Math.Min(80.0, dist);
            return score;
        }

        private static SREntity FindBestTeleportCandidate(TeleportLinkInfo link)
        {
            SREntity best = null;
            int bestScore = int.MinValue;
            // Teleport kapıları önce (gerçek kapı/portallar bu listededir).
            List<SREntity> pool = new List<SREntity>();
            try { foreach (var tp in InfoManager.TeleportAndBuildings.Snapshot()) pool.Add(tp); } catch { }
            try { foreach (var npc in InfoManager.Npcs.Snapshot()) pool.Add(npc); } catch { }
            try { foreach (var e in InfoManager.Entities.Snapshot()) pool.Add(e); } catch { }
            foreach (var e in pool)
            {
                try
                {
                    if (e == null || e.Position == null || link.BoardCoord == null)
                        continue;
                    if (e is SRPlayer || e is SRDrop || e is SRMob)
                        continue;
                    if (IsBadTeleportUid(e.UniqueID))
                        continue;
                    SRTeleport tpCheck = e as SRTeleport;
                    if (tpCheck != null && IsPlayerOpenedPortal(tpCheck))
                        continue;
                    // Köprü bölgesi dışındakiler aday değildir. Bölge filtresi
                    // yoktur: PosX/PosY sektör bilgisini zaten içerir, uzak
                    // bölgedekiler mesafeden elenir.
                    double dist = e.Position.DistanceTo(link.BoardCoord);
                    if (dist > 80.0)
                        continue;
                    int score = ScoreTeleportCandidate(e, link, dist);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = e;
                    }
                }
                catch { }
            }
            // En iyi skor bile kapıya benzemiyorsa yanlış NPC'yi (bakkal/depo)
            // seçmek yerine "bulunamadı" dön — körü körüne ilk NPC alınmaz.
            if (best != null && bestScore < 150)
                return null;
            return best;
        }

        private static bool IsUsableTeleportCandidate(SREntity e, TeleportLinkInfo link)
        {
            if (e == null)
                return false;
            SRTeleport tp = e as SRTeleport;
            if (tp != null)
                return !IsPlayerOpenedPortal(tp);
            // ModelID birebir tutuyorsa her zaman kullan (DB doğruysa).
            if (e.ID == link.NpcId && link.NpcId != 0)
                return true;
            // Işınlanma isimli NPC (Dimensional Gate / Gatekeeper / Ferry) kullan.
            if (IsTeleportLookingNpc(e))
                return true;
            return false;
        }

        private static bool IsStrongTeleportMatch(SREntity e, TeleportLinkInfo link)
        {
            if (e == null || e.Position == null || link.BoardCoord == null)
                return false;
            // ModelID eşleşmesi tek başına güçlü kanıttır (guide NPC kapıcılar dahil).
            if (link.NpcId != 0 && e.ID == link.NpcId)
                return true;
            SRTeleport tp = e as SRTeleport;
            if (tp != null && !IsPlayerOpenedPortal(tp))
            {
                try
                {
                    if (tp.TeleportOptions != null)
                    {
                        foreach (var opt in tp.TeleportOptions)
                        {
                            if (opt != null && opt.ID == link.DestinationId)
                                return true;
                        }
                    }
                }
                catch { }
            }
            if (IsTeleportLookingNpc(e))
                return ScoreTeleportCandidate(e, link, e.Position.DistanceTo(link.BoardCoord)) >= 300;
            return ScoreTeleportCandidate(e, link, e.Position.DistanceTo(link.BoardCoord)) >= 700;
        }

        private bool WaitTeleportArrival(TeleportLinkInfo link, SRCoord beforePos, int maxWaits)
        {
            for (int wait = 0; wait < maxWaits && isBotting; wait++)
            {
                Thread.Sleep(500);
                SRCoord currentPos = null;
                try { currentPos = InfoManager.Character.GetRealtimePosition(); } catch { continue; }
                if (currentPos == null || beforePos == null)
                    continue;
                if (currentPos.DistanceTo(beforePos) > 40.0 || currentPos.DistanceTo(link.ArriveCoord) < 70.0)
                    return true;
            }
            return false;
        }

        private bool ExecuteTeleportTransition(TeleportLinkInfo link)
        {
            Window w = Window.Get;
            if (link != null)
            {
                int playerLevel = (int)(InfoManager.Character != null ? InfoManager.Character.Level : 0);
                if (!CollisionPolicy.IsLinkAllowed(link, playerLevel))
                {
                    w.Log($"Ferry/Teleport: [{link.SourceName} -> {link.DestinationName}] Çarpışma sekmesi ayarları nedeniyle engellendi.");
                    return false;
                }
            }
            w.Log($"Ferry/Teleport: Transitioning [{link.SourceName}] -> [{link.DestinationName}]...");
            // Teşhis: yeni kodun koştuğu ve mesafelerin logdan belli olması için.
            try
            {
                SRCoord dbgPos = null;
                try { dbgPos = InfoManager.Character.GetRealtimePosition(); } catch { }
                int tpCount = 0, npcCount = 0;
                try { tpCount = InfoManager.TeleportAndBuildings.Count; } catch { }
                try { npcCount = InfoManager.Npcs.Count; } catch { }
                w.Log($"Ferry/Teleport: pos=[{(dbgPos != null ? ((int)dbgPos.PosX + "," + (int)dbgPos.PosY + " r" + dbgPos.Region) : "?")}] board=[{(link.BoardCoord != null ? ((int)link.BoardCoord.PosX + "," + (int)link.BoardCoord.PosY + " r" + link.BoardCoord.Region) : "?")}] dist=[{(dbgPos != null && link.BoardCoord != null ? dbgPos.DistanceTo(link.BoardCoord).ToString("F0") + "m" : "?")}] spawned(tp={tpCount},npc={npcCount}) NpcId={link.NpcId} DestId={link.DestinationId}");
            }
            catch { }

            // 0. Önce kapı koordinatına yürü (şehir scripti depoda/potioncuda
            //    bitmiş olabilir — kapı 80m+ uzaktaysa aday aramak anlamsız).
            //    Her şehrin BoardCoord'u TeleportManager'da bellidir.
            try
            {
                if (link.BoardCoord != null)
                {
                    SRCoord myPos0 = null;
                    try { myPos0 = InfoManager.Character.GetRealtimePosition(); } catch { }
                    if (myPos0 != null)
                    {
                        double distBoard = myPos0.DistanceTo(link.BoardCoord);
                        // Sadece gerçekten uzaktaysa yürü (>40m). Menzildeyken
                        // (<=40m) hiç kımıldama — kapı dışarıdan tıklanabiliyor,
                        // yaklaşmak havuz/duvara toslatıyor.
                        if (distBoard > 40.0)
                        {
                            w.LogProcess($"Walking to gate [{link.SourceName}] ({distBoard:F0}m)...");
                            bool reached = false;
                            try
                            {
                                if (NavigationManager.Get.IsAvailable)
                                {
                                    SRCoord standTarget = StandPointNear(myPos0, link.BoardCoord, 25.0);
                                    List<SRCoord> path = NavigationManager.Get.FindPath(myPos0, standTarget);
                                    if (path != null && path.Count > 0)
                                    {
                                        // Kapı çevresi waypoint'leri ele (yapıya dolandırır),
                                        // final radyal durma noktası korunur.
                                        var trimmed = new List<SRCoord>();
                                        foreach (var q in path)
                                        {
                                            try
                                            {
                                                if (q != null && q.DistanceTo(link.BoardCoord) > 30.0)
                                                    trimmed.Add(q);
                                            }
                                            catch { }
                                        }
                                        trimmed.Add(standTarget);
                                        path = trimmed;
                                        foreach (var wp in path)
                                        {
                                            if (!isBotting || m_stopBottingRequested) break;
                                            WaitMovement(wp, 12);
                                            SRCoord cur = null;
                                            try { cur = InfoManager.Character.GetRealtimePosition(); } catch { }
                                            if (cur != null && cur.DistanceTo(link.BoardCoord) <= 30.0)
                                            {
                                                reached = true;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            catch { }
                            if (!reached)
                            {
                                // NavMesh yoksa/boşsa direkt yürü (şehir içi genelde açıktır).
                                // 25m yakınına — daha içeri girme, duvar/havuz yapar.
                                SRCoord cur = null;
                                try { cur = InfoManager.Character.GetRealtimePosition(); } catch { }
                                if (cur != null && cur.DistanceTo(link.BoardCoord) > 28.0)
                                {
                                    SRCoord stand = StandPointNear(cur, link.BoardCoord, 25.0);
                                    double d = cur.DistanceTo(stand);
                                    int attempts = (int)(d / 5.0) + 10;
                                    if (attempts < 15) attempts = 15;
                                    if (attempts > 40) attempts = 40;
                                    WaitMovement(stand, attempts);
                                }
                            }
                            SRCoord after = null;
                            try { after = InfoManager.Character.GetRealtimePosition(); } catch { }
                            if (after != null && after.DistanceTo(link.BoardCoord) > 80.0)
                            {
                                w.Log($"Ferry/Teleport: kapıya yaklaşılamadı ({after.DistanceTo(link.BoardCoord):F0}m uzakta) — ışınlanma iptal.", Theme.LogLevel.Warning);
                                TeleportManager.Get.NoteLinkFailure(link);
                                return false;
                            }
                        }
                    }
                }
            }
            catch { }

            // 1. Aday topla: güçlü eşleşme (ModelID / hedef seçenekli kapı)
            //    bulunur bulunmaz bekleme bırakılır, yoksa ~9sn beklenir
            //    (ışınlanma sonrası spawn'lar geç gelebilir — erken durdurma).
            SREntity firstSeen = null;
            for (int attempt = 0; attempt < 30 && isBotting; attempt++)
            {
                firstSeen = FindBestTeleportCandidate(link);
                if (firstSeen != null && IsStrongTeleportMatch(firstSeen, link))
                    break;
                if (attempt == 29)
                    break;
                if (attempt % 10 == 9)
                {
                    try
                    {
                        int tpC = 0, npcC = 0;
                        try { tpC = InfoManager.TeleportAndBuildings.Count; } catch { }
                        try { npcC = InfoManager.Npcs.Count; } catch { }
                        w.LogProcess($"Gate spawn bekleniyor... ({(attempt + 1) * 300 / 1000}s, tp={tpC},npc={npcC})");
                    }
                    catch { }
                }
                Thread.Sleep(300);
            }

            // 1b. BoardCoord etrafında aday yoksa (DB koordinatı private
            // serverda farklı olabilir): oyuncuya yakın ışınlanma isimli
            // NPC'yi bul, yanına yürü, öyle dene. Ayrıca teşhis için
            // yakındaki TÜM entity'leri (isim+servername+ID) logla.
            if (firstSeen == null && isBotting)
            {
                try
                {
                    SRCoord myP = null;
                    try { myP = InfoManager.Character.GetRealtimePosition(); } catch { }
                    if (myP != null)
                    {
                        List<SREntity> pool2 = new List<SREntity>();
                        try { foreach (var tp in InfoManager.TeleportAndBuildings.Snapshot()) pool2.Add(tp); } catch { }
                        try { foreach (var npc in InfoManager.Npcs.Snapshot()) pool2.Add(npc); } catch { }
                        try { foreach (var e in InfoManager.Entities.Snapshot()) { if (e != null && !(e is SRPlayer) && !(e is SRDrop) && !(e is SRMob)) pool2.Add(e); } } catch { }
                        SREntity nearestGate = null;
                        double nearestGateDist = double.MaxValue;
                        List<string> gateNames = new List<string>();
                        // Teşhis: oyuncuya en yakın 12 entity (tip+isim+ID+mesafe+skor).
                        List<string> nearDump = new List<string>();
                        List<KeyValuePair<double, SREntity>> byDist = new List<KeyValuePair<double, SREntity>>();
                        foreach (var e in pool2)
                        {
                            try
                            {
                                if (e == null || e.Position == null) continue;
                                if (e is SRPlayer || e is SRDrop || e is SRMob) continue;
                                double d = myP.DistanceTo(e.Position);
                                if (d < 120.0)
                                    byDist.Add(new KeyValuePair<double, SREntity>(d, e));
                            }
                            catch { }
                        }
                        byDist.Sort((a, b) => a.Key.CompareTo(b.Key));
                        for (int i = 0; i < byDist.Count && nearDump.Count < 12; i++)
                        {
                            try
                            {
                                var e = byDist[i].Value;
                                double d = byDist[i].Key;
                                string typ = (e as SRTeleport != null) ? "TP" : (e as SRNpc != null ? "NPC" : e.GetType().Name);
                                int sc = ScoreTeleportCandidate(e, link, d);
                                nearDump.Add($"{typ}[{e.Name}|{e.ServerName}|{e.ID}]({d:F0}m,s={sc})");
                            }
                            catch { }
                        }
                        w.Log($"Ferry/Teleport: yakındaki entityler: {string.Join(", ", nearDump.ToArray())}");
                        foreach (var e in pool2)
                        {
                            try
                            {
                                if (e == null || e.Position == null) continue;
                                if (e is SRPlayer || e is SRDrop || e is SRMob) continue;
                                SRTeleport tpX = e as SRTeleport;
                                if (tpX != null && IsPlayerOpenedPortal(tpX)) continue;
                                if (!IsTeleportLookingNpc(e) && !(e as SRTeleport != null)) continue;
                                double d = myP.DistanceTo(e.Position);
                                if (d < nearestGateDist) { nearestGateDist = d; nearestGate = e; }
                                if (d < 300.0 && gateNames.Count < 8)
                                    gateNames.Add($"[{e.Name}]({d:F0}m)");
                            }
                            catch { }
                        }
                        w.Log($"Ferry/Teleport: board çevresinde aday yok — oyuncu yakınında kapı aranıyor (en yakın kapı: {(nearestGate != null ? nearestGate.Name + " " + nearestGateDist.ToString("F0") + "m" : "yok")}). Spawn: {string.Join(", ", gateNames.ToArray())}");
                        if (nearestGate != null && nearestGateDist < 150.0 && nearestGateDist > 12.0)
                        {
                            w.LogProcess($"Walking to nearby gate [{nearestGate.Name}] ({nearestGateDist:F0}m)...");
                            SRCoord stand = StandPointNear(myP, nearestGate.Position, 15.0);
                            double d0 = myP.DistanceTo(stand);
                            int attempts = (int)(d0 / 5.0) + 10;
                            if (attempts < 15) attempts = 15;
                            if (attempts > 40) attempts = 40;
                            WaitMovement(stand, attempts);
                            // Yürüdükten sonra kısa yeniden tara (uzun 9sn beklemeye girme).
                            for (int attempt = 0; attempt < 10 && isBotting; attempt++)
                            {
                                firstSeen = FindBestTeleportCandidate(link);
                                if (firstSeen != null && IsStrongTeleportMatch(firstSeen, link))
                                    break;
                                Thread.Sleep(300);
                            }
                        }
                    }
                }
                catch { }
            }

            // 2. En iyi 3 adayı sırayla dene (yanlış dialog açan elenir).
            // Board dışı yakında kapı bulunduysa ilk denemede onu kullan
            // (80m board filtrine takılsa bile).
            SREntity pendingNearbyGate = (firstSeen != null && IsUsableTeleportCandidate(firstSeen, link)) ? firstSeen : null;
            for (int t = 0; t < 3 && isBotting; t++)
            {
                SREntity targetEntity = null;
                if (t == 0 && pendingNearbyGate != null)
                {
                    targetEntity = pendingNearbyGate;
                    pendingNearbyGate = null;
                }
                else
                {
                    targetEntity = FindBestTeleportCandidate(link);
                }
                if (targetEntity == null)
                    break;

                w.Log($"Ferry/Teleport: Found candidate [{targetEntity.Name}] (UID: {targetEntity.UniqueID}, ModelID: {targetEntity.ID})");

                // 2a-0. Güvenlik: kapıya benzemeyen NPC'ye (bakkal/depo/banker)
                // UseTeleport paketi GÖNDERME — yoksa yanlış dialog açılır.
                if (!IsUsableTeleportCandidate(targetEntity, link))
                {
                    w.Log($"Ferry/Teleport: [{targetEntity.Name}] ışınlanma NPC'sine benzemiyor (ModelID {targetEntity.ID}/{link.NpcId}) — paket göndermeden eleniyor.", Theme.LogLevel.Warning);
                    MarkBadTeleportUid(targetEntity.UniqueID);
                    Thread.Sleep(300);
                    continue;
                }

                // 2a. Etkileşim bandı 8-30m: Dimensional Gate + köprü/havuz gibi
                // yapılar kocaman; yaklaşmak duvara toslatır (Constantinople
                // havuzu gibi). Kapı dışarıdan tıklanabildiği için menzil
                // içindeyse HİÇ yürüme — direkt seç. Çok yakınsan geri çık,
                // çok uzaktaysan (30m+) 20m'ye yaklaş.
                SRCoord myPos = null;
                try { myPos = InfoManager.Character.GetRealtimePosition(); } catch { }
                if (myPos != null && targetEntity.Position != null)
                {
                    double dNpc = myPos.DistanceTo(targetEntity.Position);
                    if (dNpc < 8.0)
                    {
                        w.LogProcess($"Too close to gate ({dNpc:F1}m, collision) — stepping back...");
                        WaitMovement(StandPointNear(myPos, targetEntity.Position, 15.0), 6);
                    }
                    else if (dNpc > 30.0)
                    {
                        w.LogProcess($"Walking to teleport NPC ({dNpc:F1}m)...");
                        WaitMovement(StandPointNear(myPos, targetEntity.Position, 20.0), 6);
                    }
                    else
                    {
                        w.LogProcess($"Gate in range ({dNpc:F1}m) — no walk, selecting directly.");
                    }
                }

                // 2b. Seç + teleport paketi (seçim oturmadıysa paket gönderme).
                w.LogProcess($"Selecting NPC [{targetEntity.Name}]...");
                if (!WaitSelectEntity(targetEntity.UniqueID, 10, 250, "Selecting teleport " + targetEntity.Name + "..."))
                {
                    w.Log($"Ferry/Teleport: [{targetEntity.Name}] seçilemedi — sıradaki aday deneniyor.", Theme.LogLevel.Warning);
                    MarkBadTeleportUid(targetEntity.UniqueID);
                    Thread.Sleep(500);
                    continue;
                }
                Thread.Sleep(600);
                SRCoord beforePos = null;
                try { beforePos = InfoManager.Character.GetRealtimePosition(); } catch { }
                try
                {
                    double useDist = (beforePos != null && targetEntity.Position != null) ? beforePos.DistanceTo(targetEntity.Position) : -1;
                    w.Log($"Ferry/Teleport: Requesting transport to [{link.DestinationName}] (DestID: {link.DestinationId}, dist: {(useDist >= 0 ? useDist.ToString("F1") + "m" : "?")})...");
                }
                catch { w.Log($"Ferry/Teleport: Requesting transport to [{link.DestinationName}] (DestID: {link.DestinationId})..."); }
                PacketBuilder.UseTeleport(targetEntity.UniqueID, link.DestinationId);

                // 2c. Varış bekle.
                if (WaitTeleportArrival(link, beforePos, 20))
                {
                    w.Log($"Ferry/Teleport: Successfully arrived at [{link.DestinationName}]!");
                    TeleportManager.Get.NoteLinkSuccess(link);
                    SleepInterruptible(2000); // World loading settle
                    return true;
                }

                // 2d. Başarısız: yanlış dialog açılmış olabilir — kapat,
                // adayı ele, sıradaki adaya geç.
                w.Log($"Ferry/Teleport: [{targetEntity.Name}] teleport etmedi (yanlış NPC olabilir) — dialog kapatılıp sıradaki aday deneniyor.", Theme.LogLevel.Warning);
                try { PacketBuilder.CloseNPC(targetEntity.UniqueID); } catch { }
                MarkBadTeleportUid(targetEntity.UniqueID);
                Thread.Sleep(800);
            }

            if (firstSeen == null && FindBestTeleportCandidate(link) == null)
                w.LogProcess($"Ferry/Teleport: NPC [{link.SourceName}] not found in area!", Window.ProcessState.Warning);
            TeleportManager.Get.NoteLinkFailure(link);
            return false;
        }
        #endregion
    }
}
