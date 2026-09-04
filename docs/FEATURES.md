# xBot özellik envanteri

Bu dosya, projedeki kullanıcıya dönük işlevlerin ve geliştirme durumlarının tek takip noktasıdır. Liste kaynak kod taramasıyla hazırlanmıştır; “Kullanımda” etiketi temel akışın kodda bağlı olduğunu, “Deneysel” etiketi ise akışın sunucu/sürüm bazında ayrıca doğrulanması gerektiğini gösterir.

Son kaynak taraması: 2026-09-04

## Durum sözlüğü

| Durum | Anlamı |
| :--- | :--- |
| ✅ Kullanımda | Kod, arayüz ve temel akış bağlı; normal kullanım için mevcut |
| 🧪 Deneysel | Kod/arayüz mevcut; kapsamlı oyun içi doğrulama veya entegrasyon sürüyor |
| ⚠️ Kısmi | Özelliğin bir bölümü bağlı, diğer bölümleri altyapı veya arayüz seviyesinde |
| ⏸️ Devre dışı | Kod/sekme bulunuyor ancak mevcut sürümde bilinçli olarak kullanılmıyor |
| 📌 Planlandı | Henüz uygulanmadı |

## Kullanıcı özellikleri

### Bağlantı, giriş ve istemci

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-001 | Client mode ile istemci başlatma | ✅ | `App/ClientManager.cs`, `App/EdxLoader.cs`, `xBot.Loader.Library/` |
| F-002 | Clientless çalışma | ✅ | `Network/Proxy.cs`, `Network/Gateway.cs`, `Network/Agent.cs` |
| F-003 | Gateway/Agent proxy ve paket güvenliği | ✅ | `Network/`, `SecurityAPI/` |
| F-004 | Host/port seçimi ve rastgele host | ✅ | `Network/Proxy.cs`, Settings ekranı |
| F-005 | Otomatik giriş ve karakter seçimi | ✅ | `App/Bot/Bot.Events.cs`, `App/LoginStrategyManager.cs` |
| F-006 | En yüksek seviyeli karakteri otomatik seçme | 🧪 | `App/LoginStrategyManager.cs`; UI ve ayar mevcut |
| F-007 | Relogin ve bağlantı sonrası clientless geçiş | ✅ | `Network/Proxy.cs`, `App/Bot/Bot.Events.cs` |
| F-008 | Sabit captcha ayarı | ⚠️ | Ayar/UI altyapısı var; captcha akışına tam bağlı değil |
| F-009 | Client gizleme/gösterme ve hızlı gizleme | ✅ | `App/ClientManager.cs`, `App/Window.CustomTabs.cs` |
| F-010 | Otomatik bot başlatma ve bağlantıda kalma | ✅ | `App/LoginStrategyManager.cs`, `App/Bot/Bot.Events.cs` |
| F-011 | Otomatik karakter oluşturma/silme seçenekleri | ✅ | `App/Window.cs`, `Game/PacketBuilder.cs`, ayarlar |

### Bot, savaş ve koruma

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-020 | Bot başlatma/durdurma | ✅ | `App/Bot/Bot.IA.cs` |
| F-021 | Eğitim alanında saldırı ve hareket | ✅ | `App/Bot/Bot.IA.cs`, `App/Script.cs` |
| F-022 | Saldırı ve buff beceri listeleri | ✅ | `App/Window.cs`, `Game/Objects/Common/SRSkill.cs` |
| F-023 | Beceri sıralama ve sırayla kullanma | ✅ | `App/SkillManager.cs`, Skills sekmesi |
| F-024 | Imbue seçimi ve Devil Spirit kullanımı | 🧪 | `App/SkillManager.cs`; akış bot döngüsüne bağlı |
| F-025 | Mob türüne göre hedef seçimi | ✅ | `App/Bot/Bot.IA.cs`, `CombatAIEngine.cs` |
| F-026 | Mob kaçınma/öncelik ve zayıf hedef önceliği | 🧪 | `App/CombatAIEngine.cs`; bazı seçenekler UI’da mevcut |
| F-027 | Berserk tetikleme kuralları | 🧪 | HP, mob sayısı ve nadirlik kuralları `CombatAIEngine.cs` içinde |
| F-028 | Dimension Pillar/gate filtreleme | ✅ | `CombatAIEngine.IsDimensionPillar()` ve saldırı seçimi |
| F-029 | Kiting ve panik kaçışı | 🧪 | `App/Bot/Bot.IA.cs`; oyun içi senaryo testi gerekli |
| F-030 | HP/MP/vigor/universal/purification kullanımı | ✅ | `App/Bot/Bot.Checks.cs` |
| F-031 | Recovery kit, abnormal pill ve pet HGP kontrolü | ✅ | `App/Bot/Bot.Checks.cs` |
| F-032 | Skill ile HP/MP iyileştirme | ✅ | `App/ProtectionManager.cs`; event + merkezi bot tick akışına bağlı |
| F-033 | Skill ile kötü durum temizleme | ✅ | `App/ProtectionManager.cs`; merkezi bot tick akışına bağlı |
| F-034 | Pet diriltme ve otomatik çağırma | ✅ | `App/ProtectionManager.cs`; merkezi bot tick akışına bağlı |
| F-035 | Ölüm, düşük HP/MP, dayanıklılık ve dolu envanter dönüşleri | ✅ | `App/ProtectionManager.cs`, `App/ProtectionPolicy.cs`; merkezi tick ve 14 senaryo testi |
| F-036 | Level-up sonrası otomatik STR/INT dağıtımı | ✅ | `App/StatPointManager.cs`, `Game/InfoManager.cs` |
| F-037 | Support/no-attack modu | ✅ | `App/SkillManager.cs`, `App/Bot/Bot.IA.cs` |

### Şehir, navigasyon ve rota

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-040 | Şehir döngüsü | ✅ | `App/Bot/Bot.IA.cs` |
| F-041 | Tamir, storage, çöp satışı, potion/pill satın alma | ✅ | `Game/Navigation/TownManager.cs`, `App/Bot/Bot.IA.cs` |
| F-042 | NavMesh veri okuma ve zlib açma | ✅ | `Game/Navigation/NavDataReader.cs` |
| F-043 | A* yol bulma ve yol yumuşatma | ✅ | `Game/Navigation/AStarPathfinder.cs` |
| F-044 | Çok bölgeli bileşik rota | 🧪 | `Game/Navigation/NavigationManager.cs`; `navdata/` kapsamına bağlı |
| F-045 | Teleport/ferry rota bağlantısı | 🧪 | `Game/Navigation/TeleportManager.cs` |
| F-046 | Minimap üzerinden hareket ve teleport | ✅ | `xGraphics/`, `Game/Navigation/` |
| F-047 | Trace, koordinat kaydı ve script hareketi | ✅ | `App/Script.cs`, `App/Bot/Bot.cs`, Training sekmesi |

### Eşya, envanter ve şehir ekonomisi

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-050 | Inventory/avatar/storage/pet/guild storage görüntüleme | ✅ | `App/Window.cs`, `Game/PacketParser.cs` |
| F-051 | Eşya kullanma, kuşanma, çıkarma, düşürme ve taşıma | ✅ | `Game/PacketBuilder.cs`, `Game/PacketParser.cs` |
| F-052 | Envanter sıralama | ✅ | `App/Bot/Bot.cs` |
| F-053 | Pet ile eşya toplama | ✅ | `App/Bot/Bot.IA.cs` |
| F-054 | Eşya pickup filtresi: SoX, cinsiyet ve açık kurallar | 🧪 | `App/ItemFilterManager.cs`; pickup akışına bağlı |
| F-055 | Degree/China/Europe filtresi | ⚠️ | Ayar alanları ve JSON kaydı var; tüm karar noktalarına bağlanmalı |
| F-056 | Eşyayı satma/depolama kuralları | ⚠️ | `ShouldSell`/`ShouldStore` altyapısı var; şehir döngüsüne entegrasyon kontrolü gerekli |
| F-057 | NPC alış/satış ve buy-back paketleri | ✅ | `Game/PacketBuilder.cs`, `Game/PacketParser.cs` |

### Party, guild, exchange, stall ve sohbet

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-060 | Oyuncu görüntüleme ve yenileme | ✅ | Players sekmesi, `InfoManager.cs` |
| F-061 | Party oluşturma, davet, ayrılma ve üye yönetimi | ✅ | `Game/Objects/Party/`, `Game/PacketBuilder.cs` |
| F-062 | Party matching ve otomatik reform | ✅ | `SRPartyMatch`, `Bot.Checks.cs`, paket katmanı |
| F-063 | Auto party / leader listesi | ✅ | Party ayarları, `Bot.Checks.cs` |
| F-064 | Guild görüntüleme, davet ve guild storage | ✅ | `Game/Objects/Guild/`, Guild sekmesi |
| F-065 | Academy daveti ve bilgisi | ✅ | `Game/PacketBuilder.cs`, `PacketParser.cs` |
| F-066 | Exchange ve otomatik exchange onayları | ✅ | Players sekmesi, paket katmanı |
| F-067 | Stall oluşturma, düzenleme, kapatma ve alış | ✅ | Stall sekmesi ve `Game/PacketBuilder.cs` |
| F-068 | Sohbet kanalları ve global item kullanımı | ✅ | `Game/PacketBuilder.cs`, `Bot.Events.cs` |
| F-069 | Leader sohbet komutları | ✅ | `App/Bot/Bot.Events.cs`, README komut tablosu |
| F-070 | PING, TIME ve ISEEDEADPEOPLE komutları | ✅ | `App/Bot/Bot.Events.cs`, `CHANGELOG.md` |

### Arayüz, veri ve geliştirici araçları

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-080 | Minimap, özel kontroller ve canlı oyun bilgisi | ✅ | `xGraphics/`, `App/Window.cs` |
| F-081 | Game Info/Spy ekranı | ⏸️ | `v0.5.0` changelog’una göre yeniden çalışma için devre dışı |
| F-082 | Packet analyzer: filtre, sıralama ve injection | ✅ | Settings sekmesi, `Network/`, `PacketBuilder.cs` |
| F-083 | PK2 veri çıkarma | ✅ | `PK2Extractor/` |
| F-084 | Item/skill/model/mastery/teleport/region/minimap üretimi | ✅ | `PK2Extractor/PK2Extractor.Parser.cs`, `.Media.cs` |
| F-085 | SQLite veritabanı oluşturma ve sorgulama | ✅ | `App/SQLDatabase.cs`, `Game/DataManager.cs` |
| F-086 | JSON bot/karakter ayarları ve Default profile | ✅ | `App/Settings.cs` |
| F-087 | TR/EN özel arayüz dil katmanı | 🧪 | `App/LocalizationManager.cs`, `Window.CustomTabs.cs` |
| F-088 | Hızlı kaydetme ve header HP/MP/level göstergeleri | 🧪 | `App/Window.CustomTabs.cs`; UI doğrulaması gerekli |
| F-089 | Güncelleme kontrolü ve reklam penceresi | ✅ | `App/Ads.cs`, AutoUpdater referansı |

## Geliştirilmekte olan yöneticiler

Bu bölüm, çalışma ağacında yeni görünen yöneticileri ayrı izler. Yeni özellik eklenirken ilgili manager, bot çağrı noktası, UI ve JSON ayarı birlikte kontrol edilmelidir.

| Yönetici | Sorumluluk | JSON bölümü | Mevcut bağlantı | Sonraki kontrol |
| :--- | :--- | :--- | :--- | :--- |
| `LoginStrategyManager` | Otomatik giriş, karakter seçimi, bekleme, otomatik başlatma/gizleme | `LoginStrategy` | Giriş olayları ve proxy’ye bağlı | Captcha akışını tamamla, runtime test |
| `CombatAIEngine` | Hedef önceliği, kaçınma ve berserk tetikleri | `CombatAI` | `Bot.IA` hedef seçiminde bağlı | Her kural için UI + oyun içi test |
| `SkillManager` | Imbue, Devil Spirit, support/no-attack, beceri sırası | `SkillManager` | Bot döngüsünde bağlı | Skill bulunamadığında güvenli fallback |
| `ProtectionManager` | Skill iyileştirme, pet koruması, şehir dönüş tetikleri | `ProtectionManager` | Her koruma kontrolü merkezi bot tick’inde; `ProtectionPolicy` karar katmanı kullanılıyor | Oyun içi gerçek client senaryolarıyla doğrula |
| `StatPointManager` | Level sonrası stat dağıtımı | `StatPointManager` | Level-up/InfoManager’a bağlı | STR/INT hedef doğrulaması |
| `ItemFilterManager` | Pickup/sell/store kuralları ve item kriterleri | `ItemFilterManager` | Pickup’un bir bölümü bağlı | Degree/race ve sell/store kararlarını bot akışına bağla |
| `LocalizationManager` | Yeni özel kontroller için TR/EN metinleri | Yok | Özel UI başlatılırken bağlı | Kaynak dosyalardaki encoding/metinleri temizle |

## Yeni özellik ekleme akışı

1. Bu dosyada yeni bir `F-xxx` kimliği aç.
2. Durumu `📌 Planlandı` olarak ekle; hedef kullanıcı davranışını tek cümlede yaz.
3. Kaynak dosyaları, ilgili UI sekmesini ve ayar/JSON anahtarını belirt.
4. Kod eklendiğinde `🧪 Deneysel` veya `⚠️ Kısmi` durumuna geçir.
5. Bağlantı, UI, ayarların kaydı/yüklenmesi ve oyun içi senaryoyu doğrula.
6. Doğrulama tamamlanınca `✅ Kullanımda` yap ve `CHANGELOG.md` içine sürüm/başlık ekle.
7. Kullanım şekli değiştiyse README’ye kısa bir bölüm ekle.

## Özellik kaydı formatı

Kopyalanabilir şablon için [`FEATURE_TEMPLATE.md`](FEATURE_TEMPLATE.md) dosyasını kullan.
