using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using xBot.Game.Navigation;
using System.Runtime.Serialization;
using System.Threading;
using xBot.App;
using xBot.Game;
using xBot.Game.Objects.Common;
using xBot.Game.Objects.Entity;
class FerryRuntimeScenarios
{
    static void Check(bool value, string description)
    {
        if (!value) throw new Exception(description);
        Console.WriteLine("PASS " + description);
    }
    static void Main()
    {
        // Uses the built production assembly. No network or live server is involved.
        var model=(SRModel)FormatterServices.GetUninitializedObject(typeof(SRModel));
        model.Position=new SRCoord(100,100);model.SpeedWalking=1000;
        model.MovementPosition=new SRCoord(102,100);
        Thread.Sleep(50);
        Check(model.GetRealtimePosition().DistanceTo(model.MovementPosition)<0.05,
            "real SRModel interpolates an entire two-meter server destination instead of freezing within three meters");
        var chr=(SRCharacter)FormatterServices.GetUninitializedObject(typeof(SRCharacter));
        chr.Position=new SRCoord(100,100);
        typeof(InfoManager).GetProperty("Character").GetSetMethod(true).Invoke(null,new object[]{chr});
        typeof(InfoManager).GetProperty("inGame").GetSetMethod(true).Invoke(null,new object[]{true});
        var bot=(Bot)FormatterServices.GetUninitializedObject(typeof(Bot));
        typeof(Bot).GetField("tBotting",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(bot,Thread.CurrentThread);
        Check(bot.WaitMovement(new SRCoord(102,100),0),"normal WaitMovement retains default three-meter arrival tolerance");
        Check(!bot.WaitMovement(new SRCoord(102,100),0,0.75),"tight ferry tolerance does not silently accept a two-meter waypoint; maxAttempts is an attempt count");
        Check(bot.WaitMovement(new SRCoord(100.5,100),0,0.75),"ferry tolerance accepts a genuinely reached nav waypoint");
        // Exercise the actual SQLite loader without initializing the UI or the network.
        var deployedDb=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Data","Silkroad #1","Database.sqlite3");
        if(File.Exists(deployedDb))
        {
            var teleports=(TeleportManager)FormatterServices.GetUninitializedObject(typeof(TeleportManager));
            typeof(TeleportManager).GetField("m_links",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(teleports,new List<TeleportLinkInfo>());
            typeof(TeleportManager).GetMethod("TryLoadFromDatabase",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(teleports,null);
            var dw=teleports.Links.Single(l=>l.SourceId==11 && l.DestinationId==10);
            var jangan=teleports.Links.Single(l=>l.SourceId==55 && l.DestinationId==56);
            Check(dw.MinimumLevel==50 && !dw.HasEntityModel && dw.TypeId1==4 && dw.ServerName=="GATE_DUNGEON_DH_IN",
                "production SQLite loader preserves DW gate classification metadata");
            Check(TeleportTransitionPolicy.Classify(dw,teleports.Links)==TransitionMode.WalkTrigger
                && TeleportTransitionPolicy.Classify(jangan,teleports.Links)==TransitionMode.WalkTrigger,
                "production assembly classifies deployed DW/Jangan records as WalkTrigger");
            var roc=teleports.Links.Single(l=>l.SourceId==162 && l.DestinationId==160);
            Check(TeleportTransitionPolicy.Classify(roc,teleports.Links)==TransitionMode.Interaction,
                "production assembly preserves deployed zero-id Roc interaction");
        }
        else Console.WriteLine("SKIP deployed SQLite loader: database not present");
        Console.WriteLine("Production assembly checks passed; live server transit remains untested.");
    }
}
