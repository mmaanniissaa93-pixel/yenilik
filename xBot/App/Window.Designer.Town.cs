using System;
using System.Drawing;
using System.Windows.Forms;
using xBot.Game.Objects;
using xGraphics;

namespace xBot.App
{
	partial class Window
	{
		private void InitializeComponent_Town()
		{
            // TabPageV_Control01_Town_Panel
            // 
            this.TabPageV_Control01_Town_Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TabPageV_Control01_Town_Panel.Controls.Add(this.TabPageH_Town);
            this.TabPageV_Control01_Town_Panel.Controls.Add(this.TabPageH_Town_Option01_Panel);
            this.TabPageV_Control01_Town_Panel.Controls.Add(this.TabPageH_Town_Option02_Panel);
            this.TabPageV_Control01_Town_Panel.Controls.Add(this.TabPageH_Town_Option03_Panel);
            this.TabPageV_Control01_Town_Panel.Location = new System.Drawing.Point(135, 45);
            this.TabPageV_Control01_Town_Panel.Name = "TabPageV_Control01_Town_Panel";
            this.TabPageV_Control01_Town_Panel.Size = new System.Drawing.Size(657, 372);
            this.TabPageV_Control01_Town_Panel.TabIndex = 16;
            this.TabPageV_Control01_Town_Panel.Visible = false;
            // 
            // TabPageH_Town
            // 
            this.TabPageH_Town.Controls.Add(this.TabPageH_Town_Option03);
            this.TabPageH_Town.Controls.Add(this.TabPageH_Town_Option02);
            this.TabPageH_Town.Controls.Add(this.TabPageH_Town_Option01);
            this.TabPageH_Town.Location = new System.Drawing.Point(0, 0);
            this.TabPageH_Town.Name = "TabPageH_Town";
            this.TabPageH_Town.Size = new System.Drawing.Size(657, 27);
            this.TabPageH_Town.TabIndex = 28;
            // 
            // TabPageH_Town_Option01
            // 
            this.TabPageH_Town_Option01.FlatAppearance.BorderSize = 0;
            this.TabPageH_Town_Option01.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TabPageH_Town_Option01.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TabPageH_Town_Option01.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.TabPageH_Town_Option01.Location = new System.Drawing.Point(0, 0);
            this.TabPageH_Town_Option01.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Town_Option01.Name = "TabPageH_Town_Option01";
            this.TabPageH_Town_Option01.Size = new System.Drawing.Size(180, 26);
            this.TabPageH_Town_Option01.TabIndex = 0;
            this.TabPageH_Town_Option01.Text = "Lojistik & Pot Alma";
            this.TabPageH_Town_Option01.UseVisualStyleBackColor = true;
            this.TabPageH_Town_Option01.Click += new System.EventHandler(this.TabPageH_Option_Click);
            // 
            // TabPageH_Town_Option02
            // 
            this.TabPageH_Town_Option02.FlatAppearance.BorderSize = 0;
            this.TabPageH_Town_Option02.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TabPageH_Town_Option02.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TabPageH_Town_Option02.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.TabPageH_Town_Option02.Location = new System.Drawing.Point(181, 0);
            this.TabPageH_Town_Option02.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Town_Option02.Name = "TabPageH_Town_Option02";
            this.TabPageH_Town_Option02.Size = new System.Drawing.Size(180, 26);
            this.TabPageH_Town_Option02.TabIndex = 1;
            this.TabPageH_Town_Option02.Text = "Dövüş & Mob Tercihi";
            this.TabPageH_Town_Option02.UseVisualStyleBackColor = true;
            this.TabPageH_Town_Option02.Click += new System.EventHandler(this.TabPageH_Option_Click);
            // 
            // TabPageH_Town_Option03
            // 
            this.TabPageH_Town_Option03.FlatAppearance.BorderSize = 0;
            this.TabPageH_Town_Option03.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TabPageH_Town_Option03.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.TabPageH_Town_Option03.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.TabPageH_Town_Option03.Location = new System.Drawing.Point(362, 0);
            this.TabPageH_Town_Option03.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Town_Option03.Name = "TabPageH_Town_Option03";
            this.TabPageH_Town_Option03.Size = new System.Drawing.Size(150, 26);
            this.TabPageH_Town_Option03.TabIndex = 2;
            this.TabPageH_Town_Option03.Text = "Toplama Filtresi";
            this.TabPageH_Town_Option03.UseVisualStyleBackColor = true;
            this.TabPageH_Town_Option03.Click += new System.EventHandler(this.TabPageH_Option_Click);
            // 
            // TabPageH_Town_Option01_Panel
            // 
            this.TabPageH_Town_Option01_Panel.Controls.Add(this.Town_gbxLogistics);
            this.TabPageH_Town_Option01_Panel.Controls.Add(this.Town_gbxAutoBuy);
            this.TabPageH_Town_Option01_Panel.Location = new System.Drawing.Point(0, 27);
            this.TabPageH_Town_Option01_Panel.Name = "TabPageH_Town_Option01_Panel";
            this.TabPageH_Town_Option01_Panel.Size = new System.Drawing.Size(657, 345);
            this.TabPageH_Town_Option01_Panel.TabIndex = 29;
            // 
            // Town_gbxLogistics
            // 
            this.Town_gbxLogistics.Controls.Add(this.Town_cbxEnableTownLoop);
            this.Town_gbxLogistics.Controls.Add(this.Town_cbxRepair);
            this.Town_gbxLogistics.Controls.Add(this.Town_cbxStorage);
            this.Town_gbxLogistics.Controls.Add(this.Town_cbxSellTrash);
            this.Town_gbxLogistics.Controls.Add(this.Town_cbxReturnNavMesh);
            this.Town_gbxLogistics.Controls.Add(this.Town_lblInfo);
            this.Town_gbxLogistics.ForeColor = System.Drawing.Color.LightGray;            this.Town_gbxLogistics.Location = new System.Drawing.Point(10, 8);
            this.Town_gbxLogistics.Name = "Town_gbxLogistics";
            this.Town_gbxLogistics.Size = new System.Drawing.Size(305, 325);
            this.Town_gbxLogistics.TabIndex = 0;
            this.Town_gbxLogistics.TabStop = false;
            this.Town_gbxLogistics.Text = "Şehir Döngüsü Ayarları (Town Loop)";
            // 
            // Town_cbxEnableTownLoop
            // 
            this.Town_cbxEnableTownLoop.AutoSize = true;
            this.Town_cbxEnableTownLoop.Checked = true;
            this.Town_cbxEnableTownLoop.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Town_cbxEnableTownLoop.ForeColor = System.Drawing.Color.White;
            this.Town_cbxEnableTownLoop.Location = new System.Drawing.Point(15, 28);
            this.Town_cbxEnableTownLoop.Name = "Town_cbxEnableTownLoop";
            this.Town_cbxEnableTownLoop.Size = new System.Drawing.Size(240, 20);
            this.Town_cbxEnableTownLoop.TabIndex = 1;
            this.Town_cbxEnableTownLoop.Text = "Otomatik Şehir Döngüsü (Town Loop)";
            this.Town_cbxEnableTownLoop.UseVisualStyleBackColor = true;
            // 
            // Town_cbxRepair
            // 
            this.Town_cbxRepair.AutoSize = true;
            this.Town_cbxRepair.Checked = true;
            this.Town_cbxRepair.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Town_cbxRepair.ForeColor = System.Drawing.Color.White;
            this.Town_cbxRepair.Location = new System.Drawing.Point(15, 58);
            this.Town_cbxRepair.Name = "Town_cbxRepair";
            this.Town_cbxRepair.Size = new System.Drawing.Size(225, 20);
            this.Town_cbxRepair.TabIndex = 2;
            this.Town_cbxRepair.Text = "Demircide Ekipmanları Tamir Et";
            this.Town_cbxRepair.UseVisualStyleBackColor = true;
            // 
            // Town_cbxStorage
            // 
            this.Town_cbxStorage.AutoSize = true;
            this.Town_cbxStorage.Checked = true;
            this.Town_cbxStorage.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Town_cbxStorage.ForeColor = System.Drawing.Color.White;
            this.Town_cbxStorage.Location = new System.Drawing.Point(15, 88);
            this.Town_cbxStorage.Name = "Town_cbxStorage";
            this.Town_cbxStorage.Size = new System.Drawing.Size(255, 20);
            this.Town_cbxStorage.TabIndex = 3;
            this.Town_cbxStorage.Text = "Kasaya Depola (Elixir / Stone / SOX)";
            this.Town_cbxStorage.UseVisualStyleBackColor = true;
            // 
            // Town_cbxSellTrash
            // 
            this.Town_cbxSellTrash.AutoSize = true;
            this.Town_cbxSellTrash.Checked = true;
            this.Town_cbxSellTrash.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Town_cbxSellTrash.ForeColor = System.Drawing.Color.White;
            this.Town_cbxSellTrash.Location = new System.Drawing.Point(15, 118);
            this.Town_cbxSellTrash.Name = "Town_cbxSellTrash";
            this.Town_cbxSellTrash.Size = new System.Drawing.Size(250, 20);
            this.Town_cbxSellTrash.TabIndex = 4;
            this.Town_cbxSellTrash.Text = "Çöp Ekipmanları Sat (Sell Trash)";
            this.Town_cbxSellTrash.UseVisualStyleBackColor = true;
            // 
            // Town_cbxReturnNavMesh
            // 
            this.Town_cbxReturnNavMesh.AutoSize = true;
            this.Town_cbxReturnNavMesh.Checked = true;
            this.Town_cbxReturnNavMesh.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Town_cbxReturnNavMesh.ForeColor = System.Drawing.Color.White;
            this.Town_cbxReturnNavMesh.Location = new System.Drawing.Point(15, 148);
            this.Town_cbxReturnNavMesh.Name = "Town_cbxReturnNavMesh";
            this.Town_cbxReturnNavMesh.Size = new System.Drawing.Size(260, 20);
            this.Town_cbxReturnNavMesh.TabIndex = 5;
            this.Town_cbxReturnNavMesh.Text = "NavMesh ile Slota Geri Yürü";
            this.Town_cbxReturnNavMesh.UseVisualStyleBackColor = true;
            // 
            // Town_lblInfo
            // 
            this.Town_lblInfo.ForeColor = System.Drawing.Color.LightGray;
            this.Town_lblInfo.Location = new System.Drawing.Point(15, 185);
            this.Town_lblInfo.Name = "Town_lblInfo";
            this.Town_lblInfo.Size = new System.Drawing.Size(275, 130);
            this.Town_lblInfo.TabIndex = 6;
            this.Town_lblInfo.Text = "Karakterin iksirleri bittiğinde veya çantası dolduğunda otomatik olarak şehre döner; demirci tamiri, kasa depolaması, çöp satışı ve pot ikmalini yapıp tekrar slota yürür.";
            // 
            // Town_gbxAutoBuy
            // 
            this.Town_gbxAutoBuy.Controls.Add(this.Town_cbxAutoBuy);
            this.Town_gbxAutoBuy.Controls.Add(this.Town_lblHpType);
            this.Town_gbxAutoBuy.Controls.Add(this.Town_cmbxHpType);
            this.Town_gbxAutoBuy.Controls.Add(this.Town_lblHpAmount);
            this.Town_gbxAutoBuy.Controls.Add(this.Town_nudHpAmount);
            this.Town_gbxAutoBuy.Controls.Add(this.Town_lblMpType);
            this.Town_gbxAutoBuy.Controls.Add(this.Town_cmbxMpType);
            this.Town_gbxAutoBuy.Controls.Add(this.Town_lblMpAmount);
            this.Town_gbxAutoBuy.Controls.Add(this.Town_nudMpAmount);
            this.Town_gbxAutoBuy.Controls.Add(this.Town_cbxBuyPills);
            this.Town_gbxAutoBuy.ForeColor = System.Drawing.Color.LightGray;            this.Town_gbxAutoBuy.Location = new System.Drawing.Point(325, 8);
            this.Town_gbxAutoBuy.Name = "Town_gbxAutoBuy";
            this.Town_gbxAutoBuy.Size = new System.Drawing.Size(320, 325);
            this.Town_gbxAutoBuy.TabIndex = 10;
            this.Town_gbxAutoBuy.TabStop = false;
            this.Town_gbxAutoBuy.Text = "Otomatik İksir Satın Alma (Auto Buy)";
            // 
            // Town_cbxAutoBuy
            // 
            this.Town_cbxAutoBuy.AutoSize = true;
            this.Town_cbxAutoBuy.Checked = true;
            this.Town_cbxAutoBuy.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Town_cbxAutoBuy.ForeColor = System.Drawing.Color.White;
            this.Town_cbxAutoBuy.Location = new System.Drawing.Point(15, 28);
            this.Town_cbxAutoBuy.Name = "Town_cbxAutoBuy";
            this.Town_cbxAutoBuy.Size = new System.Drawing.Size(250, 20);
            this.Town_cbxAutoBuy.TabIndex = 11;
            this.Town_cbxAutoBuy.Text = "Otomatik İksir Satın Al (Auto Buy)";
            this.Town_cbxAutoBuy.UseVisualStyleBackColor = true;
            // 
            // Town_lblHpType
            // 
            this.Town_lblHpType.ForeColor = System.Drawing.Color.LightGray;
            this.Town_lblHpType.Location = new System.Drawing.Point(15, 60);
            this.Town_lblHpType.Name = "Town_lblHpType";
            this.Town_lblHpType.Size = new System.Drawing.Size(120, 20);
            this.Town_lblHpType.TabIndex = 12;
            this.Town_lblHpType.Text = "HP İksir Boyutu:";
            // 
            // Town_cmbxHpType
            // 
            this.Town_cmbxHpType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Town_cmbxHpType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Town_cmbxHpType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Town_cmbxHpType.ForeColor = System.Drawing.Color.White;
            this.Town_cmbxHpType.Items.AddRange(new object[] {
            "X-Small",
            "Small",
            "Medium",
            "Large",
            "X-Large"});
            this.Town_cmbxHpType.Location = new System.Drawing.Point(145, 57);
            this.Town_cmbxHpType.Name = "Town_cmbxHpType";
            this.Town_cmbxHpType.Size = new System.Drawing.Size(155, 24);
            this.Town_cmbxHpType.TabIndex = 13;
            // 
            // Town_lblHpAmount
            // 
            this.Town_lblHpAmount.ForeColor = System.Drawing.Color.LightGray;
            this.Town_lblHpAmount.Location = new System.Drawing.Point(15, 95);
            this.Town_lblHpAmount.Name = "Town_lblHpAmount";
            this.Town_lblHpAmount.Size = new System.Drawing.Size(120, 20);
            this.Town_lblHpAmount.TabIndex = 14;
            this.Town_lblHpAmount.Text = "HP Hedef Adet:";
            // 
            // Town_nudHpAmount
            // 
            this.Town_nudHpAmount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Town_nudHpAmount.ForeColor = System.Drawing.Color.White;
            this.Town_nudHpAmount.Location = new System.Drawing.Point(145, 93);
            this.Town_nudHpAmount.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.Town_nudHpAmount.Name = "Town_nudHpAmount";
            this.Town_nudHpAmount.Size = new System.Drawing.Size(155, 22);
            this.Town_nudHpAmount.TabIndex = 15;
            this.Town_nudHpAmount.Value = new decimal(new int[] { 150, 0, 0, 0 });
            // 
            // Town_lblMpType
            // 
            this.Town_lblMpType.ForeColor = System.Drawing.Color.LightGray;
            this.Town_lblMpType.Location = new System.Drawing.Point(15, 135);
            this.Town_lblMpType.Name = "Town_lblMpType";
            this.Town_lblMpType.Size = new System.Drawing.Size(120, 20);
            this.Town_lblMpType.TabIndex = 16;
            this.Town_lblMpType.Text = "MP İksir Boyutu:";
            // 
            // Town_cmbxMpType
            // 
            this.Town_cmbxMpType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Town_cmbxMpType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Town_cmbxMpType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Town_cmbxMpType.ForeColor = System.Drawing.Color.White;
            this.Town_cmbxMpType.Items.AddRange(new object[] {
            "X-Small",
            "Small",
            "Medium",
            "Large",
            "X-Large"});
            this.Town_cmbxMpType.Location = new System.Drawing.Point(145, 132);
            this.Town_cmbxMpType.Name = "Town_cmbxMpType";
            this.Town_cmbxMpType.Size = new System.Drawing.Size(155, 24);
            this.Town_cmbxMpType.TabIndex = 17;
            // 
            // Town_lblMpAmount
            // 
            this.Town_lblMpAmount.ForeColor = System.Drawing.Color.LightGray;
            this.Town_lblMpAmount.Location = new System.Drawing.Point(15, 170);
            this.Town_lblMpAmount.Name = "Town_lblMpAmount";
            this.Town_lblMpAmount.Size = new System.Drawing.Size(120, 20);
            this.Town_lblMpAmount.TabIndex = 18;
            this.Town_lblMpAmount.Text = "MP Hedef Adet:";
            // 
            // Town_nudMpAmount
            // 
            this.Town_nudMpAmount.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Town_nudMpAmount.ForeColor = System.Drawing.Color.White;
            this.Town_nudMpAmount.Location = new System.Drawing.Point(145, 168);
            this.Town_nudMpAmount.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            this.Town_nudMpAmount.Name = "Town_nudMpAmount";
            this.Town_nudMpAmount.Size = new System.Drawing.Size(155, 22);
            this.Town_nudMpAmount.TabIndex = 19;
            this.Town_nudMpAmount.Value = new decimal(new int[] { 150, 0, 0, 0 });
            // 
            // Town_cbxBuyPills
            // 
            this.Town_cbxBuyPills.AutoSize = true;
            this.Town_cbxBuyPills.Checked = true;
            this.Town_cbxBuyPills.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Town_cbxBuyPills.ForeColor = System.Drawing.Color.White;
            this.Town_cbxBuyPills.Location = new System.Drawing.Point(15, 215);
            this.Town_cbxBuyPills.Name = "Town_cbxBuyPills";
            this.Town_cbxBuyPills.Size = new System.Drawing.Size(250, 20);
            this.Town_cbxBuyPills.TabIndex = 20;
            this.Town_cbxBuyPills.Text = "Universal Pill Satın Al (50 Adet)";
            this.Town_cbxBuyPills.UseVisualStyleBackColor = true;
            // 
            // TabPageH_Town_Option02_Panel
            // 
            this.TabPageH_Town_Option02_Panel.Controls.Add(this.Combat_gbxAI);
            this.TabPageH_Town_Option02_Panel.Controls.Add(this.Combat_gbxMobFilter);
            this.TabPageH_Town_Option02_Panel.Location = new System.Drawing.Point(0, 27);
            this.TabPageH_Town_Option02_Panel.Name = "TabPageH_Town_Option02_Panel";
            this.TabPageH_Town_Option02_Panel.Size = new System.Drawing.Size(657, 345);
            this.TabPageH_Town_Option02_Panel.TabIndex = 30;
            this.TabPageH_Town_Option02_Panel.Visible = false;
            // 
            // Combat_gbxAI
            // 
            this.Combat_gbxAI.Controls.Add(this.Combat_cbxAutoBerserk);
            this.Combat_gbxAI.Controls.Add(this.Combat_cbxMobPriority);
            this.Combat_gbxAI.Controls.Add(this.Combat_lblInfo);
            this.Combat_gbxAI.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Combat_gbxAI.Location = new System.Drawing.Point(10, 8);
            this.Combat_gbxAI.Name = "Combat_gbxAI";
            this.Combat_gbxAI.Size = new System.Drawing.Size(305, 325);
            this.Combat_gbxAI.TabIndex = 10;
            this.Combat_gbxAI.TabStop = false;
            this.Combat_gbxAI.Text = "Gelişmiş Dövüş Yapay Zekası (Combat AI)";
            // 
            // Combat_cbxAutoBerserk
            // 
            this.Combat_cbxAutoBerserk.AutoSize = true;
            this.Combat_cbxAutoBerserk.Checked = true;
            this.Combat_cbxAutoBerserk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Combat_cbxAutoBerserk.ForeColor = System.Drawing.Color.White;
            this.Combat_cbxAutoBerserk.Location = new System.Drawing.Point(15, 28);
            this.Combat_cbxAutoBerserk.Name = "Combat_cbxAutoBerserk";
            this.Combat_cbxAutoBerserk.Size = new System.Drawing.Size(255, 20);
            this.Combat_cbxAutoBerserk.TabIndex = 11;
            this.Combat_cbxAutoBerserk.Text = "Akıllı Berserker (Zerk on Threat)";
            this.Combat_cbxAutoBerserk.UseVisualStyleBackColor = true;
            // 
            // Combat_cbxMobPriority
            // 
            this.Combat_cbxMobPriority.AutoSize = true;
            this.Combat_cbxMobPriority.Checked = true;
            this.Combat_cbxMobPriority.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Combat_cbxMobPriority.ForeColor = System.Drawing.Color.White;
            this.Combat_cbxMobPriority.Location = new System.Drawing.Point(15, 58);
            this.Combat_cbxMobPriority.Name = "Combat_cbxMobPriority";
            this.Combat_cbxMobPriority.Size = new System.Drawing.Size(260, 20);
            this.Combat_cbxMobPriority.TabIndex = 12;
            this.Combat_cbxMobPriority.Text = "Tehdit Önceliği (Boss/Giant First)";
            this.Combat_cbxMobPriority.UseVisualStyleBackColor = true;
            // 
            // 
            // Combat_lblInfo
            // 
            this.Combat_lblInfo.ForeColor = System.Drawing.Color.LightGray;
            this.Combat_lblInfo.Location = new System.Drawing.Point(15, 175);
            this.Combat_lblInfo.Name = "Combat_lblInfo";
            this.Combat_lblInfo.Size = new System.Drawing.Size(275, 140);
            this.Combat_lblInfo.TabIndex = 15;
            this.Combat_lblInfo.Text = "Slottaki Unique/Elite/Giant canavarlara öncelik verir, etrafta 3+ mob olduğunda veya Giant çıktığında zerk basar ve yay/staff kullanan karakterlerde mesafeyi korur.";
            // 
            // Combat_gbxMobFilter
            // 
            this.Combat_gbxMobFilter.Controls.Add(this.Combat_cbxTargetGeneral);
            this.Combat_gbxMobFilter.Controls.Add(this.Combat_cbxTargetChampion);
            this.Combat_gbxMobFilter.Controls.Add(this.Combat_cbxTargetGiant);
            this.Combat_gbxMobFilter.Controls.Add(this.Combat_cbxTargetParty);
            this.Combat_gbxMobFilter.Controls.Add(this.Combat_cbxTargetElite);
            this.Combat_gbxMobFilter.Controls.Add(this.Combat_cbxTargetUnique);
            this.Combat_gbxMobFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Combat_gbxMobFilter.Location = new System.Drawing.Point(325, 8);
            this.Combat_gbxMobFilter.Name = "Combat_gbxMobFilter";
            this.Combat_gbxMobFilter.Size = new System.Drawing.Size(320, 325);
            this.Combat_gbxMobFilter.TabIndex = 20;
            this.Combat_gbxMobFilter.TabStop = false;
            this.Combat_gbxMobFilter.Text = "Canavar Tercih Filtresi (Monster Target)";
            // 
            // Combat_cbxTargetGeneral
            // 
            this.Combat_cbxTargetGeneral.AutoSize = true;
            this.Combat_cbxTargetGeneral.Checked = true;
            this.Combat_cbxTargetGeneral.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Combat_cbxTargetGeneral.ForeColor = System.Drawing.Color.White;
            this.Combat_cbxTargetGeneral.Location = new System.Drawing.Point(20, 35);
            this.Combat_cbxTargetGeneral.Name = "Combat_cbxTargetGeneral";
            this.Combat_cbxTargetGeneral.Size = new System.Drawing.Size(220, 20);
            this.Combat_cbxTargetGeneral.TabIndex = 21;
            this.Combat_cbxTargetGeneral.Text = "General (Normal) Moblara Saldır";
            this.Combat_cbxTargetGeneral.UseVisualStyleBackColor = true;
            // 
            // Combat_cbxTargetChampion
            // 
            this.Combat_cbxTargetChampion.AutoSize = true;
            this.Combat_cbxTargetChampion.Checked = true;
            this.Combat_cbxTargetChampion.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Combat_cbxTargetChampion.ForeColor = System.Drawing.Color.White;
            this.Combat_cbxTargetChampion.Location = new System.Drawing.Point(20, 75);
            this.Combat_cbxTargetChampion.Name = "Combat_cbxTargetChampion";
            this.Combat_cbxTargetChampion.Size = new System.Drawing.Size(210, 20);
            this.Combat_cbxTargetChampion.TabIndex = 22;
            this.Combat_cbxTargetChampion.Text = "Champion Moblara Saldır";
            this.Combat_cbxTargetChampion.UseVisualStyleBackColor = true;
            // 
            // Combat_cbxTargetGiant
            // 
            this.Combat_cbxTargetGiant.AutoSize = true;
            this.Combat_cbxTargetGiant.Checked = true;
            this.Combat_cbxTargetGiant.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Combat_cbxTargetGiant.ForeColor = System.Drawing.Color.White;
            this.Combat_cbxTargetGiant.Location = new System.Drawing.Point(20, 115);
            this.Combat_cbxTargetGiant.Name = "Combat_cbxTargetGiant";
            this.Combat_cbxTargetGiant.Size = new System.Drawing.Size(200, 20);
            this.Combat_cbxTargetGiant.TabIndex = 23;
            this.Combat_cbxTargetGiant.Text = "Giant Moblara Saldır";
            this.Combat_cbxTargetGiant.UseVisualStyleBackColor = true;
            // 
            // Combat_cbxTargetParty
            // 
            this.Combat_cbxTargetParty.AutoSize = true;
            this.Combat_cbxTargetParty.Checked = true;
            this.Combat_cbxTargetParty.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Combat_cbxTargetParty.ForeColor = System.Drawing.Color.White;
            this.Combat_cbxTargetParty.Location = new System.Drawing.Point(20, 155);
            this.Combat_cbxTargetParty.Name = "Combat_cbxTargetParty";
            this.Combat_cbxTargetParty.Size = new System.Drawing.Size(210, 20);
            this.Combat_cbxTargetParty.TabIndex = 24;
            this.Combat_cbxTargetParty.Text = "Party Moblara Saldır";
            this.Combat_cbxTargetParty.UseVisualStyleBackColor = true;
            // 
            // Combat_cbxTargetElite
            // 
            this.Combat_cbxTargetElite.AutoSize = true;
            this.Combat_cbxTargetElite.Checked = true;
            this.Combat_cbxTargetElite.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Combat_cbxTargetElite.ForeColor = System.Drawing.Color.White;
            this.Combat_cbxTargetElite.Location = new System.Drawing.Point(20, 195);
            this.Combat_cbxTargetElite.Name = "Combat_cbxTargetElite";
            this.Combat_cbxTargetElite.Size = new System.Drawing.Size(200, 20);
            this.Combat_cbxTargetElite.TabIndex = 25;
            this.Combat_cbxTargetElite.Text = "Elite Moblara Saldır";
            this.Combat_cbxTargetElite.UseVisualStyleBackColor = true;
            // 
            // Combat_cbxTargetUnique
            // 
            this.Combat_cbxTargetUnique.AutoSize = true;
            this.Combat_cbxTargetUnique.Checked = true;
            this.Combat_cbxTargetUnique.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Combat_cbxTargetUnique.ForeColor = System.Drawing.Color.White;
            this.Combat_cbxTargetUnique.Location = new System.Drawing.Point(20, 235);
            this.Combat_cbxTargetUnique.Name = "Combat_cbxTargetUnique";
            this.Combat_cbxTargetUnique.Size = new System.Drawing.Size(220, 20);
            this.Combat_cbxTargetUnique.TabIndex = 26;
            this.Combat_cbxTargetUnique.Text = "Unique / Boss Moblara Saldır";
            this.Combat_cbxTargetUnique.UseVisualStyleBackColor = true;
            // 
            // TabPageH_Town_Option03_Panel
            // 
            this.TabPageH_Town_Option03_Panel.Controls.Add(this.Filter_gbxPick);
            this.TabPageH_Town_Option03_Panel.Location = new System.Drawing.Point(0, 27);
            this.TabPageH_Town_Option03_Panel.Name = "TabPageH_Town_Option03_Panel";
            this.TabPageH_Town_Option03_Panel.Size = new System.Drawing.Size(657, 345);
            this.TabPageH_Town_Option03_Panel.TabIndex = 31;
            this.TabPageH_Town_Option03_Panel.Visible = false;
            // 
            // Filter_gbxPick
            // 
            this.Filter_gbxPick.Controls.Add(this.Filter_cbxPickGold);
            this.Filter_gbxPick.Controls.Add(this.Filter_cbxPickEquip);
            this.Filter_gbxPick.Controls.Add(this.Filter_cbxPickElixirStone);
            this.Filter_gbxPick.Controls.Add(this.Filter_cbxPickMaterials);
            this.Filter_gbxPick.Controls.Add(this.Filter_cbxUsePet);
            this.Filter_gbxPick.Controls.Add(this.Filter_lblInfo);
            this.Filter_gbxPick.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Filter_gbxPick.Location = new System.Drawing.Point(15, 10);
            this.Filter_gbxPick.Name = "Filter_gbxPick";
            this.Filter_gbxPick.Size = new System.Drawing.Size(625, 320);
            this.Filter_gbxPick.TabIndex = 0;
            this.Filter_gbxPick.TabStop = false;
            this.Filter_gbxPick.Text = "Yerden Eşya Toplama Seçenekleri (Pick Filter)";
            // 
            // Filter_cbxPickGold
            // 
            this.Filter_cbxPickGold.AutoSize = true;
            this.Filter_cbxPickGold.Checked = true;
            this.Filter_cbxPickGold.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Filter_cbxPickGold.ForeColor = System.Drawing.Color.White;
            this.Filter_cbxPickGold.Location = new System.Drawing.Point(25, 35);
            this.Filter_cbxPickGold.Name = "Filter_cbxPickGold";
            this.Filter_cbxPickGold.Size = new System.Drawing.Size(200, 20);
            this.Filter_cbxPickGold.TabIndex = 1;
            this.Filter_cbxPickGold.Text = "Altınları Topla (Pick Gold)";
            this.Filter_cbxPickGold.UseVisualStyleBackColor = true;
            // 
            // Filter_cbxPickEquip
            // 
            this.Filter_cbxPickEquip.AutoSize = true;
            this.Filter_cbxPickEquip.Checked = true;
            this.Filter_cbxPickEquip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Filter_cbxPickEquip.ForeColor = System.Drawing.Color.White;
            this.Filter_cbxPickEquip.Location = new System.Drawing.Point(25, 75);
            this.Filter_cbxPickEquip.Name = "Filter_cbxPickEquip";
            this.Filter_cbxPickEquip.Size = new System.Drawing.Size(270, 20);
            this.Filter_cbxPickEquip.TabIndex = 2;
            this.Filter_cbxPickEquip.Text = "Ekipmanları Topla (Silah / Zırh / Kalkan)";
            this.Filter_cbxPickEquip.UseVisualStyleBackColor = true;
            // 
            // Filter_cbxPickElixirStone
            // 
            this.Filter_cbxPickElixirStone.AutoSize = true;
            this.Filter_cbxPickElixirStone.Checked = true;
            this.Filter_cbxPickElixirStone.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Filter_cbxPickElixirStone.ForeColor = System.Drawing.Color.White;
            this.Filter_cbxPickElixirStone.Location = new System.Drawing.Point(25, 115);
            this.Filter_cbxPickElixirStone.Name = "Filter_cbxPickElixirStone";
            this.Filter_cbxPickElixirStone.Size = new System.Drawing.Size(280, 20);
            this.Filter_cbxPickElixirStone.TabIndex = 3;
            this.Filter_cbxPickElixirStone.Text = "Elixir ve Simya Taşlarını Topla (Alchemy)";
            this.Filter_cbxPickElixirStone.UseVisualStyleBackColor = true;
            // 
            // Filter_cbxPickMaterials
            // 
            this.Filter_cbxPickMaterials.AutoSize = true;
            this.Filter_cbxPickMaterials.Checked = true;
            this.Filter_cbxPickMaterials.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Filter_cbxPickMaterials.ForeColor = System.Drawing.Color.White;
            this.Filter_cbxPickMaterials.Location = new System.Drawing.Point(25, 155);
            this.Filter_cbxPickMaterials.Name = "Filter_cbxPickMaterials";
            this.Filter_cbxPickMaterials.Size = new System.Drawing.Size(260, 20);
            this.Filter_cbxPickMaterials.TabIndex = 4;
            this.Filter_cbxPickMaterials.Text = "Malzeme, Çöp ve Görev Eşyalarını Topla";
            this.Filter_cbxPickMaterials.UseVisualStyleBackColor = true;
            // 
            // Filter_cbxUsePet
            // 
            this.Filter_cbxUsePet.AutoSize = true;
            this.Filter_cbxUsePet.Checked = true;
            this.Filter_cbxUsePet.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Filter_cbxUsePet.ForeColor = System.Drawing.Color.White;
            this.Filter_cbxUsePet.Location = new System.Drawing.Point(25, 195);
            this.Filter_cbxUsePet.Name = "Filter_cbxUsePet";
            this.Filter_cbxUsePet.Size = new System.Drawing.Size(280, 20);
            this.Filter_cbxUsePet.TabIndex = 5;
            this.Filter_cbxUsePet.Text = "Toplama Peti Kullan (Varsa Pet İle Topla)";
            this.Filter_cbxUsePet.UseVisualStyleBackColor = true;
            // 
            // Filter_lblInfo
            // 
            this.Filter_lblInfo.ForeColor = System.Drawing.Color.LightGray;
            this.Filter_lblInfo.Location = new System.Drawing.Point(25, 245);
            this.Filter_lblInfo.Name = "Filter_lblInfo";
            this.Filter_lblInfo.Size = new System.Drawing.Size(560, 60);
            this.Filter_lblInfo.TabIndex = 6;
            this.Filter_lblInfo.Text = "Filtrede kapattığınız eşya grupları slottayken bot veya toplama peti tarafından toplanmaz, yok sayılarak zaman ve çanta alanı tasarrufu sağlanır.";
		}
	}
}