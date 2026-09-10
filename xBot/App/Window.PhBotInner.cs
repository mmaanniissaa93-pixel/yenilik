using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace xBot.App
{
    /// <summary>
    /// phBot birebir İÇ düzenler (Attack / Buffs / Potions).
    /// Referans: docs/phbot_ref/attack.png, buffs.png, protection_potions.png.
    /// Yöntem: mevcut bağlı kontroller AYNI nesne olarak phBot geometrisine
    /// taşınır (bot döngüsü Bot.Checks.cs'de bu nesneleri okur; yeniden
    /// üretmek davranışı bozardı). Eksik phBot kutuları TODO backend ile
    /// görsel klon olarak eklenir. Modern tema yerleşimcileriyle çakışmayı
    /// önlemek için panel.Layout/SizeChanged olaylarına EN SON abone olunur.
    /// </summary>
    public partial class Window
    {
        private bool _innersHooked;
        private bool _innerLayout;

        private static readonly Color PhBotComboGray = Color.FromArgb(225, 225, 225);

        private void LayoutPhBotInners()
        {
            try
            {
                if (!_innersHooked)
                {
                    _innersHooked = true;
                    try
                    {
                        if (TabPageH_Skills_Option01_Panel != null)
                        {
                            TabPageH_Skills_Option01_Panel.Layout += (s, e) => LayoutAttackInner();
                            TabPageH_Skills_Option01_Panel.SizeChanged += (s, e) => LayoutAttackInner();
                        }
                    }
                    catch { }
                    try
                    {
                        if (TabPageH_Skills_Option02_Panel != null)
                        {
                            TabPageH_Skills_Option02_Panel.Layout += (s, e) => LayoutBuffsInner();
                            TabPageH_Skills_Option02_Panel.SizeChanged += (s, e) => LayoutBuffsInner();
                        }
                    }
                    catch { }
                    try
                    {
                        if (TabPageH_Character_Option02_Panel != null)
                        {
                            TabPageH_Character_Option02_Panel.Layout += (s, e) => LayoutPotionsInner();
                            TabPageH_Character_Option02_Panel.SizeChanged += (s, e) => LayoutPotionsInner();
                        }
                    }
                    catch { }
                }
                LayoutAttackInner();
                LayoutBuffsInner();
                LayoutPotionsInner();
            }
            catch (Exception ex) { PhBotDebug("inners: " + ex.Message); }
        }

        private static void Place(Control c, int x, int y, int w, int h)
        {
            if (c == null) return;
            try
            {
                c.Location = new Point(x, y);
                c.Size = new Size(w, h);
                c.Visible = true;
            }
            catch { }
        }

        private static void Classicize(Control c)
        {
            if (c == null) return;
            try
            {
                c.Font = PhBotFont();
                c.ForeColor = Color.Black;
                Button b = c as Button;
                if (b != null)
                {
                    b.FlatStyle = FlatStyle.Standard;
                    b.BackColor = SystemColors.Control;
                    b.UseVisualStyleBackColor = true;
                }
                TextBox t = c as TextBox;
                if (t != null) { t.BackColor = Color.White; t.BorderStyle = BorderStyle.FixedSingle; }
                ComboBox cb = c as ComboBox;
                if (cb != null) { cb.BackColor = Color.White; }
                ListView lv = c as ListView;
                if (lv != null)
                {
                    try { lv.OwnerDraw = false; } catch { }
                    lv.BackColor = Color.White;
                    lv.ForeColor = Color.Black;
                    lv.FullRowSelect = true;
                    lv.GridLines = true;
                    try { lv.BorderStyle = BorderStyle.FixedSingle; } catch { }
                }
            }
            catch { }
        }

        private static void GrayCombo(ComboBox cb)
        {
            if (cb == null) return;
            try
            {
                cb.BackColor = PhBotComboGray;
                cb.ForeColor = Color.Black;
                cb.Font = PhBotFont();
                cb.FlatStyle = FlatStyle.Standard;
            }
            catch { }
        }

        /// <summary>phBot görsel klon kutusu (backend yok).</summary>
        private static CheckBox PhBotTodoCheck(Panel p, string name, string text, int x, int y, bool check)
        {
            if (p == null) return null;
            CheckBox c = null;
            try
            {
                if (p.Controls.ContainsKey(name))
                    c = p.Controls[name] as CheckBox;
            }
            catch { }
            if (c == null)
            {
                c = new CheckBox();
                c.Name = name;
                c.Font = PhBotFont();
                c.ForeColor = Color.Black;
                c.AutoSize = true;
                // Varsayılan işaret YALNIZCA ilk kurulumda uygulanır;
                // sonraki yerleşimler kullanıcının seçimini korur.
                c.Checked = check;
                try { p.Controls.Add(c); } catch { }
            }
            try
            {
                c.Text = text;
                c.Location = new Point(x, y);
                c.Visible = true;
            }
            catch { }
            return c;
        }

        private static TextBox PhBotTodoNumber(Panel p, string name, int x, int y, int w, string text)
        {
            if (p == null) return null;
            TextBox t = null;
            try
            {
                if (p.Controls.ContainsKey(name))
                    t = p.Controls[name] as TextBox;
            }
            catch { }
            if (t == null)
            {
                t = new TextBox();
                t.Name = name;
                t.Font = PhBotFont();
                t.BackColor = Color.White;
                t.ForeColor = Color.Black;
                t.BorderStyle = BorderStyle.FixedSingle;
                t.TextAlign = HorizontalAlignment.Left;
                try { p.Controls.Add(t); } catch { }
            }
            try
            {
                t.Location = new Point(x, y);
                t.Size = new Size(w, 24);
                t.Visible = true;
                if (string.IsNullOrEmpty(t.Text)) t.Text = text;
            }
            catch { }
            return t;
        }

        private static Label PhBotLabel(Panel p, string name, string text, int x, int y)
        {
            if (p == null) return null;
            Label l = null;
            try
            {
                if (p.Controls.ContainsKey(name))
                    l = p.Controls[name] as Label;
            }
            catch { }
            if (l == null)
            {
                l = new Label();
                l.Name = name;
                l.Font = PhBotFont();
                l.ForeColor = Color.Black;
                l.AutoSize = true;
                try { p.Controls.Add(l); } catch { }
            }
            try { l.Text = text; l.Location = new Point(x, y); l.Visible = true; }
            catch { }
            return l;
        }

        // ---------------------------------------------------------------
        // ATTACK — docs/phbot_ref/attack.png
        // [skill list] [transfer] [type combo + attack list + up/down] [options] + imbue
        // ---------------------------------------------------------------
        private void LayoutAttackInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Skills_Option01_Panel;
                Panel hostV = TabPageV_Control01_Skills_Panel;
                if (p == null || hostV == null) return;
                int W = p.Width, H = p.Height;
                if (W < 700 || H < 300) return;

                // Sekme şeridi skill listesinin sağında başlar.
                try
                {
                    Control tc = null;
                    foreach (Control c in hostV.Controls)
                    {
                        if (c is TabControl && c.Name == "TabPageH_Skills_Tabs") { tc = c; break; }
                    }
                    if (tc != null)
                    {
                        tc.Location = new Point(312, tc.Location.Y);
                        tc.Width = Math.Max(200, hostV.Width - 312 - 4);
                    }
                }
                catch { }

                // Sol: skill listesi (V panel seviyesinde, sekme hizasında).
                try
                {
                    if (Skills_lstvSkills != null)
                    {
                        int stripTop = 4;
                        try
                        {
                            if (TabPageH_Skills != null) stripTop = TabPageH_Skills.Top;
                        }
                        catch { }
                        Skills_lstvSkills.Location = new Point(6, stripTop);
                        Skills_lstvSkills.Size = new Size(300, Math.Max(200, hostV.Height - stripTop - 6));
                        Skills_lstvSkills.Visible = true;
                        Classicize(Skills_lstvSkills);
                    }
                }
                catch { }

                const int tx = 8, tw = 56;          // transfer
                const int lx = 72, lw = 290;        // liste + combo
                const int ux = 370, uw = 56;        // yukarı/aşağı
                const int ox = 440;                 // sağ seçenekler

                // Type kombo (gri) + 9 mob-tipi listesi üst üste.
                try
                {
                    GrayCombo(Skills_cmbxAttackMobType);
                    Place(Skills_cmbxAttackMobType, lx, 8, lw, 28);
                }
                catch { }
                int listTop = 44, listH = Math.Max(150, H - 44 - 70);
                ListView[] atkLists = new ListView[]
                {
                    Skills_lstvAttackMobType_General, Skills_lstvAttackMobType_Unique,
                    Skills_lstvAttackMobType_Elite, Skills_lstvAttackMobType_PartyGiant,
                    Skills_lstvAttackMobType_PartyChampion, Skills_lstvAttackMobType_PartyGeneral,
                    Skills_lstvAttackMobType_Giant, Skills_lstvAttackMobType_Champion,
                    Skills_lstvAttackMobType_Event
                };
                foreach (ListView lv in atkLists)
                {
                    if (lv == null) continue;
                    try
                    {
                        lv.Location = new Point(lx, listTop);
                        lv.Size = new Size(lw, listH);
                        Classicize(lv);
                    }
                    catch { }
                }

                // Transfer ▶◀ (mevcut ekle/çıkar butonları).
                try
                {
                    if (Skills_btnAddAttack != null)
                    {
                        Skills_btnAddAttack.Text = "▶";
                        Classicize(Skills_btnAddAttack);
                        Place(Skills_btnAddAttack, tx, 150, tw, 46);
                    }
                    if (Skills_btnRemAttack != null)
                    {
                        Skills_btnRemAttack.Text = "◀";
                        Classicize(Skills_btnRemAttack);
                        Place(Skills_btnRemAttack, tx, 202, tw, 46);
                    }
                }
                catch { }

                // Yukarı/aşağı (mavi runtime butonları gri klasik olur).
                try
                {
                    foreach (Control c in p.Controls)
                    {
                        Button b = c as Button;
                        if (b == null) continue;
                        if (b.Text != "▲" && b.Text != "▼") continue;
                        if (b.Name.StartsWith("PhBot_")) continue;
                        Classicize(b);
                        if (b.Text == "▲") Place(b, ux, 150, uw, 46);
                        else Place(b, ux, 202, uw, 46);
                    }
                }
                catch { }

                // Sağ seçenek sütunu (phBot attack.png 7..18 sırası).
                int y = 8;
                try
                {
                    if (Skills_cbxCastInOrder != null)
                    {
                        Skills_cbxCastInOrder.Text = "Cast skills in order";
                        Classicize(Skills_cbxCastInOrder);
                        Skills_cbxCastInOrder.AutoSize = true;
                        Skills_cbxCastInOrder.Location = new Point(ox, y);
                        Skills_cbxCastInOrder.Visible = true;
                    }
                }
                catch { }
                y += 30;
                PhBotTodoCheck(p, "PhBot_KillSteal", "Kill steal monsters", ox, y, true); y += 30;
                try
                {
                    if (cbxSkillNoAttack != null)
                    {
                        cbxSkillNoAttack.Text = "Don't attack monsters";
                        Classicize(cbxSkillNoAttack);
                        cbxSkillNoAttack.AutoSize = true;
                        cbxSkillNoAttack.Location = new Point(ox, y);
                        cbxSkillNoAttack.Visible = true;
                    }
                }
                catch { }
                y += 30;
                PhBotTodoCheck(p, "PhBot_ProtectParty", "Protect party members", ox, y, false);
                PhBotTodoNumber(p, "PhBot_ProtectName", ox + 190, y - 2, 140, ""); y += 30;
                PhBotTodoCheck(p, "PhBot_AttackLower", "Attack lower monsters first", ox, y, true); y += 30;
                PhBotTodoCheck(p, "PhBot_SwitchDot", "Switch monster after", ox, y, false);
                PhBotTodoNumber(p, "PhBot_SwitchDotN", ox + 190, y - 2, 60, "0");
                PhBotLabel(p, "PhBot_DotLbl", "DOT", ox + 256, y + 3); y += 30;
                PhBotTodoCheck(p, "PhBot_LowerSkills", "Use lower skills if skills for a specific monster type do not exist", ox, y, true); y += 30;
                PhBotTodoCheck(p, "PhBot_SlowerAttack", "Slower attack mode", ox, y, false); y += 30;
                PhBotTodoCheck(p, "PhBot_Lagtastic", "Lagtastic", ox, y, false); y += 30;
                PhBotTodoCheck(p, "PhBot_AutoUnique", "Auto select nearby uniques (must not be botting)", ox, y, false); y += 30;
                PhBotTodoCheck(p, "PhBot_AutoTitan", "Auto select nearby titans (must not be botting)", ox, y, false); y += 30;
                PhBotTodoCheck(p, "PhBot_TeleportSkill", "Use teleport skills", ox, y, false);

                // Alt: Imbue.
                try
                {
                    if (lblSkillImbue != null)
                    {
                        lblSkillImbue.Text = "Imbue";
                        Classicize(lblSkillImbue);
                        lblSkillImbue.AutoSize = true;
                        lblSkillImbue.Location = new Point(lx, H - 56);
                        lblSkillImbue.Visible = true;
                    }
                    if (cmbxImbue != null)
                    {
                        GrayCombo(cmbxImbue);
                        Place(cmbxImbue, lx + 60, H - 60, lw, 28);
                    }
                }
                catch { }
                try
                {
                    if (lblSkillRuntimeStatus != null)
                    {
                        Classicize(lblSkillRuntimeStatus);
                        lblSkillRuntimeStatus.Location = new Point(8, H - 26);
                        lblSkillRuntimeStatus.Size = new Size(Math.Max(200, W - 16), 20);
                        lblSkillRuntimeStatus.Visible = true;
                    }
                }
                catch { }

                // Bu sekmede yeri yok: gizle (değer korunur).
                try
                {
                    if (cbxSkillDevil != null && cbxSkillDevil.Parent == p)
                    {
                        TabPageH_Skills_Option02_Panel.Controls.Add(cbxSkillDevil);
                    }
                }
                catch { }
                try { if (Training_cbxWalkToCenter != null) Training_cbxWalkToCenter.Visible = false; }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("attack inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        // ---------------------------------------------------------------
        // BUFFS — docs/phbot_ref/buffs.png
        // ---------------------------------------------------------------
        private void LayoutBuffsInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Skills_Option02_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 700 || H < 300) return;

                const int tx = 8, tw = 56;
                const int lx = 72, lw = 290;
                const int ux = 370, uw = 56;
                const int ox = 440;

                try
                {
                    GrayCombo(Skills_cmbxBuffMobType);
                    Place(Skills_cmbxBuffMobType, lx, 8, lw, 28);
                }
                catch { }
                int listTop = 44, listH = Math.Max(150, H - 44 - 16);
                ListView[] buffLists = new ListView[]
                {
                    Skills_lstvBuffMobType_General, Skills_lstvBuffMobType_Champion,
                    Skills_lstvBuffMobType_Giant, Skills_lstvBuffMobType_PartyGeneral,
                    Skills_lstvBuffMobType_PartyChampion, Skills_lstvBuffMobType_PartyGiant,
                    Skills_lstvBuffMobType_Unique, Skills_lstvBuffMobType_Elite
                };
                foreach (ListView lv in buffLists)
                {
                    if (lv == null) continue;
                    try
                    {
                        lv.Location = new Point(lx, listTop);
                        lv.Size = new Size(lw, listH);
                        Classicize(lv);
                    }
                    catch { }
                }

                try
                {
                    if (Skills_btnAddBuff != null)
                    {
                        Skills_btnAddBuff.Text = "▶";
                        Classicize(Skills_btnAddBuff);
                        Place(Skills_btnAddBuff, tx, 150, tw, 46);
                    }
                    if (Skills_btnRemBuff != null)
                    {
                        Skills_btnRemBuff.Text = "◀";
                        Classicize(Skills_btnRemBuff);
                        Place(Skills_btnRemBuff, tx, 202, tw, 46);
                    }
                }
                catch { }

                EnsureBuffUpDown(p, ux, uw);

                int y = 8;
                PhBotTodoCheck(p, "PhBot_BuffWhile", "Buff while attacking monsters", ox, y, true); y += 30;
                try
                {
                    if (cbxSkillDevil != null)
                    {
                        if (cbxSkillDevil.Parent != p)
                        {
                            try { cbxSkillDevil.Parent.Controls.Remove(cbxSkillDevil); } catch { }
                            try { p.Controls.Add(cbxSkillDevil); } catch { }
                        }
                        cbxSkillDevil.Text = "Use Devil's Spirit / Angel's Spirit";
                        Classicize(cbxSkillDevil);
                        cbxSkillDevil.AutoSize = true;
                        cbxSkillDevil.Location = new Point(ox, y);
                        cbxSkillDevil.Visible = true;
                    }
                }
                catch { }
                y += 30;
                PhBotTodoCheck(p, "PhBot_Mirror", "Cast Mirror Reflect", ox, y, true); y += 30;
                PhBotTodoCheck(p, "PhBot_NoSwitchWpn", "Do not switch weapons for buffing", ox, y, false); y += 30;
                PhBotTodoCheck(p, "PhBot_EmergHP", "Emergency buff HP <", ox, y, false);
                PhBotTodoNumber(p, "PhBot_EmergHPN", ox + 190, y - 2, 60, "50");
                PhBotLabel(p, "PhBot_EmergHPL", "%", ox + 256, y + 3); y += 30;
                PhBotTodoCheck(p, "PhBot_EmergMP", "Emergency buff MP <", ox, y, false);
                PhBotTodoNumber(p, "PhBot_EmergMPN", ox + 190, y - 2, 60, "50");
                PhBotLabel(p, "PhBot_EmergMPL", "%", ox + 256, y + 3); y += 30;
                PhBotTodoCheck(p, "PhBot_MobAttacking", "Monsters attacking", ox, y, false);
                PhBotTodoNumber(p, "PhBot_MobAttackingN", ox + 190, y - 2, 60, "0"); y += 30;
                PhBotLabel(p, "PhBot_BadStatusL", "Bad status", ox, y + 3); y += 24;
                ComboBox bad = EnsureBadStatusCombo(p, ox, y, 420); y += 34;
                PhBotTodoCheck(p, "PhBot_RecastAll", "Recast all buffs when one buff ends if it requires a weapon switch", ox, y, false);
                try
                {
                    CheckBox rc = p.Controls["PhBot_RecastAll"] as CheckBox;
                    if (rc != null) { rc.MaximumSize = new Size(360, 0); }
                }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("buffs inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private void EnsureBuffUpDown(Panel p, int ux, int uw)
        {
            try
            {
                Button up = p.Controls["PhBot_BuffUp"] as Button;
                if (up == null)
                {
                    up = new Button();
                    up.Name = "PhBot_BuffUp";
                    up.Text = "▲";
                    up.Font = PhBotFont();
                    try { p.Controls.Add(up); } catch { }
                    up.Click += (s, e) =>
                    {
                        try
                        {
                            ListView lv = Skills_cmbxBuffMobType.Tag as ListView;
                            if (lv == null) lv = Skills_lstvBuffMobType_General;
                            if (lv != null)
                            {
                                SkillManager.MoveSelectedItemUp(lv);
                                Settings.SaveCharacterSettings();
                            }
                        }
                        catch { }
                    };
                }
                Button down = p.Controls["PhBot_BuffDown"] as Button;
                if (down == null)
                {
                    down = new Button();
                    down.Name = "PhBot_BuffDown";
                    down.Text = "▼";
                    down.Font = PhBotFont();
                    try { p.Controls.Add(down); } catch { }
                    down.Click += (s, e) =>
                    {
                        try
                        {
                            ListView lv = Skills_cmbxBuffMobType.Tag as ListView;
                            if (lv == null) lv = Skills_lstvBuffMobType_General;
                            if (lv != null)
                            {
                                SkillManager.MoveSelectedItemDown(lv);
                                Settings.SaveCharacterSettings();
                            }
                        }
                        catch { }
                    };
                }
                Classicize(up);
                Classicize(down);
                Place(up, ux, 150, uw, 46);
                Place(down, ux, 202, uw, 46);
            }
            catch (Exception ex) { PhBotDebug("buff updown: " + ex.Message); }
        }

        private ComboBox EnsureBadStatusCombo(Panel p, int x, int y, int w)
        {
            ComboBox cb = null;
            try
            {
                cb = p.Controls["PhBot_BadStatus"] as ComboBox;
                if (cb == null)
                {
                    cb = new ComboBox();
                    cb.Name = "PhBot_BadStatus";
                    cb.DropDownStyle = ComboBoxStyle.DropDownList;
                    cb.Items.Add("None");
                    cb.SelectedIndex = 0;
                    try { p.Controls.Add(cb); } catch { }
                }
                GrayCombo(cb);
                Place(cb, x, y, w, 28);
            }
            catch { }
            return cb;
        }

        // ---------------------------------------------------------------
        // POTIONS — docs/phbot_ref/protection_potions.png
        // Grup kutuları çözülür, satırlar phBot sırasına dizilir.
        // ---------------------------------------------------------------
        private void LayoutPotionsInner()
        {
            if (_innerLayout) return;
            _innerLayout = true;
            try
            {
                Panel p = TabPageH_Character_Option02_Panel;
                if (p == null) return;
                int W = p.Width;
                if (W < 500) return;
                try { p.AutoScroll = true; } catch { }

                // Grup kutularını çöz (kontroller aynı nesne kalır).
                GroupBox[] boxes = new GroupBox[] { Character_gbxPotionsPlayer, Character_gbxPotionPet };
                foreach (GroupBox gbx in boxes)
                {
                    if (gbx == null) continue;
                    try
                    {
                        if (gbx.Parent != p) continue;
                        var kids = new List<Control>();
                        foreach (Control k in gbx.Controls) kids.Add(k);
                        foreach (Control k in kids)
                        {
                            try { gbx.Controls.Remove(k); p.Controls.Add(k); } catch { }
                        }
                        gbx.Visible = false;
                    }
                    catch { }
                }

                int y = 10;
                const int step = 30;
                const int cx = 8, cw = 215;     // checkbox
                const int px = 232, pw = 52;    // yüzde kutusu
                const int pctX = 290;           // % etiketi
                const int dx = 322, dw = 72;    // gecikme kutusu

                y = PotionRow(p, Character_cbxUseHP, "Auto use HP", Character_tbxUseHP, 1000, y, step, cx, cw, px, pw, pctX, dx, dw);
                y = PotionRow(p, Character_cbxUseHPGrain, "Use HP grains", null, -1, y, step, cx, cw, px, pw, pctX, dx, dw);
                PhBotTodoCheck(p, "PhBot_PreferHPGrain", "Prefer HP grains", cx, y, false); y += step;
                y = PotionRow(p, Character_cbxUseMP, "Auto use MP", Character_tbxUseMP, 1000, y, step, cx, cw, px, pw, pctX, dx, dw);
                y = PotionRow(p, Character_cbxUseMPGrain, "Use MP grains", null, -1, y, step, cx, cw, px, pw, pctX, dx, dw);
                PhBotTodoCheck(p, "PhBot_PreferMPGrain", "Prefer MP grains", cx, y, false); y += step;
                PhBotTodoCheck(p, "PhBot_SitHP", "Auto sit HP", cx, y, false);
                PhBotTodoNumber(p, "PhBot_SitHPN", px, y - 2, pw, "0");
                PhBotLabel(p, "PhBot_SitHPL", "%", pctX, y + 3); y += step;
                PhBotTodoCheck(p, "PhBot_SitMP", "Auto sit MP", cx, y, false);
                PhBotTodoNumber(p, "PhBot_SitMPN", px, y - 2, pw, "0");
                PhBotLabel(p, "PhBot_SitMPL", "%", pctX, y + 3); y += step;
                y = PotionRow(p, Character_cbxUseHPVigor, "Auto vigor HP", Character_tbxUseHPVigor, 1000, y, step, cx, cw, px, pw, pctX, dx, dw);
                y = PotionRow(p, Character_cbxUseMPVigor, "Auto vigor MP", Character_tbxUseMPVigor, 1000, y, step, cx, cw, px, pw, pctX, dx, dw);
                y = PotionRow(p, Character_cbxUseTransportHP, "Auto heal transport", Character_tbxUseTransportHP, -1, y, step, cx, cw, px, pw, pctX, dx, dw);
                y = PotionRow(p, Character_cbxUsePetHP, "Auto heal attack pet", Character_tbxUsePetHP, -1, y, step, cx, cw, px, pw, pctX, dx, dw);
                y = PotionRow(p, Character_cbxUsePillUniversal, "Auto use universal pills", null, 1000, y, step, cx, cw, px, pw, pctX, dx, dw);
                y = PotionRow(p, Character_cbxUsePillPurification, "Auto use purification pills", null, 1000, y, step, cx, cw, px, pw, pctX, dx, dw);
                y = PotionRow(p, Character_cbxUsePetsPill, "Auto use cure potions on pet/transport", null, -1, y, step, cx, cw, px, pw, pctX, dx, dw);
                y = PotionRow(p, Character_cbxUsePetHGP, "Auto use HGP potions", Character_tbxUsePetHGP, 80, y, step, cx, cw, px, pw, pctX, dx, dw);
                PhBotTodoCheck(p, "PhBot_InvisDetect", "Auto use Invisibility Detection", cx, y, false); y += step;
            }
            catch (Exception ex) { PhBotDebug("potions inner: " + ex.Message); }
            finally { _innerLayout = false; }
        }

        private int PotionRow(Panel p, CheckBox cbx, string text, TextBox percent, int delay,
            int y, int step, int cx, int cw, int px, int pw, int pctX, int dx, int dw)
        {
            try
            {
                if (cbx != null)
                {
                    cbx.Text = text;
                    Classicize(cbx);
                    cbx.AutoSize = false;
                    cbx.Location = new Point(cx, y);
                    cbx.Size = new Size(cw, 24);
                    cbx.Visible = true;
                }
                if (percent != null)
                {
                    Classicize(percent);
                    percent.TextAlign = HorizontalAlignment.Center;
                    try { percent.MaxLength = 3; } catch { }
                    Place(percent, px, y, pw, 24);
                    PhBotLabel(p, "PhBot_Pct_" + (cbx != null ? cbx.Name : y.ToString()), "%", pctX, y + 3);
                }
                if (delay >= 0)
                {
                    string dn = "PhBot_Delay_" + (cbx != null ? cbx.Name : y.ToString());
                    TextBox d = p.Controls[dn] as TextBox;
                    if (d == null)
                    {
                        d = new TextBox();
                        d.Name = dn;
                        d.Font = PhBotFont();
                        d.BackColor = Color.White;
                        d.ForeColor = Color.Black;
                        d.BorderStyle = BorderStyle.FixedSingle;
                        try { p.Controls.Add(d); } catch { }
                    }
                    d.Location = new Point(dx, y);
                    d.Size = new Size(dw, 24);
                    d.Visible = true;
                    // TODO backend: gecikme değeri PotionPolicy'e bağlanacak.
                    if (string.IsNullOrEmpty(d.Text)) d.Text = delay.ToString();
                }
            }
            catch { }
            return y + step;
        }
    }
}
