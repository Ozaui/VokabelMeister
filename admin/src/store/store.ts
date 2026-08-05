import { configureStore } from '@reduxjs/toolkit'
import authReducer from './slices/authSlice'
import languageReducer from './slices/languageSlice'
import themeReducer from './slices/themeSlice'

export const store = configureStore({
  reducer: {
    auth: authReducer,
    language: languageReducer,
    theme: themeReducer,
  },
})

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
