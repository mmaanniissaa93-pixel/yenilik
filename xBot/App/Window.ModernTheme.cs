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
        private static readonly System.Collections.Generic.HashSet<Control> _skinnedControls = new System.Collections.Generic.HashSet<Control>();

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

                // 4. Automatically style all GroupBoxes into Cards, Buttons, and Inputs
                SkinControlHierarchy(pnlWindow);

                // 5. Modernize Content Panels & Dimensions
                ApplyModernContentPanels();

                // 6. Modernize Bottom Console & Action Buttons
                ApplyModernConsoleAndActions();

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
            SkinSingleControl(parent);

            foreach (Control c in parent.Controls)
            {
                if (!(c is ModernSidebar))
                {
                    SkinControlHierarchy(c);
                }
            }
        }

        private void SkinSingleControl(Control c)
        {
            if (c == null || _skinnedControls.Contains(c)) return;
            _skinnedControls.Add(c);

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
                    FixButtonGlyph(btn);
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
                    AutoFitListView(lv);
                    lv.Resize += (s, e) => AutoFitListView(lv);
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
                    if (Login_rbnClient != null) { Login_rbnClient.Location = new Point(14, 76); Login_rbnClient.AutoSize = false; Login_rbnClient.Size = new Size(205, 26); }
                    if (Login_btnStart != null) { Login_btnStart.Location = new Point(226, 70); Login_btnStart.Size = new Size(btnColW, 34); }

                    // Row 3: Clientless radio + Launcher
                    if (Login_rbnClientless != null) { Login_rbnClientless.Location = new Point(14, 122); Login_rbnClientless.AutoSize = false; Login_rbnClientless.Size = new Size(205, 26); }
                    if (Login_btnLauncher != null) { Login_btnLauncher.Location = new Point(226, 116); Login_btnLauncher.Size = new Size(btnColW, 34); }

                    // Row 4..6: Checkboxes
                    if (Login_cbxGoClientless != null) { Login_cbxGoClientless.Location = new Point(14, 172); Login_cbxGoClientless.AutoSize = false; Login_cbxGoClientless.Size = new Size(col1W - 28, 24); }
                    if (Login_cbxUseReturnScroll != null) { Login_cbxUseReturnScroll.Location = new Point(14, 212); Login_cbxUseReturnScroll.AutoSize = false; Login_cbxUseReturnScroll.Size = new Size(col1W - 28, 24); }
                    if (Login_cbxRelogin != null) { Login_cbxRelogin.Location = new Point(14, 252); Login_cbxRelogin.AutoSize = false; Login_cbxRelogin.Size = new Size(col1W - 28, 24); }
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
            LayoutSubTabs(TabPageV_Control01_Training_Panel, TabPageH_Training, TabPageH_Training_Option01_Panel, TabPageH_Training_Option02_Panel, TabPageH_Training_Option03_Panel, pnlTrainingCombat);
            LayoutSubTabs(TabPageV_Control01_Character_Panel, TabPageH_Character, TabPageH_Character_Option01_Panel, TabPageH_Character_Option02_Panel, TabPageH_Character_Option03_Panel, TabPageH_Character_Option04_Panel);
            LayoutSubTabs(TabPageV_Control01_Town_Panel, TabPageH_Town, TabPageH_Town_Option01_Panel, TabPageH_Town_Option02_Panel, TabPageH_Town_Option03_Panel);
            LayoutSubTabs(TabPageV_Control01_Inventory_Panel, TabPageH_Inventory, TabPageH_Inventory_Option01_Panel, TabPageH_Inventory_Option02_Panel, TabPageH_Inventory_Option03_Panel, TabPageH_Inventory_Option04_Panel);
            LayoutSubTabs(TabPageV_Control01_Party_Panel, TabPageH_Party, TabPageH_Party_Option01_Panel, TabPageH_Party_Option02_Panel, TabPageH_Party_Option03_Panel, TabPageH_Party_Option04_Panel);
            LayoutSubTabs(TabPageV_Control01_Guild_Panel, TabPageH_Guild, TabPageH_Guild_Option01_Panel, TabPageH_Guild_Option02_Panel);
            LayoutSubTabs(TabPageV_Control01_Chat_Panel, TabPageH_Chat, TabPageH_Chat_Option01_Panel, TabPageH_Chat_Option02_Panel, TabPageH_Chat_Option03_Panel, TabPageH_Chat_Option04_Panel, TabPageH_Chat_Option05_Panel, TabPageH_Chat_Option06_Panel, TabPageH_Chat_Option07_Panel, TabPageH_Chat_Option08_Panel);
            LayoutSubTabs(TabPageV_Control01_Settings_Panel, TabPageH_Settings, TabPageH_Settings_Option01_Panel, TabPageH_Settings_Option02_Panel, TabPageH_Settings_Option03_Panel, TabPageH_Settings_Option04_Panel);
            LayoutSubTabs(TabPageV_Control01_Players_Panel, TabPageH_Players, TabPageH_Players_Option01_Panel, TabPageH_Players_Option02_Panel);
            LayoutSubTabs(TabPageV_Control01_Stall_Panel, TabPageH_Stall, TabPageH_Stall_Option01_Panel, TabPageH_Stall_Option02_Panel);

            // 4. Tab-specific detailed layout and modernization passes
            ApplyModernCharacterLayout();
            ApplyModernTrainingLayout();
            ApplyModernTownLayout();
            ApplyModernInventoryLayout();
            ApplyModernPartyLayout();
            ApplyModernGuildLayout();
            ApplyModernPlayersLayout();
            ApplyModernChatLayout();
            ApplyModernStallLayout();
            ApplyModernGameInfoLayout();
            ApplyModernMinimapLayout();
            ApplyModernAcademyLayout();
        }

        private void FixButtonGlyph(Button btn)
        {
            if (btn == null) return;

            string name = btn.Name ?? string.Empty;
            string text = btn.Text ?? string.Empty;

            if (name.IndexOf("btnAddSTR", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("btnAddINT", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("btnAddAttack", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("btnAddBuff", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("btnAddSilkroad", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("AreaAdd", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("btnAddArea", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                btn.Text = "+";
                btn.Font = DarkTheme.FontBodyBold;
                return;
            }

            if (name.IndexOf("btnRemAttack", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("btnRemBuff", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("AreaRemove", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("btnRemArea", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                btn.Text = "-";
                btn.Font = DarkTheme.FontBodyBold;
                return;
            }

            if (name.IndexOf("ScriptPath", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("LoadScript", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                btn.Text = "...";
                btn.Font = DarkTheme.FontBodyBold;
                return;
            }

            if (name.IndexOf("Refresh", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (btn.Width >= 70 || (btn.Text != null && btn.Text.Length > 2)) return;
                btn.Text = "↻";
                btn.Font = DarkTheme.FontBodyBold;
                return;
            }

            if (name.IndexOf("Sort", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                if (btn.Width >= 70 || (btn.Text != null && btn.Text.Length > 2)) return;
                btn.Text = "⇅";
                btn.Font = DarkTheme.FontBodyBold;
                return;
            }

            if (name.IndexOf("NextPage", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                btn.Text = "▶";
                btn.Font = DarkTheme.FontBody;
                return;
            }

            if (name.IndexOf("LastPage", StringComparison.OrdinalIgnoreCase) >= 0 ||
                name.IndexOf("PrevPage", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                btn.Text = "◀";
                btn.Font = DarkTheme.FontBody;
                return;
            }

            string tag = (btn.Tag as string) ?? string.Empty;
            if (tag.IndexOf("Font Awesome", StringComparison.OrdinalIgnoreCase) >= 0 ||
                text.Contains("🔄") ||
                text.Any(ch => (ch >= 0xE000 && ch <= 0xF8FF) || ch == '' || ch == ''))
            {
                if (text == "" || text.Contains("\uF067") || text.Contains("\uF0FE"))
                {
                    btn.Text = "+";
                }
                else if (text.Contains("\uF068"))
                {
                    btn.Text = "-";
                }
                else if (text == "" || text.Contains("\uF07C") || text.Contains("\uF114") || text.Contains("\uF07B"))
                {
                    btn.Text = "...";
                }
                else if (text == "🔄" || text.Contains("\uF021") || text.Contains("\uF01E") || text.Contains("\uF2F9") || text.Contains("\uF2F1"))
                {
                    btn.Text = "↻";
                }
                else if (text.Contains("\uF0DC") || text.Contains("\uF160") || text.Contains("\uF161"))
                {
                    btn.Text = "⇅";
                }
                else if (text.Contains("\uF054") || text.Contains("\uF105"))
                {
                    btn.Text = "▶";
                }
                else if (text.Contains("\uF053") || text.Contains("\uF104"))
                {
                    btn.Text = "◀";
                }
                else if (text.Contains("\uF00C"))
                {
                    btn.Text = "✓";
                }
                else if (text.Contains("\uF00D"))
                {
                    btn.Text = "✕";
                }
                btn.Font = DarkTheme.FontBody;
            }
        }

        private void AutoFitListView(ListView lv)
        {
            if (lv == null || lv.Columns.Count == 0) return;
            try
            {
                int totalWidth = lv.ClientSize.Width;
                if (totalWidth <= 0) return;

                if (lv.Columns.Count == 1)
                {
                    lv.Columns[0].Width = Math.Max(50, totalWidth - 4);
                    return;
                }

                int otherColumnsWidth = 0;
                for (int i = 0; i < lv.Columns.Count - 1; i++)
                {
                    otherColumnsWidth += lv.Columns[i].Width;
                }

                int lastColWidth = totalWidth - otherColumnsWidth - 4;
                if (lastColWidth > 30)
                {
                    lv.Columns[lv.Columns.Count - 1].Width = lastColWidth;
                }
            }
            catch { }
        }

        private void LayoutSubTabs(Panel mainPanel, Panel tabStrip, params Panel[] optionPanels)
        {
            if (mainPanel == null) return;
            int tabW = mainPanel.Width;
            int tabH = mainPanel.Height;

            if (tabStrip != null)
            {
                tabStrip.Location = new Point(0, 0);
                tabStrip.Size = new Size(tabW, 32);
                tabStrip.BackColor = DarkTheme.BgSidebar;

                // Hide placeholder buttons like ". . ."
                foreach (Control c in tabStrip.Controls)
                {
                    if (c is Button b)
                    {
                        string t = (b.Text ?? "").Trim();
                        if (t == ". . ." || t == "..." || b.Name == "TabPageH_Party_Option04")
                        {
                            b.Visible = false;
                        }
                    }
                }

                var tabButtons = tabStrip.Controls.OfType<Button>().Where(b => b.Visible).OrderBy(b => b.TabIndex).ToList();
                if (tabButtons.Count > 0)
                {
                    int btnW = Math.Min(180, Math.Max(120, (tabW - 20) / tabButtons.Count));
                    for (int i = 0; i < tabButtons.Count; i++)
                    {
                        var btn = tabButtons[i];
                        btn.Location = new Point(i * (btnW + 4), 0);
                        btn.Size = new Size(btnW, 30);
                        btn.FlatStyle = FlatStyle.Flat;
                        btn.FlatAppearance.BorderSize = 0;
                        btn.Font = DarkTheme.FontBodyBold;

                        bool isActive = (tabStrip.Tag == btn || (tabStrip.Tag == null && i == 0));
                        btn.BackColor = isActive ? DarkTheme.BgCard : DarkTheme.BgCardHeader;
                        btn.ForeColor = isActive ? DarkTheme.Accent : DarkTheme.TextSecondary;

                        btn.Paint += (s, pe) =>
                        {
                            Button b = s as Button;
                            if (b == null) return;
                            bool active = (tabStrip.Tag == b || (tabStrip.Tag == null && b.TabIndex == tabButtons[0].TabIndex));
                            if (active)
                            {
                                using (SolidBrush accentBrush = new SolidBrush(DarkTheme.Accent))
                                {
                                    pe.Graphics.FillRectangle(accentBrush, 0, b.Height - 3, b.Width, 3);
                                }
                            }
                        };

                        btn.Click += (s, e) =>
                        {
                            foreach (var b in tabButtons)
                            {
                                bool active = (tabStrip.Tag == b);
                                b.BackColor = active ? DarkTheme.BgCard : DarkTheme.BgCardHeader;
                                b.ForeColor = active ? DarkTheme.Accent : DarkTheme.TextSecondary;
                                b.Invalidate();
                            }
                        };
                    }
                }
            }

            foreach (var opt in optionPanels)
            {
                if (opt != null)
                {
                    opt.Location = new Point(0, 34);
                    opt.Size = new Size(tabW, tabH - 34);
                    opt.BackColor = DarkTheme.BgDark;
                    opt.BorderStyle = BorderStyle.None;

                    foreach (Control child in opt.Controls)
                    {
                        if (child is GroupBox gb && gb.Width > 500)
                        {
                            gb.Width = tabW - gb.Left - 12;
                        }
                        else if (child is ListView lv)
                        {
                            if (lv.Width > 500)
                            {
                                lv.Width = tabW - lv.Left - 12;
                            }
                            AutoFitListView(lv);
                        }
                    }
                }
            }
        }

        private void ApplyModernCharacterLayout()
        {
            if (TabPageV_Control01_Character_Panel == null) return;
            int tabW = TabPageV_Control01_Character_Panel.Width;
            int tabH = TabPageV_Control01_Character_Panel.Height;
            int col1W = 595;
            int col2X = 605;
            int col2W = tabW - col2X - 8;

            // --- Option 01: Info Panel ---
            if (TabPageH_Character_Option01_Panel != null)
            {
                int barW = col1W - 20;

                if (Character_lblLevel != null)
                {
                    Character_lblLevel.Location = new Point(10, 8);
                    Character_lblLevel.Size = new Size(120, 22);
                    Character_lblLevel.Font = DarkTheme.FontHeader;
                    Character_lblLevel.ForeColor = DarkTheme.TextPrimary;
                }

                if (Character_pgbHP != null)
                {
                    Character_pgbHP.Location = new Point(10, 32);
                    Character_pgbHP.Size = new Size(barW, 22);
                    Character_pgbHP.BackColor = DarkTheme.Success;
                }

                if (Character_pgbMP != null)
                {
                    Character_pgbMP.Location = new Point(10, 58);
                    Character_pgbMP.Size = new Size(barW, 22);
                    Character_pgbMP.BackColor = DarkTheme.Mana;
                }

                if (Character_pgbExp != null)
                {
                    Character_pgbExp.Location = new Point(10, 84);
                    Character_pgbExp.Size = new Size(barW, 22);
                    Character_pgbExp.BackColor = DarkTheme.Warning;
                }

                if (Character_lblJobLevel != null)
                {
                    Character_lblJobLevel.Location = new Point(10, 110);
                    Character_lblJobLevel.Size = new Size(120, 22);
                    Character_lblJobLevel.Font = DarkTheme.FontBodyBold;
                    Character_lblJobLevel.ForeColor = DarkTheme.TextPrimary;
                }

                if (Character_pgbJobExp != null)
                {
                    Character_pgbJobExp.Location = new Point(135, 110);
                    Character_pgbJobExp.Size = new Size(barW - 125, 22);
                    Character_pgbJobExp.BackColor = DarkTheme.SystemPurple;
                }

                int gridY = 140;
                if (Character_lblGoldText != null) { Character_lblGoldText.Location = new Point(10, gridY); Character_lblGoldText.AutoSize = true; }
                if (Character_lblGold != null) { Character_lblGold.Location = new Point(70, gridY); Character_lblGold.Size = new Size(180, 20); }
                if (Character_lblSPText != null) { Character_lblSPText.Location = new Point(270, gridY); Character_lblSPText.AutoSize = true; }
                if (Character_lblSP != null) { Character_lblSP.Location = new Point(315, gridY); Character_lblSP.Size = new Size(160, 20); }

                gridY += 26;
                if (Character_lblLocationText != null) { Character_lblLocationText.Location = new Point(10, gridY); Character_lblLocationText.AutoSize = true; }
                if (Character_lblLocation != null) { Character_lblLocation.Location = new Point(90, gridY); Character_lblLocation.Size = new Size(barW - 90, 20); }

                gridY += 26;
                if (Character_lblCoords != null) { Character_lblCoords.Location = new Point(10, gridY); Character_lblCoords.AutoSize = true; }
                if (Character_lblCoordX != null) { Character_lblCoordX.Location = new Point(120, gridY); Character_lblCoordX.Size = new Size(100, 20); }
                if (Character_lblCoordY != null) { Character_lblCoordY.Location = new Point(230, gridY); Character_lblCoordY.Size = new Size(100, 20); }

                if (Character_pnlBuffs != null)
                {
                    Character_pnlBuffs.Location = new Point(10, 222);
                    Character_pnlBuffs.Size = new Size(barW, 160);
                    Character_pnlBuffs.BackColor = DarkTheme.BgCard;
                }

                if (Character_gbxStatPoints != null)
                {
                    Character_gbxStatPoints.Location = new Point(10, 390);
                    Character_gbxStatPoints.Size = new Size(barW, 160);

                    if (Character_lblAddSTR != null) Character_lblAddSTR.Location = new Point(16, 32);
                    if (Character_lblSTR != null) Character_lblSTR.Location = new Point(65, 32);
                    if (Character_btnAddSTR != null) { Character_btnAddSTR.Location = new Point(115, 30); Character_btnAddSTR.Size = new Size(30, 26); Character_btnAddSTR.Text = "+"; }

                    if (Character_lblAddINT != null) Character_lblAddINT.Location = new Point(165, 32);
                    if (Character_lblINT != null) Character_lblINT.Location = new Point(210, 32);
                    if (Character_btnAddINT != null) { Character_btnAddINT.Location = new Point(255, 30); Character_btnAddINT.Size = new Size(30, 26); Character_btnAddINT.Text = "+"; }

                    if (Character_lblStatPoints != null) { Character_lblStatPoints.Location = new Point(310, 30); Character_lblStatPoints.Size = new Size(100, 26); }

                    if (Character_cbxAutoStat != null) { Character_cbxAutoStat.Location = new Point(16, 75); Character_cbxAutoStat.AutoSize = true; }
                    if (Character_rbnAutoSTR != null) { Character_rbnAutoSTR.Location = new Point(140, 75); Character_rbnAutoSTR.AutoSize = true; }
                    if (Character_rbnAutoINT != null) { Character_rbnAutoINT.Location = new Point(240, 75); Character_rbnAutoINT.AutoSize = true; }
                }

                if (Character_gbxMessageFilter != null)
                {
                    Character_gbxMessageFilter.Location = new Point(col2X, 8);
                    Character_gbxMessageFilter.Size = new Size(col2W, 550);

                    if (Character_cbxMessageEvents != null) { Character_cbxMessageEvents.Location = new Point(16, 28); Character_cbxMessageEvents.AutoSize = true; }
                    if (Character_cbxMessagePicks != null) { Character_cbxMessagePicks.Location = new Point(115, 28); Character_cbxMessagePicks.AutoSize = true; }
                    if (Character_cbxMessageUniques != null) { Character_cbxMessageUniques.Location = new Point(205, 28); Character_cbxMessageUniques.AutoSize = true; }
                    if (Character_cbxMessageExp != null) { Character_cbxMessageExp.Location = new Point(305, 28); Character_cbxMessageExp.AutoSize = true; }

                    if (Character_rtbxMessageFilter != null)
                    {
                        Character_rtbxMessageFilter.Location = new Point(12, 60);
                        Character_rtbxMessageFilter.Size = new Size(col2W - 24, 475);
                        Character_rtbxMessageFilter.BackColor = DarkTheme.BgSidebar;
                        Character_rtbxMessageFilter.ForeColor = DarkTheme.TextSecondary;
                    }
                }
            }

            // --- Option 02: Potions Panel ---
            if (TabPageH_Character_Option02_Panel != null)
            {
                if (Character_gbxPotionsPlayer != null)
                {
                    Character_gbxPotionsPlayer.Location = new Point(6, 6);
                    Character_gbxPotionsPlayer.Size = new Size(col1W, 550);

                    int py = 32;
                    int rowH = 46;

                    AlignPotionRow(Character_cbxUseHP, Character_tbxUseHP, "Sağlık İksiri (HP):", py); py += rowH;
                    AlignPotionRow(Character_cbxUseMP, Character_tbxUseMP, "Mana İksiri (MP):", py); py += rowH;
                    AlignPotionRow(Character_cbxUseHPVigor, Character_tbxUseHPVigor, "Vigor İksiri (HP Vigor):", py); py += rowH;
                    AlignPotionRow(Character_cbxUseMPVigor, Character_tbxUseMPVigor, "Vigor İksiri (MP Vigor):", py); py += rowH;

                    if (Character_cbxUseHPGrain != null) { Character_cbxUseHPGrain.Text = "HP Tanesi (HP Grain)"; Character_cbxUseHPGrain.Location = new Point(16, py); Character_cbxUseHPGrain.Size = new Size(260, 24); } py += rowH;
                    if (Character_cbxUseMPGrain != null) { Character_cbxUseMPGrain.Text = "MP Tanesi (MP Grain)"; Character_cbxUseMPGrain.Location = new Point(16, py); Character_cbxUseMPGrain.Size = new Size(260, 24); } py += rowH;
                    if (Character_cbxUsePillUniversal != null) { Character_cbxUsePillUniversal.Text = "Evrensel Hap (Universal Pill)"; Character_cbxUsePillUniversal.Location = new Point(16, py); Character_cbxUsePillUniversal.Size = new Size(260, 24); } py += rowH;
                    if (Character_cbxUsePillPurification != null) { Character_cbxUsePillPurification.Text = "Arındırma Hapı (Purification Pill)"; Character_cbxUsePillPurification.Location = new Point(16, py); Character_cbxUsePillPurification.Size = new Size(260, 24); }
                }

                if (Character_gbxPotionPet != null)
                {
                    Character_gbxPotionPet.Location = new Point(col2X, 6);
                    Character_gbxPotionPet.Size = new Size(col2W, 550);

                    int py = 32;
                    int rowH = 46;

                    AlignPotionRow(Character_cbxUsePetHP, Character_tbxUsePetHP, "Pet Sağlık İksiri (Pet HP):", py); py += rowH;
                    AlignPotionRow(Character_cbxUseTransportHP, Character_tbxUseTransportHP, "Taşıma Bineği HP:", py); py += rowH;
                    if (Character_cbxUsePetsPill != null) { Character_cbxUsePetsPill.Text = "Pet Durum Hapı (Pet Pills)"; Character_cbxUsePetsPill.Location = new Point(16, py); Character_cbxUsePetsPill.Size = new Size(260, 24); } py += rowH;
                    AlignPotionRow(Character_cbxUsePetHGP, Character_tbxUsePetHGP, "Pet Açlık İksiri (Pet HGP):", py);
                }
            }

            // --- Option 03: Koruma / Protection Panel ---
            if (TabPageH_Character_Option03_Panel != null)
            {
                if (gbxProtectionSkillPet != null)
                {
                    gbxProtectionSkillPet.Location = new Point(6, 6);
                    gbxProtectionSkillPet.Size = new Size(col1W, 240);

                    if (cbxProtectionSkillHP != null) { cbxProtectionSkillHP.Location = new Point(16, 30); cbxProtectionSkillHP.Size = new Size(180, 24); cbxProtectionSkillHP.Text = "Acil HP Skilli:"; }
                    if (nudProtectionSkillHP != null) { nudProtectionSkillHP.Location = new Point(200, 29); nudProtectionSkillHP.Size = new Size(50, 24); }
                    if (lblProtectionSkillHPSuffix != null) { lblProtectionSkillHPSuffix.Location = new Point(258, 32); lblProtectionSkillHPSuffix.Text = "% veya altındayken"; lblProtectionSkillHPSuffix.AutoSize = true; }

                    if (cbxProtectionSkillMP != null) { cbxProtectionSkillMP.Location = new Point(16, 68); cbxProtectionSkillMP.Size = new Size(180, 24); cbxProtectionSkillMP.Text = "Acil MP Skilli:"; }
                    if (nudProtectionSkillMP != null) { nudProtectionSkillMP.Location = new Point(200, 67); nudProtectionSkillMP.Size = new Size(50, 24); }
                    if (lblProtectionSkillMPSuffix != null) { lblProtectionSkillMPSuffix.Location = new Point(258, 70); lblProtectionSkillMPSuffix.Text = "% veya altındayken"; lblProtectionSkillMPSuffix.AutoSize = true; }

                    if (cbxProtectionCure != null) { cbxProtectionCure.Location = new Point(16, 106); cbxProtectionCure.Size = new Size(350, 24); cbxProtectionCure.Text = "Kötü durumu (debuff) skill ile temizle"; }
                    if (cbxProtectionPetRevive != null) { cbxProtectionPetRevive.Location = new Point(16, 144); cbxProtectionPetRevive.Size = new Size(350, 24); cbxProtectionPetRevive.Text = "Ölen peti dirilt (Grass of Life ile)"; }
                    if (cbxProtectionPetSummon != null) { cbxProtectionPetSummon.Location = new Point(16, 182); cbxProtectionPetSummon.Size = new Size(350, 24); cbxProtectionPetSummon.Text = "Peti otomatik çağır (Auto Summon)"; }
                }

                if (gbxProtectionAutoStat != null)
                {
                    gbxProtectionAutoStat.Location = new Point(6, 256);
                    gbxProtectionAutoStat.Size = new Size(col1W, 300);

                    if (cbxAutoStatEnabled != null) { cbxAutoStatEnabled.Location = new Point(16, 30); cbxAutoStatEnabled.Size = new Size(350, 24); cbxAutoStatEnabled.Text = "Level atlayınca otomatik stat dağıt"; }

                    if (lblProtectionStr != null) { lblProtectionStr.Location = new Point(16, 68); lblProtectionStr.AutoSize = true; }
                    if (nudAutoStatSTR != null) { nudAutoStatSTR.Location = new Point(60, 66); nudAutoStatSTR.Size = new Size(50, 24); }

                    if (lblProtectionInt != null) { lblProtectionInt.Location = new Point(130, 68); lblProtectionInt.AutoSize = true; }
                    if (nudAutoStatINT != null) { nudAutoStatINT.Location = new Point(175, 66); nudAutoStatINT.Size = new Size(50, 24); }

                    if (btnProtectionDistributeNow != null) { btnProtectionDistributeNow.Location = new Point(250, 64); btnProtectionDistributeNow.Size = new Size(140, 28); }

                    if (lblProtectionStatGuidance != null) { lblProtectionStatGuidance.Location = new Point(16, 110); lblProtectionStatGuidance.Size = new Size(col1W - 32, 60); }
                    if (lblProtectionRemainInfo != null) { lblProtectionRemainInfo.Location = new Point(16, 185); lblProtectionRemainInfo.AutoSize = true; }
                    if (lblProtectionStatPointsRemain != null) { lblProtectionStatPointsRemain.Location = new Point(180, 185); lblProtectionStatPointsRemain.AutoSize = true; }
                }

                if (gbxProtectionReturn != null)
                {
                    gbxProtectionReturn.Location = new Point(col2X, 6);
                    gbxProtectionReturn.Size = new Size(col2W, 330);

                    int sub1X = 16;
                    int sub2X = 290;

                    if (cbxProtectionNoArrows != null) { cbxProtectionNoArrows.Location = new Point(sub1X, 30); cbxProtectionNoArrows.Size = new Size(240, 24); cbxProtectionNoArrows.Text = "Ok / bolt bitince kasabaya dön"; }
                    if (cbxProtectionFullInventory != null) { cbxProtectionFullInventory.Location = new Point(sub1X, 68); cbxProtectionFullInventory.Size = new Size(240, 24); cbxProtectionFullInventory.Text = "Çanta dolunca kasabaya dön"; }
                    if (cbxProtectionFullPetInventory != null) { cbxProtectionFullPetInventory.Location = new Point(sub1X, 106); cbxProtectionFullPetInventory.Size = new Size(240, 24); cbxProtectionFullPetInventory.Text = "Pet çantası dolunca dön"; }

                    if (cbxProtectionLowHP != null) { cbxProtectionLowHP.Location = new Point(sub1X, 144); cbxProtectionLowHP.Size = new Size(120, 24); cbxProtectionLowHP.Text = "HP stoğu düşük:"; }
                    if (nudProtectionHP != null) { nudProtectionHP.Location = new Point(sub1X + 130, 143); nudProtectionHP.Size = new Size(50, 24); }

                    if (cbxProtectionLowMP != null) { cbxProtectionLowMP.Location = new Point(sub1X, 182); cbxProtectionLowMP.Size = new Size(120, 24); cbxProtectionLowMP.Text = "MP stoğu düşük:"; }
                    if (nudProtectionMP != null) { nudProtectionMP.Location = new Point(sub1X + 130, 181); nudProtectionMP.Size = new Size(50, 24); }

                    if (cbxProtectionDurability != null) { cbxProtectionDurability.Location = new Point(sub2X, 30); cbxProtectionDurability.Size = new Size(130, 24); cbxProtectionDurability.Text = "Durability düşük:"; }
                    if (nudProtectionDurability != null) { nudProtectionDurability.Location = new Point(sub2X + 135, 29); nudProtectionDurability.Size = new Size(50, 24); }

                    if (cbxProtectionLevelUp != null) { cbxProtectionLevelUp.Location = new Point(sub2X, 68); cbxProtectionLevelUp.Size = new Size(240, 24); cbxProtectionLevelUp.Text = "Level atlayınca kasabaya dön"; }
                    if (cbxProtectionStopInTown != null) { cbxProtectionStopInTown.Location = new Point(sub2X, 106); cbxProtectionStopInTown.Size = new Size(240, 24); cbxProtectionStopInTown.Text = "Şehirde botu durdur"; }
                    if (cbxProtectionDead != null) { cbxProtectionDead.Location = new Point(sub2X, 144); cbxProtectionDead.Size = new Size(240, 24); cbxProtectionDead.Text = "Ölüm sonrası kasabaya dön"; }

                    if (lblProtectionDeadDelay != null) { lblProtectionDeadDelay.Location = new Point(sub2X, 184); lblProtectionDeadDelay.AutoSize = true; }
                    if (nudProtectionDeadDelay != null) { nudProtectionDeadDelay.Location = new Point(sub2X + 110, 182); nudProtectionDeadDelay.Size = new Size(60, 24); }
                }

                if (gbxProtectionSummary != null)
                {
                    gbxProtectionSummary.Location = new Point(col2X, 346);
                    gbxProtectionSummary.Size = new Size(col2W, 210);

                    if (btnProtectionManualPet != null) { btnProtectionManualPet.Location = new Point(16, 32); btnProtectionManualPet.Size = new Size(220, 34); }
                    if (btnProtectionSave != null) { btnProtectionSave.Location = new Point(250, 32); btnProtectionSave.Size = new Size(140, 34); }
                    if (lblProtectionOverview != null) { lblProtectionOverview.Location = new Point(16, 80); lblProtectionOverview.Size = new Size(col2W - 32, 100); }
                }
            }

            // --- Option 04: Misc Panel ---
            if (TabPageH_Character_Option04_Panel != null)
            {
                GroupBox gbxMiscRessExchange = TabPageH_Character_Option04_Panel.Controls["gbxMiscRessExchange"] as GroupBox;
                if (gbxMiscRessExchange == null)
                {
                    gbxMiscRessExchange = new GroupBox
                    {
                        Name = "gbxMiscRessExchange",
                        Text = LocalizationManager.CurrentLanguage == "TR" ? "Dirilme & Takas Ayarları" : "Resurrection & Exchange"
                    };
                    TabPageH_Character_Option04_Panel.Controls.Add(gbxMiscRessExchange);
                    SkinControlHierarchy(gbxMiscRessExchange);
                }
                gbxMiscRessExchange.Location = new Point(6, 6);
                gbxMiscRessExchange.Size = new Size(col1W, tabH - 46);

                Control[] ressControls = new Control[] {
                    Character_cbxAcceptRess, Character_cbxAcceptRessPartyOnly,
                    Character_cbxAcceptExchange, Character_cbxAcceptExchangeLeaderOnly,
                    Character_cbxConfirmExchange, Character_cbxApproveExchange, Character_cbxRefuseExchange
                };
                int my = 32;
                int rowH = 34;
                foreach (var c in ressControls)
                {
                    if (c != null)
                    {
                        if (c.Parent != gbxMiscRessExchange)
                        {
                            c.Parent?.Controls.Remove(c);
                            gbxMiscRessExchange.Controls.Add(c);
                        }
                        int x = (c == Character_cbxAcceptRessPartyOnly || c == Character_cbxAcceptExchangeLeaderOnly ||
                                 c == Character_cbxConfirmExchange || c == Character_cbxApproveExchange) ? 36 : 16;
                        c.Location = new Point(x, my);
                        c.Size = new Size(col1W - 44, 24);
                        my += (c == Character_cbxAcceptRessPartyOnly) ? rowH + 12 : rowH;
                    }
                }

                GroupBox gbxMiscPVP = TabPageH_Character_Option04_Panel.Controls["gbxMiscPVP"] as GroupBox;
                if (gbxMiscPVP == null)
                {
                    gbxMiscPVP = new GroupBox
                    {
                        Name = "gbxMiscPVP",
                        Text = LocalizationManager.CurrentLanguage == "TR" ? "PvP & Silah Değişimi" : "PvP & Weapon Switching"
                    };
                    TabPageH_Character_Option04_Panel.Controls.Add(gbxMiscPVP);
                    SkinControlHierarchy(gbxMiscPVP);
                }
                gbxMiscPVP.Location = new Point(col2X, 6);
                gbxMiscPVP.Size = new Size(col2W, 140);

                Control[] pvpControls = new Control[] { Character_cbxPVPMode, Character_cbxPVPModeUseShield };
                int py = 32;
                foreach (var c in pvpControls)
                {
                    if (c != null)
                    {
                        if (c.Parent != gbxMiscPVP)
                        {
                            c.Parent?.Controls.Remove(c);
                            gbxMiscPVP.Controls.Add(c);
                        }
                        c.Location = new Point(16, py);
                        c.Size = new Size(col2W - 32, 24);
                        py += rowH;
                    }
                }
            }
        }

        private void AlignPotionRow(CheckBox cbx, TextBox tbx, string text, int y)
        {
            if (cbx == null) return;
            cbx.Text = text;
            cbx.Location = new Point(16, y);
            cbx.Size = new Size(210, 24);
            if (tbx != null)
            {
                tbx.Location = new Point(232, y);
                tbx.Size = new Size(50, 24);
                tbx.TextAlign = HorizontalAlignment.Center;
            }
        }

        private void ApplyModernTrainingLayout()
        {
            if (TabPageV_Control01_Training_Panel == null) return;
            int tabW = TabPageV_Control01_Training_Panel.Width;
            int tabH = TabPageV_Control01_Training_Panel.Height;
            int col1W = 595;
            int col2X = 605;
            int col2W = tabW - col2X - 8;

            // --- Option 01: Area Panel ---
            if (TabPageH_Training_Option01_Panel != null)
            {
                int listW = 460;
                if (Training_lstvAreas != null)
                {
                    Training_lstvAreas.Location = new Point(6, 6);
                    Training_lstvAreas.Size = new Size(listW, tabH - 46);
                    if (Training_lstvAreas.Columns.Count > 0)
                        Training_lstvAreas.Columns[0].Width = listW - 6;
                    AutoFitListView(Training_lstvAreas);
                }

                int rightX = listW + 16;
                int rightW = tabW - rightX - 8;

                GroupBox gbxAreaDetails = TabPageH_Training_Option01_Panel.Controls["gbxAreaDetails"] as GroupBox;
                if (gbxAreaDetails == null)
                {
                    gbxAreaDetails = new GroupBox
                    {
                        Name = "gbxAreaDetails",
                        Text = LocalizationManager.CurrentLanguage == "TR" ? "Kasılma Alanı Koordinatları & Script" : "Training Area Coordinates & Script"
                    };
                    TabPageH_Training_Option01_Panel.Controls.Add(gbxAreaDetails);
                    SkinControlHierarchy(gbxAreaDetails);
                }
                gbxAreaDetails.Location = new Point(rightX, 6);
                gbxAreaDetails.Size = new Size(rightW, tabH - 46);

                Control[] areaControls = new Control[] {
                    Training_btnGetCoordinates, Training_lblRegion, Training_tbxRegion,
                    Training_lblX, Training_tbxX, Training_lblY, Training_tbxY,
                    Training_lblZ, Training_tbxZ, Training_lblRadius, Training_tbxRadius,
                    Training_lblScriptPath, Training_tbxScriptPath, Training_btnLoadScriptPath
                };
                foreach (var c in areaControls)
                {
                    if (c != null && c.Parent != gbxAreaDetails)
                    {
                        c.Parent?.Controls.Remove(c);
                        gbxAreaDetails.Controls.Add(c);
                    }
                }

                if (Training_btnGetCoordinates != null)
                {
                    Training_btnGetCoordinates.Location = new Point(16, 30);
                    Training_btnGetCoordinates.Size = new Size(220, 34);
                    Training_btnGetCoordinates.BackColor = DarkTheme.Accent;
                    Training_btnGetCoordinates.ForeColor = Color.White;
                    Training_btnGetCoordinates.FlatStyle = FlatStyle.Flat;
                }

                int coordY = 82;
                if (Training_lblRegion != null) { Training_lblRegion.Location = new Point(16, coordY + 2); Training_lblRegion.AutoSize = true; }
                if (Training_tbxRegion != null) { Training_tbxRegion.Location = new Point(70, coordY); Training_tbxRegion.Size = new Size(60, 24); }

                if (Training_lblX != null) { Training_lblX.Location = new Point(145, coordY + 2); Training_lblX.AutoSize = true; }
                if (Training_tbxX != null) { Training_tbxX.Location = new Point(165, coordY); Training_tbxX.Size = new Size(60, 24); }

                if (Training_lblY != null) { Training_lblY.Location = new Point(240, coordY + 2); Training_lblY.AutoSize = true; }
                if (Training_tbxY != null) { Training_tbxY.Location = new Point(260, coordY); Training_tbxY.Size = new Size(60, 24); }

                if (Training_lblZ != null) { Training_lblZ.Location = new Point(335, coordY + 2); Training_lblZ.AutoSize = true; }
                if (Training_tbxZ != null) { Training_tbxZ.Location = new Point(355, coordY); Training_tbxZ.Size = new Size(60, 24); }

                int radY = 126;
                if (Training_lblRadius != null) { Training_lblRadius.Location = new Point(16, radY + 2); Training_lblRadius.AutoSize = true; }
                if (Training_tbxRadius != null) { Training_tbxRadius.Location = new Point(70, radY); Training_tbxRadius.Size = new Size(60, 24); }

                int scriptY = 170;
                if (Training_lblScriptPath != null) { Training_lblScriptPath.Location = new Point(16, scriptY); Training_lblScriptPath.AutoSize = true; }
                if (Training_tbxScriptPath != null) { Training_tbxScriptPath.Location = new Point(16, scriptY + 26); Training_tbxScriptPath.Size = new Size(rightW - 74, 26); }
                if (Training_btnLoadScriptPath != null)
                {
                    Training_btnLoadScriptPath.Location = new Point(rightW - 52, scriptY + 25);
                    Training_btnLoadScriptPath.Size = new Size(36, 28);
                    Training_btnLoadScriptPath.Text = "...";
                    Training_btnLoadScriptPath.Font = DarkTheme.FontBodyBold;
                }

                Label lblAreaGuide = gbxAreaDetails.Controls["lblAreaGuide"] as Label;
                if (lblAreaGuide == null)
                {
                    lblAreaGuide = new Label
                    {
                        Name = "lblAreaGuide",
                        ForeColor = DarkTheme.TextMuted,
                        Font = DarkTheme.FontCaption,
                        Location = new Point(16, scriptY + 68),
                        Size = new Size(rightW - 32, 100),
                        Text = LocalizationManager.CurrentLanguage == "TR"
                            ? "Kasılma alanı tanımlamak için karakterinizi oyunda istediğiniz noktaya götürüp 'Get coordinates' butonuna basın. Yarıçap (Radius) karakterin alandan ne kadar uzaklaşacağını belirler. Yürüme rotası için script (.txt) dosyası seçebilirsiniz."
                            : "To define a training area, navigate your character to the desired spot in-game and click 'Get coordinates'. Radius defines the movement perimeter. You can optionally select a script (.txt) path for walk routes."
                    };
                    gbxAreaDetails.Controls.Add(lblAreaGuide);
                }
            }

            // --- Option 02: Script Panel ---
            if (TabPageH_Training_Option02_Panel != null)
            {
                if (Training_gbxRecord != null)
                {
                    Training_gbxRecord.Location = new Point(6, 6);
                    Training_gbxRecord.Size = new Size(col1W, 70);
                    if (Training_btnRecordStartStop != null) { Training_btnRecordStartStop.Location = new Point(16, 26); Training_btnRecordStartStop.Size = new Size(130, 30); }
                    if (Training_btnRecordPause != null) { Training_btnRecordPause.Location = new Point(155, 26); Training_btnRecordPause.Size = new Size(130, 30); }
                }

                if (Training_gbxOutput != null)
                {
                    Training_gbxOutput.Location = new Point(6, 82);
                    Training_gbxOutput.Size = new Size(col1W, tabH - 128);

                    if (Training_rtbxRecordOutput != null)
                    {
                        Training_rtbxRecordOutput.Location = new Point(12, 26);
                        Training_rtbxRecordOutput.Size = new Size(col1W - 24, Training_gbxOutput.Height - 38);
                        Training_rtbxRecordOutput.BackColor = DarkTheme.BgInput;
                        Training_rtbxRecordOutput.ForeColor = DarkTheme.TextPrimary;
                        Training_rtbxRecordOutput.BorderStyle = BorderStyle.None;
                    }
                }

                if (groupBox2 != null)
                {
                    groupBox2.Location = new Point(col2X, 6);
                    groupBox2.Size = new Size(col2W, tabH - 46);

                    Label lblScriptGuide = groupBox2.Controls["lblScriptGuide"] as Label;
                    if (lblScriptGuide == null)
                    {
                        lblScriptGuide = new Label
                        {
                            Name = "lblScriptGuide",
                            ForeColor = DarkTheme.TextSecondary,
                            Font = DarkTheme.FontBody,
                            Location = new Point(16, 32),
                            Size = new Size(col2W - 32, 260),
                            Text = LocalizationManager.CurrentLanguage == "TR"
                                ? "Script Kayıt ve Kullanım Kılavuzu:\n\n" +
                                  "1. Karakteriniz şehirdeyken 'START' butonuna basın.\n" +
                                  "2. Kasılma alanınıza doğru karakterinizi yürütün.\n" +
                                  "3. Yolda yapılan NPC etkileşimleri, pot alımları ve koordinatlar otomatik kaydedilir.\n" +
                                  "4. Kasılma alanına ulaştığınızda 'PAUSE' veya 'STOP' ile kaydı sonlandırın.\n" +
                                  "5. Kaydedilen dosyayı Kasılma -> Alan sekmesindeki 'Script Path' kısmından seçin.\n\n" +
                                  "Bot kasabaya döndüğünde bu rota üzerinden geri yürüyecektir."
                                : "Script Recording Guide:\n\n" +
                                  "1. Stand in town and click 'START'.\n" +
                                  "2. Walk your character toward the designated training spot.\n" +
                                  "3. NPC interactions, potion purchases, and walk coordinates are recorded.\n" +
                                  "4. Upon arriving at the training spot, click 'PAUSE' or 'STOP'.\n" +
                                  "5. Select the saved script file in Training -> Area -> 'Script Path'.\n\n" +
                                  "The bot will automatically re-walk this route upon returning to town."
                        };
                        groupBox2.Controls.Add(lblScriptGuide);
                    }
                }
            }

            // --- Option 04: Combat AI Panel ---
            if (pnlTrainingCombat != null)
            {
                if (btnTrainingCombat != null)
                {
                    btnTrainingCombat.Text = LocalizationManager.Get("UI_CombatAI_Tab", "Combat AI");
                }

                if (Combat_gbxAI != null)
                {
                    Combat_gbxAI.Location = new Point(6, 6);
                    Combat_gbxAI.Size = new Size(col1W, tabH - 46);

                    int cy = 30;
                    int rowH = 34;

                    // Section 1: Combat & Targeting Strategy
                    if (Combat_cbxMobPriority != null) { Combat_cbxMobPriority.Location = new Point(16, cy); Combat_cbxMobPriority.Size = new Size(col1W - 32, 24); } cy += rowH;
                    if (cbxCombatWeakerFirst != null) { cbxCombatWeakerFirst.Location = new Point(16, cy); cbxCombatWeakerFirst.Size = new Size(col1W - 32, 24); } cy += rowH;
                    if (cbxCombatDoNotFollow != null) { cbxCombatDoNotFollow.Location = new Point(16, cy); cbxCombatDoNotFollow.Size = new Size(col1W - 32, 24); } cy += rowH;
                    if (cbxCombatIgnorePillars != null) { cbxCombatIgnorePillars.Location = new Point(16, cy); cbxCombatIgnorePillars.Size = new Size(col1W - 32, 24); } cy += rowH + 6;

                    // Section 2: Survival & Kiting
                    if (Combat_cbxKiting != null) { Combat_cbxKiting.Location = new Point(16, cy); Combat_cbxKiting.Size = new Size(col1W - 32, 24); } cy += rowH;
                    if (Combat_cbxPanicEscape != null) { Combat_cbxPanicEscape.Location = new Point(16, cy); Combat_cbxPanicEscape.Size = new Size(col1W - 32, 24); } cy += rowH + 6;

                    // Section 3: Berserk Controls
                    if (Combat_cbxAutoBerserk != null) { Combat_cbxAutoBerserk.Location = new Point(16, cy); Combat_cbxAutoBerserk.Size = new Size(col1W - 32, 24); } cy += rowH;
                    if (cbxCombatZerkFullHP != null) { cbxCombatZerkFullHP.Location = new Point(16, cy); cbxCombatZerkFullHP.Size = new Size(col1W - 32, 24); } cy += rowH;
                    if (cbxCombatZerkRarity != null) { cbxCombatZerkRarity.Location = new Point(16, cy); cbxCombatZerkRarity.Size = new Size(col1W - 32, 24); } cy += rowH;
                    if (cbxCombatZerkAvoidance != null) { cbxCombatZerkAvoidance.Location = new Point(16, cy); cbxCombatZerkAvoidance.Size = new Size(col1W - 32, 24); } cy += rowH;

                    if (cbxCombatZerkCount != null) { cbxCombatZerkCount.Location = new Point(16, cy); cbxCombatZerkCount.Size = new Size(260, 24); }
                    if (nudCombatZerkCount != null) { nudCombatZerkCount.Location = new Point(285, cy); nudCombatZerkCount.Size = new Size(55, 24); }
                    cy += rowH + 8;

                    if (Combat_lblInfo != null) { Combat_lblInfo.Location = new Point(16, cy); Combat_lblInfo.Size = new Size(col1W - 32, 50); }
                }

                if (Combat_gbxMobFilter != null)
                {
                    Combat_gbxMobFilter.Location = new Point(col2X, 6);
                    Combat_gbxMobFilter.Size = new Size(col2W, tabH - 46);
                }
            }
        }

        private void ApplyModernTownLayout()
        {
            if (TabPageV_Control01_Town_Panel == null) return;
            int tabW = TabPageV_Control01_Town_Panel.Width;
            int tabH = TabPageV_Control01_Town_Panel.Height;
            int col1W = 595;
            int col2X = 605;
            int col2W = tabW - col2X - 8;

            // --- Option 01: Logistics & Auto-Buy ---
            if (TabPageH_Town_Option01_Panel != null)
            {
                if (Town_gbxLogistics != null)
                {
                    Town_gbxLogistics.Location = new Point(6, 6);
                    Town_gbxLogistics.Size = new Size(col1W, tabH - 46);

                    int ty = 32;
                    int rowH = 38;
                    if (Town_cbxEnableTownLoop != null) { Town_cbxEnableTownLoop.Location = new Point(16, ty); Town_cbxEnableTownLoop.Size = new Size(col1W - 32, 24); } ty += rowH;
                    if (Town_cbxRepair != null) { Town_cbxRepair.Location = new Point(16, ty); Town_cbxRepair.Size = new Size(col1W - 32, 24); } ty += rowH;
                    if (Town_cbxStorage != null) { Town_cbxStorage.Location = new Point(16, ty); Town_cbxStorage.Size = new Size(col1W - 32, 24); } ty += rowH;
                    if (Town_cbxSellTrash != null) { Town_cbxSellTrash.Location = new Point(16, ty); Town_cbxSellTrash.Size = new Size(col1W - 32, 24); } ty += rowH;
                    if (Town_cbxReturnNavMesh != null) { Town_cbxReturnNavMesh.Location = new Point(16, ty); Town_cbxReturnNavMesh.Size = new Size(col1W - 32, 24); } ty += rowH + 10;
                    if (Town_lblInfo != null) { Town_lblInfo.Location = new Point(16, ty); Town_lblInfo.Size = new Size(col1W - 32, 120); }
                }

                if (Town_gbxAutoBuy != null)
                {
                    Town_gbxAutoBuy.Location = new Point(col2X, 6);
                    Town_gbxAutoBuy.Size = new Size(col2W, tabH - 46);

                    int ay = 32;
                    int rowH = 46;
                    if (Town_cbxAutoBuy != null) { Town_cbxAutoBuy.Location = new Point(16, ay); Town_cbxAutoBuy.Size = new Size(col2W - 32, 24); } ay += rowH;

                    // HP Potions
                    if (Town_lblHpType != null) { Town_lblHpType.Location = new Point(16, ay + 4); Town_lblHpType.AutoSize = true; }
                    if (Town_cmbxHpType != null) { Town_cmbxHpType.Location = new Point(115, ay); Town_cmbxHpType.Size = new Size(140, 24); }
                    if (Town_lblHpAmount != null) { Town_lblHpAmount.Location = new Point(275, ay + 4); Town_lblHpAmount.AutoSize = true; }
                    if (Town_nudHpAmount != null) { Town_nudHpAmount.Location = new Point(375, ay); Town_nudHpAmount.Size = new Size(75, 24); }
                    ay += rowH;

                    // MP Potions
                    if (Town_lblMpType != null) { Town_lblMpType.Location = new Point(16, ay + 4); Town_lblMpType.AutoSize = true; }
                    if (Town_cmbxMpType != null) { Town_cmbxMpType.Location = new Point(115, ay); Town_cmbxMpType.Size = new Size(140, 24); }
                    if (Town_lblMpAmount != null) { Town_lblMpAmount.Location = new Point(275, ay + 4); Town_lblMpAmount.AutoSize = true; }
                    if (Town_nudMpAmount != null) { Town_nudMpAmount.Location = new Point(375, ay); Town_nudMpAmount.Size = new Size(75, 24); }
                    ay += rowH;

                    if (Town_cbxBuyPills != null) { Town_cbxBuyPills.Location = new Point(16, ay); Town_cbxBuyPills.Size = new Size(col2W - 32, 24); }
                }
            }

            // --- Option 03: Item Filter ---
            if (TabPageH_Town_Option03_Panel != null)
            {
                if (Filter_gbxPick != null)
                {
                    Filter_gbxPick.Location = new Point(6, 6);
                    Filter_gbxPick.Size = new Size(col1W, tabH - 46);
                }

                if (gbxItemFilterRules != null)
                {
                    gbxItemFilterRules.Location = new Point(col2X, 6);
                    gbxItemFilterRules.Size = new Size(col2W, 195);

                    // Degree controls
                    if (lblFilterDegree != null) { lblFilterDegree.Location = new Point(16, 32); lblFilterDegree.AutoSize = true; }
                    if (nudFilterMinDegree != null) { nudFilterMinDegree.Location = new Point(155, 30); nudFilterMinDegree.Size = new Size(50, 24); }
                    if (gbxItemFilterRules.Controls.OfType<Label>().FirstOrDefault(l => l.Text == "-") is Label dash) { dash.Location = new Point(212, 32); dash.AutoSize = true; }
                    if (nudFilterMaxDegree != null) { nudFilterMaxDegree.Location = new Point(228, 30); nudFilterMaxDegree.Size = new Size(50, 24); }
                    if (cbxFilterSox != null) { cbxFilterSox.Location = new Point(295, 30); cbxFilterSox.Size = new Size(col2W - 305, 24); }

                    // 2x2 grid for China/Europe/Male/Female
                    if (cbxFilterChina != null) { cbxFilterChina.Location = new Point(16, 64); cbxFilterChina.Size = new Size(220, 24); }
                    if (cbxFilterEurope != null) { cbxFilterEurope.Location = new Point(250, 64); cbxFilterEurope.Size = new Size(220, 24); }
                    if (cbxFilterMale != null) { cbxFilterMale.Location = new Point(16, 94); cbxFilterMale.Size = new Size(220, 24); }
                    if (cbxFilterFemale != null) { cbxFilterFemale.Location = new Point(250, 94); cbxFilterFemale.Size = new Size(220, 24); }

                    if (lblFilterInfo != null) { lblFilterInfo.Location = new Point(16, 126); lblFilterInfo.Size = new Size(col2W - 32, 58); }
                }

                if (gbxItemFilterRulesCustom != null)
                {
                    gbxItemFilterRulesCustom.Location = new Point(col2X, 208);
                    gbxItemFilterRulesCustom.Size = new Size(col2W, tabH - 248);

                    if (tbxItemRuleName != null) { tbxItemRuleName.Location = new Point(16, 28); tbxItemRuleName.Size = new Size(col2W - 200, 26); }
                    if (btnItemFilterSaveRule != null)
                    {
                        btnItemFilterSaveRule.Location = new Point(col2W - 176, 27);
                        btnItemFilterSaveRule.Size = new Size(88, 28);
                        btnItemFilterSaveRule.Text = LocalizationManager.CurrentLanguage == "TR" ? "Kural Ekle" : "Add Rule";
                    }
                    if (btnItemFilterRemoveRule != null)
                    {
                        btnItemFilterRemoveRule.Location = new Point(col2W - 84, 27);
                        btnItemFilterRemoveRule.Size = new Size(70, 28);
                        btnItemFilterRemoveRule.Text = LocalizationManager.CurrentLanguage == "TR" ? "Sil" : "Delete";
                    }

                    if (cbxItemRulePickup != null) { cbxItemRulePickup.Location = new Point(16, 62); cbxItemRulePickup.Size = new Size(100, 24); }
                    if (cbxItemRuleSell != null) { cbxItemRuleSell.Location = new Point(130, 62); cbxItemRuleSell.Size = new Size(100, 24); }
                    if (cbxItemRuleStore != null) { cbxItemRuleStore.Location = new Point(244, 62); cbxItemRuleStore.Size = new Size(100, 24); }

                    if (lstvItemRules != null)
                    {
                        lstvItemRules.Location = new Point(16, 92);
                        lstvItemRules.Size = new Size(col2W - 32, gbxItemFilterRulesCustom.Height - 104);
                        if (lstvItemRules.Columns.Count >= 2)
                        {
                            lstvItemRules.Columns[0].Width = lstvItemRules.Width - 90;
                            lstvItemRules.Columns[1].Width = 70;
                        }
                        AutoFitListView(lstvItemRules);
                    }
                }
            }
        }

        private void ApplyModernInventoryLayout()
        {
            if (TabPageV_Control01_Inventory_Panel == null) return;
            int tabW = TabPageV_Control01_Inventory_Panel.Width;
            int tabH = TabPageV_Control01_Inventory_Panel.Height;

            Action<Panel, ListView, Button, Button> setupInvSubTab = (panel, lv, btnRefresh, btnSort) =>
            {
                if (panel == null) return;
                int listH = tabH - 85;

                if (lv != null)
                {
                    lv.Location = new Point(6, 6);
                    lv.Size = new Size(tabW - 12, listH);
                    AutoFitListView(lv);
                }

                int btnY = listH + 10;
                if (btnRefresh != null)
                {
                    btnRefresh.Location = new Point(6, btnY);
                    btnRefresh.Size = new Size(95, 28);
                    btnRefresh.Text = LocalizationManager.CurrentLanguage == "TR" ? "↻ Yenile" : "↻ Refresh";
                    btnRefresh.Font = DarkTheme.FontBodyBold;
                }
                if (btnSort != null)
                {
                    btnSort.Location = new Point(108, btnY);
                    btnSort.Size = new Size(95, 28);
                    btnSort.Text = LocalizationManager.CurrentLanguage == "TR" ? "⇅ Sırala" : "⇅ Sort";
                    btnSort.Font = DarkTheme.FontBodyBold;
                }
            };

            setupInvSubTab(TabPageH_Inventory_Option01_Panel, Inventory_lstvItems, Inventory_btnItemsRefresh, Inventory_btnItemsSort);
            setupInvSubTab(TabPageH_Inventory_Option02_Panel, Inventory_lstvStorageItems, Inventory_btnStorageRefresh, Inventory_btnStorageSort);
            setupInvSubTab(TabPageH_Inventory_Option03_Panel, Inventory_lstvPet, Inventory_btnPetRefresh, null);
            setupInvSubTab(TabPageH_Inventory_Option04_Panel, Inventory_lstvAvatarItems, Inventory_btnAvatarItemsRefresh, null);
        }

        private void ApplyModernPartyLayout()
        {
            if (TabPageV_Control01_Party_Panel == null) return;
            int tabW = TabPageV_Control01_Party_Panel.Width;
            int tabH = TabPageV_Control01_Party_Panel.Height;
            int col1W = 595;
            int col2X = 605;
            int col2W = tabW - col2X - 8;

            // Option 01: Members
            if (TabPageH_Party_Option01_Panel != null)
            {
                if (Party_lblCurrentSetup != null) { Party_lblCurrentSetup.Location = new Point(6, 8); Party_lblCurrentSetup.AutoSize = true; }
                if (Party_cbxShowFGWInvites != null) { Party_cbxShowFGWInvites.Location = new Point(tabW - 250, 6); Party_cbxShowFGWInvites.Size = new Size(240, 24); }
                if (Party_lstvPartyMembers != null)
                {
                    Party_lstvPartyMembers.Location = new Point(6, 36);
                    Party_lstvPartyMembers.Size = new Size(tabW - 12, tabH - 78);
                    AutoFitListView(Party_lstvPartyMembers);
                }
            }

            // Option 02: Settings
            if (TabPageH_Party_Option02_Panel != null)
            {
                if (Party_gbxSetup != null)
                {
                    Party_gbxSetup.Location = new Point(6, 6);
                    Party_gbxSetup.Size = new Size(col1W, 175);
                }
                if (Party_gbxAcceptInvite != null)
                {
                    Party_gbxAcceptInvite.Location = new Point(6, 188);
                    Party_gbxAcceptInvite.Size = new Size(col1W, tabH - 230);
                }
                int halfH = (tabH - 52) / 2;
                if (Party_gbxLeaderList != null)
                {
                    Party_gbxLeaderList.Location = new Point(col2X, 6);
                    Party_gbxLeaderList.Size = new Size(col2W, halfH);
                }
                if (Party_gbxPlayerList != null)
                {
                    Party_gbxPlayerList.Location = new Point(col2X, halfH + 14);
                    Party_gbxPlayerList.Size = new Size(col2W, tabH - halfH - 52);
                }
            }

            // Option 03: Match
            if (TabPageH_Party_Option03_Panel != null)
            {
                if (Party_pnlAutoFormMatch != null)
                {
                    Party_pnlAutoFormMatch.Location = new Point(6, 6);
                    Party_pnlAutoFormMatch.Size = new Size(tabW - 12, 60);
                    Party_pnlAutoFormMatch.BackColor = DarkTheme.BgCard;
                }

                if (Party_lstvPartyMatch != null)
                {
                    Party_lstvPartyMatch.Location = new Point(6, 72);
                    Party_lstvPartyMatch.Size = new Size(tabW - 12, tabH - 120);
                    AutoFitListView(Party_lstvPartyMatch);
                }

                int bY = tabH - 40;
                if (Party_btnLastPage != null)
                {
                    Party_btnLastPage.Location = new Point(6, bY);
                    Party_btnLastPage.Size = new Size(36, 28);
                    Party_btnLastPage.Text = "◀";
                    Party_btnLastPage.Font = DarkTheme.FontBodyBold;
                }
                if (Party_lblPageNumber != null)
                {
                    Party_lblPageNumber.Location = new Point(48, bY + 4);
                    Party_lblPageNumber.AutoSize = true;
                }
                if (Party_btnNextPage != null)
                {
                    Party_btnNextPage.Location = new Point(95, bY);
                    Party_btnNextPage.Size = new Size(36, 28);
                    Party_btnNextPage.Text = "▶";
                    Party_btnNextPage.Font = DarkTheme.FontBodyBold;
                }
                if (Party_btnRefreshMatch != null)
                {
                    Party_btnRefreshMatch.Location = new Point(140, bY);
                    Party_btnRefreshMatch.Size = new Size(85, 28);
                    Party_btnRefreshMatch.Text = LocalizationManager.CurrentLanguage == "TR" ? "↻ Yenile" : "↻ Refresh";
                    Party_btnRefreshMatch.Font = DarkTheme.FontBodyBold;
                }
                if (Party_lblJoinToNumber != null)
                {
                    Party_lblJoinToNumber.Location = new Point(245, bY + 4);
                    Party_lblJoinToNumber.AutoSize = true;
                }
                if (Party_tbxJoinToNumber != null)
                {
                    Party_tbxJoinToNumber.Location = new Point(350, bY);
                    Party_tbxJoinToNumber.Size = new Size(70, 24);
                }
                if (Party_btnJoinMatch != null)
                {
                    Party_btnJoinMatch.Location = new Point(430, bY);
                    Party_btnJoinMatch.Size = new Size(90, 28);
                }
            }
        }

        private void ApplyModernGuildLayout()
        {
            if (TabPageV_Control01_Guild_Panel == null) return;
            int tabW = TabPageV_Control01_Guild_Panel.Width;
            int tabH = TabPageV_Control01_Guild_Panel.Height;

            if (TabPageH_Guild_Option01_Panel != null)
            {
                int optH = TabPageH_Guild_Option01_Panel.Height;
                int optW = TabPageH_Guild_Option01_Panel.Width;

                if (Guild_lblName != null) { Guild_lblName.Location = new Point(8, 8); Guild_lblName.AutoSize = true; Guild_lblName.Font = DarkTheme.FontHeader; Guild_lblName.ForeColor = DarkTheme.Accent; }
                if (Guild_lblLevel != null) { Guild_lblLevel.Location = new Point(180, 8); Guild_lblLevel.AutoSize = true; Guild_lblLevel.Font = DarkTheme.FontBody; Guild_lblLevel.ForeColor = DarkTheme.TextSecondary; }
                if (Guild_lblNotice != null) { Guild_lblNotice.Location = new Point(320, 8); Guild_lblNotice.AutoSize = true; Guild_lblNotice.Font = DarkTheme.FontBody; Guild_lblNotice.ForeColor = DarkTheme.TextMuted; }

                if (Guild_lstvInfo != null)
                {
                    Guild_lstvInfo.Location = new Point(4, 34);
                    Guild_lstvInfo.Size = new Size(optW - 8, optH - 74);
                    if (Guild_lstvInfo.Columns.Count >= 4)
                    {
                        int usable = optW - 24;
                        Guild_lstvInfo.Columns[0].Width = (int)(usable * 0.35);
                        Guild_lstvInfo.Columns[1].Width = (int)(usable * 0.15);
                        Guild_lstvInfo.Columns[2].Width = (int)(usable * 0.25);
                        Guild_lstvInfo.Columns[3].Width = (int)(usable * 0.25);
                    }
                }

                if (Guild_btnInfoRefresh != null)
                {
                    Guild_btnInfoRefresh.Location = new Point(optW - 98, optH - 34);
                    Guild_btnInfoRefresh.Size = new Size(90, 28);
                    Guild_btnInfoRefresh.Text = LocalizationManager.CurrentLanguage == "TR" ? "↻ Yenile" : "↻ Refresh";
                    Guild_btnInfoRefresh.Font = DarkTheme.FontBodyBold;
                }
            }

            if (TabPageH_Guild_Option02_Panel != null)
            {
                int optH = TabPageH_Guild_Option02_Panel.Height;
                int optW = TabPageH_Guild_Option02_Panel.Width;

                if (Guild_lstvStorage != null)
                {
                    Guild_lstvStorage.Location = new Point(4, 4);
                    Guild_lstvStorage.Size = new Size(optW - 8, optH - 44);
                    if (Guild_lstvStorage.Columns.Count >= 4)
                    {
                        int usable = optW - 24;
                        Guild_lstvStorage.Columns[0].Width = (int)(usable * 0.15);
                        Guild_lstvStorage.Columns[1].Width = (int)(usable * 0.45);
                        Guild_lstvStorage.Columns[2].Width = (int)(usable * 0.20);
                        Guild_lstvStorage.Columns[3].Width = (int)(usable * 0.20);
                    }
                }

                if (Guild_lblStorageCapacity != null)
                {
                    Guild_lblStorageCapacity.Location = new Point(8, optH - 32);
                    Guild_lblStorageCapacity.AutoSize = true;
                }

                if (Guild_btnStorageRefresh != null)
                {
                    Guild_btnStorageRefresh.Location = new Point(optW - 98, optH - 34);
                    Guild_btnStorageRefresh.Size = new Size(90, 28);
                    Guild_btnStorageRefresh.Text = LocalizationManager.CurrentLanguage == "TR" ? "↻ Yenile" : "↻ Refresh";
                    Guild_btnStorageRefresh.Font = DarkTheme.FontBodyBold;
                }
            }
        }

        private void ApplyModernPlayersLayout()
        {
            if (TabPageV_Control01_Players_Panel == null) return;

            if (TabPageH_Players_Option01_Panel != null)
            {
                int optH = TabPageH_Players_Option01_Panel.Height;
                int optW = TabPageH_Players_Option01_Panel.Width;

                if (Players_tvwPlayers != null)
                {
                    Players_tvwPlayers.Location = new Point(4, 4);
                    Players_tvwPlayers.Size = new Size(optW - 8, optH - 44);
                    Players_tvwPlayers.BackColor = DarkTheme.BgCard;
                    Players_tvwPlayers.ForeColor = DarkTheme.TextPrimary;
                    Players_tvwPlayers.BorderStyle = BorderStyle.None;
                }

                if (Players_lblPlayerCount != null)
                {
                    Players_lblPlayerCount.Location = new Point(8, optH - 32);
                    Players_lblPlayerCount.AutoSize = true;
                }

                if (Players_btnRefreshPlayers != null)
                {
                    Players_btnRefreshPlayers.Location = new Point(optW - 98, optH - 34);
                    Players_btnRefreshPlayers.Size = new Size(90, 28);
                    Players_btnRefreshPlayers.Text = LocalizationManager.CurrentLanguage == "TR" ? "↻ Yenile" : "↻ Refresh";
                    Players_btnRefreshPlayers.Font = DarkTheme.FontBodyBold;
                }
            }
        }

        private void ApplyModernChatLayout()
        {
            if (TabPageV_Control01_Chat_Panel == null) return;

            Panel[] chatPanels = new Panel[]
            {
                TabPageH_Chat_Option01_Panel, TabPageH_Chat_Option02_Panel, TabPageH_Chat_Option03_Panel,
                TabPageH_Chat_Option04_Panel, TabPageH_Chat_Option05_Panel, TabPageH_Chat_Option06_Panel,
                TabPageH_Chat_Option07_Panel, TabPageH_Chat_Option08_Panel
            };

            foreach (var pnl in chatPanels)
            {
                if (pnl == null) continue;
                int optW = pnl.Width;
                int optH = pnl.Height;

                foreach (Control c in pnl.Controls)
                {
                    if (c is RichTextBox || c is TextBox)
                    {
                        c.Location = new Point(4, 4);
                        c.Size = new Size(optW - 8, optH - 8);
                        c.BackColor = DarkTheme.BgCard;
                        c.ForeColor = DarkTheme.TextPrimary;
                        c.Font = DarkTheme.FontBody;
                    }
                }
            }
        }

        private void ApplyModernStallLayout()
        {
            if (TabPageV_Control01_Stall_Panel == null) return;

            if (TabPageH_Stall_Option01_Panel != null)
            {
                int optH = TabPageH_Stall_Option01_Panel.Height;
                int optW = TabPageH_Stall_Option01_Panel.Width;
                int leftW = 290;
                int rightX = leftW + 12;
                int rightW = optW - rightX - 6;

                if (Stall_lblInventoryStall != null)
                {
                    Stall_lblInventoryStall.Location = new Point(6, 6);
                    Stall_lblInventoryStall.Size = new Size(leftW, 26);
                    Stall_lblInventoryStall.TextAlign = ContentAlignment.MiddleLeft;
                    Stall_lblInventoryStall.ForeColor = DarkTheme.TextMuted;
                    Stall_lblInventoryStall.BorderStyle = BorderStyle.None;
                }

                if (Stall_lstvInventoryStall != null)
                {
                    Stall_lstvInventoryStall.Location = new Point(6, 36);
                    Stall_lstvInventoryStall.Size = new Size(leftW, optH - 80);
                    if (Stall_lstvInventoryStall.Columns.Count >= 1)
                    {
                        Stall_lstvInventoryStall.Columns[0].Width = leftW - 12;
                    }
                }

                if (Stall_lstvStall != null)
                {
                    Stall_lstvStall.Location = new Point(rightX, 36);
                    Stall_lstvStall.Size = new Size(rightW, optH - 80);
                    if (Stall_lstvStall.Columns.Count >= 3)
                    {
                        int usable = rightW - 20;
                        Stall_lstvStall.Columns[0].Width = (int)(usable * 0.50);
                        Stall_lstvStall.Columns[1].Width = (int)(usable * 0.25);
                        Stall_lstvStall.Columns[2].Width = (int)(usable * 0.25);
                    }
                }

                int bY = optH - 36;
                if (Stall_tbxPrice != null) { Stall_tbxPrice.Location = new Point(6, bY); Stall_tbxPrice.Size = new Size(95, 24); }
                if (Stall_tbxQuantity != null) { Stall_tbxQuantity.Location = new Point(106, bY); Stall_tbxQuantity.Size = new Size(60, 24); }
                if (Stall_btnAddItem != null)
                {
                    Stall_btnAddItem.Location = new Point(172, bY - 2);
                    Stall_btnAddItem.Size = new Size(124, 28);
                    Stall_btnAddItem.Text = LocalizationManager.CurrentLanguage == "TR" ? "+ Tezgaha Koy" : "+ Add to Stall";
                    Stall_btnAddItem.Font = DarkTheme.FontBodyBold;
                }

                if (Stall_btnIGCreateModify != null)
                {
                    Stall_btnIGCreateModify.Location = new Point(rightX, bY - 2);
                    Stall_btnIGCreateModify.Size = new Size(130, 28);
                    Stall_btnIGCreateModify.Font = DarkTheme.FontBodyBold;
                }
                if (Stall_lblState != null)
                {
                    Stall_lblState.Location = new Point(rightX + 138, bY);
                    Stall_lblState.Size = new Size(rightW - 176, 24);
                    Stall_lblState.TextAlign = ContentAlignment.MiddleLeft;
                }
                if (Stall_btnClose != null)
                {
                    Stall_btnClose.Location = new Point(optW - 36, bY - 2);
                    Stall_btnClose.Size = new Size(28, 28);
                    Stall_btnClose.Text = "✕";
                    Stall_btnClose.Font = DarkTheme.FontBodyBold;
                }
            }

            if (TabPageH_Stall_Option02_Panel != null)
            {
                if (Stall_lblStallTitle != null) { Stall_lblStallTitle.Location = new Point(16, 16); Stall_lblStallTitle.AutoSize = true; }
                if (Stall_tbxStallTitle != null) { Stall_tbxStallTitle.Location = new Point(16, 42); Stall_tbxStallTitle.Size = new Size(400, 24); }
                if (Stall_lblStallNote != null) { Stall_lblStallNote.Location = new Point(16, 82); Stall_lblStallNote.AutoSize = true; }
                if (Stall_tbxStallNote != null) { Stall_tbxStallNote.Location = new Point(16, 108); Stall_tbxStallNote.Size = new Size(400, 24); }
            }
        }

        private void ApplyModernGameInfoLayout()
        {
            if (TabPageV_Control01_GameInfo_Panel == null) return;
            int tabW = TabPageV_Control01_GameInfo_Panel.Width;
            int tabH = TabPageV_Control01_GameInfo_Panel.Height;

            if (GameInfo_tvwObjects != null)
            {
                GameInfo_tvwObjects.Location = new Point(4, 4);
                GameInfo_tvwObjects.Size = new Size(tabW - 8, tabH - 46);
                GameInfo_tvwObjects.BackColor = DarkTheme.BgCard;
                GameInfo_tvwObjects.ForeColor = DarkTheme.TextPrimary;
                GameInfo_tvwObjects.BorderStyle = BorderStyle.None;
            }

            int bY = tabH - 34;
            if (GameInfo_lblServerTime != null) { GameInfo_lblServerTime.Location = new Point(8, bY + 4); GameInfo_lblServerTime.AutoSize = true; }
            if (GameInfo_tbxServerTime != null) { GameInfo_tbxServerTime.Location = new Point(95, bY + 1); GameInfo_tbxServerTime.Size = new Size(150, 24); }

            int cbX = 256;
            int cbGap = 8;
            Control[] infoChecks = new Control[] { GameInfo_cbxPlayer, GameInfo_cbxPet, GameInfo_cbxMob, GameInfo_cbxNPC, GameInfo_cbxDrop, GameInfo_cbxOthers };
            int[] checkWidths = new int[] { 72, 62, 68, 68, 72, 78 };

            for (int i = 0; i < infoChecks.Length; i++)
            {
                var cb = infoChecks[i];
                if (cb != null)
                {
                    cb.Location = new Point(cbX, bY);
                    cb.AutoSize = false;
                    cb.Size = new Size(checkWidths[i], 26);
                    cbX += checkWidths[i] + cbGap;
                }
            }

            if (GameInfo_btnRefresh != null)
            {
                GameInfo_btnRefresh.Location = new Point(tabW - 98, bY);
                GameInfo_btnRefresh.Size = new Size(90, 28);
                GameInfo_btnRefresh.Text = LocalizationManager.CurrentLanguage == "TR" ? "↻ Yenile" : "↻ Refresh";
                GameInfo_btnRefresh.Font = DarkTheme.FontBodyBold;
            }
        }

        private void ApplyModernMinimapLayout()
        {
            if (TabPageV_Control01_Minimap_Panel == null) return;
            int tabW = TabPageV_Control01_Minimap_Panel.Width;
            int tabH = TabPageV_Control01_Minimap_Panel.Height;

            if (Minimap_pnlMap != null)
            {
                Minimap_pnlMap.Location = new Point(0, 0);
                Minimap_pnlMap.Size = new Size(tabW, tabH);
            }

            if (Minimap_panelCoords != null)
            {
                Minimap_panelCoords.Location = new Point(tabW - 320, 6);
                Minimap_panelCoords.BringToFront();
            }

            if (Minimap_tbrZoom != null)
            {
                Minimap_tbrZoom.Location = new Point(tabW - 36, 45);
                Minimap_tbrZoom.Size = new Size(30, 160);
                Minimap_tbrZoom.BringToFront();
            }
        }

        private void ApplyModernAcademyLayout()
        {
            if (TabPageV_Control01_Academy_Panel == null) return;
            int tabW = TabPageV_Control01_Academy_Panel.Width;
            int tabH = TabPageV_Control01_Academy_Panel.Height;

            GroupBox gbxAcademy = TabPageV_Control01_Academy_Panel.Controls["gbxAcademyInfo"] as GroupBox;
            if (gbxAcademy == null)
            {
                gbxAcademy = new GroupBox
                {
                    Name = "gbxAcademyInfo",
                    Text = LocalizationManager.CurrentLanguage == "TR" ? "Akademi Durumu ve Bilgisi" : "Academy Status & Information",
                    Location = new Point(6, 6),
                    Size = new Size(tabW - 12, tabH - 12)
                };

                Label lblAcademyGuide = new Label
                {
                    Name = "lblAcademyGuide",
                    Text = LocalizationManager.CurrentLanguage == "TR"
                        ? "★ Silkroad Akademi Sistemi:\n\n" +
                          "• Kurulu olan bir Akademiye üye olduğunuzda seviye atladıkça mezuniyet puanı kazanırsınız.\n" +
                          "• Karakteriniz 40. seviyeye ulaştığında otomatik mezuniyet şartları sağlanır.\n" +
                          "• Akademi buffları ve mezuniyet durumları bot arka planında otomatik takip edilmektedir.\n" +
                          "• Kurucu (Guardian) olduğunuzda asistan ve çırak davetleri topluluk sekmesinden yönetilir."
                        : "★ Silkroad Academy System:\n\n" +
                          "• When joined to an Academy, graduation points are gained as your character levels up.\n" +
                          "• Upon reaching level 40, graduation conditions are fulfilled automatically.\n" +
                          "• Academy buffs and graduation statuses are tracked automatically in the background.\n" +
                          "• When acting as Guardian, apprentice invitations can be managed via the community panel.",
                    Location = new Point(20, 36),
                    Size = new Size(tabW - 52, 200),
                    ForeColor = DarkTheme.TextSecondary,
                    Font = DarkTheme.FontBody
                };

                gbxAcademy.Controls.Add(lblAcademyGuide);
                TabPageV_Control01_Academy_Panel.Controls.Add(gbxAcademy);
                SkinControlHierarchy(gbxAcademy);
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
