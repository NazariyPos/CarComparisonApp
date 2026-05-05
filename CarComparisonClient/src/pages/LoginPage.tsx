import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { SiteHeader } from '../components/SiteHeader'
import { useAuth } from '../context/AuthContext'

interface AuthLocationState {
  from?: string
}

export function LoginPage() {
  const { isAuthenticated, isAuthReady, login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [loginOrEmail, setLoginOrEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [isSubmitting, setIsSubmitting] = useState(false)

  const redirectPath = useMemo(() => {
    const state = location.state as AuthLocationState | null
    return state?.from ?? '/'
  }, [location.state])

  useEffect(() => {
    if (isAuthReady && isAuthenticated) {
      navigate(redirectPath, { replace: true })
    }
  }, [isAuthReady, isAuthenticated, navigate, redirectPath])

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()

    const normalizedLogin = loginOrEmail.trim()

    if (!normalizedLogin || !password) {
      setError('Заповни логін або email і пароль.')
      return
    }

    setIsSubmitting(true)
    setError(null)

    try {
      await login({
        loginOrEmail: normalizedLogin,
        password,
      })

      navigate(redirectPath, { replace: true })
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Помилка авторизації.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="catalog-page auth-page">
      <div className="top-band" />
      <SiteHeader />

      <main className="catalog-main auth-main">
        <section className="auth-card" aria-labelledby="login-title">
          <p className="auth-eyebrow">Авторизація</p>
          <h1 id="login-title">Увійти в акаунт</h1>
          <p className="auth-description">
            Увійди за логіном або email, щоб керувати профілем і отримати доступ до
            персональних дій.
          </p>

          <form className="auth-form" onSubmit={handleSubmit}>
            <label className="auth-field">
              <span>Логін або email</span>
              <input
                type="text"
                autoComplete="username"
                value={loginOrEmail}
                onChange={(event) => setLoginOrEmail(event.target.value)}
                placeholder="ivan_ivanov або ivan@mail.com"
                required
              />
            </label>

            <label className="auth-field">
              <span>Пароль</span>
              <input
                type="password"
                autoComplete="current-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                placeholder="Введи пароль"
                required
              />
            </label>

            {error ? (
              <p className="auth-error" role="alert">
                {error}
              </p>
            ) : null}

            <button type="submit" className="primary-cta auth-submit" disabled={isSubmitting}>
              {isSubmitting ? 'Вхід...' : 'Увійти'}
            </button>
          </form>

          <p className="auth-switch">
            Ще не маєш акаунту? <Link to="/register">Створити акаунт</Link>
          </p>
        </section>
      </main>
    </div>
  )
}
