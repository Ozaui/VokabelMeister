import { createSlice, type PayloadAction } from '@reduxjs/toolkit'

export type Language = 'tr' | 'de'

const SUPPORTED_LANGUAGES: Language[] = ['tr', 'de']

interface LanguageState {
  language: Language
}

function resolveInitialLanguage(): Language {
  const stored = localStorage.getItem('language')
  return SUPPORTED_LANGUAGES.includes(stored as Language) ? (stored as Language) : 'tr'
}

const initialState: LanguageState = {
  language: resolveInitialLanguage(),
}

const languageSlice = createSlice({
  name: 'language',
  initialState,
  reducers: {
    setLanguage: (state, action: PayloadAction<Language>) => {
      state.language = action.payload
      localStorage.setItem('language', action.payload)
    },
  },
})

export const { setLanguage } = languageSlice.actions
export default languageSlice.reducer
