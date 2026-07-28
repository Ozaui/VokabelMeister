import axios from 'axios'
import { store } from './store'

export const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
})

// Backend ErrorMessages/SuccessMessages ve admin log okuma bu header'a göre tr/de çözer
// (CLAUDE.md §1 "İstisna"/"İkinci istisna") — admin panelin kendi dil tercihi tek kaynak.
apiClient.interceptors.request.use((config) => {
  const state = store.getState()
  if (state.auth.accessToken) {
    config.headers.Authorization = `Bearer ${state.auth.accessToken}`
  }
  config.headers['Accept-Language'] = state.language.language
  return config
})
