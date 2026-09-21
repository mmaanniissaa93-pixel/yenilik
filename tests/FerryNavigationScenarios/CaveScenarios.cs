using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using xBot.Game.Navigation;
using xBot.Game.Objects.Common;
static class CaveScenarios
{
    public static List<TeleportLinkInfo> LoadLinks()
    {
        using var json=JsonDocument.Parse(File.ReadAllText("tests/FerryNavigationScenarios/teleport-links.json"));
        var links=new List<TeleportLinkInfo>();
        foreach(var r in json.RootElement.EnumerateArray())
        {
            int N(string key)=>r.GetProperty(key).GetInt32();
            string S(string key)=>r.GetProperty(key).GetString();
            links.Add(new TeleportLinkInfo {SourceId=(uint)N("sourceid"),DestinationId=(uint)N("destinationid"),NpcId=(uint)N("id"),
                ServerName=S("servername"),SourceName=S("name"),DestinationName=S("destination"),TypeId1=N("tid1"),Gold=N("gold"),MinimumLevel=N("level"),HasEntityModel=N("has_entity")!=0,
                BoardCoord=new SRCoord((ushort)N("spawn_region"),N("spawn_x"),N("spawn_z"),N("spawn_y")),
                ArriveCoord=new SRCoord((ushort)N("pos_region"),N("pos_x"),N("pos_z"),N("pos_y"))});
        }
        foreach(var l in links) l.TransitionMode=TeleportTransitionPolicy.Classify(l,links);
        return links;
    }
    public static void Run(Action<bool,string> check)
    {
        var straight = Enumerable.Range(0, 25).Select(i => new SRCoord(24576.0 + i * 0.5, 24576.0, (ushort)32769)).ToList();
        var compact = NavigationManager.CompactCavePath(straight);
        check(compact.Count == 2 && compact.Last() == straight.Last(), "half-metre straight corridor nodes collapse to one 12m leg");
        var corner = new List<SRCoord> { straight[0], straight[10], new SRCoord(straight[10].PosX, 24581.0, (ushort)32769) };
        check(NavigationManager.CompactCavePath(corner).Count == 3, "cave compaction preserves right-angle wall corners");
        var links=LoadLinks();
        TeleportLinkInfo L(uint id,uint dest=0)=>links.First(l=>l.SourceId==id && (dest==0||l.DestinationId==dest));
        check(L(11).TransitionMode==TransitionMode.WalkTrigger && L(55).TransitionMode==TransitionMode.WalkTrigger,"real DB DW/Jangan entry gates classify WalkTrigger");
        check(L(58).TransitionMode==TransitionMode.WalkTrigger && L(58).BoardCoord.Region==L(58).ArriveCoord.Region,"same-region room portal is a trigger, not an ordinary graph edge");
        check(L(162).TransitionMode==TransitionMode.Interaction,"NpcId=0 Roc ferry remains Interaction");
        check(L(86,1).TransitionMode==TransitionMode.Interaction,"paid cave exit is not mistaken for an automatic trigger");
        var unknown=new TeleportLinkInfo {NpcId=0,TypeId1=4,ServerName="GATE_UNKNOWN",BoardCoord=L(11).BoardCoord,ArriveCoord=L(11).ArriveCoord};
        check(TeleportTransitionPolicy.Classify(unknown,links)==TransitionMode.Interaction,"unpaired zero-id gate remains conservative Interaction");
        var manager=new NavigationManager();
        foreach(var f in Directory.GetFiles("navdata","cnav*.dat"))
        {
            int id=32768+int.Parse(Path.GetFileNameWithoutExtension(f).Substring(4));
            var region=NavDataReader.Read(f,id);
            NavDataReader.TryReadBounds(f,out var x0,out var x1,out var y0,out var y1);
            check(region!=null && Math.Abs(region.MinX-x0)<0.02 && Math.Abs(region.MaxY-y1)<0.02,"cnav normalized point/bounds agreement "+Path.GetFileName(f));
        }
        foreach(uint entry in new uint[]{11,55})
        {
            var arrival=L(entry).ArriveCoord;
            var region=NavDataReader.Read($"navdata/cnav{arrival.Region&32767:00}.dat",arrival.Region);
            int node=region.FindNearestPointIndex((float)arrival.PosX,(float)arrival.PosY);
            check(region.Points[node].DistanceTo((float)arrival.PosX,(float)arrival.PosY)<15,"DB arrival matches normalized cnav "+entry);
            int targetNode=node;
            var visited=new HashSet<int>{node};var pending=new Queue<int>();pending.Enqueue(node);
            while(pending.Count>0 && visited.Count<80) {targetNode=pending.Dequeue();foreach(var n in region.Neighbors[targetNode])if(visited.Add(n))pending.Enqueue(n);}
            var t=region.Points[targetNode];var target=new SRCoord(t.X,t.Y,arrival.Region,(int)unchecked((short)t.Z));
            var path=manager.FindPath(arrival,target);
            check(path!=null && path.Count>1 && path.All(p=>p.Region==arrival.Region),"DW/Jangan same-room A* preserves game region "+entry);
            check(path.Last().DistanceTo(target)<1,"cave route reaches requested mesh target "+entry);
        }
        var a=new SRCoord(24576.0,24576.0,(ushort)32769);
        var b=new SRCoord(24576.0,24576.0,(ushort)32775);
        check(manager.FindPath(a,b)==null && manager.FindMultiRegionRoute(a,b)==null,"overlapping local cave coordinates cannot create a cross-room walk shortcut");
        check(manager.FindApproachPath(a,b)==null,"ferry approach also respects distinct cave regions");
        TeleportManager.Get.Links=links;
        var route=manager.FindCompoundRoute(L(55).BoardCoord,L(58).ArriveCoord);
        var transitions=route?.Segments.Where(s=>s.Type==RouteSegmentType.Teleport).Select(s=>s.TeleportLink.SourceId).ToArray();
        Console.WriteLine("CAVE ROUTE "+(transitions==null?"null":string.Join(" -> ",transitions)));
        check(transitions!=null && transitions.Contains(55u) && transitions.Contains(57u) && transitions.Contains(58u),"real DB multi-room route uses entry, floor trigger and same-region room trigger");
        check(route.Segments.Last().Type==RouteSegmentType.Walk && route.Segments.Last().Waypoints.All(p=>p.Region==32774),"post-trigger segment retains destination cave region");
        TriggerExecution(check,L(11),L(58));
    }
    static void TriggerExecution(Action<bool,string> check,TeleportLinkInfo entry,TeleportLinkInfo room)
    {
        SRCoord current=entry.BoardCoord; bool active=true,loading=false;int walks=0,polls=0;var logs=new List<string>();
        var nav=new WalkTriggerNavigator {Position=()=>current,Active=()=>active,Loading=()=>loading,Log=logs.Add,Pause=_=>{polls++;},
            FindPath=(pos,failed)=>new List<SRCoord>{pos},Walk=(point,interrupt)=>{walks++;current=point;return true;}};
        // Stand just outside the trigger; there is no candidate callback in this executor.
        current=new SRCoord(entry.BoardCoord.PosX+5,entry.BoardCoord.PosY);
        nav.Walk=(point,interrupt)=>{walks++;current=point;if(current.DistanceTo(entry.BoardCoord)<0.3)current=entry.ArriveCoord;return interrupt() || true;};
        check(nav.Execute(entry) && walks>1,"walk-trigger crosses the center and succeeds without NPC/entity interaction");
        var savedTrigger=entry.TriggerCoord;
        entry.TriggerCoord=new SRCoord(entry.BoardCoord.PosX+6,entry.BoardCoord.PosY,entry.BoardCoord.Region,entry.BoardCoord.Z);
        current=new SRCoord(entry.BoardCoord.PosX+5,entry.BoardCoord.PosY);walks=0;loading=false;
        nav.Walk=(point,interrupt)=>{walks++;current=point;if(current.DistanceTo(entry.TriggerCoord)<0.3)current=entry.ArriveCoord;return true;};
        check(nav.Execute(entry) && walks>=3,"walk-trigger uses a separate doorway continuation after the DB board point");
        entry.TriggerCoord=savedTrigger;
        current=new SRCoord(entry.BoardCoord.PosX+5,entry.BoardCoord.PosY);walks=0;loading=false;
        nav.Walk=(point,interrupt)=>{walks++;current=point;if(current.PosX < entry.BoardCoord.PosX-4) current=entry.ArriveCoord;return true;};
        check(nav.Execute(entry) && walks >= 3,"walk-trigger keeps moving through the gate when the center alone does not fire");
        current=new SRCoord(entry.BoardCoord.PosX+5,entry.BoardCoord.PosY);walks=polls=0;
        nav.Walk=(point,interrupt)=>{walks++;current=point;if(point.PosX < entry.BoardCoord.PosX - 1)current=entry.ArriveCoord;return true;};
        check(nav.Execute(entry), "DW entry crosses beyond the DB center when the trigger starts behind it");
        current=new SRCoord(entry.BoardCoord.PosX+5,entry.BoardCoord.PosY);walks=polls=0;
        nav.Walk=(point,interrupt)=>{walks++;current=point;if(current.DistanceTo(entry.BoardCoord)<0.3)loading=true;return true;};
        nav.Pause=_=>{polls++;if(polls==4){loading=false;current=entry.ArriveCoord;}};
        check(nav.Execute(entry) && polls==4,"asynchronous loading waits for destination and sends no source commands while loading");
        current=entry.BoardCoord;walks=polls=0;loading=false;
        nav.Pause=_=>{polls++;};nav.Walk=(point,interrupt)=>{walks++;return true;};
        check(!nav.Execute(entry) && polls==200,"merely standing on trigger is not success; two bounded transition timeouts");
        current=new SRCoord(entry.BoardCoord.PosX+30,entry.BoardCoord.PosY);walks=0;
        check(!nav.Execute(entry) && walks==0,"off-mesh gap beyond 16m never triggers blind movement");
        check(TeleportTransitionPolicy.HasArrived(room.BoardCoord,room.ArriveCoord,room.ArriveCoord),"same-region room relocation counts as transition success");
        check(!TeleportTransitionPolicy.HasArrived(entry.BoardCoord,new SRCoord(entry.BoardCoord.PosX+10,entry.BoardCoord.PosY),entry.ArriveCoord),"ordinary movement or sector changes do not count as cave arrival");
        active=false;walks=0;
        check(!nav.Execute(entry) && walks==0,"stopped bot cannot issue trigger movement");
    }
}

