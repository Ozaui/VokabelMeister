import { useTranslation } from 'react-i18next'
import LanguageSwitcher from './LanguageSwitcher'

function Topbar() {
  const { t } = useTranslation()

  return (
    <header className="flex items-center justify-between border-b border-border bg-surface px-6 py-4">
      <span className="font-semibold text-text-primary">{t('app.title')}</span>
      <LanguageSwitcher />
    </header>
  )
}

export default Topbar
