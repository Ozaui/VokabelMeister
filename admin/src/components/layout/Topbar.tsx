import { useDispatch } from 'react-redux'
import { useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { logout } from '../../store/slices/authSlice'
import { LanguageSwitcher } from '../LanguageSwitcher'
import { ThemeSwitcher } from '../ThemeSwitcher'

export function Topbar() {
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const { t } = useTranslation()

  const handleLogout = () => {
    dispatch(logout())
    navigate('/login', { replace: true })
  }

  return (
    <header className="flex h-14 items-center justify-end gap-3 border-b border-border bg-surface px-4">
      <ThemeSwitcher />
      <LanguageSwitcher />
      <button
        type="button"
        onClick={handleLogout}
        className="rounded-control px-3 py-1.5 text-sm font-medium text-text hover:bg-background"
      >
        {t('action.logout')}
      </button>
    </header>
  )
}
