import { useEffect, useMemo, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { SiteFooter } from '../components/SiteFooter'
import { SiteHeader } from '../components/SiteHeader'
import { useAuth } from '../context/AuthContext'
import {
  getTrimFullDetails,
  getReviewsByTrim,
  deleteReview,
  type TrimFullDetailsDto,
} from '../services/carApi.ts'
import { ReviewModal } from '../components/ReviewModal'
import { ConfirmDeleteModal } from '../components/ConfirmDeleteModal'

interface OwnerReviewView {
  key: string
  id: number
  userId: number
  trimId: number
  rating: number
  content: string
  username: string
  createdAt: string
}

const integerFormatter = new Intl.NumberFormat('uk-UA', {
  maximumFractionDigits: 0,
})

const formatPower = (value?: number): string => {
  if (typeof value !== 'number' || !Number.isFinite(value)) {
    return '—'
  }

  return `${integerFormatter.format(value)} к.с.`
}

const decimalFormatter = new Intl.NumberFormat('uk-UA', {
  maximumFractionDigits: 1,
})

const formatNumber = (value?: number, suffix = ''): string => {
  if (typeof value !== 'number' || !Number.isFinite(value)) {
    return '—'
  }

  return `${integerFormatter.format(value)}${suffix}`
}

const formatDecimal = (value?: number, suffix = ''): string => {
  if (typeof value !== 'number' || !Number.isFinite(value)) {
    return '—'
  }

  return `${decimalFormatter.format(value)}${suffix}`
}

const formatText = (value?: string): string => value || '—'

const technicalDetailRows = (technicalDetails?: TrimFullDetailsDto['technicalDetails']) => [
  { label: 'Максимальна швидкість', value: formatNumber(technicalDetails?.maxSpeed, ' км/год') },
  {
    label: 'Розгін 0-100 км/год',
    value: formatDecimal(technicalDetails?.acceleration0To100, ' с'),
  },
  { label: 'Код двигуна', value: formatText(technicalDetails?.engineCode) },
  { label: 'Тип двигуна', value: formatText(technicalDetails?.engineType) },
  { label: 'Кількість циліндрів', value: formatNumber(technicalDetails?.cylindersCount) },
  { label: 'Кількість клапанів', value: formatNumber(technicalDetails?.valvesCount) },
  { label: 'Ступінь стиснення', value: formatDecimal(technicalDetails?.compressionRatio) },
  { label: 'Тип пального', value: formatText(technicalDetails?.fuelType) },
  { label: 'Потужність', value: formatPower(technicalDetails?.power) },
  { label: 'Крутний момент', value: formatNumber(technicalDetails?.torque, ' Н·м') },
  { label: 'Оберти максимальної потужності', value: formatNumber(technicalDetails?.maxPowerAtRPM, ' об/хв') },
  { label: 'Оберти максимального крутного моменту', value: formatNumber(technicalDetails?.maxTorqueAtRPM, ' об/хв') },
  { label: 'Робочий обʼєм двигуна', value: formatDecimal(technicalDetails?.engineDisplacement, ' л') },
  { label: 'Тип приводу', value: formatText(technicalDetails?.driveType) },
  { label: 'Витрата пального у місті', value: formatDecimal(technicalDetails?.fuelConsumptionCity, ' л/100 км') },
  { label: 'Витрата пального в змішаному циклі', value: formatDecimal(technicalDetails?.fuelConsumptionMixed, ' л/100 км') },
  { label: 'Витрата пального на трасі', value: formatDecimal(technicalDetails?.fuelConsumptionHighway, ' л/100 км') },
  { label: 'Запас ходу на електротязі', value: formatDecimal(technicalDetails?.electricRange, ' км') },
  { label: 'Довжина', value: formatDecimal(technicalDetails?.length, ' мм') },
  { label: 'Ширина', value: formatDecimal(technicalDetails?.width, ' мм') },
  { label: 'Висота', value: formatDecimal(technicalDetails?.height, ' мм') },
  { label: 'Колісна база', value: formatDecimal(technicalDetails?.wheelbase, ' мм') },
  { label: 'Передня колія', value: formatDecimal(technicalDetails?.frontTrack, ' мм') },
  { label: 'Задня колія', value: formatDecimal(technicalDetails?.rearTrack, ' мм') },
  { label: 'Споряджена маса', value: formatDecimal(technicalDetails?.curbWeight, ' кг') },
  { label: 'Повна маса', value: formatDecimal(technicalDetails?.grossWeight, ' кг') },
  { label: 'Обʼєм паливного бака', value: formatDecimal(technicalDetails?.fuelTankCapacity, ' л') },
  { label: 'Радіус розвороту', value: formatDecimal(technicalDetails?.turningCircle, ' м') },
  { label: 'Передні гальма', value: formatText(technicalDetails?.frontBrakes) },
  { label: 'Задні гальма', value: formatText(technicalDetails?.rearBrakes) },
  { label: 'Передня підвіска', value: formatText(technicalDetails?.frontSuspension) },
  { label: 'Задня підвіска', value: formatText(technicalDetails?.rearSuspension) },
]

const formatDate = (value: string): string => {
  const timestamp = Date.parse(value)

  if (Number.isNaN(timestamp)) {
    return ''
  }

  return new Date(timestamp).toLocaleDateString('uk-UA')
}

const calculateRatingData = (reviews: OwnerReviewView[]) => {
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
}

export function TrimDetailsPage() {
  const { trimId } = useParams<{ trimId: string }>()
  const navigate = useNavigate()
  const { currentUser } = useAuth()
  const [trim, setTrim] = useState<TrimFullDetailsDto | null>(null)
  const [reviews, setReviews] = useState<OwnerReviewView[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [reviewModalReview, setReviewModalReview] = useState<
    | { id: number; userId: number; trimId: number; rating: number; content: string }
    | null
  >(null)
  const [deleteReviewId, setDeleteReviewId] = useState<number | null>(null)
  const [isDeletingReview, setIsDeletingReview] = useState(false)

  const ratingData = useMemo(() => calculateRatingData(reviews), [reviews])
  const activeStars = Math.round(ratingData.average)

  const refreshReviews = async () => {
    if (validTrimId === null) {
      return
    }

    const reviewsData = await getReviewsByTrim(validTrimId)

    const normalizedReviews: OwnerReviewView[] = (reviewsData || []).map((item) => ({
      key: `review-${item.review.id}`,
      id: item.review.id,
      userId: item.review.userId,
      trimId: item.review.trimId,
      rating: item.review.rating,
      content: item.review.content,
      username: item.username || 'Невідомий',
      createdAt: item.review.createdAt,
    }))

    setReviews(normalizedReviews)
  }

  const parsedTrimId = Number.parseInt(trimId ?? '', 10)
  const validTrimId = Number.isFinite(parsedTrimId) && parsedTrimId > 0 ? parsedTrimId : null

  useEffect(() => {
    if (validTrimId === null) {
      setError('Невірний ID комплектації.')
      setIsLoading(false)
      return
    }

    let cancelled = false

    async function loadTrimDetails() {
      try {
        const [trimData, reviewsData] = await Promise.all([
          getTrimFullDetails(validTrimId!),
          getReviewsByTrim(validTrimId!),
        ])

        if (cancelled) {
          return
        }

        if (!trimData) {
          setError('Комплектацію не знайдено.')
          setTrim(null)
          return
        }

        setTrim(trimData)

        const normalizedReviews: OwnerReviewView[] = (reviewsData || []).map((item) => ({
          key: `review-${item.review.id}`,
          id: item.review.id,
          userId: item.review.userId,
          trimId: item.review.trimId,
          rating: item.review.rating,
          content: item.review.content,
          username: item.username || 'Невідомий',
          createdAt: item.review.createdAt,
        }))

        setReviews(normalizedReviews)
      } catch {
        if (!cancelled) {
          setError('Не вдалося завантажити деталі комплектації.')
          setTrim(null)
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false)
        }
      }
    }

    void loadTrimDetails()

    return () => {
      cancelled = true
    }
  }, [validTrimId])

  if (isLoading) {
    return (
      <div className="catalog-page">
        <div className="top-band" />
        <SiteHeader />
        <main className="catalog-main">
          <p>Завантаження...</p>
        </main>
        <SiteFooter />
      </div>
    )
  }

  if (error || !trim) {
    return (
      <div className="catalog-page">
        <div className="top-band" />
        <SiteHeader />
        <main className="catalog-main">
          <p className="error-message">{error || 'Помилка завантаження'}</p>
          <button onClick={() => navigate(-1)}>Повернутися назад</button>
        </main>
        <SiteFooter />
      </div>
    )
  }

  const {
    brand,
    model,
    generation,
    generationVariant,
    name: trimName,
    transmissionType,
    doorsCount,
    seatsCount,
    technicalDetails,
  } = trim

  const pageTitle = `${brand.name} ${model.name} ${generationVariant.name} ${trimName}`
  const detailRows = technicalDetailRows(technicalDetails)
  const productionYears =
    generation.yearFrom > 0 && generation.yearTo > 0
      ? `${generation.yearFrom}-${generation.yearTo}`
      : 'Роки випуску невідомі'

  return (
    <div className="catalog-page">
      <div className="top-band" />
      <SiteHeader />

      <main className="catalog-main car-details-main">
        <section className="car-details-hero-panel">
          <div className="car-gallery-block">
            <div className="car-main-photo-wrap">
              {generation.photoUrl ? (
                <img
                  src={generation.photoUrl}
                  alt={`${brand.name} ${model.name}`}
                  className="car-main-photo"
                />
              ) : (
                <div className="placeholder-photo car-main-photo-placeholder">
                  Фото відсутнє
                </div>
              )}
            </div>
          </div>

          <aside className="car-summary-block">
            <h2>
              {brand.name} {model.name}
            </h2>
            <p className="car-summary-generation">{generationVariant.name}</p>
            <p className="car-summary-generation">
              {generationVariant.bodyStyleName} • {generationVariant.variantType}
            </p>
            <p className="car-summary-years">{productionYears}</p>

            <div className="car-summary-actions">
              <Link to={`/cars/variants/${generationVariant.id}`} className="car-secondary-link">
                Назад до моделі
              </Link>
            </div>
          </aside>
        </section>

        <h1>Технічні характеристики {pageTitle}</h1>

        <section className="trim-details-section">
          <h2>Основні характеристики</h2>
          <table className="details-table">
            <tbody>
              <tr>
                <td className="label">Комплектація</td>
                <td className="value">{trimName}</td>
              </tr>
              <tr>
                <td className="label">Коробка передач</td>
                <td className="value">{transmissionType || '—'}</td>
              </tr>
              {doorsCount && (
                <tr>
                  <td className="label">Кількість дверей</td>
                  <td className="value">{doorsCount}</td>
                </tr>
              )}
              {seatsCount && (
                <tr>
                  <td className="label">Кількість місць</td>
                  <td className="value">{seatsCount}</td>
                </tr>
              )}
            </tbody>
          </table>
        </section>

        <section className="trim-details-section">
          <h2>Технічні характеристики</h2>
          <table className="details-table">
            <tbody>
              {detailRows.map((row) => (
                <tr key={row.label}>
                  <td className="label">{row.label}</td>
                  <td className="value">{row.value}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </section>

        <section className="trim-details-section reviews-section model-block model-rating-block">
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

        <section className="trim-details-section model-block">
          <h3>Відгуки власників</h3>

          {reviews.length === 0 && (
            <p className="muted-note">Поки немає відгуків для цієї моделі.</p>
          )}

          {reviews.length > 0 && (
            <ul className="owner-reviews-list">
              {reviews.map((review) => (
                <li key={review.key} className="owner-review-card">
                  <div className="owner-review-head">
                    <strong>{review.username}</strong>
                    <span>{trim.name}</span>
                    <span>{review.rating}/10</span>
                  </div>
                  <p>{review.content || 'Без тексту відгуку'}</p>
                  {currentUser?.id === review.userId && (
                    <div className="owner-review-actions">
                      <button
                        type="button"
                        className="owner-review-edit-btn"
                        aria-label="Редагувати відгук"
                        title="Редагувати відгук"
                        onClick={() => {
                          setReviewModalReview({
                            id: review.id,
                            userId: review.userId,
                            trimId: review.trimId,
                            rating: review.rating,
                            content: review.content,
                          })
                        }}
                      >
                        <i className="fa-solid fa-pen-to-square" aria-hidden="true"></i>
                      </button>
                      <button
                        type="button"
                        className="owner-review-delete-btn"
                        aria-label="Видалити відгук"
                        title="Видалити відгук"
                        onClick={() => setDeleteReviewId(review.id)}
                      >
                        <i className="fa-solid fa-trash" aria-hidden="true"></i>
                      </button>
                    </div>
                  )}
                  {review.createdAt && (
                    <small>{formatDate(review.createdAt)}</small>
                  )}
                </li>
              ))}
            </ul>
          )}
        </section>

        <ReviewModal
          isOpen={reviewModalReview !== null}
          onClose={() => setReviewModalReview(null)}
          trims={trim ? [{ id: trim.id, name: trim.name }] : []}
          initialReview={reviewModalReview}
          title="Редагувати відгук"
          submitLabel="Зберегти зміни"
          onSubmitted={async () => {
            await refreshReviews()
          }}
          onRequireLogin={() => {
            navigate('/login', {
              state: { from: `/cars/variants/${trim?.generationVariant.id}/trims/${trim?.id}` },
            })
          }}
        />

        <ConfirmDeleteModal
          isOpen={deleteReviewId !== null}
          message="Видалити ваш відгук?"
          isSubmitting={isDeletingReview}
          onClose={() => {
            if (!isDeletingReview) {
              setDeleteReviewId(null)
            }
          }}
          onConfirm={async () => {
            if (!deleteReviewId) {
              return
            }

            setIsDeletingReview(true)

            try {
              const ok = await deleteReview(deleteReviewId)

              if (ok) {
                setDeleteReviewId(null)
                await refreshReviews()
              } else {
                alert('Не вдалося видалити відгук.')
              }
            } finally {
              setIsDeletingReview(false)
            }
          }}
        />
      </main>

      <SiteFooter />
    </div>
  )
}
