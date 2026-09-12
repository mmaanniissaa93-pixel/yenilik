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
        private readonly HashSet<string> _activeInnerLayouts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private bool BeginInnerLayout([System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            if (string.IsNullOrEmpty(caller)) return true;
            return _activeInnerLayouts.Add(caller);
        }

        private void EndInnerLayout([System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            if (!string.IsNullOrEmpty(caller))
                _activeInnerLayouts.Remove(caller);
        }

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
        private static CheckBox PhBotTodoCheck(Control p, string name, string text, int x, int y, bool check)
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

        private static TextBox PhBotTodoNumber(Control p, string name, int x, int y, int w, string text)
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

        private static CheckBox BindAttackCheck(Control p, string name, string text, int x, int y, bool currentVal, Action<bool> onValChanged)
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
                c.Checked = currentVal;
                if (onValChanged != null)
                {
                    c.CheckedChanged += (s, e) => onValChanged(c.Checked);
                }
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

        private static TextBox BindAttackText(Control p, string name, int x, int y, int w, string currentVal, Action<string> onValChanged)
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
                t.Text = currentVal ?? "";
                if (onValChanged != null)
                {
                    t.TextChanged += (s, e) => onValChanged(t.Text);
                }
                try { p.Controls.Add(t); } catch { }
            }
            try
            {
                t.Location = new Point(x, y);
                t.Size = new Size(w, 24);
                t.Visible = true;
            }
            catch { }
            return t;
        }


        private static Label PhBotLabel(Control p, string name, string text, int x, int y)
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
            if (!BeginInnerLayout()) return;
            try
            {
                Panel p = TabPageH_Skills_Option01_Panel;
                Panel hostV = TabPageV_Control01_Skills_Panel;
                if (p == null || hostV == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;

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
                        tc.Location = new Point(190, 0);
                        tc.Size = new Size(Math.Max(200, hostV.Width - 190), hostV.Height);
                    }
                }
                catch { }

                // Sol: skill listesi (V panel seviyesinde, tam yükseklik).
                try
                {
                    if (Skills_lstvSkills != null)
                    {
                        Skills_lstvSkills.Location = new Point(0, 0);
                        Skills_lstvSkills.Size = new Size(185, hostV.Height);
                        Skills_lstvSkills.Visible = true;
                        Classicize(Skills_lstvSkills);
                    }
                }
                catch { }

                const int tx = 12, tw = 32, th = 26; // transfer butonları
                const int lx = 50, lw = 175;         // liste + combo
                const int ux = 232, uw = 32, uh = 26; // yukarı/aşağı
                const int ox = 275;                  // sağ seçenekler

                // Type kombo (gri) + 9 mob-tipi listesi üst üste.
                try
                {
                    GrayCombo(Skills_cmbxAttackMobType);
                    Place(Skills_cmbxAttackMobType, lx, 8, lw, 24);
                }
                catch { }
                int listTop = 36, listH = 195;
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

                // Transfer ►◄
                try
                {
                    if (Skills_btnAddAttack != null)
                    {
                        Skills_btnAddAttack.Text = "►";
                        Classicize(Skills_btnAddAttack);
                        Place(Skills_btnAddAttack, tx, 90, tw, th);
                    }
                    if (Skills_btnRemAttack != null)
                    {
                        Skills_btnRemAttack.Text = "◄";
                        Classicize(Skills_btnRemAttack);
                        Place(Skills_btnRemAttack, tx, 125, tw, th);
                    }
                }
                catch { }

                // Yukarı/aşağı
                try
                {
                    foreach (Control c in p.Controls)
                    {
                        Button b = c as Button;
                        if (b == null) continue;
                        if (b.Text != "▲" && b.Text != "▼") continue;
                        if (b.Name.StartsWith("PhBot_")) continue;
                        Classicize(b);
                        if (b.Text == "▲") Place(b, ux, 90, uw, uh);
                        else Place(b, ux, 125, uw, uh);
                    }
                }
                catch { }

                try { p.AutoScroll = false; } catch { }

                // Sağ seçenek sütunu (phBot attack.png 7..18 sırası).
                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                int y = 8;
                try
                {
                    if (Skills_cbxCastInOrder != null)
                    {
                        Skills_cbxCastInOrder.Text = isTR ? "Sırayla beceri kullan" : "Cast skills in order";
                        Classicize(Skills_cbxCastInOrder);
                        Skills_cbxCastInOrder.AutoSize = true;
                        Skills_cbxCastInOrder.Location = new Point(ox, y);
                        Skills_cbxCastInOrder.Visible = true;
                    }
                }
                catch { }
                y += 21;

                BindAttackCheck(p, "PhBot_KillSteal", isTR ? "Canavarlara ks at" : "Kill steal monsters", ox, y, CombatAIEngine.KillSteal, v => CombatAIEngine.KillSteal = v);
                y += 21;

                try
                {
                    if (cbxSkillNoAttack != null)
                    {
                        cbxSkillNoAttack.Text = isTR ? "Canavarlara saldırma" : "Don't attack monsters";
                        Classicize(cbxSkillNoAttack);
                        cbxSkillNoAttack.AutoSize = true;
                        cbxSkillNoAttack.Location = new Point(ox, y);
                        cbxSkillNoAttack.Visible = true;
                    }
                }
                catch { }
                y += 21;

                BindAttackCheck(p, "PhBot_ProtectParty", isTR ? "Parti üyelerini koru" : "Protect party members", ox, y, CombatAIEngine.ProtectParty, v => CombatAIEngine.ProtectParty = v);
                BindAttackText(p, "PhBot_ProtectName", ox + 150, y - 2, 85, CombatAIEngine.ProtectPartyTarget, v => CombatAIEngine.ProtectPartyTarget = v);
                y += 21;

                BindAttackCheck(p, "PhBot_AttackLower", isTR ? "Önce düşük seviyeli canavarlara saldır" : "Attack lower monsters first", ox, y, CombatAIEngine.AttackLowerFirst, v => CombatAIEngine.AttackLowerFirst = v);
                y += 21;

                BindAttackCheck(p, "PhBot_SwitchDot", isTR ? "DOT tan sonra canavar değiştir" : "Switch monster after DOT", ox, y, CombatAIEngine.SwitchMonsterAfterDot, v => CombatAIEngine.SwitchMonsterAfterDot = v);
                BindAttackText(p, "PhBot_SwitchDotN", ox + 185, y - 2, 35, CombatAIEngine.SwitchMonsterDotDelay.ToString(), v => { if (int.TryParse(v, out int d)) CombatAIEngine.SwitchMonsterDotDelay = d; });
                y += 21;

                CheckBox cbLower = BindAttackCheck(p, "PhBot_LowerSkills", isTR ? "Belirli bir canavar tipi için beceri yoksa daha düşük becerileri kullan" : "Use lower skills if skills for a specific monster type do not exist", ox, y, CombatAIEngine.UseLowerSkills, v => CombatAIEngine.UseLowerSkills = v);
                if (cbLower != null) { cbLower.AutoSize = false; cbLower.Size = new Size(240, 28); }
                y += 30;

                BindAttackCheck(p, "PhBot_SlowerAttack", isTR ? "Yavaş saldırı modu" : "Slower attack mode", ox, y, CombatAIEngine.SlowerAttackMode, v => CombatAIEngine.SlowerAttackMode = v);
                y += 21;

                BindAttackCheck(p, "PhBot_Lagtastic", "Lagtastic", ox, y, CombatAIEngine.Lagtastic, v => CombatAIEngine.Lagtastic = v);
                y += 21;

                CheckBox cbUniq = BindAttackCheck(p, "PhBot_AutoUnique", isTR ? "Yakındaki benzersizleri otomatik seç (bot kapalı olmalı)" : "Auto select nearby uniques (must not be botting)", ox, y, CombatAIEngine.AutoSelectUniques, v => CombatAIEngine.AutoSelectUniques = v);
                if (cbUniq != null) { cbUniq.AutoSize = false; cbUniq.Size = new Size(240, 28); }
                y += 30;

                CheckBox cbTitan = BindAttackCheck(p, "PhBot_AutoTitan", isTR ? "Yakındaki titanları otomatik seç (bot kapalı olmalı)" : "Auto select nearby titans (must not be botting)", ox, y, CombatAIEngine.AutoSelectTitans, v => CombatAIEngine.AutoSelectTitans = v);
                if (cbTitan != null) { cbTitan.AutoSize = false; cbTitan.Size = new Size(240, 28); }
                y += 30;

                BindAttackCheck(p, "PhBot_TeleportSkill", isTR ? "Işınlanma becerilerini kullan" : "Use teleport skills", ox, y, CombatAIEngine.UseTeleportSkills, v => CombatAIEngine.UseTeleportSkills = v);

                // Alt: Imbue.
                try
                {
                    if (lblSkillImbue != null)
                    {
                        lblSkillImbue.Text = "Imbue";
                        Classicize(lblSkillImbue);
                        lblSkillImbue.AutoSize = true;
                        lblSkillImbue.Location = new Point(lx, 236);
                        lblSkillImbue.Visible = true;
                    }
                    if (cmbxImbue != null)
                    {
                        GrayCombo(cmbxImbue);
                        Place(cmbxImbue, lx, 256, lw, 24);
                    }
                }
                catch { }
                try
                {
                    if (lblSkillRuntimeStatus != null)
                    {
                        lblSkillRuntimeStatus.Visible = false;
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
                try { if (Training_cbxWalkToCenter != null && Training_cbxWalkToCenter.Parent == p) Training_cbxWalkToCenter.Visible = false; }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("attack inner: " + ex.Message); }
            finally { EndInnerLayout(); }
        }

        // ---------------------------------------------------------------
        // BUFFS — docs/phbot_ref/buffs.png
        // ---------------------------------------------------------------
        private void LayoutBuffsInner()
        {
            if (!BeginInnerLayout()) return;
            try
            {
                Panel p = TabPageH_Skills_Option02_Panel;
                if (p == null) return;
                int W = p.Width, H = p.Height;
                if (W < 200 || H < 100) return;

                p.AutoScroll = false;
                const int tx = 12, tw = 32, th = 26;
                const int lx = 50, lw = 175;
                const int ux = 232, uw = 32;
                const int ox = 275;

                try
                {
                    GrayCombo(Skills_cmbxBuffMobType);
                    Place(Skills_cmbxBuffMobType, lx, 8, lw, 24);
                }
                catch { }
                int listTop = 36, listH = 195;
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
                        Skills_btnAddBuff.Text = "►";
                        Classicize(Skills_btnAddBuff);
                        Place(Skills_btnAddBuff, tx, 90, tw, th);
                    }
                    if (Skills_btnRemBuff != null)
                    {
                        Skills_btnRemBuff.Text = "◄";
                        Classicize(Skills_btnRemBuff);
                        Place(Skills_btnRemBuff, tx, 125, tw, th);
                    }
                }
                catch { }

                EnsureBuffUpDown(p, ux, uw);

                int y = 8;
                PhBotTodoCheck(p, "PhBot_BuffWhile", "Buff while attacking monsters", ox, y, true); y += 24;
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
                y += 24;
                PhBotTodoCheck(p, "PhBot_Mirror", "Cast Mirror Reflect", ox, y, true); y += 24;
                PhBotTodoCheck(p, "PhBot_NoSwitchWpn", "Do not switch weapons for buffing", ox, y, false); y += 24;
                PhBotTodoCheck(p, "PhBot_EmergHP", "Emergency buff HP <", ox, y, false);
                PhBotTodoNumber(p, "PhBot_EmergHPN", ox + 160, y - 2, 50, "50");
                PhBotLabel(p, "PhBot_EmergHPL", "%", ox + 216, y + 3); y += 24;
                PhBotTodoCheck(p, "PhBot_EmergMP", "Emergency buff MP <", ox, y, false);
                PhBotTodoNumber(p, "PhBot_EmergMPN", ox + 160, y - 2, 50, "50");
                PhBotLabel(p, "PhBot_EmergMPL", "%", ox + 216, y + 3); y += 24;
                PhBotTodoCheck(p, "PhBot_MobAttacking", "Monsters attacking", ox, y, false);
                PhBotTodoNumber(p, "PhBot_MobAttackingN", ox + 160, y - 2, 50, "0"); y += 24;
                PhBotLabel(p, "PhBot_BadStatusL", "Bad status", ox, y + 3); y += 20;
                ComboBox bad = EnsureBadStatusCombo(p, ox, y, 260); y += 28;
                PhBotTodoCheck(p, "PhBot_RecastAll", "Recast all buffs when one buff ends if it\nrequires a weapon switch", ox, y, false);
                try
                {
                    CheckBox rc = p.Controls["PhBot_RecastAll"] as CheckBox;
                    if (rc != null)
                    {
                        rc.Text = "Recast all buffs when one buff ends if it\nrequires a weapon switch";
                        rc.AutoSize = false;
                        rc.Size = new Size(250, 36);
                    }
                }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("buffs inner: " + ex.Message); }
            finally { EndInnerLayout(); }
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
                Place(up, ux, 90, uw, 26);
                Place(down, ux, 125, uw, 26);
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
            if (!BeginInnerLayout()) return;
            try
            {
                Panel p = TabPageH_Character_Option02_Panel;
                if (p == null) return;
                int W = p.Width;
                if (W < 200) return;
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

                try { p.AutoScroll = false; } catch { }
                bool isTR = LocalizationManager.CurrentLanguage == "TR";
                int y = 10;
                const int step = 23;
                const int leftX = 14;
                const int rightX = 360;

                // --- SOL SÜTUN (Karakter Potları) ---
                // 1: Oto HP [ 0 ] % [ 1000 ]
                PlacePotionRowFixed(p, Character_cbxUseHP, isTR ? "Oto HP" : "Auto use HP", 122, Character_tbxUseHP, 140, 52, 196, 1000, 216, 52, leftX, y);
                y += step;

                // 2: HP grain kullan [ ] HP grain tercih et
                PlacePotionCheck(p, Character_cbxUseHPGrain, isTR ? "HP grain kullan" : "Use HP grains", leftX, y, 108);
                PhBotTodoCheck(p, "PhBot_PreferHPGrain", isTR ? "HP grain tercih et" : "Prefer HP grains", 125, y, false);
                y += step;

                // 3: Oto MP [ 0 ] % [ 1000 ]
                PlacePotionRowFixed(p, Character_cbxUseMP, isTR ? "Oto MP" : "Auto use MP", 122, Character_tbxUseMP, 140, 52, 196, 1000, 216, 52, leftX, y);
                y += step;

                // 4: MP grain kullan [ ] MP grain tercih et
                PlacePotionCheck(p, Character_cbxUseMPGrain, isTR ? "MP grain kullan" : "Use MP grains", leftX, y, 108);
                PhBotTodoCheck(p, "PhBot_PreferMPGrain", isTR ? "MP grain tercih et" : "Prefer MP grains", 125, y, false);
                y += step;

                // 5: HP Otur [ 0 ] %
                CheckBox cbSitHP = PhBotTodoCheck(p, "PhBot_SitHP", isTR ? "HP Otur" : "Auto sit HP", leftX, y, false);
                cbSitHP.Size = new Size(120, 20);
                PhBotTodoNumber(p, "PhBot_SitHPN", 140, y - 1, 52, "0");
                PhBotLabel(p, "PhBot_SitHPL", "%", 196, y + 2);
                y += step;

                // 6: MP Otur [ 0 ] %
                CheckBox cbSitMP = PhBotTodoCheck(p, "PhBot_SitMP", isTR ? "MP Otur" : "Auto sit MP", leftX, y, false);
                cbSitMP.Size = new Size(120, 20);
                PhBotTodoNumber(p, "PhBot_SitMPN", 140, y - 1, 52, "0");
                PhBotLabel(p, "PhBot_SitMPL", "%", 196, y + 2);
                y += step;

                // 7: Oto Vigor HP [ 0 ] % [ 1000 ]
                PlacePotionRowFixed(p, Character_cbxUseHPVigor, isTR ? "Oto Vigor HP" : "Auto vigor HP", 122, Character_tbxUseHPVigor, 140, 52, 196, 1000, 216, 52, leftX, y);
                y += step;

                // 8: Oto Vigor MP [ 0 ] % [ 1000 ]
                PlacePotionRowFixed(p, Character_cbxUseMPVigor, isTR ? "Oto Vigor MP" : "Auto vigor MP", 122, Character_tbxUseMPVigor, 140, 52, 196, 1000, 216, 52, leftX, y);
                y += step;

                // 9: Universal pill kullan [ 1000 ]
                PlacePotionRowFixed(p, Character_cbxUsePillUniversal, isTR ? "Universal pill kullan" : "Auto use universal pills", 200, null, -1, 0, -1, 1000, 216, 52, leftX, y);
                y += step;

                // 10: Purification pill kullan [ 1000 ]
                PlacePotionRowFixed(p, Character_cbxUsePillPurification, isTR ? "Purification pill kullan" : "Auto use purification pills", 200, null, -1, 0, -1, 1000, 216, 52, leftX, y);
                y += step;

                // 11: Invisibility Detection kullan
                PlacePotionCheck(p, PhBotTodoCheck(p, "PhBot_InvisDetect", isTR ? "Invisibility Detection kullan" : "Auto use Invisibility Detection", leftX, y, false), isTR ? "Invisibility Detection kullan" : "Auto use Invisibility Detection", leftX, y, 220);

                // --- SAĞ SÜTUN (Pet / Kervan Potları) ---
                int ry = 10;
                // 1: Pet için HP kullan [ 0 ] %
                PlacePotionRowFixed(p, Character_cbxUsePetHP, isTR ? "Pet için HP kullan" : "Auto heal attack pet", 132, Character_tbxUsePetHP, 495, 52, 551, -1, -1, 0, rightX, ry);
                ry += step;

                // 2: Kervan için HP kullan [ 0 ] %
                PlacePotionRowFixed(p, Character_cbxUseTransportHP, isTR ? "Kervan için HP kullan" : "Auto heal transport", 132, Character_tbxUseTransportHP, 495, 52, 551, -1, -1, 0, rightX, ry);
                ry += step;

                // 3: Pet/Kervan için kötü durum potu kullan
                PlacePotionCheck(p, Character_cbxUsePetsPill, isTR ? "Pet/Kervan için kötü durum potu kullan" : "Auto use cure potions on pet/transport", rightX, ry, 260);
                ry += step;

                // 4: Otomatik HGP kullan [ 80 ] %
                if (Character_tbxUsePetHGP != null && (string.IsNullOrEmpty(Character_tbxUsePetHGP.Text) || Character_tbxUsePetHGP.Text == "0"))
                    Character_tbxUsePetHGP.Text = "80";
                PlacePotionRowFixed(p, Character_cbxUsePetHGP, isTR ? "Otomatik HGP kullan" : "Auto use HGP potions", 132, Character_tbxUsePetHGP, 495, 52, 551, -1, -1, 0, rightX, ry);
                ry += step + 8;

                // --- BECERİ VE PET KORUMASI (ProtectionManager) ---
                PhBotLabel(p, "lblSkillHealSection", isTR ? "--- Beceri ile Koruma ---" : "--- Skill Recovery ---", rightX, ry);
                ry += step;

                if (cbxProtectionSkillHP != null)
                {
                    MoveTo(cbxProtectionSkillHP, p, rightX, ry);
                    cbxProtectionSkillHP.Text = isTR ? "HP < % ise skill bas" : "Heal skill if HP < %";
                    Classicize(cbxProtectionSkillHP);
                    cbxProtectionSkillHP.AutoSize = true;
                    cbxProtectionSkillHP.Visible = true;
                }
                if (nudProtectionSkillHP != null)
                {
                    MoveTo(nudProtectionSkillHP, p, rightX + (isTR ? 148 : 138), ry - 2);
                    nudProtectionSkillHP.Font = PhBotFont();
                    nudProtectionSkillHP.BackColor = Color.White;
                    nudProtectionSkillHP.ForeColor = Color.Black;
                    nudProtectionSkillHP.Size = new Size(45, 22);
                    nudProtectionSkillHP.Visible = true;
                }
                ry += step;

                if (cbxProtectionSkillMP != null)
                {
                    MoveTo(cbxProtectionSkillMP, p, rightX, ry);
                    cbxProtectionSkillMP.Text = isTR ? "MP < % ise skill bas" : "Mana skill if MP < %";
                    Classicize(cbxProtectionSkillMP);
                    cbxProtectionSkillMP.AutoSize = true;
                    cbxProtectionSkillMP.Visible = true;
                }
                if (nudProtectionSkillMP != null)
                {
                    MoveTo(nudProtectionSkillMP, p, rightX + (isTR ? 148 : 138), ry - 2);
                    nudProtectionSkillMP.Font = PhBotFont();
                    nudProtectionSkillMP.BackColor = Color.White;
                    nudProtectionSkillMP.ForeColor = Color.Black;
                    nudProtectionSkillMP.Size = new Size(45, 22);
                    nudProtectionSkillMP.Visible = true;
                }
                ry += step;

                if (cbxProtectionCure != null)
                {
                    MoveTo(cbxProtectionCure, p, rightX, ry);
                    cbxProtectionCure.Text = isTR ? "Kötü durumu skill ile temizle" : "Cure bad status with skill";
                    Classicize(cbxProtectionCure);
                    cbxProtectionCure.AutoSize = true;
                    cbxProtectionCure.Visible = true;
                }
                ry += step;

                if (cbxProtectionPetRevive != null)
                {
                    MoveTo(cbxProtectionPetRevive, p, rightX, ry);
                    cbxProtectionPetRevive.Text = isTR ? "Ölen peti dirilt (Grass of Life)" : "Revive pet (Grass of Life)";
                    Classicize(cbxProtectionPetRevive);
                    cbxProtectionPetRevive.AutoSize = true;
                    cbxProtectionPetRevive.Visible = true;
                }
                ry += step;

                if (cbxProtectionPetSummon != null)
                {
                    MoveTo(cbxProtectionPetSummon, p, rightX, ry);
                    cbxProtectionPetSummon.Text = isTR ? "Peti otomatik çağır" : "Auto summon pet";
                    Classicize(cbxProtectionPetSummon);
                    cbxProtectionPetSummon.AutoSize = true;
                    cbxProtectionPetSummon.Visible = true;
                }
            }
            catch (Exception ex) { PhBotDebug("potions inner: " + ex.Message); }
            finally { EndInnerLayout(); }
        }

        private void PlacePotionCheck(Panel p, CheckBox cbx, string text, int x, int y, int width)
        {
            if (cbx == null) return;
            cbx.Text = text;
            Classicize(cbx);
            cbx.Font = PhBotFont();
            cbx.AutoSize = false;
            cbx.Location = new Point(x, y);
            cbx.Size = new Size(width, 20);
            cbx.Visible = true;
        }

        private void PlacePotionRowFixed(Panel p, CheckBox cbx, string text, int cbWidth, TextBox percent, int pctX, int pctWidth, int pctLblX, int delay, int delayX, int delayWidth, int x, int y)
        {
            try
            {
                if (cbx != null)
                {
                    cbx.Text = text;
                    Classicize(cbx);
                    cbx.Font = PhBotFont();
                    cbx.AutoSize = false;
                    cbx.Location = new Point(x, y);
                    cbx.Size = new Size(cbWidth, 20);
                    cbx.Visible = true;
                }

                string pn = "PhBot_Pct_" + (cbx != null ? cbx.Name : x.ToString());
                Control pl = p.Controls[pn];
                if (percent != null && pctX >= 0)
                {
                    Classicize(percent);
                    percent.Font = PhBotFont();
                    percent.TextAlign = HorizontalAlignment.Center;
                    try { percent.MaxLength = 3; } catch { }
                    Place(percent, pctX, y - 1, pctWidth, 20);
                    PhBotLabel(p, pn, "%", pctLblX, y + 2);
                }
                else if (pl != null)
                {
                    pl.Visible = false;
                }

                string dn = "PhBot_Delay_" + (cbx != null ? cbx.Name : x.ToString());
                TextBox d = p.Controls[dn] as TextBox;
                if (delay >= 0 && delayX >= 0)
                {
                    if (d == null)
                    {
                        d = new TextBox();
                        d.Name = dn;
                        d.Font = PhBotFont();
                        d.BackColor = Color.White;
                        d.ForeColor = Color.Black;
                        d.BorderStyle = BorderStyle.FixedSingle;
                        d.TextAlign = HorizontalAlignment.Center;
                        try { p.Controls.Add(d); } catch { }
                    }
                    d.Location = new Point(delayX, y - 1);
                    d.Size = new Size(delayWidth, 20);
                    d.Visible = true;
                    if (string.IsNullOrEmpty(d.Text)) d.Text = delay.ToString();
                }
                else if (d != null)
                {
                    d.Visible = false;
                }
            }
            catch { }
        }
    }
}
