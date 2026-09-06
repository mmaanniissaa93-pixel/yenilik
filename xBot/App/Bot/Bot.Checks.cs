using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using Newtonsoft.Json.Linq;
using xBot.Game;
using xBot.Game.Objects;
using xBot.Game.Objects.Entity;
using xBot.Game.Objects.Common;
using System.Globalization;
using System.Linq;
using xBot.Game.Objects.Item;

namespace xBot.App
{
    public partial class Bot
    {
        #region (Timers, checks & cooldown controls)
        /// <summary>
        /// Cooldown timer.
        /// </summary>
        Timer tUsingHP, tUsingMP, tUsingVigor,
            tUsingUniversal, tUsingPurification,
            tUsingRecoveryKit, tUsingAbnormalPill,
            tUsingHGP,
            tCycleAutoParty;

        
        public bool IsSwitchingWeapon { get; set; }
        public Timer TimerSwitchingWeapon { get; set; } = null;
        public SRTypes.Weapon MainWeaponType { get; set; } = SRTypes.Weapon.None;

        private static byte ParsePercentSafe(string text, byte fallback = 50)
        {
            byte v;
            if (byte.TryParse((text ?? "").Trim(), out v)) return v;
            return fallback;
        }
        // Pot teşhis logları 1sn poll'de spam yapmasın: aynı mesaj en fazla 15sn'de bir.
        private static DateTime s_lastHpNoPotionLog = DateTime.MinValue;
        private static DateTime s_lastMpNoPotionLog = DateTime.MinValue;
        private static void LogPotionThrottled(ref DateTime last, string msg)
        {
            DateTime now = DateTime.UtcNow;
            if ((now - last).TotalSeconds < 15)
                return;
            last = now;
            Window.Get?.Log(msg);
        }

        #region (0x704C item-use guard: desync + throttle + reject-block)
        // Server reddedilen kullanimdan hemen sonra baglantiyi kesebiliyor (Sevar 0x1889).
        // Ayni slotun ust uste gonderilmesini engelle + slot/envarter senkronunu gondermeden dogrula.
        private static readonly Dictionary<byte, DateTime> m_rejectedUseSlots = new Dictionary<byte, DateTime>();
        private static DateTime m_lastUseItemUtc = DateTime.MinValue;
        private static byte m_lastUseItemSlot;
        private static uint m_lastUseItemId;
        private static DateTime m_lastItemSuccessUtc = DateTime.MinValue;
        private static readonly object m_useItemLock = new object();
        private const int UseItemMinIntervalMs = 1000;
        private const int RejectedSlotBlockSeconds = 5;
        private const int RejectLinkWindowSeconds = 5;
        private const int RejectGlobalBlackoutSeconds = 2;
        private static DateTime m_rejectBlackoutUntil = DateTime.MinValue;
        private static DateTime m_externalUseBlackoutUntil = DateTime.MinValue;
        private const int ExternalUseBlackoutMs = 2500;
        private const int SuccessLinkWindowSeconds = 10;

        // itemID -> usable. 0xB04C success gorulen calisir, son 10sn'de success yokken
        // gelen reject ise icerigi kara listeye alir (oturumlar arasi Config'de saklanir).
        private static readonly Dictionary<uint, bool> s_learnedUsable = new Dictionary<uint, bool>();
        private static readonly object s_learnedLock = new object();
        private static bool s_learnedLoaded = false;
        private const string LearnedItemsFile = "Config\\LearnedItems.json";

        public static bool IsItemBlockedFromUse(xBot.Game.Objects.Item.SRItem item)
        {
            if (item == null)
                return true;
            // Tüketilebilirler (3,1,*) pot, (3,2,*) hap, (3,3,*) scroll asla kalıcı
            // kara listeye girmez: reject'leri geçicidir (sunucu cooldown'u, hareket,
            // senkron, usage). Slot blackout + global blackout spam'i zaten engeller.
            // Bu aynı zamanda eskiden yanlışlıkla kara listeye giren potları da kurtarır.
            if (item.ID2 == 3 && item.ID3 >= 1 && item.ID3 <= 3)
                return false;
            EnsureLearnedItemsLoaded();
            lock (s_learnedLock)
            {
                bool ok;
                return s_learnedUsable.TryGetValue(item.ID, out ok) && !ok;
            }
        }
        public void LearnItemUsable(uint itemId, bool usable, string why)
        {
            if (itemId == 0)
                return;
            bool changed = false;
            lock (s_learnedLock)
            {
                EnsureLearnedItemsLoaded();
                bool cur;
                if (!s_learnedUsable.TryGetValue(itemId, out cur) || cur != usable)
                {
                    s_learnedUsable[itemId] = usable;
                    changed = true;
                }
            }
            lock (m_useItemLock)
            {
                if (usable)
                    m_lastItemSuccessUtc = DateTime.UtcNow;
            }
            if (changed)
            {
                Window.Get?.Log($"[Item] Ogrenildi: item {itemId} {(usable ? "calisiyor" : "CALISMIYOR (kara liste)")} [{why}]");
                SaveLearnedItems();
            }
        }
        private static void EnsureLearnedItemsLoaded()
        {
            lock (s_learnedLock)
            {
                if (s_learnedLoaded)
                    return;
                s_learnedLoaded = true;
                try
                {
                    if (!File.Exists(LearnedItemsFile))
                        return;
                    JObject root = JObject.Parse(File.ReadAllText(LearnedItemsFile));
                    JObject items = root["Items"] as JObject;
                    if (items == null)
                        return;
                    foreach (var kv in items)
                    {
                        uint id;
                        if (uint.TryParse(kv.Key, out id))
                            s_learnedUsable[id] = (bool)kv.Value;
                    }
                    if (s_learnedUsable.Count > 0)
                        Window.Get?.Log($"[Item] Ogrenilmis kullanim bilgisi yuklendi ({s_learnedUsable.Count} kayit).");
                }
                catch { }
            }
        }
        private static void SaveLearnedItems()
        {
            try
            {
                Dictionary<uint, bool> snap;
                lock (s_learnedLock)
                {
                    snap = new Dictionary<uint, bool>(s_learnedUsable);
                }
                string dir = Path.GetDirectoryName(LearnedItemsFile);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                JObject items = new JObject();
                foreach (var kv in snap)
                    items[kv.Key.ToString()] = kv.Value;
                File.WriteAllText(LearnedItemsFile, new JObject { ["Items"] = items }.ToString());
            }
            catch { }
        }

        public bool CheckUseItemThrottle(byte slot)
        {
            lock (m_useItemLock)
            {
                DateTime now = DateTime.UtcNow;
                DateTime blockedUntil;
                if (m_rejectedUseSlots.TryGetValue(slot, out blockedUntil))
                {
                    if (now < blockedUntil)
                        return false;
                    m_rejectedUseSlots.Remove(slot);
                }
                if ((now - m_lastUseItemUtc).TotalMilliseconds < UseItemMinIntervalMs)
                    return false;
                if (now < m_rejectBlackoutUntil)
                    return false;
                if (now < m_externalUseBlackoutUntil)
                    return false;
                return true;
            }
        }
        public void MarkUseItemSent(byte slot, uint itemId)
        {
            lock (m_useItemLock)
            {
                m_lastUseItemUtc = DateTime.UtcNow;
                m_lastUseItemSlot = slot;
                m_lastUseItemId = itemId;
            }
        }
        public void MarkLastUseRejected(ushort errorCode = 0)
        {
            if (errorCode == 0x185B)
            {
                // 0x185B = Silkroad sunucu hatasi: Pot veya esya bekleme suresinde (cooldown active).
                // Bu durum normal cooldown senkronizasyonudur, esya veya slot arizasi degildir.
                // Kesinlikle slot bloklanmaz, esya kara listeye alinmaz.
                bool isEu = InfoManager.Character != null && InfoManager.Character.IsEuropean();
                int cooldownMs = isEu ? 15000 : 1000;

                lock (m_useItemLock)
                {
                    m_rejectBlackoutUntil = DateTime.UtcNow.AddMilliseconds(500);
                }

                // Ilgili pot timer'ini sunucu bekleme suresine gore senkronize et.
                // Tam pencereyi (EU 15sn) baştan kurmak "lazımken basmıyor" hissi
                // verir; kısa tutup bir sonraki HP/MP güncellemesinde tekrar dene.
                int retryMs = Math.Min(cooldownMs, 3000);
                try
                {
                    var item = xBot.Game.DataManager.GetItemData(m_lastUseItemId);
                    if (item != null)
                    {
                        byte id4 = byte.Parse(item["tid4"]);
                        if (id4 == 1 && tUsingHP != null)
                        {
                            tUsingHP.Interval = retryMs;
                            tUsingHP.Stop();
                            tUsingHP.Start();
                        }
                        else if (id4 == 2 && tUsingMP != null)
                        {
                            tUsingMP.Interval = retryMs;
                            tUsingMP.Stop();
                            tUsingMP.Start();
                        }
                        else if (id4 == 3 && tUsingVigor != null)
                        {
                            tUsingVigor.Interval = Math.Min(15000, 3000);
                            tUsingVigor.Stop();
                            tUsingVigor.Start();
                        }
                    }
                }
                catch { }
                return;
            }

            uint suspectId = 0;
            byte lastSlot;
            uint lastId;
            lock (m_useItemLock)
            {
                lastSlot = m_lastUseItemSlot;
                lastId = m_lastUseItemId;
            }
            bool lastWasPotion = false;
            try
            {
                var inv = InfoManager.Character != null ? InfoManager.Character.Inventory : null;
                xBot.Game.Objects.Item.SRItem last = null;
                if (inv != null && lastSlot < inv.Capacity)
                    last = inv[lastSlot];
                if (last != null && last.ID == lastId && last.ID2 == 3 && last.ID3 >= 1 && last.ID3 <= 3)
                    lastWasPotion = true;
            }
            catch { }
            bool linked;
            lock (m_useItemLock)
            {
                linked = (DateTime.UtcNow - m_lastUseItemUtc).TotalSeconds <= RejectLinkWindowSeconds;
                if (linked)
                    m_rejectedUseSlots[m_lastUseItemSlot] = DateTime.UtcNow.AddSeconds(RejectedSlotBlockSeconds);
                m_rejectBlackoutUntil = DateTime.UtcNow.AddSeconds(RejectGlobalBlackoutSeconds);
                // Son 10sn'de hic basarili kullanim yoksa reject gecici degil, icerik sorunudur:
                // ayni item bir daha denenmesin (oturumlar arasi kalici).
                // ANCAK pot/ilaç grubunda reject geçicidir (cooldown/hareket/usage); asla
                // kalıcı kara listeye alınmaz, yoksa "MP hiç basılmıyor" olur.
                if (!lastWasPotion && (DateTime.UtcNow - m_lastItemSuccessUtc).TotalSeconds > SuccessLinkWindowSeconds)
                    suspectId = m_lastUseItemId;
            }
            // Hesaplanan usage tutmuyorsa (Sevar 0x..ED vs 0x..EC) parity ile bit0
            // alternatifi denensin; tutan varyant kalici ogrenilir.
            // Önce kanıtlanmış-yanlış öğrenme silinir, yoksa aynı baytta takılı kalınır.
            if (linked)
            {
                xBot.Game.PacketBuilder.ForgetLearnedUsage(lastId);
                xBot.Game.PacketBuilder.NoteUsageReject(lastId);
            }
            if (suspectId != 0)
                LearnItemUsable(suspectId, false, $"0xB04C reject 0x{errorCode:X4}, yakin zamanda success yok");
        }

        public void UpdatePotionCooldownIntervals()
        {
            bool isEu = InfoManager.Character != null && InfoManager.Character.IsEuropean();
            int potionInterval = isEu ? 15000 : 1000;

            if (tUsingHP != null) tUsingHP.Interval = potionInterval;
            if (tUsingMP != null) tUsingMP.Interval = potionInterval;
            if (tUsingVigor != null) tUsingVigor.Interval = 15000;
        }

        /// <summary>
        /// Gercek client potion bastiginda bot timer'larini oteleyip global blackout baslatir.
        /// Boylece bot, client'in actigi server cooldown penceresine ates etmez (0x1889+DC).
        /// </summary>
        public void NotifyExternalItemUse(byte slot, ushort usage)
        {
            var chr = InfoManager.Character;
            if (chr != null && chr.Inventory != null && slot < chr.Inventory.Capacity)
            {
                var cur = chr.Inventory[slot];
                if (cur != null)
                    xBot.Game.PacketBuilder.LearnItemUsage(cur.ID, usage);
            }
            lock (m_useItemLock)
            {
                m_lastUseItemUtc = DateTime.UtcNow;
                m_externalUseBlackoutUntil = DateTime.UtcNow.AddMilliseconds(ExternalUseBlackoutMs);
            }
            try
            {
                // usage duzeni: ID1<<2 | ID2<<5 | ID3<<7 | ID4<<11 ; ID4: 1=HP 2=MP 3=vigor
                int id4 = (usage >> 11) & 0x1F;
                bool isEu = InfoManager.Character != null && InfoManager.Character.IsEuropean();
                int potionInterval = isEu ? 15000 : 1000;

                if (id4 == 1 && tUsingHP != null)
                {
                    tUsingHP.Interval = potionInterval;
                    tUsingHP.Stop(); tUsingHP.Start();
                }
                else if (id4 == 2 && tUsingMP != null)
                {
                    tUsingMP.Interval = potionInterval;
                    tUsingMP.Stop(); tUsingMP.Start();
                }
                else if (id4 == 3 && tUsingVigor != null)
                {
                    tUsingVigor.Interval = 15000;
                    tUsingVigor.Stop(); tUsingVigor.Start();
                }
                else
                {
                    if (tUsingHP != null) { tUsingHP.Interval = potionInterval; tUsingHP.Stop(); tUsingHP.Start(); }
                    if (tUsingMP != null) { tUsingMP.Interval = potionInterval; tUsingMP.Stop(); tUsingMP.Start(); }
                    if (tUsingVigor != null) { tUsingVigor.Interval = 15000; tUsingVigor.Stop(); tUsingVigor.Start(); }
                }
            }
            catch { }
        }
        #endregion


        private void InitializeTimers()
        {
            // Preparing all neccesary timers
            tUsingHP = new Timer();
            tUsingMP = new Timer();
            tUsingVigor = new Timer();
            tUsingUniversal = new Timer();
            tUsingPurification = new Timer();
            tUsingRecoveryKit = new Timer();
            tUsingAbnormalPill = new Timer();
            tUsingHGP = new Timer();
            tCycleAutoParty = new Timer();

            // Manual reset
            tUsingHP.AutoReset = tUsingMP.AutoReset = tUsingVigor.AutoReset =
            tUsingUniversal.AutoReset = tUsingPurification.AutoReset =
            tUsingRecoveryKit.AutoReset = tUsingAbnormalPill.AutoReset =
            tCycleAutoParty.AutoReset = false;

            // Potion & pill cooldowns: EU 15s, CH 1s, Vigor 15s, pills 12s.
            bool isEu = InfoManager.Character != null && InfoManager.Character.IsEuropean();
            int potInterval = isEu ? 15000 : 1000;
            tUsingHP.Interval = tUsingMP.Interval = potInterval;
            tUsingVigor.Interval = 15000;
            tUsingUniversal.Interval = tUsingPurification.Interval = 12000;
            tUsingRecoveryKit.Interval = 1000;
            tUsingAbnormalPill.Interval = 12000;

            tCycleAutoParty.Interval = 5000;

            // Callbacks
            tUsingHP.Elapsed += CheckUsingHP;
            tUsingMP.Elapsed += CheckUsingMP;
            tUsingVigor.Elapsed += CheckUsingVigor;
            tUsingUniversal.Elapsed += CheckUsingUniversal;
            tUsingPurification.Elapsed += CheckUsingPurification;
            tCycleAutoParty.Elapsed += CheckAutoParty;
            tUsingRecoveryKit.Elapsed += CheckUsingRecoveryKit;
            tUsingAbnormalPill.Elapsed += CheckUsingAbnormalPill;
            tUsingHGP.Elapsed += CheckUsingHGP;
        }
        public void CheckUsingHP()
        {
            if (!tUsingHP.Enabled)
                CheckUsingHP(tUsingHP, null);
        }
        private void CheckUsingHP(object sender, ElapsedEventArgs e)
        {
            if (InfoManager.Character == null)
                return;
            if (InfoManager.Character.LifeStateType == SRModel.LifeState.Alive)
            {
                Window w = Window.Get;
                if (w.Character_cbxUseHP.Checked || w.Character_cbxUseHPGrain.Checked)
                {
                    byte useHP = 0; // dummy
                    w.Character_tbxUseHP.InvokeIfRequired(() => {
                        useHP = ParsePercentSafe(w.Character_tbxUseHP.Text);
                    });
                    if (InfoManager.Character.GetHPPercent() <= useHP)
                    {
                        byte slot = 0;
                        // Referans mekanik (WinForms1): Grain tikliyse _SPOTION_ içeren,
                        // normal tikliyse filtresiz ilk eşleşen. Exclude yok.
                        if (w.Character_cbxUseHPGrain.Checked && FindItem(3, 1, 1, ref slot, "_SPOTION_")
                            || w.Character_cbxUseHP.Checked && FindItem(3, 1, 1, ref slot))
                        {
                            int requiredInterval = (InfoManager.Character != null && InfoManager.Character.IsEuropean()) ? 15000 : 1000;
                            if (tUsingHP.Interval != requiredInterval)
                                tUsingHP.Interval = requiredInterval;
                            PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot);
                            tUsingHP.Start();
                        }
                        else
                        {
                            LogPotionThrottled(ref s_lastHpNoPotionLog, "[HP] Eşik tuttu (%"
                                + InfoManager.Character.GetHPPercent().ToString("0") + " <= %" + useHP
                                + ") ama envanterde uygun HP potu bulunamadı! (Grain kutusu ve pot tipini kontrol et)");
                        }
                    }
                }
                ProtectionManager.CheckSkillHealing();
            }
        }
        public void CheckUsingMP()
        {
            if (!tUsingMP.Enabled)
                CheckUsingMP(tUsingMP, null);
        }
        private void CheckUsingMP(object sender, ElapsedEventArgs e)
        {
            if (InfoManager.Character == null)
                return;
            if (InfoManager.Character.LifeStateType == SRModel.LifeState.Alive)
            {
                Window w = Window.Get;
                if (w.Character_cbxUseMP.Checked || w.Character_cbxUseMPGrain.Checked)
                {
                    byte useMP = 0; // dummy
                    WinAPI.InvokeIfRequired(w.Character_tbxUseMP, () => {
                        useMP = ParsePercentSafe(w.Character_tbxUseMP.Text);
                    });
                    if (InfoManager.Character.GetMPPercent() <= useMP)
                    {
                        byte slot = 0;
                        // Referans mekanik (WinForms1): filtresiz ilk eşleşme, exclude yok.
                        if (w.Character_cbxUseMPGrain.Checked && FindItem(3, 1, 2, ref slot, "_SPOTION_")
                            || w.Character_cbxUseMP.Checked && FindItem(3, 1, 2, ref slot))
                        {
                            int requiredInterval = (InfoManager.Character != null && InfoManager.Character.IsEuropean()) ? 15000 : 1000;
                            if (tUsingMP.Interval != requiredInterval)
                                tUsingMP.Interval = requiredInterval;
                            PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot);
                            tUsingMP.Start();
                        }
                        else
                        {
                            LogPotionThrottled(ref s_lastMpNoPotionLog, "[MP] Eşik tuttu (%"
                                + InfoManager.Character.GetMPPercent().ToString("0") + " <= %" + useMP
                                + ") ama envanterde uygun MP potu bulunamadı! (Grain kutusu ve pot tipini kontrol et)");
                        }
                    }
                }
                ProtectionManager.CheckSkillMana();
            }
        }
        public void CheckUsingVigor()
        {
            if (!tUsingVigor.Enabled)
                CheckUsingVigor(tUsingVigor, null);
        }
        private void CheckUsingVigor(object sender, ElapsedEventArgs e)
        {
            if (InfoManager.Character == null)
                return;
            if (InfoManager.Character.LifeStateType == SRModel.LifeState.Alive)
            {
                Window w = Window.Get;
                if (w.Character_cbxUseHPVigor.Checked || w.Character_cbxUseMPVigor.Checked)
                {
                    if (tUsingVigor.Interval != 15000)
                        tUsingVigor.Interval = 15000;
                    byte usePercent = 0;
                    WinAPI.InvokeIfRequired(w.Character_tbxUseHPVigor, () => {
                        usePercent = ParsePercentSafe(w.Character_tbxUseHPVigor.Text);
                    });
                    // Check hp %
                    if (InfoManager.Character.GetHPPercent() <= usePercent)
                    {
                        byte slot = 0;
                        // Referans mekanik (WinForms1): filtresiz ilk eşleşme.
                        if (FindItem(3, 1, 3, ref slot))
                        {
                            PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot);
                            tUsingVigor.Start();
                        }
                    }
                    else
                    {
                        // Check mp %
                        WinAPI.InvokeIfRequired(w.Character_tbxUseMPVigor, () => {
                            usePercent = ParsePercentSafe(w.Character_tbxUseMPVigor.Text);
                        });
                        if (InfoManager.Character.GetMPPercent() <= usePercent)
                        {
                            byte slot = 0;
                            if (FindItem(3, 1, 3, ref slot))
                            {
                                PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot);
                                tUsingVigor.Start();
                            }
                        }
                    }
                }
            }
        }
        public void CheckUsingUniversal()
        {
            if (!tUsingUniversal.Enabled)
                CheckUsingUniversal(tUsingUniversal, null);
        }
        public void CheckUsingUniversal(object sender, ElapsedEventArgs e)
        {
            if (InfoManager.Character.LifeStateType == SRModel.LifeState.Alive)
            {
                Window w = Window.Get;
                if (w.Character_cbxUsePillUniversal.Checked)
                {
                    if (((uint)InfoManager.Character.BadStatusFlags).HasFlags
                        ((uint)(SRModel.BadStatus.Freezing
                        | SRModel.BadStatus.ElectricShock
                        | SRModel.BadStatus.Burn
                        | SRModel.BadStatus.Poisoning
                        | SRModel.BadStatus.Zombie)))
                    {
                        byte slot = 0;
                        if (FindItem(3, 2, 6, ref slot))
                        {
                            if (PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot))
                                tUsingUniversal.Start();
                        }
                    }
                }
            }
        }
        public void CheckUsingPurification()
        {
            if (!tUsingPurification.Enabled)
                CheckUsingPurification(tUsingPurification, null);
        }
        private void CheckUsingPurification(object sender, ElapsedEventArgs e)
        {
            if (InfoManager.Character.LifeStateType == SRModel.LifeState.Alive)
            {
                Window w = Window.Get;
                if (w.Character_cbxUsePillPurification.Checked)
                {
                    if (((uint)InfoManager.Character.BadStatusFlags).HasFlags
                        ((uint)(SRModel.BadStatus.Dull
                        | SRModel.BadStatus.Fear
                        | SRModel.BadStatus.ShortSight
                        | SRModel.BadStatus.Bleed
                        | SRModel.BadStatus.Darkness
                        | SRModel.BadStatus.Disease
                        | SRModel.BadStatus.Confusion
                        | SRModel.BadStatus.Decay
                        | SRModel.BadStatus.Weaken
                        | SRModel.BadStatus.Impotent
                        | SRModel.BadStatus.Division
                        | SRModel.BadStatus.Panic
                        | SRModel.BadStatus.Combustion
                        | SRModel.BadStatus.Hidden)))
                    {
                        byte slot = 0;
                        if (FindItem(3, 2, 1, ref slot))
                        {
                            if (PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot))
                                tUsingPurification.Start();
                        }
                    }
                }
            }
        }
        public void CheckAutoParty()
        {
            if (!tCycleAutoParty.Enabled)
                CheckAutoParty(tCycleAutoParty, null);
        }
        private void CheckAutoParty(object sender, ElapsedEventArgs e)
        {
            Window w = Window.Get;
            if (w.Party_cbxInviteAll.Checked)
            {
                // Check players around
                if (InfoManager.Players.Count > 0)
                {
                    if (InfoManager.inParty)
                    {
                        xDictionary<string, SRPlayer> PlayersNearWithNoParty = new xDictionary<string, SRPlayer>(InfoManager.Players);
                        // Remove players nears with party
                        for (byte j = 0; j < InfoManager.Party.Members.Count; j++)
                        {
                            string PlayerName = InfoManager.Party.Members.GetAt(j).Name.ToUpper();
                            if (PlayersNearWithNoParty.ContainsKey(PlayerName))
                            {
                                PlayersNearWithNoParty.RemoveKey(PlayerName);
                            }
                        }
                        if (PlayersNearWithNoParty.Count == 0)
                            return;

                        // Check invitations setup
                        if (!w.Party_cbxInviteOnlyPartySetup.Checked
                            || InfoManager.Party.SetupFlags == w.GetPartySetup())
                        {
                            if (!InfoManager.Party.isFull)
                            {
                                PacketBuilder.InviteToParty(PlayersNearWithNoParty.GetAt(rand.Next(PlayersNearWithNoParty.Count)).UniqueID);
                                tCycleAutoParty.Start();
                            }
                        }
                    }
                    else
                    {
                        PacketBuilder.CreateParty(InfoManager.Players.GetAt(rand.Next(InfoManager.Players.Count)).UniqueID, InfoManager.Party.SetupFlags);
                        tCycleAutoParty.Start();
                    }
                }
                else
                {
                    tCycleAutoParty.Start();
                }
            }
            else if (w.Party_cbxInvitePartyList.Checked)
            {
                if (InfoManager.Players.Count > 0)
                {
                    List<string> PlayersToInvite = new List<string>();
                    w.Party_lstvPartyList.InvokeIfRequired(() => {
                        for (int j = 0; j < w.Party_lstvPartyList.Items.Count; j++)
                            PlayersToInvite.Add(w.Party_lstvPartyList.Items[j].Name);
                    });
                    // Remove if are in party already
                    for (int j = 0; j < PlayersToInvite.Count; j++)
                    {
                        for (byte k = 0; k < InfoManager.Party.Members.Count; k++)
                        {
                            if (PlayersToInvite[j].Equals(InfoManager.Party.Members.GetAt(k).Name, StringComparison.OrdinalIgnoreCase))
                            {
                                PlayersToInvite.RemoveAt(j--);
                                break;
                            }
                        }
                    }
                    if (PlayersToInvite.Count > 0)
                    {
                        // Shuffle and check the party list with near players
                        PlayersToInvite.Shuffle();
                        SRPlayer PlayerToInvite = null;
                        for (int j = 0; j < PlayersToInvite.Count; j++)
                        {
                            if ((PlayerToInvite = InfoManager.Players[PlayersToInvite[j]]) != null)
                                break;
                        }
                        if (PlayerToInvite != null)
                        {
                            if (InfoManager.inParty)
                            {
                                if (!w.Party_cbxInviteOnlyPartySetup.Checked
                                    || InfoManager.Party.SetupFlags == w.GetPartySetup())
                                {
                                    if (!InfoManager.Party.isFull)
                                    {
                                        PacketBuilder.InviteToParty(PlayerToInvite.UniqueID);
                                        tCycleAutoParty.Start();
                                    }
                                }
                            }
                            else
                            {
                                PacketBuilder.CreateParty(PlayerToInvite.UniqueID, w.GetPartySetup());
                                tCycleAutoParty.Start();
                            }
                        }
                        else
                        {
                            // No players to invite, try later
                            tCycleAutoParty.Start();
                        }
                    }
                }
                else
                {
                    // No players near to invite, try later
                    tCycleAutoParty.Start();
                }
            }
        }
        public bool CheckPartyLeaving()
        {
            Window w = Window.Get;
            if (w.Party_cbxLeavePartyNoneLeader.Checked)
            {
                bool found = false;
                w.Party_lstvLeaderList.InvokeIfRequired(() => {
                    for (byte j = 0; j < InfoManager.Party.Members.Count; j++)
                    {
                        if (w.Party_lstvLeaderList.Items.ContainsKey(InfoManager.Party.Members.GetAt(j).Name.ToUpper()))
                        {
                            found = true;
                            break;
                        }
                    }
                });

                if (!found)
                {
                    PacketBuilder.LeaveParty();
                    return true;
                }
            }
            return false;
        }
        public void CheckPartyMatchAutoReform()
        {
            PacketBuilder.RequestPartyMatch();
        }
        public void CheckUsingRecoveryKit()
        {
            if (!tUsingRecoveryKit.Enabled)
            {
                CheckUsingRecoveryKit(tUsingRecoveryKit, null);
            }
        }
        private void CheckUsingRecoveryKit(object sender, ElapsedEventArgs e)
        {
            // Checking pet using recovery kit by priority
            Window w = Window.Get;
            // Vehicle or Transport
            if (w.Character_cbxUseTransportHP.Checked)
            {
                SRCoService pet = InfoManager.MyPets.Find(p => p.isHorse() || p.isTransport());
                // Check % if there is at least one pet
                if (pet != null)
                {
                    byte useHP = 0; // dummy
                    w.Character_tbxUseTransportHP.InvokeIfRequired(() => {
                        useHP = ParsePercentSafe(w.Character_tbxUseTransportHP.Text);
                    });
                    if (pet.GetHPPercent() <= useHP)
                    {
                        byte slot = 0;
                        if (FindItem(3, 1, 4, ref slot))
                        {
                            if (PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot, pet.UniqueID))
                                tUsingRecoveryKit.Start();
                        }
                        return; // Avoid checking other pets
                    }
                }
            }

            // Attacking pet
            if (w.Character_cbxUsePetHP.Checked)
            {
                SRCoService pet = InfoManager.MyPets.Find(p => p.isAttackPet());
                // Check % if there is at least one pet
                if (pet != null)
                {
                    byte useHP = 0; // dummy
                    w.Character_tbxUsePetHP.InvokeIfRequired(() => {
                        useHP = ParsePercentSafe(w.Character_tbxUsePetHP.Text);
                    });
                    if (pet.GetHPPercent() <= useHP)
                    {
                        byte slot = 0;
                        if (FindItem(3, 1, 4, ref slot))
                        {
                            if (PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot, pet.UniqueID))
                                tUsingRecoveryKit.Start();
                        }
                        return; // Avoid checking other pets
                    }
                }
            }
        }
        public void CheckUsingAbnormalPill()
        {
            if (!tUsingAbnormalPill.Enabled)
                CheckUsingAbnormalPill(tUsingAbnormalPill, null);
        }
        private void CheckUsingAbnormalPill(object sender, ElapsedEventArgs e)
        {
            Window w = Window.Get;
            if (w.Character_cbxUsePetsPill.Checked)
            {
                // Checking pet bad status for using abnormal
                SRCoService pet = null;
                // As priority pet transport
                pet = InfoManager.MyPets.Find(p => (p.isHorse() || p.isTransport()) && p.BadStatusFlags != SRModel.BadStatus.None);
                if (pet == null)
                    pet = InfoManager.MyPets.Find(p => p.isAttackPet() && p.BadStatusFlags != SRModel.BadStatus.None);
                // At least one pet has bad status
                if (pet != null)
                {
                    byte slot = 0; // dummy
                    if (FindItem(3, 2, 7, ref slot))
                    {
                        if (PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot, pet.UniqueID))
                            tUsingAbnormalPill.Start();
                    }
                }
            }
        }
        public void CheckUsingHGP()
        {
            if (!tUsingHGP.Enabled || tUsingHGP.Interval != 1000)
            {
                CheckUsingHGP(tUsingHGP, null);
            }
        }
        private void CheckUsingHGP(object sender, ElapsedEventArgs e)
        {
            Window w = Window.Get;
            if (w.Character_cbxUsePetHGP.Checked)
            {
                SRCoService pet = InfoManager.MyPets.Find(p => p.isAttackPet());
                if (pet != null)
                {
                    SRAttackPet atkPet = (SRAttackPet)pet;

                    byte usePercent = 0;
                    w.Character_tbxUsePetHGP.InvokeIfRequired(() => {
                        usePercent = ParsePercentSafe(w.Character_tbxUsePetHGP.Text);
                    });
                    // Check hgp %
                    int HGPPercent = (int)(atkPet.HGP * 0.01); // 10000 = 100%
                    if (HGPPercent <= usePercent)
                    {
                        byte slot = 0;
                        if (FindItem(3, 1, 9, ref slot))
                        {
                            if (PacketBuilder.UseItem(InfoManager.Character.Inventory[slot], slot, pet.UniqueID))
                            {
                                tUsingHGP.ResetTimer(1000);
                                return;
                            }
                        }
                    }
                    tUsingHGP.ResetTimer(300000); // 1% decrease : -100 HGP every 5min
                }
            }
        }
        private void CheckLoginOptions(object sender, ElapsedEventArgs e)
        {
            Window w = Window.Get;

            if (w.Login_cbxGoClientless.Checked)
                GoClientless();

            if (w.Login_cbxUseReturnScroll.Checked)
                UseReturnScroll();

            if (w.Party_cbxMatchAutoReform.Checked)
                CheckPartyMatchAutoReform();

            CheckAutoParty();
        }
        public void DetectAndSetMainWeapon()
        {
            if (MainWeaponType == SRTypes.Weapon.None)
            {
                var current = GetMyWeaponType();
                if (current != SRTypes.Weapon.None)
                {
                    MainWeaponType = current;
                }
            }
        }

        public void EnsureMainWeapon()
        {
            if (MainWeaponType == SRTypes.Weapon.None)
            {
                DetectAndSetMainWeapon();
                return;
            }

            var current = GetMyWeaponType();
            if (current == MainWeaponType || IsSwitchingWeapon)
                return;

            if (InfoManager.Character == null || InfoManager.Character.Inventory == null)
                return;

            int slot = InfoManager.Character.Inventory.FindIndex(i => i != null && i.isEquipable() && ((SREquipable)i).IsWeapon() && (SRTypes.Weapon)i.ID4 == MainWeaponType, 13);
            if (slot != -1)
            {
                Window.Get?.LogProcess($"Weapon Swap: Reverting back to main weapon ({MainWeaponType})...");
                IsSwitchingWeapon = true;
                PacketBuilder.MoveItem((byte)slot, 6, SRTypes.InventoryItemMovement.InventoryToInventory);
                System.Threading.Thread.Sleep(500);
            }
        }

        public void CheckWeaponSwitch(SRSkill skill)
        {
            if (IsSwitchingWeapon || skill == null)
                return;

            DetectAndSetMainWeapon();

            // Check if the skill requires a weapon or an item to be used
            if (skill.RequiredWeaponPrimary == SRTypes.Weapon.None && skill.RequiredWeaponSecondary == SRTypes.Weapon.None && skill.RequiredItems.Count == 0)
                return;

            // Check my weapon being used
            var myWeapon = GetMyWeaponType();
            if (myWeapon == SRTypes.Weapon.None || (myWeapon != skill.RequiredWeaponPrimary && myWeapon != skill.RequiredWeaponSecondary))
            {
                // Find the item used to switch weapon
                var slot = InfoManager.Character.Inventory.FindIndex(i => i != null && i.isEquipable() && ((skill.RequiredWeaponPrimary != SRTypes.Weapon.None && ((SREquipable)i).IsWeapon() && (SRTypes.Weapon)i.ID4 == skill.RequiredWeaponPrimary) || (skill.RequiredWeaponSecondary != SRTypes.Weapon.None && ((SREquipable)i).IsWeapon() && (SRTypes.Weapon)i.ID4 == skill.RequiredWeaponSecondary)), 13);
                if (slot != -1)
                {
                    IsSwitchingWeapon = true;
                    PacketBuilder.MoveItem((byte)slot, 6, SRTypes.InventoryItemMovement.InventoryToInventory);

                    // Check if the skill can equip a shield: sword (ch), blade, sword (eu), dark staff, cleric
                    var shieldWeapons = new SRTypes.Weapon[] { SRTypes.Weapon.Sword, SRTypes.Weapon.Blade, SRTypes.Weapon.OneHandSword, SRTypes.Weapon.Warlock, SRTypes.Weapon.Cleric };
                    var itemWeaponType = (SRTypes.Weapon)InfoManager.Character.Inventory[slot].ID4;
                    if (shieldWeapons.Any(any => any == itemWeaponType))
                    {
                        // Check if Auto-Shield feature is enabled
                        bool useShield = false;
                        Window.Get.Character_cbxPVPModeUseShield.InvokeIfRequired(() => { useShield = Window.Get.Character_cbxPVPModeUseShield.Checked; });
                        if (useShield)
                        {
                            var myRace = InfoManager.Character.GetRace();
                            slot = InfoManager.Character.Inventory.FindIndex(i => i != null && i.isEquipable() && ((SREquipable)i).IsShield() && ((SREquipable)i).GetRace() == myRace, 13);
                            if (slot != -1)
                                PacketBuilder.MoveItem((byte)slot, 7, SRTypes.InventoryItemMovement.InventoryToInventory);
                        }
                    }
                    return;
                }
            }

            // Check if the skill requires an item
            if (skill.RequiredItems.Count != 0)
            {
                // Warrior forced filter
                if (skill.ServerName.StartsWith("SKILL_EU_WARRIOR"))
                {
                    // Check if is a shield skill
                    if (skill.ServerName.Contains("_SHIELD"))
                    {
                        // Ignore if has shield equiped already
                        if(GetMySecondaryEquipableType() == SREquipable.Equipable.Shield)
                            return;

                        var myRace = InfoManager.Character.GetRace();
                        var slot = InfoManager.Character.Inventory.FindIndex(i => i != null && i.isEquipable() && ((SREquipable)i).IsShield() && ((SREquipable)i).GetRace() == myRace, 13);
                        if (slot != -1)
                        {
                            IsSwitchingWeapon = true;
                            PacketBuilder.MoveItem((byte)slot, 7, SRTypes.InventoryItemMovement.InventoryToInventory);
                        }
                    }
                    else
                    {
                        List<SRTypes.Weapon> weapons = new List<SRTypes.Weapon>();
                        if (skill.ServerName.StartsWith("SKILL_EU_WARRIOR_FRENZYA_DAMAGE"))
                            weapons.Add(SRTypes.Weapon.TwoHandSword);
                        else
                        {
                            weapons.Add(SRTypes.Weapon.OneHandSword);
                            weapons.Add(SRTypes.Weapon.TwoHandSword);
                            weapons.Add(SRTypes.Weapon.DualAxes);
                        }

                        // Ignore if weapon is equiped already
                        if (weapons.Any(any => any == myWeapon))
                            return;

                        // Find weapon
                        var slot = InfoManager.Character.Inventory.FindIndex(i => i != null && i.isEquipable() && ((SREquipable)i).IsWeapon() && weapons.Any(any => any == (SRTypes.Weapon)i.ID4), 13);
                        if (slot != -1)
                        {
                            IsSwitchingWeapon = true;
                            PacketBuilder.MoveItem((byte)slot, 6, SRTypes.InventoryItemMovement.InventoryToInventory);
                        }
                    }
                }
                // Bicheon forced filter
                else if (skill.ServerName.StartsWith("SKILL_CH_SWORD"))
                {
                    // Check if is a shield skill
                    if (skill.ServerName.Contains("_SHIELD"))
                    {
                        // Ignore if has shield equiped already
                        if (GetMySecondaryEquipableType() == SREquipable.Equipable.Shield)
                            return;
                        
                        var myRace = InfoManager.Character.GetRace();
                        var slot = InfoManager.Character.Inventory.FindIndex(i => i != null && i.isEquipable() && ((SREquipable)i).IsShield() && ((SREquipable)i).GetRace() == myRace, 13);
                        if (slot != -1)
                        {
                            IsSwitchingWeapon = true;
                            PacketBuilder.MoveItem((byte)slot, 7, SRTypes.InventoryItemMovement.InventoryToInventory);
                        }
                    }
                    else
                    {
                        List<SRTypes.Weapon> weapons = new List<SRTypes.Weapon>()
                        {
                            SRTypes.Weapon.Sword,
                            SRTypes.Weapon.Blade,
                        };

                        // Ignore if weapon is equiped already
                        if (weapons.Any(any => any == myWeapon))
                            return;

                        // Find weapon
                        var slot = InfoManager.Character.Inventory.FindIndex(i => i != null && i.isEquipable() && ((SREquipable)i).IsWeapon() && weapons.Any(any => any == (SRTypes.Weapon)i.ID4), 13);
                        if (slot != -1)
                        {
                            IsSwitchingWeapon = true;
                            PacketBuilder.MoveItem((byte)slot, 6, SRTypes.InventoryItemMovement.InventoryToInventory);
                        }
                    }
                }
            }
        }
        #endregion
    }
}
