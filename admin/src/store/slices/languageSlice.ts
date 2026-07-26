import { createSlice, type PayloadAction } from '@reduxjs/toolkit'

export type Language = 'tr' | 'de'

interface LanguageState {
  language: Language
}

// CLAUDE.md §1: "desteklenmiyorsa tr'ye düşer" — ErrorMessages/SuccessMessages ile aynı kural.
function readStoredLanguage(): Language {
  const stored = localStorage.getItem('language')
  return stored === 'de' ? 'de' : 'tr'
}

const initialState: LanguageState = {
  language: readStoredLanguage(),
}

const languageSlice = createSlice({
  name: 'language',
  initialState,
  reducers: {
    // NOT (A-03.4/C-01): Users.LanguagePreference DB'de var ama yazma ucu (PUT /users/me) henüz
    // yok — bu action şimdilik yalnızca localStorage'a yazar. C-01 tamamlandığında burada bir
    // RTK Query mutation da tetiklenmeli (bkz. docs/TASK/C_kullanici_backend.md C-01 notu).
    setLanguage: (state, action: PayloadAction<Language>) => {
      state.language = action.payload
      localStorage.setItem('language', action.payload)
    },
  },
})

export const { setLanguage } = languageSlice.actions
export default languageSlice.reducer
