using System;
using System.Globalization;
using System.Windows.Forms;
using xBot.App;

namespace xBot
{
	static class Program
	{
		/// <summary>
		/// Punto de entrada principal para la aplicación.
		/// </summary>
		[STAThread]
		static void Main()
		{
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
			Application.Run(Window.Get);
		}
	}
}
