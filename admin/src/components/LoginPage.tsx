import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { useLocation, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { useLoginMutation } from '../store/api/authApi'
import { getApiErrorMessage } from '../lib/apiError'
import type { LoginRequest } from '../types/auth.types'

interface LoginLocationState {
  // ProtectedRoute (B-01) state'e TÜM location nesnesini koyar — search/hash burada da
  // taşınmazsa, ör. /words?page=3'ten atılan bir admin giriş sonrası filtreleri KAYBEDERDİ.
  from?: { pathname: string; search?: string; hash?: string }
}

export function LoginPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const location = useLocation()
  const locationState = location.state as LoginLocationState | null
  const [login, { isLoading }] = useLoginMutation()
  const [formError, setFormError] = useState<string | null>(null)
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginRequest>()

  const onSubmit = async (values: LoginRequest) => {
    setFormError(null)
    try {
      await login(values).unwrap()
      // Token burada henüz yok — backend şifreyi doğrulayıp OTP gönderdi (adım 1).
      // email + "nereden geldiği" bilgisi OtpVerifyPage'e route state ile taşınır.
      navigate('/verify-otp', { state: { email: values.email, from: locationState?.from } })
    } catch (err) {
      setFormError(getApiErrorMessage(err) ?? t('auth.genericError'))
    }
  }

  return (
    <div className="flex h-screen items-center justify-center bg-background font-body text-text">
      <form
        onSubmit={handleSubmit(onSubmit)}
        noValidate
        className="w-full max-w-sm rounded-card border border-border bg-surface p-8 shadow-sm"
      >
        <h1 className="mb-6 font-heading text-xl font-bold text-text">{t('auth.login.title')}</h1>

        {formError && (
          <p role="alert" className="mb-4 rounded-control bg-destructive/10 px-3 py-2 text-sm text-destructive">
            {formError}
          </p>
        )}

        <label htmlFor="email" className="mb-1 block text-sm font-medium text-text">
          {t('auth.login.email')}
        </label>
        <input
          id="email"
          type="email"
          className="mb-1 w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
          {...register('email', {
            required: t('auth.login.emailRequired'),
            pattern: { value: /^\S+@\S+\.\S+$/, message: t('auth.login.emailInvalid') },
          })}
        />
        {errors.email && <p className="mb-3 text-xs text-destructive">{errors.email.message}</p>}

        <label htmlFor="password" className="mb-1 block text-sm font-medium text-text">
          {t('auth.login.password')}
        </label>
        <input
          id="password"
          type="password"
          className="mb-1 w-full rounded-control border border-border bg-background px-3 py-2 text-sm text-text"
          {...register('password', { required: t('auth.login.passwordRequired') })}
        />
        {errors.password && <p className="mb-3 text-xs text-destructive">{errors.password.message}</p>}

        <button
          type="submit"
          disabled={isLoading}
          className="mt-3 w-full rounded-control bg-primary py-2 text-sm font-semibold text-white disabled:opacity-60"
        >
          {isLoading ? t('auth.login.submitting') : t('auth.login.submit')}
        </button>
      </form>
    </div>
  )
}
