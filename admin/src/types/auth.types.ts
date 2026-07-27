export interface LoginRequest {
  email: string
  password: string
}

export interface LoginResponse {
  code: string
  message: string
}

export interface VerifyOtpRequest {
  email: string
  otpCode: string
}

export interface AdminUser {
  id: number
  currentLevel: string
  themePreference: string
  languagePreference: string
}

export interface VerifyOtpResponse {
  accessToken: string
  refreshToken: string
  expiresIn: number
  user: AdminUser
  accountWasRecovered: boolean
}
