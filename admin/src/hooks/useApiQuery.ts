import { useEffect, useRef, useState } from 'react'

interface UseApiQueryOptions {
  // RTK Query'nin skip'iyle aynı amaç — ör. WordPairingPage dil id'si henüz gelmemişken
  // (languageId: 0) isteği hiç atmaz.
  skip?: boolean
}

interface UseApiQueryResult<T> {
  data: T | undefined
  error: unknown
  isLoading: boolean
  refetch: () => void
}

// RTK Query'nin providesTags/invalidatesTags'ı yok — axios'a geçişte cache invalidation'ın
// yerini bilinçli olarak BASİT bir refetch() aldı (CLAUDE.md §3 "spekülatif ortak tip yazılmaz"
// ilkesiyle aynı gerekçe: burada henüz ihtiyaç duyulmayan bir cache katmanı kurmuyoruz).
export function useApiQuery<T, P>(
  fetcher: (params: P) => Promise<T>,
  params: P,
  options?: UseApiQueryOptions,
): UseApiQueryResult<T> {
  const skip = options?.skip ?? false
  const [data, setData] = useState<T>()
  const [error, setError] = useState<unknown>()
  const [isLoading, setIsLoading] = useState(!skip)
  const [reloadToken, setReloadToken] = useState(0)
  const paramsRef = useRef(params)
  paramsRef.current = params
  const fetcherRef = useRef(fetcher)
  fetcherRef.current = fetcher
  const paramsKey = JSON.stringify(params)

  useEffect(() => {
    if (skip) return
    let cancelled = false
    setIsLoading(true)
    fetcherRef
      .current(paramsRef.current)
      .then((result) => {
        if (cancelled) return
        setData(result)
        setError(undefined)
      })
      .catch((err: unknown) => {
        if (cancelled) return
        setError(err)
      })
      .finally(() => {
        if (!cancelled) setIsLoading(false)
      })
    return () => {
      cancelled = true
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [paramsKey, skip, reloadToken])

  return { data, error, isLoading, refetch: () => setReloadToken((t) => t + 1) }
}
