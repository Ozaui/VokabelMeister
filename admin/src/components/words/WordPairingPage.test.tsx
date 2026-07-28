import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { WordPairingPage } from './WordPairingPage'
import { useGetLanguagesQuery } from '../../store/api/languagesApi'
import { useGetUnmatchedWordConceptsQuery, usePairWordConceptsMutation } from '../../store/api/wordsApi'

vi.mock('../../store/api/languagesApi', () => ({
  useGetLanguagesQuery: vi.fn(),
}))
vi.mock('../../store/api/wordsApi', () => ({
  useGetUnmatchedWordConceptsQuery: vi.fn(),
  usePairWordConceptsMutation: vi.fn(),
}))

const mockedUseGetLanguagesQuery = vi.mocked(useGetLanguagesQuery)
const mockedUseGetUnmatchedWordConceptsQuery = vi.mocked(useGetUnmatchedWordConceptsQuery)
const mockedUsePairWordConceptsMutation = vi.mocked(usePairWordConceptsMutation)

const germanItems = [
  { wordConceptId: 41, languageCode: 'de', text: 'aber', definition: 'ama, fakat, ancak', partOfSpeech: 'Conjunction', difficultyLevel: 'A1', suggestedMatchConceptId: 87 },
]
const turkishItems = [
  { wordConceptId: 87, languageCode: 'tr', text: 'fakat', definition: null, partOfSpeech: 'Conjunction', difficultyLevel: 'A1', suggestedMatchConceptId: null },
  { wordConceptId: 88, languageCode: 'tr', text: 'ancak', definition: null, partOfSpeech: 'Conjunction', difficultyLevel: 'A1', suggestedMatchConceptId: null },
]

describe('WordPairingPage', () => {
  const pairTrigger = vi.fn()

  beforeEach(() => {
    pairTrigger.mockReset().mockResolvedValue({})
    mockedUsePairWordConceptsMutation.mockReturnValue([pairTrigger, { isLoading: false }] as unknown as ReturnType<
      typeof usePairWordConceptsMutation
    >)
    mockedUseGetLanguagesQuery.mockReturnValue({
      data: [
        { id: 1, code: 'de', name: 'German', nativeName: 'Deutsch' },
        { id: 2, code: 'tr', name: 'Turkish', nativeName: 'Türkçe' },
      ],
    } as unknown as ReturnType<typeof useGetLanguagesQuery>)
    mockedUseGetUnmatchedWordConceptsQuery.mockImplementation(((arg: { languageId: number }) => ({
      data: { items: arg.languageId === 1 ? germanItems : turkishItems, totalCount: 1, page: 1, pageSize: 50 },
      isLoading: false,
      refetch: vi.fn(),
    })) as unknown as typeof useGetUnmatchedWordConceptsQuery)
  })

  it('her iki sütun da eşleşmemiş kelimeleri listeler', () => {
    render(<WordPairingPage />)

    expect(screen.getByText('aber')).toBeInTheDocument()
    expect(screen.getByText('fakat')).toBeInTheDocument()
    expect(screen.getByText('ancak')).toBeInTheDocument()
  })

  it('Almanca taraf seçilince önerilen Türkçe eşleşme vurgulanır', async () => {
    render(<WordPairingPage />)

    await userEvent.click(screen.getByText('aber'))

    // "fakat" (suggestedMatchConceptId: 87) satırında "Önerilen eşleşme" etiketi görünür.
    const fakatRow = screen.getByText('fakat').closest('button')!
    expect(fakatRow).toHaveTextContent('Önerilen eşleşme')
    const ancakRow = screen.getByText('ancak').closest('button')!
    expect(ancakRow).not.toHaveTextContent('Önerilen eşleşme')
  })

  it('mutlu yol: iki taraf seçilip Eşleştir\'e basılınca pairWordConcepts doğru primaryId ile çağrılır', async () => {
    render(<WordPairingPage />)

    await userEvent.click(screen.getByText('aber'))
    await userEvent.click(screen.getByText('fakat'))
    await userEvent.click(screen.getByRole('button', { name: 'Eşleştir' }))

    // "aber" (Almanca) İLK seçilen taraf olduğu için varsayılan birincil odur.
    expect(pairTrigger).toHaveBeenCalledWith({ primaryId: 41, otherConceptId: 87 })
  })

  it('"birincil tarafı değiştir" ile primaryId karşı tarafa döner', async () => {
    render(<WordPairingPage />)

    await userEvent.click(screen.getByText('aber'))
    await userEvent.click(screen.getByText('fakat'))
    await userEvent.click(screen.getByRole('button', { name: 'Birincil tarafı değiştir' }))
    await userEvent.click(screen.getByRole('button', { name: 'Eşleştir' }))

    expect(pairTrigger).toHaveBeenCalledWith({ primaryId: 87, otherConceptId: 41 })
  })
})
