# Kod incelemesi — 14 Eylül 2026

İnceleme mevcut çalışma ağacına uygulanmıştır; önceki ferry/cave değişiklikleri korunmuştur. Uygulama kodu değiştirilmemiştir. `xBot` altındaki 187 C# dosyasında sembol/referans taraması yapılmış; aday bulgularda UI olayları, JSON, yönlendiren özellikler ve çalışma zamanı çağrıları ayrıca okunmuştur. Bu sayı, her dosyanın her satırının denetlendiği anlamına gelmez. Derleme veya canlı oyun testi bu inceleme kapsamında yapılmamıştır. Aşağıdaki sonuçlar kaynak kodundan doğrulanan davranışlardır; sunucuda gözlenen olaylar olarak sunulmamaktadır.

Dosya yolları depo köküne göredir; satırlar inceleme anındaki çalışma ağacına aittir. P1: önce düzeltilmeli; P2: işlev/ayar tutarsızlığı; P3: temizlik.

## 1. P1 — Gizli acil dönüş kullanıcı tercihlerini atlıyor ve yanlış eşya seçebiliyor

- `xBot/App/Window.CustomTabs.cs:2021`: eski combat grupları gizleniyor; 2031'de `Combat_cbxPanicEscape.Checked = true` zorlanıyor. Bu akış 271'deki `RemoveCombatTabWidgets()` çağrısından erişilebilir.
- `xBot/App/Bot/Bot.IA.cs:903`: bu gizli kutu açıkken acil dönüş çalışıyor; kutu null olduğunda da çalışıyor.
- `Bot.IA.cs:2522`: yalnızca `(3,1,1)` HP eşyası aranıyor; Vigor `(3,1,3)` ve kullanılabilir iyileştirme skilleri değerlendirilmiyor.
- `Bot.IA.cs:2529`: dönüş için `(3,3,1)` yanında `(3,3,2)` ve `(3,3,3)` de kabul ediliyor. Aynı dosyanın 3691'inde `(3,3,2)` açıkça binek çağırmak için kullanılıyor. Bu nedenle dönüş tomarı yokken binek eşyası seçilip dönüş yapıldığı varsayılabilir. `(3,3,3)` grubunun tamamının dönüş eşyası olduğuna dair doğrulama da yok.
- Bu yol `ProtectionManager.UseReturnScrolls` ve merkezi dönüş kararını kontrol etmiyor; normal koruma kontrolünden önce çalışıyor.

Örnek: HP %20, HP potu yok, Vigor ve binek eşyası var. Kullanıcı merkezi dönüş seçeneğini kapatsa bile gizli acil dönüş devreye girebilir ve binek eşyası gönderebilir.

Öneri: acil dönüşü merkezi koruma kararına taşı; yalnızca doğrulanmış dönüş eşyaları kullan. Vigor/skill varlığı tek başına yeterli sayılmamalı, gerçekten kullanılabilirlik de değerlendirilmelidir.

## 2. P1 — Vigor HP/MP kutuları bağımsız çalışmıyor

`xBot/App/Bot/Bot.Checks.cs:493` iki kutuyu OR ile birleştiriyor. Sonrasında HP dalı (504) HP kutusunu, MP dalı (519) MP kutusunu tekrar kontrol etmiyor.

Örnek: yalnızca MP Vigor açık; MP yüksek, HP düşük. HP eşiği yine Vigor kullandırır. Yalnızca HP açıkken HP yüksek/MP düşük olması da ters yönde aynı sorunu üretir. Ayrıca 495'te bekleme değeri daima HP Vigor kontrolünden alınıyor.

Öneri: her eşik dalını kendi kutusuyla koşullandır; ortak eşyanın bekleme politikasını açıkça belirle.

## 3. P2 — Başarısız eşya kullanımı başarı sayılıyor

- `xBot/Game/PacketBuilder.cs:601-614,631-634`: `UseItem` karakter/envanter değişimi, sıfır miktar, throttle veya exception nedeniyle `false` dönebilir.
- `xBot/App/Bot/Bot.cs:376-377`: `UseReturnScroll` bu sonucu atıp `true` döndürüyor.
- `xBot/App/Bot/Bot.IA.cs:2533-2535`: acil dönüş aynı şekilde sonuçtan bağımsız bekliyor ve başarı bildiriyor.
- `xBot/App/Bot/Bot.Checks.cs:423,467,509,524`: HP/MP/Vigor kullanımının sonucu kontrol edilmeden timer başlatılıyor. Vigor'da bu, hiç gönderilmemiş bir kullanım sonrası 15 saniyelik bekleme oluşturabilir.
- Universal/Purification ise 558 ve 600'de sonucu doğru kontrol ediyor.

Öneri: yerel gönderim başarısızsa başarı durumuna geçme/timer başlatma. Yerel `true` sonucunun sunucu kabulü anlamına gelmediğini de koru.

## 4. P2 — Saatli dönüşte “botu durdur” seçeneği eziliyor

`xBot/App/ProtectionManager.cs:357` `ReturnAtTimeStopBot` için `stopAfterReturn = true` yapıyor. Hemen çağrılan `TryReturnToTown`, 486'da aynı alanı `StopBotInTown` değeriyle değiştiriyor.

Örnek: saatli dönüşün “stop” kutusu açık, genel “şehirde dur” kapalı. Başarılı dönüş yardımcısı durma isteğini false yapar.

Öneri: dönüş sebebine ait durma isteğini yardımcı metoda aktar; genel ayarla ezme. Başarısız gönderim sonrası da bekleyen durma durumu bırakma.

## 5. P2 — Universal/Purification null kontrolü eksik

`xBot/App/Bot/Bot.Checks.cs:539,572` doğrudan `InfoManager.Character.LifeStateType` okuyor. HP/MP/Vigor girişlerindeki koruma burada yok.

Çağrı yolları: timer abonelikleri 386-387; `Bot.Events.cs:369-370`; UI üzerinden `Window.cs:3365,3370`. UI yolu `inGame` kontrolü yapsa da karakter referansının kendisini sabitlemiyor.

Null karakterle exception oluşacağı açıktır; ancak “timer tetiklenince uygulama kesin çöker” sonucu bu kaynak incelemesiyle kanıtlanmış değildir. Çağrı yolunun exception işleme davranışı önemlidir.

Öneri: karakteri yerel değişkene al, null/oyun/yaşam durumunu doğrula; işlem boyunca aynı referansı kullan. Diğer metotlarda yalnızca giriş null kontrolü bulunması da eşzamanlı bağlantı kopmasına karşı tam koruma değildir.

## 6. P2 — DOT mantığı hem fazla geniş hem eksik

`xBot/App/Bot/Bot.IA.cs:1198` yalnızca `BadStatusFlags != None` kontrol ediyor. Dondurma, korku veya yavaşlatma gibi durumlar da DOT sayılıyor. 1201'de sadece iç saldırı döngüsü kırılıyor; bu mob için geçici dışlama/yeniden seçmeme kaydı oluşturulmuyor. Sonraki `GetMobFiltered` çağrısı aynı mobu tekrar seçebilir.

`SwitchMonsterDotDelay` ise JSON/UI dışında kullanılmıyor (`CombatAIEngine.cs:68`).

Öneri: gerçek DOT maskesi, uygulanma zamanı, gecikme ve başka hedefe geçişin ne zaman sona ereceği birlikte tanımlanmalı. Yalnızca `Sleep` eklemek yeterli değil.

## 7. P2 — Claude'un dört ayarına ek olarak “UseLowerSkills” de etkisiz

`xBot/App/CombatAIEngine.cs` içindeki aşağıdaki ayarlar UI/JSON bağlantısına sahip, fakat davranışı kontrol etmiyor:

| Ayar | Tanım satırı | Sonuç |
|---|---:|---|
| SwitchTargetByPosition | 37 | Hedef seçimine bağlanmamış |
| SwitchMonsterDotDelay | 68 | DOT geçişinde okunmuyor |
| UseLowerSkills | 69 | Genel skill listesine geçişi kapatmıyor |
| Lagtastic | 71 | Çalışma zamanı kararında kullanılmıyor |
| UseTeleportSkills | 74 | Teleport skill çağrısına bağlanmamış |

`xBot/App/Window.cs:1711-1712,1738-1739` mob tipinin listesi boşken General listesini koşulsuz döndürüyor. Dolayısıyla UseLowerSkills kapalı olsa da fallback sürüyor; sorun yalnızca unutulmuş bir okuma değil, kullanıcı seçeneğinin tersine çalışan davranış.

`Bot.IA.cs:1422` içindeki `TryCastTeleportSkill` için C# kaynaklarında çağrı bulunmadı.

## 8. P2 — Lure arayüzünün önemli bölümü çalışma zamanına bağlanmamış

`xBot/App/LurePolicy.cs` ve `Window.PhBotAttackTabs.cs` karşılaştırması:

| Ayarlar | Tanım satırları | Doğrulanan eksik |
|---|---:|---|
| WalkBackDist | 10 | Mesafe değeri yürüyüşte okunmuyor; enable yalnızca tick girişini açıyor |
| StopTownLessEnabled / StopTownLessCount | 18-19 | `ShouldPauseLure` bu koşulu değerlendirmiyor |
| AttackLimitEnabled / AttackMobLimit | 27-28 | Saldırı sınırında okunmuyor |
| DelayEdgeMs / DelayCenterMs / DelayHalfMs | 30-32 | Lure beklemelerinde kullanılmıyor |
| ScriptPath | 35 | Seçilen lure scripti yürütülmüyor; UseScript yalnızca giriş koşulunda okunuyor |
| RandomWalk / WalkToSpawns / SmartWalk | 37-39 | Hareket kararlarına bağlanmamış |
| PartyMemberFarAwayDistance | 41 | Kullanıcının uzaklık eşiği okunmuyor |
| BuffAfterScriptCommand | 43 | Script sonrası buff davranışına bağlanmamış |

`Bot.IA.cs:1482` parti uzaklığında kullanıcının değeri yerine `trainingRadius + 45.0` kullanıyor. `CheckLureTick` 1444-1536 arasında seçilen scripti çalıştıran veya geri yürüme mesafesini uygulayan kod yok.

Ölü olmayanlar: StopDeadParty, StopGiantParty, StopMobCount ve StopIfPartyAway, `LurePolicy.ShouldPauseLure` üzerinden gerçekten kullanılıyor. Bunları yalnızca UI referansı görünmesine bakarak ölü saymak yanlış olur.

## 9. P2 — Lure hedef seçimi normal combat filtrelerini atlıyor

`xBot/App/Bot/Bot.IA.cs:1516-1524` lure hedefini yalnızca null, mevcut hedef ve training mesafesiyle seçiyor. Normal seçicinin `CombatPolicy.CanTarget`, Ignore/Avoid ve canlı hedef kontrolleri burada yok.

Örnek: lure skill açıkken kullanıcı Avoid verdiği bir mobu normal saldırıdan çıkarsa da lure döngüsü bu moba skill gönderebilir. Bu, normal combat filtresinin çalışmamasıyla karıştırılabilir.

Öneri: lure için açıkça gerekli istisnalar haricinde ortak hedef uygunluk kararını kullan.

## 10. P2 — Parti dışına resurrect desteği partisizken çalışmıyor

`xBot/App/PartySupportManager.cs:47` parti yoksa veya üye sayısı birden büyük değilse tüm `RunTick` metodundan çıkıyor. 124-157'deki regex ile eşleşen parti dışı ölü oyuncuları diriltme koduna bu durumda ulaşılamıyor.

Öneri: sadece partiye özgü dalları parti varlığına bağla; regex hedefleri ayrı değerlendir.

## 11. P2 — “Yakındaki herkesi buffla” yalnızca ilk oyuncuyu buluyor

`xBot/App/PartySupportManager.cs:252` wildcard `*` ataması ve `BuffAllNearbyPlayers` için `Players.Find` kullanıyor. Bu yalnızca ilk oyuncuyu döndürür. Oyuncu ölü veya menzil dışındaysa 253-254'te tüm atama atlanır; diğer uygun oyuncular denenmez. İlk oyuncu uygunsa da aynı wildcard atamasında diğer oyuncuları dolaşan bir döngü yoktur.

Öneri: uygun oyuncuları filtreleyerek dolaş; mevcut buff/hedef bazlı takip ile aynı kişiye tekrar tekrar uygulamayı önle.

Ek tutarsızlık: 286-291'deki `BuffAndReturnToCenter`, yalnız buff için değil heal/ress/cure dahil her `pendingSkill` sonrası doğrudan `MoveTo` gönderiyor. Kullanıcının beklemediği merkez yürüyüşlerinin ayrı bir kaynağı olabilir.

## 12. P2 — Diğer doğrulanmış işlevsiz ayarlar

| Alan | Ayarlar ve kanıt |
|---|---|
| Otomatik yapılandırma | `AutoConfigureManager.cs:17-18`: AutoConfigureBeforeTownLoop, UseSharedPickFilter; UI/JSON dışında tüketici bulunmadı |
| Şehir scripti | `ReturnToAreaPolicy.cs:28`: ContinueTownScript; UI/JSON dışında tüketici bulunmadı |
| Etkinlik NPC | `TrainingOptionsPolicy.cs:25`: UseEasterEggEventNpcs; UI/JSON dışında tüketici bulunmadı |
| Zamanlı koruma | `ProtectionManager.cs:66-67`: ReturnDisconnectMinutesEnabled / ReturnDisconnectMinutes; 350-380'deki zamanlı kararlar bunları işlemiyor |
| Energy of Life | `TrainingOptionsPolicy.cs:48` → `CombatAIEngine.UseEnergyOfLife`: normal kullanım ayarı okunmuyor. `Bot.IA.cs:1807` yalnız UseEnergyOfLifeForZerk üzerinden `TryUseEnergyOfLife(true)` çağırıyor |

ZerkInScript ayrıca eksik bağlı: `CombatAIEngine.cs:48` içinde genel berserk motorunu etkinleştiriyor, ama script bağlamını kontrol eden bir tüketici yok. Üstelik `Bot.IA.cs:1827` düşük HP fallback'i seçilen mob tipi/script koşulundan bağımsız berserk açabiliyor. Örnek: yalnız Script seçili, normal savaşta HP %40 ve yakında mob varken script dışında zerk tetiklenebilir.

## 13. P3 — Kiting kaldırılmamış, fakat mevcut UI akışında kapalı

- `xBot/App/Window.Designer.cs:10213`: checkbox hâlâ üretiliyor; 11865'te alanı var.
- `Window.CustomTabs.cs:2030`: kutu false yapılıyor ve grup gizleniyor.
- `Bot.IA.cs:984-986`: çağrı bu kutunun açık olmasına bağlı.
- `Bot.IA.cs:2482`: ExecuteKiting gövdesi duruyor; karşı yöne 6 m hareket, doğrudan MoveTo, 500 ms Sleep. Collision ve training alanı kontrolü yok.

Sonuç: kullanıcı haklı, kod kalıntısı mevcut; ama bunu şu an aktif çalışan kiting hatası olarak sunmak doğru değil. Kullanıcı kiting istemediği için öneri algoritmayı geliştirmek değil, metodunu, çağrısını ve Designer/alan kalıntısını birlikte kaldırmaktır.

## 14. P3 — Avoid skor dalı erişilemez

`xBot/App/Bot/Bot.IA.cs:1624-1633` Avoid durumunu `CombatPolicy.CanTarget` üzerinden eliyor (`CombatPolicy.cs:33`). Aynı mob için 1756-1758'deki -5000 Avoid puanına ulaşılamıyor.

Ignore ve Avoid ürün anlamları ayrıca netleştirilmeli; mevcut kodda bu puanı silmek yeni bir davranış eklemez.

## 15. P3 — Çağrısız yardımcılar ve eski UI yapıcıları

Kaynak genelinde tanım dışında sembol referansı bulunmayan, gözden geçirilmiş örnekler:

- `xBot/App/Bot/Bot.IA.cs:1422` — TryCastTeleportSkill.
- `xBot/App/Bot/Bot.IA.cs:3549` — WalkLoop.
- `xBot/App/Bot/Bot.IA.cs:4197` — ScanFerryNearby.
- `xBot/Game/Navigation/NavigationManager.cs:817` — StandOff.
- `xBot/App/Window.CustomTabs.cs:1991,2061,2112,2142,2452,2457` — BuildCombatTabWidgets, BuildCombatBerserkCard, BuildCombatAdvancedCard, BuildCombatAvoidTable, EnsureTrainingCombatTab, ResizeTrainingTab.

ZerkWhenHPFull, ZerkAvoidanceBased ve ZerkRarityBased eski UI ile ilişkili kalıntılardır; etkin çalışma zamanı tüketicileri bulunmadı. Bunlar görünür UI'daki yeni berserk seçenekleriyle karıştırılmamalıdır.

Bu liste otomatik toplu silme listesi değildir. Yardımcıların alt çağrıları başka canlı UI tarafından kullanılabilir. Örneğin CreateCombatTick ve SaveCombatAI tanım dışında kullanılıyor; üst kart yapıcısının çağrısız olması bu ortak yardımcıları otomatik olarak ölü yapmaz. Temizlikte derleme ve UI açılış kontrolü gerekir.

## Claude bulgularına ilişkin düzeltmeler

- Null kontrolü eksik: doğru. Her timer exception'ının kesin uygulama çökmesi olduğu kanıtlanmış değil.
- Dört ayarın davranışa bağlı olmadığı: doğru. UseLowerSkills ve diğer alanlarda da benzer eksikler var.
- Avoid puanı ölü: doğru.
- Pot yok tespiti dar: doğru. Universal/Purification burada HP kaynağı olarak sayılmamalı; kod bunları kötü durum temizliği için kullanıyor. Vigor ve kullanılabilir heal skilleri anlamlı örnekler.
- Kiting collision/alan kontrolü eksik: gövde açısından doğru; mevcut UI akışı özelliği kapatıyor.

## Önerilen uygulama sırası

1. Gizli panic dönüşü, yanlış eşya seçimi, Vigor kutuları, null koruması ve başarısız gönderim sonuçları.
2. Saatli dönüşün durma isteği, hedef/lure filtreleri ve DOT geçişi.
3. İstenmeyen kiting ve çağrısız eski combat parçalarının kaldırılması.
4. Kullanılacak özelliklerin UI → ayar → karar → eylem zincirinin tamamlanması; kullanılmayacakların UI ve JSON uyumluluğu gözetilerek kaldırılması.
5. Parti buff/ress davranışlarının hedef bazlı senaryolarla doğrulanması.

Bu incelemede yalnızca bu rapor eklendi; mevcut uygulama davranışında değişiklik yapılmadı.
