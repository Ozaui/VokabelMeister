import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import tr from './locales/tr.json'
import de from './locales/de.json'

// languageSlice.ts'teki AYNI localStorage anahtarı ('language') — tek gerçek kaynak, iki
// sistem (Redux state + i18next) birbirinden bağımsız okumasın diye.
const storedLanguage = localStorage.getItem('language') === 'de' ? 'de' : 'tr'

i18n.use(initReactI18next).init({
  resources: {
    tr: { translation: tr },
    de: { translation: de },
  },
  lng: storedLanguage,
  fallbackLng: 'tr',
  interpolation: {
    escapeValue: false,
  },
})

export default i18n
