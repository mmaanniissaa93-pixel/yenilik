using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace xBot.App
{
    /// <summary>
    /// phBot "Komut Oluştur" penceresi (media_1789163340865.png birebir klon).
    /// </summary>
    public sealed class ScriptCreatorForm : Form
    {
        private GroupBox gbxControls;
        private Button btnRecord;
        private Button btnStopRecord;
        private Button btnSave;
        private Button btnSaveAs;
        private Button btnClear;
        private RadioButton rbAppend;
        private RadioButton rbPrepend;
        private TextBox txtScript;

        public string SavedScriptPath { get; private set; } = "";

        public ScriptCreatorForm(string initialScriptPath = null)
        {
            InitializeComponent();
            ApplyClassicStyles();

            if (!string.IsNullOrWhiteSpace(initialScriptPath) && File.Exists(initialScriptPath))
            {
                try
                {
                    txtScript.Text = File.ReadAllText(initialScriptPath);
                    SavedScriptPath = initialScriptPath;
                }
                catch { }
            }

            try
            {
                Bot.OnRecordLineAppended += AppendScriptLine;
                this.FormClosing += (s, e) => {
                    Bot.OnRecordLineAppended -= AppendScriptLine;
                };
            }
            catch { }
        }

        private void InitializeComponent()
        {
            this.Text = "Komut Oluştur";
            this.Size = new Size(820, 620);
            this.MinimumSize = new Size(760, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);

            // Left panel group
            gbxControls = new GroupBox();
            gbxControls.Text = "Kontroller";
            gbxControls.Location = new Point(12, 10);
            gbxControls.Size = new Size(334, 560);
            gbxControls.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            this.Controls.Add(gbxControls);

            // Row 1: Kayıt / Dur
            btnRecord = CreateBtn("Kayıt", 10, 20, 150, 24);
            btnStopRecord = CreateBtn("Dur", 168, 20, 154, 24);
            gbxControls.Controls.Add(btnRecord);
            gbxControls.Controls.Add(btnStopRecord);

            btnRecord.Click += (s, e) => {
                try
                {
                    if (Bot.Get != null)
                    {
                        Bot.Get.StartRecording();
                        AppendScriptLine("// Kayıt başlatıldı...");
                    }
                }
                catch { }
            };

            btnStopRecord.Click += (s, e) => {
                try
                {
                    if (Bot.Get != null && Bot.Get.isRecording)
                    {
                        Bot.Get.StopRecording();
                        AppendScriptLine("// Kayıt durduruldu.");
                    }
                }
                catch { }
            };

            // Row 2: Kaydet / Farklı Kaydet / Temizle
            btnSave = CreateBtn("Kaydet", 10, 48, 98, 24);
            btnSaveAs = CreateBtn("Farklı Kaydet", 112, 48, 114, 24);
            btnClear = CreateBtn("Temizle", 230, 48, 92, 24);
            gbxControls.Controls.Add(btnSave);
            gbxControls.Controls.Add(btnSaveAs);
            gbxControls.Controls.Add(btnClear);

            btnSave.Click += (s, e) => SaveScript(false);
            btnSaveAs.Click += (s, e) => SaveScript(true);
            btnClear.Click += (s, e) => {
                if (MessageBox.Show("Tüm script temizlensin mi?", "Komut Oluştur", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    txtScript.Clear();
                }
            };

            // Row 3: Radio buttons
            rbAppend = new RadioButton();
            rbAppend.Text = "Sonuna Ekle";
            rbAppend.Checked = true;
            rbAppend.Location = new Point(14, 76);
            rbAppend.AutoSize = true;

            rbPrepend = new RadioButton();
            rbPrepend.Text = "Başına ekle";
            rbPrepend.Location = new Point(14, 98);
            rbPrepend.AutoSize = true;

            gbxControls.Controls.Add(rbAppend);
            gbxControls.Controls.Add(rbPrepend);

            // Command Buttons: 2 Columns
            int yStart = 124;
            int btnH = 22;
            int gapY = 24;

            // Left column (21 buttons)
            string[] leftNames = new string[] {
                "Demirci", "Gıda Tüccarı", "Şifacı", "Atçı", "Jupiter",
                "Protector", "Depo", "Depodan Al", "Depoya Aktar", "Guild Deposu",
                "Guild Deposundan Al", "Guild Deposuna Aktar", "Komut dosyası çalıştır",
                "Styria", "Konsinye", "Tezgâh", "Unutulmuş Dünya",
                "Hedef Ticareti Başlat", "Hedef Ticareti Bitir",
                "Konsinye Ticareti Başla", "Konsinye Ticareti Bitir"
            };

            string[] leftCommands = new string[] {
                "DoBlacksmith", "DoGroceryTrader", "DoHerbalist", "DoStable", "DoJupiter",
                "DoProtectorTrader", "DoStorage", "DoStorageTake", "DoStorageStore", "DoGuildStorage",
                "DoGuildStorageTake", "DoGuildStorageStore", "DoScript",
                "DoStyria", "DoConsignment", "DoStall", "DoForgottenWorld",
                "trade, start", "trade, end",
                "consignment, start", "consignment, end"
            };

            for (int i = 0; i < leftNames.Length; i++)
            {
                string cmd = leftCommands[i];
                var b = CreateBtn(leftNames[i], 10, yStart + (i * gapY), 150, btnH);
                b.Click += (s, e) => AppendScriptLine(cmd);
                gbxControls.Controls.Add(b);
            }

            // Right column (17 full buttons + 2 split rows + 1 unequip)
            string[] rightNames = new string[] {
                "Işınlan", "Yorum", "Pet Kapa", "Görev", "Bin",
                "Eşya Kullan", "Petten in", "Bekle", "Eski Kervan Sistemi", "Dur",
                "Geri çağır", "Bağlantıyı kes", "Profil", "Otomatik Yapılandırma",
                "Reverse", "Yol Bul", "Parçala"
            };

            string[] rightCommands = new string[] {
                "teleport", "// Yorum", "recall", "quest", "mount",
                "use", "dismount", "wait, 1000", "oldtrade, spawn", "stop",
                "recall", "disconnect", "profile, Default", "autoconfig",
                "reverse", "pathfind", "dismantle"
            };

            for (int i = 0; i < rightNames.Length; i++)
            {
                string cmd = rightCommands[i];
                var b = CreateBtn(rightNames[i], 168, yStart + (i * gapY), 154, btnH);
                b.Click += (s, e) => AppendScriptLine(cmd);
                gbxControls.Controls.Add(b);
            }

            // Row 18: Döngü | Sırala | Böl (3 buttons)
            int yRow18 = yStart + (17 * gapY);
            var btnLoop = CreateBtn("Döngü", 168, yRow18, 50, btnH);
            btnLoop.Click += (s, e) => AppendScriptLine("loop");
            var btnSort = CreateBtn("Sırala", 220, yRow18, 50, btnH);
            btnSort.Click += (s, e) => AppendScriptLine("sort");
            var btnSplit = CreateBtn("Böl", 272, yRow18, 50, btnH);
            btnSplit.Click += (s, e) => AppendScriptLine("split");
            gbxControls.Controls.AddRange(new Control[] { btnLoop, btnSort, btnSplit });

            // Row 19: Becer | İstek | Giy (3 buttons)
            int yRow19 = yStart + (18 * gapY);
            var btnCast = CreateBtn("Becer", 168, yRow19, 50, btnH);
            btnCast.Click += (s, e) => AppendScriptLine("cast");
            var btnReq = CreateBtn("İstek", 220, yRow19, 50, btnH);
            btnReq.Click += (s, e) => AppendScriptLine("request");
            var btnEquip = CreateBtn("Giy", 272, yRow19, 50, btnH);
            btnEquip.Click += (s, e) => AppendScriptLine("equip");
            gbxControls.Controls.AddRange(new Control[] { btnCast, btnReq, btnEquip });

            // Row 20: Çıkart (1 button)
            int yRow20 = yStart + (19 * gapY);
            var btnUnequip = CreateBtn("Çıkart", 168, yRow20, 154, btnH);
            btnUnequip.Click += (s, e) => AppendScriptLine("unequip");
            gbxControls.Controls.Add(btnUnequip);

            // Right side: Script Editor
            txtScript = new TextBox();
            txtScript.Location = new Point(356, 16);
            txtScript.Size = new Size(this.ClientSize.Width - 368, this.ClientSize.Height - 32);
            txtScript.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtScript.Multiline = true;
            txtScript.ScrollBars = ScrollBars.Both;
            txtScript.WordWrap = false;
            txtScript.Font = new Font("Consolas", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            txtScript.BackColor = Color.White;
            txtScript.ForeColor = Color.Black;
            this.Controls.Add(txtScript);
        }

        private Button CreateBtn(string text, int x, int y, int w, int h)
        {
            var b = new Button();
            b.Text = text;
            b.Location = new Point(x, y);
            b.Size = new Size(w, h);
            b.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            b.FlatStyle = FlatStyle.Standard;
            b.UseVisualStyleBackColor = true;
            return b;
        }

        public void AppendScriptLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(AppendScriptLine), line);
                return;
            }

            if (rbPrepend != null && rbPrepend.Checked)
            {
                txtScript.Text = line + Environment.NewLine + txtScript.Text;
            }
            else
            {
                if (txtScript.Text.Length > 0 && !txtScript.Text.EndsWith(Environment.NewLine))
                    txtScript.AppendText(Environment.NewLine);
                txtScript.AppendText(line + Environment.NewLine);
            }
        }

        private void SaveScript(bool forceSaveAs)
        {
            try
            {
                string targetPath = SavedScriptPath;
                if (forceSaveAs || string.IsNullOrWhiteSpace(targetPath))
                {
                    using (var sfd = new SaveFileDialog())
                    {
                        sfd.Title = "Script Kaydet";
                        sfd.Filter = "Komut Dosyası (*.txt)|*.txt|Tüm Dosyalar (*.*)|*.*";
                        sfd.DefaultExt = "txt";
                        sfd.InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts");
                        if (!Directory.Exists(sfd.InitialDirectory))
                            Directory.CreateDirectory(sfd.InitialDirectory);

                        if (sfd.ShowDialog(this) == DialogResult.OK)
                        {
                            targetPath = sfd.FileName;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                File.WriteAllText(targetPath, txtScript.Text);
                SavedScriptPath = targetPath;
                MessageBox.Show("Script başarıyla kaydedildi:\n" + targetPath, "Komut Oluştur", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Script kaydedilirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyClassicStyles()
        {
            try
            {
                // Assign application icon if available
                if (Window.Get != null && Window.Get.Icon != null)
                {
                    this.Icon = Window.Get.Icon;
                }
            }
            catch { }
        }
    }
}
