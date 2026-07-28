import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useGetLanguagesQuery } from '../../store/api/languagesApi'
import { useGetUnmatchedWordConceptsQuery, usePairWordConceptsMutation } from '../../store/api/wordsApi'
import { getApiErrorMessage } from '../../lib/apiError'
import type { UnmatchedWordConcept } from '../../types/word.types'

interface SelectionState {
  de: number | null
  tr: number | null
  // İşlemi hangi taraftan başlattıysa (Icerik.md "Eşleştirme") o taraf varsayılan birincil olur —
  // admin isterse aşağıdaki "birincil tarafı değiştir" ile karşı tarafa çevirebilir.
  primary: 'de' | 'tr' | null
}

// İki sütun da AYNI şekilde davranır (ara, listele, seç) — dumb/controlled bir component,
// veri çekme sorumluluğu üst bileşende (WordPairingPage tek bir yerden iki sorguyu yönetir).
function UnmatchedColumn({
  title,
  search,
  onSearchChange,
  items,
  isLoading,
  selectedId,
  highlightId,
  onSelect,
}: {
  title: string
  search: string
  onSearchChange: (value: string) => void
  items: UnmatchedWordConcept[] | undefined
  isLoading: boolean
  selectedId: number | null
  highlightId: number | null
  onSelect: (concept: UnmatchedWordConcept) => void
}) {
  const { t } = useTranslation()

  return (
    <div className="flex-1 rounded-card border border-border bg-surface p-3">
      <h2 className="mb-2 font-heading text-sm font-bold text-text">{title}</h2>
      <input
        type="text"
        value={search}
        onChange={(e) => onSearchChange(e.target.value)}
        placeholder={t('words.pairing.search') ?? undefined}
        className="mb-2 w-full rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
      />
      {isLoading && <p className="text-sm text-muted">{t('words.list.loading')}</p>}
      {!isLoading && (!items || items.length === 0) && <p className="text-sm text-muted">{t('words.pairing.empty')}</p>}
      <ul className="flex max-h-96 flex-col gap-1 overflow-y-auto">
        {items?.map((concept) => {
          const isSelected = selectedId === concept.wordConceptId
          const isSuggested = highlightId === concept.wordConceptId
          return (
            <li key={concept.wordConceptId}>
              <button
                type="button"
                onClick={() => onSelect(concept)}
                className={`w-full rounded-control border px-2 py-1 text-left text-sm ${
                  isSelected
                    ? 'border-primary bg-primary/10 text-text'
                    : isSuggested
                      ? 'border-primary/50 bg-primary/5 text-text'
                      : 'border-border text-text hover:bg-background'
                }`}
              >
                <span className="font-medium">{concept.text}</span>
                {concept.definition && <span className="ml-1 text-xs text-muted">— {concept.definition}</span>}
                {isSuggested && (
                  <span className="ml-2 rounded-control bg-primary/20 px-1 text-xs text-primary">
                    {t('words.pairing.suggested')}
                  </span>
                )}
              </button>
            </li>
          )
        })}
      </ul>
    </div>
  )
}

export function WordPairingPage() {
  const { t } = useTranslation()
  const { data: languages } = useGetLanguagesQuery()
  const germanId = languages?.find((l) => l.code === 'de')?.id
  const turkishId = languages?.find((l) => l.code === 'tr')?.id

  const [germanSearch, setGermanSearch] = useState('')
  const [turkishSearch, setTurkishSearch] = useState('')
  const {
    data: germanData,
    isLoading: isGermanLoading,
    refetch: refetchGerman,
  } = useGetUnmatchedWordConceptsQuery(
    { languageId: germanId ?? 0, search: germanSearch || undefined, pageSize: 50 },
    { skip: germanId === undefined },
  )
  const {
    data: turkishData,
    isLoading: isTurkishLoading,
    refetch: refetchTurkish,
  } = useGetUnmatchedWordConceptsQuery(
    { languageId: turkishId ?? 0, search: turkishSearch || undefined, pageSize: 50 },
    { skip: turkishId === undefined },
  )

  const [selection, setSelection] = useState<SelectionState>({ de: null, tr: null, primary: null })
  const [pairWordConcepts, { isLoading: isPairing }] = usePairWordConceptsMutation()
  const [error, setError] = useState<string | null>(null)

  // Seçili Almanca kavramın önerdiği eşleşme (varsa) Türkçe sütununda vurgulanır.
  const suggestedTrId =
    selection.de !== null
      ? (germanData?.items.find((c) => c.wordConceptId === selection.de)?.suggestedMatchConceptId ?? null)
      : null

  const selectSide = (side: 'de' | 'tr', concept: UnmatchedWordConcept) => {
    setError(null)
    setSelection((prev) => {
      if (prev[side] === concept.wordConceptId) {
        // Aynı satıra tekrar tıklamak seçimi kaldırır.
        const otherSide = side === 'de' ? 'tr' : 'de'
        return { ...prev, [side]: null, primary: prev.primary === side ? (prev[otherSide] ? otherSide : null) : prev.primary }
      }
      const isFirstSelection = prev.de === null && prev.tr === null
      return { ...prev, [side]: concept.wordConceptId, primary: isFirstSelection ? side : prev.primary }
    })
  }

  const swapPrimary = () => {
    setSelection((prev) => (prev.primary === 'de' ? { ...prev, primary: 'tr' } : { ...prev, primary: 'de' }))
  }

  const canPair = selection.de !== null && selection.tr !== null && selection.primary !== null

  const handlePair = async () => {
    if (!canPair) return
    const primaryId = selection.primary === 'de' ? selection.de! : selection.tr!
    const otherConceptId = selection.primary === 'de' ? selection.tr! : selection.de!
    try {
      await pairWordConcepts({ primaryId, otherConceptId })
      setSelection({ de: null, tr: null, primary: null })
      refetchGerman()
      refetchTurkish()
    } catch (err) {
      setError(getApiErrorMessage(err) ?? t('auth.genericError'))
    }
  }

  return (
    <div className="flex flex-col gap-4">
      <h1 className="font-heading text-xl font-bold text-text">{t('words.pairing.title')}</h1>

      {error && (
        <p role="alert" className="rounded-control bg-destructive/10 px-3 py-2 text-sm text-destructive">
          {error}
        </p>
      )}

      <div className="flex flex-col gap-4 sm:flex-row">
        <UnmatchedColumn
          title={t('words.pairing.germanColumn')}
          search={germanSearch}
          onSearchChange={setGermanSearch}
          items={germanData?.items}
          isLoading={isGermanLoading}
          selectedId={selection.de}
          highlightId={null}
          onSelect={(concept) => selectSide('de', concept)}
        />
        <UnmatchedColumn
          title={t('words.pairing.turkishColumn')}
          search={turkishSearch}
          onSearchChange={setTurkishSearch}
          items={turkishData?.items}
          isLoading={isTurkishLoading}
          selectedId={selection.tr}
          highlightId={suggestedTrId}
          onSelect={(concept) => selectSide('tr', concept)}
        />
      </div>

      {selection.de !== null && selection.tr !== null && (
        <div className="rounded-card border border-border bg-surface p-3">
          <p className="mb-2 text-sm text-text">
            {t('words.pairing.primarySide')}: <strong>{t(`words.language.${selection.primary}`)}</strong>
          </p>
          <button
            type="button"
            onClick={swapPrimary}
            className="mr-2 rounded-control border border-border px-3 py-1 text-sm text-text hover:bg-background"
          >
            {t('words.pairing.swapPrimary')}
          </button>
          <button
            type="button"
            onClick={handlePair}
            disabled={!canPair || isPairing}
            className="rounded-control bg-primary px-4 py-1 text-sm font-semibold text-white disabled:opacity-60"
          >
            {t('words.pairing.pairButton')}
          </button>
        </div>
      )}
    </div>
  )
}
