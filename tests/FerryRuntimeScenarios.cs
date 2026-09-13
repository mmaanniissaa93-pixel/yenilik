using System;
using System.Collections.Generic;
using System.Reflection;
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
        Console.WriteLine("4 production assembly checks passed; live ferry transit remains untested.");
    }
}

