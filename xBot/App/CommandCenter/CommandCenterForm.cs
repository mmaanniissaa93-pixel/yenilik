using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using xBot.App.Theme;

namespace xBot.App.CommandCenter
{
    public class CommandCenterForm : Form
    {
        private Button btnTabEmotes;
        private Button btnTabChat;
        private Label lblHeaderTitle;
        private Panel pnlTabs;
        private Panel pnlCard;
        private Panel pnlEmotes;
        private Panel pnlChat;
        private Panel pnlBottom;
        private Button btnReset;
        private CheckBox chkEnable;
        private Button btnSave;

        private readonly Dictionary<string, ComboBox> _emoteCombos = new Dictionary<string, ComboBox>(StringComparer.OrdinalIgnoreCase);

        public CommandCenterForm()
        {
            InitializeComponent();
            LoadConfigValues();
            SwitchTab(true);
        }

        private void InitializeComponent()
        {
            this.Text = "Command Center";
            this.Size = new Size(740, 680);
            this.MinimumSize = new Size(680, 560);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = DarkTheme.BgDark;
            this.ForeColor = DarkTheme.TextPrimary;
            this.Font = DarkTheme.GetFont(9f);
            this.DoubleBuffered = true;
            this.ShowIcon = false;

            // 1. Top Tabs Panel
            pnlTabs = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                Padding = new Padding(16, 12, 16, 4),
                BackColor = DarkTheme.BgDark
            };

            btnTabEmotes = CreatePillButton("Emote commands", 0);
            btnTabChat = CreatePillButton("Chat commands", 150);

            btnTabEmotes.Click += (s, e) => SwitchTab(true);
            btnTabChat.Click += (s, e) => SwitchTab(false);

            pnlTabs.Controls.Add(btnTabEmotes);
            pnlTabs.Controls.Add(btnTabChat);

            // 2. Bottom Actions Panel
            pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                Padding = new Padding(16, 10, 16, 10),
                BackColor = DarkTheme.BgSidebar
            };
            pnlBottom.Paint += (s, e) =>
            {
                using (Pen p = new Pen(DarkTheme.BorderSubtle, 1))
                {
                    e.Graphics.DrawLine(p, 0, 0, pnlBottom.Width, 0);
                }
            };

            btnReset = new Button
            {
                Text = "Reset to defaults",
                Size = new Size(150, 36),
                Location = new Point(16, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = DarkTheme.BgInput,
                ForeColor = DarkTheme.TextSecondary,
                Font = DarkTheme.GetFont(9f, FontStyle.Regular),
                Cursor = Cursors.Hand
            };
            btnReset.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
            btnReset.FlatAppearance.BorderSize = 1;
            btnReset.Click += BtnReset_Click;

            btnSave = new Button
            {
                Text = "Save",
                Size = new Size(100, 36),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlBottom.Width - 116, 12),
                FlatStyle = FlatStyle.Flat,
                BackColor = DarkTheme.Accent,
                ForeColor = Color.White,
                Font = DarkTheme.GetFont(9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;

            chkEnable = new CheckBox
            {
                Text = "Enable command center",
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Location = new Point(pnlBottom.Width - 300, 20),
                ForeColor = DarkTheme.TextPrimary,
                Font = DarkTheme.GetFont(9.5f, FontStyle.Regular),
                Checked = CommandCenterManager.Enabled,
                Cursor = Cursors.Hand
            };

            pnlBottom.Controls.Add(btnReset);
            pnlBottom.Controls.Add(chkEnable);
            pnlBottom.Controls.Add(btnSave);

            // 3. Center Card Container
            pnlCard = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 4, 16, 10),
                BackColor = DarkTheme.BgDark
            };

            Panel cardInner = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = DarkTheme.BgCard,
                Padding = new Padding(0)
            };
            cardInner.Paint += (s, e) =>
            {
                using (Pen p = new Pen(DarkTheme.BorderSubtle, 1))
                {
                    e.Graphics.DrawRectangle(p, 0, 0, cardInner.Width - 1, cardInner.Height - 1);
                }
            };

            // Card Header
            Panel cardHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = DarkTheme.BgCardHeader,
                Padding = new Padding(16, 0, 16, 0)
            };
            cardHeader.Paint += (s, e) =>
            {
                using (Pen p = new Pen(DarkTheme.BorderSubtle, 1))
                {
                    e.Graphics.DrawLine(p, 0, cardHeader.Height - 1, cardHeader.Width, cardHeader.Height - 1);
                }
            };

            lblHeaderTitle = new Label
            {
                Text = "Emote Commands",
                Font = DarkTheme.GetFont(11f, FontStyle.Bold),
                ForeColor = DarkTheme.TextPrimary,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = true
            };
            cardHeader.Controls.Add(lblHeaderTitle);

            // 4. Panel for Emotes
            pnlEmotes = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = DarkTheme.BgCard,
                Padding = new Padding(12, 10, 12, 10)
            };
            BuildEmoteRows();

            // 5. Panel for Chat Commands
            pnlChat = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = DarkTheme.BgCard,
                Padding = new Padding(12, 10, 12, 10),
                Visible = false
            };
            BuildChatRows();

            cardInner.Controls.Add(pnlEmotes);
            cardInner.Controls.Add(pnlChat);
            cardInner.Controls.Add(cardHeader);

            pnlCard.Controls.Add(cardInner);

            // Assemble Form
            this.Controls.Add(pnlCard);
            this.Controls.Add(pnlBottom);
            this.Controls.Add(pnlTabs);

            this.Resize += (s, e) =>
            {
                btnSave.Location = new Point(pnlBottom.Width - 116, 12);
                chkEnable.Location = new Point(pnlBottom.Width - 300, 20);
            };
        }

        private Button CreatePillButton(string text, int x)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(x, 8),
                Size = new Size(140, 34),
                FlatStyle = FlatStyle.Flat,
                Font = DarkTheme.GetFont(9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter
            };
            btn.FlatAppearance.BorderSize = 1;
            return btn;
        }

        private void SwitchTab(bool emotesTab)
        {
            if (emotesTab)
            {
                lblHeaderTitle.Text = "Emote Commands";
                pnlEmotes.Visible = true;
                pnlChat.Visible = false;

                btnTabEmotes.BackColor = Color.FromArgb(22, 44, 70);
                btnTabEmotes.ForeColor = Color.White;
                btnTabEmotes.FlatAppearance.BorderColor = DarkTheme.BorderFocus;

                btnTabChat.BackColor = DarkTheme.BgDark;
                btnTabChat.ForeColor = DarkTheme.TextMuted;
                btnTabChat.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
            }
            else
            {
                lblHeaderTitle.Text = "Chat Commands";
                pnlEmotes.Visible = false;
                pnlChat.Visible = true;

                btnTabChat.BackColor = Color.FromArgb(22, 44, 70);
                btnTabChat.ForeColor = Color.White;
                btnTabChat.FlatAppearance.BorderColor = DarkTheme.BorderFocus;

                btnTabEmotes.BackColor = DarkTheme.BgDark;
                btnTabEmotes.ForeColor = DarkTheme.TextMuted;
                btnTabEmotes.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
            }
        }

        private void BuildEmoteRows()
        {
            pnlEmotes.Controls.Clear();
            _emoteCombos.Clear();

            int y = 8;
            int initialWidth = Math.Max(660, pnlEmotes.ClientSize.Width > 50 ? pnlEmotes.ClientSize.Width - 16 : 660);

            foreach (var emote in CommandCenterManager.Emotes)
            {
                Panel row = new Panel
                {
                    Location = new Point(8, y),
                    Size = new Size(initialWidth, 54),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    BackColor = Color.Transparent
                };
                row.Paint += (s, e) =>
                {
                    using (Pen p = new Pen(Color.FromArgb(40, 73, 109, 149), 1))
                    {
                        e.Graphics.DrawLine(p, 0, row.Height - 1, row.Width, row.Height - 1);
                    }
                };

                // Icon Box
                Panel iconBox = new Panel
                {
                    Location = new Point(6, 9),
                    Size = new Size(36, 36),
                    BackColor = Color.FromArgb(15, 20, 28)
                };
                iconBox.Paint += (s, e) =>
                {
                    using (Pen p = new Pen(Color.FromArgb(80, 109, 143, 183), 1))
                    {
                        e.Graphics.DrawRectangle(p, 0, 0, iconBox.Width - 1, iconBox.Height - 1);
                    }
                };

                PictureBox pic = new PictureBox
                {
                    Size = new Size(32, 32),
                    Location = new Point(2, 2),
                    SizeMode = PictureBoxSizeMode.CenterImage,
                    Image = CommandCenterManager.GetEmoteIcon(emote.IconName)
                };
                iconBox.Controls.Add(pic);
                row.Controls.Add(iconBox);

                // Emote Name Label
                Label lblName = new Label
                {
                    Text = emote.Name,
                    Location = new Point(56, 17),
                    AutoSize = true,
                    Font = DarkTheme.GetFont(10.5f, FontStyle.Bold),
                    ForeColor = DarkTheme.TextPrimary
                };
                row.Controls.Add(lblName);

                // Action ComboBox
                int comboWidth = Math.Max(260, row.Width - 250);
                ComboBox cmb = new ComboBox
                {
                    Location = new Point(230, 12),
                    Size = new Size(comboWidth, 32),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    DrawMode = DrawMode.OwnerDrawFixed,
                    ItemHeight = 26,
                    BackColor = DarkTheme.BgInput,
                    ForeColor = DarkTheme.TextPrimary,
                    Font = DarkTheme.GetFont(9.5f, FontStyle.Regular)
                };

                foreach (var opt in CommandCenterManager.AvailableCommands)
                {
                    cmb.Items.Add(opt);
                }

                cmb.DrawItem += (s, e) =>
                {
                    if (e.Index < 0) return;
                    bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                    Color bg = selected ? DarkTheme.Accent : DarkTheme.BgInput;
                    Color fg = Color.White;

                    using (SolidBrush b = new SolidBrush(bg))
                        e.Graphics.FillRectangle(b, e.Bounds);

                    string text = cmb.Items[e.Index].ToString();
                    using (SolidBrush textBrush = new SolidBrush(fg))
                    {
                        StringFormat sf = new StringFormat
                        {
                            LineAlignment = StringAlignment.Center,
                            Alignment = StringAlignment.Near
                        };
                        Rectangle r = new Rectangle(e.Bounds.X + 8, e.Bounds.Y, e.Bounds.Width - 8, e.Bounds.Height);
                        e.Graphics.DrawString(text, cmb.Font, textBrush, r, sf);
                    }
                };

                _emoteCombos[emote.Name] = cmb;
                row.Controls.Add(cmb);

                pnlEmotes.Controls.Add(row);
                y += 56;
            }

            pnlEmotes.Resize += (s, e) =>
            {
                int rw = Math.Max(300, pnlEmotes.ClientSize.Width - 16);
                foreach (Control c in pnlEmotes.Controls)
                {
                    if (c is Panel r)
                    {
                        r.Width = rw;
                        foreach (Control rc in r.Controls)
                        {
                            if (rc is ComboBox cb)
                            {
                                cb.Width = Math.Max(200, r.Width - 250);
                            }
                        }
                    }
                }
            };
        }

        private void BuildChatRows()
        {
            pnlChat.Controls.Clear();

            // Instruction Banner
            Panel banner = new Panel
            {
                Location = new Point(8, 8),
                Width = pnlChat.ClientSize.Width - 16,
                Height = 36,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = DarkTheme.BgCardHeader
            };
            banner.Paint += (s, e) =>
            {
                using (Pen p = new Pen(DarkTheme.BorderSubtle, 1))
                {
                    e.Graphics.DrawLine(p, 0, banner.Height - 1, banner.Width, banner.Height - 1);
                }
            };

            Label lblBanner = new Label
            {
                Text = "Type the following commands into the game chat",
                Location = new Point(12, 10),
                AutoSize = true,
                Font = DarkTheme.GetFont(9f, FontStyle.Regular),
                ForeColor = DarkTheme.TextMuted
            };
            banner.Controls.Add(lblBanner);
            pnlChat.Controls.Add(banner);

            int y = 52;
            foreach (var cmd in CommandCenterManager.ChatCommands)
            {
                Panel row = new Panel
                {
                    Location = new Point(8, y),
                    Width = pnlChat.ClientSize.Width - 16,
                    Height = 44,
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                    BackColor = Color.Transparent
                };
                row.Paint += (s, e) =>
                {
                    using (Pen p = new Pen(Color.FromArgb(30, 73, 109, 149), 1))
                    {
                        e.Graphics.DrawLine(p, 0, row.Height - 1, row.Width, row.Height - 1);
                    }
                };

                Label lblTrigger = new Label
                {
                    Text = cmd.Trigger,
                    Location = new Point(16, 12),
                    Width = 120,
                    Font = DarkTheme.GetFont(10.5f, FontStyle.Bold),
                    ForeColor = DarkTheme.TextPrimary
                };

                Label lblDesc = new Label
                {
                    Text = cmd.Description,
                    Location = new Point(160, 13),
                    AutoSize = true,
                    Font = DarkTheme.GetFont(9.5f, FontStyle.Regular),
                    ForeColor = DarkTheme.TextSecondary
                };

                row.Controls.Add(lblTrigger);
                row.Controls.Add(lblDesc);
                pnlChat.Controls.Add(row);

                y += 46;
            }
        }

        private void LoadConfigValues()
        {
            chkEnable.Checked = CommandCenterManager.Enabled;

            foreach (var emote in CommandCenterManager.Emotes)
            {
                if (_emoteCombos.TryGetValue(emote.Name, out var cmb))
                {
                    string assignedKey = CommandCenterManager.GetAssignedCommand(emote.Name);
                    int idx = 0;
                    for (int i = 0; i < cmb.Items.Count; i++)
                    {
                        if (cmb.Items[i] is CommandOption opt && string.Equals(opt.Key, assignedKey, StringComparison.OrdinalIgnoreCase))
                        {
                            idx = i;
                            break;
                        }
                    }
                    if (cmb.Items.Count > 0)
                        cmb.SelectedIndex = idx;
                }
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            CommandCenterManager.ResetToDefaults();
            LoadConfigValues();
            Window.Get?.Log("[Command Center] Mappings reset to defaults.");
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            CommandCenterManager.Enabled = chkEnable.Checked;

            foreach (var emote in CommandCenterManager.Emotes)
            {
                if (_emoteCombos.TryGetValue(emote.Name, out var cmb))
                {
                    if (cmb.SelectedItem is CommandOption opt)
                    {
                        CommandCenterManager.SetAssignedCommand(emote.Name, opt.Key);
                    }
                }
            }

            Settings.SaveBotSettings();
            Window.Get?.Log("[Command Center] Settings saved successfully.");
            this.Close();
        }
    }
}
