import { createSlice, type PayloadAction } from '@reduxjs/toolkit'

export type Theme = 'Light' | 'Dark' | 'System'

interface ThemeState {
  theme: Theme
}

// Users.ThemePreference'ın (A-03.3) DB CHECK constraint'iyle AYNI üç değer — C-01'de gerçek
// API'ye bağlanınca string'ler birebir eşleşsin diye.
function readStoredTheme(): Theme {
  const stored = localStorage.getItem('theme')
  return stored === 'Light' || stored === 'Dark' || stored === 'System' ? stored : 'System'
}

const initialState: ThemeState = {
  theme: readStoredTheme(),
}

const themeSlice = createSlice({
  name: 'theme',
  initialState,
  reducers: {
    // NOT (A-03.3/C-01): languageSlice.setLanguage ile AYNI durum — DB alanı zaten var, yazma
    // ucu (PUT /users/me) henüz yok, bu action şimdilik yalnızca localStorage'a yazar.
    setTheme: (state, action: PayloadAction<Theme>) => {
      state.theme = action.payload
      localStorage.setItem('theme', action.payload)
    },
  },
})

export const { setTheme } = themeSlice.actions
export default themeSlice.reducer
