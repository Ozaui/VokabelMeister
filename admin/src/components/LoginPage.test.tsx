import { describe, it, expect, vi, beforeEach } from 'vitest'
import { render, screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { LoginPage } from './LoginPage'
import { useLoginMutation } from '../store/api/authApi'

vi.mock('../store/api/authApi', () => ({
  useLoginMutation: vi.fn(),
}))

const mockNavigate = vi.fn()

vi.mock('react-router-dom', async () => {
  const actual = await vi.importActual<typeof import('react-router-dom')>('react-router-dom')
  return {
    ...actual,
    useNavigate: () => mockNavigate,
  }
})

const mockedUseLoginMutation = vi.mocked(useLoginMutation)

describe('LoginPage', () => {
  beforeEach(() => {
    mockNavigate.mockClear()
  })

  it('mutlu yol: doğru bilgilerle giriş yapılınca /verify-otp\'a yönlendirir', async () => {
    const trigger = vi.fn().mockReturnValue({
      unwrap: () => Promise.resolve({ code: 'OTP_SENT', message: 'OTP gönderildi' }),
    })
    mockedUseLoginMutation.mockReturnValue([trigger, { isLoading: false }] as unknown as ReturnType<
      typeof useLoginMutation
    >)

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>,
    )

    await userEvent.type(screen.getByLabelText(/e-posta/i), 'admin@wordlearner.test')
    await userEvent.type(screen.getByLabelText(/şifre/i), 'Sifre123!')
    await userEvent.click(screen.getByRole('button', { name: /giriş yap/i }))

    await waitFor(() => {
      expect(trigger).toHaveBeenCalledWith({ email: 'admin@wordlearner.test', password: 'Sifre123!' })
      expect(mockNavigate).toHaveBeenCalledWith('/verify-otp', {
        state: { email: 'admin@wordlearner.test', from: undefined },
      })
    })
  })

  it('hatalı şifre: backend hatası formda gösterilir, yönlendirme YAPILMAZ', async () => {
    const trigger = vi.fn().mockReturnValue({
      unwrap: () =>
        Promise.reject({ data: { error: { code: 'INVALID_CREDENTIALS', message: 'E-posta veya şifre hatalı' } } }),
    })
    mockedUseLoginMutation.mockReturnValue([trigger, { isLoading: false }] as unknown as ReturnType<
      typeof useLoginMutation
    >)

    render(
      <MemoryRouter>
        <LoginPage />
      </MemoryRouter>,
    )

    await userEvent.type(screen.getByLabelText(/e-posta/i), 'admin@wordlearner.test')
    await userEvent.type(screen.getByLabelText(/şifre/i), 'yanlis-sifre')
    await userEvent.click(screen.getByRole('button', { name: /giriş yap/i }))

    expect(await screen.findByRole('alert')).toHaveTextContent('E-posta veya şifre hatalı')
    expect(mockNavigate).not.toHaveBeenCalled()
  })
})
