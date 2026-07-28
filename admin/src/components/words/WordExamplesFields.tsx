import { useTranslation } from 'react-i18next'
import type { DifficultyLevel, ExampleType, WordExampleInput } from '../../types/word.types'

interface WordExamplesFieldsProps {
  value: WordExampleInput[]
  onChange: (value: WordExampleInput[]) => void
}

const LEVELS: DifficultyLevel[] = ['A1', 'A2', 'B1', 'B2', 'C1', 'C2']
const EXAMPLE_TYPES: ExampleType[] = ['Normal', 'Idiom', 'Formal', 'Colloquial']

// UpdateWordCommand yalnızca EKLEME yapar (mevcut örnekleri silme/güncelleme kapsam dışı, A-05
// bilinçli bir YAGNI kararıydı) — bu yüzden düzenleme modunda da bu liste zaten SIFIRDAN başlar,
// var olan örnekler backend'e AYRICA gönderilmez (WordFormModal onları salt-okunur ayrıca gösterir).
export function WordExamplesFields({ value, onChange }: WordExamplesFieldsProps) {
  const { t } = useTranslation()

  const addExample = () => onChange([...value, { sentenceText: '', level: 'A1', exampleType: 'Normal' }])
  const removeExample = (index: number) => onChange(value.filter((_, i) => i !== index))
  const updateExample = (index: number, patch: Partial<WordExampleInput>) =>
    onChange(value.map((example, i) => (i === index ? { ...example, ...patch } : example)))

  return (
    <div className="flex flex-col gap-2">
      {value.map((example, index) => (
        <div key={index} className="flex flex-col gap-2 rounded-control border border-border p-2 sm:flex-row sm:items-start">
          <input
            type="text"
            value={example.sentenceText}
            onChange={(e) => updateExample(index, { sentenceText: e.target.value })}
            placeholder={t('words.examples.sentencePlaceholder') ?? undefined}
            className="flex-1 rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
          />
          <select
            value={example.level}
            onChange={(e) => updateExample(index, { level: e.target.value as DifficultyLevel })}
            className="rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
          >
            {LEVELS.map((level) => (
              <option key={level} value={level}>
                {level}
              </option>
            ))}
          </select>
          <select
            value={example.exampleType}
            onChange={(e) => updateExample(index, { exampleType: e.target.value as ExampleType })}
            className="rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
          >
            {EXAMPLE_TYPES.map((type) => (
              <option key={type} value={type}>
                {t(`words.examples.type.${type}`)}
              </option>
            ))}
          </select>
          <button
            type="button"
            onClick={() => removeExample(index)}
            className="rounded-control px-2 py-1 text-sm text-destructive hover:bg-destructive/10"
          >
            {t('action.remove')}
          </button>
        </div>
      ))}
      <button
        type="button"
        onClick={addExample}
        className="self-start rounded-control border border-border px-3 py-1 text-sm text-text hover:bg-background"
      >
        + {t('words.examples.add')}
      </button>
    </div>
  )
}
