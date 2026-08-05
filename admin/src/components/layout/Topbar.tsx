import { Menu, X } from 'lucide-react'
import LanguageSwitcher from './LanguageSwitcher'
import ThemeSwitcher from './ThemeSwitcher'

interface TopbarProps {
  isSidebarOpen: boolean
  onToggleSidebar: () => void
}

function Topbar({ isSidebarOpen, onToggleSidebar }: TopbarProps) {
  return (
    <header className="flex items-center border-b border-border bg-surface px-6 py-4">
      <button
        type="button"
        aria-label="Menüyü aç/kapat"
        aria-expanded={isSidebarOpen}
        onClick={onToggleSidebar}
        className="text-text-secondary md:hidden"
      >
        {isSidebarOpen ? <X size={20} /> : <Menu size={20} />}
      </button>
      <div className="ml-auto flex items-center gap-3">
        <ThemeSwitcher />
        <LanguageSwitcher />
      </div>
    </header>
  )
}

export default Topbar
