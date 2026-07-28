// Backend'in CategoryDtos.cs'iyle (A-06, zaten yazılmış) birebir — B-04 (Kategori Yönetimi CRUD
// sayfaları) bu tipi genişletecek, burada yalnızca B-03'ün CategoryPicker'ının ihtiyaç duyduğu
// okuma şekli var.
import type { CategoryTranslation } from './word.types'

export interface Category {
  id: number
  parentCategoryId: number | null
  displayOrder: number
  icon: string | null
  color: string | null
  minLevel: string | null
  maxLevel: string | null
  translations: CategoryTranslation[]
  wordCount: number | null
  children: Category[]
}
