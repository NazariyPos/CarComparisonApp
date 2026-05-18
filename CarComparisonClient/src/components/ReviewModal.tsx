import React, { useState } from 'react'
import { createReview, updateReview } from '../services/carApi'
import { isAxiosError } from 'axios'

interface TrimOption {
  id: number
  name: string
}

interface Props {
  isOpen: boolean
  onClose: () => void
  trims: TrimOption[]
  defaultTrimId?: number | null
  initialReview?: {
    id: number
    userId: number
    trimId: number
    rating: number
    content: string
  } | null
  title?: string
  submitLabel?: string
  onSubmitted?: () => void
  onRequireLogin?: () => void
}

export const ReviewModal: React.FC<Props> = ({
  isOpen,
  onClose,
  trims,
  defaultTrimId,
  initialReview,
  title = 'Написати відгук',
  submitLabel = 'Опублікувати',
  onSubmitted,
  onRequireLogin,
}) => {
  const initialTrimId = initialReview?.trimId ?? defaultTrimId ?? (trims[0]?.id ?? null)
  const [trimId, setTrimId] = useState<number | null>(initialTrimId)
  const [rating, setRating] = useState<number>(initialReview?.rating ?? 0)
  const [content, setContent] = useState<string>(initialReview?.content ?? '')
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [error, setError] = useState<string | null>(null)

  React.useEffect(() => {
    if (isOpen) {
      setTrimId(initialReview?.trimId ?? defaultTrimId ?? (trims[0]?.id ?? null))
      setRating(initialReview?.rating ?? 0)
      setContent(initialReview?.content ?? '')
      setError(null)
    }
  }, [isOpen, defaultTrimId, initialReview, trims])

  if (!isOpen) return null

  const handleSubmit = async () => {
    if (!trimId || rating < 1 || content.trim().length === 0) {
      setError('Будь ласка, виберіть комплектацію, оцінку та напишіть відгук.')
      return
    }

    setIsSubmitting(true)
    setError(null)

    try {
      if (initialReview) {
        await updateReview(initialReview.id, {
          id: initialReview.id,
          userId: initialReview.userId,
          trimId,
          rating,
          content: content.trim(),
        })
      } else {
        await createReview({ trimId, rating, content: content.trim() })
      }
      if (onSubmitted) onSubmitted()
      onClose()
    } catch (err: unknown) {
      if (isAxiosError(err) && err.response?.status === 401) {
        if (onRequireLogin) onRequireLogin()
      } else if (isAxiosError(err) && err.response?.status === 400) {
        setError('Помилка: перевірте введені дані.')
      } else {
        setError('Не вдалося надіслати відгук. Спробуйте пізніше.')
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="modal-backdrop review-modal-backdrop" role="presentation" onClick={onClose}>
      <div
        className="modal review-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="review-modal-title"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="modal-header review-modal-header">
          <div>
            <p className="review-modal-eyebrow">Відгук</p>
            <h3 id="review-modal-title">{title}</h3>
          </div>
          <button className="modal-close review-modal-close" type="button" onClick={onClose} aria-label="Close">×</button>
        </div>

        <div className="modal-body review-modal-body">
          <label className="review-modal-field">
            <span>Модифікація</span>
            <select
              className="review-modal-control"
              value={trimId ?? ''}
              onChange={(e) => setTrimId(Number(e.target.value) || null)}
            >
              <option value="">Оберіть модифікацію</option>
              {trims.map((t) => (
                <option key={t.id} value={t.id}>{t.name}</option>
              ))}
            </select>
          </label>

          <div className="review-modal-field">
            <span>Оцінка</span>
            <div className="review-stars" aria-label="Оцінка від 1 до 10">
              {Array.from({ length: 10 }, (_, idx) => idx + 1).map((val) => (
                <button
                  key={val}
                  type="button"
                  onClick={() => setRating(val)}
                  className={val <= rating ? 'review-star review-star-active' : 'review-star'}
                  aria-label={`Оцінка ${val}`}
                  aria-pressed={rating === val}
                >
                  ★
                </button>
              ))}
            </div>
          </div>

          <label className="review-modal-field">
            <span>Відгук</span>
            <textarea
              className="review-modal-control review-modal-textarea"
              value={content}
              onChange={(e) => setContent(e.target.value)}
              rows={8}
              placeholder="Напишіть відгук"
            />
          </label>

          {error && <p className="error-text review-modal-error">{error}</p>}

          <div className="review-modal-actions">
            <button type="button" onClick={onClose} disabled={isSubmitting} className="admin-secondary-button review-modal-button">
              Відхилити
            </button>
            <button type="button" onClick={handleSubmit} disabled={isSubmitting} className="primary-cta review-modal-button">
              {submitLabel}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
