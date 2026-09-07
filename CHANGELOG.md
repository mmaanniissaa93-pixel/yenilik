# Changelog

### Unreleased
- srodevs-docs uyumu: `0x3056` EXP TC-buff (cumulated/accumulated) + level-up stat points; `0x304E` STP/HWAN-source/Egypt-AP/display baytları ve otomatik stat dağıtım tetikleme.
- srodevs-docs uyumu: logout (`0x7005/0xB005/0x7006/0xB006/0x300A`) ve rename (`0x7450/0xB450`) parser/builder/handler eklendi; `0x70A7` bodystate enumu ile Berserk uyumlu hale getirildi.
- srodevs-docs uyumu: `0xB025` chat hata kodları, `0x302D` kısıt süresi, `0x3CA2` quest logu; gateway `0xA102` tam hata haritası + custom result `0x03`, `0x2322` IBUV imaj başlığı logu.
- `SroDocsPolicy` saf karar katmanı + 22 yeni senaryo (toplam 307 test).
- Stabilite: `xDictionary/xList` kilitli hale getirildi, `RemoveAt/SetKey` bozuklukları ve `Clear` sayaç hatası düzeltildi; snapshot ile güvenli enumerasyon.
- Ayar kaydı atomik yazıma alındı (`Settings.json` + karakter profili tmp+replace), yarım yazım riski giderildi.
- Proxy hot-path: paket başına UI `Invoke` ve `HexDump` kaldırıldı, bayraklar döngü başına önbelleğe alındı, `TracePacket` dosya IO'su varsayılan kapalı, idle `Sleep(1)` / aktif `Sleep(0)`.
- Captcha: `0x2322` loglanıp `0x6323 SubmitCaptcha` ile otomatik cevaplanıyor; CLI `-captcha=` sabit ayara bağlandı.
- Academy davet handler aktif edildi; karakter silme sonucu loglanıp liste tazeleniyor; bilinmeyen agent opcode'ları seyrek loglanıyor.
- `Settings.json` örneğine `InjectMassive/InjectEncrypted`, `AutoEnterSecondaryPasscode`, `Socks5Proxy/PartySupport/Alchemy/TargetAssist/CommandCenter` blokları eklendi.
- Nav: gerçek LRU (8 bölge), `DateTime.Now` yerine `Stopwatch`, gereksiz path logları kaldırıldı; `ClientManager` `GC.Collect` kaldırıldı.
- Bot: `CancellationTokenSource` + `SleepInterruptible`, `AttackLoop while(true)` koşullu, `Stop()` 2sn Join; uzun teleport/return beklemeleri kesilebilir.
- DB: `SQLDatabase IDisposable` + lock + `using`, `DataManager` concat->parametreli sorgu + id önbelleği (item/skill/model 4000 cap).
- Kayıt: `Settings` 800ms debounce + trailing save, `LearnedUsage` 5sn debounce; `xMapTile` GDI dispose + dosya kilidi fix; emote ikon önbelleği.
- Relogin 50ms->1000ms + modulo bug fix + `Dispose`; TargetAssist 40ms->100ms + reentrancy + Bot.IA çift çağrı kaldırıldı.
- Academy `0xB47D` handler + ham log; GameInfo server saati 2sn canlı; Captcha/Academy/CastInOrder strikeout temizliği; A* node struct + kare mesafe.
- Pot: HP sabitken event gelmezse kontrol hiç çalışmıyordu; 1sn güvenlik yoklaması (bottan bağımsız, cooldown kapılı) + dirilmede zorla kontrol + eşik tutup pot bulunamazsa 15sn'de bir teşhis logu.
- Pot mekaniği referansa (WinForms1) döndürüldü: `FindBestItem`/exclude filtresi yerine `FindItem` ilk eşleşme; `UseItem` doğrudan `GetUsageType()` ile gönderiyor (öğrenilmiş/parity usage, throttle ve 15sn giriş bloğu kaldırıldı).
- Pot usage kendini iyileştiriyor: cooldown-dışı reject'te yanlış kalıcı öğrenme silinip (`ForgetLearnedUsage`) parity alternatifi (`0x..ED/0x..EC`) deneniyor; tutan değer kalıcı öğreniliyor. `bin\Release` bağımlılık DLL'leri tamamlandı (derleme hatası giderildi).
- `0x3013` karakter verisi: summoned fellow pet'i olan serverlarda (SevarOnline) pet
  blogu standarttan uzun geldiginde akis kaymasi oluyordu; skill listesi bos kaliyor
  ve paket sonu tasmasiyla karakter null yukleniyordu. Envanter sonrasi avatar +
  mastery + skill capalariyla dogrulanan otomatik resync eklendi; skill listesi artik
  otomatik doluyor.
- ProtectionManager kontrolleri merkezi bot tick akışına bağlandı.
- Ölüm gecikmesi, şehirde dönüş sonrası durma, level-up, pet envanteri, ok/bolt,
  HP/MP stoğu, dayanıklılık ve envanter dönüş kararları eklendi.
- ProtectionPolicy için 14 bağımsız senaryo testi eklendi.
- ItemFilterManager; pickup, degree/SoX/race/gender, satış ve storage akışlarına bağlandı.
- ItemFilterPolicy senaryolarıyla toplam 25 bağımsız karar testi çalıştırılabilir hale geldi.
- Protection ve Item Filter eksik ayarları UI’ye eklendi; çakışan panel yerleşimleri düzeltildi.
- Item bazlı Pickup/Sell/Store kural editörü eklendi.
- Combat AI kontrolleri Town sekmesinden çıkarılıp `Kasılma > Combat AI` alt sekmesine taşındı.
- CombatPolicy ile alan dışı takip, Dimension Pillar/kaçınma ve Berserk kararları ayrıştırıldı;
  senaryo testleri toplam 32 karara çıkarıldı.
- SkillManager için yanlış silah/cooldown durumlarında sınırlı deneme ve Common Attack fallback’i eklendi.
- `Cast skills in order` seçeneği gerçek saldırı akışına bağlandı; son skill durumu Skills ekranında gösteriliyor.
- SkillPolicy ve ImbuePolicy senaryolarıyla toplam karar testi 44’e çıkarıldı.
- Otomatik giriş, normal UI kullanımında Client ve Clientless modlarında bağlantı → server → karakter adımlarını tamamlayacak şekilde düzeltildi; ayar artık gerçek login akışına bağlı.
- Client modunda otomatik karakter seçimi, karakter listesi client’a aktarıldıktan sonra gönderilecek şekilde geciktirildi; erken `0x7001` paketinin client çökmesine yol açması engellendi.
- `LoginDelaySeconds`, komut satırıyla başlatılan otomatik girişte de gerçek bağlantı başlangıcına uygulanıyor; karakter verisi eksik olduğunda `0xB021` hareket parser’ı da artık null-ref üretmiyor.
- `0x3013` karakter verisi parser’ına SevarOnline pet-watch ve özel quest yerleşimi uyumluluğu eklendi; karakter yüklenirken oluşan stream taşması giderildi.
- Karakter verisi hatalarında byte offset’i raporlanıyor ve eksik karakter durumu bot akışına yayınlanmıyor.
- Arayüz denetimi tamamlandı: Koruma sekmesine Otomatik Stat Dağıtımı (STR/INT oranlama ve anlık dağıtım) paneli eklendi.
- Giriş sekmesindeki Sunucu/Karakter listesiyle çakışan panel (gbxStrategy) temiz bir konuma taşındı; Sabit Captcha metin kutusu, Giriş ve DC gecikme NumericUpDown kontrolleri eklendi.
- Koruma sekmesine Skill HP ve MP yüzde eşik kontrolleri (nudProtectionSkillHP/MP) ve durum özeti eklendi.
- Beceriler sekmesinde durum etiketinin yukarı/aşağı butonlarıyla çakışması ve strikeout font hatası düzeltildi.
- TR/EN dil butonlarına aktif seçim vurgusu ve alt sekme başlık lokalizasyonları bağlandı.

### v0.5.2
- Fixed auto attacking stuff

### v0.5.1
- Fixed quantity on items equipables
- Fixed party auto reform

### v0.5.0
- Restructured all game classes and system
- Added item stats information as tooltip icon
- Added global usage through chat
- Added inventory sort (fastest possible)
- Added storage viewer (shows the items from the last time opened)
- Added option to open/close storage
- Added pet pick inventory viewer
- Added guild viewer
- Added guild storage support & viewer (shows the items from the last time opened)
- Added TIME bot command on all chat to check servertime
- Added ISEEDEADPEOPLE bot command on all chat to reveal hidden players around
- Fixed teleport&buildings not extracted correctly
- Fixed inventory capacity on increasing
- Fixed pet picking directly items to inventory
- Game info viewer has been disabled (Re-Work in progress)

### v0.4.1
- Enabled guild invitation
- Fixed gold edit on exchange
- Fixed gold drop from inventory
- Fixed pet inventory movements

### v0.4.0
- Removed LAG generated by minimap update when tab is not selected
- Added stall full support using clientless mode
- Added sort to packet filter
- Fixed removing item from exchange
- Fixed item quantity update
- Fixed skill damage parsing error
- Fixed bad status not being detected
- Fixed disconnect at gold storage
- Fixed removing buffs on dead
- Improved UX at buttons
- Improved relogin on disconnect

& More improvements and bugfixes..

### v0.3.0
- Added Players tab option
- Fixed inventory update on exchange
- Added exchange full support using clientless mode
- Added options to refuse/accept/confirm/approve exchange automatically
- Improved minimap performance
- Fixed player buffs list from GameInfo
- Fixed auto reform on party match deleted
- Improved silkroad language detection
- Improved proxy switch connection

### v0.2.2
- Fixed switch to clientless mode not getting updated
- Fixed disconnects while login using multiple clients

### v0.2.1
- Fixed map icons
- Tiles from minimap now loading at asynchronous mode
- Added party invitation from minimap

### v0.2.0
- Added updates checker
- Fixed issue creating party match while having job item at inventory
- Improved attacking behaviour
- Improved UI from silkroad settings
- Removed static HWID support (Probably, will be added later as extended protocol)
- Added tracking player states
- Fixed realtime coordinates of all near spawns
- Removed delay generated by using Datetime on coordinates
- Fixed character coordinates now updated every 200ms
- Fixed female items from shops not getting extracted
- Improved adding/removing attacking skills behavior (Now can be ordered by drag)
- Fixed hide/show client getting frozen
- Changed minimap (CefSharp dependency has been removed)
- Added Pk2 minimap images extraction option
- Added moving character from minimap
- Added teleport usage from minimap
- Fixed removing buffs when overlap
- Fixed disconnect when trying to remove buffs from others
- Fixed removing skill from Curst hearts

& More improvements and bugfixes..

### v0.1.0
- Enabled Equip/Unequip item from inventory
- Added PING check (through all chat command)
- Added SRObjectDictionary class to handle stuffs quickly
- Added Start and Stop botting functionality (Only attacking at training area has been implemented)
- Added skill cooldown tracking
- Added skill casting tracking
- Fixed skill casting time
- Fixed a few settings not getting saved on check
- Fixed character AvatarInventory
- Added inventory movements using avatars
- Added inventory avatar viewer
- Added inventory movements using consigment
- Improved relogin on disconnect
- Improved opcode packet filter

& More improvements and bugfixes..

Issues at GetPosition().. Looks like is not calculating the realtime position correctly

### v0.0.11

- Added training areas support
- Added common attack to skill list
- Added filter to attacking skill list
- Fixed Trace setting not saving
- Fixed pk2 not closed on thread abort
- Fixed item quantity not getting initializated
- Fixed Ads window thread not closing

& More improvements and little fixes..

### v0.0.10

- Fixed a few fonts
- Fixed Pk2 Extractor not closing media.pk2
- Fixed item movement not getting updated correctly
- Changed SR_Client button behaviour
- Enabled Drop item option from inventory
- Issues on WinForms overloading, restoring <Window.Designer> as only <DependentUpon> child


### v0.0.9

- Added RECALL to leader commands
- Ignored buffs with 0 as duration
- Fixed buff tracking on entity spawns
- Fixed message on item pick up
- Updated README
