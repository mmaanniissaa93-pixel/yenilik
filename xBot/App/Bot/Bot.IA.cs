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
            if (w.Town_cbxEnableTownLoop != null && !w.Town_cbxEnableTownLoop.Checked)
            {
                w.LogProcess("Town Loop is disabled in Town settings.");
                return;
            }

            w.Log("Town Loop: Initiating town logistic sequence...");

            SRCoord myPosition = InfoManager.Character.GetRealtimePosition();
            if (myPosition == null)
                return;

            // Step 1: Repair at Blacksmith
            if (w.Town_cbxRepair == null || w.Town_cbxRepair.Checked)
            {
                TownServiceInfo blacksmith = TownManager.Get.FindNearestService(myPosition, TownServiceType.Blacksmith);
                if (blacksmith != null)
                {
                    w.LogProcess($"Town Loop: Walking to [{blacksmith.NpcName}] for equipment repair...");
                    List<SRCoord> pathToSmith = NavigationManager.Get.FindPath(myPosition, blacksmith.Coord);
                    if (pathToSmith != null && pathToSmith.Count > 0)
                    {
                        for (int i = 0; i < pathToSmith.Count && isBotting; i++)
                        {
                            WaitMovement(pathToSmith[i], 8);
                        }
                    }
                    else
                    {
                        WaitMovement(blacksmith.Coord, 8);
                    }

                    SREntity smithNpc = TownManager.Get.FindLiveNpc(blacksmith);
                    if (smithNpc != null)
                    {
                        WaitSelectEntity(smithNpc.UniqueID, 8, 250, "Selecting Blacksmith...");
                        Thread.Sleep(500);
                        w.Log("Town Loop: Repairing all equipped weapons and armors...");
                        PacketBuilder.RepairAllEquipments(smithNpc.UniqueID);
                        Thread.Sleep(1000);
                    }
                }
            }

            // Step 2: Storage Deposit (Elixirs, Alchemy Stones, SOX)
            myPosition = InfoManager.Character.GetRealtimePosition();
            if (w.Town_cbxStorage == null || w.Town_cbxStorage.Checked)
            {
                TownServiceInfo storage = TownManager.Get.FindNearestService(myPosition, TownServiceType.Storage);
                if (storage != null)
                {
                    w.LogProcess($"Town Loop: Walking to [{storage.NpcName}] for item storage...");
                    List<SRCoord> pathToStorage = NavigationManager.Get.FindPath(myPosition, storage.Coord);
                    if (pathToStorage != null && pathToStorage.Count > 0)
                    {
                        for (int i = 0; i < pathToStorage.Count && isBotting; i++)
                        {
                            WaitMovement(pathToStorage[i], 8);
                        }
                    }
                    else
                    {
                        WaitMovement(storage.Coord, 8);
                    }

                    SREntity storageNpc = TownManager.Get.FindLiveNpc(storage);
                    if (storageNpc != null)
                    {
                        WaitSelectEntity(storageNpc.UniqueID, 8, 250, "Selecting Storage Keeper...");
                        Thread.Sleep(500);
                        PacketBuilder.OpenStorage(storageNpc.UniqueID);
                        Thread.Sleep(1200);

                        ExecuteStorageDeposit();
                    }
                }
            }

            // Step 3: Potion Merchant (Sell trash equipment)
            myPosition = InfoManager.Character.GetRealtimePosition();
            if (w.Town_cbxSellTrash == null || w.Town_cbxSellTrash.Checked)
            {
                TownServiceInfo potionShop = TownManager.Get.FindNearestService(myPosition, TownServiceType.PotionMerchant);
                if (potionShop != null)
                {
                    w.LogProcess($"Town Loop: Walking to [{potionShop.NpcName}] for pharmacy logistics...");
                    List<SRCoord> pathToPotion = NavigationManager.Get.FindPath(myPosition, potionShop.Coord);
                    if (pathToPotion != null && pathToPotion.Count > 0)
                    {
                        for (int i = 0; i < pathToPotion.Count && isBotting; i++)
                        {
                            WaitMovement(pathToPotion[i], 8);
                        }
                    }
                    else
                    {
                        WaitMovement(potionShop.Coord, 8);
                    }

                    SREntity potionNpc = TownManager.Get.FindLiveNpc(potionShop);
                    if (potionNpc != null)
                    {
                        WaitSelectEntity(potionNpc.UniqueID, 8, 250, "Selecting Potion Merchant...");
                        Thread.Sleep(500);

                        ExecuteSellTrash();
                        Thread.Sleep(500);
                        ExecuteAutoBuyPotions(potionNpc);
                    }
                }
            }

            // If user supplied custom town script, execute it too
            if (town != null)
            {
                w.LogProcess("Running town script [" + town.FileName + "]...");
                town.Run(0);
            }

            w.Log("Town Loop: Logistics routine completed. Returning to training area...");
            Thread.Sleep(1500);
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
                    List<SRMob> mobs = InfoManager.Mobs.FindAll(m => trainingPosition.DistanceTo(m.GetRealtimePosition()) <= trainingRadius);

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
                        if (CheckTownReturnConditions() || ProtectionManager.CheckTownReturnTriggers())
                        {
                            TownLoop(null);
                            return;
                        }
                    }

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
                        // Combat AI: Ranged Kiting check
                        if (w.Combat_cbxKiting == null || w.Combat_cbxKiting.Checked)
                        {
                            ExecuteKiting(mob, myPosition);
                        }

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
                if (w.Combat_cbxTargetGeneral != null)
                {
                    bool allowed = true;
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
                    if (!allowed)
                        continue;
                }

                // CombatAI: Skip mobs on Avoid list
                if (CombatAIEngine.ShouldAvoid(m.MobType))
                    continue;

                // CombatAI: Skip Dimension Pillars if configured
                if (CombatAIEngine.IgnoreDimensionPillars && CombatAIEngine.IsDimensionPillar(m))
                    continue;

                double dist = m.GetRealtimePosition().DistanceTo(myPosition);

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
                    Thread.Sleep(4000);
                    return true;
                }
            }
            return false;
        }

        private bool CheckTownReturnConditions()
        {
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return false;

            Window w = Window.Get;
            bool checkHp = w?.Character_cbxUseHP?.Checked ?? true;
            bool checkMp = w?.Character_cbxUseMP?.Checked ?? true;

            byte hpSlot = 0;
            bool hasHp = FindItem(3, 1, 1, ref hpSlot);
            byte mpSlot = 0;
            bool hasMp = FindItem(3, 1, 2, ref mpSlot);

            // Count free inventory slots across full capacity
            int freeSlots = 0;
            var inv = InfoManager.Character.Inventory;
            for (int i = 13; i < inv.Capacity; i++)
            {
                if (inv[i] == null) freeSlots++;
            }

            // Return condition: No HP (if HP pot use enabled), No MP (if MP pot use enabled), or bag completely full (<= 1 slot free)
            bool hpTrigger = checkHp && !hasHp;
            bool mpTrigger = checkMp && !hasMp;
            bool bagTrigger = freeSlots <= 1;

            if (hpTrigger || mpTrigger || bagTrigger)
            {
                byte returnScrollSlot = 0;
                if (FindItem(3, 3, 1, ref returnScrollSlot) || FindItem(3, 3, 2, ref returnScrollSlot) || FindItem(3, 3, 3, ref returnScrollSlot))
                {
                    Window.Get?.Log($"Town Return: Logistic trigger! (HP Pots={hasHp} [check={checkHp}], MP Pots={hasMp} [check={checkMp}], Free Slots={freeSlots}/{inv.Capacity - 13}). Using Return Scroll...");
                    SRItem scrollItem = inv[returnScrollSlot];
                    PacketBuilder.UseItem(scrollItem, returnScrollSlot);
                    Thread.Sleep(5000);
                    return true;
                }
                else
                {
                    Window.Get?.LogProcess($"Town Return: Conditions met (HP={hasHp}, MP={hasMp}, Free={freeSlots}), but no Return Scroll found!", Window.ProcessState.Warning);
                }
            }
            return false;
        }

        private void ExecuteStorageDeposit()
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

                // Elixir (ID2=3, ID3=11, ID4=1) or Alchemy Stone (ID2=3, ID3=11, ID4=2) or SOX item
                bool isElixirOrStone = (item.ID2 == 3 && item.ID3 == 11 && (item.ID4 == 1 || item.ID4 == 2));
                bool isSox = (item is SREquipable eq && eq.GetRarity() != SREquipable.Rarity.None);

                if (isElixirOrStone || isSox)
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
                        w.LogProcess($"Depositing [{item.Name}] to storage slot {emptyStorageSlot}...");
                        PacketBuilder.MoveItem(slot, (byte)emptyStorageSlot, SRTypes.InventoryItemMovement.InventoryToStorage, item.Quantity);
                        Thread.Sleep(350);
                    }
                    else
                    {
                        w.LogProcess("Town Loop: Storage is full!", Window.ProcessState.Warning);
                        break;
                    }
                }
            }
        }

        private void ExecuteSellTrash()
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
                if (item is SREquipable eq && eq.GetRarity() == SREquipable.Rarity.None && !eq.isJob() && !eq.isAvatar())
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
        }

        private void LootDrops(SRCoord trainingPosition, int trainingRadius)
        {
            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return;

            Window w = Window.Get;

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

                            // ItemFilterManager: advanced degree/SoX/race/gender filter
                            if (isEquip && !ItemFilterManager.ShouldPickup(drop))
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

            bool usePet = (w == null || w.Filter_cbxUsePet == null || w.Filter_cbxUsePet.Checked);
            SRCoService pickPet = usePet ? InfoManager.MyPets.Find(p => p.isPickPet()) : null;
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

        private void ExecuteAutoBuyPotions(SREntity potionNpc)
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
