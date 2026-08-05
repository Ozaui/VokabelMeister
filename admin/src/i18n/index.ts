import i18n from 'i18next'
import { initReactI18next } from 'react-i18next'
import de from './locales/de.json'
import tr from './locales/tr.json'

i18n.use(initReactI18next).init({
  resources: {
    tr: { translation: tr },
    de: { translation: de },
  },
  lng: localStorage.getItem('language') ?? 'tr',
  fallbackLng: 'tr',
  interpolation: {
    escapeValue: false,
  },
})

export default i18n
