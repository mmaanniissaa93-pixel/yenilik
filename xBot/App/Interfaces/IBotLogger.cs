using System;
using SecurityAPI;
using xBot.App.Theme;

namespace xBot.App
{
	/// <summary>
	/// Abstraction for bot and network layer logging, decoupling core logic from Window WinForms controls.
	/// </summary>
	public interface IBotLogger
	{
		void Log(string text);
		void Log(string text, LogLevel level);
		void LogProcess(string text = "Ready", Window.ProcessState state = Window.ProcessState.Default);
		void LogPacket(string text);
		void LogError(string message, Exception ex, Packet packet = null);
		void LogSwallowed(Exception ex, string context);
	}

	/// <summary>
	/// Fallback logger used when running headless (CLI, automated tests, or prior to UI initialization).
	/// </summary>
	public class FallbackLogger : IBotLogger
	{
		public void Log(string text)
		{
			Log(text, LogLevel.Info);
		}

		public void Log(string text, LogLevel level)
		{
			ModernLogger.LogToFile($"[{DateTime.Now:HH:mm:ss}] [{level}] {text}");
		}

		public void LogProcess(string text = "Ready", Window.ProcessState state = Window.ProcessState.Default)
		{
			ModernLogger.LogToFile($"[{DateTime.Now:HH:mm:ss}] [PROCESS][{state}] {text}");
		}

		public void LogPacket(string text)
		{
			ModernLogger.LogToFile($"[{DateTime.Now:HH:mm:ss}] [PACKET] {text}");
		}

		public void LogError(string message, Exception ex, Packet packet = null)
		{
			string msg = $"[{DateTime.Now:dd-MM-yyyy HH:mm:ss}][{message}] {ex?.Message}\r\n{ex?.StackTrace}";
			if (packet != null)
			{
				msg += $"\r\n[Packet Info] Opcode: 0x{packet.Opcode:X4} Length: {packet.GetBytes().Length} Data: {packet}";
			}
			ModernLogger.LogToFile(msg);
		}

		public void LogSwallowed(Exception ex, string context)
		{
			ModernLogger.LogSwallowed(ex, context);
		}
	}

	/// <summary>
	/// Global logging dispatcher providing decoupled logging to whichever implementation is active.
	/// </summary>
	public static class BotLog
	{
		private static IBotLogger _currentLogger;

		public static IBotLogger Instance
		{
			get => _currentLogger ?? (_currentLogger = DefaultLogger);
			set => _currentLogger = value ?? DefaultLogger;
		}

		public static IBotLogger DefaultLogger { get; set; } = new FallbackLogger();

		public static void Log(string text) => Instance.Log(text);
		public static void Log(string text, LogLevel level) => Instance.Log(text, level);
		public static void LogProcess(string text = "Ready", Window.ProcessState state = Window.ProcessState.Default) => Instance.LogProcess(text, state);
		public static void LogPacket(string text) => Instance.LogPacket(text);
		public static void LogError(string message, Exception ex, Packet packet = null) => Instance.LogError(message, ex, packet);
		public static void LogSwallowed(Exception ex, string context) => Instance.LogSwallowed(ex, context);
	}
}
