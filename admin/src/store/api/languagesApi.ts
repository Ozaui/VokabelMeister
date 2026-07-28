import { apiClient } from '../api'
import { useApiQuery } from '../../hooks/useApiQuery'
import type { Language } from '../../types/word.types'

// Backend A-05.1 retrofit'i (GET /languages) — de/tr kod↔id eşlemesinin TEK gerçek kaynağı,
// migration seed'inden ezbere bilinmez. WordsController'dan ayrı olduğu için Word'e özgü
// wordsApi.ts yerine kendi dosyasında (backend'deki LanguagesController ayrımıyla aynı desen).
async function fetchLanguages(): Promise<Language[]> {
  const { data } = await apiClient.get<Language[]>('/languages')
  return data
}

export function useGetLanguagesQuery() {
  return useApiQuery(fetchLanguages, undefined)
}
