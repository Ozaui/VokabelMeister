import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import { WordListPage } from './WordListPage'
import { useDeleteWordMutation, useGetWordByIdQuery, useGetWordsQuery } from '../../store/api/wordsApi'
import wordFilterReducer from '../../store/slices/wordFilterSlice'

vi.mock('../../store/api/wordsApi', () => ({
  useGetWordsQuery: vi.fn(),
  useGetWordByIdQuery: vi.fn(),
  useDeleteWordMutation: vi.fn(),
}))

const mockedUseGetWordsQuery = vi.mocked(useGetWordsQuery)
const mockedUseGetWordByIdQuery = vi.mocked(useGetWordByIdQuery)
const mockedUseDeleteWordMutation = vi.mocked(useDeleteWordMutation)

// WordListPage useSelector/useDispatch ile wordFilterSlice'a bağlı — LoginPage'in aksine
// gerçek (küçük, senkron) bir Redux store'a ihtiyaç var, RTK Query hook'ları ayrıca mock'lanıyor.
function renderWithStore() {
  const store = configureStore({ reducer: { wordFilter: wordFilterReducer } })
  return render(
    <Provider store={store}>
      <WordListPage />
    </Provider>,
  )
}

describe('WordListPage', () => {
  beforeEach(() => {
    mockedUseGetWordByIdQuery.mockReturnValue({ data: undefined, isLoading: false } as unknown as ReturnType<
      typeof useGetWordByIdQuery
    >)
    mockedUseDeleteWordMutation.mockReturnValue([vi.fn(), { isLoading: false }] as unknown as ReturnType<
      typeof useDeleteWordMutation
    >)
  })

  it('kelime listesini render eder', () => {
    mockedUseGetWordsQuery.mockReturnValue({
      data: {
        items: [
          {
            wordConceptId: 1,
            partOfSpeech: 'Noun',
            difficultyLevel: 'A1',
            imageUrl: null,
            translations: [
              { languageCode: 'de', text: 'Tisch' },
              { languageCode: 'tr', text: 'masa' },
            ],
            categories: [],
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 20,
      },
      isLoading: false,
      refetch: vi.fn(),
    } as unknown as ReturnType<typeof useGetWordsQuery>)

    renderWithStore()

    expect(screen.getByText('Tisch / masa')).toBeInTheDocument()
  })

  it('eşleşmemiş (tek dilli) bir kavram "Eşleşmemiş" etiketiyle gösterilir', () => {
    mockedUseGetWordsQuery.mockReturnValue({
      data: {
        items: [
          {
            wordConceptId: 2,
            partOfSpeech: 'Conjunction',
            difficultyLevel: 'A1',
            imageUrl: null,
            translations: [{ languageCode: 'de', text: 'aber' }],
            categories: [],
          },
        ],
        totalCount: 1,
        page: 1,
        pageSize: 20,
      },
      isLoading: false,
      refetch: vi.fn(),
    } as unknown as ReturnType<typeof useGetWordsQuery>)

    renderWithStore()

    expect(screen.getByText('Eşleşmemiş')).toBeInTheDocument()
  })

  it('arama kutusuna yazınca useGetWordsQuery search parametresiyle çağrılır', async () => {
    mockedUseGetWordsQuery.mockReturnValue({
      data: { items: [], totalCount: 0, page: 1, pageSize: 20 },
      isLoading: false,
      refetch: vi.fn(),
    } as unknown as ReturnType<typeof useGetWordsQuery>)

    renderWithStore()

    await userEvent.type(screen.getByPlaceholderText('Ara...'), 'Tisch')

    expect(mockedUseGetWordsQuery).toHaveBeenLastCalledWith(
      expect.objectContaining({ search: 'Tisch', page: 1 }),
    )
  })

  it('seviye filtresi değişince sayfa 1\'e döner ve yeni filtreyle sorgu yapılır', async () => {
    mockedUseGetWordsQuery.mockReturnValue({
      data: { items: [], totalCount: 0, page: 1, pageSize: 20 },
      isLoading: false,
      refetch: vi.fn(),
    } as unknown as ReturnType<typeof useGetWordsQuery>)

    renderWithStore()

    await userEvent.selectOptions(screen.getByDisplayValue('Tüm Seviyeler'), 'A1')

    expect(mockedUseGetWordsQuery).toHaveBeenLastCalledWith(expect.objectContaining({ level: 'A1', page: 1 }))
  })
})
