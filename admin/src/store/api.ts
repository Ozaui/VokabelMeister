import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'
import type { RootState } from './store'

export const api = createApi({
  reducerPath: 'api',
  baseQuery: fetchBaseQuery({
    baseUrl: import.meta.env.VITE_API_URL,
    prepareHeaders: (headers, { getState }) => {
      const state = getState() as RootState
      if (state.auth.accessToken) {
        headers.set('Authorization', `Bearer ${state.auth.accessToken}`)
      }
      // Backend ErrorMessages/SuccessMessages ve admin log okuma bu header'a göre tr/de çözer
      // (CLAUDE.md §1 "İstisna"/"İkinci istisna") — admin panelin kendi dil tercihi tek kaynak.
      headers.set('Accept-Language', state.language.language)
      return headers
    },
  }),
  endpoints: () => ({}),
})
