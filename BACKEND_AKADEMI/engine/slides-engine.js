/*
  AMAÇ: Bir bölüm dosyasının (window.MODULE objesi) tam ekran slayt sunumuna dönüştürülmesi.
  NEDEN: docs/API_YOL_HARITASI/render.js akordiyon (hepsi tek sayfada açılır/kapanır) üretiyordu;
         bu araç "bir konsept = bir ekran, ileri/geri ile gez" ilkesiyle çalışır çünkü hedef kitle
         (junior'dan bile acemi) tek seferde tek şeye odaklanmalı, sayfada kaybolmamalı.
  BAĞIMLILIKLAR: slides.css (stiller). Sayfada global `MODULE` objesi tanımlı olmalı.

  KULLANIM: Bölüm HTML'i `window.MODULE = {...}` tanımlar, sonra bu dosyayı <body> sonunda yükler.
*/
(function () {
  // AMAÇ: Kullanıcıdan/koddan gelen metni HTML'e basmadan önce escape eder.
  // NEDEN: `kod` alanları gerçek C# kaynak kodu taşıyor (`<`, `>`, `&` — ör. Repository<T>);
  //        escape edilmezse hem HTML bozulur hem de (teorik) XSS riski oluşur.
  const esc = (s) => String(s == null ? '' : s)
    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');

  if (typeof MODULE === 'undefined' || !MODULE || !Array.isArray(MODULE.slaytlar) || !MODULE.slaytlar.length) {
    return; // NEDEN: MODULE tanımsızsa (ör. dosya doğrudan test ediliyor) motor sessizce hiçbir şey yapmaz.
  }

  const slides = MODULE.slaytlar;
  const total = slides.length;
  let current = 0;
  let tocOpen = false;

  // AMAÇ: Bir slaytın "İçindekiler" panelinde görünecek kısa başlığı.
  function slideLabel(s, i) {
    if (s.tur === 'kapak') return s.baslik || `Slayt ${i + 1}`;
    return s.baslik || `Slayt ${i + 1}`;
  }

  // AMAÇ: `kod` alanındaki her satırı, `satirlar[]` içinde eşleşen bir açıklaması varsa
  //       tıklanabilir bir <span> olarak işaretler.
  // NEDEN: Eski render.js yalnızca ##NEW##/##OLD## ile "hangi satır değişti"yi gösteriyordu;
  //        burada HER önemli satırın kendi neden/olmasaydı açıklaması var — okuyucu satıra
  //        tıklayınca o satıra özel açıklama panelde açılmalı (satır ↔ açıklama senkron).
  function renderKodBlock(kod, satirlar) {
    const lines = String(kod == null ? '' : kod).replace(/\n$/, '').split('\n');
    const bySatir = new Map();
    (satirlar || []).forEach((s, idx) => {
      if (s.satir) bySatir.set(s.satir.trim(), idx);
    });
    return lines
      .map((line) => {
        const trimmed = line.trim();
        const idx = bySatir.has(trimmed) ? bySatir.get(trimmed) : -1;
        const escaped = esc(line);
        if (idx === -1) return `<span class="src-line">${escaped || ' '}</span>`;
        return `<span class="src-line src-line-annotated" data-idx="${idx}" tabindex="0">${escaped}</span>`;
      })
      .join('\n');
  }

  function renderAnnotationPanel(satirlar, activeIdx) {
    if (!satirlar || !satirlar.length) return '';
    if (activeIdx == null || activeIdx < 0) {
      return `<div class="annotation-panel annotation-empty">← Soldaki kodda <b>renkli</b> bir satıra tıkla, o satırın neden öyle yazıldığını buradan oku.</div>`;
    }
    const s = satirlar[activeIdx];
    return `
      <div class="annotation-panel">
        <div class="annotation-line"><code>${esc(s.satir)}</code></div>
        <div class="annotation-block"><div class="annotation-tag">Ne yapıyor</div><div>${esc(s.aciklama)}</div></div>
        ${s.neden ? `<div class="annotation-block annotation-neden"><div class="annotation-tag">Neden böyle</div><div>${esc(s.neden)}</div></div>` : ''}
        ${s.olmasaydi ? `<div class="annotation-block annotation-olmasaydi"><div class="annotation-tag">Böyle olmasaydı</div><div>${esc(s.olmasaydi)}</div></div>` : ''}
      </div>`;
  }

  function renderKapak(s) {
    return `
      <div class="slide slide-kapak">
        ${s.ustBaslik ? `<div class="kapak-ust">${esc(s.ustBaslik)}</div>` : ''}
        <h1 class="kapak-baslik">${esc(s.baslik)}</h1>
        ${s.altBaslik ? `<p class="kapak-alt">${esc(s.altBaslik)}</p>` : ''}
      </div>`;
  }

  function renderKavram(s) {
    return `
      <div class="slide slide-kavram">
        <h2 class="slide-baslik">${esc(s.baslik)}</h2>
        <div class="kavram-aciklama">${esc(s.aciklama)}</div>
        <div class="kavram-grid">
          ${s.neden ? `<div class="kavram-kart kavram-neden"><div class="kavram-etiket">NEDEN</div><div>${esc(s.neden)}</div></div>` : ''}
          ${s.olmasaydi ? `<div class="kavram-kart kavram-olmasaydi"><div class="kavram-etiket">BÖYLE OLMASAYDI</div><div>${esc(s.olmasaydi)}</div></div>` : ''}
        </div>
      </div>`;
  }

  function renderKod(s, activeIdx) {
    return `
      <div class="slide slide-kod">
        <div class="kod-head">
          <h2 class="slide-baslik">${esc(s.baslik)}</h2>
          ${s.dosyaYolu ? `<div class="kod-dosya-yolu">📄 ${esc(s.dosyaYolu)}</div>` : ''}
        </div>
        ${s.nedenBuKlasor ? `<div class="kod-neden-klasor"><b>Neden bu klasörde/isimde:</b> ${esc(s.nedenBuKlasor)}</div>` : ''}
        <div class="kod-split">
          <pre class="kod-pre"><code id="kodBlock">${renderKodBlock(s.kod, s.satirlar)}</code></pre>
          <div id="annotationHost">${renderAnnotationPanel(s.satirlar, activeIdx)}</div>
        </div>
      </div>`;
  }

  function renderKarsilastirma(s) {
    const kol = (k, cls, etiket) => k ? `
      <div class="karsilastirma-kolon ${cls}">
        <div class="karsilastirma-etiket">${esc(etiket)}</div>
        ${k.baslik ? `<div class="karsilastirma-kolon-baslik">${esc(k.baslik)}</div>` : ''}
        <ul>${(k.maddeler || []).map((m) => `<li>${esc(m)}</li>`).join('')}</ul>
      </div>` : '';
    return `
      <div class="slide slide-karsilastirma">
        <h2 class="slide-baslik">${esc(s.baslik)}</h2>
        <div class="karsilastirma-grid">
          ${kol(s.iyi, 'karsilastirma-iyi', '✓ Doğru')}
          ${kol(s.kotu, 'karsilastirma-kotu', '✗ Yanlış')}
        </div>
      </div>`;
  }

  function renderSozluk(s) {
    return `
      <div class="slide slide-sozluk">
        <h2 class="slide-baslik">${esc(s.baslik)}</h2>
        <div class="sozluk-grid">
          ${(s.terimler || []).map((t) => `
            <div class="sozluk-kart">
              <div class="sozluk-terim">${esc(t.terim)}</div>
              <div class="sozluk-tanim">${esc(t.tanim)}</div>
            </div>`).join('')}
        </div>
      </div>`;
  }

  function renderOzet(s) {
    return `
      <div class="slide slide-ozet">
        <h2 class="slide-baslik">${esc(s.baslik)}</h2>
        <ul class="ozet-liste">
          ${(s.maddeler || []).map((m) => `<li>${esc(m)}</li>`).join('')}
        </ul>
      </div>`;
  }

  const RENDERERS = {
    kapak: renderKapak,
    kavram: renderKavram,
    kod: renderKod,
    karsilastirma: renderKarsilastirma,
    sozluk: renderSozluk,
    ozet: renderOzet,
  };

  // AMAÇ: Geçerli slaytı sahneye basar, üst çubuğu (sayaç/progress) ve nav butonlarını günceller.
  function renderCurrent(activeAnnotationIdx) {
    const stage = document.getElementById('stage');
    const s = slides[current];
    const renderer = RENDERERS[s.tur] || renderKavram;
    stage.innerHTML = renderer(s, activeAnnotationIdx);
    stage.className = `stage stage-${s.tur}`;

    document.getElementById('counter').textContent = `${current + 1} / ${total}`;
    document.getElementById('progressFill').style.width = `${((current + 1) / total) * 100}%`;

    const prevBtn = document.getElementById('prevBtn');
    const nextBtn = document.getElementById('nextBtn');
    const atFirst = current === 0;
    const atLast = current === total - 1;
    prevBtn.disabled = atFirst && !MODULE.oncekiBolum;
    prevBtn.textContent = atFirst && MODULE.oncekiBolum ? '‹ Önceki Bölüm' : '‹';
    nextBtn.disabled = atLast && !MODULE.sonrakiBolum;
    nextBtn.textContent = atLast && MODULE.sonrakiBolum ? 'Sonraki Bölüm ›' : '›';

    if (s.tur === 'kod' && s.satirlar && s.satirlar.length) {
      document.querySelectorAll('.src-line-annotated').forEach((el) => {
        el.addEventListener('click', () => {
          const idx = Number(el.getAttribute('data-idx'));
          renderCurrent(idx);
        });
      });
    }

    document.title = `${MODULE.bolumBaslik || ''} — ${s.baslik || ''}`.trim();
  }

  // AMAÇ: Belirtilen indexe git; URL hash'ini günceller (derin link + tarayıcı geri/ileri).
  function goTo(i) {
    if (i < 0) {
      if (MODULE.oncekiBolum) { window.location.href = MODULE.oncekiBolum; }
      return;
    }
    if (i >= total) {
      if (MODULE.sonrakiBolum) { window.location.href = MODULE.sonrakiBolum; }
      return;
    }
    current = i;
    window.location.hash = String(i + 1);
    renderCurrent();
  }

  function toggleToc() {
    tocOpen = !tocOpen;
    const overlay = document.getElementById('tocOverlay');
    overlay.classList.toggle('open', tocOpen);
    if (tocOpen) {
      overlay.innerHTML = `
        <div class="toc-panel">
          <div class="toc-panel-head">İçindekiler <button id="tocClose">✕</button></div>
          <ol class="toc-list">
            ${slides.map((s, i) => `<li class="${i === current ? 'toc-active' : ''}" data-idx="${i}">${esc(slideLabel(s, i))}</li>`).join('')}
          </ol>
        </div>`;
      document.getElementById('tocClose').addEventListener('click', toggleToc);
      overlay.querySelectorAll('.toc-list li').forEach((li) => {
        li.addEventListener('click', () => {
          goTo(Number(li.getAttribute('data-idx')));
          toggleToc();
        });
      });
    }
  }

  function init() {
    const app = document.getElementById('app');
    app.innerHTML = `
      <div class="topbar">
        <div class="topbar-left">
          <a class="topbar-back" href="../index.html">← Akademi</a>
          ${MODULE.bolumBaslik ? `<span class="topbar-sep">/</span><a class="topbar-back" href="index.html">${esc(MODULE.bolumBaslik.split('—')[0].trim())}</a>` : ''}
        </div>
        <div class="progress-track"><div id="progressFill" class="progress-fill"></div></div>
        <div class="topbar-right">
          <button id="tocBtn" class="toc-btn">☰ İçindekiler</button>
          <span id="counter" class="counter"></span>
        </div>
      </div>
      <div id="stage" class="stage"></div>
      <div class="navbar">
        <button id="prevBtn" class="navbtn">‹</button>
        <button id="nextBtn" class="navbtn">›</button>
      </div>
      <div id="tocOverlay" class="toc-overlay"></div>`;

    document.getElementById('prevBtn').addEventListener('click', () => goTo(current - 1));
    document.getElementById('nextBtn').addEventListener('click', () => goTo(current + 1));
    document.getElementById('tocBtn').addEventListener('click', toggleToc);

    document.addEventListener('keydown', (e) => {
      if (e.key === 'ArrowRight' || e.key === ' ') { e.preventDefault(); goTo(current + 1); }
      else if (e.key === 'ArrowLeft') { e.preventDefault(); goTo(current - 1); }
      else if (e.key === 'o' || e.key === 'O') { toggleToc(); }
      else if (e.key === 'Escape' && tocOpen) { toggleToc(); }
    });

    // NEDEN: Sayfa hash ile açılırsa (`#7`) veya kullanıcı tarayıcı geri/ileri tuşuna basarsa
    //        kalınan slaytta kalınmalı — hash tek gerçek kaynak (source of truth).
    window.addEventListener('hashchange', () => {
      const n = parseInt(window.location.hash.replace('#', ''), 10);
      if (!isNaN(n) && n >= 1 && n <= total) { current = n - 1; renderCurrent(); }
    });

    const initial = parseInt(window.location.hash.replace('#', ''), 10);
    current = (!isNaN(initial) && initial >= 1 && initial <= total) ? initial - 1 : 0;
    renderCurrent();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
