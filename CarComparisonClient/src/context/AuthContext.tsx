import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import type { PropsWithChildren } from 'react'
import {
  getCurrentUser,
  login as loginRequest,
  register as registerRequest,
  type AuthResult,
  type AuthUser,
  type LoginRequest,
  type RegisterRequest,
} from '../services/authApi'

interface AuthContextValue {
  accessToken: string | null
  currentUser: AuthUser | null
  isAuthenticated: boolean
  isAuthReady: boolean
  setAccessToken: (token: string | null) => void
  login: (request: LoginRequest) => Promise<void>
  register: (request: RegisterRequest) => Promise<void>
  logout: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

type RawDto = Record<string, unknown>

function base64UrlDecode(value: string): string {
  const normalized = value.replace(/-/g, '+').replace(/_/g, '/')
  const padded = normalized.padEnd(Math.ceil(normalized.length / 4) * 4, '=')

  return atob(padded)
}

function decodeJwtPayload(token: string): RawDto | null {
  const parts = token.split('.')

  if (parts.length < 2) {
    return null
  }

  try {
    const payload = base64UrlDecode(parts[1])
    const parsed = JSON.parse(payload) as unknown

    if (!parsed || typeof parsed !== 'object') {
      return null
    }

    return parsed as RawDto
  } catch {
    return null
  }
}

function readClaim(raw: RawDto, keys: string[]): string | undefined {
  for (const key of keys) {
    const value = raw[key]

    if (typeof value === 'string' && value.trim().length > 0) {
      return value
    }
  }

  return undefined
}

function readClaimNumber(raw: RawDto, keys: string[]): number | undefined {
  for (const key of keys) {
    const value = raw[key]

    if (typeof value === 'number' && Number.isFinite(value)) {
      return value
    }

    if (typeof value === 'string') {
      const parsed = Number.parseInt(value, 10)

      if (Number.isFinite(parsed)) {
        return parsed
      }
    }
  }

  return undefined
}

function decodeUserFromToken(token: string): AuthUser | null {
  const payload = decodeJwtPayload(token)

  if (!payload) {
    return null
  }

  const login = readClaim(payload, [
    'login',
    'Login',
    'unique_name',
    'unique_name',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name',
  ])
  const email = readClaim(payload, [
    'email',
    'Email',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress',
  ])

  if (!login || !email) {
    return null
  }

  const username = readClaim(payload, ['username', 'Username']) ?? login
  const id = readClaimNumber(payload, [
    'id',
    'Id',
    'nameid',
    'sub',
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier',
  ]) ?? 0

  return {
    id,
    login,
    username,
    email,
    isAdmin: false,
  }
}

function readStoredUser(): AuthUser | null {
  const rawUser = localStorage.getItem('currentUser')

  if (!rawUser) {
    return null
  }

  try {
    const parsed = JSON.parse(rawUser) as Partial<AuthUser>

    if (typeof parsed !== 'object' || parsed === null) {
      return null
    }

    if (typeof parsed.login !== 'string' || typeof parsed.email !== 'string') {
      return null
    }

    return {
      id: typeof parsed.id === 'number' ? parsed.id : 0,
      login: parsed.login,
      username: typeof parsed.username === 'string' ? parsed.username : parsed.login,
      email: parsed.email,
      isAdmin: Boolean(parsed.isAdmin),
      realName: typeof parsed.realName === 'string' ? parsed.realName : undefined,
      about: typeof parsed.about === 'string' ? parsed.about : undefined,
      avatarUrl: typeof parsed.avatarUrl === 'string' ? parsed.avatarUrl : undefined,
    }
  } catch {
    return null
  }
}

export function AuthProvider({ children }: PropsWithChildren) {
  const storedToken = localStorage.getItem('accessToken')
  const [accessTokenState, setAccessTokenState] = useState<string | null>(() => {
    return storedToken
  })
  const [currentUserState, setCurrentUserState] = useState<AuthUser | null>(() => {
    return readStoredUser() ?? (storedToken ? decodeUserFromToken(storedToken) : null)
  })
  const [isAuthReady, setIsAuthReady] = useState(false)

  const clearAuthState = useCallback(() => {
    setAccessTokenState(null)
    setCurrentUserState(null)
    localStorage.removeItem('accessToken')
    localStorage.removeItem('currentUser')
  }, [])

  const applyAuthResult = useCallback((result: AuthResult) => {
    setAccessTokenState(result.token)
    setCurrentUserState(result.user)
    localStorage.setItem('accessToken', result.token)
    localStorage.setItem('currentUser', JSON.stringify(result.user))
  }, [])

  const setAccessToken = useCallback((token: string | null) => {
    setAccessTokenState(token)

    if (token) {
      setCurrentUserState(null)
      localStorage.setItem('accessToken', token)
      localStorage.removeItem('currentUser')
      return
    }

    clearAuthState()
  }, [clearAuthState])

  const login = useCallback(async (request: LoginRequest) => {
    const result = await loginRequest(request)
    applyAuthResult(result)
  }, [applyAuthResult])

  const register = useCallback(async (request: RegisterRequest) => {
    const result = await registerRequest(request)
    applyAuthResult(result)
  }, [applyAuthResult])

  const logout = useCallback(() => {
    clearAuthState()
  }, [clearAuthState])

  useEffect(() => {
    let isCancelled = false

    const syncCurrentUser = async () => {
      if (!accessTokenState) {
        if (!isCancelled) {
          setCurrentUserState(null)
          localStorage.removeItem('currentUser')
          setIsAuthReady(true)
        }
        return
      }

      if (!isCancelled) {
        setIsAuthReady(false)
      }

      const decodedUser = decodeUserFromToken(accessTokenState)

      if (decodedUser) {
        setCurrentUserState((existingUser) => existingUser ?? decodedUser)
        localStorage.setItem('currentUser', JSON.stringify(decodedUser))
      }

      try {
        // The decoded JWT already gives us enough data to keep the session alive.
        // We only use the profile endpoint as a best-effort refresh.
        const profile = await getCurrentUser()

        if (!isCancelled) {
          setCurrentUserState(profile)
          localStorage.setItem('currentUser', JSON.stringify(profile))
        }
      } catch {
        // Keep the decoded JWT session alive even if /Auth/me is temporarily unavailable.
      } finally {
        if (!isCancelled) {
          setIsAuthReady(true)
        }
      }
    }

    void syncCurrentUser()

    return () => {
      isCancelled = true
    }
  }, [accessTokenState])

  const value = useMemo<AuthContextValue>(
    () => ({
      accessToken: accessTokenState,
      currentUser: currentUserState,
      isAuthenticated: Boolean(accessTokenState && currentUserState),
      isAuthReady,
      setAccessToken,
      login,
      register,
      logout,
    }),
    [accessTokenState, currentUserState, isAuthReady, setAccessToken, login, register, logout],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider')
  }

  return context
}
