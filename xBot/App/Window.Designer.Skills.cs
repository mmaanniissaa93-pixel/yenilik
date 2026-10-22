using System;
using System.Drawing;
using System.Windows.Forms;
using xBot.Game.Objects;
using xGraphics;

namespace xBot.App
{
	partial class Window
	{
		private void InitializeComponent_Skills()
		{
            // TabPageV_Control01_Skills_Panel
            // 
            this.TabPageV_Control01_Skills_Panel.Controls.Add(this.TabPageH_Skills);
            this.TabPageV_Control01_Skills_Panel.Controls.Add(this.Skills_lstvSkills);
            this.TabPageV_Control01_Skills_Panel.Controls.Add(this.TabPageH_Skills_Option01_Panel);
            this.TabPageV_Control01_Skills_Panel.Controls.Add(this.TabPageH_Skills_Option02_Panel);
            this.TabPageV_Control01_Skills_Panel.Controls.Add(this.TabPageH_Skills_Option03_Panel);
            this.TabPageV_Control01_Skills_Panel.Location = new System.Drawing.Point(135, 45);
            this.TabPageV_Control01_Skills_Panel.Name = "TabPageV_Control01_Skills_Panel";
            this.TabPageV_Control01_Skills_Panel.Size = new System.Drawing.Size(657, 372);
            this.TabPageV_Control01_Skills_Panel.TabIndex = 12;
            this.TabPageV_Control01_Skills_Panel.Visible = false;
            // 
            // TabPageH_Skills
            // 
            this.TabPageH_Skills.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.TabPageH_Skills.Controls.Add(this.TabPageH_Skills_Option03);
            this.TabPageH_Skills.Controls.Add(this.TabPageH_Skills_Option02);
            this.TabPageH_Skills.Controls.Add(this.TabPageH_Skills_Option01);
            this.TabPageH_Skills.Location = new System.Drawing.Point(185, 0);
            this.TabPageH_Skills.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Skills.Name = "TabPageH_Skills";
            this.TabPageH_Skills.Size = new System.Drawing.Size(472, 28);
            this.TabPageH_Skills.TabIndex = 25;
            // 
            // TabPageH_Skills_Option03
            // 
            this.TabPageH_Skills_Option03.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TabPageH_Skills_Option03.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.TabPageH_Skills_Option03.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Skills_Option03.FlatAppearance.BorderSize = 0;
            this.TabPageH_Skills_Option03.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.TabPageH_Skills_Option03.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Skills_Option03.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TabPageH_Skills_Option03.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TabPageH_Skills_Option03.Location = new System.Drawing.Point(315, 0);
            this.TabPageH_Skills_Option03.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Skills_Option03.Name = "TabPageH_Skills_Option03";
            this.TabPageH_Skills_Option03.Size = new System.Drawing.Size(157, 26);
            this.TabPageH_Skills_Option03.TabIndex = 14;
            this.TabPageH_Skills_Option03.Tag = "Source Sans Pro";
            this.TabPageH_Skills_Option03.Text = "Party Buff";
            this.TabPageH_Skills_Option03.UseVisualStyleBackColor = false;
            this.TabPageH_Skills_Option03.Click += new System.EventHandler(this.TabPageH_Option_Click);
            // 
            // TabPageH_Skills_Option02
            // 
            this.TabPageH_Skills_Option02.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TabPageH_Skills_Option02.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.TabPageH_Skills_Option02.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Skills_Option02.FlatAppearance.BorderSize = 0;
            this.TabPageH_Skills_Option02.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.TabPageH_Skills_Option02.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Skills_Option02.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TabPageH_Skills_Option02.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TabPageH_Skills_Option02.Location = new System.Drawing.Point(158, 0);
            this.TabPageH_Skills_Option02.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Skills_Option02.Name = "TabPageH_Skills_Option02";
            this.TabPageH_Skills_Option02.Size = new System.Drawing.Size(157, 26);
            this.TabPageH_Skills_Option02.TabIndex = 13;
            this.TabPageH_Skills_Option02.Tag = "Source Sans Pro";
            this.TabPageH_Skills_Option02.Text = "Buff";
            this.TabPageH_Skills_Option02.UseVisualStyleBackColor = false;
            this.TabPageH_Skills_Option02.Click += new System.EventHandler(this.TabPageH_Option_Click);
            // 
            // TabPageH_Skills_Option01
            // 
            this.TabPageH_Skills_Option01.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.TabPageH_Skills_Option01.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(48)))));
            this.TabPageH_Skills_Option01.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Skills_Option01.FlatAppearance.BorderSize = 0;
            this.TabPageH_Skills_Option01.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.TabPageH_Skills_Option01.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            this.TabPageH_Skills_Option01.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.TabPageH_Skills_Option01.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.TabPageH_Skills_Option01.Location = new System.Drawing.Point(0, 0);
            this.TabPageH_Skills_Option01.Margin = new System.Windows.Forms.Padding(0);
            this.TabPageH_Skills_Option01.Name = "TabPageH_Skills_Option01";
            this.TabPageH_Skills_Option01.Size = new System.Drawing.Size(158, 26);
            this.TabPageH_Skills_Option01.TabIndex = 12;
            this.TabPageH_Skills_Option01.Tag = "Source Sans Pro";
            this.TabPageH_Skills_Option01.Text = "Attack";
            this.TabPageH_Skills_Option01.UseVisualStyleBackColor = false;
            this.TabPageH_Skills_Option01.Click += new System.EventHandler(this.TabPageH_Option_Click);
            // 
            // Skills_lstvSkills
            // 
            this.Skills_lstvSkills.AllowDrop = true;
            this.Skills_lstvSkills.AllowReorder = false;
            this.Skills_lstvSkills.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvSkills.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvSkills.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader34});
            this.Skills_lstvSkills.DragDropRemoveFromSource = true;
            this.Skills_lstvSkills.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Skills_lstvSkills.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvSkills.FullRowSelect = true;
            this.Skills_lstvSkills.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvSkills.HideSelection = false;
            this.Skills_lstvSkills.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvSkills.Location = new System.Drawing.Point(0, 0);
            this.Skills_lstvSkills.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvSkills.Name = "Skills_lstvSkills";
            this.Skills_lstvSkills.ShowItemToolTips = true;
            this.Skills_lstvSkills.Size = new System.Drawing.Size(185, 372);
            this.Skills_lstvSkills.SmallImageList = this.lstimgIcons;
            this.Skills_lstvSkills.TabIndex = 26;
            this.Skills_lstvSkills.Tag = "Source Sans Pro";
            this.Skills_lstvSkills.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvSkills.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvSkills.View = System.Windows.Forms.View.Details;
            this.Skills_lstvSkills.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_Cancel);
            // 
            // columnHeader34
            // 
            this.columnHeader34.Text = "Skill name";
            this.columnHeader34.Width = 168;
            // 
            // TabPageH_Skills_Option01_Panel
            // 
            this.TabPageH_Skills_Option01_Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_cbxCastInOrder);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_btnAddAttack);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_btnRemAttack);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Training_cbxWalkToCenter);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_cmbxAttackMobType);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_lstvAttackMobType_General);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_lstvAttackMobType_Unique);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_lstvAttackMobType_Elite);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_lstvAttackMobType_PartyGiant);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_lstvAttackMobType_PartyChampion);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_lstvAttackMobType_PartyGeneral);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_lstvAttackMobType_Giant);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_lstvAttackMobType_Champion);
            this.TabPageH_Skills_Option01_Panel.Controls.Add(this.Skills_lstvAttackMobType_Event);
            this.TabPageH_Skills_Option01_Panel.Location = new System.Drawing.Point(184, 27);
            this.TabPageH_Skills_Option01_Panel.Name = "TabPageH_Skills_Option01_Panel";
            this.TabPageH_Skills_Option01_Panel.Size = new System.Drawing.Size(473, 345);
            this.TabPageH_Skills_Option01_Panel.TabIndex = 28;
            this.TabPageH_Skills_Option01_Panel.Visible = false;
            // 
            // Skills_cbxCastInOrder
            // 
            this.Skills_cbxCastInOrder.Cursor = System.Windows.Forms.Cursors.Default;
            this.Skills_cbxCastInOrder.FlatAppearance.BorderSize = 0;
            this.Skills_cbxCastInOrder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Skills_cbxCastInOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Skills_cbxCastInOrder.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_cbxCastInOrder.Location = new System.Drawing.Point(197, 7);
            this.Skills_cbxCastInOrder.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_cbxCastInOrder.Name = "Skills_cbxCastInOrder";
            this.Skills_cbxCastInOrder.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.Skills_cbxCastInOrder.Size = new System.Drawing.Size(139, 25);
            this.Skills_cbxCastInOrder.TabIndex = 46;
            this.Skills_cbxCastInOrder.Tag = "Source Sans Pro";
            this.Skills_cbxCastInOrder.Text = "Cast skills in order";
            this.Skills_cbxCastInOrder.UseVisualStyleBackColor = false;
            // 
            // Skills_btnAddAttack
            // 
            this.Skills_btnAddAttack.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Skills_btnAddAttack.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.Skills_btnAddAttack.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_btnAddAttack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(64)))));
            this.Skills_btnAddAttack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Skills_btnAddAttack.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_btnAddAttack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_btnAddAttack.Location = new System.Drawing.Point(6, 7);
            this.Skills_btnAddAttack.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_btnAddAttack.Name = "Skills_btnAddAttack";
            this.Skills_btnAddAttack.Size = new System.Drawing.Size(26, 26);
            this.Skills_btnAddAttack.TabIndex = 45;
            this.Skills_btnAddAttack.Tag = "Font Awesome 5 Pro Regular";
            this.Skills_btnAddAttack.Text = "";
            this.ToolTips.SetToolTip(this.Skills_btnAddAttack, "Add attacking skill");
            this.Skills_btnAddAttack.UseCompatibleTextRendering = true;
            this.Skills_btnAddAttack.UseVisualStyleBackColor = false;
            this.Skills_btnAddAttack.Click += new System.EventHandler(this.Control_Click);
            // 
            // Skills_btnRemAttack
            // 
            this.Skills_btnRemAttack.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Skills_btnRemAttack.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.Skills_btnRemAttack.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_btnRemAttack.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(64)))));
            this.Skills_btnRemAttack.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Skills_btnRemAttack.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Skills_btnRemAttack.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_btnRemAttack.Location = new System.Drawing.Point(31, 7);
            this.Skills_btnRemAttack.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_btnRemAttack.Name = "Skills_btnRemAttack";
            this.Skills_btnRemAttack.Size = new System.Drawing.Size(26, 26);
            this.Skills_btnRemAttack.TabIndex = 44;
            this.Skills_btnRemAttack.Tag = "Font Awesome 5 Pro Regular";
            this.Skills_btnRemAttack.Text = "";
            this.ToolTips.SetToolTip(this.Skills_btnRemAttack, "Remove attacking skill");
            this.Skills_btnRemAttack.UseCompatibleTextRendering = true;
            this.Skills_btnRemAttack.UseVisualStyleBackColor = false;
            this.Skills_btnRemAttack.Click += new System.EventHandler(this.Control_Click);
            // 
            // Training_cbxWalkToCenter
            // 
            this.Training_cbxWalkToCenter.Cursor = System.Windows.Forms.Cursors.Default;
            this.Training_cbxWalkToCenter.FlatAppearance.BorderSize = 0;
            this.Training_cbxWalkToCenter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Training_cbxWalkToCenter.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Training_cbxWalkToCenter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Training_cbxWalkToCenter.Location = new System.Drawing.Point(6, 176);
            this.Training_cbxWalkToCenter.Margin = new System.Windows.Forms.Padding(0);
            this.Training_cbxWalkToCenter.Name = "Training_cbxWalkToCenter";
            this.Training_cbxWalkToCenter.Padding = new System.Windows.Forms.Padding(6, 0, 0, 0);
            this.Training_cbxWalkToCenter.Size = new System.Drawing.Size(130, 25);
            this.Training_cbxWalkToCenter.TabIndex = 43;
            this.Training_cbxWalkToCenter.Tag = "Source Sans Pro";
            this.Training_cbxWalkToCenter.Text = "Walk to center";
            this.Training_cbxWalkToCenter.UseVisualStyleBackColor = false;
            this.Training_cbxWalkToCenter.CheckedChanged += new System.EventHandler(this.Control_CheckedChanged);
            // 
            // Skills_cmbxAttackMobType
            // 
            this.Skills_cmbxAttackMobType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_cmbxAttackMobType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Skills_cmbxAttackMobType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Skills_cmbxAttackMobType.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Skills_cmbxAttackMobType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_cmbxAttackMobType.FormattingEnabled = true;
            this.Skills_cmbxAttackMobType.Items.AddRange(new object[] {
            "General",
            "Champion",
            "Giant",
            "PartyGeneral",
            "PartyChampion",
            "PartyGiant",
            "Unique",
            "Elite",
            "Event"});
            this.Skills_cmbxAttackMobType.Location = new System.Drawing.Point(57, 7);
            this.Skills_cmbxAttackMobType.MaxDropDownItems = 10;
            this.Skills_cmbxAttackMobType.Name = "Skills_cmbxAttackMobType";
            this.Skills_cmbxAttackMobType.Size = new System.Drawing.Size(134, 24);
            this.Skills_cmbxAttackMobType.TabIndex = 31;
            this.Skills_cmbxAttackMobType.Tag = null;
            this.Skills_cmbxAttackMobType.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // Skills_lstvAttackMobType_General
            // 
            this.Skills_lstvAttackMobType_General.AllowDrop = true;
            this.Skills_lstvAttackMobType_General.AllowReorder = true;
            this.Skills_lstvAttackMobType_General.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvAttackMobType_General.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvAttackMobType_General.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader35});
            this.Skills_lstvAttackMobType_General.DragDropRemoveFromSource = false;
            this.Skills_lstvAttackMobType_General.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvAttackMobType_General.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvAttackMobType_General.FullRowSelect = true;
            this.Skills_lstvAttackMobType_General.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvAttackMobType_General.HideSelection = false;
            this.Skills_lstvAttackMobType_General.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvAttackMobType_General.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvAttackMobType_General.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvAttackMobType_General.Name = "Skills_lstvAttackMobType_General";
            this.Skills_lstvAttackMobType_General.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvAttackMobType_General.TabIndex = 11;
            this.Skills_lstvAttackMobType_General.Tag = "Source Sans Pro";
            this.Skills_lstvAttackMobType_General.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvAttackMobType_General.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvAttackMobType_General.View = System.Windows.Forms.View.Details;
            this.Skills_lstvAttackMobType_General.Visible = false;
            this.Skills_lstvAttackMobType_General.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_AttackSkill);
            this.Skills_lstvAttackMobType_General.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            // 
            // columnHeader35
            // 
            this.columnHeader35.Text = "Skill name";
            this.columnHeader35.Width = 168;
            // 
            // Skills_lstvAttackMobType_Unique
            // 
            this.Skills_lstvAttackMobType_Unique.AllowDrop = true;
            this.Skills_lstvAttackMobType_Unique.AllowReorder = true;
            this.Skills_lstvAttackMobType_Unique.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvAttackMobType_Unique.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvAttackMobType_Unique.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader42});
            this.Skills_lstvAttackMobType_Unique.DragDropRemoveFromSource = false;
            this.Skills_lstvAttackMobType_Unique.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvAttackMobType_Unique.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvAttackMobType_Unique.FullRowSelect = true;
            this.Skills_lstvAttackMobType_Unique.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvAttackMobType_Unique.HideSelection = false;
            this.Skills_lstvAttackMobType_Unique.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvAttackMobType_Unique.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvAttackMobType_Unique.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvAttackMobType_Unique.Name = "Skills_lstvAttackMobType_Unique";
            this.Skills_lstvAttackMobType_Unique.ShowItemToolTips = true;
            this.Skills_lstvAttackMobType_Unique.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvAttackMobType_Unique.TabIndex = 39;
            this.Skills_lstvAttackMobType_Unique.Tag = "Source Sans Pro";
            this.Skills_lstvAttackMobType_Unique.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvAttackMobType_Unique.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvAttackMobType_Unique.View = System.Windows.Forms.View.Details;
            this.Skills_lstvAttackMobType_Unique.Visible = false;
            this.Skills_lstvAttackMobType_Unique.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_AttackSkill);
            this.Skills_lstvAttackMobType_Unique.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            // 
            // columnHeader42
            // 
            this.columnHeader42.Text = "Skill name";
            this.columnHeader42.Width = 168;
            // 
            // Skills_lstvAttackMobType_Elite
            // 
            this.Skills_lstvAttackMobType_Elite.AllowDrop = true;
            this.Skills_lstvAttackMobType_Elite.AllowReorder = true;
            this.Skills_lstvAttackMobType_Elite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvAttackMobType_Elite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvAttackMobType_Elite.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader40});
            this.Skills_lstvAttackMobType_Elite.DragDropRemoveFromSource = false;
            this.Skills_lstvAttackMobType_Elite.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvAttackMobType_Elite.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvAttackMobType_Elite.FullRowSelect = true;
            this.Skills_lstvAttackMobType_Elite.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvAttackMobType_Elite.HideSelection = false;
            this.Skills_lstvAttackMobType_Elite.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvAttackMobType_Elite.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvAttackMobType_Elite.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvAttackMobType_Elite.Name = "Skills_lstvAttackMobType_Elite";
            this.Skills_lstvAttackMobType_Elite.ShowItemToolTips = true;
            this.Skills_lstvAttackMobType_Elite.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvAttackMobType_Elite.TabIndex = 37;
            this.Skills_lstvAttackMobType_Elite.Tag = "Source Sans Pro";
            this.Skills_lstvAttackMobType_Elite.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvAttackMobType_Elite.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvAttackMobType_Elite.View = System.Windows.Forms.View.Details;
            this.Skills_lstvAttackMobType_Elite.Visible = false;
            this.Skills_lstvAttackMobType_Elite.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_AttackSkill);
            this.Skills_lstvAttackMobType_Elite.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            // 
            // columnHeader40
            // 
            this.columnHeader40.Text = "Skill name";
            this.columnHeader40.Width = 168;
            // 
            // Skills_lstvAttackMobType_PartyGiant
            // 
            this.Skills_lstvAttackMobType_PartyGiant.AllowDrop = true;
            this.Skills_lstvAttackMobType_PartyGiant.AllowReorder = true;
            this.Skills_lstvAttackMobType_PartyGiant.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvAttackMobType_PartyGiant.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvAttackMobType_PartyGiant.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader39});
            this.Skills_lstvAttackMobType_PartyGiant.DragDropRemoveFromSource = false;
            this.Skills_lstvAttackMobType_PartyGiant.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvAttackMobType_PartyGiant.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvAttackMobType_PartyGiant.FullRowSelect = true;
            this.Skills_lstvAttackMobType_PartyGiant.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvAttackMobType_PartyGiant.HideSelection = false;
            this.Skills_lstvAttackMobType_PartyGiant.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvAttackMobType_PartyGiant.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvAttackMobType_PartyGiant.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvAttackMobType_PartyGiant.Name = "Skills_lstvAttackMobType_PartyGiant";
            this.Skills_lstvAttackMobType_PartyGiant.ShowItemToolTips = true;
            this.Skills_lstvAttackMobType_PartyGiant.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvAttackMobType_PartyGiant.TabIndex = 36;
            this.Skills_lstvAttackMobType_PartyGiant.Tag = "Source Sans Pro";
            this.Skills_lstvAttackMobType_PartyGiant.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvAttackMobType_PartyGiant.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvAttackMobType_PartyGiant.View = System.Windows.Forms.View.Details;
            this.Skills_lstvAttackMobType_PartyGiant.Visible = false;
            this.Skills_lstvAttackMobType_PartyGiant.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_AttackSkill);
            this.Skills_lstvAttackMobType_PartyGiant.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            // 
            // columnHeader39
            // 
            this.columnHeader39.Text = "Skill name";
            this.columnHeader39.Width = 168;
            // 
            // Skills_lstvAttackMobType_PartyChampion
            // 
            this.Skills_lstvAttackMobType_PartyChampion.AllowDrop = true;
            this.Skills_lstvAttackMobType_PartyChampion.AllowReorder = true;
            this.Skills_lstvAttackMobType_PartyChampion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvAttackMobType_PartyChampion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvAttackMobType_PartyChampion.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader38});
            this.Skills_lstvAttackMobType_PartyChampion.DragDropRemoveFromSource = false;
            this.Skills_lstvAttackMobType_PartyChampion.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvAttackMobType_PartyChampion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvAttackMobType_PartyChampion.FullRowSelect = true;
            this.Skills_lstvAttackMobType_PartyChampion.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvAttackMobType_PartyChampion.HideSelection = false;
            this.Skills_lstvAttackMobType_PartyChampion.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvAttackMobType_PartyChampion.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvAttackMobType_PartyChampion.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvAttackMobType_PartyChampion.Name = "Skills_lstvAttackMobType_PartyChampion";
            this.Skills_lstvAttackMobType_PartyChampion.ShowItemToolTips = true;
            this.Skills_lstvAttackMobType_PartyChampion.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvAttackMobType_PartyChampion.TabIndex = 35;
            this.Skills_lstvAttackMobType_PartyChampion.Tag = "Source Sans Pro";
            this.Skills_lstvAttackMobType_PartyChampion.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvAttackMobType_PartyChampion.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvAttackMobType_PartyChampion.View = System.Windows.Forms.View.Details;
            this.Skills_lstvAttackMobType_PartyChampion.Visible = false;
            this.Skills_lstvAttackMobType_PartyChampion.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_AttackSkill);
            this.Skills_lstvAttackMobType_PartyChampion.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            // 
            // columnHeader38
            // 
            this.columnHeader38.Text = "Skill name";
            this.columnHeader38.Width = 168;
            // 
            // Skills_lstvAttackMobType_PartyGeneral
            // 
            this.Skills_lstvAttackMobType_PartyGeneral.AllowDrop = true;
            this.Skills_lstvAttackMobType_PartyGeneral.AllowReorder = true;
            this.Skills_lstvAttackMobType_PartyGeneral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvAttackMobType_PartyGeneral.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvAttackMobType_PartyGeneral.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader37});
            this.Skills_lstvAttackMobType_PartyGeneral.DragDropRemoveFromSource = false;
            this.Skills_lstvAttackMobType_PartyGeneral.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvAttackMobType_PartyGeneral.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvAttackMobType_PartyGeneral.FullRowSelect = true;
            this.Skills_lstvAttackMobType_PartyGeneral.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvAttackMobType_PartyGeneral.HideSelection = false;
            this.Skills_lstvAttackMobType_PartyGeneral.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvAttackMobType_PartyGeneral.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvAttackMobType_PartyGeneral.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvAttackMobType_PartyGeneral.Name = "Skills_lstvAttackMobType_PartyGeneral";
            this.Skills_lstvAttackMobType_PartyGeneral.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvAttackMobType_PartyGeneral.TabIndex = 34;
            this.Skills_lstvAttackMobType_PartyGeneral.Tag = "Source Sans Pro";
            this.Skills_lstvAttackMobType_PartyGeneral.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvAttackMobType_PartyGeneral.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvAttackMobType_PartyGeneral.View = System.Windows.Forms.View.Details;
            this.Skills_lstvAttackMobType_PartyGeneral.Visible = false;
            this.Skills_lstvAttackMobType_PartyGeneral.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_AttackSkill);
            this.Skills_lstvAttackMobType_PartyGeneral.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            // 
            // columnHeader37
            // 
            this.columnHeader37.Text = "Skill name";
            this.columnHeader37.Width = 168;
            // 
            // Skills_lstvAttackMobType_Giant
            // 
            this.Skills_lstvAttackMobType_Giant.AllowDrop = true;
            this.Skills_lstvAttackMobType_Giant.AllowReorder = true;
            this.Skills_lstvAttackMobType_Giant.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvAttackMobType_Giant.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvAttackMobType_Giant.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader36});
            this.Skills_lstvAttackMobType_Giant.DragDropRemoveFromSource = false;
            this.Skills_lstvAttackMobType_Giant.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvAttackMobType_Giant.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvAttackMobType_Giant.FullRowSelect = true;
            this.Skills_lstvAttackMobType_Giant.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvAttackMobType_Giant.HideSelection = false;
            this.Skills_lstvAttackMobType_Giant.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvAttackMobType_Giant.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvAttackMobType_Giant.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvAttackMobType_Giant.Name = "Skills_lstvAttackMobType_Giant";
            this.Skills_lstvAttackMobType_Giant.ShowItemToolTips = true;
            this.Skills_lstvAttackMobType_Giant.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvAttackMobType_Giant.TabIndex = 33;
            this.Skills_lstvAttackMobType_Giant.Tag = "Source Sans Pro";
            this.Skills_lstvAttackMobType_Giant.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvAttackMobType_Giant.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvAttackMobType_Giant.View = System.Windows.Forms.View.Details;
            this.Skills_lstvAttackMobType_Giant.Visible = false;
            this.Skills_lstvAttackMobType_Giant.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_AttackSkill);
            this.Skills_lstvAttackMobType_Giant.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            // 
            // columnHeader36
            // 
            this.columnHeader36.Text = "Skill name";
            this.columnHeader36.Width = 168;
            // 
            // Skills_lstvAttackMobType_Champion
            // 
            this.Skills_lstvAttackMobType_Champion.AllowDrop = true;
            this.Skills_lstvAttackMobType_Champion.AllowReorder = true;
            this.Skills_lstvAttackMobType_Champion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvAttackMobType_Champion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvAttackMobType_Champion.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader24});
            this.Skills_lstvAttackMobType_Champion.DragDropRemoveFromSource = false;
            this.Skills_lstvAttackMobType_Champion.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvAttackMobType_Champion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvAttackMobType_Champion.FullRowSelect = true;
            this.Skills_lstvAttackMobType_Champion.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvAttackMobType_Champion.HideSelection = false;
            this.Skills_lstvAttackMobType_Champion.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvAttackMobType_Champion.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvAttackMobType_Champion.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvAttackMobType_Champion.Name = "Skills_lstvAttackMobType_Champion";
            this.Skills_lstvAttackMobType_Champion.ShowItemToolTips = true;
            this.Skills_lstvAttackMobType_Champion.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvAttackMobType_Champion.TabIndex = 32;
            this.Skills_lstvAttackMobType_Champion.Tag = "Source Sans Pro";
            this.Skills_lstvAttackMobType_Champion.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvAttackMobType_Champion.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvAttackMobType_Champion.View = System.Windows.Forms.View.Details;
            this.Skills_lstvAttackMobType_Champion.Visible = false;
            this.Skills_lstvAttackMobType_Champion.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_AttackSkill);
            this.Skills_lstvAttackMobType_Champion.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            // 
            // columnHeader24
            // 
            this.columnHeader24.Text = "Skill name";
            this.columnHeader24.Width = 168;
            // 
            // Skills_lstvAttackMobType_Event
            // 
            this.Skills_lstvAttackMobType_Event.AllowDrop = true;
            this.Skills_lstvAttackMobType_Event.AllowReorder = true;
            this.Skills_lstvAttackMobType_Event.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvAttackMobType_Event.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvAttackMobType_Event.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader41});
            this.Skills_lstvAttackMobType_Event.DragDropRemoveFromSource = false;
            this.Skills_lstvAttackMobType_Event.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Skills_lstvAttackMobType_Event.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvAttackMobType_Event.FullRowSelect = true;
            this.Skills_lstvAttackMobType_Event.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvAttackMobType_Event.HideSelection = false;
            this.Skills_lstvAttackMobType_Event.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvAttackMobType_Event.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvAttackMobType_Event.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvAttackMobType_Event.Name = "Skills_lstvAttackMobType_Event";
            this.Skills_lstvAttackMobType_Event.ShowItemToolTips = true;
            this.Skills_lstvAttackMobType_Event.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvAttackMobType_Event.TabIndex = 40;
            this.Skills_lstvAttackMobType_Event.Tag = "Source Sans Pro";
            this.Skills_lstvAttackMobType_Event.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvAttackMobType_Event.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvAttackMobType_Event.View = System.Windows.Forms.View.Details;
            this.Skills_lstvAttackMobType_Event.Visible = false;
            this.Skills_lstvAttackMobType_Event.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_AttackSkill);
            this.Skills_lstvAttackMobType_Event.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            // 
            // columnHeader41
            // 
            this.columnHeader41.Text = "Skill name";
            this.columnHeader41.Width = 168;
            // 
            // TabPageH_Skills_Option02_Panel
            // 
            this.TabPageH_Skills_Option02_Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_btnAddBuff);
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_btnRemBuff);
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_cmbxBuffMobType);
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_lstvBuffMobType_General);
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_lstvBuffMobType_Champion);
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_lstvBuffMobType_Giant);
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_lstvBuffMobType_PartyGeneral);
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_lstvBuffMobType_PartyChampion);
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_lstvBuffMobType_PartyGiant);
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_lstvBuffMobType_Unique);
            this.TabPageH_Skills_Option02_Panel.Controls.Add(this.Skills_lstvBuffMobType_Elite);
            this.TabPageH_Skills_Option02_Panel.Location = new System.Drawing.Point(184, 27);
            this.TabPageH_Skills_Option02_Panel.Name = "TabPageH_Skills_Option02_Panel";
            this.TabPageH_Skills_Option02_Panel.Size = new System.Drawing.Size(473, 345);
            this.TabPageH_Skills_Option02_Panel.TabIndex = 30;
            this.TabPageH_Skills_Option02_Panel.Visible = false;
            // 
            // Skills_btnAddBuff
            // 
            this.Skills_btnAddBuff.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Skills_btnAddBuff.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.Skills_btnAddBuff.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_btnAddBuff.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(64)))));
            this.Skills_btnAddBuff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Skills_btnAddBuff.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_btnAddBuff.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_btnAddBuff.Location = new System.Drawing.Point(6, 7);
            this.Skills_btnAddBuff.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_btnAddBuff.Name = "Skills_btnAddBuff";
            this.Skills_btnAddBuff.Size = new System.Drawing.Size(26, 26);
            this.Skills_btnAddBuff.TabIndex = 57;
            this.Skills_btnAddBuff.Tag = "Font Awesome 5 Pro Regular";
            this.Skills_btnAddBuff.Text = "";
            this.ToolTips.SetToolTip(this.Skills_btnAddBuff, "Add buffing skill");
            this.Skills_btnAddBuff.UseCompatibleTextRendering = true;
            this.Skills_btnAddBuff.UseVisualStyleBackColor = false;
            this.Skills_btnAddBuff.Click += new System.EventHandler(this.Control_Click);
            // 
            // Skills_btnRemBuff
            // 
            this.Skills_btnRemBuff.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Skills_btnRemBuff.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.Skills_btnRemBuff.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_btnRemBuff.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(62)))), ((int)(((byte)(62)))), ((int)(((byte)(64)))));
            this.Skills_btnRemBuff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Skills_btnRemBuff.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Skills_btnRemBuff.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_btnRemBuff.Location = new System.Drawing.Point(31, 7);
            this.Skills_btnRemBuff.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_btnRemBuff.Name = "Skills_btnRemBuff";
            this.Skills_btnRemBuff.Size = new System.Drawing.Size(26, 26);
            this.Skills_btnRemBuff.TabIndex = 56;
            this.Skills_btnRemBuff.Tag = "Font Awesome 5 Pro Regular";
            this.Skills_btnRemBuff.Text = "";
            this.ToolTips.SetToolTip(this.Skills_btnRemBuff, "Remove buffing skill");
            this.Skills_btnRemBuff.UseCompatibleTextRendering = true;
            this.Skills_btnRemBuff.UseVisualStyleBackColor = false;
            this.Skills_btnRemBuff.Click += new System.EventHandler(this.Control_Click);
            // 
            // Skills_cmbxBuffMobType
            // 
            this.Skills_cmbxBuffMobType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_cmbxBuffMobType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Skills_cmbxBuffMobType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Skills_cmbxBuffMobType.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.Skills_cmbxBuffMobType.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_cmbxBuffMobType.FormattingEnabled = true;
            this.Skills_cmbxBuffMobType.Items.AddRange(new object[] {
            "General",
            "Champion",
            "Giant",
            "PartyGeneral",
            "PartyChampion",
            "PartyGiant",
            "Unique",
            "Elite"});
            this.Skills_cmbxBuffMobType.Location = new System.Drawing.Point(57, 7);
            this.Skills_cmbxBuffMobType.MaxDropDownItems = 10;
            this.Skills_cmbxBuffMobType.Name = "Skills_cmbxBuffMobType";
            this.Skills_cmbxBuffMobType.Size = new System.Drawing.Size(134, 24);
            this.Skills_cmbxBuffMobType.TabIndex = 47;
            this.Skills_cmbxBuffMobType.Tag = null;
            this.Skills_cmbxBuffMobType.SelectedIndexChanged += new System.EventHandler(this.ComboBox_SelectedIndexChanged);
            // 
            // Skills_lstvBuffMobType_General
            // 
            this.Skills_lstvBuffMobType_General.AllowDrop = true;
            this.Skills_lstvBuffMobType_General.AllowReorder = true;
            this.Skills_lstvBuffMobType_General.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvBuffMobType_General.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvBuffMobType_General.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader65});
            this.Skills_lstvBuffMobType_General.DragDropRemoveFromSource = false;
            this.Skills_lstvBuffMobType_General.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvBuffMobType_General.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvBuffMobType_General.FullRowSelect = true;
            this.Skills_lstvBuffMobType_General.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvBuffMobType_General.HideSelection = false;
            this.Skills_lstvBuffMobType_General.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvBuffMobType_General.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvBuffMobType_General.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvBuffMobType_General.Name = "Skills_lstvBuffMobType_General";
            this.Skills_lstvBuffMobType_General.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvBuffMobType_General.TabIndex = 46;
            this.Skills_lstvBuffMobType_General.Tag = "Source Sans Pro";
            this.Skills_lstvBuffMobType_General.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvBuffMobType_General.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvBuffMobType_General.View = System.Windows.Forms.View.Details;
            this.Skills_lstvBuffMobType_General.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_BuffSkill);
            this.Skills_lstvBuffMobType_General.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            this.Skills_lstvBuffMobType_General.Visible = false;
            // 
            // columnHeader65
            // 
            this.columnHeader65.Text = "Skill name";
            this.columnHeader65.Width = 168;
            // 
            // Skills_lstvBuffMobType_Champion
            // 
            this.Skills_lstvBuffMobType_Champion.AllowDrop = true;
            this.Skills_lstvBuffMobType_Champion.AllowReorder = true;
            this.Skills_lstvBuffMobType_Champion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvBuffMobType_Champion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvBuffMobType_Champion.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader66});
            this.Skills_lstvBuffMobType_Champion.DragDropRemoveFromSource = false;
            this.Skills_lstvBuffMobType_Champion.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvBuffMobType_Champion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvBuffMobType_Champion.FullRowSelect = true;
            this.Skills_lstvBuffMobType_Champion.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvBuffMobType_Champion.HideSelection = false;
            this.Skills_lstvBuffMobType_Champion.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvBuffMobType_Champion.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvBuffMobType_Champion.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvBuffMobType_Champion.Name = "Skills_lstvBuffMobType_Champion";
            this.Skills_lstvBuffMobType_Champion.ShowItemToolTips = true;
            this.Skills_lstvBuffMobType_Champion.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvBuffMobType_Champion.TabIndex = 54;
            this.Skills_lstvBuffMobType_Champion.Tag = "Source Sans Pro";
            this.Skills_lstvBuffMobType_Champion.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvBuffMobType_Champion.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvBuffMobType_Champion.View = System.Windows.Forms.View.Details;
            this.Skills_lstvBuffMobType_Champion.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_BuffSkill);
            this.Skills_lstvBuffMobType_Champion.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            this.Skills_lstvBuffMobType_Champion.Visible = false;
            // 
            // columnHeader66
            // 
            this.columnHeader66.Text = "Skill name";
            this.columnHeader66.Width = 168;
            // 
            // Skills_lstvBuffMobType_Giant
            // 
            this.Skills_lstvBuffMobType_Giant.AllowDrop = true;
            this.Skills_lstvBuffMobType_Giant.AllowReorder = true;
            this.Skills_lstvBuffMobType_Giant.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvBuffMobType_Giant.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvBuffMobType_Giant.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader67});
            this.Skills_lstvBuffMobType_Giant.DragDropRemoveFromSource = false;
            this.Skills_lstvBuffMobType_Giant.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvBuffMobType_Giant.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvBuffMobType_Giant.FullRowSelect = true;
            this.Skills_lstvBuffMobType_Giant.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvBuffMobType_Giant.HideSelection = false;
            this.Skills_lstvBuffMobType_Giant.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvBuffMobType_Giant.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvBuffMobType_Giant.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvBuffMobType_Giant.Name = "Skills_lstvBuffMobType_Giant";
            this.Skills_lstvBuffMobType_Giant.ShowItemToolTips = true;
            this.Skills_lstvBuffMobType_Giant.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvBuffMobType_Giant.TabIndex = 53;
            this.Skills_lstvBuffMobType_Giant.Tag = "Source Sans Pro";
            this.Skills_lstvBuffMobType_Giant.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvBuffMobType_Giant.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvBuffMobType_Giant.View = System.Windows.Forms.View.Details;
            this.Skills_lstvBuffMobType_Giant.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_BuffSkill);
            this.Skills_lstvBuffMobType_Giant.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            this.Skills_lstvBuffMobType_Giant.Visible = false;
            // 
            // columnHeader67
            // 
            this.columnHeader67.Text = "Skill name";
            this.columnHeader67.Width = 168;
            // 
            // Skills_lstvBuffMobType_PartyGeneral
            // 
            this.Skills_lstvBuffMobType_PartyGeneral.AllowDrop = true;
            this.Skills_lstvBuffMobType_PartyGeneral.AllowReorder = true;
            this.Skills_lstvBuffMobType_PartyGeneral.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvBuffMobType_PartyGeneral.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvBuffMobType_PartyGeneral.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader68});
            this.Skills_lstvBuffMobType_PartyGeneral.DragDropRemoveFromSource = false;
            this.Skills_lstvBuffMobType_PartyGeneral.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvBuffMobType_PartyGeneral.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvBuffMobType_PartyGeneral.FullRowSelect = true;
            this.Skills_lstvBuffMobType_PartyGeneral.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvBuffMobType_PartyGeneral.HideSelection = false;
            this.Skills_lstvBuffMobType_PartyGeneral.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvBuffMobType_PartyGeneral.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvBuffMobType_PartyGeneral.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvBuffMobType_PartyGeneral.Name = "Skills_lstvBuffMobType_PartyGeneral";
            this.Skills_lstvBuffMobType_PartyGeneral.ShowItemToolTips = true;
            this.Skills_lstvBuffMobType_PartyGeneral.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvBuffMobType_PartyGeneral.TabIndex = 52;
            this.Skills_lstvBuffMobType_PartyGeneral.Tag = "Source Sans Pro";
            this.Skills_lstvBuffMobType_PartyGeneral.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvBuffMobType_PartyGeneral.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvBuffMobType_PartyGeneral.View = System.Windows.Forms.View.Details;
            this.Skills_lstvBuffMobType_PartyGeneral.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_BuffSkill);
            this.Skills_lstvBuffMobType_PartyGeneral.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            this.Skills_lstvBuffMobType_PartyGeneral.Visible = false;
            // 
            // columnHeader68
            // 
            this.columnHeader68.Text = "Skill name";
            this.columnHeader68.Width = 168;
            // 
            // Skills_lstvBuffMobType_PartyChampion
            // 
            this.Skills_lstvBuffMobType_PartyChampion.AllowDrop = true;
            this.Skills_lstvBuffMobType_PartyChampion.AllowReorder = true;
            this.Skills_lstvBuffMobType_PartyChampion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvBuffMobType_PartyChampion.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvBuffMobType_PartyChampion.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader69});
            this.Skills_lstvBuffMobType_PartyChampion.DragDropRemoveFromSource = false;
            this.Skills_lstvBuffMobType_PartyChampion.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvBuffMobType_PartyChampion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvBuffMobType_PartyChampion.FullRowSelect = true;
            this.Skills_lstvBuffMobType_PartyChampion.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvBuffMobType_PartyChampion.HideSelection = false;
            this.Skills_lstvBuffMobType_PartyChampion.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvBuffMobType_PartyChampion.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvBuffMobType_PartyChampion.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvBuffMobType_PartyChampion.Name = "Skills_lstvBuffMobType_PartyChampion";
            this.Skills_lstvBuffMobType_PartyChampion.ShowItemToolTips = true;
            this.Skills_lstvBuffMobType_PartyChampion.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvBuffMobType_PartyChampion.TabIndex = 51;
            this.Skills_lstvBuffMobType_PartyChampion.Tag = "Source Sans Pro";
            this.Skills_lstvBuffMobType_PartyChampion.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvBuffMobType_PartyChampion.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvBuffMobType_PartyChampion.View = System.Windows.Forms.View.Details;
            this.Skills_lstvBuffMobType_PartyChampion.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_BuffSkill);
            this.Skills_lstvBuffMobType_PartyChampion.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            this.Skills_lstvBuffMobType_PartyChampion.Visible = false;
            // 
            // columnHeader69
            // 
            this.columnHeader69.Text = "Skill name";
            this.columnHeader69.Width = 168;
            // 
            // Skills_lstvBuffMobType_PartyGiant
            // 
            this.Skills_lstvBuffMobType_PartyGiant.AllowDrop = true;
            this.Skills_lstvBuffMobType_PartyGiant.AllowReorder = true;
            this.Skills_lstvBuffMobType_PartyGiant.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvBuffMobType_PartyGiant.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvBuffMobType_PartyGiant.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader70});
            this.Skills_lstvBuffMobType_PartyGiant.DragDropRemoveFromSource = false;
            this.Skills_lstvBuffMobType_PartyGiant.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvBuffMobType_PartyGiant.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvBuffMobType_PartyGiant.FullRowSelect = true;
            this.Skills_lstvBuffMobType_PartyGiant.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvBuffMobType_PartyGiant.HideSelection = false;
            this.Skills_lstvBuffMobType_PartyGiant.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvBuffMobType_PartyGiant.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvBuffMobType_PartyGiant.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvBuffMobType_PartyGiant.Name = "Skills_lstvBuffMobType_PartyGiant";
            this.Skills_lstvBuffMobType_PartyGiant.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvBuffMobType_PartyGiant.TabIndex = 50;
            this.Skills_lstvBuffMobType_PartyGiant.Tag = "Source Sans Pro";
            this.Skills_lstvBuffMobType_PartyGiant.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvBuffMobType_PartyGiant.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvBuffMobType_PartyGiant.View = System.Windows.Forms.View.Details;
            this.Skills_lstvBuffMobType_PartyGiant.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_BuffSkill);
            this.Skills_lstvBuffMobType_PartyGiant.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            this.Skills_lstvBuffMobType_PartyGiant.Visible = false;
            // 
            // columnHeader70
            // 
            this.columnHeader70.Text = "Skill name";
            this.columnHeader70.Width = 168;
            // 
            // Skills_lstvBuffMobType_Unique
            // 
            this.Skills_lstvBuffMobType_Unique.AllowDrop = true;
            this.Skills_lstvBuffMobType_Unique.AllowReorder = true;
            this.Skills_lstvBuffMobType_Unique.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvBuffMobType_Unique.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvBuffMobType_Unique.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader71});
            this.Skills_lstvBuffMobType_Unique.DragDropRemoveFromSource = false;
            this.Skills_lstvBuffMobType_Unique.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvBuffMobType_Unique.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvBuffMobType_Unique.FullRowSelect = true;
            this.Skills_lstvBuffMobType_Unique.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvBuffMobType_Unique.HideSelection = false;
            this.Skills_lstvBuffMobType_Unique.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvBuffMobType_Unique.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvBuffMobType_Unique.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvBuffMobType_Unique.Name = "Skills_lstvBuffMobType_Unique";
            this.Skills_lstvBuffMobType_Unique.ShowItemToolTips = true;
            this.Skills_lstvBuffMobType_Unique.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvBuffMobType_Unique.TabIndex = 49;
            this.Skills_lstvBuffMobType_Unique.Tag = "Source Sans Pro";
            this.Skills_lstvBuffMobType_Unique.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvBuffMobType_Unique.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvBuffMobType_Unique.View = System.Windows.Forms.View.Details;
            this.Skills_lstvBuffMobType_Unique.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_BuffSkill);
            this.Skills_lstvBuffMobType_Unique.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            this.Skills_lstvBuffMobType_Unique.Visible = false;
            // 
            // columnHeader71
            // 
            this.columnHeader71.Text = "Skill name";
            this.columnHeader71.Width = 168;
            // 
            // Skills_lstvBuffMobType_Elite
            // 
            this.Skills_lstvBuffMobType_Elite.AllowDrop = true;
            this.Skills_lstvBuffMobType_Elite.AllowReorder = true;
            this.Skills_lstvBuffMobType_Elite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(34)))), ((int)(((byte)(34)))), ((int)(((byte)(38)))));
            this.Skills_lstvBuffMobType_Elite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Skills_lstvBuffMobType_Elite.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader72});
            this.Skills_lstvBuffMobType_Elite.DragDropRemoveFromSource = false;
            this.Skills_lstvBuffMobType_Elite.Font = new System.Drawing.Font("Microsoft Sans Serif", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.Skills_lstvBuffMobType_Elite.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(241)))), ((int)(((byte)(241)))));
            this.Skills_lstvBuffMobType_Elite.FullRowSelect = true;
            this.Skills_lstvBuffMobType_Elite.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.Skills_lstvBuffMobType_Elite.HideSelection = false;
            this.Skills_lstvBuffMobType_Elite.LineColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            this.Skills_lstvBuffMobType_Elite.Location = new System.Drawing.Point(6, 32);
            this.Skills_lstvBuffMobType_Elite.Margin = new System.Windows.Forms.Padding(0);
            this.Skills_lstvBuffMobType_Elite.Name = "Skills_lstvBuffMobType_Elite";
            this.Skills_lstvBuffMobType_Elite.ShowItemToolTips = true;
            this.Skills_lstvBuffMobType_Elite.Size = new System.Drawing.Size(185, 140);
            this.Skills_lstvBuffMobType_Elite.TabIndex = 48;
            this.Skills_lstvBuffMobType_Elite.Tag = "Source Sans Pro";
            this.Skills_lstvBuffMobType_Elite.TileSize = new System.Drawing.Size(201, 30);
            this.Skills_lstvBuffMobType_Elite.UseCompatibleStateImageBehavior = false;
            this.Skills_lstvBuffMobType_Elite.View = System.Windows.Forms.View.Details;
            this.Skills_lstvBuffMobType_Elite.DragItemAdding += new System.EventHandler<xGraphics.xListView.DragItemEventArgs>(this.xListView_DragItemAdding_BuffSkill);
            this.Skills_lstvBuffMobType_Elite.DragItemsChanged += new System.EventHandler(this.xListView_DragItemsChanged);
            this.Skills_lstvBuffMobType_Elite.Visible = false;
            // 
            // columnHeader72
            // 
            this.columnHeader72.Text = "Skill name";
            this.columnHeader72.Width = 168;
            // 
            // TabPageH_Skills_Option03_Panel
            // 
            this.TabPageH_Skills_Option03_Panel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.TabPageH_Skills_Option03_Panel.Location = new System.Drawing.Point(184, 27);
            this.TabPageH_Skills_Option03_Panel.Name = "TabPageH_Skills_Option03_Panel";
            this.TabPageH_Skills_Option03_Panel.Size = new System.Drawing.Size(473, 345);
            this.TabPageH_Skills_Option03_Panel.TabIndex = 29;
            this.TabPageH_Skills_Option03_Panel.Visible = false;
		}
	}
}