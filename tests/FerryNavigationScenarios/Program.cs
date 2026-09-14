using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using xBot.Game.Navigation;
using xBot.Game.Objects.Common;
class Program
{
    static int checks;
    static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        checks++; Console.WriteLine("PASS " + message);
    }
    static NavRegion Mesh(params NavPoint[] points) => new NavRegion { Points = points, Neighbors = new int[points.Length][] };
    static void Main()
    {
        var astar = new AStarPathfinder();
        var mesh = Mesh(new NavPoint(0,0,0,10,0), new NavPoint(1,2,3,11,0), new NavPoint(2,4,0,12,0), new NavPoint(3,5,0,12,0));
        mesh.Neighbors = new[] { new[] {1}, new[] {0,2}, new[] {1}, new int[0] };
        var path = astar.FindPath(mesh,0,0,5,0,true);
        Check(path.Count == 3 && path[1].PosY == 3 && path.Last().PosX == 4, "closest reachable node wins over disconnected closest node; bend preserved");
        Check(path.All(p=>p.PosX != 5), "off-mesh / disconnected board is never appended");
        Check(astar.FindPath(mesh,0,0,1,0,true).Count == 1 && astar.FindPath(mesh,0,0,1,0,true)[0].PosX == 0, "same-node path returns node, not exact target");
        path = astar.FindPath(mesh,0,0,5,0,true,new HashSet<int>{1});
        Check(path.Count == 1, "blocked node is not traversed during recovery");
        Check(astar.FindPath(mesh,200,200,5,0,true) == null, "far-away start cannot snap across terrain");
        var wire = new SRCoord((ushort)23406, 1383, 10, 1558);
        Check(Math.Abs(wire.PosX - (-4661.7)) < 0.001 && Math.Abs(wire.PosY - (-36.2)) < 0.001, "wire decoder preserves decimeter precision in negative world coordinates");
        foreach (var coord in new[] {new SRCoord(-4661.8,-36.2,10), new SRCoord(-4607.8,-0.2,10), new SRCoord(0.2,192.2,10)})
        {
            var roundtrip = new SRCoord(coord.Region,coord.X,coord.Z,coord.Y);
            Check(roundtrip.DistanceTo(coord) < 0.15, "world / wire roundtrip " + coord);
        }
        var mixed = new NavigationRoute();
        mixed.Segments.Add(RouteSegment.CreateWalk(new List<SRCoord>{new SRCoord(0,0)}));
        mixed.Segments.Add(RouteSegment.CreateTeleport(new TeleportLinkInfo()));
        mixed.Segments.Add(RouteSegment.CreateWalk(new List<SRCoord>{new SRCoord(100,0)}));
        Check(NavigationManager.FlattenWalkOnly(mixed) == null, "Walk -> Teleport -> Walk cannot be flattened");
        mixed.Segments.RemoveAt(1);
        Check(NavigationManager.FlattenWalkOnly(mixed).Count == 2, "explicit walk-only route preserves waypoints");
        var region = NavDataReader.Read(Path.Combine("navdata","nav06.dat"),6);
        var start = new SRCoord(-4636,-31);
        var board = new SRCoord(-4661.8,-36.2);
        path = astar.FindPath(region,(float)start.PosX,(float)start.PosY,(float)board.PosX,(float)board.PosY,true);
        Check(path != null && path.Count > 1 && path.Last().DistanceTo(board) < 6, "actual nav06 stall fixture reaches mesh within 6m of board");
        var indices = path.Select(p=>region.FindNearestPointIndex((float)p.PosX,(float)p.PosY)).ToArray();
        Check(Enumerable.Range(1,indices.Length-1).All(i=>region.Neighbors[indices[i-1]].Contains(indices[i])), "every nav06 step is an actual topology edge");
        Console.WriteLine("OFFLINE FIXTURE nodes=" + string.Join(" -> ",indices) + " boardDistance=" + path.Last().DistanceTo(board));
        var manager = new NavigationManager();
        var managerPath = manager.FindApproachPath(start,board);
        Check(managerPath != null && managerPath.Last().DistanceTo(board)<6, "production NavigationManager selects valid nav06 approach");
        Check(managerPath.All(p=>p.Region == 23406), "outdoor packet regions derive from world sectors, not nav file id 6");
        var shortPath = manager.FindApproachPath(path[0], new SRCoord(path[0].PosX+1,path[0].PosY+1));
        Check(shortPath.All(p=>region.Points.Any(n=>Math.Abs(n.X-p.PosX)<0.01 && Math.Abs(n.Y-p.PosY)<0.01)), "under-5m approach still uses actual mesh nodes");
        TeleportManager.Get.TestLink = new TeleportLinkInfo { BoardCoord = board, ArriveCoord = new SRCoord(12000,5000) };
        var compound = manager.FindCompoundRoute(start, new SRCoord(12005,5000));
        Check(compound != null && compound.Segments[0].Type == RouteSegmentType.Teleport,
            "inside final-approach range compound route hands off directly, without geometric board stand");
        TeleportManager.Get.TestLink = null;
        // Inject small region fixtures to exercise strict multi-region routing independently of installed data.
        InjectRegions(manager,
            MakeRegion(101,new[]{0f,5f,10f}), MakeRegion(102,new[]{10f,15f,20f}));
        Check(manager.FindMultiRegionRoute(new SRCoord(0,0),new SRCoord(20,0),true)?.TotalWaypointsCount == 6,
            "multi-region approach uses a shared node and preserves both graph legs");
        InjectRegions(manager,
            MakeRegion(101,new[]{0f,5f,10f}), MakeRegion(102,new[]{11f,15f,20f}));
        Check(manager.FindMultiRegionRoute(new SRCoord(0,0),new SRCoord(20,0),true) == null,
            "adjacent bounding boxes without a shared mesh node cannot create a blind crossing");
        ExecutorScenarios();
        CaveScenarios.Run(Check);
        Console.WriteLine("Passed " + checks + " checks. Live server movement is NOT tested here.");
    }
    static NavRegion MakeRegion(int id,float[] xs)
    {
        var r = Mesh(xs.Select((x,i)=>new NavPoint(i,x,0,0,0)).ToArray());
        r.RegionId=id;r.MinX=xs.First();r.MaxX=xs.Last();r.MinY=r.MaxY=0;
        r.Neighbors=new[]{new[]{1},new[]{0,2},new[]{1}};return r;
    }
    static void InjectRegions(NavigationManager manager, params NavRegion[] regions)
    {
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var bounds=regions.Select(r=>new NavigationManager.RegionBoundsEntry {RegionId=r.RegionId,MinX=r.MinX,MaxX=r.MaxX,MinY=0,MaxY=0,FilePath="fixture"}).ToList();
        typeof(NavigationManager).GetField("m_boundsIndex",flags).SetValue(manager,bounds);
        var cacheType=typeof(NavigationManager).GetNestedType("CacheEntry",BindingFlags.NonPublic);
        var linkedType=typeof(LinkedList<>).MakeGenericType(cacheType);
        var order=Activator.CreateInstance(linkedType);
        var nodeType=typeof(LinkedListNode<>).MakeGenericType(cacheType);
        var mapType=typeof(Dictionary<,>).MakeGenericType(typeof(int),nodeType);
        var map=Activator.CreateInstance(mapType);
        foreach(var r in regions)
        {
            var entry=Activator.CreateInstance(cacheType);
            cacheType.GetField("RegionId").SetValue(entry,r.RegionId);cacheType.GetField("Region").SetValue(entry,r);
            var node=linkedType.GetMethod("AddFirst",new[]{cacheType}).Invoke(order,new[]{entry});
            mapType.GetMethod("Add").Invoke(map,new[]{(object)r.RegionId,node});
        }
        typeof(NavigationManager).GetField("m_cacheOrder",flags).SetValue(manager,order);
        typeof(NavigationManager).GetField("m_cacheMap",flags).SetValue(manager,map);
        typeof(NavigationManager).GetMethod("BuildRegionGraph",flags).Invoke(manager,null);
    }
    static void ExecutorScenarios()
    {
        var pos=new SRCoord(0,0); bool active=true, visible=false; int walks=0,plans=0; var logs=new List<string>();
        var nav=new FerryApproachNavigator {
            Position=()=>pos, Active=()=>active, CandidateVisible=()=>visible, Pause=_=>{}, Log=logs.Add,
            FindPath=(current,failed)=>{plans++;return new List<SRCoord>{new SRCoord(5,0),new SRCoord(10,0),new SRCoord(15,0)};},
            Walk=(target,interrupt)=>{walks++;pos=new SRCoord(3,0);visible=true;return interrupt();}
        };
        Check(nav.Approach(new SRCoord(20,0)) && walks==1 && pos.PosX==3, "candidate spawning DURING waypoint interrupts before remaining waypoints");
        visible=true;walks=0;plans=0;
        Check(nav.Approach(new SRCoord(20,0)) && walks==0 && plans==0, "existing candidate bypasses approach entirely");
        visible=false;walks=plans=0;pos=new SRCoord(0,0);
        nav.FindPath=(current,failed)=>{plans++;Check(current==pos,"replan uses latest runtime position"); Check(failed.Count==plans-1,"failed targets carried to recovery");return new List<SRCoord>{new SRCoord(plans*5,0)};};
        nav.Walk=(target,interrupt)=>{walks++;pos=new SRCoord(walks,0);return false;};
        Check(!nav.Approach(new SRCoord(20,0)) && plans==3 && walks==3,"movement failure has exactly two bounded replans");
        plans=walks=0;logs.Clear();
        nav.FindPath=(current,failed)=>{plans++;return new List<SRCoord>{new SRCoord(10,0)};};
        nav.Walk=(target,interrupt)=>{walks++;pos=target;return true;};
        Check(!nav.Approach(new SRCoord(15,0)) && walks==1 && logs.Any(l=>l.Contains("reached closest nav point")), "no candidate at closest reachable node fails without walking to off-mesh board");
        active=false;walks=0;
        Check(!nav.Approach(new SRCoord(15,0)) && walks==0,"stop request sends no further movement");
        active=true;visible=false;
        nav.Walk=(target,interrupt)=>{active=false;return false;};
        Check(!nav.Approach(new SRCoord(15,0)),"stop during movement exits without recovery");
    }
}
