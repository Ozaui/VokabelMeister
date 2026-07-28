import { describe, it, expect } from 'vitest'
import wordFilterReducer, {
  setSearch,
  setLevel,
  setPartOfSpeech,
  setCategoryId,
  setPage,
  resetWordFilters,
} from './wordFilterSlice'

describe('wordFilterSlice', () => {
  it('ilk durumda filtre boş, sayfa 1\'dir', () => {
    const state = wordFilterReducer(undefined, { type: '@@INIT' })
    expect(state).toEqual({ search: '', level: null, partOfSpeech: null, categoryId: null, page: 1 })
  })

  it('sayfa 3\'teyken arama değişince sayfa 1\'e döner', () => {
    const onPage3 = wordFilterReducer(undefined, setPage(3))
    const state = wordFilterReducer(onPage3, setSearch('Tisch'))

    expect(state.search).toBe('Tisch')
    expect(state.page).toBe(1)
  })

  it('sayfa 3\'teyken level değişince sayfa 1\'e döner', () => {
    const onPage3 = wordFilterReducer(undefined, setPage(3))
    const state = wordFilterReducer(onPage3, setLevel('A1'))

    expect(state.level).toBe('A1')
    expect(state.page).toBe(1)
  })

  it('sayfa 3\'teyken partOfSpeech değişince sayfa 1\'e döner', () => {
    const onPage3 = wordFilterReducer(undefined, setPage(3))
    const state = wordFilterReducer(onPage3, setPartOfSpeech('Noun'))

    expect(state.partOfSpeech).toBe('Noun')
    expect(state.page).toBe(1)
  })

  it('sayfa 3\'teyken categoryId değişince sayfa 1\'e döner', () => {
    const onPage3 = wordFilterReducer(undefined, setPage(3))
    const state = wordFilterReducer(onPage3, setCategoryId(5))

    expect(state.categoryId).toBe(5)
    expect(state.page).toBe(1)
  })

  it('setPage yalnızca sayfayı değiştirir, filtrelere dokunmaz', () => {
    const withFilter = wordFilterReducer(undefined, setSearch('Tisch'))
    const state = wordFilterReducer(withFilter, setPage(2))

    expect(state.page).toBe(2)
    expect(state.search).toBe('Tisch')
  })

  it('resetWordFilters tüm filtreleri ve sayfayı başlangıç durumuna döndürür', () => {
    const filled = wordFilterReducer(
      wordFilterReducer(wordFilterReducer(undefined, setSearch('Tisch')), setLevel('A1')),
      setPage(4),
    )
    const state = wordFilterReducer(filled, resetWordFilters())

    expect(state).toEqual({ search: '', level: null, partOfSpeech: null, categoryId: null, page: 1 })
  })
})
