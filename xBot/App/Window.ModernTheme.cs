using System;
using System.Drawing;
using System.Linq;
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
                    DarkTheme.StyleCheckBox(cbx);
                }
                else if (c is RadioButton rbn)
                {
                    DarkTheme.StyleRadioButton(rbn);
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

            BuildTargetAssistTab();
            if (TabPageV_Control01_TargetAssist_Panel != null && TabPageV_Control01_TargetAssist_Panel.Parent == null)
            {
                pnlWindow.Controls.Add(TabPageV_Control01_TargetAssist_Panel);
            }

            // Category 1: BOT AYARLARI (Core Bot Features)
            modernSidebar.AddItem("Login", "Genel / Giriş", "⚡", "BOT AYARLARI", TabPageV_Control01_Login_Panel);
            modernSidebar.AddItem("Training", "Kasılma", "⚔", "BOT AYARLARI", TabPageV_Control01_Training_Panel);
            modernSidebar.AddItem("Skills", "Beceriler", "✦", "BOT AYARLARI", TabPageV_Control01_Skills_Panel);
            modernSidebar.AddItem("Character", "Koruma", "🛡", "BOT AYARLARI", TabPageV_Control01_Character_Panel);
            modernSidebar.AddItem("Town", "Şehir & İtem", "🏛", "BOT AYARLARI", TabPageV_Control01_Town_Panel);
            modernSidebar.AddItem("Alchemy", "Simya (+ Basma)", "⚗", "BOT AYARLARI", TabPageV_Control01_Alchemy_Panel);
            modernSidebar.AddItem("TargetAssist", "Target Assist", "🎯", "BOT AYARLARI", TabPageV_Control01_TargetAssist_Panel);

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
            int contentH = DarkTheme.DefaultWindowHeight - contentY - DarkTheme.LogPanelHeight - 30;

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
                TabPageV_Control01_Alchemy_Panel,
                TabPageV_Control01_TargetAssist_Panel
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

            // 1. Align Controls inside Login Panel with spacious modern 2-column grid
            if (TabPageV_Control01_Login_Panel != null)
            {
                int col1W = 430;
                int col2X = 442;
                int col2W = contentW - col2X - 4; // ~764px
                int card1Y = 4;
                int card1H = 330;
                int card2Y = 340;
                int card2H = contentH - card2Y - 4; // ~342px

                // Left Col 1: Connection Card
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

                    // Row 1: Silkroad path
                    if (Login_lblSilkroad != null) { Login_lblSilkroad.Location = new Point(14, 32); Login_lblSilkroad.Text = "SRO:"; Login_lblSilkroad.AutoSize = true; }
                    if (Login_cmbxSilkroad != null) { Login_cmbxSilkroad.Location = new Point(60, 28); Login_cmbxSilkroad.Size = new Size(col1W - 60 - 42, 26); Login_cmbxSilkroad.FlatStyle = FlatStyle.Flat; }
                    if (Login_btnAddSilkroad != null)
                    {
                        Login_btnAddSilkroad.Location = new Point(col1W - 36, 28);
                        Login_btnAddSilkroad.Size = new Size(26, 26);
                        Login_btnAddSilkroad.Text = "+";
                        Login_btnAddSilkroad.Font = DarkTheme.FontBodyBold;
                        Login_btnAddSilkroad.ForeColor = DarkTheme.Accent;
                        Login_btnAddSilkroad.BackColor = DarkTheme.BgCardHeader;
                        Login_btnAddSilkroad.FlatStyle = FlatStyle.Flat;
                        Login_btnAddSilkroad.FlatAppearance.BorderSize = 1;
                        Login_btnAddSilkroad.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
                    }

                    // Row 2: Client mode radio + Start
                    int btnColW = col1W - 240;
                    if (Login_rbnClient != null) { Login_rbnClient.Location = new Point(14, 76); Login_rbnClient.AutoSize = true; }
                    if (Login_btnStart != null) { Login_btnStart.Location = new Point(226, 70); Login_btnStart.Size = new Size(btnColW, 34); }

                    // Row 3: Clientless radio + Launcher
                    if (Login_rbnClientless != null) { Login_rbnClientless.Location = new Point(14, 122); Login_rbnClientless.AutoSize = true; }
                    if (Login_btnLauncher != null) { Login_btnLauncher.Location = new Point(226, 116); Login_btnLauncher.Size = new Size(btnColW, 34); }

                    // Row 4..6: Checkboxes
                    if (Login_cbxGoClientless != null) { Login_cbxGoClientless.Location = new Point(14, 172); Login_cbxGoClientless.Size = new Size(col1W - 28, 24); Login_cbxGoClientless.AutoSize = true; }
                    if (Login_cbxUseReturnScroll != null) { Login_cbxUseReturnScroll.Location = new Point(14, 212); Login_cbxUseReturnScroll.Size = new Size(col1W - 28, 24); Login_cbxUseReturnScroll.AutoSize = true; }
                    if (Login_cbxRelogin != null) { Login_cbxRelogin.Location = new Point(14, 252); Login_cbxRelogin.Size = new Size(col1W - 28, 24); Login_cbxRelogin.AutoSize = true; }
                }

                // Left Col 2: Login Credentials Card
                if (Login_gbxLogin != null)
                {
                    Login_gbxLogin.Location = new Point(4, card2Y);
                    Login_gbxLogin.Size = new Size(col1W, card2H);

                    int labelX = 14;
                    int inputX = 84;
                    int inputW = col1W - inputX - 16;

                    int row1Y = 32; // Hesap
                    int row2Y = 82; // ID
                    int row3Y = 132; // PW
                    int row4Y = 182; // Server
                    int row5Y = 232; // Karakter

                    // Row 1: Saved Account selector + Setup + Save + Delete
                    if (Login_lblAccount != null) { Login_lblAccount.Location = new Point(labelX, row1Y + 4); Login_lblAccount.AutoSize = true; Login_lblAccount.ForeColor = DarkTheme.TextMuted; }
                    if (Login_cmbxSavedAccounts != null) { Login_cmbxSavedAccounts.Location = new Point(inputX, row1Y); Login_cmbxSavedAccounts.Size = new Size(inputW - 84, 26); Login_cmbxSavedAccounts.FlatStyle = FlatStyle.Flat; }
                    if (Login_btnAccountSetup != null) { Login_btnAccountSetup.Location = new Point(inputX + inputW - 80, row1Y); Login_btnAccountSetup.Size = new Size(24, 26); }
                    if (Login_btnSaveAccount != null) { Login_btnSaveAccount.Location = new Point(inputX + inputW - 54, row1Y); Login_btnSaveAccount.Size = new Size(24, 26); }
                    if (Login_btnDeleteAccount != null) { Login_btnDeleteAccount.Location = new Point(inputX + inputW - 28, row1Y); Login_btnDeleteAccount.Size = new Size(24, 26); }

                    // Row 2: ID (Username)
                    if (Login_lblUsername != null) { Login_lblUsername.Location = new Point(labelX, row2Y + 4); Login_lblUsername.AutoSize = true; Login_lblUsername.Text = "ID:"; Login_lblUsername.ForeColor = DarkTheme.TextMuted; }
                    if (Login_tbxUsername != null)
                    {
                        Login_tbxUsername.Location = new Point(inputX + 4, row2Y + 4);
                        Login_tbxUsername.Size = new Size(inputW - 8, 20);
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
                        Login_tbxPassword.Size = new Size(inputW - 8, 20);
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
                    if (Login_cmbxServer != null) { Login_cmbxServer.Location = new Point(inputX, row4Y); Login_cmbxServer.Size = new Size(inputW, 26); Login_cmbxServer.FlatStyle = FlatStyle.Flat; }

                    // Row 5: Character
                    if (Login_lblCharacter != null) { Login_lblCharacter.Location = new Point(labelX, row5Y + 4); Login_lblCharacter.AutoSize = true; Login_lblCharacter.Text = "Karakter:"; Login_lblCharacter.ForeColor = DarkTheme.TextMuted; }
                    if (Login_cmbxCharacter != null) { Login_cmbxCharacter.Location = new Point(inputX, row5Y); Login_cmbxCharacter.Size = new Size(inputW, 26); Login_cmbxCharacter.FlatStyle = FlatStyle.Flat; }

                    Login_gbxLogin.MouseDown += (s, e) =>
                    {
                        if (new Rectangle(inputX, row2Y, inputW, 26).Contains(e.Location)) Login_tbxUsername?.Focus();
                        else if (new Rectangle(inputX, row3Y, inputW, 26).Contains(e.Location)) Login_tbxPassword?.Focus();
                    };
                }

                // Right Col 1: Server List Card
                if (Login_gbxServers != null)
                {
                    Login_gbxServers.Location = new Point(col2X, card1Y);
                    Login_gbxServers.Size = new Size(col2W, card1H);
                    if (Login_lstvServers != null)
                    {
                        Login_lstvServers.Location = new Point(8, 28);
                        Login_lstvServers.Size = new Size(col2W - 16, card1H - 36);
                        if (Login_lstvServers.Columns.Count >= 3)
                        {
                            int usableW = col2W - 22;
                            Login_lstvServers.Columns[0].Width = (int)(usableW * 0.45);
                            Login_lstvServers.Columns[1].Width = (int)(usableW * 0.30);
                            Login_lstvServers.Columns[2].Width = usableW - Login_lstvServers.Columns[0].Width - Login_lstvServers.Columns[1].Width;
                        }
                    }
                }

                // Right Col 1 (Alternate): Character List Card
                if (Login_gbxCharacters != null)
                {
                    Login_gbxCharacters.Location = new Point(col2X, card1Y);
                    Login_gbxCharacters.Size = new Size(col2W, card1H);
                    if (Login_lstvCharacters != null)
                    {
                        Login_lstvCharacters.Location = new Point(8, 28);
                        Login_lstvCharacters.Size = new Size(col2W - 16, card1H - 36);
                        if (Login_lstvCharacters.Columns.Count >= 4)
                        {
                            int usableW = col2W - 22;
                            Login_lstvCharacters.Columns[0].Width = (int)(usableW * 0.35);
                            Login_lstvCharacters.Columns[1].Width = (int)(usableW * 0.15);
                            Login_lstvCharacters.Columns[2].Width = (int)(usableW * 0.25);
                            Login_lstvCharacters.Columns[3].Width = usableW - Login_lstvCharacters.Columns[0].Width - Login_lstvCharacters.Columns[1].Width - Login_lstvCharacters.Columns[2].Width;
                        }
                    }
                }

                // Right Col 2: Login Flow Strategy Card
                if (gbxStrategy != null)
                {
                    gbxStrategy.Location = new Point(col2X, card2Y);
                    gbxStrategy.Size = new Size(col2W, card2H);
                    RepositionStrategyCardControls();
                }
            }

            // 2. Skills Panel Layout
            if (TabPageV_Control01_Skills_Panel != null)
            {
                int skillListW = 320;
                if (Skills_lstvSkills != null)
                {
                    Skills_lstvSkills.Location = new Point(0, 0);
                    Skills_lstvSkills.Size = new Size(skillListW, contentH);
                    if (Skills_lstvSkills.Columns.Count > 0)
                        Skills_lstvSkills.Columns[0].Width = skillListW - 6;
                }

                int rightX = skillListW + 6;
                int rightW = contentW - rightX - 4;

                // Tab strip (Attack / Buff / Parti Buff)
                if (TabPageH_Skills != null)
                {
                    TabPageH_Skills.Location = new Point(rightX, 0);
                    TabPageH_Skills.Size = new Size(rightW, 30);
                    int tabBtnW = rightW / 3;
                    if (TabPageH_Skills_Option01 != null) { TabPageH_Skills_Option01.Location = new Point(0, 0); TabPageH_Skills_Option01.Size = new Size(tabBtnW, 28); }
                    if (TabPageH_Skills_Option02 != null) { TabPageH_Skills_Option02.Location = new Point(tabBtnW, 0); TabPageH_Skills_Option02.Size = new Size(tabBtnW, 28); }
                    if (TabPageH_Skills_Option03 != null) { TabPageH_Skills_Option03.Location = new Point(tabBtnW * 2, 0); TabPageH_Skills_Option03.Size = new Size(rightW - tabBtnW * 2, 28); }
                }

                int optPanelH = contentH - 32;
                if (TabPageH_Skills_Option01_Panel != null)
                {
                    TabPageH_Skills_Option01_Panel.Location = new Point(rightX, 32);
                    TabPageH_Skills_Option01_Panel.Size = new Size(rightW, optPanelH);
                    int listH = optPanelH - 120;

                    // Attack skill list
                    if (Skills_lstvAttackMobType_General != null)   { Skills_lstvAttackMobType_General.Location   = new Point(6, 36); Skills_lstvAttackMobType_General.Size   = new Size(rightW - 12, listH); if (Skills_lstvAttackMobType_General.Columns.Count > 0) Skills_lstvAttackMobType_General.Columns[0].Width   = rightW - 18; }
                    if (Skills_lstvAttackMobType_Champion != null)  { Skills_lstvAttackMobType_Champion.Location  = new Point(6, 36); Skills_lstvAttackMobType_Champion.Size  = new Size(rightW - 12, listH); if (Skills_lstvAttackMobType_Champion.Columns.Count > 0) Skills_lstvAttackMobType_Champion.Columns[0].Width  = rightW - 18; }
                    if (Skills_lstvAttackMobType_Giant != null)     { Skills_lstvAttackMobType_Giant.Location     = new Point(6, 36); Skills_lstvAttackMobType_Giant.Size     = new Size(rightW - 12, listH); if (Skills_lstvAttackMobType_Giant.Columns.Count > 0) Skills_lstvAttackMobType_Giant.Columns[0].Width     = rightW - 18; }
                    if (Skills_lstvAttackMobType_PartyGeneral != null)  { Skills_lstvAttackMobType_PartyGeneral.Location   = new Point(6, 36); Skills_lstvAttackMobType_PartyGeneral.Size   = new Size(rightW - 12, listH); if (Skills_lstvAttackMobType_PartyGeneral.Columns.Count > 0) Skills_lstvAttackMobType_PartyGeneral.Columns[0].Width   = rightW - 18; }
                    if (Skills_lstvAttackMobType_PartyChampion != null) { Skills_lstvAttackMobType_PartyChampion.Location  = new Point(6, 36); Skills_lstvAttackMobType_PartyChampion.Size  = new Size(rightW - 12, listH); if (Skills_lstvAttackMobType_PartyChampion.Columns.Count > 0) Skills_lstvAttackMobType_PartyChampion.Columns[0].Width  = rightW - 18; }
                    if (Skills_lstvAttackMobType_PartyGiant != null)    { Skills_lstvAttackMobType_PartyGiant.Location     = new Point(6, 36); Skills_lstvAttackMobType_PartyGiant.Size     = new Size(rightW - 12, listH); if (Skills_lstvAttackMobType_PartyGiant.Columns.Count > 0) Skills_lstvAttackMobType_PartyGiant.Columns[0].Width     = rightW - 18; }
                    if (Skills_lstvAttackMobType_Unique != null)    { Skills_lstvAttackMobType_Unique.Location    = new Point(6, 36); Skills_lstvAttackMobType_Unique.Size    = new Size(rightW - 12, listH); if (Skills_lstvAttackMobType_Unique.Columns.Count > 0) Skills_lstvAttackMobType_Unique.Columns[0].Width    = rightW - 18; }
                    if (Skills_lstvAttackMobType_Elite != null)     { Skills_lstvAttackMobType_Elite.Location     = new Point(6, 36); Skills_lstvAttackMobType_Elite.Size     = new Size(rightW - 12, listH); if (Skills_lstvAttackMobType_Elite.Columns.Count > 0) Skills_lstvAttackMobType_Elite.Columns[0].Width     = rightW - 18; }
                    if (Skills_lstvAttackMobType_Event != null)     { Skills_lstvAttackMobType_Event.Location     = new Point(6, 36); Skills_lstvAttackMobType_Event.Size     = new Size(rightW - 12, listH); if (Skills_lstvAttackMobType_Event.Columns.Count > 0) Skills_lstvAttackMobType_Event.Columns[0].Width     = rightW - 18; }

                    // Top toolbar: [+] [-] | MobType | ▲ ▼ | CastInOrder
                    if (Skills_btnAddAttack != null) { Skills_btnAddAttack.Location = new Point(6, 4); Skills_btnAddAttack.Size = new Size(32, 28); }
                    if (Skills_btnRemAttack != null) { Skills_btnRemAttack.Location = new Point(42, 4); Skills_btnRemAttack.Size = new Size(32, 28); }
                    if (Skills_cmbxAttackMobType != null) { Skills_cmbxAttackMobType.Location = new Point(80, 5); Skills_cmbxAttackMobType.Size = new Size(180, 26); }
                    if (Skills_cbxCastInOrder != null) { Skills_cbxCastInOrder.Location = new Point(370, 6); Skills_cbxCastInOrder.Size = new Size(260, 24); }

                    int bottomY = listH + 40;
                    if (Training_cbxWalkToCenter != null) { Training_cbxWalkToCenter.Location = new Point(6, bottomY); Training_cbxWalkToCenter.Size = new Size(220, 24); }
                }

                // Buff panel (Option02)
                if (TabPageH_Skills_Option02_Panel != null)
                {
                    TabPageH_Skills_Option02_Panel.Location = new Point(rightX, 32);
                    TabPageH_Skills_Option02_Panel.Size = new Size(rightW, optPanelH);
                    int listH = optPanelH - 60;
                    if (Skills_lstvBuffMobType_General != null)   { Skills_lstvBuffMobType_General.Location   = new Point(6, 34); Skills_lstvBuffMobType_General.Size   = new Size(rightW - 12, listH); if (Skills_lstvBuffMobType_General.Columns.Count > 0) Skills_lstvBuffMobType_General.Columns[0].Width   = rightW - 18; }
                    if (Skills_lstvBuffMobType_Champion != null)  { Skills_lstvBuffMobType_Champion.Location  = new Point(6, 34); Skills_lstvBuffMobType_Champion.Size  = new Size(rightW - 12, listH); if (Skills_lstvBuffMobType_Champion.Columns.Count > 0) Skills_lstvBuffMobType_Champion.Columns[0].Width  = rightW - 18; }
                    if (Skills_lstvBuffMobType_Giant != null)     { Skills_lstvBuffMobType_Giant.Location     = new Point(6, 34); Skills_lstvBuffMobType_Giant.Size     = new Size(rightW - 12, listH); if (Skills_lstvBuffMobType_Giant.Columns.Count > 0) Skills_lstvBuffMobType_Giant.Columns[0].Width     = rightW - 18; }
                    if (Skills_lstvBuffMobType_PartyGeneral != null)  { Skills_lstvBuffMobType_PartyGeneral.Location   = new Point(6, 34); Skills_lstvBuffMobType_PartyGeneral.Size   = new Size(rightW - 12, listH); if (Skills_lstvBuffMobType_PartyGeneral.Columns.Count > 0) Skills_lstvBuffMobType_PartyGeneral.Columns[0].Width   = rightW - 18; }
                    if (Skills_lstvBuffMobType_PartyChampion != null) { Skills_lstvBuffMobType_PartyChampion.Location  = new Point(6, 34); Skills_lstvBuffMobType_PartyChampion.Size  = new Size(rightW - 12, listH); if (Skills_lstvBuffMobType_PartyChampion.Columns.Count > 0) Skills_lstvBuffMobType_PartyChampion.Columns[0].Width  = rightW - 18; }
                    if (Skills_lstvBuffMobType_PartyGiant != null)    { Skills_lstvBuffMobType_PartyGiant.Location     = new Point(6, 34); Skills_lstvBuffMobType_PartyGiant.Size     = new Size(rightW - 12, listH); if (Skills_lstvBuffMobType_PartyGiant.Columns.Count > 0) Skills_lstvBuffMobType_PartyGiant.Columns[0].Width     = rightW - 18; }
                    if (Skills_lstvBuffMobType_Unique != null)    { Skills_lstvBuffMobType_Unique.Location    = new Point(6, 34); Skills_lstvBuffMobType_Unique.Size    = new Size(rightW - 12, listH); if (Skills_lstvBuffMobType_Unique.Columns.Count > 0) Skills_lstvBuffMobType_Unique.Columns[0].Width    = rightW - 18; }
                    if (Skills_lstvBuffMobType_Elite != null)     { Skills_lstvBuffMobType_Elite.Location     = new Point(6, 34); Skills_lstvBuffMobType_Elite.Size     = new Size(rightW - 12, listH); if (Skills_lstvBuffMobType_Elite.Columns.Count > 0) Skills_lstvBuffMobType_Elite.Columns[0].Width     = rightW - 18; }
                    if (Skills_btnAddBuff != null) { Skills_btnAddBuff.Location = new Point(6, 4); Skills_btnAddBuff.Size = new Size(32, 28); }
                    if (Skills_btnRemBuff != null) { Skills_btnRemBuff.Location = new Point(42, 4); Skills_btnRemBuff.Size = new Size(32, 28); }
                    if (Skills_cmbxBuffMobType != null) { Skills_cmbxBuffMobType.Location = new Point(80, 5); Skills_cmbxBuffMobType.Size = new Size(180, 26); }
                }

                // Party Buff panel (Option03)
                if (TabPageH_Skills_Option03_Panel != null)
                {
                    TabPageH_Skills_Option03_Panel.Location = new Point(rightX, 32);
                    TabPageH_Skills_Option03_Panel.Size = new Size(rightW, optPanelH);
                }
            }

            // 3. Universal Layout Pass for all other subtab-based panels
            LayoutSubTabs(TabPageV_Control01_Training_Panel, TabPageH_Training, TabPageH_Training_Option01_Panel, TabPageH_Training_Option02_Panel, TabPageH_Training_Option03_Panel);
            LayoutSubTabs(TabPageV_Control01_Character_Panel, TabPageH_Character, TabPageH_Character_Option01_Panel, TabPageH_Character_Option02_Panel, TabPageH_Character_Option03_Panel, TabPageH_Character_Option04_Panel);
            LayoutSubTabs(TabPageV_Control01_Town_Panel, TabPageH_Town, TabPageH_Town_Option01_Panel, TabPageH_Town_Option02_Panel, TabPageH_Town_Option03_Panel);
            LayoutSubTabs(TabPageV_Control01_Inventory_Panel, TabPageH_Inventory, TabPageH_Inventory_Option01_Panel, TabPageH_Inventory_Option02_Panel, TabPageH_Inventory_Option03_Panel, TabPageH_Inventory_Option04_Panel);
            LayoutSubTabs(TabPageV_Control01_Party_Panel, TabPageH_Party, TabPageH_Party_Option01_Panel, TabPageH_Party_Option02_Panel, TabPageH_Party_Option03_Panel, TabPageH_Party_Option04_Panel);
            LayoutSubTabs(TabPageV_Control01_Guild_Panel, TabPageH_Guild, TabPageH_Guild_Option01_Panel, TabPageH_Guild_Option02_Panel);
            LayoutSubTabs(TabPageV_Control01_Chat_Panel, TabPageH_Chat, TabPageH_Chat_Option01_Panel, TabPageH_Chat_Option02_Panel, TabPageH_Chat_Option03_Panel, TabPageH_Chat_Option04_Panel, TabPageH_Chat_Option05_Panel, TabPageH_Chat_Option06_Panel, TabPageH_Chat_Option07_Panel, TabPageH_Chat_Option08_Panel);
            LayoutSubTabs(TabPageV_Control01_Settings_Panel, TabPageH_Settings, TabPageH_Settings_Option01_Panel, TabPageH_Settings_Option02_Panel, TabPageH_Settings_Option03_Panel, TabPageH_Settings_Option04_Panel);
            LayoutSubTabs(TabPageV_Control01_Players_Panel, TabPageH_Players, TabPageH_Players_Option01_Panel, TabPageH_Players_Option02_Panel);
            LayoutSubTabs(TabPageV_Control01_Stall_Panel, TabPageH_Stall, TabPageH_Stall_Option01_Panel, TabPageH_Stall_Option02_Panel);
        }

        private void LayoutSubTabs(Panel mainPanel, Panel tabStrip, params Panel[] optionPanels)
        {
            if (mainPanel == null) return;
            int tabW = mainPanel.Width;
            int tabH = mainPanel.Height;

            if (tabStrip != null)
            {
                tabStrip.Location = new Point(0, 0);
                tabStrip.Size = new Size(tabW, 30);

                var tabButtons = tabStrip.Controls.OfType<Button>().OrderBy(b => b.Location.X).ToList();
                if (tabButtons.Count > 0)
                {
                    int btnW = Math.Max(120, tabW / tabButtons.Count);
                    for (int i = 0; i < tabButtons.Count; i++)
                    {
                        tabButtons[i].Location = new Point(i * btnW, 0);
                        tabButtons[i].Size = new Size(btnW, 28);
                        tabButtons[i].FlatStyle = FlatStyle.Flat;
                        tabButtons[i].FlatAppearance.BorderSize = 0;
                    }
                }
            }

            foreach (var opt in optionPanels)
            {
                if (opt != null)
                {
                    opt.Location = new Point(0, 32);
                    opt.Size = new Size(tabW, tabH - 32);

                    foreach (Control child in opt.Controls)
                    {
                        if (child is GroupBox gb && gb.Width > 500)
                        {
                            gb.Width = tabW - gb.Left - 12;
                        }
                        else if (child is ListView lv && lv.Width > 500)
                        {
                            lv.Width = tabW - lv.Left - 12;
                        }
                    }
                }
            }
        }

        private void ApplyModernConsoleAndActions()
        {
            int contentY = DarkTheme.HeaderHeight + 4;
            int contentH = DarkTheme.DefaultWindowHeight - contentY - DarkTheme.LogPanelHeight - 34;
            int consoleX = DarkTheme.SidebarWidth + 10;
            int consoleY = contentY + contentH + 6;
            int consoleW = DarkTheme.DefaultWindowWidth - consoleX - 10;
            int consoleH = DarkTheme.LogPanelHeight;

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
                lblBotState.Location = new Point(consoleX, consoleY + consoleH + 2);
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
