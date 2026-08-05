import { useAppDispatch, useAppSelector } from '../../store/hooks'
import { setLanguage, type Language } from '../../store/slices/languageSlice'

const LANGUAGES: { code: Language; label: string }[] = [
  { code: 'tr', label: 'TR' },
  { code: 'de', label: 'DE' },
]

function LanguageSwitcher() {
  const dispatch = useAppDispatch()
  const currentLanguage = useAppSelector((state) => state.language.language)

  return (
    <div className="inline-flex overflow-hidden rounded-control border border-border">
      {LANGUAGES.map(({ code, label }) => (
        <button
          key={code}
          type="button"
          aria-pressed={currentLanguage === code}
          onClick={() => dispatch(setLanguage(code))}
          className="px-3 py-1.5 text-sm font-medium text-text-secondary transition-colors duration-150 aria-pressed:bg-accent aria-pressed:text-white"
        >
          {label}
        </button>
      ))}
    </div>
  )
}

export default LanguageSwitcher
