import { createSlice, type PayloadAction } from '@reduxjs/toolkit'
import type { AdminUser } from '../../types/auth.types'

interface AuthState {
  accessToken: string | null
  user: AdminUser | null
  isAuthenticated: boolean
}

// user, accessToken ile AYNI anda localStorage'a yazılır/okunur — ikisi birbirinden bağımsız
// kalırsa (ör. yalnızca token persist edilse) sayfa yenilendiğinde isAuthenticated=true ama
// user=null gibi tutarsız bir durum ortaya çıkardı.
function readStoredUser(): AdminUser | null {
  const raw = localStorage.getItem('authUser')
  if (!raw) return null
  try {
    return JSON.parse(raw) as AdminUser
  } catch {
    // Bozuk/elle değiştirilmiş bir değer modül yüklenirken (React mount OLMADAN ÖNCE) burayı
    // çökertip TÜM uygulamayı beyaz ekranda bırakabilirdi — kendi kendini onarır.
    localStorage.removeItem('authUser')
    return null
  }
}

const initialState: AuthState = {
  accessToken: localStorage.getItem('accessToken'),
  user: readStoredUser(),
  isAuthenticated: localStorage.getItem('accessToken') !== null,
}

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials: (state, action: PayloadAction<{ accessToken: string; user: AdminUser }>) => {
      state.accessToken = action.payload.accessToken
      state.user = action.payload.user
      state.isAuthenticated = true
      localStorage.setItem('accessToken', action.payload.accessToken)
      localStorage.setItem('authUser', JSON.stringify(action.payload.user))
    },
    logout: (state) => {
      state.accessToken = null
      state.user = null
      state.isAuthenticated = false
      localStorage.removeItem('accessToken')
      localStorage.removeItem('authUser')
    },
  },
})

export const { setCredentials, logout } = authSlice.actions
export default authSlice.reducer
