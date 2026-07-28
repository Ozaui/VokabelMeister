import { useTranslation } from 'react-i18next'
import { ConjugationGrid } from './ConjugationGrid'
import type { PartOfSpeech, TurkishNounGrammarData, TurkishVerbGrammarData } from '../../types/word.types'

// TURKISH_LANGUAGE_FEATURES.md §9'un TS karşılığı — backend WordGrammarValidator'ın tr dalıyla
// (A-05 + A-05.2 retrofit'i: vowelHarmony/possessive zorunlu) AYNI matris, kod paylaşımı yok.
// Yalnızca GrammarData JSON alan adları — GÖSTERİLEN etiketler i18n'den (words.grammar.tr.*Labels)
// gelir (CLAUDE.md §1: admin panelin kendi statik metinleri react-i18next ile tr/de çevrilir).
const TR_VERB_TENSE_KEYS = ['presentContinuous', 'aorist', 'pastDefinite', 'pastNarrative', 'future'] as const
const TR_PERSON_KEYS = ['ben', 'sen', 'o', 'biz', 'siz', 'onlar'] as const
const TR_CASE_KEYS = ['nominative', 'accusative', 'dative', 'locative', 'ablative', 'genitive'] as const

interface TurkishGrammarFieldsProps {
  partOfSpeech: PartOfSpeech
  value: TurkishNounGrammarData | TurkishVerbGrammarData | undefined
  onChange: (value: TurkishNounGrammarData | TurkishVerbGrammarData) => void
}

export function TurkishGrammarFields({ partOfSpeech, value, onChange }: TurkishGrammarFieldsProps) {
  const { t } = useTranslation()

  if (partOfSpeech === 'Noun') {
    const noun = value as TurkishNounGrammarData | undefined
    const setField = (patch: Partial<TurkishNounGrammarData>) =>
      onChange({
        plural: '',
        cases: { nominative: '', accusative: '', dative: '', locative: '', ablative: '', genitive: '' },
        vowelHarmony: 'kalın',
        possessive: { ben: '', sen: '', o: '', biz: '', siz: '', onlar: '' },
        ...noun,
        ...patch,
      })

    return (
      <div className="flex flex-col gap-3">
        <div>
          <label className="mb-1 block text-sm font-medium text-text">
            {t('words.grammar.tr.plural')} <span className="text-destructive">*</span>
          </label>
          <input
            type="text"
            value={noun?.plural ?? ''}
            onChange={(e) => setField({ plural: e.target.value })}
            className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
          />
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-text">
            {t('words.grammar.tr.vowelHarmony')} <span className="text-destructive">*</span>
          </label>
          <select
            value={noun?.vowelHarmony ?? 'kalın'}
            onChange={(e) => setField({ vowelHarmony: e.target.value as TurkishNounGrammarData['vowelHarmony'] })}
            className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
          >
            <option value="kalın">{t('words.grammar.tr.vowelHarmonyThick')}</option>
            <option value="ince">{t('words.grammar.tr.vowelHarmonyThin')}</option>
          </select>
        </div>
        <div>
          <p className="mb-1 text-sm font-medium text-text">
            {t('words.grammar.tr.cases')} <span className="text-destructive">*</span>
          </p>
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
            {TR_CASE_KEYS.map((caseKey) => (
              <div key={caseKey}>
                <label className="mb-1 block text-xs text-muted">{t(`words.grammar.tr.caseLabels.${caseKey}`)}</label>
                <input
                  type="text"
                  value={noun?.cases?.[caseKey] ?? ''}
                  onChange={(e) => setField({ cases: { ...noun?.cases, [caseKey]: e.target.value } as TurkishNounGrammarData['cases'] })}
                  className="w-full rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
                />
              </div>
            ))}
          </div>
        </div>
        <div>
          <p className="mb-1 text-sm font-medium text-text">
            {t('words.grammar.tr.possessive')} <span className="text-destructive">*</span>
          </p>
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-3">
            {TR_PERSON_KEYS.map((person) => (
              <div key={person}>
                <label className="mb-1 block text-xs text-muted">{t(`words.grammar.tr.personLabels.${person}`)}</label>
                <input
                  type="text"
                  value={noun?.possessive?.[person] ?? ''}
                  onChange={(e) =>
                    setField({ possessive: { ...noun?.possessive, [person]: e.target.value } as TurkishNounGrammarData['possessive'] })
                  }
                  className="w-full rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
                />
              </div>
            ))}
          </div>
        </div>
        {/* A-05.2: consonantMutation bilinçli olarak opsiyonel/doğrulanmamış (§8'deki henüz
            yazılmamış quiz özelliği için) — WordGrammarValidator bunu hiç kontrol etmiyor. */}
        <div>
          <label className="flex items-center gap-2 text-sm font-medium text-text">
            <input
              type="checkbox"
              checked={noun?.consonantMutation?.hasChange ?? false}
              onChange={(e) =>
                setField({
                  consonantMutation: e.target.checked
                    ? { hasChange: true, pattern: noun?.consonantMutation?.pattern ?? '', example: noun?.consonantMutation?.example ?? '' }
                    : undefined,
                })
              }
            />
            {t('words.grammar.tr.consonantMutation')}
            <span className="text-xs text-muted">({t('words.grammar.optional')})</span>
          </label>
          {noun?.consonantMutation?.hasChange && (
            <div className="mt-2 grid grid-cols-2 gap-2">
              <input
                type="text"
                placeholder={t('words.grammar.tr.consonantMutationPattern') ?? undefined}
                value={noun.consonantMutation.pattern}
                onChange={(e) => setField({ consonantMutation: { ...noun.consonantMutation!, pattern: e.target.value } })}
                className="rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
              />
              <input
                type="text"
                placeholder={t('words.grammar.tr.consonantMutationExample') ?? undefined}
                value={noun.consonantMutation.example}
                onChange={(e) => setField({ consonantMutation: { ...noun.consonantMutation!, example: e.target.value } })}
                className="rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
              />
            </div>
          )}
        </div>
      </div>
    )
  }

  // partOfSpeech === 'Verb'
  const verb = value as TurkishVerbGrammarData | undefined
  const setVerbField = (patch: Partial<TurkishVerbGrammarData>) =>
    onChange({
      verbRoot: '',
      negativeForm: '',
      conjugation: {
        presentContinuous: { ben: '', sen: '', o: '', biz: '', siz: '', onlar: '' },
        aorist: { ben: '', sen: '', o: '', biz: '', siz: '', onlar: '' },
        pastDefinite: { ben: '', sen: '', o: '', biz: '', siz: '', onlar: '' },
        pastNarrative: { ben: '', sen: '', o: '', biz: '', siz: '', onlar: '' },
        future: { ben: '', sen: '', o: '', biz: '', siz: '', onlar: '' },
      },
      ...verb,
      ...patch,
    })

  return (
    <div className="flex flex-col gap-3">
      <div>
        <label className="mb-1 block text-sm font-medium text-text">
          {t('words.grammar.tr.verbRoot')} <span className="text-destructive">*</span>
        </label>
        <input
          type="text"
          value={verb?.verbRoot ?? ''}
          onChange={(e) => setVerbField({ verbRoot: e.target.value })}
          className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
        />
      </div>
      <div>
        <label className="mb-1 block text-sm font-medium text-text">
          {t('words.grammar.tr.negativeForm')} <span className="text-destructive">*</span>
        </label>
        <input
          type="text"
          value={verb?.negativeForm ?? ''}
          onChange={(e) => setVerbField({ negativeForm: e.target.value })}
          className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
        />
      </div>
      <div>
        <p className="mb-1 text-sm font-medium text-text">
          {t('words.grammar.tr.conjugation')} <span className="text-destructive">*</span>
        </p>
        <ConjugationGrid
          tenses={TR_VERB_TENSE_KEYS.map((key) => ({ key, label: t(`words.grammar.tr.tenseLabels.${key}`) }))}
          persons={TR_PERSON_KEYS.map((key) => ({ key, label: t(`words.grammar.tr.personLabels.${key}`) }))}
          value={verb?.conjugation}
          onChange={(conjugation) => setVerbField({ conjugation: conjugation as TurkishVerbGrammarData['conjugation'] })}
        />
      </div>
    </div>
  )
}
