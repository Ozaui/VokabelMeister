#!/usr/bin/env node
// AMAÇ: Bir akademi bölüm dosyasının (window.MODULE) STANDART.md kurallarına uyup uymadığını
// otomatik denetler — yeni bir bölüm yazıldıktan HEMEN sonra (yayına almadan ÖNCE) çalıştırılır.
// KULLANIM: node AKADEMI/backend/_scripts/audit-bolum.js <bolum-dosyasi.html> [<dosya2.html> ...]
//
// Üç ayrı kontrol yapar:
//
// 1) KOD SATIR KAPSAMI — bir `kod` slaytındaki HER anlamlı satırın (boş/yalnızca-parantez/yorum
//    satırları hariç) satirlar[]'da bir karşılığı olmalı (STANDART.md §3, "Kapsam — TÜM anlamlı
//    satırlar").
//
// 2) KOD-DEĞİŞİKLİK CONTEXT SIZINTISI + +/- SATIR KAPSAMI (kritik, motor davranışından kaynaklanan
//    bir tuzak) — slides-engine.js'in renderKodDegisiklik'i satirlar[]'ı satır METNİNE göre TEK bir
//    global Map'e yazar (bkz. engine/slides-engine.js ~L218) ve diff'teki HER satırı (context/+/-
//    FARK ETMEKSİZİN) bu Map'te arar. Bu yüzden bir `+`/`-` satırı için yazılan bir satirlar[]
//    girdisi, AYNI diff bloğunda karakter karakter AYNI metne sahip bir CONTEXT (` ` önekli) satır
//    da VARSA, o context satırını da YANLIŞLIKLA tıklanabilir/açıklanmış hale getirir —
//    STANDART.md'nin "CONTEXT satırlara satirlar[] girdisi YAZILMAZ" kuralını GÖRÜNMEZ şekilde
//    ihlal eder (yazan kişi context'e hiç dokunmadığını düşünür ama motor ikisini AYIRT ETMEZ). Bu
//    script HER satirlar[] girdisinin metnini context-satır kümesiyle karşılaştırır, çakışma varsa
//    raporlar. Ayrıca STANDART.md §3.1'in kapsam kuralı YALNIZCA `+` DEĞİL `-` (silinen) satırlar
//    için de geçerlidir — bu script ikisini de tarar. Çözüm: çakışan bir satır için satirlar[]
//    girdisi YAZILMAZ (komşu benzersiz bir satırın açıklamasına "hemen alt/üstündeki satır X'in Y
//    bölümüyle birebir aynı olduğu için ayrıca açıklanmıyor" notu eklenir) — bkz. A-05 Bölüm 8'in
//    WordConceptRepository.cs/WordsController.cs kod-degisiklik slaytları, gerçek bir düzeltme
//    örneği için.
//
// 3) İÇERİKSİZ (LAZY) AÇIKLAMA — aciklama/neden/olmasaydi/nedenBuKlasor alanlarının bazıları
//    yalnızca "bu satır/dosya X ile AYNI ad alanında/klasörde" gibi bir OLGUYU bildirip NEDEN
//    sorusunu hiç YANITLAMIYOR olabilir. STANDART.md §4 "Değişmedi/aynı/BİREBİR gibi geçmişe
//    atıfla YETİNEN bir aciklama YASAK" kuralı bunu zaten yasaklıyor; bu kontrol o kuralı
//    otomatikleştirir. Gerçek bir açıklama HER ZAMAN "neden bu şekilde" sorusuna (assembly
//    taraması ad alanına bakmaz ama biz YİNE DE gruplarız çünkü X, Y, Z) değinir — yalnızca "aynı"
//    diyen bir cümle bunu YAPMAZ.

const fs = require('fs');

const BRACE_ONLY = /^[{}\)\];,\(]+$/;
// Yalnızca "X ile AYNI ad alanı/klasör(dür)?" gibi tek cümlelik, sıfır gerekçeli kalıpları yakalar —
// "AYNI" geçen ama YANINDA gerçek bir "çünkü/için/BULMAZ/BAĞIMSIZ" gerekçesi olan cümleleri YAKALAMAZ.
const LAZY_PATTERNS = [
  /\bAYNI ad alanı\.?\s*$/,
  /\bORTAK ad alanı\.?\s*$/,
  /\beşleşen ad alanı\.?\s*$/,
  /\bAYNI klasör\.?\s*$/,
  /^Klasör yoluyla eşleşen ad alanı\.?\s*$/,
  /^Değişmedi\.?\s*$/i,
  /^Aynı\.?\s*$/i,
];

function loadModule(filePath) {
  const content = fs.readFileSync(filePath, 'utf8');
  const marker = 'window.MODULE = {';
  const markerIdx = content.indexOf(marker);
  if (markerIdx === -1) return null; // ör. index.html — bölüm dosyası DEĞİL, sessizce ATLANIR
  const start = markerIdx + 'window.MODULE = '.length;
  const end = content.indexOf('</script>', start);
  const js = content.slice(start, end);
  // eslint-disable-next-line no-eval
  return eval('(' + js.slice(0, js.lastIndexOf(';')) + ')');
}

function auditFile(filePath) {
  const mod = loadModule(filePath);
  if (mod === null) return null; // bölüm dosyası DEĞİL (ör. index.html)
  const findings = [];

  mod.slaytlar.forEach((s, slideIdx) => {
    const where = `${filePath} [slayt ${slideIdx}: ${s.baslik || s.tur}]`;

    // --- Kontrol 1: kod slaytı satır kapsamı ---
    if (s.tur === 'kod') {
      const lines = String(s.kod || '').split('\n').map((l) => l.trim()).filter((l) => l.length > 0);
      const covered = new Set((s.satirlar || []).map((x) => x.satir.trim()));
      lines.forEach((l) => {
        if (l.startsWith('//')) return;
        if (BRACE_ONLY.test(l)) return;
        if (!covered.has(l)) findings.push(`[KOD EKSİK] ${where}: ${JSON.stringify(l)}`);
      });
    }

    // --- Kontrol 2: kod-degisiklik context sızıntısı + + satır kapsamı ---
    // `satirIndex` yazılmış girdiler KONUMA göre eşleşir (bkz. engine/slides-engine.js
    // renderKodDegisiklik) — bu girdiler context sızıntısı riski TAŞIMAZ, ayrı ele alınır.
    if (s.tur === 'kod-degisiklik') {
      const diffLines = String(s.diff || '').split('\n');
      const contextTexts = new Set(
        diffLines.filter((l) => l.startsWith(' ')).map((l) => l.slice(1).trim()).filter((l) => l.length > 0)
      );
      const byIndexEntries = (s.satirlar || []).filter((x) => typeof x.satirIndex === 'number');
      const byTextEntries = (s.satirlar || []).filter((x) => typeof x.satirIndex !== 'number');
      const coveredByText = new Set(byTextEntries.map((x) => x.satir.trim()));
      const coveredByIndex = new Set(byIndexEntries.map((x) => x.satirIndex));

      // satirIndex girdisinin GERÇEKTEN o konumdaki satırla eşleştiğini doğrula (yazım hatası/kaydırma önle).
      byIndexEntries.forEach((entry) => {
        const raw = diffLines[entry.satirIndex];
        if (raw === undefined) {
          findings.push(`[GEÇERSİZ satirIndex] ${where}: satirIndex=${entry.satirIndex} diff sınırları DIŞINDA → ${JSON.stringify(entry.satir)}`);
          return;
        }
        if (raw.charAt(0) === ' ') {
          findings.push(`[satirIndex CONTEXT'E İŞARET EDİYOR] ${where}: satirIndex=${entry.satirIndex} bir CONTEXT satırı, + satırı DEĞİL → ${JSON.stringify(entry.satir)}`);
        }
        if (raw.slice(1).trim() !== entry.satir.trim()) {
          findings.push(`[satirIndex METİN UYUŞMAZLIĞI] ${where}: satirIndex=${entry.satirIndex} konumundaki satır ≠ entry.satir → diff: ${JSON.stringify(raw.slice(1).trim())} vs satir: ${JSON.stringify(entry.satir)}`);
        }
      });

      // Metin-tabanlı (satirIndex'siz) girdiler context'le çakışmamalı.
      byTextEntries.forEach((entry) => {
        if (contextTexts.has(entry.satir.trim())) {
          findings.push(`[CONTEXT SIZINTISI] ${where}: satirlar[] girdisi context satırıyla ÇAKIŞIYOR (satirIndex EKLEYEREK çözülebilir) → ${JSON.stringify(entry.satir)}`);
        }
      });

      // STANDART.md §3.1: kapsam kuralı hem + hem - satırlar için geçerli (yalnızca + değil) —
      // silinen bir satır da "neden silindi" sorusunu yanıtlayan bir satirlar[] girdisi alabilir.
      diffLines.forEach((raw, lineIdx) => {
        const marker = raw.charAt(0);
        if (marker !== '+' && marker !== '-') return;
        const l = raw.slice(1).trim();
        if (l.length === 0) return;
        if (l.startsWith('//')) return;
        if (BRACE_ONLY.test(l)) return;
        if (coveredByIndex.has(lineIdx)) return;
        if (contextTexts.has(l)) return; // context ile çakışan +/- satırlar, satirIndex YAZILMADIYSA bilerek açıklanmaz
        if (coveredByText.has(l)) return;
        findings.push(`[DIFF ${marker} EKSİK] ${where}: ${JSON.stringify(l)}`);
      });
    }

    // --- Kontrol 3: içeriksiz açıklama taraması ---
    const textFields = [];
    if (s.aciklama) textFields.push(['aciklama', s.aciklama]);
    if (s.neden) textFields.push(['neden', s.neden]);
    if (s.olmasaydi) textFields.push(['olmasaydi', s.olmasaydi]);
    if (s.nedenBuKlasor) textFields.push(['nedenBuKlasor', s.nedenBuKlasor]);
    (s.satirlar || []).forEach((entry, i) => {
      if (entry.aciklama) textFields.push([`satirlar[${i}].aciklama`, entry.aciklama]);
      if (entry.neden) textFields.push([`satirlar[${i}].neden`, entry.neden]);
      if (entry.olmasaydi) textFields.push([`satirlar[${i}].olmasaydi`, entry.olmasaydi]);
    });

    textFields.forEach(([field, text]) => {
      if (LAZY_PATTERNS.some((re) => re.test(text))) {
        findings.push(`[İÇERİKSİZ AÇIKLAMA] ${where} (${field}): ${JSON.stringify(text)}`);
      }
    });
  });

  return findings;
}

const files = process.argv.slice(2);
if (files.length === 0) {
  console.error('Kullanım: node audit-bolum.js <bolum-dosyasi.html> [<dosya2.html> ...]');
  process.exit(2);
}

let totalFindings = 0;
files.forEach((f) => {
  const findings = auditFile(f);
  if (findings === null) return; // window.MODULE yok — bölüm dosyası DEĞİL, atlanır (ör. index.html)
  if (findings.length === 0) {
    console.log(`✅ ${f} — temiz.`);
  } else {
    console.log(`❌ ${f} — ${findings.length} bulgu:`);
    findings.forEach((line) => console.log('   ' + line));
    totalFindings += findings.length;
  }
});

process.exit(totalFindings > 0 ? 1 : 0);
