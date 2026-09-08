# phBot karşılaştırmalı eksik işlev analizi

Bu rapor, `https://guide.phbot.org/` adresindeki phBot Guide dokümantasyonunda
açıkça belgelenen kullanıcı işlevleri ile xBot WinForms projesinin `main`
dalındaki `v3.2` kaynak kodunun karşılaştırılmasıyla hazırlanmıştır.

İnceleme tarihi: 2026-09-08

## Kapsam ve değerlendirme yöntemi

- phBot Guide'daki 32 ana dokümantasyon sayfası incelendi.
- xBot'un arayüzü, ayar modelleri, bot döngüleri, paket oluşturucuları ve
  parser'ları kaynak seviyesinde tarandı.
- Yalnızca opcode sabitinin, parser'ın, veri modelinin veya boş bir arayüz
  panelinin bulunması özellik uygulaması sayılmadı.
- Bir özelliğin mevcut kabul edilmesi için kullanıcı akışının, motorun ve
  gerekli ayar bağlantısının bulunması arandı.
- Bu çalışma oyun içi runtime testi değildir. `Deneysel` olarak işaretlenen
  özelliklerin gerçek sunucularda ayrıca doğrulanması gerekir.

Durumlar:

| Durum | Anlamı |
| :--- | :--- |
| ❌ Yok | Kullanıcı tarafından kullanılabilir bir uygulama bulunmuyor. |
| ⚠️ Kısmi | Temel bölüm var, fakat phBot'taki işlevlerin bir kısmı eksik. |
| 🧪 Bağlı değil | Arayüz, ayar, model veya parser var; gerçek otomasyon yok. |

## 1. Başlatma, giriş ve istemci — ⚠️ Kısmi

Eksik işlevler:

- Clientless durumundan tekrar çalışan istemciye geçme (`Go Client`).
- `Go Client` için `Return/Teleport` ve `Reconnect` yöntemleri.
- Görsel captcha'yı otomatik çözme; xBot yalnızca sabit captcha metni
  gönderebiliyor.
- Başarısız denemeden sonra ayarlanabilir gateway değiştirme sayısı.
- Birden fazla botun sırayla giriş yapmasını sağlayan login queue.
- JC Planet/JCP hesap girişi.
- Login bilgilerini yayın veya ekran paylaşımında gizleme modu.
- X-Trap çalıştırma/izin verme seçeneği.
- Periyodik istemci bellek azaltma. Mevcut kod yalnızca istemci gizlenirken
  tek sefer `EmptyWorkingSet` çağırıyor.
- Tam locale seçme sihirbazı.
- iSRO, kSRO, ruSRO, VTC, Gzone, jSRO, TRSRO, Black Rogue, thSRO, ECSRO,
  RIGID ve farklı vSRO sürümleri için doğrulanmış profil/packet varyantları.
- Private server için division, server type ve sürüm şablonları.
- ProjectHax lisans/zaman ve HWID yönetimi. Bunun xBot ürün modelinde bir
  karşılığı bulunmuyor.

Not: Kodda yalnızca `GoClientless()` bulunuyor; `GoClient()` akışı yoktur.

## 2. Auto Configure ve profil sistemi — ❌ Yok

- Çin karakteri için build/weapon seçerek otomatik skill yapılandırma.
- Avrupa karakteri için primary/secondary weapon üzerinden yapılandırma.
- Karakter başına birden fazla ayar profili.
- Profil oluşturma, adlandırma, yeniden adlandırma ve silme.
- Town loop öncesinde otomatik konfigürasyon.
- Script içinden profil değiştirme.

`Default.json` desteği vardır, ancak phBot'taki çoklu profil sisteminin
karşılığı değildir.

## 3. Bildirimler — ❌ Yok

- Rare item pickup bildirimi.
- Quest'in devre dışı kalması bildirimi.
- Yakında GM belirmesi bildirimi.
- Başka oyuncunun saldırması bildirimi.
- Mob tarafından öldürülme bildirimi.
- Tray balon bildirimleri.
- Bildirim geçmişi ve bildirim türü ayarları.

Mevcut `NotifyIcon` yalnızca pencere ve tray yönetimi için kullanılıyor.

## 4. Protection ve dönüş koşulları — ⚠️ Kısmi

xBot'ta ölüm, ok/bolt, dolu çanta, dolu pet, düşük HP/MP, durability ve
level-up dönüşleri vardır. Eksik phBot koşulları:

- HP potion adedi belirli sayının altına düşünce dönüş.
- MP potion adedi kontrolü.
- Universal pill bitince dönüş.
- Vigor bitince dönüş.
- Speed scroll bitince dönüş.
- Silah, kalkan ve zırh dayanıklılıklarını ayrı ayrı izleme.
- Union Party Ticket bitince dönüş.
- Energy of Life bitince dönüş.
- Her saatten X dakika önce dönüş.
- Her X dakikada dönüş.
- Her gün belirli saatte dönüş.
- X dakika sonra dönüp disconnect olma.
- Her X dakikada doğrudan disconnect.
- Styria zamanına göre dönüş.
- Party oyuncu sayısı X'in altına düşünce dönüş.
- VTC play-time sıfırlama.
- Training area dışında ölünce hemen dönme.
- Ölünce resurrection scroll kullanma.
- X dakika mob saldırısı alınmazsa dönüş.
- Yakında unique çıkınca dönüş.
- Job transport ölünce dönüş.
- Pink status durumunda dönüş.
- Murder status durumunda disconnect.
- Kırılan ana silahı aynı tür yedek silahla değiştirme.
- Gear socket skill'lerini otomatik kullanma.

Eksik pet dönüş koşulları:

- Recovery kit bittiğinde dönüş.
- Pet revive item bittiğinde dönüş.
- Feed item bittiğinde dönüş.
- Pet abnormal potion bittiğinde dönüş.
- Transport recovery kit bittiğinde dönüş.
- Pick pet dolduğunda dönüş.
- Attack pet öldüğünde ayrı dönüş politikası.

Kaynak: `xBot/App/ProtectionManager.cs`.

## 5. Town sistemi — ⚠️ Kısmi

Mevcut motor repair, storage, trash sell, HP/MP, pill ve ammo işlemlerini
yapıyor. Eksikler:

- Oyundaki tüm NPC ürünlerini listeleyen satın alma kataloğu.
- Ürün adına göre arama.
- Her ürün için ayrı hedef miktar.
- Town satın alma ayarlarını sıfırlama.
- Yakında guild üyesi varsa guild storage'ı atlama.
- Guild storage giriş tekrar sayısı.
- Satın alma sırasında inventory dolarsa botu durdurma.
- Script sırasında speed/noise skill'lerini kapatma.
- Fazla potion/arrow/bolt satma.
- `Drop` filtreli eşyaları town'da düşürme.
- Düşük seviye `_r.txt` town scriptlerini devre dışı bırakma.
- Guild storage'a gerçekten eşya koyma.

`Bot.IA.cs`, guild storage adaylarını desteklenmiyor mesajıyla atlıyor.

## 6. Training Area ve rota — ⚠️ Kısmi

- Polygon training area oluşturma ve kullanma.
- Haritadan polygon kaydetme/kopyalama.
- Seviyeye bağlı otomatik training area değiştirme koşulları.
- Seviyeye uygun mob seçerek training area oluşturma.
- Tam script editörü.
- Teleport sonrasında otomatik `wait` ekleyen gelişmiş recorder.
- Mob/item engel arkasındaysa collision filtresi.
- Item drop'a NavMesh üzerinden gitmeyi ayrı açıp kapatma.
- Samarkand/Alexandria teleportlarını devre dışı bırakma.
- Guide/Advice NPC'lerini devre dışı bırakma.
- Teleport level sınırını yok sayma.
- Treasure Box kullanma.
- Easter Egg NPC kullanma.
- Daha iyi item'ı otomatik kuşanma.
- Flower summon.
- Repair hammer kullanma.
- Berserker regeneration potion.
- Energy of Life kullanımı.
- Ölüm veya return sonrasında reverse return scroll.
- Monster summon scroll ve Pandora's Box.
- Güçlü moblar ölmeden yeni summon açmama.
- Speed drug'ı yalnızca scriptte kullanma.
- Yarım kalan town scriptini reconnect sonrası sürdürme.
- Script devam ettirilemiyorsa return kullanma.
- Statue of Justice'tan kaçınma.
- Script adımı gecikmesi.
- Takılınca önceki koordinata dönme.
- Scriptte takılınca return.
- Cave girişinden sonra yeniden mount.
- Son recall/death konumuna Guide NPC ile dönme.

xBot'un A* ve NavMesh motoru vardır; eksik olanlar bu motoru yöneten phBot
düzeyindeki seçeneklerdir.

## 7. Script komutları — ⚠️ Kısmi (Sprint 1 başladı)

xBot'un önceki `MOVE`, `STORE`, `BUY`, `REPAIR` ve `WAIT` komutlarına ek olarak
ilk entegrasyon diliminde `walk` takma adı, `teleport`, `cast`, `use`, `stop`,
`disconnect` ve toplama peti `recall` komutları eklendi. `WAIT` artık kesilebilir;
Training > Script alanında ortak komut kataloğunu kullanan phBot-benzeri Script
Creator, doğrulama ve load/save akışı bulunur.

Kalan eksik komutlar:

- `DoBlacksmith`
- `DoHerbalist`
- `DoStable`
- `DoStorage`
- `DoGuildStorage`
- `DoGroceryTrader`
- `DoProtectorTrader`
- `DoJupiter`
- `DoStorageTake`
- `DoGuildStorageTake`
- `DoStorageStore`
- `DoGuildStorageStore`
- `DoConsignment`
- `DoStall`
- `DoScript`
- `mount`
- `killhorse`
- `terminate`
- `dismount`
- `quest`
- `begintargettrading`
- `settletargettrading`
- `styria`
- `oldtrade`
- `profile`

Temel hareket için hem `walk` hem `MOVE` kabul edilir. Geniş komut ailesi ve
ileri kontrol akışı henüz bire bir uyumlu değildir.

Kaynak: `xBot/App/Script.cs`.

## 8. Attack, buff ve support — ⚠️ Kısmi

Eksik saldırı seçenekleri:

- Kill steal.
- Saldırı altındaki party üyesini korumak için hedef değiştirme.
- Warlock için X DOT sonrası hedef değiştirme.
- Mob tipine özel skill yoksa bir alt tipin skill listesini kullanma.
- Slower attack mode.
- Lagtastic paket modu.
- Bot kapalıyken yakındaki unique'i otomatik seçme.
- Bot kapalıyken titan seçme.
- Ghost Walk/Teleport skill'iyle moba yaklaşma.

Eksik buff seçenekleri:

- Buff biter bitmez saldırı sırasında yenileme tercihi.
- Mirror Reflect.
- HP/MP acil buff listesi.
- Saldıran mob sayısına göre emergency buff.
- Bir secondary buff bitince tüm secondary buff'ları yenileme.

Force Cure benzeri bad-status temizleme ve Devil Spirit desteği mevcuttur.

## 9. Party buff, resurrect, heal ve lure — ⚠️ Kısmi

Party Support motoru temel heal, resurrect ve cure yapıyor. Eksikler:

- Skill → oyuncu eşlemeli party buff listesi.
- Party buff radius.
- Buff sonrası merkeze dönüş.
- Yakındaki party dışı oyuncuları bufflama.
- Belirli oyuncu yakındaysa party buff açma/kapatma.
- Resurrect edilecek oyuncu listesi.
- Resurrect radius ayarı.
- Resurrect gecikmesi.
- Group resurrection önceliği.
- Resurrect deneme sınırı.
- Party ortalama HP'sine göre group heal.
- Ayrı self-heal yapılandırması.
- Lure sistemi.
- Howling Shout tabanlı rastgele lure.
- Lure scripti.
- Ölü party üyesi/giant/mob sayısına göre lure durdurma.
- Lure edge/center/half-delay.
- Player attack log ve PK ölüm kaydı.

`Party Buff` paneli boştur. `PartyBuffsEnabled` ayarı kaydedilip yüklenir,
ancak çalışma motorunda kullanılmaz.

Kaynaklar: `xBot/App/PartySupportManager.cs` ve
`xBot/App/Window.Designer.cs`.

## 10. Pet — ⚠️ Kısmi

- Attack pet kullanımını bağımsız açma/kapatma.
- Maksimum auto-revive sayısı.
- Pet ölünce town'a dönüş politikası.
- Fellow pet SP recall.
- Pet saldırıya uğradığında onu koruma.
- Attack pet passive/no-attack modu.
- Yalnızca town'da summon.
- Yalnızca training area'da summon.
- Revive item yoksa town'a dönüş.
- HP potion varsa revive etme koşulu.
- Transport ve pick pet için tam bilgi/aksiyon ekranları.

Auto summon, auto revive ve pet potion kullanımı mevcuttur.

## 11. Party ve Union Party — ⚠️/❌

Normal party'de eksikler:

- Accept delay.
- Yalnızca training area'da invite kabul etme.
- Leader training area'ya dönünce party değiştirme mantığı.
- Matching'e oyuncu adına göre otomatik katılma.
- Matching başlığına göre otomatik katılma.
- Forgotten World summon popup davranışı. Checkbox vardır fakat bağlı handler
  bulunmuyor.
- Taxi ödeme sistemi.
- Saatlik ücret.
- Süre dolunca kick.
- Ödeme için private message.
- Tekrarlayan ödeme ihlallerinde blacklist.

Union Party tamamen eksiktir:

- Üç birleşik party görünümü.
- Union party daveti gönderme.
- Davet kabul etme.
- Invite/accept listeleri.
- Union party type.

Union chat kanalının bulunması Union Party yönetimi sağlamaz.

## 12. Pick Filter ve storage — ⚠️ Kısmi

Pick Filter'ın önemli bölümü uygulanmıştır. Kalan eksikler:

- `StoreGuild` kuralını gerçek guild storage hareketine dönüştürme.
- Guild storage'dan altın alma.
- Guild storage'a altın koyma.
- Otomatik dismantle paketi ve motoru.
- `Pet item'larını store/sell dışında hareket ettirme` seçeneğini gerçek akışa
  bağlama.

Dismantle arayüzü ve karar politikası vardır; motor adayları yalnızca loglayıp
atlar.

Kaynaklar: `xBot/App/ItemFilterManager.cs`,
`xBot/App/Window.PickFilter.cs` ve `xBot/App/Script.cs`.

## 13. Quest — ❌ Kullanılabilir otomasyon yok

Quest paketleri ve modelleri bulunur, ancak phBot seviyesinde Quest sekmesi ve
motoru yoktur:

- Active quest listesi.
- Quest abandon.
- Tüm quest kataloğu ve arama.
- Quest enable/disable.
- Tamamlanınca return.
- Tamamlanınca script.
- Seviyenin üstündeki quest'i alma.
- EXP ratio kontrolü.
- Training area'dan town'a yürüyerek teslim.
- Tüm enabled questler tamamlanana kadar bekleme.
- Yolda quest teslim etme.
- Town'da event quest tamamlama.
- Safe/danger job quest seçimi.
- Script kullanmadan collision tabanlı auto quest.
- Genie Lamp akışı.
- Job Cave quest döngüsü.
- Quest return scripti.

`0x3CA2` quest script parser'ının bulunması bu kullanıcı otomasyonlarını
sağlamaz.

## 14. Guild ve Academy — ⚠️/❌

Guild'de eksikler:

- Gelen guild invite popup.
- Tüm guild davetlerini otomatik kabul etme.
- Tahmini seviyeye göre yakındaki oyuncuları otomatik davet etme.

Academy tarafında yalnızca yakındaki oyuncuya davet paketi vardır. Eksikler:

- Academy üye listesi ve notice.
- Invite popup.
- Tüm davetleri veya listedekileri kabul etme.
- Tüm oyuncuları veya listedekileri davet etme.
- Listede olmayanları reddetme.
- Otomatik mezuniyet.
- Graduate level.
- PM password doğrulaması.
- Accept/invite listesi.
- Academy matching oluşturma.
- Matching listesi.
- Matching numarası, oyuncu adı veya başlıkla katılma.

Academy ana paneli kaynakta boştur. `docs/FEATURES.md` içindeki Academy kaydı
bu nedenle `yalnızca davet, bilgi yok` şeklinde değerlendirilmelidir.

## 15. Inventory özel işlemleri — ❌ Yok

- Job ticket exchange.
- İstenmeyen job reward'larını düşürme.
- Balloon event otomasyonu.
- Awakening Enhancement ile Devil/Angel Spirit yükseltme.
- Gori item exchange.
- İstenen white stat değerine ulaşınca Gori exchange'i durdurma.

Standart inventory, avatar, storage, pet inventory ve player exchange
mevcuttur.

## 16. Stall ve Consignment — ⚠️ Kısmi

Temel stall oluşturma, düzenleme ve alım vardır. Eksikler:

- Item fiyatlarını kalıcı filtre kataloğunda saklama.
- Generic item için stall filter.
- Otomatik stack birleştirme/bölme.
- Relog sonrası otomatik stall.
- Town loop öncesi/sonrası stall tercihi.
- Minimum stall item sayısına göre açma.
- Consignment görünümü.
- Consignment item ekleme.
- Süresi dolanı geri alma.
- Satış parasını settle etme.
- `DoConsignment` script entegrasyonu.

Düşük seviyeli consignment opcode tanımları vardır, fakat kullanıcı akışı
yoktur.

## 17. Alchemy — ⚠️ Yalnızca temel plus

Mevcut motor tek item, hedef plus, elixir ve isteğe bağlı lucky powder
destekler. Eksikler:

- Çoklu item alchemy queue.
- Ayrı success/failure delay.
- Belirli plus'tan sonra lucky powder kullanma.
- Astral.
- Steady.
- Immortal.
- Lucky stone.
- Failure sonrası sonraki item'a geçme.
- Item kırılırsa tüm queue'yu durdurma tercihi.
- Manuel item/powder degree override.
- 12D+ enhancer.
- Attribute stone ve yüzde hedefi.
- Alchemy catalyst.
- Tüm uygun item'ları queue'ya ekleme.
- Alchemic stone ve blue hedef değeri.
- Add All Blues.
- Gerçek dismantle.
- Disjoint.
- Shining Stone üretme.
- Tablet üretme.

Kaynak: `xBot/App/AlchemyManager.cs`.

## 18. Trade — ❌ Yok

- Town-to-town trade loop editörü.
- Başlangıç/bitiş town listesi.
- Transport seçimi.
- Star, quantity veya fill modunda mal alma.
- Trade item seçimi.
- Loop tekrar sayısı.
- Spawn olan thief'lere saldırma.
- Transport üstünde kalma.
- Transporttan inme.
- Saldırı yokken yeniden mount.
- Loop sonunda return.
- Town loop'u atlama.
- Yeni target trading.
- Eski vSRO trade sistemi.

Specialty goods paket modellerinin bulunması otomatik trade motoru sağlamaz.

## 19. Mastery ve skill geliştirme — ❌ Yok

- Otomatik mastery yükseltme.
- Otomatik skill yükseltme.
- Skill gap koruma.
- Mastery/skill seçim ağacı.
- Auto mastery temizleme.
- Auto skill temizleme.

Level-up cevap parser'ları ve opcode enumları vardır, ancak istek gönderen
yönetici ve kullanıcı arayüzü bulunmaz.

## 20. Map işlevleri — ⚠️ Kısmi

- Polygon çizimi.
- Styria kaydı.
- Random Flag kaydı.
- Random Points kaydı.
- Party Flag kaydı.
- Party Points kaydı.
- Haritadan So-Ok event quest ödüllerini toplama.
- Jupiter room teleport.
- Belgelenmiş tam cave-map işlev seti.

Training area belirleme, storage/guild storage erişimi, normal teleport ve
recall'ın bazı bölümleri mevcuttur.

## 21. Sound — ❌ Yok

Aşağıdaki olaylara özel ses sistemi yoktur:

- GM spawn.
- Unique/titan spawn.
- Unique range.
- Karakter ölümü.
- Başka oyuncuya saldırma.
- Town'a dönme.
- Private message.
- Oyuncu saldırısı.
- Thief/hunter yakında.
- Rare pickup.
- Transport ölümü.
- Unique adına özel ses seçimi.

## 22. Key Bindings — ❌ Genel sistem yok

- Start/stop bot hotkey.
- Start/stop trace hotkey.
- Get Position hotkey.
- Conditions enable/disable hotkey.
- Python fonksiyonu çalıştırma.
- Aynı tuşla birden fazla botu yönetme.

Yalnızca Target Assist için hedef değiştirme tuşu vardır; bu phBot'un genel
key-binding sistemi değildir.

## 23. Conditions — ❌ Yok

- Görsel `If → Then` koşul editörü.
- Birden fazla `Then` aksiyonu.
- Bot açık/kapalı, tracing veya stalling durumundan bağımsız koşullar.
- Conditions global enable/disable.
- Python/plugin aksiyonuyla birlikte çalışma.

Combat ve protection policy sınıfları kullanıcı tarafından oluşturulan genel
Conditions sisteminin karşılığı değildir.

## 24. Manager ve çoklu instance — ❌ Yok

- Birden fazla xBot instance'ını tek yerden görüntüleme.
- Toplu start/stop.
- Toplu hide/show.
- Disconnect olan instance'ı yeniden başlatma.
- Hesap ve komut satırı profillerini Manager'dan yönetme.

xBot, `Program.cs` içindeki mutex ile ikinci uygulama instance'ını özellikle
engellemektedir. Bu davranış phBot Manager hedefiyle ters düşer.

## 25. Komut satırı ve global ayarlar — ⚠️ Kısmi

xBot yalnızca `-silkroad=`, `-username=`, `-password=`, `-captcha=`,
`-server=`, `-character=`, `--clientless`, `--goclientless`, `--relogin` ve
`--usereturn` parametrelerini işler.

phBot'a göre eksik komut satırı parametreleri:

- `--passcode`
- `--startbot`
- `--locale`
- `--minimize`
- `--disablemap`
- `--bindip`
- `--loginserver`
- `--proxyip`
- `--proxyport`
- `--proxyuser`
- `--proxypass`
- `--disabletray`
- `--logsize`
- `--skipupdates`
- `--update`
- `--wine`
- `--hide`
- `--hideclient`
- `--privateserver`
- `--white`
- `--starttrace`
- `--tracename`
- `--fontsize`
- `--force-inject`
- `--minidump`
- `--silkroadr-channel`
- `--profile`

Global ayarlarda ayrıca bind IP, pencere başlığı şablonu, tray'i kapatma,
SOCKS4, AllowXTrap ve kalıcı memory reducer eksiktir.

Kaynak: `xBot/App/Window.cs` içindeki `LoadCommandLine()`.

## Mevcut özellik envanterinde düzeltilmesi gereken kayıtlar

`docs/FEATURES.md` içindeki bazı başlıklar phBot eşdeğerinden daha tamamlanmış
görünmektedir:

| Kayıt | Kaynak kodun gösterdiği gerçek kapsam |
| :--- | :--- |
| F-041 | Town alış sistemi tüm NPC item'larını değil, sabit potion/pill/ammo akışını destekliyor. |
| F-058 | Alchemy yalnızca temel plus; diğer alchemy modülleri yok. |
| F-065 | Academy yalnızca invite seviyesinde; bilgi ve yönetim paneli boş. |
| F-067 | Temel stall var; filter, consignment ve otomatik stall yok. |
| F-038 | Heal/resurrect/cure var; party buff ayarı çalışma motoruna bağlı değil. |
| F-056 | Normal storage çalışıyor; guild storage otomasyonu çalışmıyor. |
| Dismantle | Arayüz ve seçim politikası var, gerçek söküm işlemi yok. |
| Store Gold | Normal storage altını çalışıyor; guild storage altını çalışmıyor. |

## Öncelik önerisi

İşlevleri phBot'a yaklaştırmak için önerilen geliştirme sırası:

1. Quest motoru ve genişletilmiş script komutları.
2. Guild storage, dismantle ve consignment gibi yarım kalmış mevcut UI
   akışlarının tamamlanması.
3. Party buff/resurrect/heal yapılandırmasının tamamlanması.
4. Mastery/skill otomasyonu.
5. Trade ve Union Party motorları.
6. Protection ve training-area gelişmiş koşulları.
7. Tam alchemy queue ve stone/crafting modülleri.
8. Notifications, Sound, Conditions ve Key Bindings.
9. Çoklu instance Manager.
10. Geniş komut satırı, locale ve istemci geçiş seçenekleri.

## Kaynaklar

- phBot Guide: <https://guide.phbot.org/>
- phBot Guide MCP: <https://guide.phbot.org/~gitbook/mcp>
- Mevcut xBot envanteri: `docs/FEATURES.md`
- xBot README: `README.md`
- Script motoru: `xBot/App/Script.cs`
- Protection: `xBot/App/ProtectionManager.cs`
- Party support: `xBot/App/PartySupportManager.cs`
- Pick Filter: `xBot/App/ItemFilterManager.cs`
- Alchemy: `xBot/App/AlchemyManager.cs`
- Komut satırı: `xBot/App/Window.cs`
- Çoklu instance kısıtı: `xBot/Program.cs`
