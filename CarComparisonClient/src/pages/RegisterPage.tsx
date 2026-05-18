import { useEffect, useMemo, useState } from 'react'
import type { FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { SiteHeader } from '../components/SiteHeader'
import { useAuth } from '../context/AuthContext'

interface AuthLocationState {
  from?: string
}

export function RegisterPage() {
  const { isAuthenticated, isAuthReady, register } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [login, setLogin] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [realName, setRealName] = useState('')
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

    const normalizedLogin = login.trim()
    const normalizedEmail = email.trim()
    const normalizedRealName = realName.trim()

    if (!normalizedLogin || !normalizedEmail || !password) {
      setError('Заповни обов\'язкові поля.')
      return
    }

    if (password !== confirmPassword) {
      setError('Паролі не співпадають.')
      return
    }

    setIsSubmitting(true)
    setError(null)

    try {
      await register({
        login: normalizedLogin,
        email: normalizedEmail,
        password,
        realName: normalizedRealName || undefined,
      })

      navigate(redirectPath, { replace: true })
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Помилка реєстрації.')
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="catalog-page auth-page">
      <div className="top-band" />
      <SiteHeader />

      <main className="catalog-main auth-main">
        <section className="auth-card" aria-labelledby="register-title">
          <p className="auth-eyebrow">Реєстрація</p>
          <h1 id="register-title">Створити акаунт</h1>
          

          <form className="auth-form" onSubmit={handleSubmit}>
            <label className="auth-field">
              <span>Логін</span>
              <input
                type="text"
                autoComplete="username"
                value={login}
                onChange={(event) => setLogin(event.target.value)}
                placeholder="Лише латиниця, цифри і _"
                required
              />
            </label>

            <label className="auth-field">
              <span>Email</span>
              <input
                type="email"
                autoComplete="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                placeholder="name@example.com"
                required
              />
            </label>

            <label className="auth-field">
              <span>Пароль</span>
              <input
                type="password"
                autoComplete="new-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                placeholder="Мінімум 8 символів"
                required
              />
            </label>

            <label className="auth-field">
              <span>Підтверди пароль</span>
              <input
                type="password"
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                placeholder="Повтори пароль"
                required
              />
            </label>

            <label className="auth-field">
              <span>Ім'я (необов'язково)</span>
              <input
                type="text"
                autoComplete="name"
                value={realName}
                onChange={(event) => setRealName(event.target.value)}
                placeholder="Як до тебе звертатися"
              />
            </label>

            {error ? (
              <p className="auth-error" role="alert">
                {error}
              </p>
            ) : null}

            <button type="submit" className="primary-cta auth-submit" disabled={isSubmitting}>
              {isSubmitting ? 'Реєстрація...' : 'Зареєструватись'}
            </button>
          </form>

          <p className="auth-switch">
            Вже маєш акаунт? <Link to="/login">Увійти</Link>
          </p>
        </section>
      </main>
    </div>
  )
}
