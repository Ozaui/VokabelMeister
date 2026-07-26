import { useDispatch, useSelector } from 'react-redux'
import { useTranslation } from 'react-i18next'
import type { RootState } from '../store/store'
import { setLanguage, type Language } from '../store/slices/languageSlice'

const languages: { code: Language; label: string }[] = [
  { code: 'tr', label: 'TR' },
  { code: 'de', label: 'DE' },
]

export function LanguageSwitcher() {
  const dispatch = useDispatch()
  const { i18n } = useTranslation()
  const current = useSelector((state: RootState) => state.language.language)

  const handleChange = (code: Language) => {
    dispatch(setLanguage(code))
    i18n.changeLanguage(code)
    document.documentElement.lang = code
  }

  return (
    <div className="flex items-center gap-1 rounded-control border border-border bg-surface p-1">
      {languages.map((lang) => (
        <button
          key={lang.code}
          type="button"
          aria-pressed={current === lang.code}
          onClick={() => handleChange(lang.code)}
          className={`rounded-control px-2 py-1 text-xs font-semibold ${
            current === lang.code ? 'bg-primary text-white' : 'text-muted hover:bg-background'
          }`}
        >
          {lang.label}
        </button>
      ))}
    </div>
  )
}
