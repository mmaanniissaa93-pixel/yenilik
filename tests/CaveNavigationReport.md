# Cave navigation / walk-trigger inceleme sonucu

Release x86 çıktısı: `xBot/bin/x86/Release/xBot.exe`.

## Doğrulanan nedenler

- Mevcut runtime logunda DW girişine 2,7 m yaklaşılmış, sonra `reached closest nav point but no candidate` ile durulmuş. Bu kapı NPC etkileşimi gerektirmeyen `GATE_DUNGEON_DH_IN` kaydıdır.
- `cnav` dosyaları gerçekten okunuyordu. Fakat noktaları, birbirinden ayrılmış 10.000 m atlas hücrelerinde saklanıyor: cnav01 merkezi 85000, cnav07 merkezi 25000, cnav06 merkezi 35000. `SRCoord` ise dungeon-local metreleri 24576 tabanında tutuyor. Bounds/nearest-node/A* karşılaştırmaları dönüşümsüz yapıldığı için yanlış mesafeler üretiyordu.
- `cnavNN -> NN | 0x8000` kimliği incelenen DB region değerleriyle uyumlu. Hata bu kimliği silmekle çözülmedi; doğru dosya kimliği korundu, atlas koordinatları runtime uzayına dönüştürüldü. Dönüşüm hem bounds okumasına hem gerçek düğümlere uygulanıyor.
- A* waypoint'leri varsayılan outdoor `SRCoord` kurucusundan çıkıyordu. Cave waypoint'leri artık gerçek dungeon region'ını ve işaretli Z değerini koruyor.
- Cave odalarının yerel koordinatları çakışabilir. Salt XY yakınlığı veya bounds komşuluğu, odalar arasında yürünebilir bağlantı değildir. Aynı region'daki bağlantısız odalar için de DB portal segmentleri gerekir.

## İncelenen gerçek DB kayıtları

Kaynak: mevcut `xBot/bin/Debug/Data/Silkroad #1/Database.sqlite3`; derlenmiş uygulamanın Release DB yükleyicisi de ayrıca test edildi. Aşağıdaki koordinatlar **DB/paket birimleriyle `(region; X,Y,Z)`** gösterilmiştir; X/Y metre değerleri değildir.

| Bağlantı | Source / destination | Board / trigger | Arrival | Nav dosyaları | En yakın node mesafesi (source / arrival) |
|---|---|---|---|---|---|
| 11 → 10 | `GATE_DUNGEON_DH_IN`, Thief Town → Thief Town | `(27027;1814,1834,452)` | `(32769;1011,-862,0)` | nav02 → cnav01 | 2,67 / 3,61 m |
| 10 → 11 | `GATE_DUNGEON_DH_OUT`, Thief Town → Thief Town | `(32769;1011,-862,0)` | `(27027;1814,1834,452)` | cnav01 → nav02 | 3,61 / 2,67 m |
| 55 → 56 | `GATE_JINSI_OUT`, Qin-Shi Tomb → Tomb of Qin-Shi Emperor lv.1 | `(26284;960,1662,111)` | `(32775;-1,-3200,-11)` | nav01 → cnav07 | 4,29 / 9,96 m |
| 56 → 55 | `GATE_JINSI_IN`, lv.1 → Qin-Shi Tomb | `(32775;-1,-3200,-11)` | `(26284;960,1662,111)` | cnav07 → nav01 | 9,96 / 4,29 m |
| 57 → 59 | `GATE_JINSI_01x01_12`, lv.1 → lv.2 | `(32775;2,6495,223)` | `(32774;-13674,5596,0)` | cnav07 → cnav06 | 13,28 / 11,60 m |
| 58 → 62 | `GATE_JINSI_02x01_03`, lv.2 oda → lv.2 oda | `(32774;-10468,5596,0)` | `(32774;-8259,5592,0)` | cnav06 → cnav06 | 9,57 / 10,07 m |

Bu kayıtların `NpcId=0`, `tid1=4`, `gold=0`; model/building tablosunda entity karşılıkları yok. Giriş minimum seviyeleri DW için 50, Jangan için 70. DB adları korunmuştur; isimde "Cave" geçmesi aranmıyor.

`teleportlinks` tablosunda açık bir walk-trigger veya runtime option listesi kolonu yok. Güvenli sınıflandırma için birlikte şu kanıtlar aranıyor: entity/model yok, NpcId=0, extractor'ın tid1=4 değeri, `GATE_` servername, ücretsiz bağlantı, en az bir dungeon ucu ve koordinatları eşleşen ters bağlantı. Tek başına NpcId=0 yeterli değildir.

Karşı örnekler: `162 → 160 / GATE_RC_ROC_GATE` outdoor Roc dönüşü olduğu için Interaction kalır. `86 → 1` ücretli cave çıkışı da Interaction kalır. Ters kaydı olmayan belirsiz bağlantılar otomatik trigger kabul edilmez.

## Yeni akış

- `FindCompoundRoute` cave rotalarında bağlı yürüyüş parçalarını ve DB transition bağlantılarını birlikte arar. Testte Jangan için `55 → 57 → 58` transition dizisi oluşturuldu; son yürüyüş segmenti region 32774'ü korudu.
- `WalkTriggerNavigator`: güncel konumdan gerçek A* zincirini yürür; ardından trigger merkezine kontrollü adımlarla basar. Candidate taraması veya UseTeleport çağrısı yoktur.
- Gerçek kayıtlar mesh dışında 10–14 m trigger boşlukları içerdiğinden, yalnızca mesh sonundan başlayarak toplam en fazla 16 m boşluk, en fazla 3 m adımlarla ve her adımın hareket sonucuyla geçilir. Daha büyük boşluklarda doğrudan yürüyüş yapılmaz.
- Başarı, beklenen destination region/konumuna ulaşmayla doğrulanır. Aynı-region oda geçişlerinde konum değişimi de değerlendirilir. Normal outdoor sector değişimi başarı sayılmaz. Loading sırasında source waypoint hareketi kesilir.
- En fazla iki deneme vardır. Normal cave waypoint hataları rotayı durdurur. Cave rotası bulunamadığında eski uzun/düz yürüme fallback'i kullanılmaz.
- Ferry'nin candidate taraması, navmesh yaklaşımı ve interaction akışı korunmuştur.

## Değişen başlıca noktalar

`NavDataReader.Read/TryReadBounds`, `NavigationManager.FindPath/FindApproachPath/FindMultiRegionRoute/FindCompoundRoute/BuildRegionGraph/WarmupCacheNear`, A* ham waypoint seçimi, `TeleportManager.TryLoadFromDatabase`, `Bot.ExecuteTeleportTransition/WaitNavigationWaypoint/WaitMovement`.

Yeni sınıflar: `TeleportLinkInfo` (ayrı dosyaya taşındı), `TeleportTransitionPolicy`, `TransitionRoutePlanner`, `WalkTriggerNavigator`.

## Doğrulama

`tests/FerryNavigationScenarios.ps1` ile 76 graph/route/executor kontrolü ve 7 gerçek uygulama assembly kontrolü geçti. Bunlar 23 cnav dosyasının normalize edilmiş bounds/düğüm uyumunu, DW/Jangan yollarını, çoklu oda portal rotasını, loading/timeout/stop davranışını, gerçek SQLite yükleyicisini ve Roc ferry regresyonlarını kapsıyor.

Yeni build ile canlı DW/Jangan portal geçişi henüz yapılmadı. Eski runtime logu girişteki candidate bekleme hatasını doğruluyor; yeni davranışın canlı server/trigger testi ayrı olarak gerekli. Runtime kayıtları `[CAVE-NAV]`, `[CAVE-PATH]`, `[CAVE-TRANSITION]` altında bırakıldı.
