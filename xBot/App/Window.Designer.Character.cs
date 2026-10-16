using System;
using System.Drawing;
using System.Windows.Forms;
using xBot.Game.Objects;
using xGraphics;

namespace xBot.App
{
	partial class Window
	{
		private void InitializeComponent_Character()
		{
            // TabPageV_Control01_Character_Panel
            // 
            this.TabPageV_Control01_Character_Panel.Controls.Add(this.TabPageH_Character);
            this.TabPageV_Control01_Character_Panel.Controls.Add(this.TabPageH_Character_Option04_Panel);
            this.TabPageV_Control01_Character_Panel.Controls.Add(this.TabPageH_Character_Option02_Panel);
            this.TabPageV_Control01_Character_Panel.Controls.Add(this.TabPageH_Character_Option01_Panel);
            this.TabPageV_Control01_Character_Panel.Controls.Add(this.TabPageH_Character_Option03_Panel);
            this.TabPageV_Control01_Character_Panel.Location = new System.Drawing.Point(135, 45);
            this.TabPageV_Control01_Character_Panel.Name = "TabPageV_Control01_Character_Panel";
            this.TabPageV_Control01_Character_Panel.Size = new System.Drawing.Size(657, 372);
            this.TabPageV_Control01_Character_Panel.TabIndex = 6;
            this.TabPageV_Control01_Character_Panel.Visible = false;
            // 
            // TabPageH_Character
            // 
            this.TabPageH_Character.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.TabPageH_Character.Controls.Add(this.TabPageH_Character_Option04);
            this.TabPageH_Character.Controls.Add(this.TabPageH_Character_Option03);
            this.TabPageH_Character.Controls.Add(this.TabPageH_Character_Option02);
            this.TabPageH_Character.Controls.Add(this.TabPageH_Character_Option01);
            this.TabPageH_Character.Location = new System.Drawing.Point(0, 0);
            this.TabPageH_Character.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Character.Name = "TabPageH_Character";
            this.TabPageH_Character.Size = new System.Drawing.Size(657, 28);
            this.TabPageH_Character.TabIndex = 9;
            // 
            // TabPageH_Character_Option04
            // 
            this.TabPageH_Character_Option04.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TabPageH_Character_Option04.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.TabPageH_Character_Option04.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Character_Option04.FlatAppearance.BorderSize = 0;
            this.TabPageH_Character_Option04.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.TabPageH_Character_Option04.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Character_Option04.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TabPageH_Character_Option04.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TabPageH_Character_Option04.Location = new System.Drawing.Point(493, 0);
            this.TabPageH_Character_Option04.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Character_Option04.Name = "TabPageH_Character_Option04";
            this.TabPageH_Character_Option04.Size = new System.Drawing.Size(164, 26);
            this.TabPageH_Character_Option04.TabIndex = 15;
            this.TabPageH_Character_Option04.Tag = "Source Sans Pro";
            this.TabPageH_Character_Option04.Text = "Misc.";
            this.TabPageH_Character_Option04.UseVisualStyleBackColor = false;
            this.TabPageH_Character_Option04.Click += new System.EventHandler(this.TabPageH_Option_Click);
            // 
            // TabPageH_Character_Option03
            // 
            this.TabPageH_Character_Option03.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TabPageH_Character_Option03.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.TabPageH_Character_Option03.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Character_Option03.FlatAppearance.BorderSize = 0;
            this.TabPageH_Character_Option03.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.TabPageH_Character_Option03.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Character_Option03.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TabPageH_Character_Option03.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TabPageH_Character_Option03.Location = new System.Drawing.Point(329, 0);
            this.TabPageH_Character_Option03.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Character_Option03.Name = "TabPageH_Character_Option03";
            this.TabPageH_Character_Option03.Size = new System.Drawing.Size(164, 26);
            this.TabPageH_Character_Option03.TabIndex = 14;
            this.TabPageH_Character_Option03.Tag = "Source Sans Pro";
            this.TabPageH_Character_Option03.Text = ". . .";
            this.TabPageH_Character_Option03.UseVisualStyleBackColor = false;
            this.TabPageH_Character_Option03.Click += new System.EventHandler(this.TabPageH_Option_Click);
            // 
            // TabPageH_Character_Option02
            // 
            this.TabPageH_Character_Option02.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TabPageH_Character_Option02.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.TabPageH_Character_Option02.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Character_Option02.FlatAppearance.BorderSize = 0;
            this.TabPageH_Character_Option02.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.TabPageH_Character_Option02.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Character_Option02.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TabPageH_Character_Option02.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TabPageH_Character_Option02.Location = new System.Drawing.Point(165, 0);
            this.TabPageH_Character_Option02.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Character_Option02.Name = "TabPageH_Character_Option02";
            this.TabPageH_Character_Option02.Size = new System.Drawing.Size(164, 26);
            this.TabPageH_Character_Option02.TabIndex = 13;
            this.TabPageH_Character_Option02.Tag = "Source Sans Pro";
            this.TabPageH_Character_Option02.Text = "Potions";
            this.TabPageH_Character_Option02.UseVisualStyleBackColor = false;
            this.TabPageH_Character_Option02.Click += new System.EventHandler(this.TabPageH_Option_Click);
            // 
            // TabPageH_Character_Option01
            // 
            this.TabPageH_Character_Option01.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TabPageH_Character_Option01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.TabPageH_Character_Option01.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Character_Option01.FlatAppearance.BorderSize = 0;
            this.TabPageH_Character_Option01.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.TabPageH_Character_Option01.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Character_Option01.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TabPageH_Character_Option01.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TabPageH_Character_Option01.Location = new System.Drawing.Point(0, 0);
            this.TabPageH_Character_Option01.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Character_Option01.Name = "TabPageH_Character_Option01";
            this.TabPageH_Character_Option01.Size = new System.Drawing.Size(165, 26);
            this.TabPageH_Character_Option01.TabIndex = 12;
            this.TabPageH_Character_Option01.Tag = "Source Sans Pro";
            this.TabPageH_Character_Option01.Text = "Info";
            this.TabPageH_Character_Option01.UseVisualStyleBackColor = false;
            this.TabPageH_Character_Option01.Click += new System.EventHandler(this.TabPageH_Option_Click);
            // 
            // TabPageH_Character_Option04_Panel
            // 
            this.TabPageH_Character_Option04_Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TabPageH_Character_Option04_Panel.Controls.Add(this.Character_cbxPVPModeUseShield);
            this.TabPageH_Character_Option04_Panel.Controls.Add(this.Character_cbxPVPMode);
            this.TabPageH_Character_Option04_Panel.Controls.Add(this.Character_cbxRefuseExchange);
            this.TabPageH_Character_Option04_Panel.Controls.Add(this.Character_cbxApproveExchange);
            this.TabPageH_Character_Option04_Panel.Controls.Add(this.Character_cbxConfirmExchange);
            this.TabPageH_Character_Option04_Panel.Controls.Add(this.Character_cbxAcceptExchangeLeaderOnly);
            this.TabPageH_Character_Option04_Panel.Controls.Add(this.Character_cbxAcceptExchange);
            this.TabPageH_Character_Option04_Panel.Controls.Add(this.Character_cbxAcceptRessPartyOnly);
            this.TabPageH_Character_Option04_Panel.Controls.Add(this.Character_cbxAcceptRess);
            this.TabPageH_Character_Option04_Panel.Location = new System.Drawing.Point(0, 27);
            this.TabPageH_Character_Option04_Panel.Name = "TabPageH_Character_Option04_Panel";
            this.TabPageH_Character_Option04_Panel.Size = new System.Drawing.Size(657, 345);
            this.TabPageH_Character_Option04_Panel.TabIndex = 12;
            this.TabPageH_Character_Option04_Panel.Visible = false;
            // 
            // Character_cbxPVPModeUseShield
            // 
            this.Character_cbxPVPModeUseShield.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxPVPModeUseShield.FlatAppearance.BorderSize = 0;
            this.Character_cbxPVPModeUseShield.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxPVPModeUseShield.Font = new System.Drawing.Font("Source Sans Pro", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxPVPModeUseShield.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxPVPModeUseShield.Location = new System.Drawing.Point(456, 23);
            this.Character_cbxPVPModeUseShield.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxPVPModeUseShield.Name = "Character_cbxPVPModeUseShield";
            this.Character_cbxPVPModeUseShield.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxPVPModeUseShield.Size = new System.Drawing.Size(199, 25);
            this.Character_cbxPVPModeUseShield.TabIndex = 17;
            this.Character_cbxPVPModeUseShield.Tag = "Source Sans Pro";
            this.Character_cbxPVPModeUseShield.Text = "Equip Shield";
            this.Character_cbxPVPModeUseShield.UseVisualStyleBackColor = false;
            // 
            // Character_cbxPVPMode
            // 
            this.Character_cbxPVPMode.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxPVPMode.FlatAppearance.BorderSize = 0;
            this.Character_cbxPVPMode.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxPVPMode.Font = new System.Drawing.Font("Source Sans Pro", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxPVPMode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxPVPMode.Location = new System.Drawing.Point(450, 0);
            this.Character_cbxPVPMode.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxPVPMode.Name = "Character_cbxPVPMode";
            this.Character_cbxPVPMode.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxPVPMode.Size = new System.Drawing.Size(201, 25);
            this.Character_cbxPVPMode.TabIndex = 16;
            this.Character_cbxPVPMode.Tag = "Source Sans Pro";
            this.Character_cbxPVPMode.Text = "Auto-Switch Weapon";
            this.Character_cbxPVPMode.UseVisualStyleBackColor = false;
            // 
            // Character_cbxRefuseExchange
            // 
            this.Character_cbxRefuseExchange.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxRefuseExchange.FlatAppearance.BorderSize = 0;
            this.Character_cbxRefuseExchange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxRefuseExchange.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxRefuseExchange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxRefuseExchange.Location = new System.Drawing.Point(0, 50);
            this.Character_cbxRefuseExchange.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxRefuseExchange.Name = "Character_cbxRefuseExchange";
            this.Character_cbxRefuseExchange.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxRefuseExchange.Size = new System.Drawing.Size(142, 25);
            this.Character_cbxRefuseExchange.TabIndex = 15;
            this.Character_cbxRefuseExchange.Tag = "Source Sans Pro";
            this.Character_cbxRefuseExchange.Text = "Refuse exchanges";
            this.Character_cbxRefuseExchange.UseVisualStyleBackColor = false;
            this.Character_cbxRefuseExchange.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxApproveExchange
            // 
            this.Character_cbxApproveExchange.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxApproveExchange.FlatAppearance.BorderSize = 0;
            this.Character_cbxApproveExchange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxApproveExchange.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxApproveExchange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxApproveExchange.Location = new System.Drawing.Point(6, 150);
            this.Character_cbxApproveExchange.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxApproveExchange.Name = "Character_cbxApproveExchange";
            this.Character_cbxApproveExchange.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxApproveExchange.Size = new System.Drawing.Size(152, 25);
            this.Character_cbxApproveExchange.TabIndex = 14;
            this.Character_cbxApproveExchange.Tag = "Source Sans Pro";
            this.Character_cbxApproveExchange.Text = "Approve inmediatly";
            this.Character_cbxApproveExchange.UseVisualStyleBackColor = false;
            this.Character_cbxApproveExchange.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxConfirmExchange
            // 
            this.Character_cbxConfirmExchange.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxConfirmExchange.FlatAppearance.BorderSize = 0;
            this.Character_cbxConfirmExchange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxConfirmExchange.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxConfirmExchange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxConfirmExchange.Location = new System.Drawing.Point(6, 125);
            this.Character_cbxConfirmExchange.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxConfirmExchange.Name = "Character_cbxConfirmExchange";
            this.Character_cbxConfirmExchange.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxConfirmExchange.Size = new System.Drawing.Size(151, 25);
            this.Character_cbxConfirmExchange.TabIndex = 13;
            this.Character_cbxConfirmExchange.Tag = "Source Sans Pro";
            this.Character_cbxConfirmExchange.Text = "Confirm inmediatly";
            this.Character_cbxConfirmExchange.UseVisualStyleBackColor = false;
            this.Character_cbxConfirmExchange.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxAcceptExchangeLeaderOnly
            // 
            this.Character_cbxAcceptExchangeLeaderOnly.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxAcceptExchangeLeaderOnly.FlatAppearance.BorderSize = 0;
            this.Character_cbxAcceptExchangeLeaderOnly.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxAcceptExchangeLeaderOnly.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxAcceptExchangeLeaderOnly.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxAcceptExchangeLeaderOnly.Location = new System.Drawing.Point(6, 100);
            this.Character_cbxAcceptExchangeLeaderOnly.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxAcceptExchangeLeaderOnly.Name = "Character_cbxAcceptExchangeLeaderOnly";
            this.Character_cbxAcceptExchangeLeaderOnly.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxAcceptExchangeLeaderOnly.Size = new System.Drawing.Size(151, 25);
            this.Character_cbxAcceptExchangeLeaderOnly.TabIndex = 12;
            this.Character_cbxAcceptExchangeLeaderOnly.Tag = "Source Sans Pro";
            this.Character_cbxAcceptExchangeLeaderOnly.Text = "Only Leader list";
            this.Character_cbxAcceptExchangeLeaderOnly.UseVisualStyleBackColor = false;
            this.Character_cbxAcceptExchangeLeaderOnly.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxAcceptExchange
            // 
            this.Character_cbxAcceptExchange.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxAcceptExchange.FlatAppearance.BorderSize = 0;
            this.Character_cbxAcceptExchange.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxAcceptExchange.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxAcceptExchange.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxAcceptExchange.Location = new System.Drawing.Point(0, 75);
            this.Character_cbxAcceptExchange.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxAcceptExchange.Name = "Character_cbxAcceptExchange";
            this.Character_cbxAcceptExchange.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxAcceptExchange.Size = new System.Drawing.Size(142, 25);
            this.Character_cbxAcceptExchange.TabIndex = 11;
            this.Character_cbxAcceptExchange.Tag = "Source Sans Pro";
            this.Character_cbxAcceptExchange.Text = "Accept exchanges";
            this.Character_cbxAcceptExchange.UseVisualStyleBackColor = false;
            this.Character_cbxAcceptExchange.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxAcceptRessPartyOnly
            // 
            this.Character_cbxAcceptRessPartyOnly.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxAcceptRessPartyOnly.FlatAppearance.BorderSize = 0;
            this.Character_cbxAcceptRessPartyOnly.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxAcceptRessPartyOnly.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxAcceptRessPartyOnly.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxAcceptRessPartyOnly.Location = new System.Drawing.Point(6, 25);
            this.Character_cbxAcceptRessPartyOnly.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxAcceptRessPartyOnly.Name = "Character_cbxAcceptRessPartyOnly";
            this.Character_cbxAcceptRessPartyOnly.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxAcceptRessPartyOnly.Size = new System.Drawing.Size(157, 25);
            this.Character_cbxAcceptRessPartyOnly.TabIndex = 10;
            this.Character_cbxAcceptRessPartyOnly.Tag = "Source Sans Pro";
            this.Character_cbxAcceptRessPartyOnly.Text = "Only party members";
            this.Character_cbxAcceptRessPartyOnly.UseVisualStyleBackColor = false;
            this.Character_cbxAcceptRessPartyOnly.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxAcceptRess
            // 
            this.Character_cbxAcceptRess.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxAcceptRess.FlatAppearance.BorderSize = 0;
            this.Character_cbxAcceptRess.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxAcceptRess.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxAcceptRess.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxAcceptRess.Location = new System.Drawing.Point(0, 0);
            this.Character_cbxAcceptRess.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxAcceptRess.Name = "Character_cbxAcceptRess";
            this.Character_cbxAcceptRess.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxAcceptRess.Size = new System.Drawing.Size(157, 25);
            this.Character_cbxAcceptRess.TabIndex = 9;
            this.Character_cbxAcceptRess.Tag = "Source Sans Pro";
            this.Character_cbxAcceptRess.Text = "Accept resurrection";
            this.Character_cbxAcceptRess.UseVisualStyleBackColor = false;
            this.Character_cbxAcceptRess.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // TabPageH_Character_Option02_Panel
            // 
            this.TabPageH_Character_Option02_Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TabPageH_Character_Option02_Panel.Controls.Add(this.Character_gbxPotionPet);
            this.TabPageH_Character_Option02_Panel.Controls.Add(this.Character_gbxPotionsPlayer);
            this.TabPageH_Character_Option02_Panel.Location = new System.Drawing.Point(0, 27);
            this.TabPageH_Character_Option02_Panel.Name = "TabPageH_Character_Option02_Panel";
            this.TabPageH_Character_Option02_Panel.Size = new System.Drawing.Size(657, 345);
            this.TabPageH_Character_Option02_Panel.TabIndex = 11;
            this.TabPageH_Character_Option02_Panel.Visible = false;
            // 
            // Character_gbxPotionPet
            // 
            this.Character_gbxPotionPet.Controls.Add(this.Character_tbxUsePetHGP);
            this.Character_gbxPotionPet.Controls.Add(this.Character_cbxUsePetHGP);
            this.Character_gbxPotionPet.Controls.Add(this.Character_cbxUsePetsPill);
            this.Character_gbxPotionPet.Controls.Add(this.Character_tbxUseTransportHP);
            this.Character_gbxPotionPet.Controls.Add(this.Character_cbxUseTransportHP);
            this.Character_gbxPotionPet.Controls.Add(this.Character_tbxUsePetHP);
            this.Character_gbxPotionPet.Controls.Add(this.Character_cbxUsePetHP);
            this.Character_gbxPotionPet.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_gbxPotionPet.ForeColor = System.Drawing.Color.LightGray;
            this.Character_gbxPotionPet.Location = new System.Drawing.Point(182, 0);
            this.Character_gbxPotionPet.Margin = new System.Windows.Forms.Padding(8, 0, 8, 8);
            this.Character_gbxPotionPet.Name = "Character_gbxPotionPet";
            this.Character_gbxPotionPet.Size = new System.Drawing.Size(216, 216);
            this.Character_gbxPotionPet.TabIndex = 23;
            this.Character_gbxPotionPet.TabStop = false;
            this.Character_gbxPotionPet.Tag = "Source Sans Pro";
            this.Character_gbxPotionPet.Text = "Pet";
            // 
            // Character_tbxUsePetHGP
            // 
            this.Character_tbxUsePetHGP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Character_tbxUsePetHGP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Character_tbxUsePetHGP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_tbxUsePetHGP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_tbxUsePetHGP.Location = new System.Drawing.Point(128, 89);
            this.Character_tbxUsePetHGP.MaxLength = 3;
            this.Character_tbxUsePetHGP.Name = "Character_tbxUsePetHGP";
            this.Character_tbxUsePetHGP.Size = new System.Drawing.Size(35, 23);
            this.Character_tbxUsePetHGP.TabIndex = 26;
            this.Character_tbxUsePetHGP.Tag = "Source Sans Pro";
            this.Character_tbxUsePetHGP.Text = "0";
            this.Character_tbxUsePetHGP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Character_tbxUsePetHGP.TextChanged += new System.EventHandler(this.Control_TextChanged);
            // 
            // Character_cbxUsePetHGP
            // 
            this.Character_cbxUsePetHGP.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUsePetHGP.FlatAppearance.BorderSize = 0;
            this.Character_cbxUsePetHGP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUsePetHGP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUsePetHGP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUsePetHGP.Location = new System.Drawing.Point(2, 89);
            this.Character_cbxUsePetHGP.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUsePetHGP.Name = "Character_cbxUsePetHGP";
            this.Character_cbxUsePetHGP.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUsePetHGP.Size = new System.Drawing.Size(212, 25);
            this.Character_cbxUsePetHGP.TabIndex = 25;
            this.Character_cbxUsePetHGP.Tag = "Source Sans Pro";
            this.Character_cbxUsePetHGP.Text = "Use HGP on pet               %";
            this.ToolTips.SetToolTip(this.Character_cbxUsePetHGP, "Recover starvation from pet when is less or equal than");
            this.Character_cbxUsePetHGP.UseVisualStyleBackColor = false;
            this.Character_cbxUsePetHGP.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxUsePetsPill
            // 
            this.Character_cbxUsePetsPill.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUsePetsPill.FlatAppearance.BorderSize = 0;
            this.Character_cbxUsePetsPill.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUsePetsPill.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUsePetsPill.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUsePetsPill.Location = new System.Drawing.Point(2, 64);
            this.Character_cbxUsePetsPill.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUsePetsPill.Name = "Character_cbxUsePetsPill";
            this.Character_cbxUsePetsPill.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUsePetsPill.Size = new System.Drawing.Size(212, 25);
            this.Character_cbxUsePetsPill.TabIndex = 24;
            this.Character_cbxUsePetsPill.Tag = "Source Sans Pro";
            this.Character_cbxUsePetsPill.Text = "Use abnormal state pills";
            this.Character_cbxUsePetsPill.UseVisualStyleBackColor = false;
            this.Character_cbxUsePetsPill.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_tbxUseTransportHP
            // 
            this.Character_tbxUseTransportHP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Character_tbxUseTransportHP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Character_tbxUseTransportHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_tbxUseTransportHP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_tbxUseTransportHP.Location = new System.Drawing.Point(156, 39);
            this.Character_tbxUseTransportHP.MaxLength = 3;
            this.Character_tbxUseTransportHP.Name = "Character_tbxUseTransportHP";
            this.Character_tbxUseTransportHP.Size = new System.Drawing.Size(35, 23);
            this.Character_tbxUseTransportHP.TabIndex = 23;
            this.Character_tbxUseTransportHP.Tag = "Source Sans Pro";
            this.Character_tbxUseTransportHP.Text = "0";
            this.Character_tbxUseTransportHP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Character_tbxUseTransportHP.TextChanged += new System.EventHandler(this.Control_TextChanged);
            // 
            // Character_cbxUseTransportHP
            // 
            this.Character_cbxUseTransportHP.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUseTransportHP.FlatAppearance.BorderSize = 0;
            this.Character_cbxUseTransportHP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUseTransportHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUseTransportHP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUseTransportHP.Location = new System.Drawing.Point(2, 39);
            this.Character_cbxUseTransportHP.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUseTransportHP.Name = "Character_cbxUseTransportHP";
            this.Character_cbxUseTransportHP.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUseTransportHP.Size = new System.Drawing.Size(212, 25);
            this.Character_cbxUseTransportHP.TabIndex = 22;
            this.Character_cbxUseTransportHP.Tag = "Source Sans Pro";
            this.Character_cbxUseTransportHP.Text = "Use HP on transport               %";
            this.ToolTips.SetToolTip(this.Character_cbxUseTransportHP, "Use health potion when HP from transport is less or equal than");
            this.Character_cbxUseTransportHP.UseVisualStyleBackColor = false;
            this.Character_cbxUseTransportHP.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_tbxUsePetHP
            // 
            this.Character_tbxUsePetHP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Character_tbxUsePetHP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Character_tbxUsePetHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_tbxUsePetHP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_tbxUsePetHP.Location = new System.Drawing.Point(118, 14);
            this.Character_tbxUsePetHP.MaxLength = 3;
            this.Character_tbxUsePetHP.Name = "Character_tbxUsePetHP";
            this.Character_tbxUsePetHP.Size = new System.Drawing.Size(35, 23);
            this.Character_tbxUsePetHP.TabIndex = 21;
            this.Character_tbxUsePetHP.Tag = "Source Sans Pro";
            this.Character_tbxUsePetHP.Text = "0";
            this.Character_tbxUsePetHP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Character_tbxUsePetHP.TextChanged += new System.EventHandler(this.Control_TextChanged);
            // 
            // Character_cbxUsePetHP
            // 
            this.Character_cbxUsePetHP.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUsePetHP.FlatAppearance.BorderSize = 0;
            this.Character_cbxUsePetHP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUsePetHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUsePetHP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUsePetHP.Location = new System.Drawing.Point(2, 14);
            this.Character_cbxUsePetHP.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUsePetHP.Name = "Character_cbxUsePetHP";
            this.Character_cbxUsePetHP.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUsePetHP.Size = new System.Drawing.Size(212, 25);
            this.Character_cbxUsePetHP.TabIndex = 20;
            this.Character_cbxUsePetHP.Tag = "Source Sans Pro";
            this.Character_cbxUsePetHP.Text = "Use HP on pet               %";
            this.ToolTips.SetToolTip(this.Character_cbxUsePetHP, "Use health potion when HP from pet is less or equal than");
            this.Character_cbxUsePetHP.UseVisualStyleBackColor = false;
            this.Character_cbxUsePetHP.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_gbxPotionsPlayer
            // 
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_tbxUseHP);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_tbxUseMP);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_cbxUsePillPurification);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_cbxUseMPGrain);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_cbxUsePillUniversal);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_tbxUseMPVigor);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_tbxUseHPVigor);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_cbxUseMPVigor);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_cbxUseHP);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_cbxUseHPGrain);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_cbxUseHPVigor);
            this.Character_gbxPotionsPlayer.Controls.Add(this.Character_cbxUseMP);
            this.Character_gbxPotionsPlayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_gbxPotionsPlayer.ForeColor = System.Drawing.Color.LightGray;
            this.Character_gbxPotionsPlayer.Location = new System.Drawing.Point(6, 0);
            this.Character_gbxPotionsPlayer.Margin = new System.Windows.Forms.Padding(8, 0, 8, 8);
            this.Character_gbxPotionsPlayer.Name = "Character_gbxPotionsPlayer";
            this.Character_gbxPotionsPlayer.Size = new System.Drawing.Size(169, 216);
            this.Character_gbxPotionsPlayer.TabIndex = 22;
            this.Character_gbxPotionsPlayer.TabStop = false;
            this.Character_gbxPotionsPlayer.Tag = "Source Sans Pro";
            this.Character_gbxPotionsPlayer.Text = "Player";
            // 
            // Character_tbxUseHP
            // 
            this.Character_tbxUseHP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Character_tbxUseHP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Character_tbxUseHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_tbxUseHP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_tbxUseHP.Location = new System.Drawing.Point(77, 14);
            this.Character_tbxUseHP.MaxLength = 3;
            this.Character_tbxUseHP.Name = "Character_tbxUseHP";
            this.Character_tbxUseHP.Size = new System.Drawing.Size(35, 23);
            this.Character_tbxUseHP.TabIndex = 9;
            this.Character_tbxUseHP.Tag = "Source Sans Pro";
            this.Character_tbxUseHP.Text = "0";
            this.Character_tbxUseHP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Character_tbxUseHP.TextChanged += new System.EventHandler(this.Control_TextChanged);
            // 
            // Character_tbxUseMP
            // 
            this.Character_tbxUseMP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Character_tbxUseMP.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Character_tbxUseMP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_tbxUseMP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_tbxUseMP.Location = new System.Drawing.Point(77, 64);
            this.Character_tbxUseMP.MaxLength = 3;
            this.Character_tbxUseMP.Name = "Character_tbxUseMP";
            this.Character_tbxUseMP.Size = new System.Drawing.Size(35, 23);
            this.Character_tbxUseMP.TabIndex = 12;
            this.Character_tbxUseMP.Tag = "Source Sans Pro";
            this.Character_tbxUseMP.Text = "0";
            this.Character_tbxUseMP.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Character_tbxUseMP.TextChanged += new System.EventHandler(this.Control_TextChanged);
            // 
            // Character_cbxUsePillPurification
            // 
            this.Character_cbxUsePillPurification.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUsePillPurification.FlatAppearance.BorderSize = 0;
            this.Character_cbxUsePillPurification.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUsePillPurification.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUsePillPurification.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUsePillPurification.Location = new System.Drawing.Point(2, 189);
            this.Character_cbxUsePillPurification.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUsePillPurification.Name = "Character_cbxUsePillPurification";
            this.Character_cbxUsePillPurification.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUsePillPurification.Size = new System.Drawing.Size(165, 25);
            this.Character_cbxUsePillPurification.TabIndex = 19;
            this.Character_cbxUsePillPurification.Tag = "Source Sans Pro";
            this.Character_cbxUsePillPurification.Text = "Use purification pills";
            this.Character_cbxUsePillPurification.UseVisualStyleBackColor = false;
            this.Character_cbxUsePillPurification.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxUseMPGrain
            // 
            this.Character_cbxUseMPGrain.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUseMPGrain.FlatAppearance.BorderSize = 0;
            this.Character_cbxUseMPGrain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUseMPGrain.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUseMPGrain.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUseMPGrain.Location = new System.Drawing.Point(2, 89);
            this.Character_cbxUseMPGrain.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUseMPGrain.Name = "Character_cbxUseMPGrain";
            this.Character_cbxUseMPGrain.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUseMPGrain.Size = new System.Drawing.Size(165, 25);
            this.Character_cbxUseMPGrain.TabIndex = 13;
            this.Character_cbxUseMPGrain.Tag = "Source Sans Pro";
            this.Character_cbxUseMPGrain.Text = "Use MP grains";
            this.ToolTips.SetToolTip(this.Character_cbxUseMPGrain, "Use mana potion grain if is available");
            this.Character_cbxUseMPGrain.UseVisualStyleBackColor = false;
            this.Character_cbxUseMPGrain.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxUsePillUniversal
            // 
            this.Character_cbxUsePillUniversal.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUsePillUniversal.FlatAppearance.BorderSize = 0;
            this.Character_cbxUsePillUniversal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUsePillUniversal.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUsePillUniversal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUsePillUniversal.Location = new System.Drawing.Point(2, 164);
            this.Character_cbxUsePillUniversal.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUsePillUniversal.Name = "Character_cbxUsePillUniversal";
            this.Character_cbxUsePillUniversal.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUsePillUniversal.Size = new System.Drawing.Size(165, 25);
            this.Character_cbxUsePillUniversal.TabIndex = 18;
            this.Character_cbxUsePillUniversal.Tag = "Source Sans Pro";
            this.Character_cbxUsePillUniversal.Text = "Use universal pills";
            this.Character_cbxUsePillUniversal.UseVisualStyleBackColor = false;
            this.Character_cbxUsePillUniversal.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_tbxUseMPVigor
            // 
            this.Character_tbxUseMPVigor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Character_tbxUseMPVigor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Character_tbxUseMPVigor.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_tbxUseMPVigor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_tbxUseMPVigor.Location = new System.Drawing.Point(108, 139);
            this.Character_tbxUseMPVigor.MaxLength = 3;
            this.Character_tbxUseMPVigor.Name = "Character_tbxUseMPVigor";
            this.Character_tbxUseMPVigor.Size = new System.Drawing.Size(35, 23);
            this.Character_tbxUseMPVigor.TabIndex = 17;
            this.Character_tbxUseMPVigor.Tag = "Source Sans Pro";
            this.Character_tbxUseMPVigor.Text = "0";
            this.Character_tbxUseMPVigor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Character_tbxUseMPVigor.TextChanged += new System.EventHandler(this.Control_TextChanged);
            // 
            // Character_tbxUseHPVigor
            // 
            this.Character_tbxUseHPVigor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.Character_tbxUseHPVigor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Character_tbxUseHPVigor.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_tbxUseHPVigor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_tbxUseHPVigor.Location = new System.Drawing.Point(108, 114);
            this.Character_tbxUseHPVigor.MaxLength = 3;
            this.Character_tbxUseHPVigor.Name = "Character_tbxUseHPVigor";
            this.Character_tbxUseHPVigor.Size = new System.Drawing.Size(35, 23);
            this.Character_tbxUseHPVigor.TabIndex = 15;
            this.Character_tbxUseHPVigor.Tag = "Source Sans Pro";
            this.Character_tbxUseHPVigor.Text = "0";
            this.Character_tbxUseHPVigor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.Character_tbxUseHPVigor.TextChanged += new System.EventHandler(this.Control_TextChanged);
            // 
            // Character_cbxUseMPVigor
            // 
            this.Character_cbxUseMPVigor.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUseMPVigor.FlatAppearance.BorderSize = 0;
            this.Character_cbxUseMPVigor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUseMPVigor.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUseMPVigor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUseMPVigor.Location = new System.Drawing.Point(2, 139);
            this.Character_cbxUseMPVigor.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUseMPVigor.Name = "Character_cbxUseMPVigor";
            this.Character_cbxUseMPVigor.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUseMPVigor.Size = new System.Drawing.Size(165, 25);
            this.Character_cbxUseMPVigor.TabIndex = 16;
            this.Character_cbxUseMPVigor.Tag = "Source Sans Pro";
            this.Character_cbxUseMPVigor.Text = "Use vigor MP              %";
            this.ToolTips.SetToolTip(this.Character_cbxUseMPVigor, "Use vigor potion when MP less or equal than");
            this.Character_cbxUseMPVigor.UseVisualStyleBackColor = false;
            this.Character_cbxUseMPVigor.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxUseHP
            // 
            this.Character_cbxUseHP.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUseHP.FlatAppearance.BorderSize = 0;
            this.Character_cbxUseHP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUseHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUseHP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUseHP.Location = new System.Drawing.Point(2, 14);
            this.Character_cbxUseHP.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUseHP.Name = "Character_cbxUseHP";
            this.Character_cbxUseHP.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUseHP.Size = new System.Drawing.Size(165, 25);
            this.Character_cbxUseHP.TabIndex = 8;
            this.Character_cbxUseHP.Tag = "Source Sans Pro";
            this.Character_cbxUseHP.Text = "Use HP               %";
            this.ToolTips.SetToolTip(this.Character_cbxUseHP, "Use health potion when HP less or equal than");
            this.Character_cbxUseHP.UseVisualStyleBackColor = false;
            this.Character_cbxUseHP.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxUseHPGrain
            // 
            this.Character_cbxUseHPGrain.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUseHPGrain.FlatAppearance.BorderSize = 0;
            this.Character_cbxUseHPGrain.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUseHPGrain.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUseHPGrain.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUseHPGrain.Location = new System.Drawing.Point(2, 39);
            this.Character_cbxUseHPGrain.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUseHPGrain.Name = "Character_cbxUseHPGrain";
            this.Character_cbxUseHPGrain.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUseHPGrain.Size = new System.Drawing.Size(165, 25);
            this.Character_cbxUseHPGrain.TabIndex = 10;
            this.Character_cbxUseHPGrain.Tag = "Source Sans Pro";
            this.Character_cbxUseHPGrain.Text = "Use HP grains";
            this.ToolTips.SetToolTip(this.Character_cbxUseHPGrain, "Use health potion grain if is available");
            this.Character_cbxUseHPGrain.UseVisualStyleBackColor = false;
            this.Character_cbxUseHPGrain.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxUseHPVigor
            // 
            this.Character_cbxUseHPVigor.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUseHPVigor.FlatAppearance.BorderSize = 0;
            this.Character_cbxUseHPVigor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUseHPVigor.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUseHPVigor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUseHPVigor.Location = new System.Drawing.Point(2, 114);
            this.Character_cbxUseHPVigor.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUseHPVigor.Name = "Character_cbxUseHPVigor";
            this.Character_cbxUseHPVigor.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUseHPVigor.Size = new System.Drawing.Size(165, 25);
            this.Character_cbxUseHPVigor.TabIndex = 14;
            this.Character_cbxUseHPVigor.Tag = "Source Sans Pro";
            this.Character_cbxUseHPVigor.Text = "Use vigor HP              %";
            this.ToolTips.SetToolTip(this.Character_cbxUseHPVigor, "Use vigor potion when HP less or equal than");
            this.Character_cbxUseHPVigor.UseVisualStyleBackColor = false;
            this.Character_cbxUseHPVigor.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxUseMP
            // 
            this.Character_cbxUseMP.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxUseMP.FlatAppearance.BorderSize = 0;
            this.Character_cbxUseMP.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxUseMP.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxUseMP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxUseMP.Location = new System.Drawing.Point(2, 64);
            this.Character_cbxUseMP.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxUseMP.Name = "Character_cbxUseMP";
            this.Character_cbxUseMP.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxUseMP.Size = new System.Drawing.Size(165, 25);
            this.Character_cbxUseMP.TabIndex = 11;
            this.Character_cbxUseMP.Tag = "Source Sans Pro";
            this.Character_cbxUseMP.Text = "Use MP               %";
            this.ToolTips.SetToolTip(this.Character_cbxUseMP, "Use mana potion when MP less or equal than");
            this.Character_cbxUseMP.UseVisualStyleBackColor = false;
            this.Character_cbxUseMP.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // TabPageH_Character_Option01_Panel
            // 
            this.TabPageH_Character_Option01_Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_pnlBuffs);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblCoordY);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblCoordX);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblCoords);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblLocation);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblLocationText);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblSP);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblSPText);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblGold);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblGoldText);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblJobLevel);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_pgbJobExp);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_gbxStatPoints);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_lblLevel);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_gbxMessageFilter);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_pgbExp);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_pgbMP);
            this.TabPageH_Character_Option01_Panel.Controls.Add(this.Character_pgbHP);
            this.TabPageH_Character_Option01_Panel.Location = new System.Drawing.Point(0, 27);
            this.TabPageH_Character_Option01_Panel.Name = "TabPageH_Character_Option01_Panel";
            this.TabPageH_Character_Option01_Panel.Size = new System.Drawing.Size(657, 345);
            this.TabPageH_Character_Option01_Panel.TabIndex = 10;
            this.TabPageH_Character_Option01_Panel.Visible = false;
            // 
            // Character_pnlBuffs
            // 
            this.Character_pnlBuffs.AutoScroll = true;
            this.Character_pnlBuffs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Character_pnlBuffs.Location = new System.Drawing.Point(5, 235);
            this.Character_pnlBuffs.Name = "Character_pnlBuffs";
            this.Character_pnlBuffs.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Character_pnlBuffs.Size = new System.Drawing.Size(340, 102);
            this.Character_pnlBuffs.TabIndex = 23;
            this.ToolTips.SetToolTip(this.Character_pnlBuffs, "Active buffs");
            // 
            // Character_lblCoordY
            // 
            this.Character_lblCoordY.AutoEllipsis = true;
            this.Character_lblCoordY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Character_lblCoordY.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblCoordY.Location = new System.Drawing.Point(228, 170);
            this.Character_lblCoordY.Name = "Character_lblCoordY";
            this.Character_lblCoordY.Size = new System.Drawing.Size(117, 20);
            this.Character_lblCoordY.TabIndex = 33;
            this.Character_lblCoordY.Tag = "Source Sans Pro";
            this.Character_lblCoordY.Text = "- - -";
            this.Character_lblCoordY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Character_lblCoordX
            // 
            this.Character_lblCoordX.AutoEllipsis = true;
            this.Character_lblCoordX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Character_lblCoordX.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblCoordX.Location = new System.Drawing.Point(107, 170);
            this.Character_lblCoordX.Name = "Character_lblCoordX";
            this.Character_lblCoordX.Size = new System.Drawing.Size(117, 20);
            this.Character_lblCoordX.TabIndex = 32;
            this.Character_lblCoordX.Tag = "Source Sans Pro";
            this.Character_lblCoordX.Text = "- - -";
            this.Character_lblCoordX.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Character_lblCoords
            // 
            this.Character_lblCoords.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblCoords.Location = new System.Drawing.Point(5, 170);
            this.Character_lblCoords.Name = "Character_lblCoords";
            this.Character_lblCoords.Size = new System.Drawing.Size(102, 20);
            this.Character_lblCoords.TabIndex = 31;
            this.Character_lblCoords.Tag = "Source Sans Pro";
            this.Character_lblCoords.Text = "Coordinates :";
            this.ToolTips.SetToolTip(this.Character_lblCoords, "Game position");
            // 
            // Character_lblLocation
            // 
            this.Character_lblLocation.AutoEllipsis = true;
            this.Character_lblLocation.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Character_lblLocation.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblLocation.Location = new System.Drawing.Point(83, 144);
            this.Character_lblLocation.Name = "Character_lblLocation";
            this.Character_lblLocation.Size = new System.Drawing.Size(262, 20);
            this.Character_lblLocation.TabIndex = 30;
            this.Character_lblLocation.Tag = "Source Sans Pro";
            this.Character_lblLocation.Text = "- - -";
            this.Character_lblLocation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Character_lblLocationText
            // 
            this.Character_lblLocationText.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblLocationText.Location = new System.Drawing.Point(5, 144);
            this.Character_lblLocationText.Name = "Character_lblLocationText";
            this.Character_lblLocationText.Size = new System.Drawing.Size(78, 20);
            this.Character_lblLocationText.TabIndex = 29;
            this.Character_lblLocationText.Tag = "Source Sans Pro";
            this.Character_lblLocationText.Text = "Location :";
            this.ToolTips.SetToolTip(this.Character_lblLocationText, "World location");
            // 
            // Character_lblSP
            // 
            this.Character_lblSP.AutoEllipsis = true;
            this.Character_lblSP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Character_lblSP.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblSP.Location = new System.Drawing.Point(234, 118);
            this.Character_lblSP.Name = "Character_lblSP";
            this.Character_lblSP.Size = new System.Drawing.Size(111, 20);
            this.Character_lblSP.TabIndex = 28;
            this.Character_lblSP.Tag = "Source Sans Pro";
            this.Character_lblSP.Text = "- - -";
            this.Character_lblSP.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Character_lblSPText
            // 
            this.Character_lblSPText.AutoSize = true;
            this.Character_lblSPText.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblSPText.Location = new System.Drawing.Point(198, 118);
            this.Character_lblSPText.Name = "Character_lblSPText";
            this.Character_lblSPText.Size = new System.Drawing.Size(42, 20);
            this.Character_lblSPText.TabIndex = 27;
            this.Character_lblSPText.Tag = "Source Sans Pro";
            this.Character_lblSPText.Text = "SP :";
            this.ToolTips.SetToolTip(this.Character_lblSPText, "Skill Points");
            // 
            // Character_lblGold
            // 
            this.Character_lblGold.AutoEllipsis = true;
            this.Character_lblGold.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Character_lblGold.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblGold.Location = new System.Drawing.Point(55, 118);
            this.Character_lblGold.Name = "Character_lblGold";
            this.Character_lblGold.Size = new System.Drawing.Size(140, 20);
            this.Character_lblGold.TabIndex = 26;
            this.Character_lblGold.Tag = "Source Sans Pro";
            this.Character_lblGold.Text = "- - -";
            this.Character_lblGold.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Character_lblGoldText
            // 
            this.Character_lblGoldText.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblGoldText.Location = new System.Drawing.Point(5, 118);
            this.Character_lblGoldText.Name = "Character_lblGoldText";
            this.Character_lblGoldText.Size = new System.Drawing.Size(50, 20);
            this.Character_lblGoldText.TabIndex = 25;
            this.Character_lblGoldText.Tag = "Source Sans Pro";
            this.Character_lblGoldText.Text = "Gold :";
            this.ToolTips.SetToolTip(this.Character_lblGoldText, "Gold at inventory");
            // 
            // Character_lblJobLevel
            // 
            this.Character_lblJobLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblJobLevel.Location = new System.Drawing.Point(5, 90);
            this.Character_lblJobLevel.Name = "Character_lblJobLevel";
            this.Character_lblJobLevel.Size = new System.Drawing.Size(90, 22);
            this.Character_lblJobLevel.TabIndex = 24;
            this.Character_lblJobLevel.Tag = "Source Sans Pro";
            this.Character_lblJobLevel.Text = "Job Lv. - - -";
            this.ToolTips.SetToolTip(this.Character_lblJobLevel, "Job Level");
            // 
            // Character_pgbJobExp
            // 
            this.Character_pgbJobExp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(119)))), ((int)(((byte)(73)))), ((int)(((byte)(33)))));
            this.Character_pgbJobExp.BackColorDegradationLevel = 4;
            this.Character_pgbJobExp.Display = xGraphics.xProgressBarDisplay.Percentage;
            this.Character_pgbJobExp.DisplayShadow = System.Drawing.Color.Black;
            this.Character_pgbJobExp.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_pgbJobExp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_pgbJobExp.Location = new System.Drawing.Point(95, 90);
            this.Character_pgbJobExp.Name = "Character_pgbJobExp";
            this.Character_pgbJobExp.Size = new System.Drawing.Size(250, 22);
            this.Character_pgbJobExp.TabIndex = 23;
            this.Character_pgbJobExp.Tag = "Source Sans Pro";
            this.ToolTips.SetToolTip(this.Character_pgbJobExp, "Job Experience");
            this.Character_pgbJobExp.Value = ((ulong)(0ul));
            this.Character_pgbJobExp.ValueMaximum = ((ulong)(0ul));
            // 
            // Character_gbxStatPoints
            // 
            this.Character_gbxStatPoints.Controls.Add(this.Character_lblStatPoints);
            this.Character_gbxStatPoints.Controls.Add(this.Character_lblINT);
            this.Character_gbxStatPoints.Controls.Add(this.Character_lblSTR);
            this.Character_gbxStatPoints.Controls.Add(this.Character_btnAddSTR);
            this.Character_gbxStatPoints.Controls.Add(this.Character_lblAddINT);
            this.Character_gbxStatPoints.Controls.Add(this.Character_lblAddSTR);
            this.Character_gbxStatPoints.Controls.Add(this.Character_btnAddINT);
            this.Character_gbxStatPoints.Controls.Add(this.Character_cbxAutoStat);
            this.Character_gbxStatPoints.Controls.Add(this.Character_rbnAutoSTR);
            this.Character_gbxStatPoints.Controls.Add(this.Character_rbnAutoINT);
            this.Character_gbxStatPoints.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_gbxStatPoints.ForeColor = System.Drawing.Color.LightGray;
            this.Character_gbxStatPoints.Location = new System.Drawing.Point(5, 190);
            this.Character_gbxStatPoints.Margin = new System.Windows.Forms.Padding(8, 0, 8, 8);
            this.Character_gbxStatPoints.Name = "Character_gbxStatPoints";
            this.Character_gbxStatPoints.Size = new System.Drawing.Size(340, 75);
            this.Character_gbxStatPoints.TabIndex = 20;
            this.Character_gbxStatPoints.TabStop = false;
            this.Character_gbxStatPoints.Tag = "Source Sans Pro";
            this.Character_gbxStatPoints.Text = "Stat Points & Auto Distribution";
            // 
            // Character_cbxAutoStat
            // 
            this.Character_cbxAutoStat.AutoSize = true;
            this.Character_cbxAutoStat.Checked = true;
            this.Character_cbxAutoStat.CheckState = System.Windows.Forms.CheckState.Checked;
            this.Character_cbxAutoStat.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Character_cbxAutoStat.Location = new System.Drawing.Point(10, 48);
            this.Character_cbxAutoStat.Name = "Character_cbxAutoStat";
            this.Character_cbxAutoStat.Size = new System.Drawing.Size(78, 20);
            this.Character_cbxAutoStat.TabIndex = 25;
            this.Character_cbxAutoStat.Text = "Auto Stat:";
            this.Character_cbxAutoStat.UseVisualStyleBackColor = true;
            // 
            // Character_rbnAutoSTR
            // 
            this.Character_rbnAutoSTR.AutoSize = true;
            this.Character_rbnAutoSTR.Checked = true;
            this.Character_rbnAutoSTR.ForeColor = System.Drawing.Color.White;
            this.Character_rbnAutoSTR.Location = new System.Drawing.Point(100, 48);
            this.Character_rbnAutoSTR.Name = "Character_rbnAutoSTR";
            this.Character_rbnAutoSTR.Size = new System.Drawing.Size(81, 20);
            this.Character_rbnAutoSTR.TabIndex = 26;
            this.Character_rbnAutoSTR.TabStop = true;
            this.Character_rbnAutoSTR.Text = "Pure STR";
            this.Character_rbnAutoSTR.UseVisualStyleBackColor = true;
            // 
            // Character_rbnAutoINT
            // 
            this.Character_rbnAutoINT.AutoSize = true;
            this.Character_rbnAutoINT.ForeColor = System.Drawing.Color.White;
            this.Character_rbnAutoINT.Location = new System.Drawing.Point(195, 48);
            this.Character_rbnAutoINT.Name = "Character_rbnAutoINT";
            this.Character_rbnAutoINT.Size = new System.Drawing.Size(78, 20);
            this.Character_rbnAutoINT.TabIndex = 27;
            this.Character_rbnAutoINT.Text = "Pure INT";
            this.Character_rbnAutoINT.UseVisualStyleBackColor = true;
            // 
            // Character_lblStatPoints
            // 
            this.Character_lblStatPoints.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Character_lblStatPoints.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblStatPoints.Location = new System.Drawing.Point(250, 15);
            this.Character_lblStatPoints.Name = "Character_lblStatPoints";
            this.Character_lblStatPoints.Size = new System.Drawing.Size(84, 22);
            this.Character_lblStatPoints.TabIndex = 23;
            this.Character_lblStatPoints.Tag = "Source Sans Pro";
            this.Character_lblStatPoints.Text = "- - -";
            this.Character_lblStatPoints.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ToolTips.SetToolTip(this.Character_lblStatPoints, "Stat points remain");
            // 
            // Character_lblINT
            // 
            this.Character_lblINT.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblINT.Location = new System.Drawing.Point(173, 15);
            this.Character_lblINT.Name = "Character_lblINT";
            this.Character_lblINT.Size = new System.Drawing.Size(45, 22);
            this.Character_lblINT.TabIndex = 24;
            this.Character_lblINT.Tag = "Source Sans Pro";
            this.Character_lblINT.Text = "- - -";
            this.Character_lblINT.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Character_lblSTR
            // 
            this.Character_lblSTR.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblSTR.Location = new System.Drawing.Point(51, 15);
            this.Character_lblSTR.Name = "Character_lblSTR";
            this.Character_lblSTR.Size = new System.Drawing.Size(45, 22);
            this.Character_lblSTR.TabIndex = 23;
            this.Character_lblSTR.Tag = "Source Sans Pro";
            this.Character_lblSTR.Text = "- - -";
            this.Character_lblSTR.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // Character_btnAddSTR
            // 
            this.Character_btnAddSTR.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Character_btnAddSTR.Enabled = false;
            this.Character_btnAddSTR.FlatAppearance.BorderSize = 0;
            this.Character_btnAddSTR.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Character_btnAddSTR.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(64)))));
            this.Character_btnAddSTR.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_btnAddSTR.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_btnAddSTR.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_btnAddSTR.Location = new System.Drawing.Point(96, 13);
            this.Character_btnAddSTR.Margin = new System.Windows.Forms.Padding(0);
            this.Character_btnAddSTR.Name = "Character_btnAddSTR";
            this.Character_btnAddSTR.Size = new System.Drawing.Size(28, 26);
            this.Character_btnAddSTR.TabIndex = 22;
            this.Character_btnAddSTR.Tag = "Font Awesome 5 Pro Regular";
            this.Character_btnAddSTR.Text = "";
            this.ToolTips.SetToolTip(this.Character_btnAddSTR, "Add STR");
            this.Character_btnAddSTR.UseCompatibleTextRendering = true;
            this.Character_btnAddSTR.UseVisualStyleBackColor = false;
            this.Character_btnAddSTR.Click += new System.EventHandler(this.Control_Click);
            this.Character_btnAddSTR.MouseEnter += new System.EventHandler(this.Control_Focus_Enter);
            this.Character_btnAddSTR.MouseLeave += new System.EventHandler(this.Control_Focus_Leave);
            // 
            // Character_lblAddINT
            // 
            this.Character_lblAddINT.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblAddINT.Location = new System.Drawing.Point(128, 15);
            this.Character_lblAddINT.Name = "Character_lblAddINT";
            this.Character_lblAddINT.Size = new System.Drawing.Size(45, 22);
            this.Character_lblAddINT.TabIndex = 21;
            this.Character_lblAddINT.Tag = "Source Sans Pro";
            this.Character_lblAddINT.Text = "INT :";
            this.Character_lblAddINT.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolTips.SetToolTip(this.Character_lblAddINT, "Intelligence");
            // 
            // Character_lblAddSTR
            // 
            this.Character_lblAddSTR.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblAddSTR.Location = new System.Drawing.Point(6, 15);
            this.Character_lblAddSTR.Name = "Character_lblAddSTR";
            this.Character_lblAddSTR.Size = new System.Drawing.Size(45, 22);
            this.Character_lblAddSTR.TabIndex = 20;
            this.Character_lblAddSTR.Tag = "Source Sans Pro";
            this.Character_lblAddSTR.Text = "STR :";
            this.Character_lblAddSTR.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.ToolTips.SetToolTip(this.Character_lblAddSTR, "Strength");
            // 
            // Character_btnAddINT
            // 
            this.Character_btnAddINT.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Character_btnAddINT.Enabled = false;
            this.Character_btnAddINT.FlatAppearance.BorderSize = 0;
            this.Character_btnAddINT.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Character_btnAddINT.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(64)))));
            this.Character_btnAddINT.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_btnAddINT.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_btnAddINT.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_btnAddINT.Location = new System.Drawing.Point(218, 13);
            this.Character_btnAddINT.Margin = new System.Windows.Forms.Padding(0);
            this.Character_btnAddINT.Name = "Character_btnAddINT";
            this.Character_btnAddINT.Size = new System.Drawing.Size(28, 26);
            this.Character_btnAddINT.TabIndex = 2;
            this.Character_btnAddINT.Tag = "Font Awesome 5 Pro Regular";
            this.Character_btnAddINT.Text = "";
            this.ToolTips.SetToolTip(this.Character_btnAddINT, "Add INT");
            this.Character_btnAddINT.UseCompatibleTextRendering = true;
            this.Character_btnAddINT.UseVisualStyleBackColor = false;
            this.Character_btnAddINT.Click += new System.EventHandler(this.Control_Click);
            this.Character_btnAddINT.MouseEnter += new System.EventHandler(this.Control_Focus_Enter);
            this.Character_btnAddINT.MouseLeave += new System.EventHandler(this.Control_Focus_Leave);
            // 
            // Character_lblLevel
            // 
            this.Character_lblLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel);
            this.Character_lblLevel.Location = new System.Drawing.Point(5, 62);
            this.Character_lblLevel.Name = "Character_lblLevel";
            this.Character_lblLevel.Size = new System.Drawing.Size(55, 22);
            this.Character_lblLevel.TabIndex = 19;
            this.Character_lblLevel.Tag = "Source Sans Pro";
            this.Character_lblLevel.Text = "Lv. - - -";
            this.ToolTips.SetToolTip(this.Character_lblLevel, "Level");
            // 
            // Character_gbxMessageFilter
            // 
            this.Character_gbxMessageFilter.Controls.Add(this.Character_cbxMessageEvents);
            this.Character_gbxMessageFilter.Controls.Add(this.Character_cbxMessagePicks);
            this.Character_gbxMessageFilter.Controls.Add(this.Character_cbxMessageUniques);
            this.Character_gbxMessageFilter.Controls.Add(this.Character_cbxMessageExp);
            this.Character_gbxMessageFilter.Controls.Add(this.Character_rtbxMessageFilter);
            this.Character_gbxMessageFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Character_gbxMessageFilter.ForeColor = System.Drawing.Color.LightGray;
            this.Character_gbxMessageFilter.Location = new System.Drawing.Point(350, 0);
            this.Character_gbxMessageFilter.Margin = new System.Windows.Forms.Padding(8, 0, 8, 8);
            this.Character_gbxMessageFilter.Name = "Character_gbxMessageFilter";
            this.Character_gbxMessageFilter.Size = new System.Drawing.Size(300, 337);
            this.Character_gbxMessageFilter.TabIndex = 18;
            this.Character_gbxMessageFilter.TabStop = false;
            this.Character_gbxMessageFilter.Tag = "Source Sans Pro";
            this.Character_gbxMessageFilter.Text = "Message filtering";
            // 
            // Character_cbxMessageEvents
            // 
            this.Character_cbxMessageEvents.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxMessageEvents.FlatAppearance.BorderSize = 0;
            this.Character_cbxMessageEvents.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxMessageEvents.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Strikeout, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxMessageEvents.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxMessageEvents.Location = new System.Drawing.Point(207, 14);
            this.Character_cbxMessageEvents.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxMessageEvents.Name = "Character_cbxMessageEvents";
            this.Character_cbxMessageEvents.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxMessageEvents.Size = new System.Drawing.Size(74, 24);
            this.Character_cbxMessageEvents.TabIndex = 22;
            this.Character_cbxMessageEvents.Tag = "Source Sans Pro";
            this.Character_cbxMessageEvents.Text = "Events";
            this.ToolTips.SetToolTip(this.Character_cbxMessageEvents, "Event notices");
            this.Character_cbxMessageEvents.UseVisualStyleBackColor = false;
            this.Character_cbxMessageEvents.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxMessagePicks
            // 
            this.Character_cbxMessagePicks.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxMessagePicks.FlatAppearance.BorderSize = 0;
            this.Character_cbxMessagePicks.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxMessagePicks.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxMessagePicks.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxMessagePicks.Location = new System.Drawing.Point(60, 14);
            this.Character_cbxMessagePicks.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxMessagePicks.Name = "Character_cbxMessagePicks";
            this.Character_cbxMessagePicks.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxMessagePicks.Size = new System.Drawing.Size(64, 24);
            this.Character_cbxMessagePicks.TabIndex = 21;
            this.Character_cbxMessagePicks.Tag = "Source Sans Pro";
            this.Character_cbxMessagePicks.Text = "Picks";
            this.ToolTips.SetToolTip(this.Character_cbxMessagePicks, "Picked Drops");
            this.Character_cbxMessagePicks.UseVisualStyleBackColor = false;
            this.Character_cbxMessagePicks.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxMessageUniques
            // 
            this.Character_cbxMessageUniques.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxMessageUniques.FlatAppearance.BorderSize = 0;
            this.Character_cbxMessageUniques.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxMessageUniques.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxMessageUniques.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxMessageUniques.Location = new System.Drawing.Point(124, 14);
            this.Character_cbxMessageUniques.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxMessageUniques.Name = "Character_cbxMessageUniques";
            this.Character_cbxMessageUniques.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxMessageUniques.Size = new System.Drawing.Size(83, 24);
            this.Character_cbxMessageUniques.TabIndex = 20;
            this.Character_cbxMessageUniques.Tag = "Source Sans Pro";
            this.Character_cbxMessageUniques.Text = "Uniques";
            this.ToolTips.SetToolTip(this.Character_cbxMessageUniques, "Unique spawns");
            this.Character_cbxMessageUniques.UseVisualStyleBackColor = false;
            this.Character_cbxMessageUniques.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_cbxMessageExp
            // 
            this.Character_cbxMessageExp.Cursor = System.Windows.Forms.Cursors.Default;
            this.Character_cbxMessageExp.FlatAppearance.BorderSize = 0;
            this.Character_cbxMessageExp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Character_cbxMessageExp.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_cbxMessageExp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_cbxMessageExp.Location = new System.Drawing.Point(2, 14);
            this.Character_cbxMessageExp.Margin = new System.Windows.Forms.Padding(0);
            this.Character_cbxMessageExp.Name = "Character_cbxMessageExp";
            this.Character_cbxMessageExp.Padding = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.Character_cbxMessageExp.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.Character_cbxMessageExp.Size = new System.Drawing.Size(58, 24);
            this.Character_cbxMessageExp.TabIndex = 19;
            this.Character_cbxMessageExp.Tag = "Source Sans Pro";
            this.Character_cbxMessageExp.Text = "Exp.";
            this.ToolTips.SetToolTip(this.Character_cbxMessageExp, "Experience & SP Experience");
            this.Character_cbxMessageExp.UseVisualStyleBackColor = false;
            this.Character_cbxMessageExp.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Character_rtbxMessageFilter
            // 
            this.Character_rtbxMessageFilter.AutoScroll = true;
            this.Character_rtbxMessageFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Character_rtbxMessageFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Character_rtbxMessageFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Character_rtbxMessageFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_rtbxMessageFilter.Location = new System.Drawing.Point(1, 40);
            this.Character_rtbxMessageFilter.Margin = new System.Windows.Forms.Padding(1);
            this.Character_rtbxMessageFilter.MaxLines = 2048;
            this.Character_rtbxMessageFilter.Name = "Character_rtbxMessageFilter";
            this.Character_rtbxMessageFilter.ReadOnly = true;
            this.Character_rtbxMessageFilter.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.Character_rtbxMessageFilter.Size = new System.Drawing.Size(298, 297);
            this.Character_rtbxMessageFilter.TabIndex = 6;
            this.Character_rtbxMessageFilter.TabStop = false;
            this.Character_rtbxMessageFilter.Tag = "Source Sans Pro";
            this.Character_rtbxMessageFilter.Text = "";
            // 
            // Character_pgbExp
            // 
            this.Character_pgbExp.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(128)))), ((int)(((byte)(10)))));
            this.Character_pgbExp.BackColorDegradationLevel = 5;
            this.Character_pgbExp.Display = xGraphics.xProgressBarDisplay.Percentage;
            this.Character_pgbExp.DisplayShadow = System.Drawing.Color.Black;
            this.Character_pgbExp.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_pgbExp.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_pgbExp.Location = new System.Drawing.Point(60, 62);
            this.Character_pgbExp.Name = "Character_pgbExp";
            this.Character_pgbExp.Size = new System.Drawing.Size(285, 22);
            this.Character_pgbExp.TabIndex = 5;
            this.Character_pgbExp.Tag = "Source Sans Pro";
            this.ToolTips.SetToolTip(this.Character_pgbExp, "Experience");
            this.Character_pgbExp.Value = ((ulong)(0ul));
            this.Character_pgbExp.ValueMaximum = ((ulong)(0ul));
            // 
            // Character_pgbMP
            // 
            this.Character_pgbMP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(10)))), ((int)(((byte)(10)))), ((int)(((byte)(255)))));
            this.Character_pgbMP.BackColorDegradationLevel = 6;
            this.Character_pgbMP.Display = xGraphics.xProgressBarDisplay.Percentage;
            this.Character_pgbMP.DisplayShadow = System.Drawing.Color.Black;
            this.Character_pgbMP.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_pgbMP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_pgbMP.Location = new System.Drawing.Point(5, 34);
            this.Character_pgbMP.Name = "Character_pgbMP";
            this.Character_pgbMP.Size = new System.Drawing.Size(340, 22);
            this.Character_pgbMP.TabIndex = 4;
            this.Character_pgbMP.Tag = "Source Sans Pro";
            this.ToolTips.SetToolTip(this.Character_pgbMP, "Mana Points");
            this.Character_pgbMP.Value = ((ulong)(0ul));
            this.Character_pgbMP.ValueMaximum = ((ulong)(0ul));
            this.Character_pgbMP.Click += new System.EventHandler(this.Control_Click);
            // 
            // Character_pgbHP
            // 
            this.Character_pgbHP.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(10)))), ((int)(((byte)(10)))));
            this.Character_pgbHP.BackColorDegradationLevel = 6;
            this.Character_pgbHP.Display = xGraphics.xProgressBarDisplay.Percentage;
            this.Character_pgbHP.DisplayShadow = System.Drawing.Color.Black;
            this.Character_pgbHP.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Character_pgbHP.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Character_pgbHP.Location = new System.Drawing.Point(5, 6);
            this.Character_pgbHP.Name = "Character_pgbHP";
            this.Character_pgbHP.Size = new System.Drawing.Size(340, 22);
            this.Character_pgbHP.TabIndex = 3;
            this.Character_pgbHP.Tag = "Source Sans Pro";
            this.ToolTips.SetToolTip(this.Character_pgbHP, "Health Points");
            this.Character_pgbHP.Value = ((ulong)(0ul));
            this.Character_pgbHP.ValueMaximum = ((ulong)(0ul));
            this.Character_pgbHP.Click += new System.EventHandler(this.Control_Click);
            // 
            // TabPageH_Character_Option03_Panel
            // 
            this.TabPageH_Character_Option03_Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TabPageH_Character_Option03_Panel.Location = new System.Drawing.Point(0, 27);
            this.TabPageH_Character_Option03_Panel.Name = "TabPageH_Character_Option03_Panel";
            this.TabPageH_Character_Option03_Panel.Size = new System.Drawing.Size(657, 345);
            this.TabPageH_Character_Option03_Panel.TabIndex = 13;
            this.TabPageH_Character_Option03_Panel.Visible = false;
		}
	}
}