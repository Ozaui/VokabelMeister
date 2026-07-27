import { afterEach } from 'vitest'
import { cleanup } from '@testing-library/react'
import '@testing-library/jest-dom/vitest'
// i18n singleton'ı test ortamında da başlatır — aksi halde useTranslation() çağıran
// her component testi, hiç init edilmemiş bir i18next örneğiyle karşılaşırdı.
import '../i18n/i18n'

// vite.config.ts'te `globals: true` KULLANILMAMASI (test dosyalarında `describe`/`it`/`expect`
// açıkça import edilir) nedeniyle React Testing Library'nin otomatik `afterEach(cleanup)` kaydı
// (global `afterEach`'i arar) devreye GİRMEZ — elle tetiklenmezse bir testin DOM'u bir SONRAKİ
// testte kalır, "aynı role/label iki kez bulundu" hatası verir.
afterEach(() => {
  cleanup()
})
