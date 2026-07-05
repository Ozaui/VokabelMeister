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

    (api.adimlar || []).forEach((a, i) => {
      const open = i === 0 ? ' open' : '';
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
