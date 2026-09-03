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

        #region (Handle everything about botting)
        /// <summary>
        /// Start botting.
        /// </summary>
        public void Start()
        {
            if (InfoManager.inGame && !isBotting)
            {
                tBotting = new Thread(this.ThreadBotting);
                tBotting.Priority = ThreadPriority.AboveNormal;
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
                tBotting.Abort();
                tBotting = null;
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
            while (true)
            {
                // Checking where am I ?
                w.LogProcess("Checking current location...");
                SRCoord myPosition = InfoManager.Character.GetRealtimePosition();
                currentScript = Script.GetNearestTownScript(myPosition, 50);
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
                            }

                            // 2. Fallback: User movement script
                            string scriptPath = w.TrainingArea_GetScript();
                            if (!string.IsNullOrEmpty(scriptPath) && File.Exists(scriptPath))
                            {
                                w.LogProcess("Loading training script...");
                                currentScript = new Script(scriptPath);
                                int nearIndex = currentScript.GetNearMovement(myPosition, 80);
                                if (nearIndex != -1)
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
                                // 3. Fallback: Direct walk to training position with collision avoidance
                                w.Log("Walking towards training area with collision avoidance...");
                                if (!WaitMovement(trainingPosition, 15))
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
        private void TownLoop(Script town)
        {
            Window w = Window.Get;
            w.LogProcess("In town. Checking status...");

            byte hpSlot = 0, mpSlot = 0;
            bool hasHP = FindItem(3, 1, 1, ref hpSlot);
            bool hasMP = FindItem(3, 1, 2, ref mpSlot);
            w.Log($"Town Status: HP Potions={hasHP}, MP Potions={hasMP}");

            // Eğer kasaba scripti varsa çalıştır
            if (town != null)
            {
                w.LogProcess("Running town script [" + town.FileName + "]...");
                town.Run(0);
            }

            // Kasılma alanına dönüş scriptini kontrol et
            string scriptPath = w.TrainingArea_GetScript();
            if (!string.IsNullOrEmpty(scriptPath) && File.Exists(scriptPath))
            {
                w.Log("Starting route to training area...");
                currentScript = new Script(scriptPath);
                currentScript.Run(0);
            }
            else
            {
                w.Log("Town finished. Please set a script to walk to training area.");
                Stop();
            }
        }
        private void AttackLoop()
        {
            Window w = Window.Get;

            SRCoord myPosition, trainingPosition;
            int trainingRadius;

            bool doMovement = true;
            while (true)
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
                            if (!myPosition.Equals(trainingPosition))
                            {
                                // Move and wait
                                timeTraveling = myPosition.TimeTo(trainingPosition, InfoManager.Character.GetMovementSpeed());
                                MoveTo(trainingPosition);
                                w.LogProcess("Walking to center (" + timeTraveling + "ms)...");
                                WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged, InfoManager.MonitorBuffRemoved }, timeTraveling);
                            }
                        }
                        else
                        {
                            // Random walk
                            int random = rand.Next(-trainingRadius, trainingRadius);
                            // Take care about where am I
                            SRCoord newPosition;
                            if (trainingPosition.inDungeon())
                                newPosition = new SRCoord(trainingPosition.PosX + random, trainingPosition.PosY + random, trainingPosition.Region, trainingPosition.Z);
                            else
                                newPosition = new SRCoord(trainingPosition.PosX + random, trainingPosition.PosY + random);
                            // Move and wait
                            timeTraveling = myPosition.TimeTo(trainingPosition, InfoManager.Character.GetMovementSpeed());
                            MoveTo(newPosition);
                            w.LogProcess("Walking randomly (" + timeTraveling + "ms)...");
                            WaitHandle.WaitAny(new WaitHandle[] { InfoManager.MonitorMobSpawnChanged, InfoManager.MonitorBuffRemoved }, timeTraveling);
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

                if (trainingRadius > 0)
                {
                    // Attacking
                    List<SRMob> mobs = InfoManager.Mobs.FindAll(m => trainingPosition.DistanceTo(m.GetRealtimePosition()) <= trainingRadius);
                    SRMob mob = GetMobFiltered(mobs);
                    if (mob == null)
                    {
                        // No mob to attack
                        w.LogProcess("No mobs around to attack");
                        LootDrops(trainingPosition, trainingRadius);
                        doMovement = true;
                        continue;
                    }
                    else
                    {
                        // Load skills and iterate it
                        SRSkill[] skillshots = w.Skills_GetSkillShots(mob.MobType);
                        if (skillshots != null && skillshots.Length != 0)
                        {
                            // Try to select mob
                            if (WaitSelectEntity(mob.UniqueID, 2, 250, "Selecting " + mob.Name + " (" + mob.MobType + ")..."))
                            {
                                // Iterate skills
                                for (int k = 0; k <= skillshots.Length; k++)
                                {
                                    // loop control
                                    if (k == skillshots.Length)
                                        break;
                                    SRSkill skillshot = skillshots[k];

                                    // Check if skill is enabled
                                    if (!skillshot.isCastingEnabled)
                                        continue;

                                    // Check and fix the weapon used for this skillshot
                                    SRTypes.Weapon myWeapon = GetMyWeaponType();
                                    if (skillshot.ID == 1)
                                    {
                                        // Common attack, fix the basic skill
                                        if (myWeapon != SRTypes.Weapon.None)
                                        {
                                            skillshot = new SRSkill(DataManager.GetCommonAttack(myWeapon));
                                            skillshot.Name = "Common Attack";
                                        }
                                    }
                                    else
                                    {
                                        // Check the required weapon
                                        SRTypes.Weapon weaponRequired = skillshot.RequiredWeaponPrimary;
                                        w.LogProcess("Checking weapon required (" + weaponRequired + ")...");
                                        while (myWeapon != weaponRequired)
                                        {
                                            // Check the first 4 slots from inventory
                                            int slotInventory = InfoManager.Character.Inventory.FindIndex(item => item != null && item.ID2 == 1 && item.ID3 == 6 && item.ID4 == (byte)weaponRequired, 13, 16);
                                            if (slotInventory != -1)
                                            {
                                                w.LogProcess("Changing weapon (" + myWeapon + ")...");
                                                // Try to change it
                                                byte maxWeaponChangeAttempts = 5; // Check max. 4 times to skip the mob (max. 1 seconds actually)
                                                while (myWeapon != weaponRequired && maxWeaponChangeAttempts > 0)
                                                {
                                                    PacketBuilder.MoveItem((byte)slotInventory, 6, SRTypes.InventoryItemMovement.InventoryToInventory);
                                                    maxWeaponChangeAttempts--;
                                                    InfoManager.MonitorWeaponChanged.WaitOne(250);
                                                    myWeapon = GetMyWeaponType();
                                                }
                                                if (maxWeaponChangeAttempts == 0)
                                                {
                                                    w.LogProcess("Weapon changing failed!");
                                                    continue;
                                                }
                                            }
                                            else
                                            {
                                                w.LogProcess("Weapon required not found (" + myWeapon + ")...");
                                                continue;
                                            }
                                            InfoManager.MonitorWeaponChanged.WaitOne(250);
                                            myWeapon = GetMyWeaponType();
                                        }
                                    }
                                    // Check if mob is alive
                                    if (InfoManager.Mobs.ContainsKey(mob.UniqueID))
                                    {
                                        w.LogProcess("Casting skill " + skillshot.Name + " (" + skillshot.CastingTime + "ms)...");
                                        PacketBuilder.AttackTarget(mob.UniqueID, skillshot.ID);
                                        if (InfoManager.MonitorSkillCast.WaitOne(500))
                                        {
                                            // Skill casted, create character cooldown
                                            Thread.Sleep(skillshot.CastingTime);
                                        }
                                        else
                                        {
                                            // Timeout: Skill not casted
                                            if (!InfoManager.Mobs.ContainsKey(mob.UniqueID))
                                            {
                                                // Mob it's dead?
                                                break;
                                            }
                                            else
                                            {
                                                // Recast skillshot
                                                k--;
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        // mob selection failed
                                        break;
                                    }
                                }

                                // Mob öldükten sonra drop topla
                                LootDrops(trainingPosition, trainingRadius);
                            }
                        }
                        else
                        {
                            w.LogProcess("Skillshots not found");
                        }
                    }
                }
            }
        }

        private SRMob GetMobFiltered(List<SRMob> mobs)
        {
            SRMob mob = null;
            // Get nearest around me
            SRCoord myPosition = InfoManager.Character.GetRealtimePosition();
            double minDistance = 0;
            for (int j = 0; j < mobs.Count; j++)
            {
                double d = mobs[j].GetRealtimePosition().DistanceTo(myPosition);
                if (mob == null || d < minDistance)
                {
                    minDistance = d;
                    mob = mobs[j];
                }
            }
            return mob;
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
        }

        private void LootDrops(SRCoord trainingPosition, int trainingRadius)
        {
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return;

            // Çantada boş yer var mı? (13. slottan itibaren)
            int emptySlot = InfoManager.Character.Inventory.FindIndex(i => i == null, 13);
            if (emptySlot == -1)
                return; // Envanter dolu

            SRCoord myPos = InfoManager.Character.GetRealtimePosition();
            List<SRDrop> drops = new List<SRDrop>();

            for (int i = 0; i < InfoManager.Entities.Count; i++)
            {
                SREntity entity = InfoManager.Entities.GetAt(i);
                if (entity is SRDrop drop)
                {
                    double dist = drop.GetRealtimePosition().DistanceTo(myPos);
                    if (dist <= 35.0)
                    {
                        drops.Add(drop);
                    }
                }
            }

            if (drops.Count == 0)
                return;

            // En yakından uzağa sırala
            drops.Sort((a, b) => a.GetRealtimePosition().DistanceTo(myPos).CompareTo(b.GetRealtimePosition().DistanceTo(myPos)));

            SRCoService pickPet = InfoManager.MyPets.Find(p => p.isPickPet());
            uint petId = pickPet != null ? pickPet.UniqueID : 0;

            for (int d = 0; d < drops.Count && d < 3; d++)
            {
                SRDrop drop = drops[d];
                if (!InfoManager.isEntityNear(drop.UniqueID))
                    continue;

                if (petId != 0)
                {
                    PacketBuilder.PickUpItem(drop.UniqueID, petId);
                    Thread.Sleep(200);
                }
                else
                {
                    // Karakter ile toplama
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

        private void WalkLoop()
        {

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

                // Çarpışma / Takılma Algılayıcı (Stuck Detection)
                if (lastPosition != null && myPosition.DistanceTo(lastPosition) < 0.6)
                {
                    stuckCounter++;
                    if (stuckCounter >= 2)
                    {
                        // 2 denemede ilerleyemedi -> Önünde engel/duvar var!
                        Window.Get.LogProcess("Collision detected! Executing obstacle bypass maneuver...");

                        // Hedefe doğru olan vektör
                        double dx = position.PosX - myPosition.PosX;
                        double dy = position.PosY - myPosition.PosY;
                        double dist = Math.Sqrt(dx * dx + dy * dy);
                        if (dist < 0.001) dist = 1.0;

                        // Normalleştirilmiş birim vektör
                        double nx = dx / dist;
                        double ny = dy / dist;

                        // 90 derece teğet dik vektör (sağa veya sola)
                        double perpX = avoidToRight ? -ny : ny;
                        double perpY = avoidToRight ? nx : -nx;
                        avoidToRight = !avoidToRight; // Bir sonraki takılmada ters yöne dene

                        // 4.5 metre yana kaçınma koordinatı
                        double avoidStep = 4.5;
                        SRCoord bypassCoord;
                        if (myPosition.inDungeon())
                            bypassCoord = new SRCoord(myPosition.PosX + perpX * avoidStep, myPosition.PosY + perpY * avoidStep, myPosition.Region, myPosition.Z);
                        else
                            bypassCoord = new SRCoord(myPosition.PosX + perpX * avoidStep, myPosition.PosY + perpY * avoidStep);

                        MoveTo(bypassCoord);
                        Thread.Sleep(900);
                        stuckCounter = 0;
                        continue;
                    }
                }
                else
                {
                    stuckCounter = 0;
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
                    Thread.Sleep(2000); // World loading settle
                    return true;
                }
            }

            w.LogProcess("Ferry/Teleport: Teleport transition timed out. Trying once more...", Window.ProcessState.Warning);
            PacketBuilder.UseTeleport(targetEntity.UniqueID, link.DestinationId);
            Thread.Sleep(2000);
            return InfoManager.Character.GetRealtimePosition().DistanceTo(beforePos) > 40.0;
        }
        #endregion
    }
}
