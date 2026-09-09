using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace xGraphics
{
	public class xRichTextBox : RichTextBox
	{
		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, ref Point lParam);
		[DllImport("user32.dll")]
		private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, IntPtr lParam);
		[DllImport("user32")]
		private static extern int GetCaretPos(out Point p);
		private const int WM_USER = 1024;
		private const int WM_SETREDRAW = 11;
		private const int EM_GETEVENTMASK = 1083;
		private const int EM_SETEVENTMASK = 1093;
		private const int EM_GETSCROLLPOS = 1245;
		private const int EM_SETSCROLLPOS = 1246;
		private const int WM_VSCROLL = 277;
		private const int SB_PAGEBOTTOM = 7;

		private Point _ScrollPoint;
		private bool _Painting = true;
		private IntPtr _EventMask;
		private int _SuspendIndex = 0;
		private int _SuspendLength = 0;
		private bool _Trimming;

		public bool AutoScroll
		{
			get{
				return _AutoScroll;
			}
			set{
				if (_AutoScroll == value) return;
				if (value)
					this.TextChanged += xRichTextBox_TextChanged_AutoScroll;
				else
					this.TextChanged -= xRichTextBox_TextChanged_AutoScroll;
				_AutoScroll = value;
			}
		}
		private bool _AutoScroll;
		public int MaxLines {	get; set;	}
		public xRichTextBox()
		{
			MaxLines = int.MaxValue;
		}
		public void SuspendPainting()
		{
			if (_Painting)
			{
				_SuspendIndex = this.SelectionStart;
				_SuspendLength = SelectionLength;
				SendMessage(this.Handle, 1245, 0, ref _ScrollPoint);
				SendMessage(this.Handle, 11, 0, IntPtr.Zero);
				_EventMask = SendMessage(this.Handle, 1083, 0, IntPtr.Zero);
				_Painting = false;
			}
		}
		public void ResumePainting()
		{
			if (!_Painting)
			{
				Select(_SuspendIndex, _SuspendLength);
				SendMessage(this.Handle, 1246, 0, ref _ScrollPoint);
				SendMessage(this.Handle, 1093, 0, _EventMask);
				SendMessage(this.Handle, 11, 1, IntPtr.Zero);
				_Painting = true;
				Invalidate();
			}
		}

		public new void AppendText(string text)
		{
			if (AutoScroll)
			{
				base.AppendText(text);
			}
			else
			{
				SuspendPainting();
				base.AppendText(text);
				ResumePainting();
			}
		}

		protected override void OnTextChanged(EventArgs e)
		{
			if (_Trimming) return;
			// Also enforce the limit when callers use a RichTextBox reference or
			// append a multi-line packet. Lines would copy/split the entire buffer.
			if (MaxLines > 0 && MaxLines < int.MaxValue && TextLength > 0)
			{
				int excess = GetLineFromCharIndex(TextLength) + 1 - MaxLines;
				int cutIndex = excess > 0 ? GetFirstCharIndexFromLine(excess) : 0;
				if (cutIndex > 0)
				{
					bool wasPainting = _Painting;
					bool wasReadOnly = ReadOnly;
					_Trimming = true;
					try
					{
						SuspendPainting();
						ReadOnly = false;
						Select(0, cutIndex);
						SelectedText = string.Empty;
					}
					finally
					{
						ReadOnly = wasReadOnly;
						if (wasPainting) ResumePainting();
						_Trimming = false;
					}
				}
			}
			base.OnTextChanged(e);
		}

		private void xRichTextBox_TextChanged_AutoScroll(object sender, EventArgs e)
		{
			//SendMessage(base.Handle, WM_VSCROLL, SB_PAGEBOTTOM, IntPtr.Zero);
			base.SelectionStart = TextLength;
			ScrollToCaret();
		}
	}
}
