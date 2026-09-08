# xBot WinForms — Tersine Mühendislik ve Teknik İyileştirme Raporu

**İnceleme tarihi:** 8 Eylül 2026  
**İncelenen revizyon:** `f6eafb5`  
**Kapsam:** C# WinForms uygulaması, ağ proxy katmanı, paket güvenliği/parsing, bot yöneticileri, navigasyon verisi, PK2 okuyucu, SQLite erişimi, ayarlar ve Win32 C++ loader.  
**Yöntem:** Kaynak kod üzerinden statik tersine mühendislik, hedefli kod deneyleri, Release/x86 tam yeniden derleme, mevcut senaryo testleri ve tüm NavMesh dosyalarının yapısal kontrolü.

> Bu rapor canlı oyun sunucusuna bağlanmadan hazırlanmıştır. Paket uyumluluğu, gerçek istemci enjeksiyonu, uzun süreli bellek/CPU davranışı ve oyun içi otomasyon sonuçları ayrıca entegrasyon ortamında doğrulanmalıdır.

## 1. Yönetici özeti

Proje işlev bakımından geniş ve çalışır durumdadır. Release/x86 yapılandırmasında hem `xBot.exe` hem `Client.Library.dll` yeniden derlenmiştir. Başlangıçtaki 317 politika senaryosuna koleksiyon bütünlüğü ve DPAPI için 5 regresyon testi eklenmiştir. Depodaki 77 navigasyon dosyasının tamamı açılmış; 1.062.557 noktanın temel kayıt yapısı geçerli bulunmuştur.

Buna rağmen üretim güvenilirliğini etkileyen birkaç önemli kusur vardır. En yüksek risk otomatik simya akışındadır: aynı işlem iki farklı zamanlayıcıdan tetiklenebilmekte, önceki sunucu cevabı beklenmeden yeni deneme gönderilebilmekte ve bağlantı kesildiğinde çalışan durum sıfırlanmamaktadır. Bu, yeni oturumda yanlış envanter slotuna işlem uygulanmasına veya beklenenden fazla eşya tüketimine yol açabilir. Kimlik bilgileri tarafında DPAPI kullanılmış olsa da koruma başarısız olduğunda parola düz metin kaydedilmektedir; global SOCKS5 parolası ve varsayılan ikincil PIN zaten doğrudan JSON'a yazılmaktadır.

Öncelikli teknik borçlar ağ gönderim döngüsündeki kısmi gönderim hesabı, `xDictionary.SetKey` veri bütünlüğü hatası, sunucu değişiminde temizlenmeyen veri önbelleği, PK2 okuyucusundaki native bellek sızıntısı ve arka plan thread'inden WinForms kontrollerine erişen ayar kayıt mekanizmasıdır.

Önerilen uygulama sırası:

1. Otomatik simya ve gizli bilgi saklama sorunlarını kapatın.
2. Ağ gönderimini, koleksiyon bütünlüğünü ve profil önbelleğini düzeltin.
3. Thread yaşam döngüsünü ve ayar kayıt mimarisini güvenli hale getirin.
4. PK2/NavMesh girdi doğrulamasını ve gözlemlenebilirliği güçlendirin.
5. Büyük sınıfları parçalayıp entegrasyon testleri ve CI kurun.

## 2. Projenin tersine çıkarılmış mimarisi

### Uygulama durumu

Bu rapordan sonra öncelik sırasıyla uygulanan düzeltmeler:

| Bulgu | Durum | Uygulanan değişiklik |
|---|---:|---|
| P0-01 Otomatik simya | ✅ | Tek scheduler, bekleyen cevap kapısı, 15 sn güvenli timeout, item referansı/ID kontrolü ve disconnect stop |
| P0-02 Gizli bilgiler | ✅ | Ortak DPAPI `SecretStore`; plaintext fallback kaldırıldı; hesap, global proxy ve PIN alanları migrate edildi; çalışma ayarları Git dışındaki `Settings.user.json` dosyasına taşındı |
| P1-01 Kısmi socket send | ✅ | Gateway/Agent için ortak kalan-byte hesabı kullanan `SendAll` |
| P1-02 `xDictionary.SetKey` | ✅ | Çakışmayı reddeden `TrySetKey`; 3 regresyon senaryosu |
| P1-03 DB cache izolasyonu | ✅ | Başarılı DB değişimi/disconnect sırasında dispose ve cache temizleme |
| P1-04 PK2 güvenliği | ✅ | Native bellek serbest bırakma, kısa struct/range/depth/chain döngüsü kontrolleri ve gerçek stream dispose |
| P1-05 Ayar thread güvenliği | ✅ | Gecikmeli kayıt UI thread'ine taşındı; kapanışta senkron flush |
| P1-06 Navigasyon LRU | ✅ | Kilitli cache erişimi ve yükleme sonrası double-check |
| P1-07 Loader payload | ✅ | Magic, format sürümü, uzunluk, FNV-1a checksum ve alan limitleri; hatada hook aktivasyonu yok |
| P2-01 Reklam kaynağı | ✅ | HTTPS zorunluluğu, response/kolon/URL doğrulaması ve yalnız HTTPS link açma |
| P2-02 Hassas loglar | ✅ | Login/auth/captcha/PIN opcode redaction ve 5 MB log rotasyonu |
| P2-04 Thread yaşam döngüsü | 🟡 | Simya wake/stop düzeltildi; proxy ve teleport worker'ları background yapıldı; kalan worker'lar sonraki refactor kapsamında |
| P2-05 SQLite command paylaşımı | ✅ | Sonuç sorgusu/prepared command için thread ownership; diğer sorgular için yerel command nesnesi |
| P2-06 DDJ temp dosyaları | ✅ | `try/finally` temizliği ve kısa/null veri doğrulaması |

Bu tablo uygulanan kod durumunu gösterir; aşağıdaki bulgular, değişikliklerin gerekçesini ve kabul ölçütlerini tarihsel olarak korur.

```text
Program.cs
  └─ Window (WinForms, komut satırı, kullanıcı olayları)
      ├─ Settings / AccountManager / politika ve özellik yöneticileri
      ├─ Bot (döngüler, savaş, koruma, şehir, item, party)
      └─ Proxy
          ├─ Gateway bağlantısı
          ├─ Agent bağlantısı
          ├─ SecurityAPI (şifreleme, framing, paket kuyruğu)
          └─ PacketParser / PacketBuilder
               └─ InfoManager (oturum içi oyun durumu)
                    ├─ DataManager → SQLite / PK2 ile üretilmiş oyun DB'si
                    └─ NavigationManager → NavDataReader → A* / teleport

İstemcili mod:
Window → ClientManager → askıya alınmış SR_Client → DLL injection
       → Client.Library.dll → Winsock detour → yerel Gateway/Agent proxy

Clientless mod:
Window → Proxy → doğrudan Gateway/Agent protokol akışı
```

### Ana veri akışları

- **Giriş:** UI veya komut satırı → `InfoManager.SetCredentials` → `PacketBuilder.Login` → Gateway → Agent.
- **Paket:** Socket → `Security.Recv` → paket çözme → `Gateway/Agent.PacketHandler` → `PacketParser` → `InfoManager` ve UI.
- **Bot kararı:** timer/thread → politika sınıfı → manager → `PacketBuilder` → Agent gönderim kuyruğu.
- **Kalıcı ayar:** WinForms kontrolleri ve manager durumları → `JObject` → Git dışındaki `Settings.user.json` veya `Config/*.json`; `Settings.json` başlangıç şablonudur.
- **Oyun verisi:** PK2 çıkarımı → SQLite → `DataManager` sorguları ve süreç içi cache.
- **Navigasyon:** karakter/hedef koordinatı → bölge seçimi → NavMesh yükleme/cache → A* → yumuşatılmış waypoint dizisi.

### Kod hacmi ve yoğunlaşma

İnceleme kapsamında, derleme çıktıları ve üçüncü parti Detours kaynakları hariç 151 C#/C++/header dosyasında yaklaşık 65.149 satır vardır. En yoğun dosyalar:

| Dosya | Yaklaşık satır | Sonuç |
|---|---:|---|
| `App/Window.Designer.cs` | 11.898 | Normal WinForms üretimi; elle düzenlenmemeli |
| `App/Window.cs` | 4.119 | UI, oturum ve çok sayıda özellik birbirine bağlı |
| `Game/PacketParser.cs` | 3.835 | Protokol ayrıştırma tek sınıfta yoğunlaşmış |
| `App/Window.CustomTabs.cs` | 3.581 | UI üretimi, binding ve iş mantığı karışık |
| `App/Bot/Bot.IA.cs` | 2.725 | Çok sayıda bot durumu tek döngüye bağlı |
| `App/Window.ModernTheme.cs` | 2.566 | Tema ile yerleşim davranışı iç içe |
| `Game/InfoManager.cs` | 1.874 | Global mutable oturum durumu |
| `PK2Extractor/PK2Extractor.Parser.cs` | 1.521 | Format ayrıştırma ve veri üretimi yoğun |

## 3. Doğrulanan kritik ve yüksek öncelikli bulgular

### P0-01 — Otomatik simya cevabı beklemeden tekrar deneyebilir

**Kanıt:** `App/AlchemyManager.cs` kendi `AlchemyLoop` thread'inde `RunTick()` çağırıyor. Aynı metot `App/Bot/Bot.IA.cs` içindeki ana bot döngüsünden de çağrılıyor. `Monitor.TryEnter` yalnızca aynı anda çalışmayı engelliyor; sunucuya gönderilmiş ve cevabı beklenen bir denemeyi temsil eden `isPending`/request id/timeout durumu yok. `CurrentAttempts`, paket gönderilmeden hemen önce artıyor. `InfoManager.OnDisconnected` → `Bot.OnDisconnected` akışı `AlchemyManager.Stop()` çağırmıyor.

**Etki:** Geciken sunucu cevabında peş peşe fuse paketleri, fazla elixir/powder tüketimi, bağlantı sonrası eski slot ve hedefle işlemin devam etmesi, kırılabilen eşyalarda maddi kayıp.

**Değişiklik:** Tek bir zamanlayıcı sahibi seçin. `Idle → RequestSent → ResultReceived/TimedOut → Idle` durum makinesi kurun. Her denemede en az bir sunucu cevabı veya kontrollü timeout bekleyin. Disconnect, teleport, karakter değişimi ve envanter slot kimliği değişiminde işlemi kesin olarak durdurun. Başlangıçta slotun item unique/model kimliğini saklayıp her denemeden önce eşleşmeyi doğrulayın.

**Kabul testi:** Gecikmiş/tekrarlı/kayıp `0xB150` cevapları altında bir bekleyen istekten fazlası oluşmamalı; disconnect/relogin sonrası hiçbir fuse paketi kendiliğinden gönderilmemeli.

### P0-02 — Gizli bilgiler bazı yollarda düz metin saklanıyor

**Kanıt:** `AccountManager.Protect` DPAPI hatasında `catch { return plain; }` ile düz metne geri dönüyor. `Socks5Config.ToJson()` global proxy parolasını doğrudan `Password` alanına, `LoginStrategyManager.ToJson()` varsayılan ikincil PIN'i doğrudan `SecondaryPasscode` alanına yazıyor. `Settings.json` depoda izlenen bir dosyadır.

**Etki:** Windows kullanıcı profili/DPAPI problemi, kopyalanan ayar dosyası, yanlışlıkla commit veya destek paketi paylaşımı halinde oyun, proxy ve PIN bilgilerinin açığa çıkması.

**Değişiklik:** Tek bir `SecretStore` katmanı oluşturun. DPAPI koruması başarısızsa kayıt işlemini reddedin ve kullanıcıya anlaşılır hata gösterin; hiçbir koşulda düz metne geri dönmeyin. Global proxy parolasını ve varsayılan PIN'i de aynı formatla koruyun. `Settings.json` yerine secrets içermeyen `Settings.example.json` izleyin; gerçek ayar dosyasını `.gitignore` kapsamına alın. Bellekteki parola alanlarını kullanım sonrası mümkün olduğunca kısa ömürlü tutun.

**Kabul testi:** Kaydedilen JSON içinde bilinen parola/PIN byte dizisi bulunmamalı; DPAPI hata simülasyonunda dosya güncellenmemeli ve mevcut güvenli kayıt korunmalı.

### P1-01 — Socket kısmi gönderim döngüsü kalan byte sayısını kullanmıyor

**Kanıt:** `Network/Proxy.cs` Gateway ve Agent çıkışında her döngüde `Socket.Send(buffer.Buffer, buffer.Offset, buffer.Size, ...)` çağrılıyor. İlk çağrı kısmi gönderirse `Offset` artıyor fakat `Size` aynı kalıyor. `Socket.Send` dönüş değeri gerçekten gönderilen byte sayısıdır; sonraki çağrının count değeri `Size - Offset` olmalıdır.

**Etki:** Yoğunluk veya küçük socket buffer altında dizi sınırı hatası, paket kuyruğunda kopma ya da geçersiz veri gönderimi. Normal yerel testlerde tam gönderim yaygın olduğu için hata gizli kalabilir.

**Değişiklik:** Ortak bir `SendAll(Socket, TransferBuffer)` metodu yazın; `remaining = Size - Offset` kullanın, `count == 0` durumunu bağlantı kapanması sayın. Gateway ve Agent kopyalarını aynı metoda taşıyın.

**Kabul testi:** Sahte sender her çağrıda 1–3 byte kabul ettiğinde tüm veri tam ve bir kez gönderilmeli; sıfır byte dönüşünde döngü sonsuza girmemeli.

### P1-02 — `xDictionary.SetKey` çakışmada iç yapıyı bozuyor

**Kanıt:** `Game/Objects/xDictionary.cs` içinde `SetKey(old, new)` çağrısında `new` zaten varsa enumerator listesindeki eski anahtar `new` ile değiştiriliyor ve sözlük değeri eziliyor. Hedef anahtar enumerator içinde zaten bulunduğu için yineleniyor. Yapılan küçük deneyde `a→one`, `b→two`, ardından `SetKey(a,b)` sonrası `Count=2` ve snapshot `one,one`; `RemoveKey(b)` sonrasında `Count=1` iken snapshot boş kaldı.

**Etki:** Sayaç, indeks ve snapshot uyuşmazlığı; entity/inventory benzeri yapılarda sessiz veri kaybı veya yanlış nesne seçimi. Şu anda depo içinde doğrudan çağrı bulunmaması riski erteler, fakat sınıfın public API'si kusurludur.

**Değişiklik:** Çakışma politikasını belirleyin: tercihen `TryRenameKey` hedef varsa `false` dönsün. Alternatif overwrite davranışında hedefin mevcut enumerator kaydını kaldırıp sıralamayı açıkça tanımlayın. İç tutarlılık invariant testleri ekleyin.

**Kabul testi:** Rename sonrası `Count`, snapshot ve `ContainsKey` her zaman aynı öğe kümesini göstermeli; hedef çakışması deterministik davranmalı.

### P1-03 — Oyun veri cache'i sunucu/profil değişiminde eski veri döndürebilir

**Kanıt:** `Game/DataManager.cs` statik cache anahtarları `model:<id>`, `item:<id>`, `skill:<id>` biçiminde. Anahtarda `SilkroadName`/veritabanı kimliği yok; cache yalnızca 4.000 öğeyi geçince topluca temizleniyor. `ConnectToDatabase` ve `DisconnectDatabase` cache'i temizlemiyor.

**Etki:** Aynı süreçte başka private server veya veri tabanına geçildiğinde aynı ID için önceki sunucunun model, item veya skill kaydı kullanılabilir. Paket parsing ve bot kararları yanlış tipe dayanabilir.

**Değişiklik:** Cache'i bağlantı bağlamının instance üyesi yapın veya anahtara veritabanının canonical path + sürümünü ekleyin. Başarılı connect öncesinde ve disconnect sırasında cache'i temizleyin. Bağlantı değişimini atomik hale getirin ve önceki `SQLDatabase` nesnesini dispose edin.

**Kabul testi:** Aynı ID'yi farklı değerlerle içeren iki test DB'si arasında geçişte ikinci sorgu mutlaka ikinci DB sonucunu vermeli.

### P1-04 — PK2 struct dönüşümünde native bellek sızıntısı ve bozuk girdide sınır riski var

**Kanıt:** `PK2ReaderAPI/Pk2Reader.cs:BufferToStruct` her çağrıda `Marshal.AllocHGlobal(buffer.Length)` kullanıyor ancak `Marshal.FreeHGlobal` çağırmıyor. Ayrıca kısa okunan buffer, hedef struct boyutundan küçük olsa bile `PtrToStructure` çalıştırılıyor. PK2 entry zincirleri ve dosya `Position/Size` alanları stream sınırına göre doğrulanmadan kullanılıyor.

**Etki:** Büyük PK2 dosyalarının taranmasında sürekli unmanaged bellek artışı; bozuk veya kötü hazırlanmış PK2 ile sınır dışı okuma, exception, uzun recursive zincir veya yüksek bellek tüketimi.

**Değişiklik:** `Marshal.SizeOf(returnStruct)` kadar veri şartı koyun; pointer'ı `try/finally` içinde serbest bırakın. Header doğrulaması, stream sınırı, maksimum klasör/entry/derinlik, zincir döngüsü tespiti ve file range kontrolü ekleyin.

**Kabul testi:** Aynı PK2'yi yüzlerce kez aç/kapat testinde private bytes sürekli büyümemeli; truncate edilmiş ve döngülü entry zincirleri kontrollü `InvalidDataException` üretmeli.

### P1-05 — Ayar debounce thread'i UI kontrollerini arka plandan okuyor

**Kanıt:** `Settings.QueueTrailingBotSave` ve `QueueTrailingCharSave`, `ThreadPool.QueueUserWorkItem` ile gecikmeli olarak `Save*Settings()` çağırıyor. Bu metotlar doğrudan `Window` üzerindeki textbox, checkbox ve listview özelliklerini okuyor. WinForms kontrolleri yalnızca oluşturuldukları UI thread'inden güvenle erişilebilir.

**Etki:** Debug ortamında cross-thread exception, Release ortamında yarış koşulu veya tutarsız JSON snapshot'ı; pencere kapanırken sessiz kayıt kaybı. Geniş `catch { }` blokları problemi görünmez hale getirebilir.

**Değişiklik:** UI thread'inde immutable bir settings DTO snapshot'ı alın; dosya yazımını arka planda bu DTO ile yapın. Daha basit çözüm olarak debounce için UI `Timer` kullanın. Kapanışta pending kaydı flush edin ve tamamlanmasını sınırlı süre bekleyin.

**Kabul testi:** `Control.CheckForIllegalCrossThreadCalls=true` ile hızlı ayar değişimi ve pencere kapatma testi exceptionsız çalışmalı; son kullanıcı değeri JSON'a yazılmalı.

### P1-06 — Navigasyon LRU cache'i eşzamanlı erişime açık

**Kanıt:** `NavigationManager.GetOrLoadRegion`, normal pathfinding sırasında ve `PreloadRegionAsync` içindeki `Task.Run` üzerinden çağrılabiliyor. Metot aynı `Dictionary` ve `LinkedList` yapılarını kilitsiz değiştiriyor.

**Etki:** Koleksiyon bozulması, `InvalidOperationException`, LRU sırasının kopması veya aynı bölgenin birden fazla kez pahalı biçimde yüklenmesi.

**Değişiklik:** Cache map/order erişimini tek lock altında tutun. Dosya okumayı lock dışında yapıp double-check ile ekleyin veya `ConcurrentDictionary<int, Lazy<Task<NavRegion>>>` yaklaşımı kullanın.

**Kabul testi:** Aynı ve farklı bölgeler için 50+ paralel preload/path isteği hata vermemeli; cache boyutu 8'i geçmemeli ve her bölge tek kez yüklenmeli.

### P1-07 — Native loader payload verisine güveniyor

**Kanıt:** `PayloadHelper.h` string uzunluğunu dosyadan okuyup negatif/üst sınır kontrolü olmadan `std::string::resize` yapıyor. `Library.cpp::LoadConfig` gateway adres sayısını da üst sınır koymadan döngüye alıyor. Okuma sonuçları ve stream durumu doğrulanmadan global hook ayarları etkinleştiriliyor.

**Etki:** Bozuk/geçersiz temp payload ile aşırı bellek ayırma, yarım ayar kullanma veya injected süreçte çökme.

**Değişiklik:** Payload'a magic, format version, toplam uzunluk ve checksum ekleyin. String, liste ve dosya boyutlarına katı limit koyun; her read sonucunu doğrulayın. Tam parse başarılı olmadan `g_Activated=true` yapmayın.

**Kabul testi:** Truncate, negatif uzunluk, çok büyük count ve yanlış checksum örneklerinin tamamı hook kurmadan reddedilmeli.

## 4. Orta öncelikli bulgular

### P2-01 — HTTP reklam kaynağı uzaktan kontrol edilen içerik ve URL açıyor

`App/Ads.cs`, `http://bit.ly/xBot-ads-check` adresinden doğrulanmamış metin indiriyor, uzaktaki görseli yüklüyor ve tıklamada gelen URL'yi `Process.Start` ile açıyor. HTTP bütünlük ve sunucu kimliği sağlamaz. Reklam özelliğini kaldırın veya sabit HTTPS origin, zaman aşımı, içerik boyutu, şema allowlist (`https`) ve güvenli CSV/JSON parser kullanın. Redirect sonrası nihai origin'i de doğrulayın.

### P2-02 — Log ve paket analizörü hassas veri sızdırabilir

Packet analyzer tüm paket byte'larını UI/log akışına aktarabiliyor; `ModernLogger` paket özetlerini `session_debug.log` içine yazıyor. Giriş, özel mesaj, PIN veya kişisel veri içeren opcode'lar için redaction politikası yok. Hassas opcode denylist'i, log rotasyonu/boyut sınırı ve kullanıcıya açık “hassas paketleri dahil et” seçeneği ekleyin. Varsayılan kapalı kalsın.

### P2-03 — Çok sayıda boş `catch` gerçek hataları gizliyor

Network, parser, UI, navigasyon ve PK2 katmanlarında çok sayıda `catch { }` veya yalnızca `return` vardır. Beklenen kapanma hatalarını ayrı exception türleriyle ele alın; beklenmeyen hatalarda kategori, opcode/işlem, bağlantı durumu ve exception chain kaydedin. Loglarda parola ve ham kimlik bilgisi bulunmamasını merkezi olarak sağlayın.

### P2-04 — Thread ve timer sahipliği dağınık

Proxy, bot, simya, kayıt, PK2 extraction ve reklam akışları doğrudan `Thread`, `Thread.Sleep` ve `Interrupt` kullanıyor. Bazı thread'ler background değil, bazıları join edilmiyor; kapanışta yalnızca belirli bileşenler durduruluyor. Her uzun ömürlü iş için tek sahip, `CancellationToken`, sınırlı bekleme ve idempotent `Stop/Dispose` sözleşmesi kurun.

### P2-05 — `SQLDatabase` ortak command nesnesi tam kilitli değil

`Prepare`, `Bind`, `ExecuteQuery` ve bazı query yolları aynı `q` alanını kullanıyor fakat bütün işlem dizisi tek lock altında değil. Farklı thread'ler parameter/CommandText durumunu karıştırabilir. Ortak mutable command'i kaldırıp her sorguda yerel `SQLiteCommand` oluşturun; transaction'ı nesne olarak yönetin.

### P2-06 — PK2/DDJ geçici dosya temizliği exception-safe değil

`DDSReader` temp dosyayı ancak başarılı yüklemeden sonra siliyor. Decode/yükleme hatasında dosya kalır. `try/finally` kullanın, DDJ için minimum 20 byte kontrolü ekleyin ve mümkünse kütüphanenin memory decode API'sini kullanın.

### P2-07 — Çalışma dizinine bağlı yollar davranışı belirsizleştiriyor

`Settings.json`, `Config`, `session_debug.log`, PK2 dizinleri ve bazı kaynaklar current working directory üzerinden çözülüyor; bazı navigasyon/script yolları ise exe dizinini de arıyor. Uygulama farklı bir klasörden başlatıldığında veriler farklı yere yazılabilir. Tek bir `AppPaths` sınıfında executable, user-data, cache, log ve read-only asset köklerini açıkça tanımlayın.

### P2-08 — Bağımlılık ve çıktı yönetimi tekrarlanabilir değil

Ana proje eski `packages.config` kullanıyor; bazı referanslar `bin/Release/*.dll` üzerinden koşullu ekleniyor. Native proje `v145` toolset'e sabitlenmiş. Bu yapı temiz makinede restore/build'i zorlaştırır ve hangi binary'nin kaynakla eşleştiğini belirsiz bırakır. NuGet `PackageReference`, kilit dosyası, doğrulanmış native asset paketi ve CI üzerinde temiz checkout build kurun. Üçüncü parti lisans ve sürüm envanteri üretin.

### P2-09 — Global mutable durum oturum izolasyonunu zayıflatıyor

`InfoManager`, `DataManager` ve birçok manager statik state taşıyor. Disconnect/teleport/character switch sırasında her alanın elle temizlenmesi gerekiyor; simya örneğinde bu sözleşme kaçmış. `GameSession` nesnesi oluşturup entity store, managers, DB context ve cancellation scope'u bu session'a bağlayın.

### P2-10 — Büyük sınıflar değişiklik riskini artırıyor

`Window`, `PacketParser`, `InfoManager` ve `Bot.IA` çok farklı sorumlulukları birleştiriyor. Paket parser'ı opcode ailelerine; UI'ı view-model/binding katmanlarına; botu combat, movement, inventory, town state machine'lerine ayırın. İlk hedef yeniden yazım değil, mevcut davranışı koruyan seam/interface oluşturmaktır.

## 5. Düşük öncelikli ve kalite geliştirmeleri

- `.gitignore` dosyasının yorumları bozuk encoding ile görünüyor; UTF-8 olarak normalize edin.
- `throw ex;` kullanılan yerlerde stack trace kaybolur; `throw;` kullanın.
- Saat formatlarında `hh` yerine teşhis logları için `HH` kullanın; ISO-8601 timestamp ve UTC offset ekleyin.
- `Environment.TickCount` tabanlı debounce hesabını taşma güvenli `TickCount64` veya monotonic timer ile değiştirin.
- Public API'lerde `isRunning`, `isLoaded` gibi adları .NET isimlendirmesine (`IsRunning`) taşıyın; bu işi davranış düzeltmelerinden ayrı yapın.
- Türkçe/İngilizce sabit metinleri `LocalizationManager` kaynaklarına taşıyın.
- README'deki “kullanıcı adı ve şifre kalıcı saklanmaz” ifadesi hesap yöneticisinin parola kaydetmesiyle çelişiyor; gerçek davranışı açıkça belgeleyin.
- Depoda `LICENSE` görünmüyor. Kodun kökeni ve üçüncü parti bileşenler nedeniyle dağıtım öncesi lisans dosyası ve NOTICE ekleyin.
- Native build artıkları depo ağacında görünmese bile `Win32/Debug` ve `Win32/Release` temiz checkout/CI çıktısından ayrılmalı; kaynak dağıtım paketine girmemeli.

## 6. Test ve doğrulama boşlukları

Mevcut `ProtectionScenarios` testi 322 adet karar/politika/koleksiyon/secret senaryosunu kapsıyor. Bu iyi bir temel, ancak gerçek arızaların çoğu sınır katmanlarındadır. Aşağıdaki test projeleri eklenmelidir:

| Test paketi | Kapsam |
|---|---|
| `NetworkScenarios` | Kısmi send/receive, 0 byte, timeout, socket kapanışı, Gateway→Agent geçişi, SOCKS5 parçalı cevaplar |
| `PacketCorpusTests` | Bilinen paket örnekleri, truncate paketler, bilinmeyen varyantlar, remaining-read invariant'ı |
| `PersistenceTests` | Atomic save, debounce, kapanış flush, bozuk JSON, DPAPI hata yolu, eski ayar migrasyonu |
| `SessionIsolationTests` | Server/DB değişimi, disconnect/relogin, statik manager reset sözleşmesi |
| `AlchemyStateMachineTests` | Gecikmiş, kayıp, yinelenen sonuç; slot değişimi; disconnect; hedefe ulaşma |
| `NavigationTests` | Paralel preload, cache limiti, ulaşılamayan hedef, Z/edge flag, gerçek rota fixture'ları |
| `Pk2FuzzTests` | Truncate header, taşan size/position, döngülü chain, büyük count, bozuk DDJ |
| `NativePayloadTests` | Magic/version/checksum, negatif/çok büyük uzunluk, yarım payload |

CI sırası temiz Windows runner üzerinde restore → Release/x86 build → senaryo testleri → fixture/fuzz smoke → artifact hash/SBOM olmalıdır. Native injection ve gerçek oyun bağlantısı ayrı, manuel onaylı entegrasyon pipeline'ında tutulmalıdır.

## 7. Önerilen hedef mimari

### Oturum sınırı

`GameSession` aşağıdaki bileşenlerin ömrünü yönetmeli:

- Gateway/Agent transport ve cancellation token
- `GameStateStore` (karakter, entity, party, guild, inventory)
- `GameDataContext` (seçili SQLite ve cache namespace)
- Bot state machine ve özellik yöneticileri
- Navigasyon cache'i
- Oturum log correlation id

Disconnect tek bir `DisposeAsync/StopAsync` yolundan geçmeli. Yeni bağlantı eski session tamamen kapanmadan başlamamalıdır.

### Protokol sınırı

Her opcode ailesi için küçük handler sınıfları kullanın: login, character, inventory, combat, social, stall, alchemy. Handler girdi olarak sınır kontrollü reader, çıktı olarak domain event üretmeli. UI veya global state doğrudan parser içinde değiştirilmemelidir.

### Ayar sınırı

UI kontrollerinden ayar okumayı manager'lardan ayırın:

```text
WinForms controls → SettingsViewModel/DTO → validation → atomic repository
                                              └─ secret fields → DPAPI SecretStore
```

JSON'a schema version ekleyin. Her sürüm geçişi için explicit migration yazın; bozuk dosyada mevcut dosyayı `.corrupt-<timestamp>` adıyla koruyup güvenli varsayılan açın.

### Bot davranışı

Uzun `if/timer` zincirleri yerine açık durumlar kullanın:

```text
Disconnected → LoggingIn → CharacterSelected → InTown/Training
Training → Fighting / Looting / Recovering / Returning
Returning → Traveling / TownServices → Training
```

Her yan özellik, bir komut göndermeden önce session id, in-flight action ve cooldown kontrolü yapmalıdır.

## 8. Uygulanabilir geliştirme planı

### Faz 0 — Güvenli davranış (1–3 gün)

- Otomatik simyada tek scheduler + in-flight cevap kapısı + disconnect stop.
- DPAPI plaintext fallback'i kaldırma; proxy/PIN secret migrasyonu.
- `Settings.json` için secrets temizliği ve `.gitignore` düzeni.
- Ağ `SendAll` düzeltmesi ve test double ile kısmi gönderim testi.

**Çıkış ölçütü:** Eşya tüketen bir işlem cevap beklemeden tekrarlanamaz; disk üzerinde açık parola/PIN kalmaz; partial send testi geçer.

### Faz 1 — Veri bütünlüğü ve yaşam döngüsü (3–5 gün)

- `xDictionary.SetKey` sözleşmesi ve invariant testleri.
- DB/profile cache izolasyonu.
- Settings DTO snapshot ve kapanış flush.
- Alchemy, proxy, recording, timers için ortak stop/dispose sözleşmesi.
- Navigation LRU eşzamanlılık düzeltmesi.

**Çıkış ölçütü:** Disconnect/relogin stress testi ve iki DB arasında geçiş testi 100 tur hatasız çalışır.

### Faz 2 — Girdi sertleştirme ve gözlemlenebilirlik (4–7 gün)

- PK2 reader bellek/sınır düzeltmeleri ve corpus testleri.
- Native payload version/checksum/limitleri.
- Packet reader için merkezi `TryRead/RequireRemaining` yardımcıları.
- Log redaction, rotasyon ve correlation id.
- HTTP reklam akışının kaldırılması veya HTTPS allowlist ile yeniden yazılması.

**Çıkış ölçütü:** Bozuk input corpus'u kontrollü hata üretir; uzun PK2 taramasında native bellek trendi sabit kalır; log taramasında secret bulunmaz.

### Faz 3 — Build ve mimari ayrıştırma (1–3 hafta)

- Tekrarlanabilir dependency restore ve temiz Windows CI.
- `PacketParser`ı alan handler'larına bölme.
- `GameSession` ve instance tabanlı state/cache geçişi.
- `Window` içindeki iş mantığını view-model/controller katmanına çıkarma.
- NavMesh ve paket fixture entegrasyon testleri.

**Çıkış ölçütü:** Temiz checkout tek komutla build/test olur; yeni opcode veya bot özelliği `Window`, `InfoManager` ve dev parser dosyalarını birlikte değiştirmeden eklenebilir.

## 9. Dosya bazında değişiklik listesi

| Dosya/bölge | Yapılacak iş | Öncelik |
|---|---|---:|
| `App/AlchemyManager.cs` | State machine, tek scheduler, item identity, timeout | P0 |
| `App/Bot/Bot.IA.cs` | İkinci simya tick kaynağını kaldır | P0 |
| `App/Bot/Bot.Events.cs` | Disconnect'te simya ve tüm session özelliklerini durdur | P0 |
| `App/AccountManager.cs` | Plaintext fallback'i kaldır, merkezi secret store | P0 |
| `App/LoginStrategyManager.cs` | PIN'i korumalı sakla/migrate et | P0 |
| `Network/Proxy.cs` | Global proxy secret koruması; `SendAll`; lifecycle | P0/P1 |
| `Game/Objects/xDictionary.cs` | Çakışma güvenli rename ve invariant | P1 |
| `Game/DataManager.cs` | DB'ye bağlı cache, connect/disconnect dispose | P1 |
| `App/Settings.cs` | UI-thread snapshot, debounce ve shutdown flush | P1 |
| `Game/Navigation/NavigationManager.cs` | Thread-safe LRU, tek yükleme | P1 |
| `PK2ReaderAPI/Pk2Reader.cs` | `FreeHGlobal`, range/chain/depth doğrulaması | P1 |
| `xBot.Loader.Library/PayloadHelper.h` | Length limit ve checked read | P1 |
| `xBot.Loader.Library/Library.cpp` | Payload doğrulama, yalnızca başarılı parse sonrası activate | P1 |
| `App/Ads.cs` | Kaldır veya HTTPS + origin/schema/size doğrulaması | P2 |
| `App/SQLDatabase.cs` | Sorgu başına command, transaction API | P2 |
| `App/Theme/ModernLogger.cs` | Redaction, rotasyon, retention | P2 |
| `Game/PacketParser.cs` | Handler ayrıştırması ve boundary checks | P2 |
| `App/Window*.cs` | Binding/controller ayrımı | P2 |
| `xBot.csproj`, `packages.config` | Tekrarlanabilir package/build yapısı | P2 |
| `README.md`, `.gitignore`, `LICENSE` | Gerçek credential davranışı, encoding, lisans | P2/P3 |

## 10. Doğrulama kayıtları

Bu inceleme sırasında aşağıdaki kontroller çalıştırıldı:

- `msbuild xBot.sln /t:Rebuild /p:Configuration=Release /p:Platform=x86` — başarılı; native loader ve WinForms uygulaması üretildi.
- `dotnet run --project tests/ProtectionScenarios/ProtectionScenarios.csproj` — başlangıçta **317/317 başarılı**; düzeltme sonrasında 3 koleksiyon regresyon testi eklendi.
- `navdata/*.dat` yapısal taraması — **77/77 açıldı**, toplam **1.062.557** nokta, temel record boyutunda **0 geçersiz dosya**.
- `xDictionary.SetKey` hedefli runtime deneyi — çakışma halinde iç tutarsızlık yeniden üretildi.
- Secret persistence, socket send, PK2 marshal ve navigation cache akışları kaynak seviyesinde karşılaştırmalı incelendi.

Derleme ve mevcut testlerin geçmesi, rapordaki concurrency, güvenlik ve yaşam döngüsü sorunlarını geçersiz kılmaz; mevcut test paketi bu sınırları kapsamamaktadır.

## 11. İlk sprint için net iş listesi

1. `AlchemyManager` için `AwaitingResult` alanı ve request timeout ekleyin; `Bot.IA` içindeki ikinci tick'i kaldırın; disconnect'te `Stop()` çağırın.
2. `SecretStore.ProtectOrThrow/Unprotect` oluşturun; üç secret kaynağını migrate edin ve düz metin fallback'i yasaklayın.
3. Gateway/Agent gönderim kodunu tek `SendAll` yardımcı metoduna alın.
4. `xDictionary` rename testlerini yazıp çakışmayı reddedin.
5. `DataManager` cache'ini DB context değişiminde sıfırlayın ve DB nesnesini dispose edin.
6. Ayar kaydı için UI thread'inde DTO snapshot alın; app kapanışında pending kaydı flush edin.
7. Navigation LRU erişimini kilitleyin ve paralel preload testi ekleyin.
8. `BufferToStruct` native belleğini `finally` ile serbest bırakın; PK2 range kontrollerini ekleyin.

Bu sekiz iş tamamlandığında projenin en yüksek veri kaybı, credential sızıntısı, bağlantı kopması ve session karışması riskleri büyük ölçüde kapanmış olur.
