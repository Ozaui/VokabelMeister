import { createSlice, type PayloadAction } from '@reduxjs/toolkit'

export type Theme = 'Light' | 'Dark' | 'System'

const SUPPORTED_THEMES: Theme[] = ['Light', 'Dark', 'System']

interface ThemeState {
  theme: Theme
}

function resolveInitialTheme(): Theme {
  const stored = localStorage.getItem('theme')
  return SUPPORTED_THEMES.includes(stored as Theme) ? (stored as Theme) : 'System'
}

const initialState: ThemeState = {
  theme: resolveInitialTheme(),
}

const themeSlice = createSlice({
  name: 'theme',
  initialState,
  reducers: {
    setTheme: (state, action: PayloadAction<Theme>) => {
      state.theme = action.payload
      localStorage.setItem('theme', action.payload)
    },
  },
})

export const { setTheme } = themeSlice.actions
export default themeSlice.reducer
