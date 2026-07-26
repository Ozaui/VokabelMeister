import { useDispatch, useSelector } from 'react-redux'
import { useTranslation } from 'react-i18next'
import { Sun, Moon, Monitor } from 'lucide-react'
import type { RootState } from '../store/store'
import { setTheme, type Theme } from '../store/slices/themeSlice'

const themes: { value: Theme; labelKey: string; Icon: typeof Sun }[] = [
  { value: 'Light', labelKey: 'theme.light', Icon: Sun },
  { value: 'Dark', labelKey: 'theme.dark', Icon: Moon },
  { value: 'System', labelKey: 'theme.system', Icon: Monitor },
]

export function ThemeSwitcher() {
  const dispatch = useDispatch()
  const { t } = useTranslation()
  const current = useSelector((state: RootState) => state.theme.theme)

  const handleChange = (value: Theme) => {
    dispatch(setTheme(value))
  }

  return (
    <div className="flex items-center gap-1 rounded-control border border-border bg-surface p-1">
      {themes.map(({ value, labelKey, Icon }) => (
        <button
          key={value}
          type="button"
          aria-pressed={current === value}
          aria-label={t(labelKey)}
          title={t(labelKey)}
          onClick={() => handleChange(value)}
          className={`rounded-control p-1.5 ${
            current === value ? 'bg-primary text-white' : 'text-muted hover:bg-background'
          }`}
        >
          <Icon size={16} />
        </button>
      ))}
    </div>
  )
}
