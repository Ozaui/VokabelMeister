import { apiClient } from '../api'
import { useApiQuery } from '../../hooks/useApiQuery'
import { useApiMutation } from '../../hooks/useApiMutation'
import type {
  CreateWordRequest,
  GetUnmatchedWordConceptsParams,
  GetWordsParams,
  PagedResult,
  PairWordConceptsRequest,
  UnmatchedWordConcept,
  UpdateWordRequest,
  WordConceptDetail,
  WordListItem,
} from '../../types/word.types'

async function fetchWords(params: GetWordsParams): Promise<PagedResult<WordListItem>> {
  const { data } = await apiClient.get<PagedResult<WordListItem>>('/words', { params })
  return data
}

async function fetchWordById(id: number): Promise<WordConceptDetail> {
  const { data } = await apiClient.get<WordConceptDetail>(`/words/${id}`)
  return data
}

// force=true, backend'in aynı dilde aynı Text'te 409 döndürdüğü durumda admin onaylayınca gönderilir.
async function createWord({ body, force }: { body: CreateWordRequest; force?: boolean }): Promise<WordConceptDetail> {
  const { data } = await apiClient.post<WordConceptDetail>('/words', body, {
    params: force ? { force: true } : undefined,
  })
  return data
}

async function updateWord({ body, force }: { body: UpdateWordRequest; force?: boolean }): Promise<WordConceptDetail> {
  const { data } = await apiClient.put<WordConceptDetail>(`/words/${body.id}`, body, {
    params: force ? { force: true } : undefined,
  })
  return data
}

async function deleteWord(id: number): Promise<void> {
  await apiClient.delete(`/words/${id}`)
}

async function fetchUnmatchedWordConcepts(
  params: GetUnmatchedWordConceptsParams,
): Promise<PagedResult<UnmatchedWordConcept>> {
  const { data } = await apiClient.get<PagedResult<UnmatchedWordConcept>>('/words/unmatched', { params })
  return data
}

async function pairWordConcepts(body: PairWordConceptsRequest): Promise<WordConceptDetail> {
  const { data } = await apiClient.post<WordConceptDetail>('/words/pair', body)
  return data
}

export function useGetWordsQuery(params: GetWordsParams) {
  return useApiQuery(fetchWords, params)
}

export function useGetWordByIdQuery(id: number) {
  return useApiQuery(fetchWordById, id)
}

export function useCreateWordMutation() {
  return useApiMutation(createWord)
}

export function useUpdateWordMutation() {
  return useApiMutation(updateWord)
}

export function useDeleteWordMutation() {
  return useApiMutation(deleteWord)
}

export function useGetUnmatchedWordConceptsQuery(
  params: GetUnmatchedWordConceptsParams,
  options?: { skip?: boolean },
) {
  return useApiQuery(fetchUnmatchedWordConcepts, params, options)
}

export function usePairWordConceptsMutation() {
  return useApiMutation(pairWordConcepts)
}
