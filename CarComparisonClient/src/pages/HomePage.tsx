import { NavLink } from 'react-router-dom'
import { SiteHeader } from '../components/SiteHeader'

export function HomePage() {

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

      <SiteHeader />

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
