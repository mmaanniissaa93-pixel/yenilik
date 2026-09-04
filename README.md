# xBot

Silkroad Online için WinForms tabanlı bot, istemci yükleyici ve clientless proxy projesi. Proje `vsro1.188` tabanlı eski sürümle başlamış, güncel çalışma ağacında bot otomasyonu, NavMesh navigasyonu ve genişletilmiş ayar yöneticileri geliştirilmektedir.

> Bu README ve özellik envanteri, kodun mevcut durumunu anlatır. Bir özelliğin arayüzde görünmesi, özelliğin her sunucu/sürümde tamamen doğrulandığı anlamına gelmez.

## Durum özeti

- Sürüm temeli: `v0.5.2` changelog’u
- Masaüstü uygulaması: C# WinForms, .NET Framework 4.8
- Native bileşen: C++ `xBot.Loader.Library`, Win32 DLL
- Veri: SQLite, JSON ayarları ve `navdata/*.dat` NavMesh dosyaları
- Arayüz dilleri: mevcut arayüz İngilizce; yeni özel kontrollerde TR/EN dil desteği bulunmaktadır
- Ayrıntılı özellik takip dosyası: [`docs/FEATURES.md`](docs/FEATURES.md)
- Sürüm geçmişi: [`CHANGELOG.md`](CHANGELOG.md)

## Öne çıkan özellikler

### Bağlantı ve istemci

- Client mode ve clientless mode
- Gateway/Agent proxy akışı ve Blowfish paket güvenliği
- Silkroad istemcisi başlatma, DLL injection ve istemci görünürlüğünü değiştirme
- Otomatik giriş, karakter seçimi, tekrar bağlanma ve bağlantı sonrası clientless geçişi
- Sunucu/host/port seçimi; sıralı veya rastgele host seçimi
- Komut satırından kullanıcı adı, parola, sunucu, karakter ve çalışma modu alma

### Bot ve otomasyon

- Botu başlatma/durdurma
- Eğitim alanında saldırı ve hareket döngüsü
- Saldırı/buff becerileri, beceri sıralaması ve cooldown takibi
- HP, MP, vigor, universal, purification, recovery kit, abnormal pill ve pet HGP kontrolleri
- Şehir döngüsü: tamir, depolama, çöp satışı, potion/pill satın alma ve eğitim alanına dönüş
- Trace, koordinat kaydı, script ile hareket ve eğitim alanı desteği
- Pet ile eşya toplama ve eşya filtreleme
- Level-up sonrası otomatik STR/INT dağıtımı

### Navigasyon

- Bölgesel NavMesh dosyalarının okunması ve zlib açılması
- A* yol bulma ve yol yumuşatma
- Bölgeler arası bileşik rota
- Teleport ve ferry bağlantılarının seçilmesi
- Şehir servisleri için NPC bulma ve hedefe yürüyüş
- Minimap üzerinden koordinat seçerek karakter hareketi

### Oyun ve sosyal sistemler

- Karakter, oyuncu, mob, NPC, pet, teleport ve drop bilgilerinin takibi
- Envanter, avatar, storage, pet ve guild storage görüntüleme
- Eşya kullanma, kuşanma/çıkarma, düşürme, taşıma ve envanter sıralama
- Exchange ve altın işlemleri
- Party, party matching, otomatik party daveti/reformu ve leader komutları
- Guild, academy ve guild daveti
- Stall açma, düzenleme, kapatma, görüntüleme ve satın alma
- All/private/party/guild/union/academy/stall/global sohbeti
- Oyun saati ve ping bilgisi için sohbet komutları

### Araçlar ve gözlem

- Minimap ve canlı yakın varlık takibi
- Packet analyzer: istemci/sunucu paketlerini filtreleme ve paket enjeksiyonu
- PK2 extractor: item, skill, model, mastery, teleport, bölge ve minimap verilerini çıkarma
- SQLite tabanlı oyun verisi üretimi
- Otomatik karakter oluşturma ve karakter yönetimi
- JSON tabanlı bot/karakter ayarları ve `Config\\Default.json` ile varsayılan profil

## Kurulum ve çalıştırma

### Gereksinimler

- Windows
- Visual Studio 2017 veya daha yeni bir sürüm
- .NET Framework 4.8 Developer Pack
- C++ workload ve Windows SDK (native loader için)
- Projede tanımlı üçüncü parti DLL’ler: `AutoUpdater.NET`, `DevIL.NET2`, `Newtonsoft.Json`, `System.Data.SQLite`

`packages/` klasörü `.gitignore` kapsamındadır. Gerekli NuGet paketlerini Visual Studio/NuGet üzerinden geri yükleyin. `AutoUpdater.NET`, `DevIL.NET2` ve `System.Data.SQLite` referanslarının proje dosyasında belirtilen `bin\\Release` konumlarında bulunması gerekebilir.

### Derleme

1. `xBot.sln` dosyasını Visual Studio ile açın.
2. Önce `xBot.Loader.Library` projesini, ardından `xBot` projesini derleyin.
3. Native loader Win32 olarak derlenir; istemci mimarisiyle uyumlu bir yapı seçin.
4. Çalıştırmadan önce istemci yolu ve gerekli Silkroad veri yollarını Ayarlar ekranından tanımlayın.

> Bu proje istemci belleğine/prosesine bağlanan bir loader içerir. Yalnızca izinli, güvenli ve ilgili sunucunun kurallarına uygun ortamlarda kullanın.

## Komut satırı seçenekleri

| Seçenek | Açıklama |
| :--- | :--- |
| `-silkroad=?` | Seçilecek Silkroad profili |
| `-username=?` | Oyun kullanıcı adı |
| `-password=?` | Oyun parolası |
| `-server=?` | Sunucu kanalı |
| `-captcha=?` | Captcha değeri; mevcut akışta uygulanmamış olabilir |
| `-character=?` | Seçilecek karakter |
| `--clientless` | Clientless modda çalış |
| `--relogin` | Bağlantı kopunca tekrar giriş yap |
| `--goclientless` | Oyuna girdikten sonra clientless moda geç |
| `--usereturn` | Oyuna girdikten sonra envanterden return scroll kullan |

## Leader sohbet komutları

Komutlar büyük harfle yazılmalıdır. `*` zorunlu, `?` isteğe bağlı parametreyi belirtir. Komutların kullanılabilmesi için gönderen oyuncunun Leader listesinde olması ve Party > Settings içindeki ilgili izinlerin açılması gerekir.

| Komut | Açıklama |
| :--- | :--- |
| `TRACE ?Charname` | Oyuncuyu takip etmeyi başlatır |
| `NOTRACE` | Takibi durdurur |
| `RETURN` | Return scroll kullanır |
| `INJECT *Opcode ?Encrypted ?Data` | Paket enjekte eder |
| `TELEPORT *SourceZoneName *DestinationZoneName` | İsimlerle teleport kullanır |
| `TELEPORT *SourceModelID *DestinationModelID` | Model ID’leriyle teleport kullanır |
| `RECALL *ZoneName` | Teleport recall noktası belirler |

Örnekler: `TRACE JellyBitz`, `INJECT 3091 false 01`, `INJECT 3091 01`, `TELEPORT 2011 2056`, `RECALL hotan`.

## Ayarlar ve dosyalar

- `Settings.json`: genel bot/istemci ayarları
- `Config\\<Silkroad>_<Server>_<Character>.json`: karakter ayarları
- `Config\\Default.json`: isteğe bağlı varsayılan karakter profili
- `navdata\\`: bölge NavMesh verileri
- `client-signatures.cfg`: loader imza/patch tanımları
- `xBot\\Game\\DataManager.cs`: oyun verisi ve veritabanı yollarının merkezi

Yeni bir işlev eklerken şu dosyaları birlikte güncelleyin:

1. [`docs/FEATURES.md`](docs/FEATURES.md) içindeki durum ve kaynak satırı
2. Kullanıcıya görünen önemli değişiklikler için [`CHANGELOG.md`](CHANGELOG.md)
3. Kullanım/ayar değiştiyse bu README

Özellik ekleme şablonu: [`docs/FEATURE_TEMPLATE.md`](docs/FEATURE_TEMPLATE.md).

## Proje yapısı

```text
xBot-WinForms/
├─ xBot/                    # WinForms uygulaması ve oyun mantığı
│  ├─ App/                  # Pencere, bot döngüsü, ayarlar ve yöneticiler
│  ├─ Game/                 # Paketler, oyun modelleri, NavMesh ve veriler
│  ├─ Network/              # Gateway, Agent, Proxy ve bağlantı yönetimi
│  ├─ PK2Extractor/         # PK2 okuma/veri çıkarma
│  ├─ SecurityAPI/          # Paket güvenliği ve Blowfish
│  └─ xGraphics/            # Minimap ve özel WinForms kontrolleri
├─ xBot.Loader.Library/     # C++ client injection/loader DLL’i
├─ xBotLoader/              # Loader kaynak/entegrasyon bileşeni
├─ navdata/                 # Bölge NavMesh dosyaları
├─ docs/                    # Özellik envanteri ve geliştirme takibi
├─ CHANGELOG.md
└─ xBot.sln
```

## Katkı ve geliştirme takibi

Özelliklerin tek takip noktası [`docs/FEATURES.md`](docs/FEATURES.md)’dir. Yeni bir işlev için önce bir `F-xxx` kaydı açın; kod, arayüz, ayar anahtarı ve doğrulama durumunu aynı kayıtta tutun. Durumu `Planlandı → Geliştiriliyor → Deneysel → Kullanımda` akışında güncelleyin. Bir özellik devre dışıysa veya yalnızca arayüz/ayar altyapısı varsa bunu özellikle belirtin.

## Koruma, item filtre ve Combat AI senaryo testleri

Koruma kararlarının temel senaryoları bağımsız bir test projesinde çalıştırılabilir:

```powershell
dotnet run --project tests/ProtectionScenarios/ProtectionScenarios.csproj
```

Bu testler; koruma kararlarının yanı sıra item pickup, degree, SoX, China/Europe,
gender, satış/depolama kuralları ile Combat AI’nin kaçınma, Dimension Pillar,
alan dışı takip ve Berserk tetik kararlarını; SkillPolicy ise cooldown/fallback
ve sıralı combo kararlarını, ImbuePolicy ise Çin Fire/Cold/Lightning skill ve
aktif buff adlandırmalarını kontrol eder. Karakter yüklendiğinde algılanan imbue
skill’leri seviyeleriyle Skills > Attack ekranında listelenir. Toplam 44 senaryo
çalıştırılır.

## Teşekkürler

- Drew “pushedx” Benton — önceki çalışmalar ve kaynaklar
- DaxterSoul — paket yapıları konusunda yardım
- [ConfuserEx2](https://github.com/mkaring/ConfuserEx)
- FontAwesome ve Silkroad görsel/veri kaynakları

Detaylı geçmiş için [`CHANGELOG.md`](CHANGELOG.md) dosyasına bakın.
