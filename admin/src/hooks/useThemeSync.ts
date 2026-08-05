import { useEffect } from 'react'
import { useAppSelector } from '../store/hooks'

function applyDarkClass(isDark: boolean) {
  document.documentElement.classList.toggle('dark', isDark)
}

export function useThemeSync() {
  const theme = useAppSelector((state) => state.theme.theme)

  useEffect(() => {
    if (theme === 'System') {
      const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
      applyDarkClass(mediaQuery.matches)

      const handleChange = (event: MediaQueryListEvent) => applyDarkClass(event.matches)
      mediaQuery.addEventListener('change', handleChange)
      return () => mediaQuery.removeEventListener('change', handleChange)
    }

    applyDarkClass(theme === 'Dark')
  }, [theme])
}
