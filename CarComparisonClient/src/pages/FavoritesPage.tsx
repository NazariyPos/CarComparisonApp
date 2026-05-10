import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { SiteFooter } from '../components/SiteFooter'
import { SiteHeader } from '../components/SiteHeader'
import { getFavorites, removeFavorite, type GenerationCardDto } from '../services/carApi'

export function FavoritesPage() {
  const [favorites, setFavorites] = useState<GenerationCardDto[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function load() {
    setIsLoading(true)
    setError(null)

    try {
      const data = await getFavorites()
      setFavorites(data)
    } catch {
      setError('Не вдалося завантажити обране')
      setFavorites([])
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void load()
  }, [])

  const handleRemove = async (id: number | undefined) => {
    if (!id) return

    const ok = await removeFavorite(id)
    if (ok) {
      setFavorites((s) => s.filter((f) => (f.generationVariantId ?? f.generationId) !== id))
    }
  }

  return (
    <div className="catalog-page">
      <div className="top-band">
        <div className="top-band-title">
          <h2>Обране</h2>
        </div>
      </div>
      <SiteHeader />

      <main className="catalog-main">
        <section className="results-panel favorites-panel">

          {isLoading && <p className="muted-note">Завантаження...</p>}
          {error && <p className="error-text">{error}</p>}

          {!isLoading && favorites.length === 0 && (
            <p className="muted-note no-favorites">Немає збережених авто в обраному.</p>
          )}

          {favorites.length > 0 && (
            <ul className="result-grid">
              {favorites.map((item) => {
                const photoUrl = item.photoUrl
                const id = item.trimId ?? item.generationVariantId ?? item.generationId

                return (
                  <li
                    key={`${id}-${item.modelId}`}
                    className="result-card"
                  >
                    {photoUrl ? (
                      <img
                        src={photoUrl}
                        alt={`${item.brandName} ${item.modelName}`}
                        className="result-card-photo"
                      />
                    ) : (
                      <div className="placeholder-photo">Зображення скоро</div>
                    )}

                    <strong>
                      {item.brandName} {item.modelName}
                    </strong>
                    <span>
                      {item.displayGenerationName} ({item.yearFrom}-{item.yearTo})
                    </span>
                    <small>
                      {item.bodyType} • {item.trimCount} комплектацій
                    </small>

                    <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
                      <Link
                        to={
                          item.generationVariantId
                            ? `/cars/variants/${item.generationVariantId}`
                            : `/cars/${item.generationId}`
                        }
                        className="result-card-link"
                      >
                        Переглянути
                      </Link>

                      <button
                        type="button"
                        className="primary-cta"
                        onClick={() => void handleRemove(id)}
                      >
                        Прибрати з обраного
                      </button>
                    </div>
                  </li>
                )
              })}
            </ul>
          )}
        </section>
      </main>

      <SiteFooter />
    </div>
  )
}

export default FavoritesPage
