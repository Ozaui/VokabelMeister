import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { WordFormModal } from './WordFormModal'
import { useCreateWordMutation, useUpdateWordMutation } from '../../store/api/wordsApi'
import { useGetCategoriesQuery } from '../../store/api/categoriesApi'

vi.mock('../../store/api/wordsApi', () => ({
  useCreateWordMutation: vi.fn(),
  useUpdateWordMutation: vi.fn(),
}))
vi.mock('../../store/api/categoriesApi', () => ({
  useGetCategoriesQuery: vi.fn(),
}))

const mockedUseCreateWordMutation = vi.mocked(useCreateWordMutation)
const mockedUseUpdateWordMutation = vi.mocked(useUpdateWordMutation)
const mockedUseGetCategoriesQuery = vi.mocked(useGetCategoriesQuery)

describe('WordFormModal', () => {
  const createTrigger = vi.fn()
  const updateTrigger = vi.fn()

  beforeEach(() => {
    createTrigger.mockReset().mockResolvedValue({})
    updateTrigger.mockReset().mockResolvedValue({})
    mockedUseCreateWordMutation.mockReturnValue([createTrigger, { isLoading: false }] as unknown as ReturnType<
      typeof useCreateWordMutation
    >)
    mockedUseUpdateWordMutation.mockReturnValue([updateTrigger, { isLoading: false }] as unknown as ReturnType<
      typeof useUpdateWordMutation
    >)
    mockedUseGetCategoriesQuery.mockReturnValue({ data: [], isLoading: false } as unknown as ReturnType<
      typeof useGetCategoriesQuery
    >)
  })

  it('varsayılan olarak Almanca + İsim seçiliyken Almanca gramer alanları (cinsiyet/çoğul/hâller) render edilir', () => {
    render(<WordFormModal onClose={vi.fn()} />)

    // Etiketler zorunlu alan işareti (kırmızı *) taşıdığı için tam eşleşme yerine regex kullanılır.
    expect(screen.getByText(/^Cinsiyet/)).toBeInTheDocument()
    expect(screen.getByText(/^Çoğul/)).toBeInTheDocument()
    expect(screen.getByText(/Hâller \(Nominativ/)).toBeInTheDocument()
    // Türkçe'ye özgü alanlar henüz görünmemeli
    expect(screen.queryByText(/^Ünlü Uyumu/)).not.toBeInTheDocument()
  })

  it('dil Türkçe\'ye çevrilince Türkçe gramer alanları (ünlü uyumu/iyelik) render edilir', async () => {
    render(<WordFormModal onClose={vi.fn()} />)

    await userEvent.selectOptions(screen.getByRole('combobox', { name: 'Dil' }), 'tr')

    expect(screen.getByText(/^Ünlü Uyumu/)).toBeInTheDocument()
    expect(screen.getByText(/^İyelik Ekleri/)).toBeInTheDocument()
    // Almanca'ya özgü alan artık görünmemeli
    expect(screen.queryByText(/^Cinsiyet/)).not.toBeInTheDocument()
  })

  it('Tür "Diğer" seçilince gramer bölümü hiç render edilmez', async () => {
    render(<WordFormModal onClose={vi.fn()} />)

    await userEvent.selectOptions(screen.getByLabelText(/Tür/), 'Other')

    expect(screen.queryByText('Gramer Bilgisi')).not.toBeInTheDocument()
    expect(screen.queryByText(/^Cinsiyet/)).not.toBeInTheDocument()
  })

  it('mutlu yol: zorunlu alanlar doldurulup gönderilince createWord çağrılır ve kapanır', async () => {
    const onClose = vi.fn()
    render(<WordFormModal onClose={onClose} />)

    // partOfSpeech'i grameri basit tutmak için "Diğer"e çeviriyoruz (Noun/Verb'in 18-30
    // hücrelik çekim tablosunu doldurmak bu testin amacı değil).
    await userEvent.selectOptions(screen.getByLabelText(/Tür/), 'Other')
    await userEvent.type(screen.getByLabelText(/Kelime/), 'Tisch')
    await userEvent.click(screen.getByRole('button', { name: 'Kaydet' }))

    expect(createTrigger).toHaveBeenCalledWith(
      expect.objectContaining({
        body: expect.objectContaining({
          partOfSpeech: 'Other',
          translations: [expect.objectContaining({ languageCode: 'de', text: 'Tisch' })],
        }),
      }),
    )
    expect(onClose).toHaveBeenCalled()
  })

  it('Kelime alanı boş bırakılıp gönderilince hata gösterilir, createWord ÇAĞRILMAZ', async () => {
    const onClose = vi.fn()
    render(<WordFormModal onClose={onClose} />)

    await userEvent.selectOptions(screen.getByLabelText(/Tür/), 'Other')
    await userEvent.click(screen.getByRole('button', { name: 'Kaydet' }))

    expect(await screen.findByText('Kelime alanı zorunludur')).toBeInTheDocument()
    expect(createTrigger).not.toHaveBeenCalled()
    expect(onClose).not.toHaveBeenCalled()
  })
})
