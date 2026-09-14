# Kod incelemesi düzeltmeleri — 14 Eylül 2026

Bu rapor `CodeAudit-2026-09-14.md` bulgularının uygulama sonucudur. Önceki ferry/cave çalışması korunmuştur. Kullanıcı tercihine göre kiting kaldırılmış; Lagtastic, teleport, lure ve diğer görünür seçenekler korunarak çalışma zamanı akışlarına bağlanmıştır.

## Uygulanan değişiklikler

| İnceleme alanı | Son davranış |
|---|---|
| Gizli panic dönüşü | Kullanıcı koruma ayarlarını atlayan ikinci dönüş yolu ve gizli checkbox kaldırıldı. Dönüş kararı merkezi koruma motorundan geçiyor. |
| Potion güvenliği | HP/MP/Vigor/Universal/Purification karakter ve envanter yokken çıkıyor; karakter referansı sabitleniyor. Gönderim başarısızsa cooldown başlamıyor. |
| Vigor | HP ve MP kutuları kendi eşiklerini ve kendi kullanıcı gecikmelerini kullanıyor; ortak eşyanın 15 saniyelik alt bekleme sınırı korunuyor. |
| Return Scroll | Yalnız uygun dönüş eşyası kullanılıyor; yerel paket gönderiminin başarısızlığı çağırana `false` dönüyor. |
| Saatli dönüş | Sebebe özel durma isteği genel şehirde dur ayarıyla ezilmiyor. Bekleyen dönüş sırasında tekrar paket/saldırı/lure başlatılmıyor. Dönüşten sonra bağlantıyı kesme süresi bağlandı. |
| Hedef sırası | Listedeki daha yüksek öncelikli uygun mob saldırı sınırında devralabiliyor. Yeni seçim de aynı sırayı uyguluyor; önceki hedefi tekrar seçerek dönüp durmuyor. |
| DOT | Bleed/Poisoning/Burn kullanılıyor; donma DOT sayılmıyor. İlk gözlemden itibaren ayarlanan saniye bekleniyor; uygun başka mob yoksa hedef bırakılmıyor. Bırakılan hedef geçici olarak erteleniyor. |
| Alt skill listesi | Açıkken mob türüne göre daha alt yapılandırılmış listeler deneniyor; kapalıyken boş özel liste otomatik olarak General'a düşmüyor. |
| Lagtastic | Cevapsız kalan saldırıda karakter eylem kanalının boşalması beklenerek tek ek deneme yapılıyor. Açık sunucu reddinde ek deneme yapılmıyor. |
| Teleport skill | Öğrenilmiş, kullanılabilir skill ve veri tabanındaki mesafe kullanılıyor. Aynı alan, training sınırı, navigasyon yolu ve duvarı kestirmeme kontrollerinden sonra konum hedefli cast gönderiliyor; hareket teyidi gelmezse normal yaklaşma sürüyor. |
| Lure | Skill/script seçimi, dış/yarı/merkez beklemeleri, rastgele ve spawn yönüne yürüyüş, smart yol, mob limiti ve parti durdurma koşulları bağlandı. Normal combat uygunluk filtresi kullanılıyor. No Attack modunda da çalışıyor. İptal edilen script tamamlanmış sayılmıyor; merkez dönüşü başarısızken bitiş buff'ı uygulanmıyor. |
| Parti desteği | Wildcard buff uygun oyuncuları sırayla dolaşıyor, mevcut buff'ları ve kısa tekrar aralığını dikkate alıyor. Parti dışı regex diriltme partisizken çalışıyor. Merkeze dönme yalnız buff sonrasında uygulanıyor ve lure yürüyüşünü kesmiyor. |
| Diğer ayarlar | Şehir öncesi otomatik yapılandırma, ortak pickup filtresi, şehir scriptine devam, scriptte berserk, normal Energy of Life ve Easter Egg NPC arama/etkileşim yolları bağlandı. |
| Ortak filtre | Pickup/Pet seçimleri paylaşılırken karaktere özel satış ve depolama tercihleri korunuyor. Kayıt hatası karakter ayarlarını kaydetme akışını kesmiyor. |
| Temizlik | Kiting metodu/çağrısı/Designer alanları, ölü Avoid puanı, kullanılmayan eski combat kartları ve çağrısız yardımcılar kaldırıldı. Diğer ekranların kullandığı ortak UI yardımcıları korundu. |

Lure script kutusu ve diğer lure kontrolleri ayar kaydına bağlandı. “Her script komutundan sonra buff” etiketi gerçek davranışı açıklıyor. DOT gecikmesi arayüzde açıkça saniye olarak gösteriliyor. Yeni çalışma zamanı yardımcıları `Bot.AdvancedCombat.cs`, `CombatRotationState.cs` ve `SharedPickFilterPolicy.cs` dosyalarına ayrıldı.

## Doğrulama

- `MSBuild xBot/xBot.csproj /p:Configuration=Release /p:Platform=x86 /verbosity:minimal /nologo`: başarılı.
- `dotnet run --project tests/ProtectionScenarios/ProtectionScenarios.csproj -c Release`: başarılı; saatli dönüşün bağımsız durma kararı eklendi.
- `dotnet run --project tests/BotEngineScenarios/BotEngineScenarios.csproj -c Release`: 59 kontrol başarılı. DOT zamanlaması/yeniden seçim, ortak filtre ve mevcut eylem/loot senaryoları dahil.
- `dotnet run --project tests/FerryNavigationScenarios/FerryNavigationScenarios.csproj -c Release`: 76 kontrol başarılı; gerçek yerel DB ile cave rota ve tetikleyici senaryoları dahil.
- 32-bit Windows PowerShell, STA üzerinden `tests/TrainingCombatScenarios.ps1`, son Release EXE ile: 134 kontrol başarılı. Yeni `CombatAuditIntegration.cs` gerçek uygulama tipleri ve bellekte paket kuyruğu kullanıyor; sunucu bağlantısı açmıyor. Null callback, Vigor kutuları, başarısız gönderim, alt skill seçimi, buff hedefleri, partisiz diriltme ve script iptal/exception senaryoları dahil.
- `git diff --check`: başarılı.

## Doğrulamanın sınırları

Canlı oyun/sunucu testi yapılmadı. Testte paketin kuyruğa alınması sunucunun eylemi kabul ettiğinin kanıtı değildir. Özellikle konum hedefli teleport paketinin sunucu sürümüne uyumu, fiziksel lure yürüyüşü ve etkinlik NPC davranışı canlı ortamda doğrulanmalıdır. Easter Egg yalnızca sunucunun tek bir konuşma seçeneği sunduğu durumda otomatik etkileşir; belirsiz menüde işlem yapmayıp log yazar. Otomatik yapılandırma mevcut build uygulama mekanizmasını şehir öncesinde çağırır; yeni bir mastery öğrenme sistemi eklemez.

Bu çalışma belirtilen inceleme bulgularını düzeltir; bütün projede başka ölü kod veya hata bulunmadığını iddia etmez.
