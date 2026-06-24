/*
  AMAÇ: Tek bir API sayfasını (window.API objesi) HTML'e dönüştüren paylaşılan render motoru.
  NEDEN: Her API HTML'i yalnızca veri objesi içersin; render mantığı tek yerde dursun (DRY).
  BAĞIMLILIKLAR: style.css (stiller). Sayfada global `API` objesi tanımlı olmalı.

  KULLANIM: API HTML dosyası `const API = {...}` tanımlar, sonra bu dosyayı yükler.
*/
(function () {
  const esc = (s) => String(s == null ? '' : s)
    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
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
          <pre><code>${esc(a.kod)}</code></pre>
        </div>
      </div>`;
    });

    el.innerHTML = html;
  }

  if (window.API) render(window.API);
})();
