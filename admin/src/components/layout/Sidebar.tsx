import { NavLink } from 'react-router-dom'
import { useTranslation } from 'react-i18next'

export function Sidebar() {
  const { t } = useTranslation()
  const links = [
    { to: '/', label: t('nav.dashboard') },
    { to: '/words', label: t('nav.words') },
    { to: '/words/pairing', label: t('nav.wordPairing') },
  ]

  return (
    <aside className="w-56 shrink-0 border-r border-border bg-surface">
      <div className="px-4 py-4 font-heading text-lg font-bold text-text">VokabelMeister</div>
      <nav className="flex flex-col gap-1 px-2">
        {links.map((link) => (
          <NavLink
            key={link.to}
            to={link.to}
            end
            className={({ isActive }) =>
              `rounded-control px-3 py-2 text-sm font-medium ${
                isActive ? 'bg-primary text-white' : 'text-text hover:bg-background'
              }`
            }
          >
            {link.label}
          </NavLink>
        ))}
      </nav>
    </aside>
  )
}
