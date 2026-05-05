import axios from 'axios'
import { apiClient } from './apiClient'

export interface AuthUser {
  id: number
  login: string
  username: string
  email: string
  isAdmin: boolean
  realName?: string
  about?: string
  avatarUrl?: string
}

export interface LoginRequest {
  loginOrEmail: string
  password: string
}

export interface RegisterRequest {
  login: string
  email: string
  password: string
  realName?: string
}

export interface AuthResult {
  token: string
  user: AuthUser
}

type RawDto = Record<string, unknown>

function readNumber(raw: RawDto, camelKey: string, pascalKey: string): number {
  const value = raw[camelKey] ?? raw[pascalKey]
  return typeof value === 'number' ? value : 0
}

function readString(raw: RawDto, camelKey: string, pascalKey: string): string {
  const value = raw[camelKey] ?? raw[pascalKey]
  return typeof value === 'string' ? value : ''
}

function readOptionalString(
  raw: RawDto,
  camelKey: string,
  pascalKey: string,
): string | undefined {
  const value = raw[camelKey] ?? raw[pascalKey]
  return typeof value === 'string' && value.trim().length > 0 ? value : undefined
}

function mapAuthUser(raw: RawDto): AuthUser {
  return {
    id: readNumber(raw, 'id', 'Id'),
    login: readString(raw, 'login', 'Login'),
    username: readString(raw, 'username', 'Username'),
    email: readString(raw, 'email', 'Email'),
    isAdmin: Boolean(raw.isAdmin ?? raw.IsAdmin),
    realName: readOptionalString(raw, 'realName', 'RealName'),
    about: readOptionalString(raw, 'about', 'About'),
    avatarUrl: readOptionalString(raw, 'avatarUrl', 'AvatarUrl'),
  }
}

function readErrorMessage(error: unknown, fallback: string): string {
  if (axios.isAxiosError(error)) {
    const responseData = error.response?.data

    if (responseData && typeof responseData === 'object') {
      const rawData = responseData as RawDto
      const message = rawData.message ?? rawData.Message

      if (typeof message === 'string' && message.trim().length > 0) {
        return message
      }
    }

    if (typeof error.message === 'string' && error.message.trim().length > 0) {
      return error.message
    }
  }

  if (error instanceof Error && error.message.trim().length > 0) {
    return error.message
  }

  return fallback
}

function mapAuthResult(data: unknown): AuthResult {
  if (!data || typeof data !== 'object') {
    throw new Error('Некоректна відповідь сервера авторизації.')
  }

  const raw = data as RawDto
  const token = raw.token ?? raw.Token
  const user = raw.user ?? raw.User

  if (typeof token !== 'string' || token.trim().length === 0) {
    throw new Error('Сервер не повернув токен доступу.')
  }

  if (!user || typeof user !== 'object') {
    throw new Error('Сервер не повернув дані користувача.')
  }

  return {
    token,
    user: mapAuthUser(user as RawDto),
  }
}

export async function login(request: LoginRequest): Promise<AuthResult> {
  try {
    const { data } = await apiClient.post<unknown>('/Auth/login', request)
    return mapAuthResult(data)
  } catch (error) {
    throw new Error(readErrorMessage(error, 'Не вдалося виконати авторизацію.'))
  }
}

export async function register(request: RegisterRequest): Promise<AuthResult> {
  try {
    const { data } = await apiClient.post<unknown>('/Auth/register', request)
    return mapAuthResult(data)
  } catch (error) {
    throw new Error(readErrorMessage(error, 'Не вдалося створити акаунт.'))
  }
}

export async function getCurrentUser(): Promise<AuthUser> {
  try {
    const { data } = await apiClient.get<unknown>('/Auth/me')

    if (!data || typeof data !== 'object') {
      throw new Error('Сервер повернув некоректний профіль користувача.')
    }

    return mapAuthUser(data as RawDto)
  } catch (error) {
    throw new Error(readErrorMessage(error, 'Не вдалося отримати профіль користувача.'))
  }
}
