using System;
using System.Drawing;
using System.Windows.Forms;
using xBot.App.Theme;

namespace xBot.App
{
    /// <summary>
    /// Dialog for managing saved accounts and character login credentials,
    /// including secondary passcode (PIN) input.
    /// </summary>
    public class AccountSetupForm : Form
    {
        private Label lblTitle;
        private Label lblSubtitle;
        private ModernCard pnlLeftCard;
        private ModernCard pnlRightCard;

        // Left Card Controls
        private Label lblUsername;
        private TextBox txtUsername;
        private Label lblPassword;
        private TextBox txtPassword;
        private Label lblSecondary;
        private TextBox txtSecondary;
        private Label lblServer;
        private TextBox txtServer;
        private Button btnAddUpdate;
        private Label lblCount;

        // Right Card Controls
        private ListView lstvAccounts;
        private Button btnRemoveSelected;

        // Bottom Controls
        private Label lblStatus;
        private Button btnOK;

        public AccountSetupForm()
        {
            InitializeComponent();
            LoadAccounts();
        }

        private void InitializeComponent()
        {
            this.Text = "Account Setup";
            this.ClientSize = new Size(760, 460);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = DarkTheme.BgDark;
            this.ForeColor = DarkTheme.TextPrimary;
            this.Font = DarkTheme.FontBody;

            // Title and Subtitle
            lblTitle = new Label
            {
                Text = "Account Setup",
                Location = new Point(24, 16),
                AutoSize = true,
                Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Pixel),
                ForeColor = DarkTheme.TextPrimary
            };

            lblSubtitle = new Label
            {
                Text = "Manage saved accounts and character login credentials.",
                Location = new Point(25, 42),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Pixel),
                ForeColor = DarkTheme.TextMuted
            };

            // Left Card
            pnlLeftCard = new ModernCard
            {
                Location = new Point(24, 72),
                Size = new Size(340, 320),
                CardColor = DarkTheme.BgCard,
                BorderColor = DarkTheme.BorderSubtle,
                BorderRadius = 8
            };

            lblUsername = new Label
            {
                Text = "Username",
                Location = new Point(18, 16),
                AutoSize = true,
                Font = DarkTheme.FontCaption,
                ForeColor = DarkTheme.TextMuted
            };

            txtUsername = new TextBox
            {
                Location = new Point(18, 36),
                Size = new Size(304, 26),
                BackColor = DarkTheme.BgInput,
                ForeColor = DarkTheme.TextPrimary,
                Font = DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };

            lblPassword = new Label
            {
                Text = "Password",
                Location = new Point(18, 74),
                AutoSize = true,
                Font = DarkTheme.FontCaption,
                ForeColor = DarkTheme.TextMuted
            };

            txtPassword = new TextBox
            {
                Location = new Point(18, 94),
                Size = new Size(146, 26),
                BackColor = DarkTheme.BgInput,
                ForeColor = DarkTheme.TextPrimary,
                Font = DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle,
                UseSystemPasswordChar = true
            };

            lblSecondary = new Label
            {
                Text = "Secondary",
                Location = new Point(176, 74),
                AutoSize = true,
                Font = DarkTheme.FontCaption,
                ForeColor = DarkTheme.TextMuted
            };

            txtSecondary = new TextBox
            {
                Location = new Point(176, 94),
                Size = new Size(146, 26),
                BackColor = DarkTheme.BgInput,
                ForeColor = DarkTheme.TextPrimary,
                Font = DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 8
            };

            lblServer = new Label
            {
                Text = "Server name",
                Location = new Point(18, 134),
                AutoSize = true,
                Font = DarkTheme.FontCaption,
                ForeColor = DarkTheme.TextMuted
            };

            txtServer = new TextBox
            {
                Location = new Point(18, 154),
                Size = new Size(304, 26),
                BackColor = DarkTheme.BgInput,
                ForeColor = DarkTheme.TextPrimary,
                Font = DarkTheme.FontBody,
                BorderStyle = BorderStyle.FixedSingle
            };

            btnAddUpdate = new Button
            {
                Text = "Add / Update",
                Location = new Point(18, 198),
                Size = new Size(120, 32),
                BackColor = DarkTheme.Accent,
                ForeColor = Color.White,
                Font = DarkTheme.FontBodyBold,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnAddUpdate.FlatAppearance.BorderSize = 0;
            btnAddUpdate.Click += (s, e) => OnAddUpdateClicked();

            lblCount = new Label
            {
                Text = "0 account(s) loaded.",
                Location = new Point(18, 246),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Italic, GraphicsUnit.Pixel),
                ForeColor = DarkTheme.TextFaint
            };

            pnlLeftCard.Controls.Add(lblUsername);
            pnlLeftCard.Controls.Add(txtUsername);
            pnlLeftCard.Controls.Add(lblPassword);
            pnlLeftCard.Controls.Add(txtPassword);
            pnlLeftCard.Controls.Add(lblSecondary);
            pnlLeftCard.Controls.Add(txtSecondary);
            pnlLeftCard.Controls.Add(lblServer);
            pnlLeftCard.Controls.Add(txtServer);
            pnlLeftCard.Controls.Add(btnAddUpdate);
            pnlLeftCard.Controls.Add(lblCount);

            // Right Card
            pnlRightCard = new ModernCard
            {
                Location = new Point(380, 72),
                Size = new Size(356, 320),
                CardColor = DarkTheme.BgCard,
                BorderColor = DarkTheme.BorderSubtle,
                BorderRadius = 8
            };

            lstvAccounts = new ListView
            {
                Location = new Point(14, 16),
                Size = new Size(328, 230),
                View = View.Details,
                FullRowSelect = true,
                MultiSelect = false,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = DarkTheme.BgDark,
                ForeColor = DarkTheme.TextPrimary,
                Font = DarkTheme.FontBody,
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            lstvAccounts.Columns.Add("USERNAME:", 165);
            lstvAccounts.Columns.Add("SERVER NAME:", 155);
            lstvAccounts.SelectedIndexChanged += (s, e) => OnAccountSelectionChanged();

            btnRemoveSelected = new Button
            {
                Text = "Remove Selected",
                Location = new Point(14, 260),
                Size = new Size(130, 28),
                BackColor = DarkTheme.BgInput,
                ForeColor = DarkTheme.TextSecondary,
                Font = DarkTheme.FontCaption,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRemoveSelected.FlatAppearance.BorderSize = 1;
            btnRemoveSelected.FlatAppearance.BorderColor = DarkTheme.BorderSubtle;
            btnRemoveSelected.Click += (s, e) => OnRemoveSelectedClicked();

            pnlRightCard.Controls.Add(lstvAccounts);
            pnlRightCard.Controls.Add(btnRemoveSelected);

            // Bottom Bar
            lblStatus = new Label
            {
                Text = string.Empty,
                Location = new Point(24, 412),
                AutoSize = true,
                Font = new Font("Segoe UI", 11F, FontStyle.Italic, GraphicsUnit.Pixel),
                ForeColor = DarkTheme.TextMuted
            };

            btnOK = new Button
            {
                Text = "OK",
                Location = new Point(646, 404),
                Size = new Size(90, 32),
                BackColor = DarkTheme.Accent,
                ForeColor = Color.White,
                Font = DarkTheme.FontBodyBold,
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK,
                Cursor = Cursors.Hand
            };
            btnOK.FlatAppearance.BorderSize = 0;
            btnOK.Click += (s, e) =>
            {
                Settings.SaveBotSettings();
                this.Close();
            };

            this.Controls.Add(lblTitle);
            this.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlLeftCard);
            this.Controls.Add(pnlRightCard);
            this.Controls.Add(lblStatus);
            this.Controls.Add(btnOK);
        }

        private void LoadAccounts()
        {
            lstvAccounts.Items.Clear();
            foreach (var acc in AccountManager.Accounts)
            {
                ListViewItem item = new ListViewItem(acc.Username);
                item.SubItems.Add(acc.Server ?? string.Empty);
                item.Tag = acc;

                if (!string.IsNullOrEmpty(AccountManager.SelectedAccountUsername) &&
                    acc.Username.Equals(AccountManager.SelectedAccountUsername, StringComparison.OrdinalIgnoreCase))
                {
                    item.Selected = true;
                }

                lstvAccounts.Items.Add(item);
            }

            lblCount.Text = $"{AccountManager.Accounts.Count} account(s) loaded.";

            if (lstvAccounts.SelectedItems.Count > 0)
            {
                PopulateFields((SavedAccount)lstvAccounts.SelectedItems[0].Tag);
            }
        }

        private void PopulateFields(SavedAccount acc)
        {
            if (acc == null) return;
            txtUsername.Text = acc.Username ?? string.Empty;
            txtPassword.Text = acc.Password ?? string.Empty;
            txtSecondary.Text = acc.SecondaryPasscode ?? string.Empty;
            txtServer.Text = acc.Server ?? string.Empty;
        }

        private void OnAccountSelectionChanged()
        {
            if (lstvAccounts.SelectedItems.Count > 0 && lstvAccounts.SelectedItems[0].Tag is SavedAccount acc)
            {
                AccountManager.SelectedAccountUsername = acc.Username;
                PopulateFields(acc);
            }
        }

        private void OnAddUpdateClicked()
        {
            string user = txtUsername.Text.Trim();
            if (string.IsNullOrWhiteSpace(user))
            {
                lblStatus.Text = "Please enter a valid username.";
                return;
            }

            SavedAccount existing = AccountManager.GetAccount(user);
            SavedAccount acc = existing ?? new SavedAccount();

            acc.Username = user;
            acc.Password = txtPassword.Text;
            acc.SecondaryPasscode = txtSecondary.Text.Trim();
            acc.Server = txtServer.Text.Trim();

            AccountManager.SaveAccount(acc);
            AccountManager.SelectedAccountUsername = acc.Username;

            LoadAccounts();
            lblStatus.Text = "Account saved.";
        }

        private void OnRemoveSelectedClicked()
        {
            if (lstvAccounts.SelectedItems.Count == 0)
            {
                lblStatus.Text = "No account selected to remove.";
                return;
            }

            if (lstvAccounts.SelectedItems[0].Tag is SavedAccount acc)
            {
                string targetUser = acc.Username;
                if (AccountManager.DeleteAccount(targetUser))
                {
                    txtUsername.Text = string.Empty;
                    txtPassword.Text = string.Empty;
                    txtSecondary.Text = string.Empty;
                    txtServer.Text = string.Empty;

                    LoadAccounts();
                    lblStatus.Text = "Account removed.";
                }
            }
        }
    }
}
