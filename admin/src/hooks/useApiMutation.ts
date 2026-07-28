import { useState } from 'react'

type UseApiMutationResult<T, A> = [(arg: A) => Promise<T>, { isLoading: boolean }]

// RTK Query mutation hook'larının yerini alır — trigger promise'i RTK'daki gibi .unwrap()
// GEREKTİRMEZ, axios zaten 2xx dışı yanıtlarda reject eder (çağıran taraf düz try/catch kullanır).
export function useApiMutation<T, A>(mutationFn: (arg: A) => Promise<T>): UseApiMutationResult<T, A> {
  const [isLoading, setIsLoading] = useState(false)

  const trigger = async (arg: A) => {
    setIsLoading(true)
    try {
      return await mutationFn(arg)
    } finally {
      setIsLoading(false)
    }
  }

  return [trigger, { isLoading }]
}
