import { Link, NavLink, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

export function HomePage() {
  const { currentUser, isAuthenticated, logout } = useAuth()
  const location = useLocation()
  const returnPath = `${location.pathname}${location.search}${location.hash}`
  const canAccessAdminPhotos = isAuthenticated && currentUser?.isAdmin

  return (
    <div className="home-page">
      <div className="home-top-gradient" />

      <div className="home-grain" aria-hidden="true">
        <svg width="100%" height="100%">
          <filter id="noise">
            <feTurbulence type="fractalNoise" baseFrequency="0.8" numOctaves="4" />
          </filter>
          <rect width="100%" height="100%" filter="url(#noise)" />
        </svg>
      </div>

      <header className="home-header">
        <nav className="home-nav" aria-label="Main navigation">
          <div className="home-logo">CarDD</div>

          <div className="home-nav-links">
            <NavLink to="/brands" className="home-nav-link">
              Каталог авто
            </NavLink>
            <NavLink to="/comparison" className="home-nav-link">
              Порівняння
            </NavLink>
            {canAccessAdminPhotos ? (
              <NavLink to="/admin/photos" className="home-nav-link">
                Адмін фото
              </NavLink>
            ) : null}

            {isAuthenticated && currentUser ? (
              <div className="home-account" aria-label="Поточний користувач">
                <span className="home-account-trigger">{currentUser.login}</span>
                <div className="home-account-menu" role="menu" aria-label="Меню акаунту">
                  <button
                    type="button"
                    className="home-account-logout"
                    onClick={logout}
                    role="menuitem"
                  >
                    Вийти
                  </button>
                </div>
              </div>
            ) : (
              <Link
                to="/login"
                state={{ from: returnPath }}
                className="home-login-button"
              >
                Увійти
              </Link>
            )}
          </div>
        </nav>
      </header>

      <section className="home-hero">
        <div className="home-hero-text">
          <h1>Обирай та порівнюй</h1>
          <p>
            Технічні характеристики, порівняння та рейтинг авто - все, щоб обрати
            твій ідеальний автомобіль
          </p>
        </div>
      </section>

      <section className="home-how-it-works">
        <div className="steps-card">
          <h2>Як це працює?</h2>

          <div className="step-block step-delay-1">
            <h3>Знайди авто</h3>
            <p>Наш розумний пошук допоможе знайти саме те, що потрібно.</p>
          </div>

          <div className="step-block step-delay-2">
            <h3>Досліди характеристики</h3>
            <p>Всі характеристики, фото та відгуки в одному місці.</p>
          </div>

          <div className="step-block step-delay-3">
            <h3>Порівняй варіанти</h3>
            <p>Дізнайся, яке авто підходить саме тобі.</p>
          </div>

          <h2 className="cta-title">Почни прямо зараз</h2>
          <NavLink to="/brands" className="home-cta-button">
            Обрати авто
          </NavLink>
        </div>
      </section>

      <div className="home-bottom-gradient" />
    </div>
  )
}
