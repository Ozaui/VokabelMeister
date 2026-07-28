import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useDispatch, useSelector } from 'react-redux'
import { useDeleteWordMutation, useGetWordByIdQuery, useGetWordsQuery } from '../../store/api/wordsApi'
import {
  setCategoryId as setCategoryFilter,
  setLevel,
  setPage,
  setPartOfSpeech,
  setSearch,
} from '../../store/slices/wordFilterSlice'
import type { RootState } from '../../store/store'
import { WordFormModal } from './WordFormModal'
import type { DifficultyLevel, PartOfSpeech } from '../../types/word.types'

const LEVELS: DifficultyLevel[] = ['A1', 'A2', 'B1', 'B2', 'C1', 'C2']
const PARTS_OF_SPEECH: PartOfSpeech[] = [
  'Noun',
  'Verb',
  'Adjective',
  'Adverb',
  'Conjunction',
  'Preposition',
  'Pronoun',
  'Other',
]

// wordConceptId !== null iken WordFormModal'ı düzenleme modunda açar — liste yalnızca özet
// (WordListItem) taşıdığı için, tam çeviri/gramer verisi bu ayrı sorguyla (GetWordByIdQuery)
// yalnızca düzenle tıklanınca çekilir.
function EditWordModal({ wordConceptId, onClose }: { wordConceptId: number; onClose: () => void }) {
  const { data, isLoading } = useGetWordByIdQuery(wordConceptId)
  if (isLoading || !data) return null
  return <WordFormModal initialData={data} onClose={onClose} />
}

export function WordListPage() {
  const { t } = useTranslation()
  const dispatch = useDispatch()
  const filter = useSelector((state: RootState) => state.wordFilter)
  const { data, isLoading, refetch } = useGetWordsQuery({
    search: filter.search || undefined,
    level: filter.level ?? undefined,
    partOfSpeech: filter.partOfSpeech ?? undefined,
    categoryId: filter.categoryId ?? undefined,
    page: filter.page,
    pageSize: 20,
  })
  const [deleteWord, { isLoading: isDeleting }] = useDeleteWordMutation()
  const [isCreating, setIsCreating] = useState(false)
  const [editingId, setEditingId] = useState<number | null>(null)

  const handleDelete = async (id: number) => {
    if (window.confirm(t('words.list.deleteConfirm') ?? '')) {
      await deleteWord(id)
      refetch()
    }
  }

  const totalPages = data ? Math.max(1, Math.ceil(data.totalCount / data.pageSize)) : 1

  return (
    <div className="flex flex-col gap-4">
      <div className="flex items-center justify-between">
        <h1 className="font-heading text-xl font-bold text-text">{t('words.list.title')}</h1>
        <button
          type="button"
          onClick={() => setIsCreating(true)}
          className="rounded-control bg-primary px-4 py-2 text-sm font-semibold text-white"
        >
          {t('words.list.addButton')}
        </button>
      </div>

      <div className="flex flex-wrap gap-2">
        <input
          type="text"
          value={filter.search}
          onChange={(e) => dispatch(setSearch(e.target.value))}
          placeholder={t('words.list.search') ?? undefined}
          className="rounded-control border border-border bg-surface px-3 py-2 text-sm text-text"
        />
        <select
          value={filter.level ?? ''}
          onChange={(e) => dispatch(setLevel((e.target.value || null) as DifficultyLevel | null))}
          className="rounded-control border border-border bg-surface px-3 py-2 text-sm text-text"
        >
          <option value="">{t('words.list.allLevels')}</option>
          {LEVELS.map((level) => (
            <option key={level} value={level}>
              {level}
            </option>
          ))}
        </select>
        <select
          value={filter.partOfSpeech ?? ''}
          onChange={(e) => dispatch(setPartOfSpeech((e.target.value || null) as PartOfSpeech | null))}
          className="rounded-control border border-border bg-surface px-3 py-2 text-sm text-text"
        >
          <option value="">{t('words.list.allPartsOfSpeech')}</option>
          {PARTS_OF_SPEECH.map((pos) => (
            <option key={pos} value={pos}>
              {t(`words.partOfSpeech.${pos}`)}
            </option>
          ))}
        </select>
        {filter.categoryId !== null && (
          <button
            type="button"
            onClick={() => dispatch(setCategoryFilter(null))}
            className="rounded-control border border-border px-3 py-2 text-sm text-text hover:bg-background"
          >
            {t('words.list.filterCategory') ?? ''} #{filter.categoryId} ×
          </button>
        )}
      </div>

      {isLoading && <p className="text-sm text-muted">{t('words.list.loading')}</p>}
      {!isLoading && data && data.items.length === 0 && (
        <p className="text-sm text-muted">{t('words.list.empty')}</p>
      )}

      {!isLoading && data && data.items.length > 0 && (
        <div className="overflow-x-auto rounded-card border border-border bg-surface">
          <table className="w-full text-sm">
            <thead>
              <tr className="border-b border-border bg-background text-left text-muted">
                <th className="p-3 font-medium">{t('words.form.text')}</th>
                <th className="p-3 font-medium">{t('words.form.partOfSpeech')}</th>
                <th className="p-3 font-medium">{t('words.form.difficultyLevel')}</th>
                <th className="p-3 font-medium" />
              </tr>
            </thead>
            <tbody>
              {data.items.map((item) => (
                <tr key={item.wordConceptId} className="border-b border-border last:border-0">
                  <td className="p-3 text-text">
                    {item.translations.map((tr) => tr.text).join(' / ')}
                    {item.translations.length < 2 && (
                      <span className="ml-2 rounded-control bg-warning/10 px-2 py-0.5 text-xs text-warning">
                        {t('words.list.unmatchedBadge')}
                      </span>
                    )}
                  </td>
                  <td className="p-3 text-text">{t(`words.partOfSpeech.${item.partOfSpeech}`)}</td>
                  <td className="p-3 text-text">{item.difficultyLevel}</td>
                  <td className="p-3 text-right">
                    <button
                      type="button"
                      onClick={() => setEditingId(item.wordConceptId)}
                      className="mr-2 rounded-control px-2 py-1 text-primary hover:bg-primary/10"
                    >
                      {t('action.edit')}
                    </button>
                    <button
                      type="button"
                      onClick={() => handleDelete(item.wordConceptId)}
                      disabled={isDeleting}
                      className="rounded-control px-2 py-1 text-destructive hover:bg-destructive/10 disabled:opacity-40"
                    >
                      {t('action.delete')}
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {!isLoading && data && data.totalCount > 0 && (
        <div className="flex items-center justify-end gap-3 text-sm text-text">
          <span>{t('words.list.pageInfo', { page: filter.page, totalPages })}</span>
          <button
            type="button"
            disabled={filter.page <= 1}
            onClick={() => dispatch(setPage(filter.page - 1))}
            className="rounded-control border border-border px-3 py-1 disabled:opacity-40"
          >
            {t('words.list.previous')}
          </button>
          <button
            type="button"
            disabled={filter.page >= totalPages}
            onClick={() => dispatch(setPage(filter.page + 1))}
            className="rounded-control border border-border px-3 py-1 disabled:opacity-40"
          >
            {t('words.list.next')}
          </button>
        </div>
      )}

      {isCreating && (
        <WordFormModal
          onClose={() => {
            setIsCreating(false)
            refetch()
          }}
        />
      )}
      {editingId !== null && (
        <EditWordModal
          wordConceptId={editingId}
          onClose={() => {
            setEditingId(null)
            refetch()
          }}
        />
      )}
    </div>
  )
}
