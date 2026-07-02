/*
  AMAÇ: Tek bir frontend feature sayfasını (window.FEATURE objesi) HTML'e dönüştüren render motoru.
  NEDEN: API_YOL_HARITASI/render.js ile aynı mantık, frontend alan adlarına (uygulama/tip) uyarlandı;
         backend motoruna dokunulmadan bağımsız kopya olarak tutuldu (iki sistem birbirini etkilemesin).
  BAĞIMLILIKLAR: ../API_YOL_HARITASI/style.css (stiller paylaşılır). Sayfada global `FEATURE` objesi tanımlı olmalı.

  KULLANIM: Feature HTML dosyası `window.FEATURE = {...}` tanımlar, sonra bu dosyayı yükler.
*/
(function () {
  const esc = (s) => String(s == null ? '' : s)
    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

  // NEDEN: Kod bloklarında // ile başlayan yorum satırlarını yeşil span'e sarar;
  //        esc() sonrası çalışır, dolayısıyla HTML injection riski yoktur.
  const hlCode = (s) => s.replace(/^([ \t]*\/\/.*)$/gm, '<span class="cmt">$1</span>');
  const uClass = (u) => 'u-' + String(u || 'web').toLowerCase();

  // AMAÇ: FEATURE objesini #content içine bas.
  function render(feature) {
    const el = document.getElementById('content');
    if (!feature || !el) return;

    document.title = `${feature.baslik} — Frontend Yol Haritası`;

    let html = `
      <div class="api-head">
        <span class="method ${uClass(feature.uygulama)}" style="font-size:.72rem">${esc((feature.uygulama || '').toUpperCase())}</span>
        <span class="api-path">${esc(feature.tip)}</span>
        <span class="badge">${esc(feature.faz)}</span>
      </div>
      <h1 style="margin-top:.3rem">${esc(feature.baslik)}</h1>
      <p class="lead">${esc(feature.ozet)}</p>`;

    (feature.adimlar || []).forEach((a, i) => {
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
          <pre><code>${hlCode(esc(a.kod))}</code></pre>
        </div>
      </div>`;

      // AMAÇ: `tur:'api'` adımı, backend'e istek atılan tam nokta — buradan sonrası backend
      //        tarafındadır. NEDEN: Okuyucu bu endpoint'in gerçek implementasyonunu (entity→
      //        controller) API Yol Haritası'nda görebilmeli; adımın hemen altında gösterilir.
      if (a.tur === 'api' && a.backendRef) {
        html += `<div class="note xref"><b>⚙️ Buradan sonrası backend tarafında:</b>
          <a href="${esc(a.backendRef.dosya)}">${esc(a.backendRef.baslik)}</a></div>`;
      }
    });

    el.innerHTML = html;
  }

  // NEDEN: `const FEATURE` top-level olsa bile window.FEATURE'a atanmaz (ES6 lexical scope);
  // typeof guard ile her iki tanım biçimi de (const/var/window.FEATURE) çalışır.
  if (typeof FEATURE !== 'undefined') render(FEATURE); // eslint-disable-line no-undef
})();
