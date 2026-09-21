# DPI ve WinForms Designer incelemesi — 2026-09-20

## DOĞRULANAN SORUNLAR

- Yeni bir Windows DPI veya Visual Studio Designer arızası doğrulanmadı. `AutoScaleMode.None` hata olarak sınıflandırılmadı.
- Hesapla doğrulanan ekran sınırı: mevcut 996×539 minimum dış pencere, 1024×768 fiziksel ekranın %125 ölçeklemede 819×614, %150 ölçeklemede 682×512 mantıksal alanına sığmaz. Bu, gerçek monitör testi değildir; görev çubuğu alanı hesaba katılmadan bile oluşan boyut kısıtıdır.
- Kapsam dışı mevcut görsel bulgu: Hesap Bilgileri ekranındaki Türkçe `PIN / Güvenlik Kodu` etiketi %100 önizlemede de metin kutusunun altında kesiliyor. %125/%150 raster büyütme aynı kesilmeyi büyütüyor; yeni bir DPI regresyonu olarak sınıflandırılmadı. Kaynak: `Window.PhBotInner5.cs`, `lblCredPin` x=195, giriş x=280.

## DÜZELTİLEN SORUNLAR

Uygulamada yeni düzeltme yapılmadı. Kanıtlanmamış Designer riskleri için constructor koruması, erişim belirleyicisi değişikliği veya AutoScaleMode değişikliği eklenmedi. Düşük çözünürlük kısıtı için minimum boyutu küçültmek mevcut sabit koordinatlı içeriği kesebileceğinden uygulanmadı.

Önceki sidebar kaydırma sınırı, klavye odağının görünürlüğü ve Komut Oluştur kaydırma düzeltmeleri korundu.

## DPI TEST SONUÇLARI

Test hostu: 32-bit Windows PowerShell, STA; yüklenen Debug/x86 xBot assembly; izole çalışma klasörü; `Window_Load` kaldırılarak giriş/bağlantı akışı başlatılmadı. Test hostu penceresinde `GetDpiForWindow=96`, DPI awareness=0, en yakın monitör ölçeği=%100 ölçüldü. Bunlar xBot.exe sürecinin doğrudan ölçümü değildir.

Windows DPI ayarı değiştirilmedi. %125/%150 için fiziksel monitör geçişi, WM_DPICHANGED, DWM bileşimi veya gerçek yüksek-DPI font rasterizasyonu test edilmedi.

Simülasyon yöntemi: 1440×880 fiziksel istemci alanı bütçesi ölçeğe bölünerek mantıksal viewport küçültüldü. Çalışan formun DrawToBitmap çıktısı 1/1.25/1.5 katsayısıyla raster olarak büyütüldü. Kontrollerin Scale/Font/AutoScaleMode özellikleri değiştirilmedi. Dış pencere çerçevesi istemci alanı bütçesine dahil değildir.

| Ölçek | Mantıksal istemci alanı | Sekme / navigasyon | Geometri ve scroll |
|---|---|---|---|
| %100 | 1440×880 | 86 geçiş geçti | Sidebar/log/aksiyon paneli çakışma kontrolleri geçti; saldırı panelinde görünür kontrol sınır aşımı yok |
| %125 simülasyon | 1152×704 | 86 geçiş geçti | Aynı kontroller geçti; Komut Oluştur ilk/son düğmesine kaydırma geçti |
| %150 simülasyon | 960×586 | 86 geçiş geçti | Aynı kontroller geçti; Komut Oluştur ilk/son düğmesine kaydırma geçti |

Önceki regresyonlar ayrıca geçti: sidebar aşağı kaydır/büyüt, son navigasyon düğmesine klavye odağı, 760×580 / 820×620 / 1000×800 komut penceresi, resize ve navigasyon. Bağlı HP kontrolünün aynı nesne olduğu ReferenceEquals ile kontrol edildi.

Saldırı, Hesap Bilgileri ve Komut Oluştur raster görüntüleri üretildi; seçilmiş %100/%125/%150 örnekleri görsel olarak incelendi. Saldırı örneklerinde sidebar/ikon hizası ve aksiyon metinlerinde yeni kesilme görülmedi. Bu, tüm sekmelerde tüm metinlerin piksel düzeyinde doğrulandığı anlamına gelmez. Sekme testleri görünürlük testidir; kapsamlı metin sığma veya tüm kardeş kontrollerin overlap testi değildir.

Kod riskleri:

- `Window.Designer.cs`: `AutoScaleMode.None`.
- `app.manifest`: dpiAware bildirimi yorum içinde; `App.config` ve Program.cs içinde aktif DPI opt-in görülmedi.
- `Window.ReferenceLayout.cs`: 182px sidebar, 27px satır adımı, 212×108 aksiyon bölgesi ve sabit 996×539 minimum pencere.
- `Window.PhBotInner.cs`: saldırı/buff kolonlarında sabit 50/232/275px koordinatlar; AutoScroll kapalı alanlar.
- `Window.PhBotInner5.cs`: giriş alanlarında sabit kolonlar.
- `ScriptCreatorForm.cs`: sabit iki sütun; mevcut minimum grup yüksekliği ve form scroll davranışı korundu.

Microsoft belgesine göre DPI farkındalığı olmayan uygulamalarda Windows bitmap büyütme kullanabilir; bu geometrinin birlikte büyümesini sağlar ancak bulanıklık oluşturabilir. Mevcut manifest için bu, kaynak incelemesinden yapılan çıkarımdır; uyumluluk override'ları ve gerçek xBot.exe süreci test edilmedi. Kaynak: [Microsoft — High DPI Desktop Application Development](https://learn.microsoft.com/en-us/windows/win32/hidpi/high-dpi-desktop-application-development-on-windows).

## DESIGNER TEST SONUÇLARI

**Gerçek Visual Studio Designer açılışı doğrulanmadı.** Visual Studio 18 Insiders kurulu; Computer Use `list_apps` ve `list_windows` çağrıları `native pipe is unavailable ... os error 2` hatası verdi. Açma/serialize/kaydetme turu yapılamadı.

Birbirinden ayrı yapılan kontroller:

- Temiz solution derlemesi: dört formun Designer kodu derlendi.
- Kaynak/reflection/resource kontrolü: Window 596, About 11, Ads 10, Pk2Extractor 16 başlatılan üye bulundu. InitializeComponent metotları mevcut. Designer kaynaklarındaki beş doğrudan GetObject anahtarı null olmadan yüklendi; ImageList/Image/Icon nesneleri çözüldü.
- Bağımsız .NET DesignSurface/IDesignerHost testi: xRichTextBox, xProgressBar, xListView, xMap, xMapControl oluşturuldu; Site.DesignMode=true ve ilgili .NET kontrol designer'ı mevcut. Bu test Visual Studio kaynak yükleyicisini çalıştırmaz.
- InitializeComponent içinde yeni runtime-only kod yok; bu aşamada hiçbir Designer dosyası değiştirilmedi. Mevcut timer Dispose çağrıları InitializeComponent dışında ve null kontrollü; taşınmadı.
- Constructor incelemesi: Window constructor'ı runtime UI hazırlığı, font yükleme, singleton yayını ve timer başlatan InitializeValues yolunu içeriyor; tasarım-zamanı koruması yok. About/Ads owner parametresi, Pk2Extractor path parametreleri kullanıyor. Bunlar yalnızca gerçek Designer yükleme yoluyla değerlendirilebilecek risklerdir; constructor varlığı/erişimi tek başına Designer arızası ilan edilmedi.
- Custom xMap constructor'ı harita katmanı/tile hazırlığı yapıyor. İzole, harita verisi olmayan tasarım-hostu testinde hata oluşmadı; gerçek veriyle VS testi yapılmadı.

## DEĞİŞEN DOSYALAR

- `tests/ReferenceLayoutPreview.ps1`: isteğe bağlı `-AuditDpiDesigner` anahtarı ve gerçek kontrol kimliği assertion'ı.
- `tests/DpiDesignerChecks.ps1`: salt okunur DPI ölçümü, viewport/raster simülasyonu, resource/üye kontrolleri ve bağımsız tasarım-hostu testi.
- `tests/DpiDesignerReport.md`: bu rapor.

Bu aşamanın başlangıcındaki hash listesiyle karşılaştırma: xBot altındaki C#, csproj, resx, config ve manifest dosyalarında **0 değişiklik**. Önceki düzenlemeler, event bağlantıları, kontrol adları, backend ve AutoScaleMode korundu. DevExpress paketi/bağımlılığı eklenmedi; daha önce okunan layout skill'i yalnızca referans olarak kullanıldı.

## BUILD SONUCU

`msbuild xBot.sln /t:Rebuild /p:Configuration=Debug /p:Platform=x86 /v:minimal /nologo`

**0 hata, 0 uyarı.** C++ loader ve C# uygulaması temizden derlendi. Log: `tests/bin/dpi-designer/build.log`.

Tekrar çalıştırma (32-bit PowerShell):

```powershell
& C:/Windows/SysWOW64/WindowsPowerShell/v1.0/powershell.exe -NoProfile -STA -File tests/ReferenceLayoutPreview.ps1 -AssemblyPath xBot/bin/x86/Debug/xBot.exe -OutputDirectory tests/bin/dpi-designer/preview -AuditDpiDesigner
```

Test tamamlandı, çıkış kodu 0. Ayrıntılı ölçüm çıktısı: `tests/bin/dpi-designer/preview/dpi-designer-results.txt`. Görüntüler aynı klasörde `Simulated-*.png`; bunlar üretilen/ignore edilen test artefaktlarıdır.

## KALAN RİSKLER

Gerçek Windows %125/%150 testi, monitörler arası taşıma, xBot.exe uyumluluk override'ları, tüm metinlerin tüm dillerde sığması ve gerçek Visual Studio Designer aç/serialize/kaydet turu doğrulanmadı. AutoScaleMode veya DPI farkındalığı değişikliği bu kanıtlar olmadan yapılmamalı; mevcut sabit koordinatları kısmen ölçeklemek yeni çakışmalar yaratabilir.
