import { Monitor, Moon, Sun } from 'lucide-react'
import { useAppDispatch, useAppSelector } from '../../store/hooks'
import { setTheme, type Theme } from '../../store/slices/themeSlice'

const THEMES: { value: Theme; label: string; Icon: typeof Sun }[] = [
  { value: 'Light', label: 'Açık tema', Icon: Sun },
  { value: 'Dark', label: 'Koyu tema', Icon: Moon },
  { value: 'System', label: 'Sistem teması', Icon: Monitor },
]

function ThemeSwitcher() {
  const dispatch = useAppDispatch()
  const currentTheme = useAppSelector((state) => state.theme.theme)

  return (
    <div className="inline-flex overflow-hidden rounded-control border border-border">
      {THEMES.map(({ value, label, Icon }) => (
        <button
          key={value}
          type="button"
          aria-label={label}
          aria-pressed={currentTheme === value}
          onClick={() => dispatch(setTheme(value))}
          className="px-3 py-1.5 text-text-secondary transition-colors duration-150 aria-pressed:bg-accent aria-pressed:text-white"
        >
          <Icon size={18} />
        </button>
      ))}
    </div>
  )
}

export default ThemeSwitcher
