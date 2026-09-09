# Changelog

- `core/UBot.Trade` referansıyla eski vSRO ticaretinin ilk güvenli dilimi eklendi: `oldtrade,spawn[,item]`, `oldtrade,buy,0|quantity[,item]` ve `oldtrade,sell`; NPC talk seçeneği 12, taşıta özel `0x7034/0xB034` alım-satım gövdeleri, farklı şehir malı kontrolü ve sunucu yankısı bekleme uygulanıyor. phBot'ın 1-5 yıldız eşiği kaynakta bulunmadığından yanlış miktar yerine açık uyarıyla duruyor.
- phBot benzeri `Trade > Loop / Items / Options` ekranı ve özel trade döngü motoru eklendi. Yönlü şehir rotaları profile kaydedilir; transport/item seçimi, fill/kesin miktar, tekrar, return scroll, normal town hazırlığı, stay mounted/stay off/remount ve güvenli bitiş desteklenir. Kopuk rota zinciri başlatılmaz; taşıtta satılmamış mal varsa terminate engellenir.
- Trade waypoint koruması eklendi: taşıt 25 metreden fazla geride kalırsa karakter geri döner; seçenek açıksa taşıt çevresindeki `TypeID4=2` spawn thief NPC'leri ayarlanan yarıçapta common-attack fallback ile temizlenir.

## 2026-09-08 - Başlangıç ve bildirim alanı düzeltmesi

- Runtime filtre kontrolleri kurulurken tetiklenen ayar kaydı, tamamlanmamış ana pencereyi yeniden oluşturarak sınırsız `Window`/`NotifyIcon` üretmesine neden oluyordu. Pencere oluşturma yeniden girişi ve başlangıç sırasındaki ayar yazımı engellendi.
- Uygulamaya süreçler arası tek örnek kilidi eklendi. İkinci çalıştırma yeni bir tray simgesi oluşturmaz ve açık pencereyi öne getirmeyi dener.
- Normal kapanışta bildirim alanı simgesi hemen gizlenir.
- `Release|Any CPU` yapılandırması, uygulamanın x86 bağımlılıklarıyla uyumlu olacak şekilde x86 hedefleyecek biçimde düzeltildi.
- SQLite başvurusu geçici `bin\Release` dosyasından NuGet paketine taşındı; x86/x64 `SQLite.Interop.dll` dosyaları standart mimari alt klasörlerine kopyalanıyor ve eksik paket içeriği derleme sırasında açık hata veriyor.

### Unreleased
- Stall/Consignment satış filtresine `*` ve `?` wildcard destekli generic item/ServerName kuralları eklendi; tam eşleşme öncelikli, birden fazla wildcard eşleşmesinde en özgül kural seçiliyor.
- `profile[,name]` komutu eklendi; `Config` altındaki adlandırılmış JSON karakter profillerini UI thread'inde güvenle yüklüyor, parametresiz kullanım `Default.json` seçiyor, path traversal ve gereksiz yeniden yükleme engelleniyor.
- `terminate[,horse|transport]` komutu eklendi; parametresiz kullanım aktif at ve transportların tamamını, parametreli kullanım yalnızca seçilen türü doğru pet-terminate paketi ve sunucu kaldırma onayıyla sonlandırıyor.
- phBot uyumlu `DoScript` eklendi: komut yalnızca town scriptinde seçili training alanının yürüyüş dosyasını ilk adımdan çalıştırıyor; eksik/bozuk yol town akışını düşürmeden uyarıya dönüşüyor.
- Script Engine v2 oyuncu stall otomasyonu eklendi: `DoStall`, Consignment ile ortak kalıcı item/adet/fiyat filtresinden stall oluşturuyor, aynı envanter slotunu yeniden eklemiyor, boş stall slotlarını dolduruyor ve create/add/open adımlarında B0B1/B0BA cevaplarını bekliyor. Stall > Options ekranına otomatik açma ve minimum item sayısı ayarları eklendi; minimum sağlanmazsa stall kapalı kalıyor.
- Script Engine v2 Consignment tamamlandı: `DoConsignment` doğrulanmış `NPC_OPEN_MARKET` / `talk=0x21` oturumunda B50E ilanlarını gerçek modele ayrıştırıyor; kalıcı item-adet-fiyat filtreleriyle envanter miktarını aşmadan 7508 register yapıyor, süresi dolanları 7509 ile geri alıyor ve satılanları 750B ile tahsil ediyor. Tüm mutasyonlar B508/B509/B50B onay kapılıdır. Stall ekranına phBot-benzeri Consignment filtre editörü, durum listesi ve otomatik retrieve/settle seçenekleri eklendi; etkin Sevar istemcisinin paket yapıları statik olarak doğrulandı.
- Demirci tamiri mağaza diyaloğundan ayrıldı: NPC seçiminin hemen ardından `0x703E (UID + repairType=2)` gönderiliyor, `0xB03E` başarı/hata cevabı ayrıştırılıp bekleniyor ve yalnız sunucu onayından sonra tamamlandı sayılıyor. Onaysız ilk deneme artık eski `REPAIR` satırını yanlışlıkla bastırmıyor.
- Gold keep amount, phBot davranışıyla bağımsız personal-storage aksiyonu yapıldı: işaretliyken karakterde ayarlanan miktarı bırakıp bütün fazlayı yatırıyor; ayrı storage maksimumu yalnız kendi kutusu seçiliyse uygulanıyor. Legacy `BUY` şehir adımları NPC türüne göre gerçek işlemlere bağlandı: demirci alış hedefinden bağımsız tamir+satış, herbalist satış+potion, grocery satış+arrow/bolt kontrolü, diğer shop NPC'leri satış yapıyor; eski `BUY SMITH` + `REPAIR` dizisinde çift tamir önleniyor.
- Sevar storage kopması/yankısız kalması düzeltildi: envanter ile kişisel/guild storage arasındaki çapraz `0x7034` gövdesi doğru 7 baytlık `type + src + dst + NPC UID` biçimine alındı; bu hareketlerde geçersiz quantity alanı kaldırıldı. Storage oturumu yalnız güncel veri veya isteğe ait `0xB046 talk=3` cevabıyla açılmış sayılıyor ve ilk hareket öncesi hazır olma aralığı uygulanıyor. Pet/storage/guild eşya ve altın hareketlerinin başarılı sunucu cevapları bekleyen işlemi uyandırıyor; bağlantı kopunca bekleme erken kesiliyor. Pet aktarımı başarısız olsa bile gold adımı bağımsız devam ediyor ve karar değerlerini logluyor. Yinelenen disconnect diagnostic çıktısı teke indirildi; gold aksiyon kutularından biri seçiliyse ana gold kutusu kapalı olsa bile yönetim etkinleşiyor.
- Script Engine v2 ilk dilimi: merkezi komut kataloğu ve doğrulama, phBot uyumlu `walk` takma adı, `cast`, `use`, `teleport`, `recall`, `stop`, `disconnect` komutları ve kesilebilir `wait` eklendi. Training > Script alanına komut seçici, parametre editörü, doğrulama ve load/save içeren phBot-benzeri Script Creator yerleştirildi.
- Script Engine v2 town dilimi: `DoBlacksmith`, `DoHerbalist`, `DoStable`, `DoStorage`, `DoStorageStore`, `DoGroceryTrader`, `DoProtectorTrader` ve `DoJupiter` eklendi. Komutlar doğrulanmış NPC/mağaza oturumu üzerinden mevcut tamir, satış, depolama, iksir ve cephane lojistiğine bağlandı; cephane alımı klasik town loop ile ortaklaştırıldı.
- Guild storage script dilimi: `DoGuildStorage` ve `DoGuildStorageStore` eklendi. Guild üyeliği/yetkisi, NPC konuşma sonucu, `0x7250` kilit onayı ve taze `0x3253/0x3255/0x3254` veri zinciri doğrulanmadan eşya veya altın paketi gönderilmiyor; her çıkış yolunda `0x7251` kilit açma uygulanıyor.
- Storage take dilimi: item filtresine ayrı `Take` ve `TakeGuild` sütunları, JSON kalıcılığı ve korunacak boş envanter slotu ayarı eklendi. `DoStorageTake` ile `DoGuildStorageTake`, yalnızca işaretli eşyaları alıyor; her eşya hareketi başarılı sunucu cevabını bekliyor ve onay gelmezse güvenli biçimde duruyor.
- Binek script dilimi: phBot uyumlu `mount[,fellow|transport]`, `dismount` ve `killhorse` komutları eklendi. Binme/inme işlemleri `0x70CB` sunucu cevabını doğruluyor; at/transport sonlandırma doğru `0x70C6` opcode'una taşındı ve petin kaldırıldığı görülene kadar bekleniyor.
- Güvenlik/stabilite turu: Auto Alchemy tek scheduler ve sunucu-cevap kapısına alındı; timeout, hedef item değişimi ve disconnect halinde güvenli durdurma eklendi.
- Hesap parolası, proxy parolası ve ikincil PIN ortak Windows DPAPI katmanına taşındı; şifreleme hatasında düz metin kayıt kaldırıldı ve kullanıcı ayarları Git dışındaki `Settings.user.json` dosyasına ayrıldı.
- Gateway/Agent kısmi socket gönderimi düzeltildi; hassas login/auth/captcha/PIN paketleri loglarda maskelendi ve log rotasyonu eklendi.
- DB cache'i sunucu değişiminde izole edildi; navigasyon LRU eşzamanlı erişimi, ayar debounce UI-thread erişimi ve kapanış flush akışı düzeltildi.
- PK2 okuyucusunda unmanaged bellek sızıntısı, stream dispose ve bozuk arşiv sınır/chain/depth kontrolleri düzeltildi; DDJ temp dosyaları exception-safe temizleniyor.
- Native loader payload'ına magic, sürüm, boyut, FNV-1a checksum ve alan limitleri eklendi; geçersiz payload hook kurulmadan reddediliyor.
- Reklam kaynağı ve hedef URL'ler HTTPS ile sınırlandı; response yapısı/boyutu doğrulanıyor.
- SQLite ortak prepared-command akışına thread ownership eklendi; bağımsız sorgular yerel command nesnesi kullanıyor.
- `xDictionary` anahtar çakışması düzeltildi; koleksiyon ve DPAPI için 5 regresyon testiyle toplam senaryo sayısı 322'ye çıkarıldı.
- srodevs-docs uyumu: `0x3056` EXP TC-buff (cumulated/accumulated) + level-up stat points; `0x304E` STP/HWAN-source/Egypt-AP/display baytları ve otomatik stat dağıtım tetikleme.
- srodevs-docs uyumu: logout (`0x7005/0xB005/0x7006/0xB006/0x300A`) ve rename (`0x7450/0xB450`) parser/builder/handler eklendi; `0x70A7` bodystate enumu ile Berserk uyumlu hale getirildi.
- srodevs-docs uyumu: `0xB025` chat hata kodları, `0x302D` kısıt süresi, `0x3CA2` quest logu; gateway `0xA102` tam hata haritası + custom result `0x03`, `0x2322` IBUV imaj başlığı logu.
- `SroDocsPolicy` saf karar katmanı + 22 yeni senaryo (toplam 307 test).
- Derin tarama turu: `0xB05A/0xB059/0xB060/0xB069/0xB06A/0xB250/0x30FF/0xB309/0x305C/0x3038/0x3091/0x304D` için teşhis handler’ları bağlandı; handlersız server opcode kalmadı.
- `0x302D` chat kısıtı byte düzeltmesi; `0xB006` cancel hata kodu; `0xA103` auth hata adları (C9/C10/full/IP); petition GuildWar(10)/Resurrection(8).
- Ortam durumu `InfoManager`’da tutuluyor (`0x3020/0x3027/0x3809`); gateway `0xA104/0xA106/0xA100` tam yapıda, download stub eklendi; `RestoreCharacter` builder.
- 10 yeni politika senaryosu (toplam 317 test).
- Stabilite: `xDictionary/xList` kilitli hale getirildi, `RemoveAt` ve `Clear` sayaç hataları düzeltildi; snapshot ile güvenli enumerasyon.
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
