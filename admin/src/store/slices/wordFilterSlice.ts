import { createSlice, type PayloadAction } from '@reduxjs/toolkit'
import type { DifficultyLevel, PartOfSpeech } from '../../types/word.types'

interface WordFilterState {
  search: string
  level: DifficultyLevel | null
  partOfSpeech: PartOfSpeech | null
  categoryId: number | null
  page: number
}

const initialState: WordFilterState = {
  search: '',
  level: null,
  partOfSpeech: null,
  categoryId: null,
  page: 1,
}

const wordFilterSlice = createSlice({
  name: 'wordFilter',
  initialState,
  reducers: {
    // Arama/filtre değiştiğinde sayfa 1'e döner — aksi hâlde 3. sayfadayken filtre değiştirilirse
    // yeni sonuç kümesinde var olmayan bir sayfada boş bir liste görülür.
    setSearch: (state, action: PayloadAction<string>) => {
      state.search = action.payload
      state.page = 1
    },
    setLevel: (state, action: PayloadAction<DifficultyLevel | null>) => {
      state.level = action.payload
      state.page = 1
    },
    setPartOfSpeech: (state, action: PayloadAction<PartOfSpeech | null>) => {
      state.partOfSpeech = action.payload
      state.page = 1
    },
    setCategoryId: (state, action: PayloadAction<number | null>) => {
      state.categoryId = action.payload
      state.page = 1
    },
    setPage: (state, action: PayloadAction<number>) => {
      state.page = action.payload
    },
    resetWordFilters: () => initialState,
  },
})

export const { setSearch, setLevel, setPartOfSpeech, setCategoryId, setPage, resetWordFilters } =
  wordFilterSlice.actions
export default wordFilterSlice.reducer
