import { BrowserRouter, Routes, Route } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ProtectedRoute } from './components/ProtectedRoute'
import { AppLayout } from './components/layout/AppLayout'
import { LoginPage } from './components/LoginPage'
import { OtpVerifyPage } from './components/OtpVerifyPage'
import { useThemeSync } from './hooks/useThemeSync'

function DashboardPlaceholder() {
  const { t } = useTranslation()
  return <div className="font-heading text-xl font-bold text-text">{t('nav.dashboard')} — B-07'de gelecek</div>
}

function App() {
  useThemeSync()

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/verify-otp" element={<OtpVerifyPage />} />
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
