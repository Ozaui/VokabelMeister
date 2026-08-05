import { Menu, X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import LanguageSwitcher from './LanguageSwitcher'

interface TopbarProps {
  isSidebarOpen: boolean
  onToggleSidebar: () => void
}

function Topbar({ isSidebarOpen, onToggleSidebar }: TopbarProps) {
  const { t } = useTranslation()

  return (
    <header className="flex items-center justify-between border-b border-border bg-surface px-6 py-4">
      <div className="flex items-center gap-3">
        <button
          type="button"
          aria-label="Menüyü aç/kapat"
          aria-expanded={isSidebarOpen}
          onClick={onToggleSidebar}
          className="text-text-secondary md:hidden"
        >
          {isSidebarOpen ? <X size={20} /> : <Menu size={20} />}
        </button>
        <span className="font-semibold text-text-primary">{t('app.title')}</span>
      </div>
      <LanguageSwitcher />
    </header>
  )
}

export default Topbar
