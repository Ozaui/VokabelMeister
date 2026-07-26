import { Navigate, Outlet, useLocation } from 'react-router-dom'
import { useSelector } from 'react-redux'
import type { RootState } from '../store/store'

export function ProtectedRoute() {
  const isAuthenticated = useSelector((state: RootState) => state.auth.isAuthenticated)
  const location = useLocation()

  if (!isAuthenticated) {
    // B-02'de LoginPage bu state'i okuyup girişten sonra buraya geri döner.
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  return <Outlet />
}
