# Özellik ve backend durumu

Bu liste, arayüzde görünen her alanın aynı ölçüde çalıştığını varsaymaz. “Backend'e bağlı” kaynakta event → state → runtime/persistence zinciri bulunduğu anlamına gelir; oyun içi doğrulama anlamına gelmez. Kontrolleri kapalı olan özellikler özellikle belirtilmiştir.

| Alan | Kod durumu | Kapsam / sınır |
|---|---|---|
| Giriş, sunucu profilleri ve hesaplar | Backend'e bağlı | Gateway/Agent/Proxy bağlantısı ve hesap ayarları var. Sunucu profiline özgü paket davranışı gerçek sunucu oturumunda doğrulanmalıdır. |
| Kasılma alanı ve navigasyon | Backend'e bağlı | Konum/yarıçap, rota, NavMesh/A* ve çarpışma bileşenleri bot döngüsünde kullanılıyor. Geçersiz/ulaşılamayan alanlar oyun ve nav dosyalarına bağlıdır. |
| Savaş ve beceriler | Backend'e bağlı | Hedef filtresi/puanı, mob türü tercihleri, beceri menzili, basic attack yedeği ve saldırı onayı kullanılıyor. Hedef kilidi, açık hedef döndürme kuralı olmadıkça skor değişimlerinin moblar arasında salınım yapmasını engeller. |
| Devil Spirit | Backend'e bağlı | Ready tetikleyicisi mob varlığından bağımsızdır. Mob sayısı/türü tetikleyicileri saldıran mob state'ini ve “saldırmıyor olsa da” seçeneğini kullanır. Cooldown, eşya ve sunucu onayı canlı oyunda doğrulanmalıdır. |
| Koruma ve iksirler | Kısmi | Mevcut HP/MP, dönüş ve skill koruma yolları var. PhBot Sockets desteklenmiyor; kontrollerin yerine açıklama gösteriliyor. Scrolls ekranında yalnız Speed çalışır; diğer scroll kontrolleri kapalıdır. |
| Healing ve Attack Log | Kısmi | Self-heal ve öğrenilmiş healer skill listesi bağlıdır; grup heal desteklenmiyor ve açıklamayla kapalıdır. Attack Log, oyuncuların karakteri hedeflediği bilgiyi listeler; canlı PK/oyuncu saldırısı doğrulaması yapılmadı. |
| Pet ve eşya toplama | Backend'e bağlı | Pet politikası, toplama kuyruğu ve item filtresi runtime'a bağlı; seçenekler profil yüklemesinde yenilenir. |
| Party ve diriltme | Backend'e bağlı | Party support/resurrect state'i karakter profiline kaydedilir. Davet/kabul gibi oyun sunucusu kuralları canlı oturum ister. |
| Quest | Backend'e bağlı | Quest kuralları, parser ve paket/navigasyon akışı var. Değişken quest paketlerinin sunucu profiline göre canlı kontrolü gerekir. |
| Auto Configure profilleri | Backend'e bağlı | Yeni profiller `Config/Profiles` altında oluşturulur, seçilir, yeniden adlandırılır ve silinir. Default ve etkin profil silinemez. Eski `Config/*.json` profilleri seçilebilir; profil düğmeleriyle silinemez/yeniden adlandırılamaz. |
| Trade loop | Backend'e bağlı | PhBot rota/ayar kontrolleri gerçek TradeLoopManager girdilerine bağlıdır. Sunucu ve şehir rotaları canlı oyunda doğrulanmalıdır. |
| Inventory ek sekmeleri | Kısmi | Job/Transport/Guild listeleri oyun state'inden salt okunur gösterilir; Job Pouch parser'da sunulmadığı için açıklamalıdır. Exchange, Job Ticket, Item Inflation/Awakening ve Gori işlemlerinin backend'i yoktur ve kapalıdır. |
| Stall | Kısmi | Ana stall kontrolleri mevcuttur; envanter tek gerçek listeyi kullanır. Filter desteklenmiyor ve açıklamayla kapalıdır. Town-script, auto-refill ve chat otomasyonu desteklenmiyor. |
| Alchemy | Kısmi | Gear yükseltme akışı mevcut manager'a bağlıdır; Start/Stop görünürdür. Shining Stone, Tablet ve toplu kuyruk özellikleri desteklenmiyor ve kapalıdır. |
| Harita | Kısmi | Harita ve koordinat görünümü mevcut; polygon kaydetme/düzenleme desteklenmiyor ve kapalıdır. |
| Players, Guild, Academy, Union Party | Kısmi | Players ve Guild'in oyun state'inden gelen listeleri kullanılabilir. Union Party, Academy yönetimi ve bağlı olmayan davet/kabul eylemleri desteklenmiyor; açıklama gösterilir. |
| Sound, Key Bindings, Conditions, Masteries, Notifications | Desteklenmiyor | Bu özellikler için tam runtime/backend zinciri yok; yanıltıcı kontroller kapalıdır. Kasılma Conditions builder da oyun döngüsüne bağlı değildir. |
| Chat ve paket analizörü | Kısmi | Normal chat/komut ve analizör akışı mevcut; Notifications ekranının ayrı bildirim backend'i yoktur. |
| Ayarlar ve kayıt | Backend'e bağlı | Bot-geneli ve karakter profili dosyaları ayrı yazılır. JSON yazımı yedek korumalıdır; Quick Save artık gerçek yazma sonucunu gösterir. |

## Doğrulama sınırı

Bu kaynak düzeltme turunda derleme veya test çalıştırılmadı. Ayrıca oyun istemcisine bağlanıp paket/onay akışları doğrulanmadı. Dolayısıyla tabloda “backend'e bağlı” yazması, kodda izlenen bağlantının mevcut olduğunu belirtir; üretim çalışma sonucu değildir. Tüm UI olaylarının tek tek çalışma doğrulaması için her satırdaki UI → manager/policy → runtime → kayıt zincirini senaryo bazında çalıştırmak gerekir.
