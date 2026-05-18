import { useEffect, useMemo, useState } from 'react'
import { useLocation, useNavigate } from 'react-router-dom'
import { SiteFooter } from '../components/SiteFooter'
import { SiteHeader } from '../components/SiteHeader'
import { ReviewModal } from '../components/ReviewModal'
import { ConfirmDeleteModal } from '../components/ConfirmDeleteModal'
import { useAuth } from '../context/AuthContext'
import { getCurrentUser, type AuthUser } from '../services/authApi'
import { deleteReview, getReviewsByUser, type ReviewWithDetailsDto } from '../services/carApi'

const formatDate = (value: string): string => {
  const timestamp = Date.parse(value)

  if (Number.isNaN(timestamp)) {
    return ''
  }

  return new Date(timestamp).toLocaleDateString('uk-UA')
}

const roleLabel = (isAdmin: boolean): string => {
  return isAdmin ? 'Адміністратор' : 'Користувач'
}

export function UserProfilePage() {
  const { currentUser, isAuthenticated, isAuthReady } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [profile, setProfile] = useState<AuthUser | null>(currentUser)
  const [reviews, setReviews] = useState<ReviewWithDetailsDto[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [reviewModalReview, setReviewModalReview] = useState<
    | { id: number; userId: number; trimId: number; trimName: string; rating: number; content: string }
    | null
  >(null)
  const [deleteReviewId, setDeleteReviewId] = useState<number | null>(null)
  const [isDeletingReview, setIsDeletingReview] = useState(false)

  useEffect(() => {
    if (!isAuthReady) {
      return
    }

    if (!isAuthenticated) {
      navigate('/login', {
        replace: true,
        state: { from: `${location.pathname}${location.search}${location.hash}` },
      })
    }
  }, [isAuthenticated, isAuthReady, location.hash, location.pathname, location.search, navigate])

  useEffect(() => {
    if (!isAuthReady || !isAuthenticated) {
      return
    }

    let cancelled = false

    async function loadProfileData() {
      setIsLoading(true)
      setError(null)

      try {
        let resolvedProfile: AuthUser | null = currentUser

        try {
          const freshProfile = await getCurrentUser()
          resolvedProfile = freshProfile
        } catch {
          // Keep using the profile from auth context if refresh fails.
        }

        if (cancelled) {
          return
        }

        setProfile(resolvedProfile)

        const userId = resolvedProfile?.id ?? 0

        if (userId > 0) {
          const reviewList = await getReviewsByUser(userId)

          if (!cancelled) {
            setReviews(reviewList)
          }
        } else {
          setReviews([])
        }
      } catch {
        if (!cancelled) {
          setError('Не вдалося завантажити профіль користувача.')
          setReviews([])
        }
      } finally {
        if (!cancelled) {
          setIsLoading(false)
        }
      }
    }

    void loadProfileData()

    return () => {
      cancelled = true
    }
  }, [currentUser, isAuthReady, isAuthenticated])

  const sortedReviews = useMemo(() => {
    return [...reviews].sort((left, right) => {
      const leftTime = Date.parse(left.review.createdAt)
      const rightTime = Date.parse(right.review.createdAt)

      if (Number.isNaN(leftTime) || Number.isNaN(rightTime)) {
        return 0
      }

      return rightTime - leftTime
    })
  }, [reviews])

  const refreshReviews = async () => {
    const userId = profile?.id ?? currentUser?.id ?? 0

    if (userId > 0) {
      const reviewList = await getReviewsByUser(userId)
      setReviews(reviewList)
    }
  }

  return (
    <div className="catalog-page profile-page">
      <div className="top-band">
        <div className="top-band-title">
          <h2>Профіль користувача</h2>
        </div>
      </div>
      <SiteHeader />

      <main className="catalog-main">
        <section className="results-panel profile-panel">
          {isLoading && <p className="muted-note">Завантаження профілю...</p>}

          {!isLoading && error && <p className="error-text">{error}</p>}

          {!isLoading && !error && profile && (
            <>
              <article className="profile-summary-card">
                <h1>{profile.realName || profile.username || profile.login}</h1>
                <dl className="profile-summary-grid">
                  <div>
                    <dt>Логін</dt>
                    <dd>{profile.login || '—'}</dd>
                  </div>
                  <div>
                    <dt>Email</dt>
                    <dd>{profile.email || '—'}</dd>
                  </div>
                  <div>
                    <dt>Роль</dt>
                    <dd>{roleLabel(profile.isAdmin)}</dd>
                  </div>
                  <div>
                    <dt>Імʼя користувача</dt>
                    <dd>{profile.username || '—'}</dd>
                  </div>
                </dl>
                <p className="profile-about">
                  {profile.about || 'Користувач ще не додав інформацію про себе.'}
                </p>
              </article>

              <section className="profile-reviews-section">
                <h3>Мої відгуки</h3>

                {sortedReviews.length === 0 ? (
                  <p className="muted-note">Ви ще не залишали відгуків.</p>
                ) : (
                  <ul className="profile-reviews-list">
                    {sortedReviews.map((item) => (
                      <li key={`profile-review-${item.review.id}`} className="profile-review-card">
                        <div className="profile-review-head">
                          <strong>
                            {item.brand} {item.model}
                          </strong>
                          <span>{item.review.rating}/10</span>
                        </div>
                        <p className="profile-review-car">
                          {item.generation} • {item.trim}
                        </p>
                        <p>{item.review.content || 'Без тексту відгуку'}</p>
                        <div className="profile-review-actions">
                          <button
                            type="button"
                            className="profile-review-edit-btn"
                            aria-label="Редагувати відгук"
                            title="Редагувати відгук"
                            onClick={() => {
                              setReviewModalReview({
                                id: item.review.id,
                                userId: item.review.userId,
                                trimId: item.review.trimId,
                                trimName: item.trim,
                                rating: item.review.rating,
                                content: item.review.content,
                              })
                            }}
                          >
                            <i className="fa-solid fa-pen-to-square" aria-hidden="true"></i>
                          </button>
                          <button
                            type="button"
                            className="profile-review-delete-btn"
                            aria-label="Видалити відгук"
                            title="Видалити відгук"
                            onClick={() => setDeleteReviewId(item.review.id)}
                          >
                            <i className="fa-solid fa-trash" aria-hidden="true"></i>
                          </button>
                        </div>
                        {item.review.createdAt && (
                          <small>{formatDate(item.review.createdAt)}</small>
                        )}
                      </li>
                    ))}
                  </ul>
                )}
              </section>

              <ReviewModal
                isOpen={reviewModalReview !== null}
                onClose={() => setReviewModalReview(null)}
                trims={reviewModalReview ? [{ id: reviewModalReview.trimId, name: reviewModalReview.trimName }] : []}
                initialReview={reviewModalReview}
                title="Редагувати відгук"
                submitLabel="Зберегти зміни"
                onSubmitted={async () => {
                  await refreshReviews()
                }}
                onRequireLogin={() => {
                  navigate('/login', {
                    replace: true,
                    state: { from: `${location.pathname}${location.search}${location.hash}` },
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
            </>
          )}
        </section>
      </main>

      <SiteFooter />
    </div>
  )
}

export default UserProfilePage
