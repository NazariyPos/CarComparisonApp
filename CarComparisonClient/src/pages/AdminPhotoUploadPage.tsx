import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { useLocation, useNavigate } from 'react-router-dom'
import { SiteFooter } from '../components/SiteFooter'
import { SiteHeader } from '../components/SiteHeader'
import { useAuth } from '../context/AuthContext'
import {
  deleteGenerationVariantImage,
  getBrands,
  getGenerationVariantImages,
  getGenerationVariantsByModel,
  getGenerationsByModel,
  getModelsByBrand,
  setGenerationVariantImagePrimary,
  uploadGenerationVariantImage,
  type BrandBasicDto,
  type GenerationImageDto,
  type GenerationDto,
  type GenerationVariantDto,
  type ModelDto,
} from '../services/carApi.ts'

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

const formatDate = (value: string): string => {
  const timestamp = Date.parse(value)

  if (Number.isNaN(timestamp)) {
    return value
  }

  return new Date(timestamp).toLocaleString('uk-UA')
}

export function AdminPhotoUploadPage() {
  const { currentUser, isAuthenticated, isAuthReady } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [brands, setBrands] = useState<BrandBasicDto[]>([])
  const [models, setModels] = useState<ModelDto[]>([])
  const [generations, setGenerations] = useState<GenerationDto[]>([])
  const [variants, setVariants] = useState<GenerationVariantDto[]>([])
  const [images, setImages] = useState<GenerationImageDto[]>([])

  const [brandId, setBrandId] = useState('')
  const [modelId, setModelId] = useState('')
  const [generationId, setGenerationId] = useState('')
  const [variantId, setVariantId] = useState('')
  const [selectedFile, setSelectedFile] = useState<File | null>(null)
  const [isPrimary, setIsPrimary] = useState(true)
  const [sortOrder, setSortOrder] = useState('')
  const [isLoading, setIsLoading] = useState(false)
  const [isUploading, setIsUploading] = useState(false)
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (!isAuthReady) {
      return
    }

    if (!isAuthenticated) {
      navigate('/login', {
        replace: true,
        state: { from: `${location.pathname}${location.search}${location.hash}` },
      })
      return
    }

    if (!currentUser?.isAdmin) {
      navigate('/', { replace: true })
    }
  }, [currentUser?.isAdmin, isAuthenticated, isAuthReady, location.hash, location.pathname, location.search, navigate])

  const selectedBrand = useMemo(
    () => brands.find((item) => String(item.id) === brandId),
    [brands, brandId],
  )
  const selectedModel = useMemo(
    () => models.find((item) => String(item.id) === modelId),
    [models, modelId],
  )
  const selectedGeneration = useMemo(
    () => generations.find((item) => String(item.id) === generationId),
    [generations, generationId],
  )
  const selectedVariant = useMemo(
    () => variants.find((item) => String(item.id) === variantId),
    [variants, variantId],
  )

  const visibleVariants = useMemo(() => {
    if (!generationId) {
      return variants
    }

    return variants.filter((item) => String(item.generationId) === generationId)
  }, [generationId, variants])

  useEffect(() => {
    async function loadBrands() {
      setIsLoading(true)
      setError(null)

      try {
        const data = await getBrands()
        setBrands(data)
      } catch {
        setError('Не вдалося завантажити список марок.')
      } finally {
        setIsLoading(false)
      }
    }

    void loadBrands()
  }, [])

  useEffect(() => {
    async function loadModels() {
      if (!brandId) {
        setModels([])
        setGenerations([])
        setVariants([])
        setGenerationId('')
        setVariantId('')
        setImages([])
        return
      }

      try {
        const data = await getModelsByBrand(Number.parseInt(brandId, 10))
        setModels(data)
      } catch {
        setModels([])
      }
    }

    void loadModels()
  }, [brandId])

  useEffect(() => {
    async function loadModelScope() {
      if (!modelId) {
        setGenerations([])
        setVariants([])
        setGenerationId('')
        setVariantId('')
        setImages([])
        return
      }

      try {
        const [generationData, variantData] = await Promise.all([
          getGenerationsByModel(Number.parseInt(modelId, 10)),
          getGenerationVariantsByModel(Number.parseInt(modelId, 10)),
        ])

        setGenerations(generationData)
        setVariants(variantData)
      } catch {
        setGenerations([])
        setVariants([])
      }
    }

    void loadModelScope()
  }, [modelId])

  useEffect(() => {
    if (generationId && !generations.some((item) => String(item.id) === generationId)) {
      setGenerationId('')
      setVariantId('')
      setImages([])
    }
  }, [generationId, generations])

  useEffect(() => {
    if (!variantId) {
      setImages([])
      return
    }

    const selected = variants.find((item) => String(item.id) === variantId)
    if (!selected) {
      setImages([])
      return
    }

    if (generationId && String(selected.generationId) !== generationId) {
      setVariantId('')
      setImages([])
      return
    }

    const activeVariant = selected

    async function loadImages() {
      try {
        const data = await getGenerationVariantImages(activeVariant.generationId, activeVariant.id)
        setImages(data)
      } catch {
        setImages([])
      }
    }

    void loadImages()
  }, [generationId, variantId, variants])

  const handleUpload = async () => {
    if (!selectedFile || !selectedVariant) {
      setError('Вибери варіант і файл зображення.')
      return
    }

    const parsedSortOrder = sortOrder.trim() ? Number.parseInt(sortOrder, 10) : undefined

    setIsUploading(true)
    setError(null)
    setMessage(null)

    try {
      const created = await uploadGenerationVariantImage(
        selectedVariant.generationId,
        selectedVariant.id,
        selectedFile,
        isPrimary,
        parsedSortOrder,
      )

      if (!created) {
        setError('Не вдалося завантажити фото.')
        return
      }

      const refreshedImages = await getGenerationVariantImages(
        selectedVariant.generationId,
        selectedVariant.id,
      )

      setImages(refreshedImages)
      setSelectedFile(null)
      setSortOrder('')
      setMessage('Фото завантажено.')
    } catch {
      setError('Не вдалося завантажити фото.')
    } finally {
      setIsUploading(false)
    }
  }

  const handleSetPrimary = async (imageId: number) => {
    if (!selectedVariant) {
      return
    }

    const updated = await setGenerationVariantImagePrimary(
      selectedVariant.generationId,
      selectedVariant.id,
      imageId,
    )

    if (!updated) {
      setError('Не вдалося змінити головне фото.')
      return
    }

    const refreshedImages = await getGenerationVariantImages(
      selectedVariant.generationId,
      selectedVariant.id,
    )

    setImages(refreshedImages)
    setMessage('Головне фото оновлено.')
  }

  const handleDeleteImage = async (imageId: number) => {
    if (!selectedVariant) {
      return
    }

    const deleted = await deleteGenerationVariantImage(
      selectedVariant.generationId,
      selectedVariant.id,
      imageId,
    )

    if (!deleted) {
      setError('Не вдалося видалити фото.')
      return
    }

    const refreshedImages = await getGenerationVariantImages(
      selectedVariant.generationId,
      selectedVariant.id,
    )

    setImages(refreshedImages)
    setMessage('Фото видалено.')
  }

  return (
    <div className="catalog-page admin-page">
      <div className="top-band admin-top-band" />
      <SiteHeader />

      <main className="catalog-main admin-main">
        {!isAuthReady ? (
          <section className="admin-panel">
            <h2>Перевірка доступу</h2>
            <p className="muted-note">Завантажуємо дані користувача...</p>
          </section>
        ) : null}

        <section className="admin-hero">
          <div>
            <p className="admin-eyebrow">Адмін-функціонал</p>
            <h1>Додавання фото для модифікації</h1>
            <p className="admin-intro">
              На цій сторінці ви можете додати/видалити фото для конкретної модифікації автомобіля, а також встановити головне фото.
            </p>
          </div>

          <Link to="/brands" className="admin-back-link">
            Повернутися до каталогу
          </Link>
        </section>

        <section className="admin-panel">
          <h2>Обери цільову модифікацію</h2>

          <div className="admin-grid">
            <label className="admin-field">
              <span>Марка</span>
              <select
                value={brandId}
                onChange={(event) => {
                  setBrandId(event.target.value)
                  setModelId('')
                  setGenerationId('')
                  setVariantId('')
                  setImages([])
                }}
              >
                <option value="">Оберіть марку</option>
                {brands.map((brand) => (
                  <option key={brand.id} value={brand.id}>
                    {brand.name}
                  </option>
                ))}
              </select>
            </label>

            <label className="admin-field">
              <span>Модель</span>
              <select
                value={modelId}
                onChange={(event) => {
                  setModelId(event.target.value)
                  setGenerationId('')
                  setVariantId('')
                  setImages([])
                }}
                disabled={!brandId}
              >
                <option value="">Оберіть модель</option>
                {models.map((model) => (
                  <option key={model.id} value={model.id}>
                    {model.name}
                  </option>
                ))}
              </select>
            </label>

            <label className="admin-field">
              <span>Покоління</span>
              <select
                value={generationId}
                onChange={(event) => {
                  setGenerationId(event.target.value)
                  setVariantId('')
                  setImages([])
                }}
                disabled={!modelId}
              >
                <option value="">Оберіть покоління</option>
                {generations.map((generation) => (
                  <option key={generation.id} value={generation.id}>
                    {generation.name} ({generation.yearFrom}-{generation.yearTo})
                  </option>
                ))}
              </select>
            </label>

            <label className="admin-field">
              <span>Модифікація</span>
              <select
                value={variantId}
                onChange={(event) => setVariantId(event.target.value)}
                disabled={!modelId || visibleVariants.length === 0}
              >
                <option value="">Оберіть модифікацію</option>
                {visibleVariants.map((variant) => (
                  <option key={variant.id} value={variant.id}>
                    {variant.name} • {variant.bodyStyleName} • {variant.yearFrom}-{variant.yearTo}
                  </option>
                ))}
              </select>
            </label>
          </div>

          {selectedBrand && selectedModel && selectedGeneration && selectedVariant && (
            <div className="admin-selection-summary">
              <strong>{selectedBrand.name}</strong>
              <span>{selectedModel.name}</span>
              <span>{selectedGeneration.name}</span>
              <span>{selectedVariant.name}</span>
            </div>
          )}
        </section>

        <section className="admin-panel">
          <h2>Завантажити фото</h2>
          <div className="admin-grid admin-upload-grid">
            <label className="admin-field admin-file-field">
              <span>Файл</span>
              <input
                type="file"
                accept="image/png,image/jpeg,image/webp"
                onChange={(event) => setSelectedFile(event.target.files?.[0] ?? null)}
              />
            </label>

            <label className="admin-field">
              <span>Порядок сортування</span>
              <input
                type="number"
                min={1}
                value={sortOrder}
                onChange={(event) => setSortOrder(event.target.value)}
                placeholder="Наприклад 1"
              />
            </label>

            <label className="admin-check-field">
              <input
                type="checkbox"
                checked={isPrimary}
                onChange={(event) => setIsPrimary(event.target.checked)}
              />
              <span>Зробити головним фото</span>
            </label>
          </div>

          <button
            type="button"
            className="primary-cta"
            onClick={handleUpload}
            disabled={!selectedVariant || !selectedFile || isUploading}
          >
            {isUploading ? 'Завантаження...' : 'Завантажити фото'}
          </button>

          {message && <p className="status-pill admin-status-pill">{message}</p>}
          {error && <p className="error-text">{error}</p>}
        </section>

        <section className="admin-panel">
          <h2>Поточні фото</h2>
          {selectedVariant ? (
            images.length > 0 ? (
              <ul className="admin-gallery">
                {images.map((image) => (
                  <li key={image.id} className="admin-gallery-item">
                    <img
                      src={resolveImageUrl(image.url)}
                      alt={`${selectedVariant.name} ${image.id}`}
                      className="admin-gallery-image"
                    />
                    <div className="admin-gallery-meta">
                      <strong>{image.isPrimary ? 'Головне фото' : 'Фото'}</strong>
                      <span>Порядок: {image.sortOrder}</span>
                      <span>{formatDate(image.createdAt)}</span>
                    </div>
                    <div className="admin-gallery-actions">
                      <button
                        type="button"
                        className="admin-secondary-button"
                        onClick={() => void handleSetPrimary(image.id)}
                        disabled={image.isPrimary}
                      >
                        Зробити головним
                      </button>
                      <button
                        type="button"
                        className="admin-danger-button"
                        onClick={() => void handleDeleteImage(image.id)}
                      >
                        Видалити
                      </button>
                    </div>
                  </li>
                ))}
              </ul>
            ) : (
              <p className="muted-note">Для цієї модифікації ще немає фото.</p>
            )
          ) : (
            <p className="muted-note">Спершу обери модифікацію.</p>
          )}
        </section>

        {isLoading && <p className="muted-note">Завантаження довідкових даних...</p>}
      </main>

      <SiteFooter />
    </div>
  )
}