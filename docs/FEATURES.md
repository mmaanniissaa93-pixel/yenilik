# xBot özellik envanteri

Bu dosya, projedeki kullanıcıya dönük işlevlerin ve geliştirme durumlarının tek takip noktasıdır. Liste kaynak kod taramasıyla hazırlanmıştır; “Kullanımda” etiketi temel akışın kodda bağlı olduğunu, “Deneysel” etiketi ise akışın sunucu/sürüm bazında ayrıca doğrulanması gerektiğini gösterir.

Son kaynak taraması: 2026-09-08

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
| F-005 | Otomatik giriş ve karakter seçimi | ✅ | `App/Bot/Bot.Events.cs`, `Game/PacketParser.cs`, `App/LoginStrategyManager.cs`; Client ve Clientless akışlarında bağlantı → server → karakter adımları otomatik ilerliyor; karakter verisi parser’ı özel server yerleşimlerini de doğruluyor |
| F-006 | En yüksek seviyeli karakteri otomatik seçme | ✅ | `App/LoginStrategyManager.cs`, `App/Window.CustomTabs.cs`; otomatik girişte FirstFound/HighestLevel seçimi uygulanıyor ve test edildi |
| F-007 | Relogin ve bağlantı sonrası clientless geçiş | ✅ | `Network/Proxy.cs`, `App/Bot/Bot.Events.cs` |
| F-008 | Sabit captcha ayarı | 🧪 | `App/LoginStrategyManager.cs`, `Game/PacketParser.cs:CaptchaData`, `Game/PacketBuilder.cs:SubmitCaptcha (0x6323)`, CLI `-captcha=`; captcha paketi loglanıp sabit/UI kodu otomatik gönderiliyor — oyun içi doğrulama gerekli |
| F-009 | Client gizleme/gösterme ve hızlı gizleme | ✅ | `App/ClientManager.cs`, `App/Window.CustomTabs.cs` |
| F-010 | Otomatik bot başlatma ve bağlantıda kalma | ✅ | `App/LoginStrategyManager.cs`, `App/Bot/Bot.Events.cs` |
| F-011 | Otomatik karakter oluşturma/silme seçenekleri | ✅ | `App/Window.cs`, `Game/PacketBuilder.cs`, ayarlar |
| F-012 | Çoklu hesap ve profil yöneticisi | ✅ | `App/AccountManager.cs`, `App/SecretStore.cs`, `App/Settings.cs`, `App/Window.CustomTabs.cs`; çoklu hesap ekleme, silme, otomatik doldurma ve Windows DPAPI ile korunan secret alanları |
| F-013 | İkincil güvenlik şifresi / PIN otomasyonu | ✅ | `App/SecondaryPasscodePolicy.cs`, `App/LoginStrategyManager.cs`, `App/SecretStore.cs`, `Game/PacketBuilder.cs`, `Network/Agent.cs`; `0x7625`/`0x3625`/`0xB625` paketleri, DPAPI saklama ve hesap başına PIN desteği |
| F-014 | SOCKS5 proxy desteği (RFC 1928 / 1929) | ✅ | `App/Socks5Policy.cs`, `Network/Socks5Handler.cs`, `Network/Proxy.cs`; Gateway ve Agent için kimlik doğrulamalı ve hesap bazlı SOCKS5 tüneli, proxy parolaları DPAPI ile saklanır |

### Bot, savaş ve koruma

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-020 | Bot başlatma/durdurma | ✅ | `App/Bot/Bot.IA.cs` |
| F-021 | Eğitim alanında saldırı ve hareket | ✅ | `App/Bot/Bot.IA.cs`, `App/Script.cs` |
| F-022 | Saldırı ve buff beceri listeleri | ✅ | `App/Window.cs`, `Game/Objects/Common/SRSkill.cs` |
| F-023 | Beceri sıralama ve sırayla kullanma | ✅ | `App/SkillManager.cs`, `App/SkillPolicy.cs`, Skills sekmesi; sıra seçeneği saldırı akışına bağlı |
| F-024 | Çin Imbue seçimi, Devil Spirit ve güvenli skill fallback’i | ✅ | `App/SkillManager.cs`, `App/SkillPolicy.cs`, `App/ImbuePolicy.cs`; karakterin algılanan imbue skill’leri seviyeleriyle listeleniyor, seçilen skill ID’si akışta kullanılıyor ve 62 test ile doğrulandı |
| F-025 | Mob türüne göre hedef seçimi | ✅ | `App/Bot/Bot.IA.cs`, `CombatAIEngine.cs` |
| F-026 | Mob kaçınma/öncelik ve zayıf hedef önceliği | ✅ | `App/CombatAIEngine.cs`, `App/CombatPolicy.cs`; UI `Kasılma > Combat AI` altında ve kural testleriyle doğrulandı |
| F-027 | Berserk tetikleme kuralları | ✅ | HP, mob sayısı ve nadirlik kuralları; UI `Kasılma > Combat AI` altında ve kural testleriyle doğrulandı |
| F-028 | Dimension Pillar/gate filtreleme | ✅ | `CombatAIEngine.IsDimensionPillar()` ve saldırı seçimi |
| F-029 | Kiting ve panik kaçışı | ✅ | `App/Bot/Bot.IA.cs`, `App/Window.CustomTabs.cs`; Town dışındaki `Kasılma > Combat AI` sekmesinde menzil koruma ve acil durum kaçışı bağlı |
| F-030 | HP/MP/vigor/universal/purification kullanımı | ✅ | `App/Bot/Bot.Checks.cs` |
| F-031 | Recovery kit, abnormal pill ve pet HGP kontrolü | ✅ | `App/Bot/Bot.Checks.cs` |
| F-032 | Skill ile HP/MP iyileştirme | ✅ | `App/ProtectionManager.cs`; event + merkezi bot tick akışına bağlı |
| F-033 | Skill ile kötü durum temizleme | ✅ | `App/ProtectionManager.cs`; merkezi bot tick akışına bağlı |
| F-034 | Pet diriltme ve otomatik çağırma | ✅ | `App/ProtectionManager.cs`; merkezi bot tick akışına bağlı |
| F-035 | Ölüm, düşük HP/MP, dayanıklılık ve dolu envanter dönüşleri | ✅ | `App/ProtectionManager.cs`, `App/ProtectionPolicy.cs`, `App/Window.CustomTabs.cs`; merkezi tick, UI ve 14 senaryo testi |
| F-036 | Level-up sonrası otomatik STR/INT dağıtımı | ✅ | `App/StatPointManager.cs`, `Game/InfoManager.cs`, `App/Window.CustomTabs.cs`; Pure STR/INT ve hibrit oran arayüzü Koruma sekmesinde aktif |
| F-037 | Support/no-attack modu | ✅ | `App/SkillManager.cs`, `App/Bot/Bot.IA.cs` |
| F-038 | Parti iyileştirme, canlandırma ve koruma | ✅ | `App/PartySupportManager.cs`, `App/PartyPolicy.cs`, `App/Bot/Bot.IA.cs`; canı düşen üyelere Cleric heal, ölen üyelere resurrect, debuff silme ve 18 bağımsız test |
| F-039 | Akıllı çift silah değişimi ve ana silaha geri dönüş | ✅ | `App/Bot/Bot.Checks.cs`, `App/Bot/Bot.IA.cs`, `App/PartyPolicy.cs`; buff ve destek sonrası ana saldırı silahına (Staff, Bow, 2H vb.) otomatik geri dönüş |

### Şehir, navigasyon ve rota

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-040 | Şehir döngüsü | ✅ | `App/Bot/Bot.IA.cs` |
| F-041 | Tamir, storage, çöp satışı, potion/pill satın alma | ✅ | `Game/Navigation/TownManager.cs`, `App/Bot/Bot.IA.cs` |
| F-042 | NavMesh veri okuma ve zlib açma | ✅ | `Game/Navigation/NavDataReader.cs` |
| F-043 | A* yol bulma ve yol yumuşatma | ✅ | `Game/Navigation/AStarPathfinder.cs` |
| F-044 | Çok bölgeli bileşik rota | ✅ | `Game/Navigation/NavigationManager.cs`; NavMesh A* ve bölgeler arası geçiş bağlı |
| F-045 | Teleport/ferry rota bağlantısı | ✅ | `Game/Navigation/TeleportManager.cs`, `App/Bot/Bot.IA.cs`; nehir feribotları ve şehir kapısı teleport geçişleri bağlı |
| F-046 | Minimap üzerinden hareket ve teleport | ✅ | `xGraphics/`, `Game/Navigation/` |
| F-047 | Trace, koordinat kaydı ve Script Engine v2 | 🧪 | `App/Script.cs`, `App/ScriptCommandCatalog.cs`, `App/Bot/Bot.cs`, Training > Script; eski komutlara ek olarak `walk/cast/use/teleport/recall/stop/disconnect`, temel town/storage komutları ve onay kontrollü `mount/dismount/killhorse`, kesilebilir bekleme, satır doğrulama ve phBot-benzeri Script Creator bağlıdır. Yeni oyun komutları sunucu üzerinde doğrulanmayı bekliyor. |

### Eşya, envanter ve şehir ekonomisi

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-050 | Inventory/avatar/storage/pet/guild storage görüntüleme | ✅ | `App/Window.cs`, `Game/PacketParser.cs` |
| F-051 | Eşya kullanma, kuşanma, çıkarma, düşürme ve taşıma | ✅ | `Game/PacketBuilder.cs`, `Game/PacketParser.cs` |
| F-052 | Envanter sıralama | ✅ | `App/Bot/Bot.cs` |
| F-053 | Pet ile eşya toplama | ✅ | `App/Bot/Bot.IA.cs` |
| F-054 | Eşya pickup filtresi: SoX, cinsiyet ve açık kurallar | ✅ | `App/ItemFilterManager.cs`, `App/Bot/Bot.IA.cs`, `App/Window.CustomTabs.cs`; tüm drop türlerinde bağlı ve UI kural editörü mevcut |
| F-055 | Degree/China/Europe filtresi | ✅ | `App/ItemFilterManager.cs`, `App/ItemFilterPolicy.cs`, `App/Window.CustomTabs.cs`; pickup kararında uygulanıyor ve UI’dan ayarlanıyor |
| F-056 | Eşyayı satma/depolama ve depodan alma kuralları | 🧪 | `App/ItemFilterManager.cs`, `App/Bot/Bot.IA.cs`, `App/Window.PickFilter.cs`; kişisel/guild depo için ayrı Store ve Take sütunları, boş slot koruması, JSON kalıcılığı ve hareket başına sunucu onayı bağlıdır. Take paketlerinin oyun içi sunucu doğrulaması bekleniyor. |
| F-057 | NPC alış/satış ve buy-back paketleri | ✅ | `Game/PacketBuilder.cs`, `Game/PacketParser.cs` |
| F-058 | Otomatik Simya (+ Basma / Auto Alchemy) | ✅ | `App/AlchemyPolicy.cs`, `App/AlchemyManager.cs`, `Game/PacketBuilder.cs`, `Network/Agent.cs`; tek scheduler, sunucu cevabı bekleme, timeout, hedef item kimliği ve disconnect güvenlik kapıları |

### Party, guild, exchange, stall ve sohbet

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-060 | Oyuncu görüntüleme ve yenileme | ✅ | Players sekmesi, `InfoManager.cs` |
| F-061 | Party oluşturma, davet, ayrılma ve üye yönetimi | ✅ | `Game/Objects/Party/`, `Game/PacketBuilder.cs` |
| F-062 | Party matching ve otomatik reform | ✅ | `SRPartyMatch`, `Bot.Checks.cs`, paket katmanı |
| F-063 | Auto party / leader listesi | ✅ | Party ayarları, `Bot.Checks.cs` |
| F-064 | Guild görüntüleme, davet ve guild storage | ✅ | `Game/Objects/Guild/`, Guild sekmesi |
| F-065 | Academy daveti ve bilgisi | 🧪 | `Game/PacketBuilder.cs:InviteToAcademy`, `App/Window.cs` Players menü handler bağlı; bilinmeyen opcode logu (`Agent.cs`) ile teşhis — oyun içi doğrulama gerekli |
| F-066 | Exchange ve otomatik exchange onayları | ✅ | Players sekmesi, paket katmanı |
| F-067 | Stall oluşturma, düzenleme, kapatma ve alış | ✅ | Stall sekmesi ve `Game/PacketBuilder.cs` |
| F-068 | Sohbet kanalları ve global item kullanımı | ✅ | `Game/PacketBuilder.cs`, `Bot.Events.cs` |
| F-069 | Leader sohbet komutları | ✅ | `App/Bot/Bot.Events.cs`, README komut tablosu |
| F-070 | PING, TIME ve ISEEDEADPEOPLE komutları | ✅ | `App/Bot/Bot.Events.cs`, `CHANGELOG.md` |
| F-071 | Komuta merkezi / Emote ve Chat uzaktan yönetim | ✅ | `App/CommandCenter/CommandCenterManager.cs`, `CommandCenterPolicy.cs`, `CommandCenterForm.cs`; emote eşleme, sohbet komutları (`\start`, `\stop`, `\area`, `\buff`, `\show`, `\here`) ve koyu tema penceresi |

### Arayüz, veri ve geliştirici araçları

| ID | Özellik | Durum | Kaynak / not |
| :--- | :--- | :---: | :--- |
| F-080 | Minimap, özel kontroller ve canlı oyun bilgisi | ✅ | `xGraphics/`, `App/Window.cs` |
| F-081 | Game Info/Spy ekranı | 🧪 | `App/Window.cs`, `App/Window.CustomTabs.cs:StartGameInfoLiveTimer`; server saati 2sn canlı, ağaç manuel refresh — tam live tree için periyodik rebuild gerekli |
| F-082 | Packet analyzer: filtre, sıralama ve injection | ✅ | Settings sekmesi, `Network/`, `PacketBuilder.cs` |
| F-083 | PK2 veri çıkarma | ✅ | `PK2Extractor/` |
| F-084 | Item/skill/model/mastery/teleport/region/minimap üretimi | ✅ | `PK2Extractor/PK2Extractor.Parser.cs`, `.Media.cs` |
| F-085 | SQLite veritabanı oluşturma ve sorgulama | ✅ | `App/SQLDatabase.cs`, `Game/DataManager.cs` |
| F-086 | JSON bot/karakter ayarları ve Default profile | ✅ | `App/Settings.cs` |
| F-087 | TR/EN özel arayüz dil katmanı | ✅ | `App/LocalizationManager.cs`, `Window.CustomTabs.cs`; çift yönlü dinamik dil geçişi aktif |
| F-088 | Hızlı kaydetme ve header HP/MP/level göstergeleri | ✅ | `App/Window.CustomTabs.cs`, `App/Window.ModernTheme.cs`; canlı başlık metrikleri ve kayıt bağlı |
| F-089 | Güncelleme kontrolü ve reklam penceresi | ✅ | `App/Ads.cs`, AutoUpdater referansı |
| F-090 | Modern karanlık tema motoru ve canlı durum çubuğu | ✅ | `App/Theme/DarkTheme.cs`, `ModernSidebar`, `ModernStatusBar`, `Window.ModernTheme.cs`; CPU/RAM, Ping, Gold, SP ve durum rozeti |
| F-091 | Logout/rename akışı (srodevs-docs) | ✅ | `0x7005/0xB005/0x7006/0xB006/0x300A` logout + `0x7450/0xB450` rename parser/builder ve `Agent.cs` handler; `SroDocsPolicy` mesaj haritası |
| F-092 | Chat ack/restrict + quest script (srodevs-docs) | ✅ | `0xB025` hata kodları, `0x302D` kısıt süresi, `0x3CA2` script logu; `SroDocsPolicy.GetChatErrorMessage` |
| F-093 | EXP TC-buff + InfoUpdate tam yapısı (srodevs-docs) | ✅ | `0x3056` cumulated/accumulated + stat points, `0x304E` STP(3)/HWAN source/AP(16)/display baytları; 22 SroDocs senaryosu |
| F-094 | Gateway login/IBUV uyumu (srodevs-docs) | ✅ | `0xA102` tam hata haritası (AlreadyConnected/ServerFull/IPLimit/billing/age + block alt tipleri + custom result 0x03), `0x2322` imaj başlığı logu |
| F-095 | Teleport/party/guild/mail/drop teşhis handler’ları (srodevs-docs) | ✅ | `0xB05A/0xB059/0xB060/0xB069/0xB06A/0xB250/0x30FF/0xB309/0x305C/0x3038/0x3091/0x304D` parser + `Agent.cs` bağlantısı; oyun akışı polling’de kalır |
| F-096 | Ortam saati ve hava durumu (srodevs-docs) | ✅ | `0x3020/0x3027` moonphase/saat/dakika + `0x3809` weather `InfoManager`’da tutulur (`GameHour/Minute/Moonphase/WeatherType/Intensity`) |
| F-097 | Gateway notice/ping/patch + download stub (srodevs-docs) | ✅ | `0xA104` notice, `0xA106` ping, `0xA100` PatchErrorCode + dosya listesi, `0x6004/0x1001/0xA004` download stub; `RequestNotice/RequestShardListPing/RestoreCharacter` builder |
| F-098 | Auth/patch/petition/weather politika haritası (srodevs-docs) | ✅ | `0xA103` auth hata kodları (C9/C10/full/IP), patch hata adları, petition GuildWar(10)/Resurrection(8), `SroDocsPolicy` + 32 senaryo |
| F-099 | Quest Automation v1 | 🧪 | `App/QuestAutomationManager.cs`, `QuestAutomationPolicy.cs`, modern Görevler paneli, karakter profili `QuestAutomation`; aktif liste, enable/disable, abandon, tamamlanınca return/script ve event/So-Ok ödülü bağlı. RefQuest kataloğu ile normal kabul/teslim v2 kapsamında. |
| F-100 | Quest Automation v2 — görev yaşam döngüsü | 🧪 | Tek NPC konuşması; `0x30D5` add/remove ile kabul/teslim onayı, 12 sn timeout ve en fazla 3 deneme, kontrollü tekrar kabul. PK2 `npcpos.txt` + görev metniyle genel NPC keşfi; silah türü/ad/kullanıcı ödül tercihi; açık UI durumları. [Doğrulama ve kalan canlı kontroller](QUEST_AUTOMATION_VALIDATION.md). PK2 DB yeniden oluşturulmalı. |

## Geliştirilmekte olan yöneticiler

Bu bölüm, çalışma ağacında yeni görünen yöneticileri ayrı izler. Yeni özellik eklenirken ilgili manager, bot çağrı noktası, UI ve JSON ayarı birlikte kontrol edilmelidir.

| Yönetici | Sorumluluk | JSON bölümü | Mevcut bağlantı | Sonraki kontrol |
| :--- | :--- | :--- | :--- | :--- |
| `LoginStrategyManager` | Otomatik giriş, server/karakter seçimi, bekleme, otomatik başlatma/gizleme | `LoginStrategy` | Client ve Clientless akışlarında UI alanları, `PacketParser` ve `Bot.Events` ile bağlı | Gerçek server bağlantısı ve captcha ile runtime test |
| `CombatAIEngine` | Hedef önceliği, kaçınma ve berserk tetikleri | `CombatAI` | `Bot.IA` hedef seçiminde bağlı; arayüz `Kasılma > Combat AI` altında | Her kural için UI + oyun içi test |
| `SkillManager` | Imbue skill seçimi, Devil Spirit, support/no-attack, beceri sırası ve fallback | `SkillManager` | Bot döngüsünde bağlı; algılanan imbue’ler seviyeleriyle Skills > Attack ekranında görünür | Oyun içi skill/weapon senaryolarıyla doğrula |
| `ProtectionManager` | Skill iyileştirme, pet koruması, şehir dönüş tetikleri | `ProtectionManager` | Her koruma kontrolü merkezi bot tick’inde; `ProtectionPolicy` karar katmanı kullanılıyor | Oyun içi gerçek client senaryolarıyla doğrula |
| `StatPointManager` | Level sonrası stat dağıtımı | `StatPointManager` | Level-up/InfoManager’a bağlı | STR/INT hedef doğrulaması |
| `ItemFilterManager` | Pickup/sell/store kuralları ve item kriterleri | `ItemFilterManager` | Pickup ve şehir lojistiğinin tamamına bağlı | Gerçek client item çeşitleriyle runtime doğrulama |
| `LocalizationManager` | Yeni özel kontroller için TR/EN metinleri | Yok | Özel UI başlatılırken bağlı | Dil anahtarları temizlendi |
| `CommandCenterManager` | Emote ve chat ile uzaktan bot kontrolü | `CommandCenter` | Paket dinleme, `Agent.cs`, bağımsız form ve 18 testle bağlı | Gerçek party chat/emote senaryolarıyla doğrula |
| `AccountManager` | Çoklu hesap kaydetme, profil yönetimi | `Accounts` | Git dışındaki `Settings.user.json`, DPAPI `SecretStore` ve Giriş sekmesi ile bağlı | Çoklu istemci geçişi testi |
| `PartySupportManager` | Parti üyelerini iyileştirme, canlandırma, debuff silme | `PartySupport` | `Bot.IA` döngüsünde bağlı; `PartyPolicy` karar katmanı ve 18 testle doğrulandı | Oyun içi party senaryolarıyla doğrula |
| `SecondaryPasscodePolicy` | İkincil güvenlik şifresi (PIN) kontrolü, hesap fallback çözümleme | `LoginStrategy` | `PacketBuilder`, `PacketParser` ve `Bot.Events` ile bağlı; 14 testle doğrulandı | Sunucu PIN ekranında runtime test |
| `Socks5Config` | Gateway ve Agent bağlantılarını SOCKS5 tüneline yönlendirme | `Socks5Proxy` | `Proxy.cs`, `Socks5Handler.cs` ve `Socks5Policy` ile bağlı; 14 testle doğrulandı | Proxy IP üzerinden gateway bağlantısı testi |
| `AlchemyManager` | Otomatik simya (+ basma), slot ve elixir/powder eşleme, güvenlik limitleri | `Alchemy` | `Bot.IA` döngüsünde bağlı; `AlchemyPolicy` karar katmanı ve 25 bağımsız testle doğrulandı | Oyun içi simya ve elixir tüketimi ile runtime test |
| `SroDocsPolicy` | srodevs-docs mesaj/karar haritası (login/auth/logout/chat/rename/patch/petition/weather/angle) | Yok | `PacketParser`, `Agent.cs`, `Proxy.cs`, `Gateway.cs` ile bağlı; 32 bağımsız testle doğrulandı | Gerçek server paketleriyle runtime test |
| `QuestAutomationManager` | Aktif veya katalogdaki görev kuralları, konuşma gözlemi ve tamamlanma aksiyonları | `QuestAutomation` | `0x30D5` parser, `0x30D4` tip 4/5 diyalog parser'ı, C→S seçim kaydı, PK2 görev/NPC kataloğu, bot thread kuyruğu ve Görevler paneline bağlı | Gerçek sunucuda normal kabul/teslim seçimlerinin eşleştirilmesi |

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
