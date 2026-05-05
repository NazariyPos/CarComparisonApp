import { Link, NavLink, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

const navClassName = ({ isActive }: { isActive: boolean }) =>
  isActive ? 'site-nav-link site-nav-link-active' : 'site-nav-link'

export function SiteHeader() {
  const { currentUser, isAuthenticated, logout } = useAuth()
  const location = useLocation()
  const returnPath = `${location.pathname}${location.search}${location.hash}`
  const canAccessAdminPhotos = isAuthenticated && currentUser?.isAdmin

  return (
    <header className="site-header">
      <nav className="site-nav" aria-label="Main navigation">
        <NavLink to="/" className="site-logo">
          CarDD
        </NavLink>

        <div className="site-nav-links">
          <NavLink to="/brands" className={navClassName}>
            Каталог авто
          </NavLink>
          <NavLink to="/comparison" className={navClassName}>
            Порівняння
          </NavLink>
          {canAccessAdminPhotos ? (
            <NavLink to="/admin/photos" className={navClassName}>
              Адмін фото
            </NavLink>
          ) : null}
          {canAccessAdminPhotos ? (
            <NavLink to="/admin/catalog" className={navClassName}>
              Адмін каталог
            </NavLink>
          ) : null}
        </div>

        {isAuthenticated && currentUser ? (
          <div className="site-account" aria-label="Поточний користувач">
            <span className="site-account-trigger">{currentUser.login}</span>
            <div className="site-account-menu" role="menu" aria-label="Меню акаунту">
              <button type="button" className="site-account-logout" onClick={logout} role="menuitem">
                Вийти
              </button>
            </div>
          </div>
        ) : (
          <Link
            to="/login"
            state={{ from: returnPath }}
            className="site-login-button"
          >
            Увійти
          </Link>
        )}
      </nav>
    </header>
  )
}
