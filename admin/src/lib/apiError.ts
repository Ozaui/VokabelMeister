import { isAxiosError } from 'axios'

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
export function getApiErrorMessage(error: unknown): string | null {
  if (isAxiosError(error) && hasErrorBody(error.response?.data)) {
    return error.response!.data.error.message
  }
  return null
}

// B-03 (WordFormModal) — "409 DuplicateWordException (WORD_TEXT_ALREADY_EXISTS) → ?force=true ile
// onaylayarak tekrar gönder" akışı Code'a göre dallanır, dile göre değişen Message'a değil.
export function getApiErrorCode(error: unknown): string | null {
  if (isAxiosError(error) && hasErrorBody(error.response?.data)) {
    return error.response!.data.error.code
  }
  return null
}
