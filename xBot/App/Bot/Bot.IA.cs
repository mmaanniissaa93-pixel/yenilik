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
                    try
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
            if (w.Town_cbxEnableTownLoop != null && !w.Town_cbxEnableTownLoop.Checked)
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

            // Town sekmesindeki "çöp sat" onayı script dışı çağrılarda da çalışsın:
            // potion satıcısına uğranmadıysa bile envanterdeki sell-kurallı eşyalar
            // burada elden çıkarılır (script zaten sattıysa döngü boş geçer).
            try
            {
                bool sellTrash = false;
                w.Town_cbxSellTrash.InvokeIfRequired(() => { sellTrash = w.Town_cbxSellTrash.Checked; });
                if (sellTrash)
                    ExecuteSellTrash();
            }
            catch { }

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
                                // Move and wait
                                timeTraveling = myPosition.TimeTo(trainingPosition, InfoManager.Character.GetMovementSpeed());
                                MoveTo(trainingPosition);
                                w.LogProcess("Walking to center (" + timeTraveling + "ms)...");
                                WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged, InfoManager.MonitorBuffRemoved }, Math.Min(timeTraveling, 3000));
                            }
                        }
                        else
                        {
                            // Avoid blind random walk across terrain / sectors. If drifted, return to center.
                            if (myPosition.DistanceTo(trainingPosition) > 15.0)
                            {
                                timeTraveling = myPosition.TimeTo(trainingPosition, InfoManager.Character.GetMovementSpeed());
                                MoveTo(trainingPosition);
                                w.LogProcess("Returning towards center (" + timeTraveling + "ms)...");
                                WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged, InfoManager.MonitorBuffRemoved }, Math.Min(timeTraveling, 3000));
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

                // Check auto alchemy (+ basma)
                if (AlchemyManager.IsRunning)
                {
                    AlchemyManager.RunTick();
                }

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

                    // Ensure Imbue & Devil Spirit are active before combat
                    SkillManager.EnsureImbueActive();
                    SkillManager.CheckDevilSpirit();

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

                    // Combat AI: Check Berserker activation
                    if (w.Combat_cbxAutoBerserk == null || w.Combat_cbxAutoBerserk.Checked)
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

                    SRMob mob = GetMobFiltered(mobs, trainingPosition, trainingRadius);
                    if (mob == null)
                    {
                        // No mob to attack
                        w.LogProcess("No mobs around to attack");
                        LootDrops(trainingPosition, trainingRadius);
                        WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged }, 1000);
                        if (myPosition.DistanceTo(trainingPosition) > 20.0)
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
                        double maxAttackRange = GetWeaponAttackRange(myWeapon);
                        SRCoord mobPosition = mob.GetRealtimePosition();
                        myPosition = InfoManager.Character.GetRealtimePosition();
                        double distanceToMob = myPosition.DistanceTo(mobPosition);

                        if (distanceToMob > maxAttackRange)
                        {
                            w.LogProcess($"Approaching {mob.Name} ({distanceToMob:F1}m)...");
                            MoveTo(mobPosition);
                            int approachAttempts = 0;
                            while (isBotting && InfoManager.Mobs.ContainsKey(mob.UniqueID) && approachAttempts < 20)
                            {
                                Thread.Sleep(100);
                                myPosition = InfoManager.Character.GetRealtimePosition();
                                mobPosition = mob.GetRealtimePosition();
                                if (myPosition.DistanceTo(mobPosition) <= maxAttackRange)
                                    break;
                                approachAttempts++;
                                if (approachAttempts % 5 == 0)
                                    MoveTo(mobPosition);
                            }
                        }

                        // Quick target selection (does not stall attack loop if confirmation is delayed)
                        if (InfoManager.SelectedEntityUniqueID != mob.UniqueID)
                        {
                            PacketBuilder.SelectEntity(mob.UniqueID);
                            InfoManager.MonitorEntitySelected.WaitOne(80);
                        }

                        int currentSkillIndex = 0;
                        int noSkillAttempts = 0;

                            while (isBotting && InfoManager.Mobs.ContainsKey(mob.UniqueID))
                            {
                                if (SkillManager.NoAttackMode)
                                    break;

                                // NOT: Imbue/Devil zaten mob başında (dış döngüde) kontrol ediliyor;
                                // her vuruşta tekrar taramak iç döngüde mikro-takılma yapıyordu.
                                // Distance check to target
                                myWeapon = GetMyWeaponType();
                                maxAttackRange = GetWeaponAttackRange(myWeapon);
                                mobPosition = mob.GetRealtimePosition();
                                myPosition = InfoManager.Character.GetRealtimePosition();
                                distanceToMob = myPosition.DistanceTo(mobPosition);

                                if (distanceToMob > maxAttackRange)
                                {
                                    MoveTo(mobPosition);
                                    Thread.Sleep(200);
                                    continue;
                                }

                                SRSkill[] skillshots = w.Skills_GetSkillShots(mob.MobType);
                                SRSkill skillToCast = null;
                                uint currentMP = InfoManager.Character != null ? InfoManager.Character.MP : 0;

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
                                                        if (!candidate.Enabled)
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
                                            if (candidate != null && candidate.ID != 1 && candidate.Enabled && candidate.isCastingEnabled
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
                                    noSkillAttempts++;
                                    if (noSkillAttempts >= 2 && SkillPolicy.ShouldUseFallback(InfoManager.Mobs.ContainsKey(mob.UniqueID), false))
                                    {
                                        TryFallbackAttack(mob, w);
                                        noSkillAttempts = 0;
                                    }
                                    else
                                    {
                                        Thread.Sleep(60);
                                    }
                                    continue;
                                }

                                noSkillAttempts = 0;

                                if (skillToCast.ID == 1)
                                {
                                    skillToCast = SkillManager.GetFallbackAttack(GetMyWeaponType());
                                }

                                if (!InfoManager.Mobs.ContainsKey(mob.UniqueID))
                                    break;

                                w.LogProcess("Casting skill " + skillToCast.Name + " (" + skillToCast.CastingTime + "ms)...");

                                long hpBefore = 0;
                                try { hpBefore = mob.HP; } catch { }

                                InfoManager.LastSkillCastSuccess = false;
                                InfoManager.LastSkillCastErrorCode = 0;
                                InfoManager.MonitorSkillCast.Reset();

                                PacketBuilder.AttackTarget(mob.UniqueID, skillToCast.ID);

                                bool confirmed = InfoManager.MonitorSkillCast.WaitOne(800);
                                bool treatAsSuccess = confirmed && InfoManager.LastSkillCastSuccess;
                                if (!treatAsSuccess)
                                {
                                    // Geciken onay / kayıp paket: mob canı azaldıysa veya öldüyse
                                    // cast tutmuş demektir, başarısız sayıp bekleme.
                                    bool mobGone = !InfoManager.Mobs.ContainsKey(mob.UniqueID);
                                    long hpAfter = 0;
                                    try { hpAfter = mob.HP; } catch { }
                                    if (mobGone || (hpBefore > 0 && hpAfter < hpBefore))
                                        treatAsSuccess = true;
                                }

                                if (treatAsSuccess)
                                {
                                    SkillManager.RecordCastSuccess(skillToCast);
                                    try { skillToCast.StartCooldown(); } catch { }

                                    // Smooth pipeline: tam cast süresi kadar kör bekleme; sıradaki
                                    // skill hazır olur olmaz devam et. Üst üste reddediliyorsa
                                    // server erken ateşe izin vermiyor demektir, tam bekle.
                                    int castTime = Math.Max(0, skillToCast.CastingTime);
                                    int fullWait = Math.Max(350, castTime);
                                    int minGap = SkillManager.ConsecutiveCastFailures >= 2
                                        ? fullWait
                                        : Math.Max(250, (castTime * 3) / 4);
                                    int elapsed = 0;
                                    while (elapsed < fullWait && isBotting)
                                    {
                                        if (!InfoManager.Mobs.ContainsKey(mob.UniqueID))
                                            break;
                                        if (elapsed >= minGap && IsAnyAttackSkillReady(skillshots))
                                            break;
                                        Thread.Sleep(30);
                                        elapsed += 30;
                                    }
                                }
                                else
                                {
                                    string reason = !confirmed ? "Zaman aşımı"
                                        : (InfoManager.LastSkillCastErrorCode == 0 ? "Sunucu reddetti"
                                            : $"Reddedildi (0x{InfoManager.LastSkillCastErrorCode:X4})");
                                    SkillManager.RecordCastFailure(skillToCast, reason);
                                    if (!InfoManager.Mobs.ContainsKey(mob.UniqueID))
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
                if (s == null || !s.Enabled || !s.isCastingEnabled)
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

        private bool TryFallbackAttack(SRMob mob, Window w)
        {
            if (mob == null || !InfoManager.Mobs.ContainsKey(mob.UniqueID))
                return false;

            SRSkill fallback = SkillManager.GetFallbackAttack(GetMyWeaponType());
            if (fallback == null)
                return false;

            w.LogProcess("Casting Common Attack fallback...");
            InfoManager.LastSkillCastSuccess = false;
            InfoManager.LastSkillCastErrorCode = 0;
            InfoManager.MonitorSkillCast.Reset();

            PacketBuilder.AttackTarget(mob.UniqueID, 1u);
            if (InfoManager.MonitorSkillCast.WaitOne(1200))
            {
                if (InfoManager.LastSkillCastSuccess)
                {
                    SkillManager.RecordCastSuccess(fallback);
                    int elapsed = 0;
                    int animTime = Math.Max(400, fallback.CastingTime);
                    while (elapsed < animTime && isBotting)
                    {
                        if (!InfoManager.Mobs.ContainsKey(mob.UniqueID))
                            break;
                        Thread.Sleep(50);
                        elapsed += 50;
                    }
                    return true;
                }
            }

            SkillManager.RecordCastFailure(fallback, "Common Attack yanıt vermedi");
            Thread.Sleep(80);
            return false;
        }

        private SRMob GetMobFiltered(List<SRMob> mobs, SRCoord trainingPosition, int trainingRadius)
        {
            if (mobs == null || mobs.Count == 0)
                return null;

            SRMob bestMob = null;
            double bestScore = double.MinValue;
            SRCoord myPosition = InfoManager.Character.GetRealtimePosition();

            Window w = Window.Get;
            bool enablePriority = (w.Combat_cbxMobPriority == null || w.Combat_cbxMobPriority.Checked);

            for (int j = 0; j < mobs.Count; j++)
            {
                SRMob m = mobs[j];

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

                if (!CombatPolicy.CanTarget(new CombatTargetInput
                {
                    AllowedByType = allowed,
                    Avoided = CombatAIEngine.ShouldAvoid(m.MobType),
                    IsDimensionPillar = CombatAIEngine.IsDimensionPillar(m),
                    IgnoreDimensionPillars = CombatAIEngine.IgnoreDimensionPillars,
                    DoNotFollowMobs = CombatAIEngine.DoNotFollowMobs,
                    WithinTrainingArea = withinTrainingArea
                }))
                    continue;

                double dist = mobPosition.DistanceTo(myPosition);

                // If priority is disabled, strictly choose the nearest mob
                if (!enablePriority)
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

                // CombatAI: Bonus for preferred mob types
                if (CombatAIEngine.IsPreferred(m.MobType))
                    score += 200.0;

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

        private void CheckBerserker(List<SRMob> nearbyMobs)
        {
            if (InfoManager.Character == null)
                return;

            // Only trigger if Berserker bar is 100% full (5 points)
            if (InfoManager.Character.BerserkPoints < 5)
                return;

            // Don't trigger if already in Berserk mode
            if (InfoManager.Character.SpeedBerserk > 0)
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
                Window.Get?.Log("Combat AI: Berserker trigger! Activating Berserk mode!");
                PacketBuilder.ActivateBerserk();
                Thread.Sleep(400);
            }
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
                    return 14.0;
                default:
                    return 3.5;
            }
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

        public void ExecuteStorageDeposit()
        {
            Window w = Window.Get;
            if (InfoManager.Character == null || InfoManager.Character.Storage == null)
                return;

            var inv = InfoManager.Character.Inventory;
            var storage = InfoManager.Character.Storage;

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
                        PacketBuilder.MoveItem(slot, (byte)emptyStorageSlot, SRTypes.InventoryItemMovement.InventoryToStorage, item.Quantity);
                        // Server yankısı (0xB034) gelmezse paket reddedilmiş demektir:
                        // üst üste göndermek kick yedirir, o yüzden dur.
                        if (!InfoManager.MonitorInventoryMovement.WaitOne(2000))
                        {
                            w.Log($"STORE: [{item.Name}] server tarafından kabul edilmedi (yankı yok) — depozit durduruldu.");
                            break;
                        }
                        Thread.Sleep(350);
                    }
                    else
                    {
                        w.LogProcess("Town Loop: Storage is full!", Window.ProcessState.Warning);
                        break;
                    }
                }
            }

            // Unload and store items from active Pick Pet
            if (InfoManager.MyPets != null)
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
                                break;
                            }

                            // Find empty slot in character inventory to intermediate pet to storage
                            int emptyCharSlot = inv.FindIndex(i => i == null, 13);
                            if (emptyCharSlot != -1)
                            {
                                w.Log($"Transferring [{pItem.Name}] from pet slot {pSlot} to inventory...");
                                InfoManager.MonitorInventoryMovement.Reset();
                                PacketBuilder.MoveItem(pSlot, (byte)emptyCharSlot, SRTypes.InventoryItemMovement.PetToInventory, pickPet.UniqueID);
                                if (!InfoManager.MonitorInventoryMovement.WaitOne(2000))
                                {
                                    w.Log($"STORE: pet transferi kabul edilmedi — depozit durduruldu.");
                                    break;
                                }
                                Thread.Sleep(350);

                                w.Log($"Depositing [{pItem.Name}] x{pItem.Quantity} (inv:{emptyCharSlot} -> stor:{emptyStorageSlot})...");
                                InfoManager.MonitorInventoryMovement.Reset();
                                PacketBuilder.MoveItem((byte)emptyCharSlot, (byte)emptyStorageSlot, SRTypes.InventoryItemMovement.InventoryToStorage, pItem.Quantity);
                                if (!InfoManager.MonitorInventoryMovement.WaitOne(2000))
                                {
                                    w.Log($"STORE: [{pItem.Name}] server tarafından kabul edilmedi (yankı yok) — depozit durduruldu.");
                                    break;
                                }
                                Thread.Sleep(350);
                            }
                            else
                            {
                                w.LogProcess("Town Loop: Character inventory full while transferring pet items.", Window.ProcessState.Warning);
                                break;
                            }
                        }
                    }
                }
            }

            ReportUnhandledFilterItems(w);
        }

        /// <summary>
        /// StoreGuild / Dismantle kurallarına takılan eşyaları raporlar.
        /// (Guild deposu açma ve dismantle paketleri bu server sürümünde doğrulanmadığı
        /// için motor bunları uygulamaz — sadece sayı verir, eşya çantada kalır.)
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
                    w.Log($"StoreGuild: {guildCount} eşya guild deposu istiyor (bu sürümde desteklenmiyor, çantada bırakıldı).");
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
            if (opt == null || !opt.Enabled)
                return;
            if (InfoManager.Character == null || !isBotting)
                return;
            try
            {
                ulong invGold = InfoManager.Character.Gold;
                ulong storGold = InfoManager.Character.StorageGold;

                // 1. Depoya altın koy (keep üstü).
                if (opt.StoreGoldInStorage && invGold > opt.GoldKeepAmount)
                {
                    ulong amount = invGold - opt.GoldKeepAmount;
                    if (opt.StoreGoldMax > 0 && storGold + amount > opt.StoreGoldMax)
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
                        InfoManager.MonitorInventoryMovement.WaitOne(2000);
                        Thread.Sleep(400);
                    }
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
                            InfoManager.MonitorInventoryMovement.WaitOne(2000);
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
                if (ItemFilterManager.Pick.DontPickItems)
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

        private void LootDrops(SRCoord trainingPosition, int trainingRadius)
        {
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return;

            Window w = Window.Get;

            if (ItemFilterManager.Pick.DontPickItems)
                return;

            // Çantada boş yer var mı? (13. slottan itibaren)
            int emptySlot = InfoManager.Character.Inventory.FindIndex(i => i == null, 13);
            if (emptySlot == -1 && !ItemFilterManager.Pick.PickEvenWhenFull)
                return; // Envanter dolu

            SRCoord myPos = InfoManager.Character.GetRealtimePosition();

            bool wantPet = ItemFilterManager.Pick.UsePickPet
                && (w == null || w.Filter_cbxUsePet == null || w.Filter_cbxUsePet.Checked);
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

            List<SRDrop> drops = new List<SRDrop>();

            for (int i = 0; i < InfoManager.Entities.Count; i++)
            {
                SREntity entity = InfoManager.Entities.GetAt(i);
                if (entity is SRDrop drop)
                {
                    double dist = drop.GetRealtimePosition().DistanceTo(myPos);
                    if (dist <= 35.0)
                    {
                        // Apply Pick Filters
                        if (w != null && w.Filter_cbxPickGold != null)
                        {
                            bool isGold = drop.isGold();
                            bool isEquip = drop.isEquipable();
                            bool isElixirStone = (drop.ID2 == 3 && drop.ID3 == 11 && (drop.ID4 == 1 || drop.ID4 == 2));
                            bool isMaterial = (!isGold && !isEquip && !isElixirStone);

                            if (isGold && !w.Filter_cbxPickGold.Checked)
                                continue;
                            if (isEquip && !w.Filter_cbxPickEquip.Checked)
                                continue;
                            if (isElixirStone && !w.Filter_cbxPickElixirStone.Checked)
                                continue;
                            if (isMaterial && !w.Filter_cbxPickMaterials.Checked)
                                continue;

							// Basic UI filters are followed by the persistent item filter.
						}

						// Karakter listesi (Pick) VEYA pet listesi (Pet): ikisi de
						// bağımsızdır, biri tutsa damla listeye girer.
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
            }

            if (drops.Count == 0)
                return;

            // En yakından uzağa sırala
            drops.Sort((a, b) => a.GetRealtimePosition().DistanceTo(myPos).CompareTo(b.GetRealtimePosition().DistanceTo(myPos)));

            for (int d = 0; d < drops.Count && d < 3; d++)
            {
                SRDrop drop = drops[d];
                if (!InfoManager.isEntityNear(drop.UniqueID))
                    continue;

                // Pick ve Pet sütunları bağımsızdır:
                // Pick=Yes -> karakter yürüyüp toplar (pet açık olsa bile).
                // Pet=Yes  -> pet kapar. İkisi de Yes ise ikisi de dener.
                // Pet-only (Pick=No) + pet yok/dolu + karakter yedeği açıksa karakter toplar.
                var dropRule = ItemFilterManager.GetRule(drop.Name) ?? ItemFilterManager.GetRule(drop.ServerName);
                bool petOnly = dropRule != null && dropRule.Pet && !dropRule.Pickup;
                bool petWants = petId != 0 && ItemFilterManager.ShouldUsePet(drop, true, petFull);
                bool charWants = ItemFilterManager.ShouldPickup(drop);
                if (petOnly && !petWants && ItemFilterManager.Pick.PickWithCharIfPetGoneFull)
                    charWants = true;

                if (petWants)
                {
                    PacketBuilder.PickUpItem(drop.UniqueID, petId);
                    Thread.Sleep(200);
                }

                if (charWants)
                {
                    // Pet kapmış olabilir; paket boşa giderse zararsız, yine dene.
                    if (!InfoManager.isEntityNear(drop.UniqueID))
                        continue;
                    double dist = myPos.DistanceTo(drop.GetRealtimePosition());
                    if (dist > 4.0)
                    {
                        MoveTo(drop.GetRealtimePosition());
                        Thread.Sleep(300);
                    }
                    PacketBuilder.PickUpItem(drop.UniqueID);
                    Thread.Sleep(300);
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
                w.LogProcess($"Auto Buy: Purchasing {missingHp} HP potions from {potionNpc.Name}...");
                byte slotInShop = (byte)hpIndex;
                PacketBuilder.BuyItemFromShop(0, slotInShop, (ushort)missingHp, potionNpc.UniqueID);
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
                w.LogProcess($"Auto Buy: Purchasing {missingMp} MP potions from {potionNpc.Name}...");
                byte slotInShop = (byte)(5 + mpIndex);
                PacketBuilder.BuyItemFromShop(0, slotInShop, (ushort)missingMp, potionNpc.UniqueID);
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
                    w.LogProcess($"Auto Buy: Purchasing {missingPills} Universal Pills...");
                    PacketBuilder.BuyItemFromShop(0, 13, (ushort)missingPills, potionNpc.UniqueID);
                    Thread.Sleep(600);
                }
            }

            // 4. Return Scrolls
            int currentScrolls = CountReturnScrolls();
            int targetScrolls = 5;
            int missingScrolls = targetScrolls - currentScrolls;
            if (missingScrolls > 0)
            {
                w.LogProcess($"Auto Buy: Purchasing {missingScrolls} Return Scrolls...");
                PacketBuilder.BuyItemFromShop(0, 14, (ushort)missingScrolls, potionNpc.UniqueID);
                Thread.Sleep(600);
            }
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
            bool isBow = (ammoType == AmmoType.Arrow);

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
            if (groceryNpc != null)
            {
                WaitSelectEntity(groceryNpc.UniqueID, 8, 250, "Selecting Grocery Merchant...");
                Thread.Sleep(500);

                byte shopSlot = TownLogisticsPolicy.GetAmmoShopSlot(ammoType);
                string ammoName = isBow ? "Arrows" : "Bolts";
                w.LogProcess($"Auto Buy: Purchasing {ammoName} from {groceryNpc.Name}...");
                // Buy 2 stacks of ammunition
                PacketBuilder.BuyItemFromShop(0, shopSlot, 1, groceryNpc.UniqueID);
                Thread.Sleep(600);
                PacketBuilder.BuyItemFromShop(0, shopSlot, 1, groceryNpc.UniqueID);
                Thread.Sleep(600);

                // Auto-equip ammunition if secondary slot 7 is empty
                var inv = InfoManager.Character.Inventory;
                if (inv != null && inv.Capacity > 7 && (inv[7] == null || inv[7].Quantity == 0))
                {
                    byte invSlot = 0;
                    if (Bot.Get.FindItem(3, 1, 7, ref invSlot))
                    {
                        w.LogProcess("Auto Equip: Equipping ammunition to slot 7...");
                        PacketBuilder.MoveItem(invSlot, 7, SRTypes.InventoryItemMovement.InventoryToInventory);
                        Thread.Sleep(500);
                    }
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
                if (ReturnToAreaPolicy.UseSpeedDrug)
                    TryUseSpeedDrug();
                if (ReturnToAreaPolicy.UseMount)
                    TrySummonMount();
            }
            catch { }
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
                    // Hareket varsa bypass serisini sıfırla (yönü koru)
                    if (lastPosition != null && myPosition.DistanceTo(lastPosition) > 2.0)
                        bypassCount = 0;
                }
                lastPosition = myPosition;

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

        private bool ExecuteTeleportTransition(TeleportLinkInfo link)
        {
            Window w = Window.Get;
            w.Log($"Ferry/Teleport: Transitioning [{link.SourceName}] -> [{link.DestinationName}]...");

            // 1. Locate NPC / Teleport entity across all collections
            SREntity targetEntity = null;
            for (int attempt = 0; attempt < 30 && isBotting; attempt++)
            {
                // Search Npcs first (ferry ticket sellers are NPCs)
                targetEntity = InfoManager.Npcs.Find(npc => npc.ID == link.NpcId ||
                    (npc.Name != null && npc.Name.IndexOf(link.SourceName, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (npc.Position != null && npc.Position.DistanceTo(link.BoardCoord) <= 40.0));

                // Search TeleportAndBuildings
                if (targetEntity == null)
                {
                    targetEntity = InfoManager.TeleportAndBuildings.Find(tp => tp.ID == link.NpcId ||
                        (tp.Name != null && tp.Name.IndexOf(link.SourceName, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (tp.Position != null && tp.Position.DistanceTo(link.BoardCoord) <= 40.0));
                }

                // Search all Entities as fallback
                if (targetEntity == null)
                {
                    targetEntity = InfoManager.Entities.Find(e => e.ID == link.NpcId ||
                        (e.Name != null && e.Name.IndexOf(link.SourceName, StringComparison.OrdinalIgnoreCase) >= 0) ||
                        (e.Position != null && e.Position.DistanceTo(link.BoardCoord) <= 40.0));
                }

                if (targetEntity != null)
                    break;

                Thread.Sleep(300);
            }

            if (targetEntity == null)
            {
                w.LogProcess($"Ferry/Teleport: NPC [{link.SourceName}] not found in area!", Window.ProcessState.Warning);
                return false;
            }

            w.Log($"Ferry/Teleport: Found NPC [{targetEntity.Name}] (UID: {targetEntity.UniqueID}, ModelID: {targetEntity.ID})");

            // 2. Walk close to NPC if not already within 4 meters
            SRCoord myPos = InfoManager.Character.GetRealtimePosition();
            if (targetEntity.Position != null && myPos.DistanceTo(targetEntity.Position) > 4.0)
            {
                w.LogProcess($"Walking to ferry NPC ({myPos.DistanceTo(targetEntity.Position):F1}m)...");
                WaitMovement(targetEntity.Position, 6);
            }

            // 3. Select NPC
            w.LogProcess($"Selecting NPC [{targetEntity.Name}]...");
            WaitSelectEntity(targetEntity.UniqueID, 10, 250, "Selecting ferry ticket seller...");
            Thread.Sleep(600);

            // 4. Send UseTeleport packet
            w.Log($"Ferry/Teleport: Requesting transport to [{link.DestinationName}] (DestID: {link.DestinationId})...");
            PacketBuilder.UseTeleport(targetEntity.UniqueID, link.DestinationId);

            // 5. Wait for teleport / loading / position shift
            SRCoord beforePos = InfoManager.Character.GetRealtimePosition();
            for (int wait = 0; wait < 35 && isBotting; wait++)
            {
                Thread.Sleep(500);
                SRCoord currentPos = InfoManager.Character.GetRealtimePosition();
                if (currentPos.DistanceTo(beforePos) > 40.0 || currentPos.DistanceTo(link.ArriveCoord) < 70.0)
                {
                    w.Log($"Ferry/Teleport: Successfully arrived at [{link.DestinationName}]!");
                    SleepInterruptible(2000); // World loading settle
                    return true;
                }
            }

            w.LogProcess("Ferry/Teleport: Teleport transition timed out. Trying once more...", Window.ProcessState.Warning);
            PacketBuilder.UseTeleport(targetEntity.UniqueID, link.DestinationId);
            SleepInterruptible(2000);
            return InfoManager.Character.GetRealtimePosition().DistanceTo(beforePos) > 40.0;
        }
        #endregion
    }
}
