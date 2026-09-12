using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace xBot.App.Splash
{
	/// <summary>
	/// Standalone borderless layered splash window that displays a desktop-floating transparent PNG frame sequence.
	/// Automatically disposes all bitmap resources and GDI handles upon animation completion.
	/// </summary>
	internal sealed class TransparentSplashForm : Form
	{
		private const string OutputFolderName = "splash_frames";
		private const string SourceFolderName = "xbot_splash_transparent_frames";
		private const int TargetFps = 24; // ~24 FPS (48 frames / 24 = ~2.0 seconds)
		private const int FrameIntervalMs = 41; // 1000 / 24 ≈ 41.67 ms

		private Timer _animationTimer;
		private Bitmap[] _frames;
		private int _currentFrameIndex;
		private bool _isEnding;

		public TransparentSplashForm(Bitmap[] frames)
		{
			_frames = frames ?? throw new ArgumentNullException(nameof(frames));

			FormBorderStyle = FormBorderStyle.None;
			ShowInTaskbar = false;
			StartPosition = FormStartPosition.Manual;
			TopMost = true;
			BackColor = Color.Black;

			_animationTimer = new Timer();
			_animationTimer.Interval = FrameIntervalMs;
			_animationTimer.Tick += AnimationTimer_Tick;
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= LayeredWindowRenderer.WS_EX_LAYERED;
				cp.ExStyle |= LayeredWindowRenderer.WS_EX_NOACTIVATE;
				cp.ExStyle |= LayeredWindowRenderer.WS_EX_TOOLWINDOW;
				return cp;
			}
		}

		protected override bool ShowWithoutActivation
		{
			get { return true; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (_frames == null || _frames.Length == 0)
			{
				EndSplash();
				return;
			}

			// Center window on the active monitor's working area
			Screen activeScreen = Screen.FromPoint(Cursor.Position) ?? Screen.PrimaryScreen;
			Rectangle workArea = activeScreen.WorkingArea;

			int frameWidth = _frames[0].Width;
			int frameHeight = _frames[0].Height;

			Width = frameWidth;
			Height = frameHeight;

			int posX = workArea.Left + Math.Max(0, (workArea.Width - frameWidth) / 2);
			int posY = workArea.Top + Math.Max(0, (workArea.Height - frameHeight) / 2);
			Location = new Point(posX, posY);

			// Render the first frame immediately
			LayeredWindowRenderer.RenderFrame(this, _frames[0]);
			_currentFrameIndex = 1;

			// Start animation timer
			_animationTimer.Start();
		}

		private void AnimationTimer_Tick(object sender, EventArgs e)
		{
			if (_isEnding)
				return;

			if (_frames == null || _currentFrameIndex >= _frames.Length)
			{
				EndSplash();
				return;
			}

			Bitmap currentBmp = _frames[_currentFrameIndex];
			if (currentBmp != null)
			{
				LayeredWindowRenderer.RenderFrame(this, currentBmp);
			}

			_currentFrameIndex++;

			if (_currentFrameIndex >= _frames.Length)
			{
				EndSplash();
			}
		}

		private void EndSplash()
		{
			if (_isEnding)
				return;
			_isEnding = true;

			if (_animationTimer != null)
			{
				_animationTimer.Stop();
				_animationTimer.Tick -= AnimationTimer_Tick;
				_animationTimer.Dispose();
				_animationTimer = null;
			}

			Close();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_animationTimer != null)
				{
					_animationTimer.Stop();
					_animationTimer.Tick -= AnimationTimer_Tick;
					_animationTimer.Dispose();
					_animationTimer = null;
				}

				if (_frames != null)
				{
					for (int i = 0; i < _frames.Length; i++)
					{
						if (_frames[i] != null)
						{
							try { _frames[i].Dispose(); } catch { }
							_frames[i] = null;
						}
					}
					_frames = null;
				}
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Shows the transparent splash window if frames are available.
		/// Safe against exceptions; does not block application startup on failure.
		/// </summary>
		public static void ShowSplash()
		{
			try
			{
				string framesDirectory = ResolveFramesDirectory();
				if (string.IsNullOrEmpty(framesDirectory) || !Directory.Exists(framesDirectory))
					return;

				string[] pngFiles = Directory.GetFiles(framesDirectory, "*.png");
				if (pngFiles == null || pngFiles.Length == 0)
					return;

				// Sort files alphanumerically (e.g. frame_001.png ... frame_048.png)
				Array.Sort(pngFiles, StringComparer.OrdinalIgnoreCase);

				List<Bitmap> loadedFrames = new List<Bitmap>(pngFiles.Length);
				for (int i = 0; i < pngFiles.Length; i++)
				{
					try
					{
						Bitmap frame = LayeredWindowRenderer.LoadPremultipliedFrame(pngFiles[i]);
						if (frame != null)
							loadedFrames.Add(frame);
					}
					catch
					{
						// Skip corrupted single frame safely
					}
				}

				if (loadedFrames.Count == 0)
					return;

				using (TransparentSplashForm splash = new TransparentSplashForm(loadedFrames.ToArray()))
				{
					splash.ShowDialog();
				}
			}
			catch
			{
				// Failsafe: Continue startup smoothly if splash encounters unexpected error
			}
		}

		/// <summary>
		/// Resolves the transparent splash frame directory relative to application base directory or dev environment.
		/// </summary>
		private static string ResolveFramesDirectory()
		{
			string baseDir = AppDomain.CurrentDomain.BaseDirectory;

			// 1. Output directory: splash_frames
			string candidate = Path.Combine(baseDir, OutputFolderName);
			if (Directory.Exists(candidate) && HasPngFiles(candidate))
				return candidate;

			// 2. Output directory: xbot_splash_transparent_frames
			candidate = Path.Combine(baseDir, SourceFolderName);
			if (Directory.Exists(candidate) && HasPngFiles(candidate))
				return candidate;

			// 3. Dev fallback: 2-3 levels up (e.g. bin/Debug or bin/x86/Debug to repo root)
			string devCandidate = Path.GetFullPath(Path.Combine(baseDir, @"..\..\" + SourceFolderName));
			if (Directory.Exists(devCandidate) && HasPngFiles(devCandidate))
				return devCandidate;

			devCandidate = Path.GetFullPath(Path.Combine(baseDir, @"..\..\..\" + SourceFolderName));
			if (Directory.Exists(devCandidate) && HasPngFiles(devCandidate))
				return devCandidate;

			devCandidate = Path.GetFullPath(Path.Combine(baseDir, @"..\" + SourceFolderName));
			if (Directory.Exists(devCandidate) && HasPngFiles(devCandidate))
				return devCandidate;

			return null;
		}

		private static bool HasPngFiles(string dir)
		{
			try
			{
				string[] files = Directory.GetFiles(dir, "*.png");
				return files != null && files.Length > 0;
			}
			catch
			{
				return false;
			}
		}
	}
}
