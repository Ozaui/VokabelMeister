import { useTranslation } from 'react-i18next'
import { ConjugationGrid } from './ConjugationGrid'
import type { GermanNounGrammarData, GermanVerbGrammarData, PartOfSpeech } from '../../types/word.types'

// GERMAN_LANGUAGE_FEATURES.md §10'un TS karşılığı — backend WordGrammarValidator'ın de dalıyla
// AYNI zorunlu/koşullu/yasak matrisi, kod paylaşımı yok (iki ayrı katman, bilinçli tekrar).
// Yalnızca GrammarData JSON alan adları (WordGrammarValidator ile birebir) — GÖSTERİLEN etiketler
// i18n'den (words.grammar.de.tenseLabels/personLabels/caseLabels) gelir, CLAUDE.md §1 "admin
// panelin kendi statik arayüz metinleri react-i18next ile çevrilir" kuralı burada da geçerli.
const DE_VERB_TENSE_KEYS = ['present', 'preterite', 'perfect'] as const
const DE_PERSON_KEYS = ['ich', 'du', 'erSieEs', 'wir', 'ihr', 'sie'] as const
const DE_CASE_KEYS = ['nominative', 'accusative', 'dative', 'genitive'] as const

interface GermanGrammarFieldsProps {
  partOfSpeech: PartOfSpeech
  value: GermanNounGrammarData | GermanVerbGrammarData | undefined
  onChange: (value: GermanNounGrammarData | GermanVerbGrammarData) => void
}

export function GermanGrammarFields({ partOfSpeech, value, onChange }: GermanGrammarFieldsProps) {
  const { t } = useTranslation()

  if (partOfSpeech === 'Noun') {
    const noun = value as GermanNounGrammarData | undefined
    const setField = (patch: Partial<GermanNounGrammarData>) =>
      onChange({ gender: 'Masculine', plural: '', cases: { nominative: '', accusative: '', dative: '', genitive: '' }, ...noun, ...patch })

    return (
      <div className="flex flex-col gap-3">
        <div>
          <label className="mb-1 block text-sm font-medium text-text">
            {t('words.grammar.de.gender')} <span className="text-destructive">*</span>
          </label>
          <select
            value={noun?.gender ?? 'Masculine'}
            onChange={(e) => setField({ gender: e.target.value as GermanNounGrammarData['gender'] })}
            className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
          >
            <option value="Masculine">der ({t('words.grammar.de.masculine')})</option>
            <option value="Feminine">die ({t('words.grammar.de.feminine')})</option>
            <option value="Neuter">das ({t('words.grammar.de.neuter')})</option>
          </select>
        </div>
        <div>
          <label className="mb-1 block text-sm font-medium text-text">
            {t('words.grammar.de.plural')} <span className="text-destructive">*</span>
          </label>
          <input
            type="text"
            value={noun?.plural ?? ''}
            onChange={(e) => setField({ plural: e.target.value })}
            className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
          />
        </div>
        <div>
          <p className="mb-1 text-sm font-medium text-text">
            {t('words.grammar.de.cases')} <span className="text-destructive">*</span>
          </p>
          <div className="grid grid-cols-2 gap-2 sm:grid-cols-4">
            {DE_CASE_KEYS.map((caseKey) => (
              <div key={caseKey}>
                <label className="mb-1 block text-xs text-muted">{t(`words.grammar.de.caseLabels.${caseKey}`)}</label>
                <input
                  type="text"
                  value={noun?.cases?.[caseKey] ?? ''}
                  onChange={(e) =>
                    setField({
                      cases: {
                        nominative: '',
                        accusative: '',
                        dative: '',
                        genitive: '',
                        ...noun?.cases,
                        [caseKey]: e.target.value,
                      },
                    })
                  }
                  className="w-full rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
                />
              </div>
            ))}
          </div>
        </div>
      </div>
    )
  }

  // partOfSpeech === 'Verb'
  const verb = value as GermanVerbGrammarData | undefined
  const setVerbField = (patch: Partial<GermanVerbGrammarData>) =>
    onChange({
      isSeparableVerb: false,
      auxiliaryVerb: '',
      pastParticiple: '',
      conjugation: {
        present: { ich: '', du: '', erSieEs: '', wir: '', ihr: '', sie: '' },
        preterite: { ich: '', du: '', erSieEs: '', wir: '', ihr: '', sie: '' },
        perfect: { ich: '', du: '', erSieEs: '', wir: '', ihr: '', sie: '' },
      },
      ...verb,
      ...patch,
    })

  return (
    <div className="flex flex-col gap-3">
      <label className="flex items-center gap-2 text-sm font-medium text-text">
        <input
          type="checkbox"
          checked={verb?.isSeparableVerb ?? false}
          onChange={(e) =>
            setVerbField({
              isSeparableVerb: e.target.checked,
              separablePrefix: e.target.checked ? verb?.separablePrefix : undefined,
            })
          }
        />
        {t('words.grammar.de.isSeparableVerb')} <span className="text-destructive">*</span>
      </label>
      {verb?.isSeparableVerb && (
        <div>
          <label className="mb-1 block text-sm font-medium text-text">
            {t('words.grammar.de.separablePrefix')} <span className="text-destructive">*</span>
          </label>
          <input
            type="text"
            value={verb?.separablePrefix ?? ''}
            onChange={(e) => setVerbField({ separablePrefix: e.target.value })}
            className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
          />
        </div>
      )}
      <div>
        <label className="mb-1 block text-sm font-medium text-text">
          {t('words.grammar.de.auxiliaryVerb')} <span className="text-destructive">*</span>
        </label>
        <input
          type="text"
          value={verb?.auxiliaryVerb ?? ''}
          onChange={(e) => setVerbField({ auxiliaryVerb: e.target.value })}
          className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
        />
      </div>
      <div>
        <label className="mb-1 block text-sm font-medium text-text">
          {t('words.grammar.de.pastParticiple')} <span className="text-destructive">*</span>
        </label>
        <input
          type="text"
          value={verb?.pastParticiple ?? ''}
          onChange={(e) => setVerbField({ pastParticiple: e.target.value })}
          className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
        />
      </div>
      <div>
        <p className="mb-1 text-sm font-medium text-text">
          {t('words.grammar.de.conjugation')} <span className="text-destructive">*</span>
        </p>
        <ConjugationGrid
          tenses={DE_VERB_TENSE_KEYS.map((key) => ({ key, label: t(`words.grammar.de.tenseLabels.${key}`) }))}
          persons={DE_PERSON_KEYS.map((key) => ({ key, label: t(`words.grammar.de.personLabels.${key}`) }))}
          value={verb?.conjugation}
          onChange={(conjugation) =>
            setVerbField({ conjugation: conjugation as GermanVerbGrammarData['conjugation'] })
          }
        />
      </div>
    </div>
  )
}
