/*
  AMAÇ: Tek bir API sayfasını (window.API objesi) HTML'e dönüştüren paylaşılan render motoru.
  NEDEN: Her API HTML'i yalnızca veri objesi içersin; render mantığı tek yerde dursun (DRY).
  BAĞIMLILIKLAR: style.css (stiller). Sayfada global `API` objesi tanımlı olmalı.

  KULLANIM: API HTML dosyası `const API = {...}` tanımlar, sonra bu dosyayı yükler.
*/
(function () {
  const esc = (s) => String(s == null ? '' : s)
    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

  // AMAÇ: Bir kod bloğunun satır satır işlenip HTML'e çevrilmesi (git diff benzeri vurgulama).
  // NEDEN: "Yeniden kullanılan kod tekrar yazılır" kuralı gereği reused bir dosyanın (ör.
  //        WordLearnerDbContext.cs) TAM hâli her API sayfasında tekrar gösterilir — ama okuyucu
  //        "bu API için asıl değişen satır hangisi?" sorusunu satırlarca eski koddan ayırt edemez.
  //        Kod string'inin ham hâlinde (escape'ten ÖNCE) satır başına iki marker konabilir:
  //        `##NEW##` = bu API'de eklenen satır (yeşil), `##OLD##` = bu API'de kaldırılan/değişen
  //        eski satır (kırmızı + üstü çizili — dosyada artık YOK, yalnızca "neyin yerine geçtiğini"
  //        göstermek için burada tutuluyor). Marker'lar satırdan sökülüp farklı renkte gösterilir.
  //        esc() önce çalışır (HTML injection riski yok), marker'lar ASCII olduğu için escape'ten etkilenmez.
  const NEW_MARKER = '##NEW##';
  const OLD_MARKER = '##OLD##';
  function renderKod(rawKod) {
    return String(rawKod == null ? '' : rawKod)
      .split('\n')
      .map((rawLine) => {
        const isNew = rawLine.startsWith(NEW_MARKER);
        const isOld = !isNew && rawLine.startsWith(OLD_MARKER);
        const marker = isNew ? NEW_MARKER : isOld ? OLD_MARKER : '';
        const rawContent = marker ? rawLine.slice(marker.length) : rawLine;
        let line = esc(rawContent).replace(/^([ \t]*\/\/.*)$/, '<span class="cmt">$1</span>');
        if (isNew) return `<span class="line-new">${line}</span>`;
        if (isOld) return `<span class="line-old">${line}</span>`;
        return line;
      })
      .join('\n');
  }
  const mClass = (m) => 'm-' + String(m || 'get').toLowerCase();

  // AMAÇ: `adim.grup` metnini anchor id'sine çevirir (İçindekiler linki ↔ bölüm başlığı eşleşsin).
  // NEDEN: Türkçe karakterler/boşluklar ham haliyle geçerli bir HTML id değil; dönüşüm kusurlu olsa
  //        bile TEK bir fonksiyondan geçtiği için üretici ve tüketici id'si her zaman birebir eşleşir.
  const slug = (s) => String(s || '')
    .toLowerCase()
    .replace(/[ığüşöç]/g, (c) => ({ ı: 'i', ğ: 'g', ü: 'u', ş: 's', ö: 'o', ç: 'c' }[c]))
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');

  // AMAÇ: API objesini #content içine bas.
  function render(api) {
    const el = document.getElementById('content');
    if (!api || !el) return;

    document.title = `${api.metot} ${api.yol} — API Yol Haritası`;

    let html = `
      <div class="api-head">
        <span class="method ${mClass(api.metot)}" style="font-size:.72rem">${esc(api.metot)}</span>
        <span class="api-path">${esc(api.yol)}</span>
        <span class="badge">${esc(api.auth)}</span>
        <span class="badge">${esc(api.faz)}</span>
      </div>
      <h1 style="margin-top:.3rem">${esc(api.baslik)}</h1>
      <p class="lead">${esc(api.ozet)}</p>`;

    // AMAÇ: Bu API için yeni eklenen NuGet paketlerini (varsa) adımlardan önce listeler.
    // NEDEN: Her API tek başına baştan sona okunabilir/kurulabilir olmalı — okuyucu kodu
    //        çalıştırmadan önce hangi paketleri eklemesi gerektiğini görmeli (TASK.md kuralı).
    if ((api.paketler || []).length) {
      html += `<div class="note"><b>📦 Bu API için eklenen paketler:</b><ul class="xref-list">`;
      api.paketler.forEach((p) => {
        html += `<li><code>${esc(p.paket)}</code> <span class="badge">${esc(p.versiyon)}</span> <span class="badge">${esc(p.proje)}</span> — ${esc(p.neden)}</li>`;
      });
      html += `</ul></div>`;
    }

    // AMAÇ: `adim.grup` alanı doluysa adımları görsel bölümlere ayırır — büyük API'larda
    //       (A-03'ün 60+ adımı gibi) düz akordiyon listesi yerine gezilebilir bir yapı sağlar.
    //       `api.katmanlar` (opsiyonel, [{ad, gruplar:[...]}]) verilirse gruplar bir üst seviyede
    //       mimari katmana göre de toplanır (ör. "Application — Servisler" → 7 grup).
    // NEDEN: `kod` alanına ASLA dokunmaz (hiçbir adım kırpılmaz/gizlenmez) — yalnızca adımların
    //        ÜSTÜNE bölüm/katman başlığı eklenir; `grup`/`katmanlar` verilmezse davranış eskisiyle
    //        birebir aynıdır (küçük API'lar, ör. A-02, hiç grup kullanmayabilir).
    // NEDEN adım'a değil API'ye `katmanlar`: bir katman birden çok grubu kapsar (grup zaten adım
    //       başına tekrar ediyor) — katmanı da adım başına yazmak 64 satırı tekrar değiştirmek
    //       demekti; bunun yerine grup adı → katman adı eşlemesi TEK bir yerde (üstte) tutulur.
    const gruplar = [];
    (api.adimlar || []).forEach((a) => {
      if (a.grup && !gruplar.includes(a.grup)) gruplar.push(a.grup);
    });
    const grupToKatman = {};
    (api.katmanlar || []).forEach((k) => {
      (k.gruplar || []).forEach((g) => {
        grupToKatman[g] = k.ad;
      });
    });
    if (gruplar.length > 1) {
      html += `<div class="toc"><b>İçindekiler</b>`;
      if ((api.katmanlar || []).length) {
        html += `<div class="toc-katmanlar">`;
        api.katmanlar.forEach((k) => {
          const buradakiGruplar = (k.gruplar || []).filter((g) => gruplar.includes(g));
          if (!buradakiGruplar.length) return;
          html += `<div class="toc-katman"><div class="toc-katman-ad">${esc(k.ad)}</div><ul>`;
          buradakiGruplar.forEach((g) => {
            const sayi = (api.adimlar || []).filter((a) => a.grup === g).length;
            html += `<li><a href="#grup-${slug(g)}">${esc(g)}</a> <span class="toc-count">${sayi}</span></li>`;
          });
          html += `</ul></div>`;
        });
        html += `</div>`;
      } else {
        html += `<ul>`;
        gruplar.forEach((g) => {
          const sayi = (api.adimlar || []).filter((a) => a.grup === g).length;
          html += `<li><a href="#grup-${slug(g)}">${esc(g)}</a> <span class="toc-count">${sayi}</span></li>`;
        });
        html += `</ul>`;
      }
      html += `</div>`;
    }

    let sonGrup = null;
    let sonKatman = null;
    (api.adimlar || []).forEach((a, i) => {
      const open = i === 0 ? ' open' : '';
      if (a.grup && a.grup !== sonGrup) {
        const katman = grupToKatman[a.grup];
        if (katman && katman !== sonKatman) {
          html += `<div class="step-katman" id="katman-${slug(katman)}">${esc(katman)}</div>`;
          sonKatman = katman;
        }
        html += `<div class="step-group" id="grup-${slug(a.grup)}">${esc(a.grup)}</div>`;
        sonGrup = a.grup;
      }
      html += `
      <div class="step">
        <div class="step-head" onclick="this.nextElementSibling.classList.toggle('open')">
          <span class="step-num">${esc(a.num != null ? a.num : i + 1)}</span>
          <div>
            <div class="step-title">${esc(a.baslik)}</div>
            <div class="step-file">${esc(a.dosya)}</div>
          </div>
          <span class="step-tag t-${esc(a.tur)}">${esc(a.tur)}</span>
        </div>
        <div class="step-body${open}">
          <div class="desc">${esc(a.aciklama)}</div>
          <pre><code>${renderKod(a.kod)}</code></pre>
        </div>
      </div>`;
    });

    // AMAÇ: Aynı katmandaki (backend↔backend) kardeş API sayfalarına çapraz link — ör. büyük bir
    //       task alt-task'lara bölündüğünde (A-03 ↔ A-03.1) her ikisi kendi dosyasında kalır ama
    //       birbirine bağlanır.
    // NEDEN ayrı dosya: `frontendRefs` katmanlar arası (backend→frontend) geçişi anlatır; bu ise
    //       aynı katmanda kardeş sayfa geçişidir — farklı bir ok/etiket ile karışması önlenir.
    if ((api.relatedRefs || []).length) {
      html += `<div class="note xref"><b>🔗 İlgili API'lar:</b><ul class="xref-list">`;
      api.relatedRefs.forEach((r) => {
        html += `<li><a href="${esc(r.dosya)}">${esc(r.baslik)}</a></li>`;
      });
      html += `</ul></div>`;
    }

    // AMAÇ: Bu API'yi tüketen frontend feature sayfalarına çapraz link.
    // NEDEN: Backend işi burada biter; devamı (bu endpoint'i çağıran component/ekran) Frontend Yol
    //        Haritası'nda anlatılır — okuyucu buradan doğrudan oraya geçebilmeli.
    if ((api.frontendRefs || []).length) {
      html += `<div class="note xref"><b>🧩 Buradan sonrası frontend tarafında:</b><ul class="xref-list">`;
      api.frontendRefs.forEach((r) => {
        html += `<li><a href="${esc(r.dosya)}">${esc(r.baslik)}</a></li>`;
      });
      html += `</ul></div>`;
    }

    el.innerHTML = html;
  }

  // NEDEN: `const API` top-level olsa bile window.API'ye atanmaz (ES6 lexical scope);
  // typeof guard ile her iki tanım biçimi de (const/var/window.API) çalışır.
  if (typeof API !== 'undefined') render(API); // eslint-disable-line no-undef
})();
