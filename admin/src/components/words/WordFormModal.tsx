import { useState } from 'react'
import { useFormik, type FormikErrors, type FormikTouched } from 'formik'
import * as Yup from 'yup'
import { useTranslation } from 'react-i18next'
import type { TFunction } from 'i18next'
import { useCreateWordMutation, useUpdateWordMutation } from '../../store/api/wordsApi'
import { getApiErrorCode, getApiErrorMessage } from '../../lib/apiError'
import { GermanGrammarFields } from './GermanGrammarFields'
import { TurkishGrammarFields } from './TurkishGrammarFields'
import { WordExamplesFields } from './WordExamplesFields'
import { CategoryPicker } from './CategoryPicker'
import type {
  DifficultyLevel,
  GermanNounGrammarData,
  GermanVerbGrammarData,
  PartOfSpeech,
  TurkishNounGrammarData,
  TurkishVerbGrammarData,
  WordConceptDetail,
  WordFormValues,
} from '../../types/word.types'

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
const LEVELS: DifficultyLevel[] = ['A1', 'A2', 'B1', 'B2', 'C1', 'C2']

function buildWordFormSchema(t: TFunction) {
  return Yup.object({
    partOfSpeech: Yup.string().required(),
    difficultyLevel: Yup.string().required(),
    imageUrl: Yup.string(),
    categoryIds: Yup.array().of(Yup.number().required()).required(),
    translations: Yup.array()
      .of(
        Yup.object({
          languageCode: Yup.string().required(),
          text: Yup.string().required(t('words.form.textRequired')),
          definition: Yup.string(),
        }),
      )
      .required(),
  })
}

interface WordFormModalProps {
  // Verilmezse "ekle" modu (yeni WordConcept, tek dil seçilerek başlar) — verilirse "düzenle"
  // modu (concept'in var olan çevirileri düzenlenir, yeni dil EKLENMEZ — o WordPairingPage'in işi).
  initialData?: WordConceptDetail
  onClose: () => void
}

// Yup translations[].text hatası Formik'in FormikErrors&lt;T&gt; derin/opsiyonel tipinde gelir —
// admin bir çeviriyi boş bırakıp kaydedince "Kaydet" sessizce hiçbir şey yapmasın diye burada
// güvenli bir şekilde çözülür (aksi halde buton neden tepki vermediği anlaşılmazdı).
function translationTextError(
  errors: FormikErrors<WordFormValues>,
  touched: FormikTouched<WordFormValues>,
  index: number,
): string | undefined {
  if (!touched.translations?.[index]?.text) return undefined
  const entry = errors.translations?.[index]
  return typeof entry === 'object' && entry ? entry.text : undefined
}

function toFormValues(initialData: WordConceptDetail | undefined): WordFormValues {
  if (!initialData) {
    return {
      partOfSpeech: 'Noun',
      difficultyLevel: 'A1',
      imageUrl: '',
      categoryIds: [],
      translations: [{ languageCode: 'de', text: '', definition: '', wordDetail: undefined, examples: [] }],
    }
  }

  return {
    partOfSpeech: initialData.partOfSpeech,
    difficultyLevel: initialData.difficultyLevel,
    imageUrl: initialData.imageUrl ?? '',
    categoryIds: initialData.categories.map((c) => c.categoryId),
    translations: initialData.translations.map((translation) => ({
      languageCode: translation.languageCode,
      text: translation.text,
      definition: translation.definition ?? '',
      wordDetail: translation.wordDetail
        ? {
            pronunciation: translation.wordDetail.pronunciation ?? '',
            audioUrl: translation.wordDetail.audioUrl ?? '',
            notes: translation.wordDetail.notes ?? '',
            commonMistakes: translation.wordDetail.commonMistakes ?? '',
            grammarData: translation.wordDetail.grammarData ?? undefined,
          }
        : undefined,
      // UpdateWordCommand yalnızca EKLEME yapar (bkz. WordExamplesFields yorumu) — var olan
      // örnekler burada tekrar gönderilmez, liste boş başlar.
      examples: [],
    })),
  }
}

export function WordFormModal({ initialData, onClose }: WordFormModalProps) {
  const { t } = useTranslation()
  const isEdit = !!initialData
  const [createWord, { isLoading: isCreating }] = useCreateWordMutation()
  const [updateWord, { isLoading: isUpdating }] = useUpdateWordMutation()
  const [formError, setFormError] = useState<string | null>(null)
  const [duplicatePending, setDuplicatePending] = useState<WordFormValues | null>(null)
  const isSubmitting = isCreating || isUpdating

  const submit = async (values: WordFormValues, force: boolean) => {
    setFormError(null)
    const hasGrammar = values.partOfSpeech === 'Noun' || values.partOfSpeech === 'Verb'
    const payload = {
      partOfSpeech: values.partOfSpeech,
      difficultyLevel: values.difficultyLevel,
      imageUrl: values.imageUrl || null,
      categoryIds: values.categoryIds,
      translations: values.translations.map((translation) => ({
        languageCode: translation.languageCode,
        text: translation.text,
        definition: translation.definition || null,
        // Noun/Verb'de grammarData ZORUNLU (WordGrammarValidator GRAMMAR_DATA_REQUIRED), diğer
        // türlerde tamamen NULL olmalı (GRAMMAR_DATA_MUST_BE_NULL_FOR_OTHER) — wordDetail'in
        // diğer alanları (telaffuz/ses/not) türden bağımsız her zaman gönderilebilir.
        wordDetail: translation.wordDetail
          ? {
              pronunciation: translation.wordDetail.pronunciation || null,
              audioUrl: translation.wordDetail.audioUrl || null,
              notes: translation.wordDetail.notes || null,
              commonMistakes: translation.wordDetail.commonMistakes || null,
              grammarData: hasGrammar ? (translation.wordDetail.grammarData ?? null) : null,
            }
          : hasGrammar
            ? { pronunciation: null, audioUrl: null, notes: null, commonMistakes: null, grammarData: null }
            : null,
        examples: translation.examples && translation.examples.length > 0 ? translation.examples : undefined,
      })),
    }

    try {
      if (isEdit) {
        await updateWord({ body: { id: initialData!.wordConceptId, ...payload }, force })
      } else {
        await createWord({ body: payload, force })
      }
      setDuplicatePending(null)
      onClose()
    } catch (err) {
      if (!force && getApiErrorCode(err) === 'WORD_TEXT_ALREADY_EXISTS') {
        setDuplicatePending(values)
        return
      }
      setDuplicatePending(null)
      setFormError(getApiErrorMessage(err) ?? t('auth.genericError'))
    }
  }

  const formik = useFormik<WordFormValues>({
    initialValues: toFormValues(initialData),
    validationSchema: buildWordFormSchema(t),
    onSubmit: (values) => submit(values, false),
  })

  const partOfSpeech = formik.values.partOfSpeech
  const hasGrammar = partOfSpeech === 'Noun' || partOfSpeech === 'Verb'

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <form
        onSubmit={formik.handleSubmit}
        noValidate
        className="flex max-h-[90vh] w-full max-w-2xl flex-col overflow-y-auto rounded-card border border-border bg-surface p-6 shadow-sm"
      >
        <h2 className="mb-4 font-heading text-lg font-bold text-text">
          {isEdit ? t('words.form.editTitle') : t('words.form.createTitle')}
        </h2>

        {formError && (
          <p role="alert" className="mb-4 rounded-control bg-destructive/10 px-3 py-2 text-sm text-destructive">
            {formError}
          </p>
        )}

        {duplicatePending && (
          <div role="alert" className="mb-4 rounded-control bg-warning/10 px-3 py-2 text-sm text-text">
            <p className="mb-2">{t('words.form.duplicateWarning')}</p>
            <button
              type="button"
              onClick={() => submit(duplicatePending, true)}
              disabled={isSubmitting}
              className="rounded-control bg-primary px-3 py-1 text-sm font-semibold text-white disabled:opacity-60"
            >
              {t('words.form.duplicateConfirm')}
            </button>
          </div>
        )}

        <div className="mb-4 grid grid-cols-2 gap-3">
          <div>
            <label htmlFor="partOfSpeech" className="mb-1 block text-sm font-medium text-text">
              {t('words.form.partOfSpeech')} <span className="text-destructive">*</span>
            </label>
            <select
              id="partOfSpeech"
              name="partOfSpeech"
              value={formik.values.partOfSpeech}
              onChange={formik.handleChange}
              className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
            >
              {PARTS_OF_SPEECH.map((pos) => (
                <option key={pos} value={pos}>
                  {t(`words.partOfSpeech.${pos}`)}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label htmlFor="difficultyLevel" className="mb-1 block text-sm font-medium text-text">
              {t('words.form.difficultyLevel')} <span className="text-destructive">*</span>
            </label>
            <select
              id="difficultyLevel"
              name="difficultyLevel"
              value={formik.values.difficultyLevel}
              onChange={formik.handleChange}
              className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
            >
              {LEVELS.map((level) => (
                <option key={level} value={level}>
                  {level}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="mb-4">
          <label htmlFor="imageUrl" className="mb-1 block text-sm font-medium text-text">
            {t('words.form.imageUrl')}
          </label>
          <input
            id="imageUrl"
            name="imageUrl"
            type="text"
            value={formik.values.imageUrl}
            onChange={formik.handleChange}
            className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
          />
        </div>

        <div className="mb-4">
          <label className="mb-1 block text-sm font-medium text-text">{t('words.form.categories')}</label>
          <CategoryPicker
            selectedIds={formik.values.categoryIds}
            onChange={(ids) => formik.setFieldValue('categoryIds', ids)}
          />
        </div>

        {formik.values.translations.map((translation, index) => (
          <fieldset key={index} className="mb-4 rounded-control border border-border p-3">
            <legend className="px-1 text-sm font-semibold text-text">
              {isEdit ? (
                // Düzenleme modunda dil sabit — yeni dil eklemek WordPairingPage'in işi.
                <span>{t(`words.language.${translation.languageCode}`)}</span>
              ) : (
                <select
                  aria-label={t('words.form.language') ?? undefined}
                  name={`translations.${index}.languageCode`}
                  value={translation.languageCode}
                  onChange={formik.handleChange}
                  className="rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
                >
                  <option value="de">{t('words.language.de')}</option>
                  <option value="tr">{t('words.language.tr')}</option>
                </select>
              )}
            </legend>

            <div className="mt-2 mb-3">
              <label htmlFor={`text-${index}`} className="mb-1 block text-sm font-medium text-text">
                {t('words.form.text')} <span className="text-destructive">*</span>
              </label>
              <input
                id={`text-${index}`}
                name={`translations.${index}.text`}
                type="text"
                value={translation.text}
                onChange={formik.handleChange}
                onBlur={formik.handleBlur}
                className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
              />
              {translationTextError(formik.errors, formik.touched, index) && (
                <p className="mt-1 text-xs text-destructive">
                  {translationTextError(formik.errors, formik.touched, index)}
                </p>
              )}
            </div>

            <div className="mb-3">
              <label htmlFor={`definition-${index}`} className="mb-1 block text-sm font-medium text-text">
                {t('words.form.definition')}
              </label>
              <input
                id={`definition-${index}`}
                name={`translations.${index}.definition`}
                type="text"
                value={translation.definition ?? ''}
                onChange={formik.handleChange}
                className="w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
              />
            </div>

            {hasGrammar && (
              <div className="mb-3">
                <p className="mb-1 text-sm font-semibold text-text">{t('words.form.grammar')}</p>
                {translation.languageCode === 'de' ? (
                  <GermanGrammarFields
                    partOfSpeech={partOfSpeech}
                    value={translation.wordDetail?.grammarData as GermanNounGrammarData | GermanVerbGrammarData | undefined}
                    onChange={(value) => formik.setFieldValue(`translations.${index}.wordDetail.grammarData`, value)}
                  />
                ) : (
                  <TurkishGrammarFields
                    partOfSpeech={partOfSpeech}
                    value={translation.wordDetail?.grammarData as TurkishNounGrammarData | TurkishVerbGrammarData | undefined}
                    onChange={(value) => formik.setFieldValue(`translations.${index}.wordDetail.grammarData`, value)}
                  />
                )}
              </div>
            )}

            <div className="mb-3 grid grid-cols-2 gap-2">
              <div>
                <label className="mb-1 block text-xs text-muted">{t('words.form.pronunciation')}</label>
                <input
                  type="text"
                  name={`translations.${index}.wordDetail.pronunciation`}
                  value={translation.wordDetail?.pronunciation ?? ''}
                  onChange={formik.handleChange}
                  className="w-full rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
                />
              </div>
              <div>
                <label className="mb-1 block text-xs text-muted">{t('words.form.audioUrl')}</label>
                <input
                  type="text"
                  name={`translations.${index}.wordDetail.audioUrl`}
                  value={translation.wordDetail?.audioUrl ?? ''}
                  onChange={formik.handleChange}
                  className="w-full rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
                />
              </div>
              <div className="col-span-2">
                <label className="mb-1 block text-xs text-muted">{t('words.form.notes')}</label>
                <input
                  type="text"
                  name={`translations.${index}.wordDetail.notes`}
                  value={translation.wordDetail?.notes ?? ''}
                  onChange={formik.handleChange}
                  className="w-full rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
                />
              </div>
              <div className="col-span-2">
                <label className="mb-1 block text-xs text-muted">{t('words.form.commonMistakes')}</label>
                <input
                  type="text"
                  name={`translations.${index}.wordDetail.commonMistakes`}
                  value={translation.wordDetail?.commonMistakes ?? ''}
                  onChange={formik.handleChange}
                  className="w-full rounded-control border border-border bg-background px-2 py-1 text-sm text-text"
                />
              </div>
            </div>

            <div>
              <p className="mb-1 text-sm font-semibold text-text">{t('words.form.examples')}</p>
              <WordExamplesFields
                value={translation.examples ?? []}
                onChange={(value) => formik.setFieldValue(`translations.${index}.examples`, value)}
              />
            </div>
          </fieldset>
        ))}

        <div className="mt-2 flex justify-end gap-2">
          <button
            type="button"
            onClick={onClose}
            className="rounded-control border border-border px-4 py-2 text-sm text-text hover:bg-background"
          >
            {t('action.cancel')}
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="rounded-control bg-primary px-4 py-2 text-sm font-semibold text-white disabled:opacity-60"
          >
            {isSubmitting ? t('words.form.submitting') : t('action.save')}
          </button>
        </div>
      </form>
    </div>
  )
}
