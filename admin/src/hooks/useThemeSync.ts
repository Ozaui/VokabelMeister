import { useEffect } from 'react'
import { useSelector } from 'react-redux'
import type { RootState } from '../store/store'

// index.html'deki senkron script sayfa AÇILIRKEN aynı hesaplamayı yapıyor (FOUC önlemi) — bu hook
// yalnızca SONRAKİ değişiklikleri (tema değiştirme, System modundayken OS tercihinin değişmesi)
// canlı olarak `.dark` class'ına yansıtır.
export function useThemeSync() {
  const theme = useSelector((state: RootState) => state.theme.theme)

  useEffect(() => {
    const media = window.matchMedia('(prefers-color-scheme: dark)')

    function applyTheme() {
      const isDark = theme === 'Dark' || (theme === 'System' && media.matches)
      document.documentElement.classList.toggle('dark', isDark)
    }

    applyTheme()

    if (theme === 'System') {
      media.addEventListener('change', applyTheme)
      return () => media.removeEventListener('change', applyTheme)
    }
  }, [theme])
}
