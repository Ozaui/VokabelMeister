import { useTranslation } from 'react-i18next'
import { useGetCategoriesQuery } from '../../store/api/categoriesApi'
import type { Category } from '../../types/category.types'

interface CategoryPickerProps {
  selectedIds: number[]
  onChange: (ids: number[]) => void
}

// Kategori adı admin'in seçili diline göre gösterilir (CategoryTranslation[] içinden ara),
// hiçbiri yoksa (henüz o dilde çeviri girilmemişse) ilk çeviri fallback olur.
function categoryLabel(category: Category, languageCode: string): string {
  const translation =
    category.translations.find((t) => t.languageCode === languageCode) ?? category.translations[0]
  return translation?.name ?? `#${category.id}`
}

function CategoryNode({
  category,
  depth,
  languageCode,
  selectedIds,
  onToggle,
}: {
  category: Category
  depth: number
  languageCode: string
  selectedIds: number[]
  onToggle: (id: number) => void
}) {
  return (
    <>
      <label
        className="flex items-center gap-2 rounded-control px-2 py-1 text-sm text-text hover:bg-background"
        style={{ paddingLeft: `${depth * 1.25 + 0.5}rem` }}
      >
        <input
          type="checkbox"
          checked={selectedIds.includes(category.id)}
          onChange={() => onToggle(category.id)}
        />
        {categoryLabel(category, languageCode)}
      </label>
      {category.children.map((child) => (
        <CategoryNode
          key={child.id}
          category={child}
          depth={depth + 1}
          languageCode={languageCode}
          selectedIds={selectedIds}
          onToggle={onToggle}
        />
      ))}
    </>
  )
}

export function CategoryPicker({ selectedIds, onChange }: CategoryPickerProps) {
  const { t, i18n } = useTranslation()
  const { data: categories, isLoading } = useGetCategoriesQuery()

  const toggle = (id: number) => {
    onChange(selectedIds.includes(id) ? selectedIds.filter((x) => x !== id) : [...selectedIds, id])
  }

  if (isLoading) return <p className="text-sm text-muted">{t('words.categories.loading')}</p>
  if (!categories || categories.length === 0)
    return <p className="text-sm text-muted">{t('words.categories.empty')}</p>

  return (
    <div className="max-h-48 overflow-y-auto rounded-control border border-border bg-background p-1">
      {categories.map((category) => (
        <CategoryNode
          key={category.id}
          category={category}
          depth={0}
          languageCode={i18n.language}
          selectedIds={selectedIds}
          onToggle={toggle}
        />
      ))}
    </div>
  )
}
