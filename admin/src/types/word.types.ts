// Backend'in WordDtos.cs / CreateWordCommand.cs / UpdateWordCommand.cs / GetWordsQuery.cs /
// GetUnmatchedWordConceptsQuery.cs / PairWordConceptsCommand.cs alanlarını birebir yansıtır.

export type PartOfSpeech =
  | 'Noun'
  | 'Verb'
  | 'Adjective'
  | 'Adverb'
  | 'Conjunction'
  | 'Preposition'
  | 'Pronoun'
  | 'Other'

// CK_WordConcepts_Level / CK_WordExamples_Level ile birebir (backend DB CHECK constraint'i).
export type DifficultyLevel = 'A1' | 'A2' | 'B1' | 'B2' | 'C1' | 'C2'

// CK_WordExamples_ExampleType ile birebir.
export type ExampleType = 'Normal' | 'Idiom' | 'Formal' | 'Colloquial'

export type LanguageCode = 'de' | 'tr'

// --- GrammarData (WordDetail.GrammarData JSON) ---
// GERMAN_LANGUAGE_FEATURES.md §10 / TURKISH_LANGUAGE_FEATURES.md §9 — backend WordGrammarValidator'ın
// TS karşılığı. Hangi tipin geçerli olduğu dışarıdan (languageCode + partOfSpeech) belirlenir, JSON'un
// kendisinde bir "tur" alanı YOK (backend de bunu taşımıyor).

export interface GermanConjugationRow {
  ich: string
  du: string
  erSieEs: string
  wir: string
  ihr: string
  sie: string
}

export interface GermanNounGrammarData {
  // Backend validator değeri doğrulamıyor (yalnızca boş olmadığını) — "Masculine"/"Feminine"/"Neuter"
  // API_ENDPOINTS.md örneğinden gelen kabul edilmiş konvansiyon, DB CHECK'i YOK.
  gender: 'Masculine' | 'Feminine' | 'Neuter'
  plural: string
  cases: {
    nominative: string
    accusative: string
    dative: string
    genitive: string
  }
}

export interface GermanVerbGrammarData {
  isSeparableVerb: boolean
  // isSeparableVerb=true iken zorunlu, false iken boş kalmalı (WordGrammarValidator).
  separablePrefix?: string
  auxiliaryVerb: string
  pastParticiple: string
  conjugation: {
    present: GermanConjugationRow
    preterite: GermanConjugationRow
    perfect: GermanConjugationRow
  }
}

export interface TurkishConjugationRow {
  ben: string
  sen: string
  o: string
  biz: string
  siz: string
  onlar: string
}

export interface TurkishPossessive {
  ben: string
  sen: string
  o: string
  biz: string
  siz: string
  onlar: string
}

// A-05.2: consonantMutation bilinçli olarak opsiyonel/doğrulanmamış — yalnızca gelecekteki bir
// quiz özelliğinde (TURKISH_LANGUAGE_FEATURES.md §8) kullanılacak, WordGrammarValidator bunu
// hiç kontrol etmiyor.
export interface TurkishConsonantMutation {
  hasChange: boolean
  pattern: string
  example: string
}

export interface TurkishNounGrammarData {
  plural: string
  cases: {
    nominative: string
    accusative: string
    dative: string
    locative: string
    ablative: string
    genitive: string
  }
  vowelHarmony: 'kalın' | 'ince'
  possessive: TurkishPossessive
  consonantMutation?: TurkishConsonantMutation
}

export interface TurkishVerbGrammarData {
  verbRoot: string
  negativeForm: string
  conjugation: {
    presentContinuous: TurkishConjugationRow
    aorist: TurkishConjugationRow
    pastDefinite: TurkishConjugationRow
    pastNarrative: TurkishConjugationRow
    future: TurkishConjugationRow
  }
}

export type GrammarData =
  | GermanNounGrammarData
  | GermanVerbGrammarData
  | TurkishNounGrammarData
  | TurkishVerbGrammarData

// --- Okuma DTO'ları (backend WordDtos.cs) ---

export interface WordDetail {
  pronunciation: string | null
  audioUrl: string | null
  notes: string | null
  commonMistakes: string | null
  grammarData: GrammarData | null
}

export interface WordExample {
  id: number
  sentenceText: string
  level: DifficultyLevel
  exampleType: ExampleType
  pairedExampleId: number | null
}

export interface WordTranslationSummary {
  languageCode: LanguageCode
  text: string
}

export interface WordTranslation {
  languageCode: LanguageCode
  text: string
  definition: string | null
  wordDetail: WordDetail | null
  examples: WordExample[]
}

export interface CategoryTranslation {
  languageCode: LanguageCode
  name: string
  description: string | null
}

export interface WordCategorySummary {
  categoryId: number
  translations: CategoryTranslation[]
}

export interface WordListItem {
  wordConceptId: number
  partOfSpeech: PartOfSpeech
  difficultyLevel: DifficultyLevel
  imageUrl: string | null
  translations: WordTranslationSummary[]
  categories: WordCategorySummary[]
}

export interface WordConceptDetail {
  wordConceptId: number
  partOfSpeech: PartOfSpeech
  difficultyLevel: DifficultyLevel
  imageUrl: string | null
  translations: WordTranslation[]
  categories: WordCategorySummary[]
}

export interface UnmatchedWordConcept {
  wordConceptId: number
  languageCode: LanguageCode
  text: string
  definition: string | null
  partOfSpeech: PartOfSpeech
  difficultyLevel: DifficultyLevel
  suggestedMatchConceptId: number | null
}

export interface Language {
  id: number
  code: LanguageCode
  name: string
  nativeName: string
}

// --- Yazma girdileri (backend CreateWordCommand.cs / UpdateWordCommand.cs) ---

export interface WordExampleInput {
  sentenceText: string
  level: DifficultyLevel
  exampleType: ExampleType
}

export interface WordDetailInput {
  pronunciation?: string | null
  audioUrl?: string | null
  notes?: string | null
  commonMistakes?: string | null
  grammarData?: GrammarData | null
}

export interface WordTranslationInput {
  languageCode: LanguageCode
  text: string
  definition?: string | null
  wordDetail?: WordDetailInput | null
  examples?: WordExampleInput[] | null
}

export interface CreateWordRequest {
  partOfSpeech: PartOfSpeech
  difficultyLevel: DifficultyLevel
  imageUrl?: string | null
  translations: WordTranslationInput[]
  categoryIds?: number[] | null
}

export interface UpdateWordRequest {
  id: number
  partOfSpeech: PartOfSpeech
  difficultyLevel: DifficultyLevel
  imageUrl?: string | null
  translations: WordTranslationInput[]
  categoryIds?: number[] | null
}

// --- Liste/filtre sorguları ---

export interface GetWordsParams {
  level?: DifficultyLevel
  partOfSpeech?: PartOfSpeech
  search?: string
  categoryId?: number
  page?: number
  pageSize?: number
}

export interface GetUnmatchedWordConceptsParams {
  languageId: number
  search?: string
  page?: number
  pageSize?: number
}

export interface PairWordConceptsRequest {
  primaryId: number
  otherConceptId: number
}

// Backend'in PagedResult<T>'iyle (Application/Common/Models/PagedResult.cs) birebir.
export interface PagedResult<T> {
  items: T[]
  totalCount: number
  page: number
  pageSize: number
}

// Formda gösterilen ortak alanlar — WordFormModal'ın Formik initialValues/submit tipi.
export interface WordFormValues {
  partOfSpeech: PartOfSpeech
  difficultyLevel: DifficultyLevel
  imageUrl: string
  categoryIds: number[]
  translations: WordTranslationInput[]
}
