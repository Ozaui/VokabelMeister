import { useTranslation } from 'react-i18next'
import Topbar from './components/layout/Topbar'
import { useLanguageSync } from './hooks/useLanguageSync'

function App() {
  useLanguageSync()
  const { t } = useTranslation()

  return (
    <div className="flex min-h-svh flex-col bg-background text-text-primary">
      <Topbar />
      <div className="flex flex-1 items-center justify-center">
        <div className="rounded-card border border-border bg-surface p-8 shadow-sm">
          <h1 className="text-2xl font-semibold">{t('app.title')}</h1>
          <p className="mt-2 text-text-secondary">{t('common.placeholderNotice')}</p>
        </div>
      </div>
    </div>
  )
}

export default App
