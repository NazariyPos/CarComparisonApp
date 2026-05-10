import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams, useSearchParams } from 'react-router-dom'
import { SiteFooter } from '../components/SiteFooter'
import { SiteHeader } from '../components/SiteHeader'
import {
  getGenerationVariantWithTrims,
  getGenerationWithTrims,
  getReviewsByTrim,
  getTrimFullDetails,
  type GenerationWithTrimsDto,
  type ReviewWithDetailsDto,
  type TrimFullDetailsDto,
} from '../services/carApi.ts'

interface TrimTableRow {
  id: number
  generationVariantId: number
  name: string
  transmissionType: string
  power?: number
  driveType?: string
}

interface OwnerReviewView {
  key: string
  rating: number
  content: string
  username: string
  trimName: string
  createdAt: string
}

interface PageData {
  generation: GenerationWithTrimsDto
  trims: TrimTableRow[]
  reviews: OwnerReviewView[]
}

const integerFormatter = new Intl.NumberFormat('uk-UA', {
  maximumFractionDigits: 0,
})

const decimalFormatter = new Intl.NumberFormat('uk-UA', {
  minimumFractionDigits: 1,
  maximumFractionDigits: 1,
})

const isAbsoluteUrl = (url: string): boolean => {
  return /^https?:\/\//i.test(url) || url.startsWith('data:')
}

const resolveImageUrl = (url: string): string => {
  if (isAbsoluteUrl(url)) {
    return url
  }

  if (url.startsWith('/')) {
    return url
  }

  return `/${url}`
}

const collectPhotos = (
  generation: GenerationWithTrimsDto,
  variantId?: number | null,
): string[] => {
  const rawUrls: string[] = []

  if (generation.photoUrl) {
    rawUrls.push(generation.photoUrl)
  }

  generation.variants
    .filter((variant) => (variantId ? variant.id === variantId : true))
    .forEach((variant) => {
    if (variant.photoUrl) {
      rawUrls.push(variant.photoUrl)
    }

      variant.images.forEach((image) => {
        if (image.url) {
          rawUrls.push(image.url)
        }
      })
    })

  return Array.from(
    new Set(
      rawUrls
        .map((url) => url.trim())
        .filter((url) => url.length > 0)
        .map(resolveImageUrl),
    ),
  )
}

const formatPower = (value?: number): string => {
  if (typeof value !== 'number' || !Number.isFinite(value)) {
    return '—'
  }

  return `${integerFormatter.format(value)} к.с.`
}

const formatDate = (value: string): string => {
  const timestamp = Date.parse(value)

  if (Number.isNaN(timestamp)) {
    return ''
  }

  return new Date(timestamp).toLocaleDateString('uk-UA')
}

const normalizeReviews = (
  reviewsByTrim: ReviewWithDetailsDto[][],
  trimNameById: Map<number, string>,
): OwnerReviewView[] => {
  const seenKeys = new Set<string>()
  const result: OwnerReviewView[] = []

  reviewsByTrim.forEach((reviews, trimBatchIndex) => {
    reviews.forEach((item, reviewIndex) => {
      const reviewId = item.review.id
      const dedupeKey =
        reviewId > 0
          ? `review-${reviewId}`
          : `review-${trimBatchIndex}-${reviewIndex}`

      if (seenKeys.has(dedupeKey)) {
        return
      }

      seenKeys.add(dedupeKey)
      result.push({
        key: dedupeKey,
        rating: item.review.rating,
        content: item.review.content,
        username: item.username || 'Невідомий',
        trimName: item.trim || trimNameById.get(item.review.trimId) || 'Комплектація',
        createdAt: item.review.createdAt,
      })
    })
  })

  return result.sort((left, right) => {
    const leftTime = Date.parse(left.createdAt)
    const rightTime = Date.parse(right.createdAt)

    if (Number.isNaN(leftTime) || Number.isNaN(rightTime)) {
      return 0
    }

    return rightTime - leftTime
  })
}

export function CarDetailsPage() {
  const { generationId, generationVariantId } = useParams()
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()

  const [pageData, setPageData] = useState<PageData | null>(null)
  const [selectedPhoto, setSelectedPhoto] = useState('')
  const [selectedVariantId, setSelectedVariantId] = useState<number | null>(null)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const parsedGenerationId = Number.parseInt(generationId ?? '', 10)
    const parsedGenerationVariantId = Number.parseInt(generationVariantId ?? '', 10)
    const hasVariantRouteId = Number.isFinite(parsedGenerationVariantId) && parsedGenerationVariantId > 0
    const hasGenerationRouteId = Number.isFinite(parsedGenerationId) && parsedGenerationId > 0

    if (!hasVariantRouteId && !hasGenerationRouteId) {
      setPageData(null)
      setError('Некоректний ідентифікатор авто.')
      return
    }

    let cancelled = false

    async function loadPageData() {
      setIsLoading(true)
      setError(null)

      try {
        const generation = hasVariantRouteId
          ? await getGenerationVariantWithTrims(parsedGenerationVariantId)
          : await getGenerationWithTrims(parsedGenerationId)

        if (!generation) {
          if (!cancelled) {
            setError('Не вдалося завантажити сторінку авто.')
            setPageData(null)
          }
          return
        }

        const trimIds = generation.trims.map((trim) => trim.id)
        const trimNameById = new Map(generation.trims.map((trim) => [trim.id, trim.name]))

        const [fullTrimDetails, reviewBatches] = await Promise.all([
          Promise.all(trimIds.map((trimId) => getTrimFullDetails(trimId))),
          Promise.all(trimIds.map((trimId) => getReviewsByTrim(trimId))),
        ])

        if (cancelled) {
          return
        }

        const fullTrimById = new Map<number, TrimFullDetailsDto>()

        fullTrimDetails.forEach((trim) => {
          if (trim) {
            fullTrimById.set(trim.id, trim)
          }
        })

        const trimRows: TrimTableRow[] = generation.trims.map((trim) => {
          const full = fullTrimById.get(trim.id)

          return {
            id: trim.id,
            generationVariantId: trim.generationVariantId,
            name: trim.name,
            transmissionType: trim.transmissionType || full?.transmissionType || '—',
            power: full?.technicalDetails?.power,
            driveType: full?.technicalDetails?.driveType,
          }
        })

        const reviews = normalizeReviews(reviewBatches, trimNameById)

        setPageData({
          generation,
          trims: trimRows,
          reviews,
        })
      } catch {
        if (!cancelled) {
          setError('Не вдалося завантажити сторінку авто.')
          setPageData(null)
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false)
        }
      }
    }

    void loadPageData()

    return () => {
      cancelled = true
    }
  }, [generationId, generationVariantId])

  // Збережение переглядиvenil покоління у localStorage
  useEffect(() => {
    if (!pageData) return

    const generation = pageData.generation
    const viewItem = {
      generationId: generation.id,
      generationVariantId: generation.generationVariantId || generation.id,
      generationName: generation.name,
      brandName: generation.brand.name,
      modelName: generation.model.name,
      bodyType: generation.model.bodyType || '',
      brandId: generation.brand.id,
      modelId: generation.model.id,
      photoUrl: generation.photoUrl || '',
      timestamp: Date.now(),
    }

    try {
      const stored = localStorage.getItem('recentViews')
      const recentViews = stored ? JSON.parse(stored) : []

      // Видалити дубліkat, якщо вже переглянуте
      const filtered = recentViews.filter(
        (item: typeof viewItem) => !(
          item.generationId === viewItem.generationId &&
          item.generationVariantId === viewItem.generationVariantId
        )
      )

      // Додати нове в початок, зберегти останні 20
      const updated = [viewItem, ...filtered].slice(0, 20)
      localStorage.setItem('recentViews', JSON.stringify(updated))
    } catch (e) {
      console.error('Failed to save recent view:', e)
    }
  }, [pageData])

  const galleryPhotos = useMemo(() => {
    if (!pageData) {
      return []
    }

    return collectPhotos(pageData.generation, selectedVariantId)
  }, [pageData, selectedVariantId])

  useEffect(() => {
    if (!pageData) {
      return
    }

    const parsedRouteVariantId = Number.parseInt(generationVariantId ?? '', 10)
    const routeVariantId =
      Number.isFinite(parsedRouteVariantId) && parsedRouteVariantId > 0 ? parsedRouteVariantId : null

    const parsedVariantId = Number.parseInt(searchParams.get('variantId') ?? '', 10)
    const queryVariantId =
      Number.isFinite(parsedVariantId) && parsedVariantId > 0 ? parsedVariantId : null

    const requestedVariantId = routeVariantId ?? queryVariantId

    if (requestedVariantId !== null) {
      const requestedVariantExists = pageData.generation.variants.some(
        (variant) => variant.id === requestedVariantId,
      )

      if (requestedVariantExists) {
        setSelectedVariantId(requestedVariantId)
        return
      }
    }

    const defaultVariant =
      pageData.generation.variants.find((variant) => variant.isDefault) ??
      pageData.generation.variants[0]

    if (!defaultVariant) {
      setSelectedVariantId(null)
      return
    }

    setSelectedVariantId((current) =>
      current && pageData.generation.variants.some((variant) => variant.id === current)
        ? current
        : defaultVariant.id,
    )
  }, [pageData, searchParams, generationVariantId])

  useEffect(() => {
    if (galleryPhotos.length === 0) {
      setSelectedPhoto('')
      return
    }

    if (!selectedPhoto || !galleryPhotos.includes(selectedPhoto)) {
      setSelectedPhoto(galleryPhotos[0])
    }
  }, [galleryPhotos, selectedPhoto])

  const ratingData = useMemo(() => {
    const reviews = pageData?.reviews ?? []
    const total = reviews.length
    const sum = reviews.reduce((acc, review) => acc + review.rating, 0)
    const average = total > 0 ? sum / total : 0

    const distribution = Array.from({ length: 10 }, (_, index) => {
      const score = 10 - index
      const count = reviews.filter((review) => review.rating === score).length
      const percent = total > 0 ? (count / total) * 100 : 0

      return {
        score,
        count,
        percent,
      }
    })

    return {
      total,
      average,
      distribution,
    }
  }, [pageData])

  const activeVariant = useMemo(() => {
    if (!pageData || selectedVariantId === null) {
      return null
    }

    return pageData.generation.variants.find((variant) => variant.id === selectedVariantId) ?? null
  }, [pageData, selectedVariantId])

  const visibleTrims = useMemo(() => {
    if (!pageData) {
      return []
    }

    if (!selectedVariantId) {
      return pageData.trims
    }

    return pageData.trims.filter(
      (trim) => trim.generationVariantId === selectedVariantId,
    )
  }, [pageData, selectedVariantId])

  const activeStars = Math.round(ratingData.average)

  return (
    <div className="catalog-page">
      <div className="top-band" />
      <SiteHeader />

      <main className="catalog-main car-details-main">
        {isLoading && <p className="muted-note">Завантаження сторінки авто...</p>}
        {error && <p className="error-text">{error}</p>}

        {pageData && (
          <>
            <section className="car-details-hero-panel">
              <div className="car-gallery-block">
                <div className="car-main-photo-wrap">
                  {selectedPhoto ? (
                    <img
                      src={selectedPhoto}
                      alt={`${pageData.generation.brand.name} ${pageData.generation.model.name}`}
                      className="car-main-photo"
                    />
                  ) : (
                    <div className="placeholder-photo car-main-photo-placeholder">
                      Фото відсутні
                    </div>
                  )}
                </div>

                {galleryPhotos.length > 1 && (
                  <div className="car-thumbs-row" role="tablist" aria-label="Галерея фото">
                    {galleryPhotos.map((photoUrl, index) => (
                      <button
                        key={`${photoUrl}-${index}`}
                        type="button"
                        className={
                          photoUrl === selectedPhoto
                            ? 'car-thumb-button car-thumb-button-active'
                            : 'car-thumb-button'
                        }
                        onClick={() => setSelectedPhoto(photoUrl)}
                      >
                        <img
                          src={photoUrl}
                          alt={`Фото ${index + 1}`}
                          className="car-thumb-image"
                        />
                      </button>
                    ))}
                  </div>
                )}
              </div>

              <aside className="car-summary-block">
                <h2>
                  {pageData.generation.brand.name} {pageData.generation.model.name}
                </h2>
                <p className="car-summary-generation">{pageData.generation.displayName}</p>
                {activeVariant && (
                  <p className="car-summary-generation">
                    {activeVariant.bodyStyleName} • {activeVariant.variantType}
                  </p>
                )}
                <p className="car-summary-years">
                  {pageData.generation.yearFrom}-{pageData.generation.yearTo}
                </p>

                <div className="car-summary-actions">
                  <Link to="/brands" className="car-secondary-link">
                    Назад до пошуку
                  </Link>
                </div>
              </aside>
            </section>

            <section className="model-block">
              <h3>Комплектації</h3>

              <div className="model-trims-table-wrap">
                <table className="model-trims-table">
                  <thead>
                    <tr>
                      <th scope="col">Назва</th>
                      <th scope="col">Коробка передач</th>
                      <th scope="col">Потужність</th>
                      <th scope="col">Тип приводу</th>
                    </tr>
                  </thead>
                  <tbody>
                    {visibleTrims.map((trim) => (
                      <tr
                        key={trim.id}
                        className="clickable-row"
                        onClick={() => navigate(`/cars/variants/${trim.generationVariantId}/trims/${trim.id}`)}
                      >
                        <td>{trim.name}</td>
                        <td>{trim.transmissionType || '—'}</td>
                        <td>{formatPower(trim.power)}</td>
                        <td>{trim.driveType || '—'}</td>
                      </tr>
                    ))}
                    {visibleTrims.length === 0 && (
                      <tr>
                        <td colSpan={4} className="muted-note">
                          Немає доступних комплектацій для цього варіанта покоління.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </section>

            <section className="model-block model-rating-block">
              <h3>Середня оцінка від власників</h3>

              <div className="owner-rating-summary">
                <div className="owner-rating-stars" aria-hidden="true">
                  {Array.from({ length: 10 }, (_, index) => (
                    <span
                      key={`star-${index}`}
                      className={
                        index < activeStars
                          ? 'owner-rating-star owner-rating-star-active'
                          : 'owner-rating-star'
                      }
                    >
                      ★
                    </span>
                  ))}
                </div>
                <strong className="owner-rating-value">
                  {ratingData.total > 0
                    ? `${decimalFormatter.format(ratingData.average)}/10`
                    : 'Немає оцінок'}
                </strong>
              </div>

              <h4>Рейтинг відгуків</h4>
              <ul className="owner-rating-list">
                {ratingData.distribution.map((item) => (
                  <li key={`rating-${item.score}`} className="owner-rating-row">
                    <span className="owner-rating-score">{item.score}</span>
                    <div className="owner-rating-track" aria-hidden="true">
                      <div
                        className="owner-rating-fill"
                        style={{ width: `${item.percent}%` }}
                      />
                    </div>
                    <span className="owner-rating-percent">
                      {decimalFormatter.format(item.percent)}%
                    </span>
                  </li>
                ))}
              </ul>
            </section>

            <section className="model-block">
              <h3>Відгуки власників</h3>

              {pageData.reviews.length === 0 && (
                <p className="muted-note">Поки немає відгуків для цієї моделі.</p>
              )}

              {pageData.reviews.length > 0 && (
                <ul className="owner-reviews-list">
                  {pageData.reviews.map((review) => (
                    <li key={review.key} className="owner-review-card">
                      <div className="owner-review-head">
                        <strong>{review.username}</strong>
                        <span>{review.trimName}</span>
                        <span>{review.rating}/10</span>
                      </div>
                      <p>{review.content || 'Без тексту відгуку'}</p>
                      {review.createdAt && (
                        <small>{formatDate(review.createdAt)}</small>
                      )}
                    </li>
                  ))}
                </ul>
              )}
            </section>
          </>
        )}
      </main>

      <SiteFooter />
    </div>
  )
}
