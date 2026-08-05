import { useEffect } from 'react'
import i18n from '../i18n'
import { useAppSelector } from '../store/hooks'

export function useLanguageSync() {
  const language = useAppSelector((state) => state.language.language)

  useEffect(() => {
    document.documentElement.lang = language
    i18n.changeLanguage(language)
  }, [language])
}
