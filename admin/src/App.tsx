import { Route, Routes } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import AppLayout from './components/layout/AppLayout'
import ProtectedRoute from './routes/ProtectedRoute'
import { useLanguageSync } from './hooks/useLanguageSync'

function DashboardPlaceholder() {
  const { t } = useTranslation()

  return (
    <div className="rounded-card border border-border bg-surface p-8 shadow-sm">
      <h1 className="text-2xl font-semibold">{t('app.title')}</h1>
      <p className="mt-2 text-text-secondary">{t('common.placeholderNotice')}</p>
    </div>
  )
}

function App() {
  useLanguageSync()

  return (
    <Routes>
      <Route path="/login" element={<p>Giriş sayfası B-02&apos;de eklenecek.</p>} />
      <Route element={<ProtectedRoute />}>
        <Route path="/" element={<AppLayout />}>
          <Route index element={<DashboardPlaceholder />} />
        </Route>
      </Route>
    </Routes>
  )
}

export default App
