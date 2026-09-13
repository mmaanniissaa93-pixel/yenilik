using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using xBot.App;
using xBot.Game;
using xBot.Game.Objects;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Item;
using xBot.Network;
using SecurityAPI;

public static class BotEngineIntegration
{
    static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    static T Empty<T>() { return (T)FormatterServices.GetUninitializedObject(typeof(T)); }
    static void Set(object obj, string name, object value) { obj.GetType().GetProperty(name).GetSetMethod(true).Invoke(obj, new[] { value }); }
    static void Check(bool ok, string label) { if (!ok) throw new Exception(label); Console.WriteLine("PASS: " + label); }
    static object Call(Bot bot, string name, params object[] args) { return typeof(Bot).GetMethod(name, Private).Invoke(bot, args); }

    public static void Run(Bot bot, Window window)
    {
        var old = InfoManager.Character;
        bool wasInGame = InfoManager.inGame;
        var setter = typeof(InfoManager).GetProperty("Character").GetSetMethod(true);
        var gameSetter = typeof(InfoManager).GetProperty("inGame").GetSetMethod(true);
        var character = Empty<SRCharacter>();
        character.UniqueID = 9001;
        character.Name = "EngineTest";
        character.Position = new SRCoord(100.0, 100.0);
        character.LifeStateType = SRModel.LifeState.Alive;
        character.Inventory = new xList<SRItem>(32);
        var drop = Empty<SRDrop>();
        drop.UniqueID = 9002; drop.Name = "Test Gold"; drop.ServerName = "ITEM_ETC_GOLD_TEST";
        drop.Position = new SRCoord(110.0, 100.0);
        Set(drop, "ID2", (byte)3); Set(drop, "ID3", (byte)5);
        var pet = Empty<SRPickPet>();
        pet.UniqueID = 9003; pet.Position = character.Position;
        Set(pet, "ID4", (byte)4);
        Set(pet, "Inventory", new xList<SRItem>(8));
        try
        {
            // No sockets/proxy: an accidental character move or pickup makes the test fail.
            setter.Invoke(null, new object[] { character });
            gameSetter.Invoke(null, new object[] { true });
            InfoManager.CharacterActions.Reset();
            InfoManager.Entities[drop.UniqueID] = drop;
            InfoManager.MyPets[pet.UniqueID] = pet;
            ItemFilterManager.Reset(); EzFilterManager.Reset();
            window.TrainingArea_SetFromCurrent(character.Position);
            var info = (TrainingAreaInfo)((System.Windows.Forms.ListViewItem)window.Training_lstvAreas.Tag).Tag;
            info.PickRadius = 20;
            ItemFilterManager.Pick.PickItemsFirst = true;
            Check(!(bool)Call(bot, "HasLootableDrops"), "Real priority scan ignores gold when filter is empty");
            Call(bot, "LootDrops", character.Position, 50);
            Check(InfoManager.CharacterActions.CanStart, "Empty filter leaves combat channel ready");
            ItemFilterManager.SetRuleFull(drop.ServerName, false, true, false, false, false);
            Check(ItemFilterManager.ResolveLootActor(drop, true, false) == LootActor.Pet, "Runtime filter resolves server-name pet rule");
            Check(!(bool)Call(bot, "HasLootableDrops"), "Pet-only loot does not preempt character's next fight");
            var timer = Stopwatch.StartNew();
            Call(bot, "LootDrops", character.Position, 50);
            Check(timer.ElapsedMilliseconds < 200 && InfoManager.CharacterActions.CanStart, "Pet-only character loop sends no movement, pickup, or wait");
            Check(Call(bot, "FindLootDrop", LootActor.Pet, character.Position, 20, new PickupQueue()) == drop, "Pet worker finds same eligible drop independently");
            InfoManager.MyPets.RemoveKey(pet.UniqueID);
            Check(!(bool)Call(bot, "HasLootableDrops"), "Missing pet does not implicitly activate character fallback");
            ItemFilterManager.Pick.PickWithCharIfPetGoneFull = true;
            Check((bool)Call(bot, "HasLootableDrops"), "Explicit fallback is honored by actual priority scan");
            info.PickRadius = 4;
            Check(!(bool)Call(bot, "HasLootableDrops"), "Four-meter pickup radius is not expanded to 45 meters");
            info.PickRadius = 20;
            ItemFilterManager.Pick.Enabled = false;
            Check(!(bool)Call(bot, "HasLootableDrops"), "Saved rule cannot reactivate disabled filter");
            ItemFilterManager.Pick.Enabled = true;
            var skill = Empty<SRSkill>();
            skill.ID = 12345; skill.Enabled = true; skill.ServerName = "SKILL_CH_FIRE_GUNG_TEST";
            skill.RequiredWeaponPrimary = SRTypes.Weapon.None;
            skill.RequiredWeaponSecondary = SRTypes.Weapon.None;
            skill.Cooldown = 30000;
            var skills = new[] { skill };
            var weapon = SRTypes.Weapon.Sword;
            double readyRange = (double)Call(bot, "GetEffectiveAttackRange", skills, weapon);
            skill.StartCooldown();
            double coolingRange = (double)Call(bot, "GetEffectiveAttackRange", skills, weapon);
            Check(readyRange == coolingRange && readyRange >= 14, "Real ranged skill keeps range throughout cooldown");
            window.Training_tbxRadius.Text = "73";
            Check(info.Radius == 73 && window.TrainingArea_GetRadius() == 73, "Legacy radius input updates current area model without invalid subitem index");
            RunBuffTests(bot, window, character);
        }
        finally
        {
            InfoManager.Entities.RemoveKey(drop.UniqueID);
            InfoManager.MyPets.RemoveKey(pet.UniqueID);
            gameSetter.Invoke(null, new object[] { wasInGame });
            setter.Invoke(null, new object[] { old });
            InfoManager.CharacterActions.Reset();
            ItemFilterManager.Reset(); EzFilterManager.Reset();
        }
    }

    static SRSkill Skill(uint id, string name, uint cooldown, uint duration)
    {
        var skill = Empty<SRSkill>();
        skill.ID = id; skill.ServerName = name; skill.Name = name; skill.Icon = ""; skill.Params = "";
        skill.Enabled = true; skill.MPUsage = 1; skill.Cooldown = cooldown; skill.DurationMax = duration;
        skill.GroupID = id; skill.RequiredWeaponPrimary = SRTypes.Weapon.None; skill.RequiredWeaponSecondary = SRTypes.Weapon.None;
        typeof(SRSkill).GetField("<RequiredItems>k__BackingField", Private).SetValue(skill, new List<SRSkill.Params_ItemRequired>());
        return skill;
    }
    static void ApplyBuff(SRSkill skill, uint uid)
    {
        var buff = Empty<SRBuff>();
        buff.ID = skill.ID; buff.UniqueID = uid; buff.GroupID = skill.GroupID; buff.DurationMax = skill.DurationMax;
        buff.ServerName = skill.ServerName; buff.Name = skill.Name; buff.Icon = ""; buff.Params = "";
        typeof(InfoManager).GetMethod("OnEntityBuffAdded", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new object[] { InfoManager.Character.UniqueID, buff });
    }
    static void RemoveBuff(uint uid)
    {
        typeof(InfoManager).GetMethod("OnEntityBuffRemoved", BindingFlags.Static | BindingFlags.NonPublic)
            .Invoke(null, new object[] { uid });
    }
    static uint RequestedSkill(Packet packet)
    {
        packet.Lock(); packet.ReadByte(); packet.ReadByte(); return packet.ReadUInt();
    }
    static void RunBuffTests(Bot bot, Window window, SRCharacter character)
    {
        var oldProxy = bot.Proxy;
        string oldElement = SkillManager.SelectedImbue;
        uint oldId = SkillManager.SelectedImbueSkillId;
        bool oldReady = SkillManager.DevilWhenReady;
        bool oldNoAttack = SkillManager.NoAttackMode;
        var remote = new Context(); // No socket or worker: capture production packet serialization only.
        var agent = Empty<Agent>();
        typeof(Agent).GetField("<Remote>k__BackingField", Private).SetValue(agent, remote);
        var proxy = Empty<Proxy>(); Set(proxy, "Agent", agent);
        var outgoing = (List<Packet>)typeof(Security).GetField("m_outgoing_packets", Private).GetValue(remote.Security);
        var imbue = Skill(1377, "SKILL_CH_FIRE_GIGONGTA_A_09", 16000, 16000);
        var attack = Skill(60001, "SKILL_CH_FIRE_GUNG_TEST", 1000, 0);
        var learned = new xDictionary<uint, SRSkill>(); learned[imbue.ID] = imbue; learned[attack.ID] = attack;
        Set(character, "Skills", learned);
        character.Buffs = new xDictionary<uint, SRBuff>();
        Set(character, "MP", (uint)1000);
        character.InventoryAvatar = new xList<SRItem>(8);
        var devilItem = Empty<SREquipable>(); Set(devilItem, "ServerName", "ITEM_EVENT_NASRUN_TEST"); Set(devilItem, "Name", "Devil");
        Set(devilItem, "ID3", (byte)SREquipable.Equipable.DevilSpirit);
        character.InventoryAvatar[0] = devilItem;
        var mob = Empty<SRMob>(); mob.UniqueID = 9900; mob.Position = character.Position; mob.LifeStateType = SRModel.LifeState.Alive;
        var mobs = new List<SRMob> { mob };
        try
        {
            bot.Proxy = proxy;
            InfoManager.CharacterActions.Reset(); SkillManager.ResetCastSession(); SkillManager.BeginBotRun();
            SkillManager.NoAttackMode = false; SkillManager.SelectedImbue = "Fire"; SkillManager.SelectedImbueSkillId = imbue.ID;
            SkillManager.DevilWhenReady = true;
            // The equipped Devil is not a learned skill. Supply fixture DB metadata in its real cache.
            var devil = SkillManager.GetCastSkill(31141);
            devil.ServerName = "SKILL_EVENT_NASRUN_ECCENTRICDEMON_01"; devil.GroupID = 31141;
            devil.Cooldown = 1800000; devil.DurationMax = 600000; devil.CastingTime = 967;
            var timer = Stopwatch.StartNew();
            Check((bool)Call(bot, "MaintainCombatBuffs", mobs, SRMob.Mob.General), "First combat tick sends due imbue");
            Check(timer.ElapsedMilliseconds < 500 && outgoing.Count == 1 && RequestedSkill(outgoing[0]) == imbue.ID, "Startup sends actual imbue packet without fixed multi-second delay");
            Check(!(bool)Call(bot, "MaintainCombatBuffs", mobs, SRMob.Mob.General) && outgoing.Count == 1, "Pending imbue cannot overlap Devil or attack");
            ApplyBuff(imbue, 91001);
            Check(InfoManager.CharacterActions.CanStart && !imbue.isCastingEnabled, "Actual buff-add event acknowledges imbue and starts its cooldown");
            Check((bool)Call(bot, "MaintainCombatBuffs", mobs, SRMob.Mob.General) && outgoing.Count == 2 && RequestedSkill(outgoing[1]) == 31141,
                "Equipped unlearned Devil is sent immediately after imbue confirmation");
            ApplyBuff(devil, 91002);
            Check(!devil.isCastingEnabled, "Server-confirmed auxiliary Devil starts real 30-minute cooldown");
            Check(!SkillManager.CheckDevilSpirit(mobs) && outgoing.Count == 2, "Active Devil is not recast");
            RemoveBuff(91002);
            SkillManager.BeginBotRun();
            Check(!SkillManager.CheckDevilSpirit(mobs) && outgoing.Count == 2, "Stop/start cannot bypass confirmed Devil cooldown");
            RemoveBuff(91001);
            typeof(SRSkill).GetField("m_CooldownTimer", Private).SetValue(imbue, null); // Advance to real imbue expiry.
            Check((bool)Call(bot, "MaintainCombatBuffs", mobs, SRMob.Mob.General) && outgoing.Count == 3 && RequestedSkill(outgoing[2]) == imbue.ID,
                "Expired imbue renews during same encounter without changing target or restarting bot");
            ApplyBuff(imbue, 91003); RemoveBuff(91003);
            typeof(SRSkill).GetField("m_CooldownTimer", Private).SetValue(imbue, null);
            Check(InfoManager.CharacterActions.TryReserve() && !SkillManager.EnsureImbueActive(), "Busy character defers due imbue");
            InfoManager.CharacterActions.Release();
            Check(SkillManager.EnsureImbueActive() && outgoing.Count == 4, "Due imbue survives busy channel and sends on next opportunity");
            InfoManager.CharacterActions.Reset();
            window.AddSkillsBulk(new[] { imbue, attack });
            Check(window.Skills_lstvSkills.Items.Count == 1 && window.Skills_lstvSkills.Items[0].Tag == attack,
                "Real skill list excludes imbue while retaining attack skill");
            Check(SkillManager.GetAvailableImbueSkills().Count == 1, "Excluded imbue remains selectable in its own list");
            Check(!SkillManager.CanUseAttackSkill(attack, 0) && SkillManager.CanUseAttackSkill(attack, 1), "Zero MP no longer authorizes mana-consuming attack");
            attack.Enabled = false;
            Check(!SkillManager.CanUseAttackSkill(attack, 1000), "Disabled attack is not resurrected by usable-skill classification");
            attack.Enabled = true;
            SkillManager.OnAttackRejected(attack.ID, 5);
            Check(!SkillManager.CanUseAttackSkill(attack, 1000) && attack.isCastingEnabled, "Cooldown rejection defers retry without inventing full cooldown");
            Check(!SkillManager.CanUseAttackSkill(imbue, 1000), "Imbue is rejected even if an old attack row survives");
            SkillManager.SelectedImbueSkillId = 0; SkillManager.SelectedImbue = "None";
            var buffSkill = Skill(60002, "SKILL_CH_FIRE_BUFF_PROTECTION", 0, 300000);
            learned[buffSkill.ID] = buffSkill;
            window.Skills_lstvBuffMobType_General.Items.Add(new System.Windows.Forms.ListViewItem(buffSkill.Name) { Tag = buffSkill });
            window.Skills_lstvBuffMobType_General.Items.Add(new System.Windows.Forms.ListViewItem(imbue.Name) { Tag = imbue });
            Check(window.Skills_GetBuffs().Length == 1, "Old imbue buff row is excluded by runtime reader");
            Check((bool)Call(bot, "MaintainCombatBuffs", mobs, SRMob.Mob.General) && outgoing.Count == 5 && RequestedSkill(outgoing[4]) == buffSkill.ID,
                "Ordinary buff is maintained by same action scheduler");
            ApplyBuff(buffSkill, 91004);
            Check(!SkillManager.HasActiveImbue("Fire"), "Fire protection is not mistaken for active fire imbue");
            Check(!(bool)Call(bot, "MaintainCombatBuffs", mobs, SRMob.Mob.General) && outgoing.Count == 5, "Active ordinary buff does not interrupt next attack");
            RemoveBuff(91004);
            Check((bool)Call(bot, "MaintainCombatBuffs", mobs, SRMob.Mob.General) && outgoing.Count == 6, "Ordinary buff renews after actual removal");
            SkillManager.UseDevilSpirit = false;
            Check(!SkillManager.DevilWhenReady, "Legacy Devil checkbox controls same ready setting");
            SkillManager.DevilWhenReady = true;
            Check(SkillManager.UseDevilSpirit, "Ready checkbox is reflected in other Devil screen");
            InfoManager.CharacterActions.Reset();
            attack.RequiredWeaponPrimary = SRTypes.Weapon.Sword; attack.RequiredWeaponSecondary = SRTypes.Weapon.None;
            int packetCount = outgoing.Count;
            Check(!(bool)Call(bot, "TryPrepareAttackSkill", attack, window) && outgoing.Count == packetCount,
                "Empty weapon slot is not a match for unused secondary requirement");
            attack.RequiredWeaponSecondary = SRTypes.Weapon.Blade;
            var spare = Empty<SREquipable>();
            Set(spare, "ID2", (byte)1); Set(spare, "ID3", (byte)6); Set(spare, "ID4", (byte)SRTypes.Weapon.Blade);
            character.Inventory[20] = spare;
            var acknowledgement = new System.Threading.Thread(() => {
                System.Threading.Thread.Sleep(400);
                character.Inventory[6] = spare; character.Inventory[20] = null;
                InfoManager.MonitorWeaponChanged.Set();
            });
            acknowledgement.Start();
            try {
                Check((bool)Call(bot, "TryPrepareAttackSkill", attack, window), "Weapon beyond first inventory row and valid secondary type are accepted after acknowledgement");
                Check(outgoing.Count == packetCount + 1, "Delayed weapon response causes only one move, preventing swap-back");
            } finally { acknowledgement.Join(); }
        }
        finally
        {
            RemoveBuff(91001); RemoveBuff(91002); RemoveBuff(91003); RemoveBuff(91004);
            bot.Proxy = oldProxy;
            SkillManager.SelectedImbue = oldElement; SkillManager.SelectedImbueSkillId = oldId;
            SkillManager.DevilWhenReady = oldReady; SkillManager.NoAttackMode = oldNoAttack;
            SkillManager.ResetCastSession(); InfoManager.CharacterActions.Reset();
        }
    }
}
