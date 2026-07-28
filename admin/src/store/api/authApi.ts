import { apiClient } from '../api'
import { useApiMutation } from '../../hooks/useApiMutation'
import type {
  LoginRequest,
  LoginResponse,
  VerifyOtpRequest,
  VerifyOtpResponse,
} from '../../types/auth.types'

async function login(body: LoginRequest): Promise<LoginResponse> {
  const { data } = await apiClient.post<LoginResponse>('/auth/login', body)
  return data
}

async function verifyOtp(body: VerifyOtpRequest): Promise<VerifyOtpResponse> {
  const { data } = await apiClient.post<VerifyOtpResponse>('/auth/login/verify-otp', body)
  return data
}

export function useLoginMutation() {
  return useApiMutation(login)
}

export function useVerifyOtpMutation() {
  return useApiMutation(verifyOtp)
}
