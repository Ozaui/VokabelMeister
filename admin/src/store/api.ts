import axios from 'axios'
import { store } from './store'

const apiClient = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5001/api/v1',
})

apiClient.interceptors.request.use((config) => {
  const { accessToken } = store.getState().auth
  if (accessToken) {
    config.headers.Authorization = `Bearer ${accessToken}`
  }

  config.headers['Accept-Language'] = localStorage.getItem('language') ?? 'tr'

  return config
})

export default apiClient
