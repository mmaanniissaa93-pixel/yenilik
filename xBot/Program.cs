using System;
using System.Globalization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using xBot.App;

namespace xBot
{
	static class Program
	{
		private const string SingleInstanceMutexName = @"Local\xBot.WinForms.SingleInstance";
		private const int SW_RESTORE = 9;

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

		private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

		[DllImport("user32.dll")]
		private static extern int GetWindowTextLength(IntPtr hWnd);

		private static void ActivateExistingInstance()
		{
			try
			{
				Process current = Process.GetCurrentProcess();
				foreach (Process process in Process.GetProcessesByName(current.ProcessName))
				{
					if (process.Id == current.Id)
						continue;

					IntPtr windowHandle = process.MainWindowHandle;
					if (windowHandle == IntPtr.Zero)
					{
						uint targetProcessId = (uint)process.Id;
						EnumWindows(delegate(IntPtr candidate, IntPtr state)
						{
							uint ownerProcessId;
							GetWindowThreadProcessId(candidate, out ownerProcessId);
							if (ownerProcessId == targetProcessId && GetWindowTextLength(candidate) > 0)
							{
								windowHandle = candidate;
								return false;
							}
							return true;
						}, IntPtr.Zero);
					}

					if (windowHandle == IntPtr.Zero)
						continue;
					ShowWindow(windowHandle, SW_RESTORE);
					SetForegroundWindow(windowHandle);
					break;
				}
			}
			catch { }
		}

		/// <summary>
		/// Punto de entrada principal para la aplicación.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			bool testMode = false;
			try
			{
				if (args != null)
				{
					foreach (string a in args)
					{
						if (a != null && a.Equals("--phbot-test", StringComparison.OrdinalIgnoreCase))
						{
							testMode = true;
							break;
						}
					}
				}
			}
			catch { }
			string mutexName = testMode ? @"Local\xBot.WinForms.PhBotTest" : SingleInstanceMutexName;
			bool createdNew;
			using (Mutex singleInstanceMutex = new Mutex(true, mutexName, out createdNew))
			{
				if (!createdNew)
				{
					ActivateExistingInstance();
					return;
				}

			// Set  default locale for thread/ui as English
			// made just to avoid tolower/toupper issues with the app in other locales
            CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.DefaultThreadCurrentCulture = CultureInfo.GetCultureInfo("en-US");

			// Beklenmeyen çökmelerde iz bırak (client açarken kapanma gibi durumların teşhisi için)
			AppDomain.CurrentDomain.UnhandledException += (s, e) =>
			{
				try
				{
					Exception ex = e.ExceptionObject as Exception;
					string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
					System.IO.File.AppendAllText(path, DateTime.Now.ToString("dd/MM/yyyy|HH:mm:ss") + "[CRASH]" + (ex != null ? ex.ToString() : "bilinmeyen") + Environment.NewLine);
				}
				catch { }
			};
			Application.ThreadException += (s, e) =>
			{
				try
				{
					string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash.log");
					System.IO.File.AppendAllText(path, DateTime.Now.ToString("dd/MM/yyyy|HH:mm:ss") + "[UI-CRASH]" + e.Exception.ToString() + Environment.NewLine);
				}
				catch { }
			};

            Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			if (!testMode)
			{
				xBot.App.Splash.TransparentSplashForm.ShowSplash();
			}

			Window mainWindow = Window.Get;
			Application.Run(mainWindow);
			GC.KeepAlive(singleInstanceMutex);
			}
		}
	}
}
