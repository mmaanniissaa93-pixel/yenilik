using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace xBot.App.Theme
{
    public enum LogLevel
    {
        Info,
        Success,
        Warning,
        Error,
        System
    }

    public class PacketTrace
    {
        public DateTime Timestamp { get; set; }
        public string Direction { get; set; }
        public ushort Opcode { get; set; }
        public int Length { get; set; }
        public string Summary { get; set; }
    }

    /// <summary>
    /// Thread-safe color-coded logger for RichTextBox controls with timestamp, severity badges, and persistent file telemetry.
    /// </summary>
    public static class ModernLogger
    {
        public static int MaxLineCount { get; set; } = 500;
        /// <summary>
        /// Kapalıysa TracePacket disk IO yapmaz (proxy hot-path için). Analyzer açıkken UI tarafından açılır.
        /// </summary>
        public static bool EnablePacketTrace { get; set; } = false;
        private static readonly object fileLock = new object();
        private static readonly object traceLock = new object();
        private static readonly Queue<PacketTrace> packetTraceQueue = new Queue<PacketTrace>();

        public static void LogToFile(string line)
        {
            try
            {
                lock (fileLock)
                {
                    File.AppendAllText("session_debug.log", line + Environment.NewLine);
                }
            }
            catch { }
        }

        public static void TracePacket(string direction, ushort opcode, int length, string summary = "")
        {
            if (!EnablePacketTrace)
                return;
            try
            {
                var entry = new PacketTrace
                {
                    Timestamp = DateTime.Now,
                    Direction = direction,
                    Opcode = opcode,
                    Length = length,
                    Summary = summary
                };

                lock (traceLock)
                {
                    if (packetTraceQueue.Count >= 100)
                        packetTraceQueue.Dequeue();
                    packetTraceQueue.Enqueue(entry);
                }

                string detail = string.IsNullOrEmpty(summary) ? "" : $" | {summary}";
                LogToFile($"[{entry.Timestamp:HH:mm:ss.fff}] [{direction,-14}] 0x{opcode:X4} ({length,4} bytes){detail}");
            }
            catch { }
        }

        public static List<PacketTrace> GetRecentPackets()
        {
            lock (traceLock)
            {
                return packetTraceQueue.ToList();
            }
        }

        public static Color ColorTimestamp => DarkTheme.TextFaint;     // #64748B
        public static Color ColorInfo      => DarkTheme.InfoBlue;      // #38BDF8
        public static Color ColorSuccess   => DarkTheme.Success;       // #10B981
        public static Color ColorWarning   => DarkTheme.Warning;       // #F59E0B
        public static Color ColorError     => DarkTheme.Danger;        // #EF4444
        public static Color ColorSystem    => DarkTheme.SystemPurple;  // #A855F7

        public static Color GetLevelColor(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Success: return ColorSuccess;
                case LogLevel.Warning: return ColorWarning;
                case LogLevel.Error:   return ColorError;
                case LogLevel.System:  return ColorSystem;
                case LogLevel.Info:
                default:               return ColorInfo;
            }
        }

        public static string GetLevelTag(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Success: return "[SUCCESS]";
                case LogLevel.Warning: return "[WARN]   ";
                case LogLevel.Error:   return "[ERROR]  ";
                case LogLevel.System:  return "[SYSTEM] ";
                case LogLevel.Info:
                default:               return "[INFO]   ";
            }
        }

        public static LogLevel DetectLevel(string text)
        {
            if (string.IsNullOrEmpty(text)) return LogLevel.Info;

            string lower = text.ToLowerInvariant();
            if (lower.Contains("error") || lower.Contains("hata") || lower.Contains("fail") || lower.Contains("exception"))
                return LogLevel.Error;

            if (lower.Contains("warn") || lower.Contains("uyarı") || lower.Contains("disconnect") || lower.Contains("dc'd"))
                return LogLevel.Warning;

            if (lower.Contains("success") || lower.Contains("başarı") || lower.Contains("connected") || lower.Contains("started") || lower.Contains("kaydedildi"))
                return LogLevel.Success;

            if (lower.Contains("system") || lower.Contains("sistem") || lower.Contains("custom ui"))
                return LogLevel.System;

            return LogLevel.Info;
        }

        public static void Log(RichTextBox rtbx, string message, LogLevel? level = null)
        {
            if (rtbx == null || rtbx.IsDisposed) return;

            if (rtbx.InvokeRequired)
            {
                try
                {
                    rtbx.BeginInvoke(new Action(() => Log(rtbx, message, level)));
                }
                catch { }
                return;
            }

            try
            {
                LogLevel resolvedLevel = level ?? DetectLevel(message);
                string timeStr = $"[{DateTime.Now:HH:mm:ss}] ";
                string tagStr = GetLevelTag(resolvedLevel) + " ";
                string bodyStr = message + Environment.NewLine;

                LogToFile(timeStr + tagStr + message);

                // Enforce buffer limit efficiently without allocating string arrays
                if (rtbx.TextLength > 35000)
                {
                    int cutIndex = rtbx.GetFirstCharIndexFromLine(50);
                    if (cutIndex > 0)
                    {
                        rtbx.Select(0, cutIndex);
                        rtbx.SelectedText = string.Empty;
                    }
                    else
                    {
                        rtbx.Clear();
                    }
                }

                // Fast append with colored badge
                int start = rtbx.TextLength;
                rtbx.AppendText(timeStr + tagStr + bodyStr);

                // Colorize the level badge
                rtbx.Select(start + timeStr.Length, tagStr.Length);
                rtbx.SelectionColor = GetLevelColor(resolvedLevel);

                // Restore caret to end
                rtbx.Select(rtbx.TextLength, 0);
                rtbx.ScrollToCaret();
            }
            catch { }
        }
    }
}
