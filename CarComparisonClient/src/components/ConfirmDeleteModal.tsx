import React from 'react'

interface ConfirmDeleteModalProps {
  isOpen: boolean
  title?: string
  message: string
  confirmLabel?: string
  cancelLabel?: string
  isSubmitting?: boolean
  onConfirm: () => Promise<void> | void
  onClose: () => void
}

export const ConfirmDeleteModal: React.FC<ConfirmDeleteModalProps> = ({
  isOpen,
  title = 'Підтвердження видалення',
  message,
  confirmLabel = 'Видалити',
  cancelLabel = 'Скасувати',
  isSubmitting = false,
  onConfirm,
  onClose,
}) => {
  if (!isOpen) {
    return null
  }

  return (
    <div className="modal-backdrop" role="presentation" onClick={onClose}>
      <div
        className="modal confirm-delete-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="confirm-delete-title"
        onClick={(event) => event.stopPropagation()}
      >
        <div className="modal-header confirm-delete-header">
          <h3 id="confirm-delete-title">{title}</h3>
          <button
            className="modal-close"
            type="button"
            onClick={onClose}
            disabled={isSubmitting}
            aria-label="Закрити"
          >
            ×
          </button>
        </div>

        <div className="modal-body confirm-delete-body">
          <p>{message}</p>
          <div className="confirm-delete-actions">
            <button
              type="button"
              className="admin-secondary-button"
              onClick={onClose}
              disabled={isSubmitting}
            >
              {cancelLabel}
            </button>
            <button
              type="button"
              className="admin-danger-button"
              onClick={() => void onConfirm()}
              disabled={isSubmitting}
            >
              {isSubmitting ? 'Видалення...' : confirmLabel}
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}
