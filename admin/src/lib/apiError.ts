import type { SerializedError } from '@reduxjs/toolkit'
import type { FetchBaseQueryError } from '@reduxjs/toolkit/query'

interface ApiErrorBody {
  error: { code: string; message: string }
}

function hasErrorBody(data: unknown): data is ApiErrorBody {
  return (
    typeof data === 'object' &&
    data !== null &&
    'error' in data &&
    typeof (data as ApiErrorBody).error?.message === 'string'
  )
}

// Backend ApiErrorResponse'un { error: { code, message } } şekli ile birebir eşleşir
// (backend/WordLearner.Application/Common/Models/ApiErrorResponse.cs) — message zaten
// Accept-Language'a göre çözülmüş, admin panelin ek bir çeviri yapmasına gerek yok.
export function getApiErrorMessage(error: FetchBaseQueryError | SerializedError | unknown): string | null {
  if (error && typeof error === 'object' && 'data' in error && hasErrorBody((error as { data: unknown }).data)) {
    return (error as { data: ApiErrorBody }).data.error.message
  }
  return null
}
