import { useEffect, useState } from 'react'
import { useFormik } from 'formik'
import * as Yup from 'yup'
import { useLocation, useNavigate } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import type { TFunction } from 'i18next'
import { useDispatch } from 'react-redux'
import { useVerifyOtpMutation } from '../store/api/authApi'
import { setCredentials } from '../store/slices/authSlice'
import { getApiErrorMessage } from '../lib/apiError'

interface OtpFormValues {
  otpCode: string
}

interface OtpLocationState {
  email?: string
  from?: { pathname: string; search?: string; hash?: string }
}

function buildOtpSchema(t: TFunction) {
  return Yup.object({
    otpCode: Yup.string()
      .required(t('auth.otp.codeRequired'))
      .matches(/^\d{6}$/, t('auth.otp.codeInvalid')),
  })
}

export function OtpVerifyPage() {
  const { t } = useTranslation()
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const location = useLocation()
  const state = location.state as OtpLocationState | null
  const [verifyOtp, { isLoading }] = useVerifyOtpMutation()
  const [formError, setFormError] = useState<string | null>(null)

  useEffect(() => {
    // E-posta olmadan bu sayfaya doğrudan gelinmişse (ör. sayfa yenilendi) hangi hesabın
    // doğrulanacağı bilinmiyor demektir — LoginPage'e geri gönder.
    if (!state?.email) {
      navigate('/login', { replace: true })
    }
  }, [state, navigate])

  const formik = useFormik<OtpFormValues>({
    initialValues: { otpCode: '' },
    validationSchema: buildOtpSchema(t),
    onSubmit: async (values) => {
      setFormError(null)
      try {
        const result = await verifyOtp({ email: state!.email!, otpCode: values.otpCode })
        dispatch(setCredentials({ accessToken: result.accessToken, user: result.user }))
        // pathname TEK BAŞINA yeterli değil — ör. /words?page=3'ten atılmış bir admin,
        // search/hash de geri taşınmazsa filtrelerini/sayfa numarasını KAYBEDER.
        const destination = state!.from
          ? `${state!.from.pathname}${state!.from.search ?? ''}${state!.from.hash ?? ''}`
          : '/'
        navigate(destination, { replace: true })
      } catch (err) {
        setFormError(getApiErrorMessage(err) ?? t('auth.genericError'))
      }
    },
  })

  if (!state?.email) return null

  return (
    <div className="flex h-screen items-center justify-center bg-background font-body text-text">
      <form
        onSubmit={formik.handleSubmit}
        noValidate
        className="w-full max-w-sm rounded-card border border-border bg-surface p-8 shadow-sm"
      >
        <h1 className="mb-2 font-heading text-xl font-bold text-text">{t('auth.otp.title')}</h1>
        <p className="mb-6 text-sm text-muted">{t('auth.otp.subtitle', { email: state.email })}</p>

        {formError && (
          <p role="alert" className="mb-4 rounded-control bg-destructive/10 px-3 py-2 text-sm text-destructive">
            {formError}
          </p>
        )}

        <label htmlFor="otpCode" className="mb-1 block text-sm font-medium text-text">
          {t('auth.otp.code')}
        </label>
        <input
          id="otpCode"
          name="otpCode"
          type="text"
          inputMode="numeric"
          maxLength={6}
          autoComplete="one-time-code"
          value={formik.values.otpCode}
          onChange={formik.handleChange}
          onBlur={formik.handleBlur}
          className="mb-1 w-full rounded-control border border-border bg-background px-3 py-2 text-center text-lg tracking-[0.5em] text-text"
        />
        {formik.touched.otpCode && formik.errors.otpCode && (
          <p className="mb-3 text-xs text-destructive">{formik.errors.otpCode}</p>
        )}

        <button
          type="submit"
          disabled={isLoading}
          className="mt-3 w-full rounded-control bg-primary py-2 text-sm font-semibold text-white disabled:opacity-60"
        >
          {isLoading ? t('auth.otp.submitting') : t('auth.otp.submit')}
        </button>
      </form>
    </div>
  )
}
