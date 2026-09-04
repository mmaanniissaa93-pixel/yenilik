# Özellik kaydı şablonu

`docs/FEATURES.md` içine yeni özellik eklerken aşağıdaki bilgileri doldur.

```md
| F-xxx | Özelliğin kısa adı | 📌 Planlandı | `Kaynak/Dosya.cs`; ilgili sekme |
```

Detay gerekiyorsa:

```md
### F-xxx — Özellik adı

- Durum: 📌 Planlandı
- Amaç: Kullanıcıya sağladığı davranış
- Arayüz: İlgili sekme, buton veya kontrol
- Kod: `xBot/App/...cs`
- Ayar/JSON: `Settings.json` veya `Config/...json` içindeki anahtar
- Bağımlılıklar: Gerekli manager, paket veya veri dosyaları
- Doğrulama: Uygulanan test/senaryo
- Açık noktalar: Bilinen eksikler veya sürüm/sunucu kısıtları
```

Durum akışı:

`📌 Planlandı` → `🧪 Deneysel` → `✅ Kullanımda`

Bir özelliğin yalnızca bir parçası bağlıysa `⚠️ Kısmi` kullan ve eksik bağlantıyı “Açık noktalar” alanında yaz. Bilinçli olarak kullanılmayan eski bir ekran/işlev için `⏸️ Devre dışı` durumunu kullan.

Her tamamlanan özellik için:

1. `FEATURES.md` durumunu güncelle.
2. Kaynak ve JSON anahtarlarını doğrula.
3. Ayarların kaydedilip geri yüklendiğini kontrol et.
4. Kullanım değiştiyse README’ye ekle.
5. Sürüm notunu `CHANGELOG.md` içine yaz.
