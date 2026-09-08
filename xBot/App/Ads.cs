using System;
using System.Windows.Forms;
using System.Net;

namespace xBot.App
{
	public partial class Ads : Form
	{
		/// <summary>
		/// Excel data positions.
		/// </summary>
		public enum EXCEL : byte
		{
			DATE,
			TITLE,
			URL_WEBSITE,
			URL_BANNER,
			TIME_MIN,
			TIME_MAX,
			URL_MINIBANNER
		}
		private string[] ExcelData;
		private Timer AutoClosingTimer;
		private int AutoClosingTimeMax;
		private int AutoClosingTimeMin;
		private const int MaxAdvertisementBytes = 256 * 1024;

		public Ads(Form w)
		{
			InitializeComponent();
			InitializeFonts(this);
			base.Icon = w.Icon;
			Text = base.ProductName + " - Advertising";
			base.Tag = "Advertising ";
			lblHeader.Text = base.Tag + "...";
		}
		private void InitializeFonts(Control c)
		{
			// Using fontName as TAG to be selected from WinForms
			c.Font = Fonts.GetFont(c.Font, (string)c.Tag);
			c.Tag = null;
			for (int j = 0; j < c.Controls.Count; j++)
        InitializeFonts(c.Controls[j]);
		}
		/// <summary>
		/// Try to load the advertisement and returns the result. Returns null if cannot be loaded.
		/// </summary>
		public bool TryLoad()
		{
			ExcelData = null;
			try
			{
				string txtFile;
				using (WebClient client = new WebClient())
				{
					txtFile = client.DownloadString("https://bit.ly/xBot-ads-check");
				}
				if (txtFile.Length > MaxAdvertisementBytes)
					throw new InvalidOperationException("Advertisement response is too large.");
				string[] rows = txtFile.Split(new string[]{"\r\n"}, StringSplitOptions.None);
				if (rows.Length < 2)
					throw new FormatException("Advertisement response has no data rows.");
				string[] colums = rows[0].Split(new string[]{ "," }, StringSplitOptions.None);
				if (colums.Length <= (int)EXCEL.URL_MINIBANNER)
					throw new FormatException("Advertisement header is incomplete.");
				string[] str_today2 = colums[0].Split('(', ')')[1].Split('/', ' ', ':');
				DateTime today = new DateTime(int.Parse(str_today2[2]), int.Parse(str_today2[1]), int.Parse(str_today2[0]), int.Parse(str_today2[3]), int.Parse(str_today2[4]), 0);

				for (int i = 1; i < rows.Length; i++)
				{
					colums = rows[i].Split(new string[] { "," }, StringSplitOptions.None);
					if (colums.Length <= (int)EXCEL.URL_MINIBANNER)
						continue;
					str_today2 = colums[0].Split('/', ' ', ':');
					if (today <= new DateTime(int.Parse(str_today2[2]), int.Parse(str_today2[1]), int.Parse(str_today2[0]), int.Parse(str_today2[3]), int.Parse(str_today2[4]), 0)){
						ExcelData = colums;
						break;
					}
				}
				if (ExcelData != null)
					ValidateMediaData();
			}
			catch
			{
				ExcelData = null;
			}
			return ExcelData != null;
		}
		private void LoadMediaData()
		{
			TryGetHttpsUri(GetData(EXCEL.URL_BANNER), out Uri bannerUri);
			TryGetHttpsUri(GetData(EXCEL.URL_WEBSITE), out Uri websiteUri);
			lblAdName.Text = GetData(EXCEL.TITLE);
			ToolTips.SetToolTip(lblAdName,GetData(EXCEL.TITLE));
			pbxBanner.Load(bannerUri.ToString());
			pbxBanner.Tag = websiteUri;
			ToolTipLink.SetToolTip(pbxBanner, websiteUri.ToString());
		}
		private void ValidateMediaData()
		{
			if (!TryGetHttpsUri(GetData(EXCEL.URL_BANNER), out Uri bannerUri)
				|| !TryGetHttpsUri(GetData(EXCEL.URL_WEBSITE), out Uri websiteUri)
				|| !TryGetHttpsUri(GetData(EXCEL.URL_MINIBANNER), out Uri miniBannerUri))
				throw new InvalidOperationException("Advertisement URLs must use HTTPS.");
			ExcelData[(int)EXCEL.URL_BANNER] = bannerUri.ToString();
			ExcelData[(int)EXCEL.URL_WEBSITE] = websiteUri.ToString();
			ExcelData[(int)EXCEL.URL_MINIBANNER] = miniBannerUri.ToString();
		}
		private static bool TryGetHttpsUri(string value, out Uri uri)
		{
			return Uri.TryCreate(value, UriKind.Absolute, out uri)
				&& string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
		}
		public bool isLoaded()
		{
			return ExcelData != null;
		}
		public string GetData(EXCEL type)
		{
			// Fix the csv behaviour
			string data = ExcelData[(uint)type];
			if (data.StartsWith("\"") && data.EndsWith("\""))
				data = data.Substring(1, data.Length - 2);
			return data;
		}
		private void Ads_Load(object sender, EventArgs e)
		{
			if (isLoaded())
			{
				LoadMediaData();
				AutoClosingTimeMin = int.Parse(GetData(EXCEL.TIME_MIN));
				AutoClosingTimeMax = int.Parse(GetData(EXCEL.TIME_MAX));
				if (AutoClosingTimeMax < AutoClosingTimeMin || AutoClosingTimer != null)
				{
					AutoClosingTimeMin = AutoClosingTimeMax;
				}
				lblHeader.Text = base.Tag.ToString() + AutoClosingTimeMin + " ...";
				AutoClosingTimer = new Timer();
				AutoClosingTimer.Interval = 1000;
				AutoClosingTimer.Tag = 0;
				AutoClosingTimer.Tick += AutoClosingTimer_Tick;
				AutoClosingTimer.Start();
			}
			Focus();
			BringToFront();
		}
		private void Ads_Closing(object sender, FormClosingEventArgs e)
		{
			if (AutoClosingTimer != null)
			{
				AutoClosingTimer.Stop();
			}
		}
		private void AutoClosingTimer_Tick(object sender, EventArgs e)
		{
			int time = (int)AutoClosingTimer.Tag + 1;
			AutoClosingTimer.Tag = time;
			if (time < AutoClosingTimeMin)
			{
				lblHeader.Text = base.Tag.ToString() + (AutoClosingTimeMin - time) + " ...";
			}
			else if (time == AutoClosingTimeMin)
			{
				lblHeader.Text = base.Tag.ToString() + "...";
				btnWinExit.Enabled = true;
			}
			// Auto close it
			if (time >= AutoClosingTimeMax)
			{
				AutoClosingTimer.Stop();
				Close();
			}
		}
		private void Control_Click(object sender, EventArgs e)
		{
			Control c = (Control)sender;
			switch (c.Name)
			{
				case "btnWinExit":
					this.Close();
					break;
				case "pbxBanner":
					if (pbxBanner.Tag is Uri uri && string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
						System.Diagnostics.Process.Start(uri.ToString());
					break;
			}
		}
	}
}
