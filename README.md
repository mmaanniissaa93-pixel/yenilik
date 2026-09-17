# xBot

Silkroad Online için geliştirilen WinForms tabanlı bot ve istemci araçları. Proje; client ve clientless bağlantı modlarını, paket/proxy katmanını, NavMesh tabanlı hareketi, otomasyon panellerini ve istemciye bağlanan Win32 kütüphanesini tek çözümde toplar.

> Proje geliştirme ve test aşamasındadır. İstemciye bağlanan özellikleri yalnızca izinli ortamlarda kullanın.

## İçerik

- [Özellikler](#özellikler)
- [Gereksinimler](#gereksinimler)
- [Kurulum ve derleme](#kurulum-ve-derleme)
- [Çalıştırma](#çalıştırma)
- [Test senaryoları](#test-senaryoları)
- [Ayarlar ve dosyalar](#ayarlar-ve-dosyalar)

## Özellikler

- Client ve clientless çalışma modları; Gateway/Agent proxy, SOCKS5 ve Blowfish güvenliği.
- Otomatik giriş, karakter seçimi, yeniden bağlanma ve komut satırı seçenekleri.
- Saldırı, buff, imbue, cooldown, combat rotation, potion ve pet kontrolleri.
- Şehir döngüsü: tamir, satış, depolama, alışveriş ve eğitim alanına dönüş.
- Quest, trade loop, stall/consignment, party/guild/academy ve sohbet işlemleri.
- NavMesh okuma, A* yol bulma, teleport/ferry rotaları ve şehir NPC navigasyonu.
- Minimap, paket analizörü/enjeksiyonu, PK2 çıkarma ve SQLite veri üretimi.

Ayrıntılı özellik durumu için [docs/FEATURES.md](docs/FEATURES.md), değişiklik geçmişi için [CHANGELOG.md](CHANGELOG.md) dosyasına bakın.

## Gereksinimler

- Windows ve Visual Studio.
- .NET Framework 4.8 Developer Pack ve Desktop development with .NET iş yükü.
- Native loader için Desktop development with C++, Windows 10 SDK ve projedeki MSVC toolset (v145).
- NuGet paketleri: `Newtonsoft.Json`, `System.Data.SQLite.Core` ve `packages.config` içindeki yardımcı paketler.
- Testler için .NET 8 SDK.

`packages/` klasörü Git dışında tutulur. Visual Studio NuGet geri yüklemesini kullanın veya çözüm kökünde çalıştırın:

~~~powershell
nuget restore .\xBot.sln
~~~

## Kurulum ve derleme

1. `xBot.sln` dosyasını Visual Studio ile açın ve NuGet paketlerini geri yükleyin.
2. Çözüm platformunu `x86` seçin; ana uygulama ve native kütüphane 32 bit hedeflenir.
3. Önce `xBot.Loader.Library`, sonra `xBot` projesini derleyin. Native proje `Client.Library.dll` üretir.
4. Çalıştırmadan önce istemci yolu, Silkroad veri yolu ve sunucu bilgilerini Ayarlar ekranında tanımlayın.

`xBot.Loader.Library` Microsoft Detours ile derlenir. `xBot.Loader.Library/Detours/detours.lib` yoksa native proje derlenemez. Client modunda paket yakalama için `Client.Library.dll` uygulama klasöründe olmalıdır.

EDX loader kullanılıyorsa çalışma klasöründe `xBotLoader.exe`, `xBotLoader.dll` ve gerektiğinde `xBotLoader.ini` bulunmalıdır. Bu dosyalar C# projesi tarafından üretilmez.

## Çalıştırma

Visual Studio'dan `xBot` projesini başlatın veya `xBot/bin/x86/Release/xBot.exe` dosyasını çalıştırın. Uzun mesafe NavMesh hareketi için `navdata/`, şehir rotaları için `Town/` klasörlerinin erişilebilir olması gerekir. İkinci çalıştırma mevcut pencereyi öne getirir.

### Komut satırı seçenekleri

| Seçenek | Açıklama |
| --- | --- |
| `-silkroad=<profil>` | Silkroad profilini seçer |
| `-username=<kullanıcı>` / `-password=<parola>` | Giriş bilgilerini doldurur |
| `-server=<sunucu>` / `-character=<karakter>` | Sunucu ve karakteri seçer |
| `-captcha=<değer>` | Captcha değerini iletir |
| `--clientless` | Clientless modda başlar |
| `--relogin` | Bağlantı kopunca yeniden giriş yapar |
| `--goclientless` | Girişten sonra clientless moda geçer |
| `--usereturn` | Girişten sonra return scroll kullanır |
| `--phbot-test` | Test için ayrı tek örnek kilidi kullanır |

Örnek: `xBot.exe -silkroad=vsro -server=1 --clientless --relogin`

## Test senaryoları

Testler canlı sunucuya bağlanmaz; karar katmanlarını, NavMesh verisini ve WinForms yardımcılarını offline fixture'larla sınar:

~~~powershell
dotnet run --project tests/ProtectionScenarios/ProtectionScenarios.csproj -c Release
dotnet run --project tests/BotEngineScenarios/BotEngineScenarios.csproj -c Release
dotnet run --project tests/CollisionScenarios/CollisionScenarios.csproj -c Release
dotnet run --project tests/FerryNavigationScenarios/FerryNavigationScenarios.csproj -c Release
dotnet run --project tests/MemoryScenarios/MemoryScenarios.csproj -c Release -- navdata
dotnet run --project tests/QuestAutomationScenarios/QuestAutomationScenarios.csproj -c Release
~~~

Ek açıklamalar: [FerryNavigationScenarios README](tests/FerryNavigationScenarios/README.md) ve [MemoryScenarios README](tests/MemoryScenarios/README.md). Bu testler canlı istemci hareketini veya sunucu yanıtlarını doğrulamaz.

## Ayarlar ve dosyalar

- `Settings.json`: başlangıç ayarları.
- `Settings.user.json`: kullanıcı ayarları; parola, proxy parolası veya PIN içerebilir.
- `Config/<Silkroad>_<Server>_<Character>.json`: karakter profili; `Config/Default.json` varsayılan profildir.
- `navdata/`: NavMesh; `Town/`: şehir rotaları.
- `client-signatures.cfg`: loader imza ve patch tanımları.
- `xBot/Game/DataManager.cs`: PK2 ve SQLite veri yollarının merkezi.

`Settings.user.json` ve `Config/` `.gitignore` ile hariç tutulur. Hesap bilgilerini paylaşmadan önce dosyaları kontrol edin.

## Proje yapısı

~~~text
xBot-WinForms/
├─ xBot/                    # .NET Framework 4.8 WinForms uygulaması
├─ xBot.Loader.Library/     # Win32 client hook DLL'i (C++)
├─ xBotLoader/              # EDX loader kaynakları
├─ navdata/                 # NavMesh verileri
├─ Town/                    # Şehir rotaları
├─ tests/                   # .NET 8 offline senaryoları
├─ docs/                    # Özellik ve geliştirme dokümantasyonu
├─ CHANGELOG.md
└─ xBot.sln
~~~

Yeni özelliklerde `docs/FEATURES.md` ve kullanıcıya görünen değişikliklerde `CHANGELOG.md` dosyasını güncelleyin. Sorun bildiriminde işletim sistemi, istemci/sunucu sürümü, yapılandırma, log ve yeniden üretme adımlarını ekleyin; hassas bilgileri paylaşmayın.

## Teşekkürler

- Drew “pushedx” Benton — önceki çalışmalar ve kaynaklar
- DaxterSoul — paket yapıları konusunda yardım
- [ConfuserEx2](https://github.com/mkaring/ConfuserEx)
- FontAwesome ve Silkroad görsel/veri kaynakları
