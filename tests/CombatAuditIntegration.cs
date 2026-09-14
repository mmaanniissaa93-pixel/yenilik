using System;
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

public static class CombatAuditIntegration
{
    static BindingFlags instance = BindingFlags.Instance | BindingFlags.NonPublic;
    static BindingFlags statics = BindingFlags.Static | BindingFlags.NonPublic;
    static T Empty<T>() { return (T)FormatterServices.GetUninitializedObject(typeof(T)); }
    static void Set(object obj, string name, object value) { obj.GetType().GetProperty(name).GetSetMethod(true).Invoke(obj, new[] { value }); }
    static void Info(string name, object value) { typeof(InfoManager).GetProperty(name).GetSetMethod(true).Invoke(null, new[] { value }); }
    static void Check(bool ok, string label) { if (!ok) throw new Exception(label); Console.WriteLine("PASS: " + label); }
    static System.Timers.Timer Timer(Bot bot, string name) { return (System.Timers.Timer)typeof(Bot).GetField(name, instance).GetValue(bot); }
    static SRItem Item(uint id, byte type3, byte type4, string name)
    {
        var item = Empty<SRItem>(); Set(item, "ID", id); Set(item, "ID1", (byte)3); Set(item, "ID2", (byte)3);
        Set(item, "ID3", type3); Set(item, "ID4", type4); Set(item, "Name", name); Set(item, "ServerName", name);
        item.Quantity = 10; item.QuantityMax = 50; return item;
    }
    static SRSkill Skill(uint id, string name)
    {
        var skill = Empty<SRSkill>(); skill.ID = id; skill.Name = name; skill.ServerName = name; skill.Params = "";
        skill.Enabled = true; skill.GroupID = id;
        typeof(SRSkill).GetField("<RequiredItems>k__BackingField", instance).SetValue(skill, new List<SRSkill.Params_ItemRequired>());
        return skill;
    }
    static SRPlayer Player(uint id, string name, double x, bool dead)
    {
        var p = Empty<SRPlayer>(); p.UniqueID = id; p.Name = name; p.Position = new SRCoord(x, 100.0);
        p.LifeStateType = dead ? SRModel.LifeState.Dead : SRModel.LifeState.Alive;
        p.Buffs = new xDictionary<uint, SRBuff>(); return p;
    }
    static uint Target(Packet p) { p.Lock(); p.ReadByte(); p.ReadByte(); p.ReadUInt(); p.ReadByte(); return p.ReadUInt(); }
    static void ReadySupport() { InfoManager.CharacterActions.Reset(); typeof(PartySupportManager).GetField("lastSupportActionUtc", statics).SetValue(null, DateTime.MinValue); }

    public static void Run(Bot bot, Window window)
    {
        var oldCharacter = InfoManager.Character; var oldParty = InfoManager.Party; bool oldGame = InfoManager.inGame; var oldProxy = bot.Proxy;
        var originalPlayers = InfoManager.Players.Snapshot();
        var remote = new Context(); // No socket, no proxy worker, no server connection.
        var agent = Empty<Agent>(); typeof(Agent).GetField("<Remote>k__BackingField", instance).SetValue(agent, remote);
        var proxy = Empty<Proxy>(); Set(proxy, "Agent", agent);
        var packets = (List<Packet>)typeof(Security).GetField("m_outgoing_packets", instance).GetValue(remote.Security);
        try
        {
            Info("inGame", false); Info("Character", null); bot.Proxy = proxy;
            bot.CheckUsingHP(); bot.CheckUsingMP(); bot.CheckUsingVigor(); bot.CheckUsingUniversal(); bot.CheckUsingPurification();
            Check(packets.Count == 0, "All potion callbacks tolerate a disconnected character");
            Check(!bot.UseReturnScroll(), "Return helper reports failure without a character");
            Check(typeof(Window).GetField("Combat_cbxKiting") == null && typeof(Bot).GetMethod("ExecuteKiting", instance) == null,
                "Kiting checkbox, field and runtime method are removed");
            Check(typeof(Bot).GetMethod("CheckPanicEscape", instance) == null, "Hidden panic escape no longer bypasses protection settings");

            var character = Empty<SRCharacter>(); character.UniqueID = 98000; character.Name = "AuditFixture"; character.ServerName = "CHAR_CH_TEST";
            character.Position = new SRCoord(100.0, 100.0); character.LifeStateType = SRModel.LifeState.Alive;
            character.Inventory = new xList<SRItem>(32); character.Buffs = new xDictionary<uint, SRBuff>();
            Set(character, "HPMax", (uint)100); Set(character, "MPMax", (uint)100); Set(character, "HP", (uint)20); Set(character, "MP", (uint)90);
            Info("Character", character);
            character.Inventory[13] = Item(98001, 1, 3, "ITEM_ETC_ALL_POTION_TEST");
            window.Character_tbxUseHPVigor.Text = "50"; window.Character_tbxUseMPVigor.Text = "50";
            window.Character_cbxUseHPVigor.Checked = false; window.Character_cbxUseMPVigor.Checked = true;
            bot.CheckUsingVigor();
            Check(packets.Count == 0, "MP-only Vigor ignores low HP when MP is sufficient");
            window.Character_cbxUseHPVigor.Checked = true; window.Character_cbxUseMPVigor.Checked = false;
            Set(character, "HP", (uint)90); Set(character, "MP", (uint)20);
            bot.CheckUsingVigor();
            Check(packets.Count == 0, "HP-only Vigor ignores low MP when HP is sufficient");
            Set(character, "HP", (uint)20); bot.CheckUsingVigor();
            Check(packets.Count == 1 && Timer(bot, "tUsingVigor").Enabled, "Enabled low-HP Vigor sends one packet and starts cooldown");
            Timer(bot, "tUsingVigor").Stop();
            character.Inventory[13] = null; character.Inventory[14] = Item(98002, 1, 3, "ITEM_ETC_ALL_POTION_TEST2");
            typeof(Bot).GetField("m_lastUseItemUtc", statics).SetValue(null, DateTime.MinValue);
            bot.Proxy = null; bot.CheckUsingVigor();
            Check(!Timer(bot, "tUsingVigor").Enabled, "Failed Vigor send does not start a fifteen-second cooldown");
            character.Inventory[15] = Item(98003, 3, 1, "ITEM_ETC_SCROLL_RETURN_01");
            Check(!bot.UseReturnScroll(), "Return helper propagates packet send failure");
            bot.Proxy = proxy;
            character.Inventory[15] = null; character.Inventory[16] = Item(98009, 3, 1, "ITEM_ETC_SCROLL_RETURN_01");
            Check(bot.UseReturnScroll(), "Return helper reports an actual queued return packet");

            var general = Skill(98004, "GeneralSkill"); var champion = Skill(98005, "ChampionSkill");
            var lists = new Dictionary<SRMob.Mob, SRSkill[]>(); lists[SRMob.Mob.General] = new[] { general }; lists[SRMob.Mob.Champion] = new[] { champion };
            var resolve = typeof(Window).GetMethod("ResolveAttackSkillList", statics);
            CombatAIEngine.UseLowerSkills = true;
            Check(((SRSkill[])resolve.Invoke(null, new object[] { lists, SRMob.Mob.Giant }))[0] == champion, "Giant fallback chooses configured Champion before General");
            CombatAIEngine.UseLowerSkills = false;
            Check(((SRSkill[])resolve.Invoke(null, new object[] { lists, SRMob.Mob.Giant })).Length == 0, "Disabling lower skills disables fallback");
            Check(((SRSkill[])resolve.Invoke(null, new object[] { lists, SRMob.Mob.Champion }))[0] == champion, "Exact mob-type skills remain enabled without fallback");
            var mob = Empty<SRMob>(); mob.BadStatusFlags = SRModel.BadStatus.Freezing;
            var hasDot = typeof(Bot).GetMethod("HasDamageOverTime", statics);
            Check(!(bool)hasDot.Invoke(null, new object[] { mob }), "Freezing is not treated as DOT");
            mob.BadStatusFlags = SRModel.BadStatus.Bleed;
            Check((bool)hasDot.Invoke(null, new object[] { mob }), "Bleeding is treated as DOT");
            var teleport = Skill(98006, "Teleport"); teleport.Params = "0|1952803890|500|125|0";
            var distance = typeof(Bot).GetMethod("GetTeleportDistance", statics);
            Check((double)distance.Invoke(null, new object[] { teleport }) == 12.5, "Teleport distance comes from skill data in decimetres");

            Info("inGame", true); Info("Party", null); InfoManager.Players.Clear();
            var buff = Skill(98007, "AuditBuff"); var ress = Skill(98008, "AuditRess");
            var skills = new xDictionary<uint, SRSkill>(); skills[buff.ID] = buff; skills[ress.ID] = ress; Set(character, "Skills", skills);
            var dead = Player(98100, "Dead", 102, true); var far = Player(98101, "Far", 800, false);
            var first = Player(98102, "First", 102, false); var second = Player(98103, "Second", 104, false);
            InfoManager.Players[dead.Name] = dead; InfoManager.Players[far.Name] = far;
            InfoManager.Players[first.Name] = first; InfoManager.Players[second.Name] = second;
            PartySupportManager.PartyAutoRessEnabled = false; PartySupportManager.PartyHealEnabled = false; PartySupportManager.PartyCureEnabled = false;
            PartySupportManager.PartyBuffsEnabled = true; PartySupportManager.BuffAndReturnToCenter = false; PartySupportManager.BuffAllNearbyPlayers = true;
            PartySupportManager.PartyBuffAssignments.Clear(); PartySupportManager.PartyBuffAssignments.Add(new PartyBuffAssignment { PlayerName = "*", SkillId = buff.ID });
            int count = packets.Count; ReadySupport(); PartySupportManager.RunTick();
            Check(packets.Count == count + 1 && Target(packets[count]) == first.UniqueID, "Wildcard buff skips dead and distant first matches");
            count = packets.Count; ReadySupport(); PartySupportManager.RunTick();
            Check(packets.Count == count + 1 && Target(packets[count]) == second.UniqueID, "Wildcard buff advances to the next eligible player");
            PartySupportManager.PartyBuffsEnabled = false; PartySupportManager.PartyAutoRessEnabled = true;
            ResurrectPolicy.ResurrectRegexList.Clear(); ResurrectPolicy.ResurrectRegexList.Add("^Dead$");
            ResurrectPolicy.SelectedResSkills.Clear(); ResurrectPolicy.SelectedResSkills.Add(ress.Name); ResurrectPolicy.ResurrectDelayMs = 0;
            count = packets.Count; ReadySupport(); PartySupportManager.RunTick();
            Check(packets.Count == count + 1 && Target(packets[count]) == dead.UniqueID, "Non-party regex resurrection works while the character has no party");
        }
        finally
        {
            foreach (string name in new[] { "tUsingHP", "tUsingMP", "tUsingVigor", "tUsingUniversal", "tUsingPurification" }) Timer(bot, name).Stop();
            Info("inGame", oldGame); Info("Character", oldCharacter); Info("Party", oldParty); bot.Proxy = oldProxy;
            InfoManager.Players.Clear(); foreach (var player in originalPlayers) InfoManager.Players[player.Name] = player;
            InfoManager.CharacterActions.Reset(); ProtectionManager.ResetRuntimeState();
            PartySupportManager.PartyBuffAssignments.Clear(); ResurrectPolicy.ResurrectRegexList.Clear(); ResurrectPolicy.SelectedResSkills.Clear();
        }
    }
}
