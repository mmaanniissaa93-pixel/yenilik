using System;
using System.Drawing;
using System.Windows.Forms;
using xBot.App.Theme;
using xBot.Game;

namespace xBot.App
{
    public partial class Window
    {
        private ModernSidebar modernSidebar;
        private ModernStatusBar modernHeaderHP;
        private ModernStatusBar modernHeaderMP;
        private ModernStatusBar modernHeaderLevel;

        private bool _isModernThemeApplied = false;
        private static readonly System.Collections.Generic.Dictionary<GroupBox, string> _originalGroupBoxTitles = new System.Collections.Generic.Dictionary<GroupBox, string>();

        /// <summary>
        /// Applies the modern dark mode theme, expanded 980x640 window dimensions,
        /// categorized sidebar, modern status bars, and color-coded logging console.
        /// </summary>
        public void ApplyModernTheme()
        {
            if (_isModernThemeApplied) return;
            _isModernThemeApplied = true;

            try
            {
                // 1. Expand Window Dimensions & Form Styling
                this.SuspendLayout();
                this.FormBorderStyle = FormBorderStyle.None;
                this.ClientSize = new Size(DarkTheme.DefaultWindowWidth, DarkTheme.DefaultWindowHeight);
                this.BackColor = DarkTheme.BgDark;
                this.ForeColor = DarkTheme.TextSecondary;
                this.StartPosition = FormStartPosition.CenterScreen;

                if (pnlWindow != null)
                {
                    pnlWindow.Location = new Point(0, 0);
                    pnlWindow.Size = new Size(DarkTheme.DefaultWindowWidth, DarkTheme.DefaultWindowHeight);
                    pnlWindow.BackColor = DarkTheme.BgDark;
                }

                // 2. Modernize Header (Title bar)
                ApplyModernHeader();

                // 3. Modernize Sidebar (Categorized with Active Bar)
                ApplyModernSidebar();

                // 4. Modernize Content Panels & Dimensions
                ApplyModernContentPanels();

                // 5. Modernize Bottom Console & Action Buttons
                ApplyModernConsoleAndActions();

                // 6. Automatically style all GroupBoxes into Cards, Buttons, and Inputs
                SkinControlHierarchy(pnlWindow);

                this.ResumeLayout(true);
            }
            catch (Exception ex)
            {
                Log("[Modern Theme Error] " + ex.Message, LogLevel.Error);
            }
        }

        private void SkinControlHierarchy(Control parent)
        {
            if (parent == null) return;

            foreach (Control c in parent.Controls)
            {
                if (c is GroupBox gbx)
                {
                    string title;
                    if (!_originalGroupBoxTitles.TryGetValue(gbx, out title) || string.IsNullOrEmpty(title))
                    {
                        title = gbx.Text;
                        if (gbx == Login_gbxLogin)
                        {
                            title = "Giriş Bilgileri (Login)";
                        }
                        if (!string.IsNullOrEmpty(title))
                        {
                            _originalGroupBoxTitles[gbx] = title;
                        }
                    }
                    gbx.Text = string.Empty; // Prevent default WinForms GroupBox text from ever rendering
                    gbx.ForeColor = DarkTheme.TextPrimary;
                    gbx.BackColor = Color.Transparent;
                    gbx.Font = DarkTheme.FontHeader;

                    gbx.Paint += (s, pe) =>
                    {
                        Graphics g = pe.Graphics;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

                        // Clear entire client area to erase any native WinForms gray border
                        using (SolidBrush parentBrush = new SolidBrush(DarkTheme.BgDark))
                        {
                            g.FillRectangle(parentBrush, gbx.ClientRectangle);
                        }

                        Rectangle rect = new Rectangle(0, 0, gbx.Width - 1, gbx.Height - 1);
                        if (rect.Width <= 0 || rect.Height <= 0) return;

                        // Draw clean rounded card background
                        using (System.Drawing.Drawing2D.GraphicsPath path = DarkTheme.CreateRoundedRectangle(rect, 8))
                        {
                            using (SolidBrush bgBrush = new SolidBrush(DarkTheme.BgCard))
                            {
                                g.FillPath(bgBrush, path);
                            }

                            string displayTitle = title;
                            if (string.IsNullOrEmpty(displayTitle))
                            {
                                _originalGroupBoxTitles.TryGetValue(gbx, out displayTitle);
                            }

                            int minChildTop = 999;
                            foreach (Control child in gbx.Controls)
                            {
                                if (child.Visible && child.Top < minChildTop)
                                    minChildTop = child.Top;
                            }

                            int headerY = (minChildTop >= 24) ? 6 : 2;
                            int headerH = (minChildTop >= 24) ? 18 : 14;
                            int pillY = (minChildTop >= 24) ? 8 : 4;
                            int pillH = (minChildTop >= 24) ? 14 : 10;

                            // Accent indicator pill and title text cleanly inside the card header
                            if (!string.IsNullOrEmpty(displayTitle))
                            {
                                Rectangle pillRect = new Rectangle(12, pillY, 3, pillH);
                                using (System.Drawing.Drawing2D.GraphicsPath pillPath = DarkTheme.CreateRoundedRectangle(pillRect, 1))
                                using (SolidBrush pillBrush = new SolidBrush(DarkTheme.Accent))
                                {
                                    g.FillPath(pillBrush, pillPath);
                                }

                                RectangleF textRect = new RectangleF(21, headerY, gbx.Width - 26, headerH);
                                using (SolidBrush textBrush = new SolidBrush(DarkTheme.TextPrimary))
                                {
                                    StringFormat sf = new StringFormat
                                    {
                                        Alignment = StringAlignment.Near,
                                        LineAlignment = StringAlignment.Center,
                                        Trimming = StringTrimming.EllipsisCharacter
                                    };
                                    g.DrawString(displayTitle, DarkTheme.FontHeader, textBrush, textRect, sf);
                                }
                            }

                            // Subtle card border
                            using (Pen borderPen = new Pen(DarkTheme.BorderSubtle, 1f))
                            {
                                g.DrawPath(borderPen, path);
                            }
                        }

                        // Draw modern input containers for single-line TextBoxes and ComboBoxes
                        foreach (Control child in gbx.Controls)
                        {
                            if (child is TextBox tbx && !tbx.Multiline && child.Visible)
                            {
                                Rectangle boxRect = new Rectangle(child.Left - 5, child.Top - 3, child.Width + 10, child.Height + 6);
                                bool isFoc = tbx.Focused;
                                Color fillCol = isFoc ? DarkTheme.BgInputFocus : DarkTheme.BgInput;
                                Color borderCol = isFoc ? DarkTheme.BorderFocus : DarkTheme.BorderSubtle;
                                float borderW = isFoc ? 1.5f : 1f;

                                using (System.Drawing.Drawing2D.GraphicsPath boxPath = DarkTheme.CreateRoundedRectangle(boxRect, 4))
                                {
                                    using (SolidBrush boxBrush = new SolidBrush(fillCol))
                                    {
                                        g.FillPath(boxBrush, boxPath);
                                    }
                                    using (Pen boxPen = new Pen(borderCol, borderW))
                                    {
                                        g.DrawPath(boxPen, boxPath);
                                    }
                                }
                            }
                            else if (child is ComboBox cmb && child.Visible)
                            {
                                if (cmb.Focused)
                                {
                                    Rectangle cmbBorder = new Rectangle(cmb.Left - 1, cmb.Top - 1, cmb.Width + 2, cmb.Height + 2);
                                    using (Pen focusPen = new Pen(DarkTheme.BorderFocus, 1.5f))
                                    {
                                        g.DrawRectangle(focusPen, cmbBorder);
                                    }
                                }
                            }
                        }
                    };
                }
                else if (c is Button btn)
                {
                    if (btn != btnWinMinimize && btn != btnWinExit && btn != btnWinRestore && btn != btnBotStart && btn != btnClientOptions)
                    {
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 1;
                        btn.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
                        btn.Font = DarkTheme.FontBodyBold;

                        string upperText = (btn.Text ?? string.Empty).ToUpperInvariant();
                        if (upperText.Contains("START") || upperText.Contains("BAŞLAT"))
                        {
                            btn.BackColor = DarkTheme.Success;
                            btn.ForeColor = Color.White;
                            btn.FlatAppearance.BorderColor = DarkTheme.Success;
                        }
                        else if (upperText.Contains("LAUNCHER") || upperText.Contains("GİRİŞ"))
                        {
                            btn.BackColor = DarkTheme.Accent;
                            btn.ForeColor = Color.White;
                            btn.FlatAppearance.BorderColor = DarkTheme.Accent;
                        }
                        else
                        {
                            btn.BackColor = DarkTheme.BgInput;
                            btn.ForeColor = DarkTheme.TextPrimary;
                        }
                    }
                }
                else if (c is TextBox tbx)
                {
                    if (!tbx.Multiline)
                    {
                        tbx.BorderStyle = BorderStyle.None;
                    }
                    tbx.BackColor = DarkTheme.BgInput;
                    tbx.ForeColor = DarkTheme.TextPrimary;
                    tbx.Font = DarkTheme.FontBody;
                    tbx.TextAlign = HorizontalAlignment.Left;
                    tbx.GotFocus += (s, e) =>
                    {
                        tbx.BackColor = DarkTheme.BgInputFocus;
                        if (tbx.Parent != null) tbx.Parent.Invalidate();
                    };
                    tbx.LostFocus += (s, e) =>
                    {
                        tbx.BackColor = DarkTheme.BgInput;
                        if (tbx.Parent != null) tbx.Parent.Invalidate();
                    };
                }
                else if (c is NumericUpDown nud)
                {
                    nud.BackColor = DarkTheme.BgInput;
                    nud.ForeColor = DarkTheme.TextPrimary;
                    nud.BorderStyle = BorderStyle.FixedSingle;
                    nud.Font = DarkTheme.FontBody;
                    nud.GotFocus += (s, e) =>
                    {
                        nud.BackColor = DarkTheme.BgInputFocus;
                        if (nud.Parent != null) nud.Parent.Invalidate();
                    };
                    nud.LostFocus += (s, e) =>
                    {
                        nud.BackColor = DarkTheme.BgInput;
                        if (nud.Parent != null) nud.Parent.Invalidate();
                    };
                }
                else if (c is ComboBox cmb)
                {
                    cmb.BackColor = DarkTheme.BgInput;
                    cmb.ForeColor = DarkTheme.TextPrimary;
                    cmb.FlatStyle = FlatStyle.Flat;
                    cmb.Font = DarkTheme.FontBody;
                    cmb.GotFocus += (s, e) =>
                    {
                        cmb.BackColor = DarkTheme.BgInputFocus;
                        if (cmb.Parent != null) cmb.Parent.Invalidate();
                    };
                    cmb.LostFocus += (s, e) =>
                    {
                        cmb.BackColor = DarkTheme.BgInput;
                        if (cmb.Parent != null) cmb.Parent.Invalidate();
                    };
                }
                else if (c is CheckBox cbx)
                {
                    cbx.ForeColor = DarkTheme.TextPrimary;
                    cbx.Font = DarkTheme.FontBody;
                }
                else if (c is RadioButton rbn)
                {
                    rbn.ForeColor = DarkTheme.TextPrimary;
                    rbn.Font = DarkTheme.FontBody;
                }
                else if (c is Label lbl)
                {
                    if (lbl != lblHeaderText01 && lbl != lblHeaderText02)
                    {
                        lbl.ForeColor = DarkTheme.TextMuted;
                        lbl.Font = DarkTheme.FontBody;
                    }
                }
                else if (c is ListView lv)
                {
                    lv.BackColor = DarkTheme.BgCard;
                    lv.ForeColor = DarkTheme.TextPrimary;
                    lv.BorderStyle = BorderStyle.FixedSingle;
                    lv.Font = DarkTheme.FontBody;
                    if (lv.Columns.Count == 1 && lv.Width > 20)
                    {
                        lv.Columns[0].Width = Math.Max(lv.Columns[0].Width, lv.Width - 6);
                    }
                    lv.OwnerDraw = true;
                    lv.DrawColumnHeader += (s, e) =>
                    {
                        using (SolidBrush bgBrush = new SolidBrush(DarkTheme.BgCardHeader))
                        using (SolidBrush textBrush = new SolidBrush(DarkTheme.TextSecondary))
                        using (Pen borderPen = new Pen(DarkTheme.BorderSubtle, 1f))
                        {
                            e.Graphics.FillRectangle(bgBrush, e.Bounds);
                            e.Graphics.DrawRectangle(borderPen, e.Bounds.X, e.Bounds.Y, e.Bounds.Width - 1, e.Bounds.Height - 1);
                            StringFormat sf = new StringFormat
                            {
                                Alignment = StringAlignment.Near,
                                LineAlignment = StringAlignment.Center
                            };
                            Rectangle textRect = new Rectangle(e.Bounds.X + 6, e.Bounds.Y, e.Bounds.Width - 12, e.Bounds.Height);
                            e.Graphics.DrawString(e.Header.Text, DarkTheme.FontBodyBold, textBrush, textRect, sf);
                        }
                    };
                    lv.DrawItem += (s, e) => { e.DrawDefault = true; };
                    lv.DrawSubItem += (s, e) => { e.DrawDefault = true; };
                }

                if (c.HasChildren && !(c is ModernSidebar))
                {
                    SkinControlHierarchy(c);
                }
            }
        }

        private void ApplyModernHeader()
        {
            if (pnlHeader == null) return;

            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Size = new Size(DarkTheme.DefaultWindowWidth, DarkTheme.HeaderHeight);
            pnlHeader.BackColor = DarkTheme.BgHeader;

            // Header Title Labels
            if (lblHeaderText01 != null)
            {
                lblHeaderText01.Font = DarkTheme.FontTitle;
                lblHeaderText01.ForeColor = DarkTheme.TextPrimary;
                lblHeaderText01.Location = new Point(48, 12);
            }

            if (lblHeaderText02 != null)
            {
                lblHeaderText02.Font = DarkTheme.GetFont(9f, FontStyle.Bold);
                lblHeaderText02.ForeColor = DarkTheme.AccentHover;
                if (lblHeaderText01 != null)
                {
                    lblHeaderText02.Location = new Point(48 + lblHeaderText01.PreferredWidth + 6, 14);
                }
            }

            // Window Control Buttons (Minimize / Exit)
            if (btnWinExit != null)
            {
                btnWinExit.Location = new Point(pnlHeader.Width - 34, 9);
                btnWinExit.Size = new Size(26, 26);
                btnWinExit.FlatStyle = FlatStyle.Flat;
                btnWinExit.FlatAppearance.BorderSize = 0;
                btnWinExit.BackColor = Color.Transparent;
                btnWinExit.ForeColor = DarkTheme.TextMuted;
                btnWinExit.Font = DarkTheme.FontBodyBold;
                btnWinExit.Text = "✕";
            }

            if (btnWinRestore != null)
            {
                btnWinRestore.Visible = false; // Fixed-size modern utility window
            }

            if (btnWinMinimize != null)
            {
                btnWinMinimize.Location = new Point(pnlHeader.Width - 62, 9);
                btnWinMinimize.Size = new Size(26, 26);
                btnWinMinimize.FlatStyle = FlatStyle.Flat;
                btnWinMinimize.FlatAppearance.BorderSize = 0;
                btnWinMinimize.BackColor = Color.Transparent;
                btnWinMinimize.ForeColor = DarkTheme.TextMuted;
                btnWinMinimize.Font = DarkTheme.FontBodyBold;
                btnWinMinimize.Text = "—";
            }

            // Reposition Language & Utility buttons
            if (btnLangEN != null)
            {
                btnLangEN.Location = new Point(pnlHeader.Width - 100, 10);
                btnLangEN.Size = new Size(32, 24);
                btnLangEN.FlatStyle = FlatStyle.Flat;
                btnLangEN.FlatAppearance.BorderSize = 1;
                btnLangEN.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
                btnLangEN.Font = DarkTheme.GetFont(8f, FontStyle.Bold);
            }

            if (btnLangTR != null)
            {
                btnLangTR.Location = new Point(pnlHeader.Width - 136, 10);
                btnLangTR.Size = new Size(32, 24);
                btnLangTR.FlatStyle = FlatStyle.Flat;
                btnLangTR.FlatAppearance.BorderSize = 1;
                btnLangTR.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
                btnLangTR.Font = DarkTheme.GetFont(8f, FontStyle.Bold);
            }

            if (btnQuickSave != null)
            {
                btnQuickSave.Location = new Point(pnlHeader.Width - 204, 10);
                btnQuickSave.Size = new Size(64, 24);
                btnQuickSave.Text = "KAYDET";
                btnQuickSave.Font = DarkTheme.GetFont(7.5f, FontStyle.Bold);
                btnQuickSave.BackColor = DarkTheme.BgCardHeader;
                btnQuickSave.ForeColor = DarkTheme.Success;
                btnQuickSave.FlatStyle = FlatStyle.Flat;
                btnQuickSave.FlatAppearance.BorderSize = 1;
                btnQuickSave.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
            }

            if (btnQuickHideClient != null)
            {
                btnQuickHideClient.Location = new Point(pnlHeader.Width - 264, 10);
                btnQuickHideClient.Size = new Size(56, 24);
                btnQuickHideClient.Text = "GİZLE";
                btnQuickHideClient.Font = DarkTheme.GetFont(7.5f, FontStyle.Bold);
                btnQuickHideClient.BackColor = DarkTheme.BgCardHeader;
                btnQuickHideClient.ForeColor = DarkTheme.TextSecondary;
                btnQuickHideClient.FlatStyle = FlatStyle.Flat;
                btnQuickHideClient.FlatAppearance.BorderSize = 1;
                btnQuickHideClient.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
            }

            if (btnCommandCenter != null)
            {
                btnCommandCenter.Location = new Point(pnlHeader.Width - 338, 10);
                btnCommandCenter.Size = new Size(68, 24);
                btnCommandCenter.Text = "KOMUT";
                btnCommandCenter.Font = DarkTheme.GetFont(7.5f, FontStyle.Bold);
                btnCommandCenter.BackColor = DarkTheme.BgCardHeader;
                btnCommandCenter.ForeColor = Color.FromArgb(56, 189, 248);
                btnCommandCenter.FlatStyle = FlatStyle.Flat;
                btnCommandCenter.FlatAppearance.BorderSize = 1;
                btnCommandCenter.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
            }

            // Custom-Drawn Modern Status Bars (LVL / HP / MP)
            int statRightX = pnlHeader.Width - 348;

            modernHeaderMP = new ModernStatusBar
            {
                BarType = StatBarType.MP,
                DisplayMode = StatBarDisplayMode.Percentage,
                Size = new Size(104, 24),
                Location = new Point(statRightX - 108, 10)
            };
            pnlHeader.Controls.Add(modernHeaderMP);

            modernHeaderHP = new ModernStatusBar
            {
                BarType = StatBarType.HP,
                DisplayMode = StatBarDisplayMode.Percentage,
                Size = new Size(104, 24),
                Location = new Point(statRightX - 220, 10)
            };
            pnlHeader.Controls.Add(modernHeaderHP);

            modernHeaderLevel = new ModernStatusBar
            {
                BarType = StatBarType.Level,
                DisplayMode = StatBarDisplayMode.TextOnly,
                CustomText = "LVL --",
                Size = new Size(66, 24),
                Location = new Point(statRightX - 294, 10)
            };
            pnlHeader.Controls.Add(modernHeaderLevel);

            // Hide old text-based stat labels if present
            if (lblHeaderLevel != null) lblHeaderLevel.Visible = false;
            if (lblHeaderHP != null) lblHeaderHP.Visible = false;
            if (lblHeaderMP != null) lblHeaderMP.Visible = false;
        }

        private void ApplyModernSidebar()
        {
            // Hide the old legacy WinForms sidebar panel to avoid visual overlap
            if (TabPageV_Control01 != null)
            {
                TabPageV_Control01.Visible = false;
            }

            modernSidebar = new ModernSidebar
            {
                Location = new Point(0, DarkTheme.HeaderHeight),
                Size = new Size(DarkTheme.SidebarWidth, DarkTheme.DefaultWindowHeight - DarkTheme.HeaderHeight - 54),
                Dock = DockStyle.None
            };

            BuildAlchemyTab();
            if (TabPageV_Control01_Alchemy_Panel != null && TabPageV_Control01_Alchemy_Panel.Parent == null)
            {
                pnlWindow.Controls.Add(TabPageV_Control01_Alchemy_Panel);
            }

            // Category 1: BOT AYARLARI (Core Bot Features)
            modernSidebar.AddItem("Login", "Genel / Giriş", "⚡", "BOT AYARLARI", TabPageV_Control01_Login_Panel);
            modernSidebar.AddItem("Training", "Kasılma", "⚔", "BOT AYARLARI", TabPageV_Control01_Training_Panel);
            modernSidebar.AddItem("Skills", "Beceriler", "✦", "BOT AYARLARI", TabPageV_Control01_Skills_Panel);
            modernSidebar.AddItem("Character", "Koruma", "🛡", "BOT AYARLARI", TabPageV_Control01_Character_Panel);
            modernSidebar.AddItem("Town", "Şehir & İtem", "🏛", "BOT AYARLARI", TabPageV_Control01_Town_Panel);
            modernSidebar.AddItem("Alchemy", "Simya (+ Basma)", "⚗", "BOT AYARLARI", TabPageV_Control01_Alchemy_Panel);

            // Category 2: TOPLULUK (Social & Community)
            modernSidebar.AddItem("Inventory", "Envanter", "▣", "TOPLULUK", TabPageV_Control01_Inventory_Panel);
            modernSidebar.AddItem("Party", "Parti", "◈", "TOPLULUK", TabPageV_Control01_Party_Panel);
            modernSidebar.AddItem("Guild", "Guild", "★", "TOPLULUK", TabPageV_Control01_Guild_Panel);
            modernSidebar.AddItem("Academy", "Akademi", "🎓", "TOPLULUK", TabPageV_Control01_Academy_Panel);
            modernSidebar.AddItem("Players", "Oyuncular", "●", "TOPLULUK", TabPageV_Control01_Players_Panel);
            modernSidebar.AddItem("Chat", "Sohbet", "✉", "TOPLULUK", TabPageV_Control01_Chat_Panel);
            modernSidebar.AddItem("Stall", "Stall", "⚖", "TOPLULUK", TabPageV_Control01_Stall_Panel);

            // Category 3: SİSTEM (System & Information)
            modernSidebar.AddItem("Minimap", "Harita", "◎", "SİSTEM", TabPageV_Control01_Minimap_Panel);
            modernSidebar.AddItem("GameInfo", "Oyun Bilgisi", "ℹ", "SİSTEM", TabPageV_Control01_GameInfo_Panel);
            modernSidebar.AddItem("Settings", "Ayarlar", "⚙", "SİSTEM", TabPageV_Control01_Settings_Panel);

            modernSidebar.TabSelected += (key, panel) =>
            {
                Button legacyButton = GetLegacyButtonByKey(key);
                if (legacyButton != null)
                {
                    try { TabPageV_Option_Click(legacyButton, EventArgs.Empty); } catch { }
                }
            };

            pnlWindow.Controls.Add(modernSidebar);
            modernSidebar.SelectTab("Login", false);
        }

        private Button GetLegacyButtonByKey(string key)
        {
            switch (key)
            {
                case "Login": return TabPageV_Control01_Login;
                case "Training": return TabPageV_Control01_Training;
                case "Skills": return TabPageV_Control01_Skills;
                case "Character": return TabPageV_Control01_Character;
                case "Town": return TabPageV_Control01_Town;
                case "Inventory": return TabPageV_Control01_Inventory;
                case "Party": return TabPageV_Control01_Party;
                case "Guild": return TabPageV_Control01_Guild;
                case "Academy": return TabPageV_Control01_Academy;
                case "Players": return TabPageV_Control01_Players;
                case "Chat": return TabPageV_Control01_Chat;
                case "Stall": return TabPageV_Control01_Stall;
                case "Minimap": return TabPageV_Control01_Minimap;
                case "GameInfo": return TabPageV_Control01_GameInfo;
                case "Settings": return TabPageV_Control01_Settings;
                default: return null;
            }
        }

        private void ApplyModernContentPanels()
        {
            int contentX = DarkTheme.SidebarWidth + 10;
            int contentY = DarkTheme.HeaderHeight + 4;
            int contentW = DarkTheme.DefaultWindowWidth - contentX - 10;
            int contentH = 412;

            Panel[] allTabPanels = new Panel[]
            {
                TabPageV_Control01_Login_Panel,
                TabPageV_Control01_Training_Panel,
                TabPageV_Control01_Skills_Panel,
                TabPageV_Control01_Character_Panel,
                TabPageV_Control01_Town_Panel,
                TabPageV_Control01_Inventory_Panel,
                TabPageV_Control01_Party_Panel,
                TabPageV_Control01_Guild_Panel,
                TabPageV_Control01_Academy_Panel,
                TabPageV_Control01_Players_Panel,
                TabPageV_Control01_Chat_Panel,
                TabPageV_Control01_Stall_Panel,
                TabPageV_Control01_Minimap_Panel,
                TabPageV_Control01_GameInfo_Panel,
                TabPageV_Control01_Settings_Panel,
                TabPageV_Control01_Alchemy_Panel
            };

            foreach (var panel in allTabPanels)
            {
                if (panel != null)
                {
                    panel.Location = new Point(contentX, contentY);
                    panel.Size = new Size(contentW, contentH);
                    panel.BackColor = DarkTheme.BgDark;
                    panel.BorderStyle = BorderStyle.None;
                }
            }

            // Align Controls inside Login Panel with strict pixel grid
            if (TabPageV_Control01_Login_Panel != null)
            {
                int col1W = 240;
                int col2X = 252;
                int col2W = contentW - col2X - 4; // ~519px
                int card1Y = 4;
                int card1H = 196;
                int card2Y = 208;
                int card2H = 196;

                // Left Col 1: Connection Card (Y: 4 -> 200, H: 196)
                if (Login_gbxConnection != null)
                {
                    Login_gbxConnection.Location = new Point(4, card1Y);
                    Login_gbxConnection.Size = new Size(col1W, card1H);

                    // Move Silkroad client controls inside Connection card
                    if (Login_lblSilkroad != null && Login_lblSilkroad.Parent != Login_gbxConnection)
                    {
                        TabPageV_Control01_Login_Panel.Controls.Remove(Login_lblSilkroad);
                        Login_gbxConnection.Controls.Add(Login_lblSilkroad);
                    }
                    if (Login_cmbxSilkroad != null && Login_cmbxSilkroad.Parent != Login_gbxConnection)
                    {
                        TabPageV_Control01_Login_Panel.Controls.Remove(Login_cmbxSilkroad);
                        Login_gbxConnection.Controls.Add(Login_cmbxSilkroad);
                    }
                    if (Login_btnAddSilkroad != null && Login_btnAddSilkroad.Parent != Login_gbxConnection)
                    {
                        TabPageV_Control01_Login_Panel.Controls.Remove(Login_btnAddSilkroad);
                        Login_gbxConnection.Controls.Add(Login_btnAddSilkroad);
                    }

                    // Row 1: Silkroad selection
                    if (Login_lblSilkroad != null) { Login_lblSilkroad.Location = new Point(14, 28); Login_lblSilkroad.Text = "SRO:"; Login_lblSilkroad.AutoSize = true; }
                    if (Login_cmbxSilkroad != null) { Login_cmbxSilkroad.Location = new Point(54, 25); Login_cmbxSilkroad.Size = new Size(146, 24); Login_cmbxSilkroad.FlatStyle = FlatStyle.Flat; }
                    if (Login_btnAddSilkroad != null)
                    {
                        Login_btnAddSilkroad.Location = new Point(204, 24);
                        Login_btnAddSilkroad.Size = new Size(24, 25);
                        Login_btnAddSilkroad.Text = "+";
                        Login_btnAddSilkroad.Font = DarkTheme.FontBodyBold;
                        Login_btnAddSilkroad.ForeColor = DarkTheme.Accent;
                        Login_btnAddSilkroad.BackColor = DarkTheme.BgCardHeader;
                        Login_btnAddSilkroad.FlatStyle = FlatStyle.Flat;
                        Login_btnAddSilkroad.FlatAppearance.BorderSize = 1;
                        Login_btnAddSilkroad.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
                    }

                    // Row 2: Client mode radio + Start
                    if (Login_rbnClient != null) Login_rbnClient.Location = new Point(14, 58);
                    if (Login_btnStart != null) { Login_btnStart.Location = new Point(120, 54); Login_btnStart.Size = new Size(108, 28); }

                    // Row 3: Clientless radio + Launcher
                    if (Login_rbnClientless != null) Login_rbnClientless.Location = new Point(14, 90);
                    if (Login_btnLauncher != null) { Login_btnLauncher.Location = new Point(120, 86); Login_btnLauncher.Size = new Size(108, 28); }

                    // Row 4..6: Checkboxes
                    if (Login_cbxGoClientless != null) { Login_cbxGoClientless.Location = new Point(14, 122); Login_cbxGoClientless.Size = new Size(218, 20); }
                    if (Login_cbxUseReturnScroll != null) { Login_cbxUseReturnScroll.Location = new Point(14, 144); Login_cbxUseReturnScroll.Size = new Size(218, 20); }
                    if (Login_cbxRelogin != null) { Login_cbxRelogin.Location = new Point(14, 166); Login_cbxRelogin.Size = new Size(218, 20); }
                }

                // Left Col 2: Login Credentials Card (Y: 208 -> 404, H: 196)
                if (Login_gbxLogin != null)
                {
                    Login_gbxLogin.Location = new Point(4, card2Y);
                    Login_gbxLogin.Size = new Size(col1W, card2H);

                    int labelX = 12;
                    int inputX = 68;
                    int inputW = col1W - inputX - 12; // 160px

                    int row1Y = 26; // Hesap
                    int row2Y = 58; // ID
                    int row3Y = 90; // PW
                    int row4Y = 122; // Server
                    int row5Y = 154; // Karakter

                    // Row 1: Saved Account selector + Setup + Save + Delete
                    if (Login_lblAccount != null) { Login_lblAccount.Location = new Point(labelX, row1Y + 4); Login_lblAccount.AutoSize = true; Login_lblAccount.ForeColor = DarkTheme.TextMuted; }
                    if (Login_cmbxSavedAccounts != null) { Login_cmbxSavedAccounts.Location = new Point(inputX, row1Y); Login_cmbxSavedAccounts.Size = new Size(inputW - 68, 24); Login_cmbxSavedAccounts.FlatStyle = FlatStyle.Flat; }
                    if (Login_btnAccountSetup != null) { Login_btnAccountSetup.Location = new Point(inputX + inputW - 66, row1Y); Login_btnAccountSetup.Size = new Size(20, 24); }
                    if (Login_btnSaveAccount != null) { Login_btnSaveAccount.Location = new Point(inputX + inputW - 44, row1Y); Login_btnSaveAccount.Size = new Size(20, 24); }
                    if (Login_btnDeleteAccount != null) { Login_btnDeleteAccount.Location = new Point(inputX + inputW - 22, row1Y); Login_btnDeleteAccount.Size = new Size(20, 24); }

                    // Row 2: ID (Username)
                    if (Login_lblUsername != null) { Login_lblUsername.Location = new Point(labelX, row2Y + 4); Login_lblUsername.AutoSize = true; Login_lblUsername.Text = "ID:"; Login_lblUsername.ForeColor = DarkTheme.TextMuted; }
                    if (Login_tbxUsername != null)
                    {
                        Login_tbxUsername.Location = new Point(inputX + 4, row2Y + 4);
                        Login_tbxUsername.Size = new Size(inputW - 8, 16);
                        Login_tbxUsername.BorderStyle = BorderStyle.None;
                        Login_tbxUsername.TextAlign = HorizontalAlignment.Left;
                        Login_tbxUsername.BackColor = DarkTheme.BgInput;
                        Login_tbxUsername.ForeColor = DarkTheme.TextPrimary;
                    }

                    // Row 3: PW (Password)
                    if (Login_lblPassword != null) { Login_lblPassword.Location = new Point(labelX, row3Y + 4); Login_lblPassword.AutoSize = true; Login_lblPassword.Text = "PW:"; Login_lblPassword.ForeColor = DarkTheme.TextMuted; }
                    if (Login_tbxPassword != null)
                    {
                        Login_tbxPassword.Location = new Point(inputX + 4, row3Y + 4);
                        Login_tbxPassword.Size = new Size(inputW - 8, 16);
                        Login_tbxPassword.BorderStyle = BorderStyle.None;
                        Login_tbxPassword.TextAlign = HorizontalAlignment.Left;
                        Login_tbxPassword.BackColor = DarkTheme.BgInput;
                        Login_tbxPassword.ForeColor = DarkTheme.TextPrimary;
                    }

                    // Hide unused legacy captcha in this card
                    if (Login_lblCaptcha != null) Login_lblCaptcha.Visible = false;
                    if (Login_tbxCaptcha != null) Login_tbxCaptcha.Visible = false;

                    // Row 4: Server
                    if (Login_lblServer != null) { Login_lblServer.Location = new Point(labelX, row4Y + 4); Login_lblServer.AutoSize = true; Login_lblServer.Text = "Server:"; Login_lblServer.ForeColor = DarkTheme.TextMuted; }
                    if (Login_cmbxServer != null) { Login_cmbxServer.Location = new Point(inputX, row4Y); Login_cmbxServer.Size = new Size(inputW, 24); Login_cmbxServer.FlatStyle = FlatStyle.Flat; }

                    // Row 5: Character
                    if (Login_lblCharacter != null) { Login_lblCharacter.Location = new Point(labelX, row5Y + 4); Login_lblCharacter.AutoSize = true; Login_lblCharacter.Text = "Karakter:"; Login_lblCharacter.ForeColor = DarkTheme.TextMuted; }
                    if (Login_cmbxCharacter != null) { Login_cmbxCharacter.Location = new Point(inputX, row5Y); Login_cmbxCharacter.Size = new Size(inputW, 24); Login_cmbxCharacter.FlatStyle = FlatStyle.Flat; }

                    Login_gbxLogin.MouseDown += (s, e) =>
                    {
                        if (new Rectangle(inputX, row2Y, inputW, 24).Contains(e.Location)) Login_tbxUsername?.Focus();
                        else if (new Rectangle(inputX, row3Y, inputW, 24).Contains(e.Location)) Login_tbxPassword?.Focus();
                    };
                }

                // Right Col 1: Server List Card (Y: 4 -> 200, H: 196)
                if (Login_gbxServers != null)
                {
                    Login_gbxServers.Location = new Point(col2X, card1Y);
                    Login_gbxServers.Size = new Size(col2W, card1H);
                    if (Login_lstvServers != null)
                    {
                        Login_lstvServers.Location = new Point(8, 26);
                        Login_lstvServers.Size = new Size(Login_gbxServers.Width - 16, Login_gbxServers.Height - 34);
                        if (Login_lstvServers.Columns.Count >= 3)
                        {
                            Login_lstvServers.Columns[0].Width = 190;
                            Login_lstvServers.Columns[1].Width = 170;
                            Login_lstvServers.Columns[2].Width = 140;
                        }
                    }
                }

                // Right Col 1 (Alternate): Character List Card (same position/size as Server List)
                if (Login_gbxCharacters != null)
                {
                    Login_gbxCharacters.Location = new Point(col2X, card1Y);
                    Login_gbxCharacters.Size = new Size(col2W, card1H);
                    if (Login_lstvCharacters != null)
                    {
                        Login_lstvCharacters.Location = new Point(8, 26);
                        Login_lstvCharacters.Size = new Size(Login_gbxCharacters.Width - 16, Login_gbxCharacters.Height - 34);
                        if (Login_lstvCharacters.Columns.Count >= 4)
                        {
                            Login_lstvCharacters.Columns[0].Width = 180;
                            Login_lstvCharacters.Columns[1].Width = 75;
                            Login_lstvCharacters.Columns[2].Width = 110;
                            Login_lstvCharacters.Columns[3].Width = 135;
                        }
                    }
                }

                // Right Col 2: Login Flow Strategy Card (Y: 208 -> 404, H: 196)
                if (gbxStrategy != null)
                {
                    gbxStrategy.Location = new Point(col2X, card2Y);
                    gbxStrategy.Size = new Size(col2W, card2H);
                }
            }
        }

        private void ApplyModernConsoleAndActions()
        {
            int consoleX = DarkTheme.SidebarWidth + 10;
            int consoleY = 466;
            int consoleW = DarkTheme.DefaultWindowWidth - consoleX - 10;
            int consoleH = 142;

            // Modernize RichTextBox Console
            if (rtbxLogs != null)
            {
                rtbxLogs.Location = new Point(consoleX, consoleY);
                rtbxLogs.Size = new Size(consoleW, consoleH);
                rtbxLogs.BackColor = DarkTheme.BgSidebar;
                rtbxLogs.ForeColor = DarkTheme.TextSecondary;
                rtbxLogs.BorderStyle = BorderStyle.FixedSingle;
                rtbxLogs.Font = DarkTheme.FontConsole;

                if (rtbxLogs.ContextMenuStrip == null)
                {
                    ContextMenuStrip ctx = new ContextMenuStrip();
                    ToolStripMenuItem itemClear = new ToolStripMenuItem("Temizle (Clear Logs)");
                    itemClear.Click += (s, e) => { rtbxLogs.Clear(); };
                    ToolStripMenuItem itemCopy = new ToolStripMenuItem("Kopyala (Copy)");
                    itemCopy.Click += (s, e) => { if (!string.IsNullOrEmpty(rtbxLogs.SelectedText)) rtbxLogs.Copy(); else { rtbxLogs.SelectAll(); rtbxLogs.Copy(); } };
                    ctx.Items.Add(itemClear);
                    ctx.Items.Add(itemCopy);
                    rtbxLogs.ContextMenuStrip = ctx;
                }
            }

            // Sleek bottom status label (Ready / Connected)
            if (lblBotState != null)
            {
                if (pnlWindow != null && lblBotState.Parent != pnlWindow)
                {
                    this.Controls.Remove(lblBotState);
                    pnlWindow.Controls.Add(lblBotState);
                }
                lblBotState.Location = new Point(consoleX, 614);
                lblBotState.Size = new Size(consoleW, 20);
                lblBotState.BackColor = DarkTheme.BgDark;
                lblBotState.ForeColor = DarkTheme.Accent;
                lblBotState.Font = DarkTheme.FontCaption;
                lblBotState.TextAlign = ContentAlignment.MiddleLeft;
                lblBotState.Text = "● Hazır (Ready)";
                lblBotState.Visible = true;
                lblBotState.BringToFront();
            }

            // Quick Bot Start and Client Options buttons positioned cleanly in sidebar footer
            if (btnBotStart != null)
            {
                btnBotStart.Location = new Point(10, DarkTheme.DefaultWindowHeight - 48);
                btnBotStart.Size = new Size(80, 36);
                btnBotStart.FlatStyle = FlatStyle.Flat;
                btnBotStart.FlatAppearance.BorderSize = 0;
                btnBotStart.BackColor = DarkTheme.Success;
                btnBotStart.ForeColor = Color.White;
                btnBotStart.Font = DarkTheme.FontBodyBold;
                btnBotStart.Text = "BAŞLAT";
                btnBotStart.BringToFront();
            }

            if (btnClientOptions != null)
            {
                btnClientOptions.Location = new Point(95, DarkTheme.DefaultWindowHeight - 48);
                btnClientOptions.Size = new Size(80, 36);
                btnClientOptions.FlatStyle = FlatStyle.Flat;
                btnClientOptions.FlatAppearance.BorderSize = 0;
                btnClientOptions.BackColor = DarkTheme.BgInput;
                btnClientOptions.ForeColor = DarkTheme.TextPrimary;
                btnClientOptions.Font = DarkTheme.FontBodyBold;
                btnClientOptions.Text = "SEÇENEK";
                btnClientOptions.BringToFront();
            }

            // Hide old standalone analyzer button so it doesn't float over sidebar
            if (btnAnalyzer != null)
            {
                btnAnalyzer.Visible = false;
            }
        }

        /// <summary>
        /// Updates the modern HP/MP/LVL status bars from character state.
        /// </summary>
        public void UpdateModernStatusBars()
        {
            try
            {
                if (InfoManager.Character == null) return;

                if (modernHeaderLevel != null && !modernHeaderLevel.IsDisposed)
                {
                    modernHeaderLevel.InvokeIfRequired(() =>
                    {
                        modernHeaderLevel.CustomText = "LVL " + InfoManager.Character.Level;
                        modernHeaderLevel.CurrentValue = (ulong)InfoManager.Character.Level;
                    });
                }

                if (modernHeaderHP != null && !modernHeaderHP.IsDisposed)
                {
                    modernHeaderHP.InvokeIfRequired(() =>
                    {
                        modernHeaderHP.SetValues(InfoManager.Character.HP, InfoManager.Character.HPMax);
                    });
                }

                if (modernHeaderMP != null && !modernHeaderMP.IsDisposed)
                {
                    modernHeaderMP.InvokeIfRequired(() =>
                    {
                        modernHeaderMP.SetValues(InfoManager.Character.MP, InfoManager.Character.MPMax);
                    });
                }
            }
            catch { }
        }
    }
}
