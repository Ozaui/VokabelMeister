# Tasarım Sistemi (Admin Panel)

**Özet:** Admin panel (Faz B) için "Menekşe + Mercan" renk paleti (Primary `#6D5DFC` + Accent `#FB923C`), Nunito/DM Sans tipografi ikilisi, rounded (keskin köşesiz) + tek katmanlı yumuşak gölge stil kuralları ve sıkı bir ikon-minimalizmi kuralı (dekoratif ikon yok, eksik görsel yerine placeholder — türetilmiş ikon/illüstrasyon yok). **Yalnızca tasarım kararı — B-01 (admin panel kurulumu) henüz yapılmadığı için hiçbir Tailwind config'ine/koda işlenmedi.** Web/Mobil kendi tasarım kararlarını ayrı alacak.
**Kütüphaneler:** — (henüz kod yok; ileride TailwindCSS + Google Fonts Nunito/DM Sans)
**Bağlantılar:** [[Roller_ve_Erisim]] · [[Icerik_Domain]] (Category.Icon/Color alanları) · [[Gelistirme_Yol_Haritasi]]

## Kaynak
Tam içerik → `docs/REFERENCE/DESIGN_SYSTEM.md`.

## Kararın Kökeni
Kullanıcı admin panel tasarımını başka bir agent'a (görsel mockup üretecek) yaptırmadan önce, bu
sohbette renk paleti/stil yönünü netleştirdi: modern+sade+"tatlı"/yumuşak (rounded, keskin köşe
yok), renk yalnızca anlamlı vurgu için, mobil uyumlu. Üç palet seçeneği sunuldu (Menekşe+Mercan /
Deniz Mavisi+Nane / Lavanta+Yeşil), kullanıcı **Menekşe + Mercan**'ı seçti. Ayrıca tasarım
agent'larının yaygın bir kötü alışkanlığı — eksik görsel/fotoğraf yerine ikon türetmek veya elle
çizmek — kullanıcı tarafından açıkça yasaklandı, bu yüzden "İkon kullanımı — sıkı kural" maddesi
eklendi (bkz. `DESIGN_SYSTEM.md` §3). `Categories.Icon`/`Categories.Color` alanlarının gerçekten
var olduğu [[Icerik_Domain]] şemasından doğrulandı — kategori ikonu bu kurala istisna, çünkü veride
karşılığı olan gerçek bir alan (uydurma değil).

## Kategori Hiyerarşisi Doğrulaması (aynı sohbette)
Kullanıcı bir mockup ekran görüntüsünde 3 seviyeli kategori ağacı (üst→alt→alt-alt) gördü,
"sistemimiz buna uygun mu" diye sordu. `Categories.ParentCategoryId` self-referencing FK olduğu
için (bkz. [[Icerik_Domain]]) sınırsız derinlik destekleniyor — mockup mimariyle uyumlu. Tek
uyuşmayan nokta: mockup'ta "TEMEL"/"ORTA SEVİYE" gibi insan-dostu etiketler vardı, oysa şemada
seviye yalnızca CEFR kodları (`A1..C2`, `Categories.MinLevel/MaxLevel`) olarak tutuluyor — bu bir
görsel-katman mapping kararı olarak kullanıcıya bırakıldı, henüz karar verilmedi/kodlanmadı.
