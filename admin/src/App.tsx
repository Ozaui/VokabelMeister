import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AppLayout } from './components/layout/AppLayout'
import { useThemeSync } from './hooks/useThemeSync'

// NOT: /login sayfası B-02'de LoginPage ile değişecek — B-01 kapsamı yalnızca
// ProtectedRoute + layout iskeletinin uçtan uca çalıştığını kanıtlayan yer tutucu.
function LoginPlaceholder() {
  return (
    <div className="flex h-screen items-center justify-center bg-background font-body text-text">
      Giriş sayfası — B-02'de gelecek
    </div>
  )
}

function DashboardPlaceholder() {
  const { t } = useTranslation()
  return <div className="font-heading text-xl font-bold text-text">{t('nav.dashboard')} — B-07'de gelecek</div>
}

function App() {
  useThemeSync()

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPlaceholder />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<AppLayout />}>
            <Route path="/" element={<DashboardPlaceholder />} />
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  )
}

export default App
