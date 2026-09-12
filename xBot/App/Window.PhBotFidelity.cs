using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace xBot.App
{
    /// <summary>
    /// phBot birebir görsel sadakat katmanı.
    /// 1) Sidebar'daki "•" noktalar yerine kodla çizilen siyah glifler
    ///    (gerçek phBot'taki gibi her satırda farklı ikon).
    /// 2) Düz H-buton şeritleri yerine gerçek WinForms TabControl
    ///    (visual style açıkken phBot sekmeleriyle piksel aynı görünür).
    /// Referans: docs/phbot_ref/attack.png, protection_potions.png.
    /// </summary>
    public partial class Window
    {
        private bool _tabSync;

        // ---------------------------------------------------------------
        // Kodla çizilen 20x20 sidebar glifleri (siyah, şeffaf zemin).
        // ---------------------------------------------------------------
        private static class PhBotIcons
        {
            private static readonly Dictionary<string, Image> _cache =
                new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

            public static Image Get(string key)
            {
                if (string.IsNullOrEmpty(key)) return null;
                Image img;
                if (_cache.TryGetValue(key, out img)) return img;
                try
                {
                    var bmp = new Bitmap(20, 20);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.AntiAlias;
                        g.Clear(Color.Transparent);
                        Draw(g, key);
                    }
                    _cache[key] = bmp;
                    return bmp;
                }
                catch { return null; }
            }

            private static void Draw(Graphics g, string key)
            {
                Color ink = Color.FromArgb(30, 30, 30);
                using (Pen p = new Pen(ink, 1.8f))
                {
                    p.StartCap = LineCap.Round;
                    p.EndCap = LineCap.Round;
                    p.LineJoin = LineJoin.Round;
                    using (SolidBrush b = new SolidBrush(ink))
                    using (Font f = new Font("Tahoma", 7f, FontStyle.Bold, GraphicsUnit.Point))
                    {
                        switch (key)
                        {
                            case "xBot":
                            case "ProjectHax":
                                using (Font pf = new Font("Tahoma", 8.5f, FontStyle.Bold, GraphicsUnit.Point))
                                using (SolidBrush pb = new SolidBrush(Color.FromArgb(200, 30, 30)))
                                using (SolidBrush xb = new SolidBrush(Color.FromArgb(30, 30, 30)))
                                {
                                    g.DrawString("x", pf, pb, -1, 1);
                                    g.DrawString("B", pf, xb, 7, 1);
                                }
                                break;
                            case "İstatistikler":
                            case "Statistics": // 3 bar
                                g.FillRectangle(b, 3, 12, 3, 5);
                                g.FillRectangle(b, 8, 8, 3, 9);
                                g.FillRectangle(b, 13, 4, 3, 13);
                                break;
                            case "Silkroad Bağlantısı":
                            case "Silkroad Login": // kapı
                                g.DrawRectangle(p, 5, 2, 10, 16);
                                g.FillEllipse(b, 12, 9, 2, 2);
                                break;
                            case "Otomatik Yapılandırma":
                            case "Auto Configure": // sihirli değnek yıldızı
                                g.DrawLine(p, 4, 16, 12, 8);
                                DrawStar(g, b, 14, 5, 5);
                                g.FillEllipse(b, 4, 3, 1.6f, 1.6f);
                                break;
                            case "Yardımcı":
                            case "Assistant":
                            case "TargetAssist": // konuşma balonu + 3 nokta
                                g.DrawRectangle(p, 3, 5, 14, 10);
                                g.FillEllipse(b, 5.5f, 9, 1.8f, 1.8f);
                                g.FillEllipse(b, 9f, 9, 1.8f, 1.8f);
                                g.FillEllipse(b, 12.5f, 9, 1.8f, 1.8f);
                                break;
                            case "Bildirimler":
                            case "Notifications": // çan
                                g.DrawArc(p, 5, 4, 10, 9, 180, 180);
                                g.DrawLine(p, 5, 9, 5, 14);
                                g.DrawLine(p, 15, 9, 15, 14);
                                g.DrawLine(p, 3, 14, 17, 14);
                                g.FillEllipse(b, 8.5f, 15, 3, 3);
                                g.FillEllipse(b, 9, 1.5f, 2, 2);
                                break;
                            case "Koruma":
                            case "Protection": // kalkan
                                {
                                    Point[] pts = { new Point(10, 2), new Point(16, 5), new Point(16, 10), new Point(10, 18), new Point(4, 10), new Point(4, 5) };
                                    g.DrawPolygon(p, pts);
                                    break;
                                }
                            case "Şehir":
                            case "Town": // banka
                                g.DrawLine(p, 3, 7, 10, 2);
                                g.DrawLine(p, 10, 2, 17, 7);
                                g.DrawLine(p, 4, 8, 4, 15);
                                g.DrawLine(p, 10, 8, 10, 15);
                                g.DrawLine(p, 16, 8, 16, 15);
                                g.DrawLine(p, 2, 17, 18, 17);
                                break;
                            case "Kasılma Alanı":
                            case "Training Area": // hedef
                                g.DrawEllipse(p, 4, 4, 12, 12);
                                g.DrawEllipse(p, 7, 7, 6, 6);
                                g.FillEllipse(b, 9, 9, 2, 2);
                                g.DrawLine(p, 10, 1, 10, 4);
                                g.DrawLine(p, 10, 16, 10, 19);
                                g.DrawLine(p, 1, 10, 4, 10);
                                g.DrawLine(p, 16, 10, 19, 10);
                                break;
                            case "Saldırı":
                            case "Attack": // kılıç (çapraz)
                                using (Pen bladePen = new Pen(ink, 2.6f))
                                {
                                    g.DrawLine(bladePen, 5, 15, 14, 6);
                                }
                                g.DrawLine(p, 4, 13, 8, 17);
                                g.DrawLine(p, 14, 4, 16, 6);
                                g.DrawLine(p, 14, 6, 16, 4);
                                break;
                            case "Pet": // pati
                                g.FillEllipse(b, 6, 10, 8, 6);
                                g.FillEllipse(b, 3.5f, 6, 3.4f, 4);
                                g.FillEllipse(b, 8.3f, 4.5f, 3.4f, 4);
                                g.FillEllipse(b, 13.2f, 6, 3.4f, 4);
                                break;
                            case "Parti":
                            case "Party": // iki kişi
                                g.DrawEllipse(p, 3, 5, 5, 5);
                                g.DrawArc(p, 1, 11, 9, 8, 180, 180);
                                g.DrawEllipse(p, 12, 5, 5, 5);
                                g.DrawArc(p, 10, 11, 9, 8, 180, 180);
                                break;
                            case "Birlik Partisi":
                            case "Union Party": // üç halka
                                g.DrawEllipse(p, 2, 7, 5, 5);
                                g.DrawEllipse(p, 7.5f, 7, 5, 5);
                                g.DrawEllipse(p, 13, 7, 5, 5);
                                g.DrawLine(p, 7, 9.5f, 7.5f, 9.5f);
                                break;
                            case "Toplama Filtresi":
                            case "Pick Filter": // huni
                                {
                                    Point[] pts = { new Point(3, 3), new Point(17, 3), new Point(11, 11), new Point(11, 17), new Point(9, 17), new Point(9, 11) };
                                    g.DrawPolygon(p, pts);
                                    break;
                                }
                            case "Görev":
                            case "Quest": // ampul
                                g.DrawEllipse(p, 5, 2, 10, 9);
                                g.DrawLine(p, 8, 12, 8, 16);
                                g.DrawLine(p, 12, 12, 12, 16);
                                g.DrawLine(p, 7, 14, 13, 14);
                                g.DrawLine(p, 8, 17, 12, 17);
                                break;
                            case "Oyuncular":
                            case "Players": // tek kişi + liste çizgisi
                                g.DrawEllipse(p, 4, 3, 6, 6);
                                g.DrawArc(p, 2, 10, 10, 9, 180, 180);
                                g.DrawLine(p, 15, 5, 15, 16);
                                g.DrawLine(p, 13, 7, 17, 7);
                                break;
                            case "Lonca":
                            case "Guild": // bayrak
                                g.DrawLine(p, 5, 2, 5, 18);
                                {
                                    Point[] pts = { new Point(5, 3), new Point(16, 3), new Point(13, 6), new Point(16, 9), new Point(5, 9) };
                                    g.DrawPolygon(p, pts);
                                }
                                break;
                            case "Akademi":
                            case "Academy": // kep
                                {
                                    Point[] pts = { new Point(10, 3), new Point(18, 7), new Point(10, 11), new Point(2, 7) };
                                    g.DrawPolygon(p, pts);
                                    g.DrawLine(p, 6, 9, 6, 14);
                                    g.DrawLine(p, 6, 14, 12, 14);
                                }
                                break;
                            case "Envanter":
                            case "Inventory": // sandık
                                g.DrawRectangle(p, 3, 7, 14, 10);
                                g.DrawLine(p, 3, 7, 3, 5);
                                g.DrawLine(p, 17, 7, 17, 5);
                                g.DrawLine(p, 3, 5, 17, 5);
                                g.FillRectangle(b, 9, 10, 2.4f, 4);
                                break;
                            case "Tezgah":
                            case "Stall": // tente + tezgah
                                g.DrawArc(p, 3, 3, 5, 5, 180, 180);
                                g.DrawArc(p, 8, 3, 5, 5, 180, 180);
                                g.DrawArc(p, 13, 3, 5, 5, 180, 180);
                                g.DrawLine(p, 3, 8, 18, 8);
                                g.DrawRectangle(p, 5, 10, 11, 7);
                                break;
                            case "Kervan":
                            case "Trade": // çift yön oku
                                g.DrawLine(p, 3, 7, 15, 7);
                                g.DrawLine(p, 12, 4, 15, 7);
                                g.DrawLine(p, 12, 10, 15, 7);
                                g.DrawLine(p, 17, 13, 5, 13);
                                g.DrawLine(p, 8, 10, 5, 13);
                                g.DrawLine(p, 8, 16, 5, 13);
                                break;
                            case "Simya":
                            case "Alchemy": // şişe
                                g.DrawLine(p, 8, 2, 8, 7);
                                g.DrawLine(p, 12, 2, 12, 7);
                                {
                                    Point[] pts = { new Point(8, 7), new Point(12, 7), new Point(16, 17), new Point(4, 17) };
                                    g.DrawPolygon(p, pts);
                                }
                                g.DrawLine(p, 7, 13, 13, 13);
                                break;
                            case "Ustalıklar":
                            case "Masteries": // katmanlar
                                {
                                    Point[] p1 = { new Point(10, 3), new Point(17, 7), new Point(10, 11), new Point(3, 7) };
                                    Point[] p2 = { new Point(10, 8), new Point(17, 12), new Point(10, 16), new Point(3, 12) };
                                    g.DrawPolygon(p, p1);
                                    g.DrawPolygon(p, p2);
                                }
                                break;
                            case "Sohbet":
                            case "Chat": // balon
                                g.DrawRectangle(p, 2, 4, 13, 9);
                                {
                                    Point[] pts = { new Point(6, 13), new Point(6, 17), new Point(10, 13) };
                                    g.DrawPolygon(p, pts);
                                }
                                g.DrawLine(p, 5, 7, 12, 7);
                                g.DrawLine(p, 5, 10, 10, 10);
                                break;
                            case "Harita":
                            case "Map": // iğne
                                g.DrawEllipse(p, 5, 2, 10, 10);
                                {
                                    Point[] pts = { new Point(6, 9), new Point(14, 9), new Point(10, 18) };
                                    g.DrawPolygon(p, pts);
                                }
                                g.FillEllipse(b, 8.5f, 5.5f, 3, 3);
                                break;
                            case "Ses":
                            case "Sound": // hoparlör
                                {
                                    Point[] pts = { new Point(3, 8), new Point(7, 8), new Point(11, 4), new Point(11, 16), new Point(7, 12), new Point(3, 12) };
                                    g.DrawPolygon(p, pts);
                                }
                                g.DrawArc(p, 12, 7, 4, 6, 270, 180);
                                g.DrawArc(p, 12, 4, 9, 12, 270, 180);
                                break;
                            case "Kısayollar":
                            case "Key Bindings": // klavye
                                g.DrawRectangle(p, 2, 6, 16, 9);
                                g.FillRectangle(b, 4, 8, 2, 2);
                                g.FillRectangle(b, 7.5f, 8, 2, 2);
                                g.FillRectangle(b, 11, 8, 2, 2);
                                g.FillRectangle(b, 14.5f, 8, 2, 2);
                                g.FillRectangle(b, 5, 11.5f, 10, 1.8f);
                                break;
                            case "Koşullar":
                            case "Conditions": // </> 
                                g.DrawLine(p, 8, 5, 4, 10);
                                g.DrawLine(p, 4, 10, 8, 15);
                                g.DrawLine(p, 12, 5, 16, 10);
                                g.DrawLine(p, 16, 10, 12, 15);
                                break;
                            case "Eklentiler":
                            case "Plugins": // dişli
                                g.DrawEllipse(p, 6, 6, 8, 8);
                                g.FillEllipse(b, 8.7f, 8.7f, 2.6f, 2.6f);
                                for (int i = 0; i < 8; i++)
                                {
                                    double a = i * Math.PI / 4;
                                    float x1 = 10 + (float)(Math.Cos(a) * 4.4);
                                    float y1 = 10 + (float)(Math.Sin(a) * 4.4);
                                    float x2 = 10 + (float)(Math.Cos(a) * 7);
                                    float y2 = 10 + (float)(Math.Sin(a) * 7);
                                    g.DrawLine(p, x1, y1, x2, y2);
                                }
                                break;
                            default: // içi boş daire
                                g.DrawEllipse(p, 5, 5, 10, 10);
                                break;
                        }
                    }
                }
            }

            private static void DrawStar(Graphics g, SolidBrush b, float cx, float cy, float r)
            {
                PointF[] pts = new PointF[8];
                for (int i = 0; i < 8; i++)
                {
                    double a = i * Math.PI / 4 - Math.PI / 2;
                    float rr = (i % 2 == 0) ? r : r * 0.42f;
                    pts[i] = new PointF(cx + (float)(Math.Cos(a) * rr), cy + (float)(Math.Sin(a) * rr));
                }
                g.FillPolygon(b, pts);
            }
        }

        // ---------------------------------------------------------------
        // H-şeridi gerçek TabControl ile sar. Buton/paneller yerinde kalır
        // (tüm mevcut kod referansları çalışır); şerit gizlenir, sekmeler
        // TabControl üzerinden butonlara PerformClick gönderir.
        // TabPageH_Option_Click sonundaki senkron bloğu ters yönü kapatır.
        // ---------------------------------------------------------------
        private void WrapStripWithTabs(Panel hostV, Control strip)
        {
            WrapXBotReferenceTabs(hostV, strip);
        }

        private void WrapAllPhBotStrips()
        {
            try
            {
                WrapStripWithTabs(TabPageV_Control01_Character_Panel,
                    TabPageH_Character_Option01 != null ? TabPageH_Character_Option01.Parent : null);
                WrapStripWithTabs(TabPageV_Control01_Skills_Panel,
                    TabPageH_Skills_Option01 != null ? TabPageH_Skills_Option01.Parent : null);
                WrapStripWithTabs(TabPageV_Control01_Town_Panel,
                    TabPageH_Town_Option01 != null ? TabPageH_Town_Option01.Parent : null);
                WrapStripWithTabs(TabPageV_Control01_Training_Panel,
                    TabPageH_Training_Option01 != null ? TabPageH_Training_Option01.Parent : null);
                WrapStripWithTabs(TabPageV_Control01_Party_Panel,
                    TabPageH_Party_Option01 != null ? TabPageH_Party_Option01.Parent : null);
                WrapStripWithTabs(TabPageV_Control01_Players_Panel,
                    TabPageH_Players_Option01 != null ? TabPageH_Players_Option01.Parent : null);
                WrapStripWithTabs(TabPageV_Control01_Guild_Panel,
                    TabPageH_Guild_Option01 != null ? TabPageH_Guild_Option01.Parent : null);
                WrapStripWithTabs(TabPageV_Control01_Inventory_Panel,
                    TabPageH_Inventory_Option01 != null ? TabPageH_Inventory_Option01.Parent : null);
                WrapStripWithTabs(TabPageV_Control01_Stall_Panel,
                    TabPageH_Stall_Option01 != null ? TabPageH_Stall_Option01.Parent : null);
                try
                {
                    LocalizationManager.OnLanguageChanged -= RefreshWrappedTabTitles;
                    LocalizationManager.OnLanguageChanged += RefreshWrappedTabTitles;
                }
                catch { }
            }
            catch (Exception ex) { PhBotDebug("wrap all: " + ex.Message); }
        }

        private void RefreshWrappedTabTitles()
        {
            if (_tabSync) return;
            _tabSync = true;
            try
            {
                ForEachControl(pnlWindow, delegate(Control c)
                {
                    TabControl tc = c as TabControl;
                    if (tc == null || !tc.Name.EndsWith("_Tabs")) return;
                    foreach (TabPage tp in tc.TabPages)
                    {
                        Button b = tp.Tag as Button;
                        if (b == null) continue;
                        try { if (tp.Text != b.Text) tp.Text = b.Text; } catch { }
                    }
                });
            }
            finally { _tabSync = false; }
        }
    }
}
