import { apiClient } from '../api'
import { useApiQuery } from '../../hooks/useApiQuery'
import type { Category } from '../../types/category.types'

// Yalnızca okuma — B-04 (Kategori Yönetimi) bu dosyaya create/update/delete çağrılarını
// ekleyecek. B-03'ün WordFormModal'ındaki CategoryPicker (kategori seçimi) için GET /categories
// (A-06'da zaten yazılmış) yeterli.
async function fetchCategories(): Promise<Category[]> {
  const { data } = await apiClient.get<Category[]>('/categories')
  return data
}

export function useGetCategoriesQuery() {
  return useApiQuery(fetchCategories, undefined)
}
