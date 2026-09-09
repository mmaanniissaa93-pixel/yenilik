using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using xBot.Game.Navigation;
using xBot.Game.Objects.Common;
using xGraphics;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        NavigationRecords();
        MapLifetime();
        ImageOwnership();
        TileLoading();
        LogAllocation();
        if (args.Length > 0) RealNavigationFiles(args[0]);
        Console.WriteLine("PASS: memory and navigation scenarios");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void NavigationRecords()
    {
        string path = Path.GetTempFileName();
        try
        {
            using (var file = File.Create(path))
            using (var zip = new ZLibStream(file, CompressionLevel.Fastest))
            using (var writer = new BinaryWriter(zip))
            {
                writer.Write(4u);
                for (int i = 0; i < 4; i++)
                {
                    writer.Write((float)(i * 10 - 5));
                    writer.Write((float)(i * -20 + 7));
                    writer.Write((ushort)i);
                    writer.Write((ushort)(i + 1));
                }
                // Duplicates, self edges and invalid targets must not change connectivity.
                writer.Write((byte)5);
                foreach (int edge in new[] { 1, 1, 0, -1, 99 }) writer.Write(edge);
                writer.Write((byte)2); writer.Write(0); writer.Write(2);
                writer.Write((byte)0);
                writer.Write((byte)0);
            }
            NavRegion region = NavDataReader.Read(path, 7);
            Check(region.Neighbors[0].SequenceEqual(new[] { 1 }), "duplicate edges");
            Check(region.Neighbors[1].SequenceEqual(new[] { 0, 2 }), "edge order");
            Check(region.Neighbors[2].SequenceEqual(new[] { 1 }), "reverse edge");
            Check(region.Neighbors[3].Length == 0, "isolated point");
            Check(region.Points[2].Z == 2 && region.Points[2].EdgeFlag == 3, "point fields");
            CheckBounds(path, region);

            using (var file = File.Create(path))
            using (var zip = new ZLibStream(file, CompressionLevel.Fastest))
            using (var writer = new BinaryWriter(zip))
            {
                writer.Write(2u); writer.Write(1f); // Truncated point records.
            }
            Check(NavDataReader.Read(path, 7) == null, "truncated points accepted");
            Check(!NavDataReader.TryReadBounds(path, out _, out _, out _, out _), "truncated bounds accepted");
            File.WriteAllBytes(path, new byte[3]);
            Check(!NavDataReader.TryReadBounds(path, out _, out _, out _, out _), "short zlib accepted");
        }
        finally { File.Delete(path); }
    }

    private static void CheckBounds(string path, NavRegion region)
    {
        Check(NavDataReader.TryReadBounds(path, out float minX, out float maxX, out float minY, out float maxY), "bounds missing");
        Check(minX == region.MinX && maxX == region.MaxX && minY == region.MinY && maxY == region.MaxY, "bounds changed");
    }

    private static void MapLifetime()
    {
        using (var map = new xMap())
        {
            for (int i = 1; i <= 80; i++)
            {
                var previous = map.Controls.OfType<xMapTile>().ToArray();
                map.SetView(new SRCoord(i * 192, i * 192));
                Check(map.Controls.OfType<xMapTile>().Count() == map.TileCount * map.TileCount, "map cache grew");
                foreach (var tile in previous)
                    Check(map.Controls.Contains(tile) || tile.IsDisposed, "evicted tile not disposed");
                var visible = map.Controls.OfType<xMapTile>().ToArray();
                map.ClearCache();
                Check(visible.All(tile => !tile.IsDisposed), "visible tiles evicted");
            }
            var oldTiles = map.Controls.OfType<xMapTile>().ToArray();
            map.Zoom = 2;
            Check(oldTiles.All(tile => tile.IsDisposed), "zoom leaked tiles");
            Check(map.Controls.OfType<xMapTile>().Count() == 49, "zoom tile count");
            map.SetView(new SRCoord(25000.0, 25000.0, (ushort)32769, 0));
            Check(map.Controls.OfType<xMapTile>().All(tile => tile.Name.Contains("floor01")), "dungeon layer");
            map.SetView(new SRCoord(25000.0, 25000.0, (ushort)32769, 150));
            Check(map.Controls.OfType<xMapTile>().All(tile => tile.Name.Contains("floor02")), "dungeon floor change");
            map.SetView(new SRCoord(0, 0));
            Check(map.Controls.OfType<xMapTile>().All(tile => !tile.Name.Contains("/d/")), "dungeon exit layer");
            var first = new xMapControl();
            var second = new xMapControl();
            map.AddMarker(1, first);
            map.AddMarker(1, second);
            Check(first.IsDisposed, "duplicate marker leaked");
            map.RemoveMarker(1);
            Check(second.IsDisposed, "removed marker leaked");
            var third = new xMapControl();
            map.AddMarker(2, third);
            map.ClearMarkers();
            Check(third.IsDisposed, "cleared marker leaked");
        }
    }

    private static bool Disposed(Image image)
    {
        try { return image.Width == 0; }
        catch (ArgumentException) { return true; }
    }

    private static void ImageOwnership()
    {
        using (var shared = new Bitmap(8, 8))
        {
            var control = new xMapControl { Image = shared };
            var first = new Bitmap(8, 8);
            var second = new Bitmap(8, 8);
            control.SetOwnedImage(first);
            control.SetOwnedImage(second);
            Check(Disposed(first), "old rotation leaked");
            control.Dispose();
            Check(Disposed(second), "last rotation leaked");
            Check(shared.Width == 8, "shared resource disposed");
        }
    }

    private static void TileLoading()
    {
        string path = Path.GetTempFileName();
        try
        {
            using (var bitmap = new Bitmap(16, 16)) bitmap.Save(path, System.Drawing.Imaging.ImageFormat.Png);
            // Establish the same UI synchronization context as the application.
            using (var host = new Control())
            {
                host.CreateControl();
                var removed = Enumerable.Range(0, 30).Select(i => new xMapTile(i, 0)).ToArray();
                foreach (var tile in removed) { tile.LoadAsyncTile(path, new Size(64, 64)); tile.Dispose(); }
                using (var current = new xMapTile(0, 0))
                {
                    current.LoadAsyncTile(path, new Size(32, 32));
                    current.LoadAsyncTile(path, new Size(48, 48));
                    var timer = Stopwatch.StartNew();
                    while ((current.Image == null || current.Image.Width != 48) && timer.ElapsedMilliseconds < 10000)
                    {
                        Application.DoEvents();
                        Thread.Sleep(1);
                    }
                    Check(current.Image != null && current.Image.Width == 48, "latest tile load lost");
                    Check(removed.All(tile => tile.Image == null), "async load revived disposed tile");
                }
            }
        }
        finally { File.Delete(path); }
    }

    private static void LogAllocation()
    {
        using (var log = new xRichTextBox { MaxLines = 10, AutoScroll = true, WordWrap = false })
        {
            for (int i = 0; i < 100; i++) log.AppendText(i + "\n");
            Check(log.Lines.Length <= 10 && log.Text.Contains("99"), "bounded chat history");
            RichTextBox chat = log;
            for (int i = 0; i < 100; i++) chat.AppendText("packet\nline 2\nline 3\n");
            Check(log.Lines.Length <= 10 && log.Text.EndsWith("line 3\n"), "base/multiline append bypassed limit");
            log.ReadOnly = true;
            chat.AppendText(string.Concat(Enumerable.Repeat("readonly\n", 100)));
            Check(log.ReadOnly && log.Lines.Length <= 10, "readonly log limit");
        }
    }

    private static void RealNavigationFiles(string folder)
    {
        string[] files = Directory.GetFiles(folder, "*.dat");
        long allocated = 0, boundsAllocated = 0;
#if BASELINE
        long beforeAllocated = 0, beforeBoundsAllocated = 0;
#endif
        foreach (string path in files)
        {
            long start = GC.GetAllocatedBytesForCurrentThread();
            NavRegion region = NavDataReader.Read(path, 1);
            allocated += GC.GetAllocatedBytesForCurrentThread() - start;
            Check(region != null, "invalid real region: " + path);
            start = GC.GetAllocatedBytesForCurrentThread();
            CheckBounds(path, region);
            boundsAllocated += GC.GetAllocatedBytesForCurrentThread() - start;
#if BASELINE
            start = GC.GetAllocatedBytesForCurrentThread();
            NavRegion original = BaselineNavDataReader.Read(path, 1);
            beforeAllocated += GC.GetAllocatedBytesForCurrentThread() - start;
            Check(original.Points.SequenceEqual(region.Points), "point data changed: " + path);
            for (int i = 0; i < region.Points.Length; i++)
                Check(original.Neighbors[i].SequenceEqual(region.Neighbors[i]), "topology/order changed: " + path);
            start = GC.GetAllocatedBytesForCurrentThread();
            Check(BaselineNavDataReader.TryReadBounds(path, out _, out _, out _, out _), "baseline bounds");
            beforeBoundsAllocated += GC.GetAllocatedBytesForCurrentThread() - start;
#endif
        }
        Console.WriteLine($"Navigation: {files.Length} files; total managed allocations: load {allocated / 1048576.0:F2} MiB; bounds {boundsAllocated / 1048576.0:F2} MiB");
#if BASELINE
        Console.WriteLine($"Baseline allocations: load {beforeAllocated / 1048576.0:F2} MiB; bounds {beforeBoundsAllocated / 1048576.0:F2} MiB");
        Check(allocated < beforeAllocated && boundsAllocated < beforeBoundsAllocated, "allocation regression");
#endif
    }
}
