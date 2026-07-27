import { describe, it, expect, beforeEach } from 'vitest'
import authReducer, { setCredentials, logout } from './authSlice'
import type { AdminUser } from '../../types/auth.types'

const mockUser: AdminUser = {
  id: 1,
  currentLevel: 'A1',
  themePreference: 'System',
  languagePreference: 'tr',
}

describe('authSlice', () => {
  beforeEach(() => {
    localStorage.clear()
  })

  it('ilk yüklemede (localStorage boş) authenticated değildir', () => {
    const state = authReducer(undefined, { type: '@@INIT' })
    expect(state.isAuthenticated).toBe(false)
    expect(state.accessToken).toBeNull()
    expect(state.user).toBeNull()
  })

  it('setCredentials token+user\'ı state\'e VE localStorage\'a yazar', () => {
    const state = authReducer(undefined, setCredentials({ accessToken: 'jwt-123', user: mockUser }))

    expect(state.accessToken).toBe('jwt-123')
    expect(state.user).toEqual(mockUser)
    expect(state.isAuthenticated).toBe(true)
    expect(localStorage.getItem('accessToken')).toBe('jwt-123')
    expect(JSON.parse(localStorage.getItem('authUser')!)).toEqual(mockUser)
  })

  it('logout token+user\'ı hem state\'ten hem localStorage\'dan siler', () => {
    const authenticated = authReducer(undefined, setCredentials({ accessToken: 'jwt-123', user: mockUser }))
    const state = authReducer(authenticated, logout())

    expect(state.accessToken).toBeNull()
    expect(state.user).toBeNull()
    expect(state.isAuthenticated).toBe(false)
    expect(localStorage.getItem('accessToken')).toBeNull()
    expect(localStorage.getItem('authUser')).toBeNull()
  })
})
