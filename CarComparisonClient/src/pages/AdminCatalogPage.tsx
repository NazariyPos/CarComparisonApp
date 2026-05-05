import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { SiteFooter } from '../components/SiteFooter'
import { SiteHeader } from '../components/SiteHeader'
import { useAuth } from '../context/AuthContext'
import {
  type BodyStyleOptionDto,
  type BrandBasicDto,
  type GenerationDto,
  type GenerationWithTrimsDto,
  type ModelDto,
  createBrand,
  createGeneration,
  createGenerationVariant,
  createModel,
  createTechnicalDetails,
  createTrim,
  getBrands,
  getGenerationWithTrims,
  getGenerationsByModel,
  getSearchFacets,
  getModelsByBrand,
} from '../services/carApi'

export function AdminCatalogPage() {
  const { currentUser, isAuthenticated, isAuthReady } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [brands, setBrands] = useState<BrandBasicDto[]>([])
  const [bodyStyles, setBodyStyles] = useState<BodyStyleOptionDto[]>([])
  const [models, setModels] = useState<ModelDto[]>([])
  const [generations, setGenerations] = useState<GenerationDto[]>([])
  const [generationDetails, setGenerationDetails] = useState<GenerationWithTrimsDto | null>(null)

  const [brandId, setBrandId] = useState('')
  const [modelId, setModelId] = useState('')
  const [generationId, setGenerationId] = useState('')
  const [variantId, setVariantId] = useState('')
  const [trimId, setTrimId] = useState('')

  const [brandName, setBrandName] = useState('')
  const [modelName, setModelName] = useState('')
  const [modelBodyType, setModelBodyType] = useState('')
  const [generationName, setGenerationName] = useState('')
  const [generationYearFrom, setGenerationYearFrom] = useState('')
  const [generationYearTo, setGenerationYearTo] = useState('')
  const [variantName, setVariantName] = useState('')
  const [variantType, setVariantType] = useState('Standard')
  const [variantBodyStyleId, setVariantBodyStyleId] = useState('')
  const [variantDoorsCount, setVariantDoorsCount] = useState('')
  const [variantYearFrom, setVariantYearFrom] = useState('')
  const [variantYearTo, setVariantYearTo] = useState('')
  const [trimName, setTrimName] = useState('')
  const [trimTransmissionType, setTrimTransmissionType] = useState('')
  const [trimDoorsCount, setTrimDoorsCount] = useState('')
  const [trimSeatsCount, setTrimSeatsCount] = useState('')
  const [fuelType, setFuelType] = useState('')
  const [power, setPower] = useState('')
  const [driveType, setDriveType] = useState('')

  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isLoadingLookups, setIsLoadingLookups] = useState(false)

  useEffect(() => {
    if (!isAuthReady) return
    if (!isAuthenticated) {
      navigate('/login', { replace: true, state: { from: location.pathname } })
      return
    }
    if (!currentUser?.isAdmin) {
      navigate('/', { replace: true })
    }
  }, [currentUser?.isAdmin, isAuthenticated, isAuthReady, location.pathname, navigate])

  useEffect(() => {
    async function loadInitialData() {
      setIsLoadingLookups(true)
      setError(null)

      try {
        const brandData = await getBrands()
        setBrands(brandData)
      } catch {
        setError('Не вдалося завантажити марки. Перевірте, чи запущений API.')
      }

      try {
        const facetData = await getSearchFacets({})
        setBodyStyles(facetData.bodyStyles)
      } catch {
        setBodyStyles([])
      } finally {
        setIsLoadingLookups(false)
      }
    }

    void loadInitialData()
  }, [])

  useEffect(() => {
    async function loadModels() {
      if (!brandId) {
        setModels([])
        return
      }

      setModels(await getModelsByBrand(Number.parseInt(brandId, 10)))
    }

    void loadModels()
  }, [brandId])

  useEffect(() => {
    async function loadModelScope() {
      if (!modelId) {
        setGenerations([])
        setGenerationDetails(null)
        setGenerationId('')
        setVariantId('')
        setTrimId('')
        return
      }

      const generationData = await getGenerationsByModel(Number.parseInt(modelId, 10))
      setGenerations(generationData)
    }

    void loadModelScope()
  }, [modelId])

  useEffect(() => {
    async function loadGenerationScope() {
      if (!generationId) {
        setGenerationDetails(null)
        setVariantId('')
        setTrimId('')
        return
      }

      const details = await getGenerationWithTrims(Number.parseInt(generationId, 10))
      setGenerationDetails(details)

      if (details?.variants.length) {
        const existingVariant = details.variants.find((item) => String(item.id) === variantId)
        setVariantId(existingVariant ? String(existingVariant.id) : String(details.variants[0].id))
      } else {
        setVariantId('')
      }

      setTrimId('')
    }

    void loadGenerationScope()
  }, [generationId])

  const selectedBrand = useMemo(() => brands.find((item) => String(item.id) === brandId), [brandId, brands])
  const selectedModel = useMemo(() => models.find((item) => String(item.id) === modelId), [modelId, models])
  const selectedGeneration = useMemo(
    () => generations.find((item) => String(item.id) === generationId),
    [generationId, generations],
  )
  const selectedVariant = useMemo(
    () => generationDetails?.variants.find((item) => String(item.id) === variantId) ?? null,
    [generationDetails, variantId],
  )
  const selectedTrims = useMemo(() => {
    if (!generationDetails || !variantId) return []
    return generationDetails.trims.filter((item) => String(item.generationVariantId) === variantId)
  }, [generationDetails, variantId])
  const selectedTrim = useMemo(
    () => selectedTrims.find((item) => String(item.id) === trimId) ?? null,
    [selectedTrims, trimId],
  )

  const activePathLabel = useMemo(
    () => [selectedBrand?.name, selectedModel?.name, selectedGeneration?.name, selectedVariant?.name]
      .filter(Boolean)
      .join(' / '),
    [selectedBrand?.name, selectedModel?.name, selectedGeneration?.name, selectedVariant?.name],
  )

  const resetModelScope = () => {
    setModelId('')
    setGenerationId('')
    setVariantId('')
    setTrimId('')
    setModels([])
    setGenerations([])
    setGenerationDetails(null)
  }

  const resetGenerationScope = () => {
    setGenerationId('')
    setVariantId('')
    setTrimId('')
    setGenerations([])
    setGenerationDetails(null)
  }

  const handleCreateBrand = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage(null)
    setError(null)

    const created = await createBrand(brandName)
    if (!created) {
      setError('Не вдалося створити марку або вона вже існує')
      return
    }

    setBrands((current) => [...current, created])
    setBrandName('')
    setMessage(`Марку створено: ${created.name}`)
  }

  const handleCreateModel = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage(null)
    setError(null)

    if (!brandId) {
      setError('Оберіть марку')
      return
    }

    const created = await createModel(Number.parseInt(brandId, 10), modelName, modelBodyType || undefined)
    if (!created) {
      setError('Не вдалося створити модель або вона вже існує')
      return
    }

    setModels((current) => [...current, { ...created, bodyType: '' }])
    setModelName('')
    setModelBodyType('')
    setMessage(`Модель створено: ${created.name}`)
  }

  const handleCreateGeneration = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage(null)
    setError(null)

    if (!brandId || !modelId) {
      setError('Оберіть марку і модель')
      return
    }

    const created = await createGeneration(
      Number.parseInt(modelId, 10),
      generationName,
      Number.parseInt(generationYearFrom || '0', 10),
      Number.parseInt(generationYearTo || '0', 10),
    )

    if (!created) {
      setError('Не вдалося створити покоління або воно вже існує')
      return
    }

    setGenerations((current) => [...current, { ...created, yearFrom: Number.parseInt(generationYearFrom || '0', 10), yearTo: Number.parseInt(generationYearTo || '0', 10) }])
    setGenerationName('')
    setGenerationYearFrom('')
    setGenerationYearTo('')
    setMessage(`Покоління створено: ${created.name}`)
  }

  const handleCreateVariant = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage(null)
    setError(null)

    if (!generationId) {
      setError('Оберіть марку, модель і покоління')
      return
    }

    const bodyStyleId = Number.parseInt(variantBodyStyleId, 10)
    const created = await createGenerationVariant(Number.parseInt(generationId, 10), {
      name: variantName,
      variantType,
      bodyStyleId: Number.isFinite(bodyStyleId) && bodyStyleId > 0 ? bodyStyleId : undefined,
      doorsCount: Number.parseInt(variantDoorsCount || '0', 10),
      yearFrom: Number.parseInt(variantYearFrom || '0', 10),
      yearTo: Number.parseInt(variantYearTo || '0', 10),
      isDefault: false,
    })

    if (!created) {
      setError('Не вдалося створити комплектацію')
      return
    }

    setVariantName('')
    setVariantType('Standard')
    setVariantBodyStyleId('')
    setVariantDoorsCount('')
    setVariantYearFrom('')
    setVariantYearTo('')
    setMessage(`Модифікацію створено: ${created.name}`)

    const details = await getGenerationWithTrims(Number.parseInt(generationId, 10))
    setGenerationDetails(details)
  }

  const handleCreateTrim = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage(null)
    setError(null)

    if (!variantId) {
      setError('Оберіть марку, модель, покоління і варіант покоління')
      return
    }

    const created = await createTrim(Number.parseInt(variantId, 10), {
      name: trimName,
      transmissionType: trimTransmissionType || undefined,
      doorsCount: trimDoorsCount ? Number.parseInt(trimDoorsCount, 10) : undefined,
      seatsCount: trimSeatsCount ? Number.parseInt(trimSeatsCount, 10) : undefined,
    })

    if (!created) {
      setError('Не вдалося створити комплектацію або вона вже існує')
      return
    }

    setTrimName('')
    setTrimTransmissionType('')
    setTrimDoorsCount('')
    setTrimSeatsCount('')
    setMessage(`Комплектацію створено: ${created.name}`)

    const details = await getGenerationWithTrims(Number.parseInt(generationId, 10))
    setGenerationDetails(details)
  }

  const handleCreateTechnical = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage(null)
    setError(null)

    if (!trimId) {
      setError('Оберіть марку, модель, покоління і комплектацію')
      return
    }

    const created = await createTechnicalDetails(Number.parseInt(trimId, 10), {
      fuelType: fuelType || undefined,
      power: power ? Number.parseInt(power, 10) : undefined,
      driveType: driveType || undefined,
    })

    if (!created) {
      setError('Не вдалося додати або оновити технічні характеристики')
      return
    }

    setFuelType('')
    setPower('')
    setDriveType('')
    setMessage('Технічні характеристики збережено')

    const details = await getGenerationWithTrims(Number.parseInt(generationId, 10))
    setGenerationDetails(details)
  }

  return (
    <div className="catalog-page admin-page">
      <div className="top-band admin-top-band" />
      <SiteHeader />

      <main className="catalog-main admin-main">
        <section className="admin-hero">
          <div>
            <p className="admin-eyebrow">Адмін-функціонал</p>
            <h1>Керування каталогом</h1>
            <p className="admin-intro">
              Додавайте марку, модель, покоління, комплектацію і технічні характеристики у потрібному порядку.
            </p>
          </div>

          <Link to="/brands" className="admin-back-link">Повернутися до каталогу</Link>
        </section>

        <section className="admin-panel">
          <h2>Поточний вибір</h2>
          <p className="muted-note">
            {activePathLabel || 'Оберіть існуючу марку, модель, покоління або комплектацію.'}
          </p>
          {selectedVariant && selectedTrim ? (
            <div className="admin-selection-summary">
              <strong>{selectedVariant.name}</strong>
              <span>{selectedTrim.name}</span>
            </div>
          ) : null}
        </section>

        <section className="admin-panel">
          <h2>Додати марку</h2>
          <form onSubmit={handleCreateBrand} className="admin-form-stack">
            <label className="admin-field">
              <span>Назва марки</span>
              <input value={brandName} onChange={(event) => setBrandName(event.target.value)} />
            </label>
            <button type="submit" className="primary-cta">Додати марку</button>
          </form>
        </section>

        <section className="admin-panel">
          <h2>Додати модель</h2>
          {isLoadingLookups ? (
            <p className="muted-note">Завантажуємо марки...</p>
          ) : null}
          {!isLoadingLookups && brands.length === 0 ? (
            <p className="error-text">Марки не завантажені. Спочатку додайте марку або перевірте доступність API.</p>
          ) : null}
          <form onSubmit={handleCreateModel} className="admin-form-stack">
            <label className="admin-field">
              <span>Оберіть марку</span>
              <select value={brandId} onChange={(event) => { setBrandId(event.target.value); resetModelScope(); }}>
                <option value="">Оберіть марку</option>
                {brands.map((brand) => (
                  <option key={brand.id} value={brand.id}>{brand.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Назва моделі</span>
              <input value={modelName} onChange={(event) => setModelName(event.target.value)} />
            </label>
            <label className="admin-field">
              <span>Тип кузова</span>
              <input value={modelBodyType} onChange={(event) => setModelBodyType(event.target.value)} />
            </label>
            <button type="submit" className="primary-cta" disabled={!brandId}>Додати модель</button>
          </form>
        </section>

        <section className="admin-panel">
          <h2>Додати покоління</h2>
          <form onSubmit={handleCreateGeneration} className="admin-form-stack">
            <label className="admin-field">
              <span>Оберіть марку</span>
              <select value={brandId} onChange={(event) => { setBrandId(event.target.value); resetModelScope(); }}>
                <option value="">Оберіть марку</option>
                {brands.map((brand) => (
                  <option key={brand.id} value={brand.id}>{brand.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Оберіть модель</span>
              <select value={modelId} onChange={(event) => { setModelId(event.target.value); resetGenerationScope(); }} disabled={!brandId}>
                <option value="">Оберіть модель</option>
                {models.map((model) => (
                  <option key={model.id} value={model.id}>{model.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Назва покоління</span>
              <input value={generationName} onChange={(event) => setGenerationName(event.target.value)} />
            </label>
            <label className="admin-field">
              <span>Рік від</span>
              <input type="number" value={generationYearFrom} onChange={(event) => setGenerationYearFrom(event.target.value)} />
            </label>
            <label className="admin-field">
              <span>Рік до</span>
              <input type="number" value={generationYearTo} onChange={(event) => setGenerationYearTo(event.target.value)} />
            </label>
            <button type="submit" className="primary-cta" disabled={!brandId || !modelId}>Додати покоління</button>
          </form>
        </section>

        <section className="admin-panel">
          <h2>Додати варіант покоління</h2>
          <form onSubmit={handleCreateVariant} className="admin-form-stack">
            <label className="admin-field">
              <span>Оберіть марку</span>
              <select value={brandId} onChange={(event) => { setBrandId(event.target.value); resetModelScope(); }}>
                <option value="">Оберіть марку</option>
                {brands.map((brand) => (
                  <option key={brand.id} value={brand.id}>{brand.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Оберіть модель</span>
              <select value={modelId} onChange={(event) => { setModelId(event.target.value); resetGenerationScope(); }} disabled={!brandId}>
                <option value="">Оберіть модель</option>
                {models.map((model) => (
                  <option key={model.id} value={model.id}>{model.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Оберіть покоління</span>
              <select value={generationId} onChange={(event) => { setGenerationId(event.target.value); setVariantId(''); setTrimId(''); }} disabled={!modelId}>
                <option value="">Оберіть покоління</option>
                {generations.map((generation) => (
                  <option key={generation.id} value={generation.id}>{generation.name} ({generation.yearFrom}-{generation.yearTo})</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Назва варіанта</span>
              <input value={variantName} onChange={(event) => setVariantName(event.target.value)} />
            </label>
            <label className="admin-field">
              <span>Тип варіанту</span>
              <input value={variantType} onChange={(event) => setVariantType(event.target.value)} />
            </label>
            <label className="admin-field">
              <span>Оберіть кузов</span>
              <select value={variantBodyStyleId} onChange={(event) => setVariantBodyStyleId(event.target.value)} disabled={bodyStyles.length === 0}>
                <option value="">Оберіть кузов</option>
                {bodyStyles.map((style) => (
                  <option key={style.id} value={style.id}>{style.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field"><span>Двері</span><input type="number" value={variantDoorsCount} onChange={(event) => setVariantDoorsCount(event.target.value)} /></label>
            <label className="admin-field"><span>Рік від</span><input type="number" value={variantYearFrom} onChange={(event) => setVariantYearFrom(event.target.value)} /></label>
            <label className="admin-field"><span>Рік до</span><input type="number" value={variantYearTo} onChange={(event) => setVariantYearTo(event.target.value)} /></label>
            <button type="submit" className="primary-cta" disabled={!generationId || bodyStyles.length === 0}>Додати варіант</button>
          </form>
        </section>

        <section className="admin-panel">
          <h2>Додати комплектацію</h2>
          <form onSubmit={handleCreateTrim} className="admin-form-stack">
            <label className="admin-field">
              <span>Оберіть марку</span>
              <select value={brandId} onChange={(event) => { setBrandId(event.target.value); resetModelScope(); }}>
                <option value="">Оберіть марку</option>
                {brands.map((brand) => (
                  <option key={brand.id} value={brand.id}>{brand.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Оберіть модель</span>
              <select value={modelId} onChange={(event) => { setModelId(event.target.value); resetGenerationScope(); }} disabled={!brandId}>
                <option value="">Оберіть модель</option>
                {models.map((model) => (
                  <option key={model.id} value={model.id}>{model.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Оберіть покоління</span>
              <select value={generationId} onChange={(event) => { setGenerationId(event.target.value); setVariantId(''); setTrimId(''); }} disabled={!modelId}>
                <option value="">Оберіть покоління</option>
                {generations.map((generation) => (
                  <option key={generation.id} value={generation.id}>{generation.name} ({generation.yearFrom}-{generation.yearTo})</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Оберіть варіант покоління</span>
              <select value={variantId} onChange={(event) => { setVariantId(event.target.value); setTrimId(''); }} disabled={!generationDetails || generationDetails.variants.length === 0}>
                <option value="">Оберіть варіант покоління</option>
                {generationDetails?.variants.map((variant) => (
                  <option key={variant.id} value={variant.id}>{variant.name} • {variant.bodyStyleName}</option>
                ))}
              </select>
            </label>
            <label className="admin-field"><span>Назва комплектації</span><input value={trimName} onChange={(event) => setTrimName(event.target.value)} /></label>
            <label className="admin-field"><span>Трансмісія</span><input value={trimTransmissionType} onChange={(event) => setTrimTransmissionType(event.target.value)} /></label>
            <label className="admin-field"><span>Двері</span><input type="number" value={trimDoorsCount} onChange={(event) => setTrimDoorsCount(event.target.value)} /></label>
            <label className="admin-field"><span>Кількість місць</span><input type="number" value={trimSeatsCount} onChange={(event) => setTrimSeatsCount(event.target.value)} /></label>
            <button type="submit" className="primary-cta" disabled={!variantId}>Додати комплектацію</button>
          </form>
        </section>

        <section className="admin-panel">
          <h2>Додати технічні характеристики</h2>
          <form onSubmit={handleCreateTechnical} className="admin-form-stack">
            <label className="admin-field">
              <span>Оберіть марку</span>
              <select value={brandId} onChange={(event) => { setBrandId(event.target.value); resetModelScope(); }}>
                <option value="">Оберіть марку</option>
                {brands.map((brand) => (
                  <option key={brand.id} value={brand.id}>{brand.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Оберіть модель</span>
              <select value={modelId} onChange={(event) => { setModelId(event.target.value); resetGenerationScope(); }} disabled={!brandId}>
                <option value="">Оберіть модель</option>
                {models.map((model) => (
                  <option key={model.id} value={model.id}>{model.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Оберіть покоління</span>
              <select value={generationId} onChange={(event) => { setGenerationId(event.target.value); setVariantId(''); setTrimId(''); }} disabled={!modelId}>
                <option value="">Оберіть покоління</option>
                {generations.map((generation) => (
                  <option key={generation.id} value={generation.id}>{generation.name} ({generation.yearFrom}-{generation.yearTo})</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Оберіть комплектацію</span>
              <select value={trimId} onChange={(event) => setTrimId(event.target.value)} disabled={!variantId || selectedTrims.length === 0}>
                <option value="">Оберіть комплектацію</option>
                {selectedTrims.map((trimItem) => (
                  <option key={trimItem.id} value={trimItem.id}>{trimItem.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field"><span>Тип палива</span><input value={fuelType} onChange={(event) => setFuelType(event.target.value)} /></label>
            <label className="admin-field"><span>Потужність (к.с.)</span><input type="number" value={power} onChange={(event) => setPower(event.target.value)} /></label>
            <label className="admin-field"><span>Привід</span><input value={driveType} onChange={(event) => setDriveType(event.target.value)} /></label>
            <button type="submit" className="primary-cta" disabled={!trimId}>Додати технічні характеристики</button>
          </form>
        </section>

        <section className="admin-panel">
          <h2>Наявний вибір у каталозі</h2>
          <p className="muted-note">{activePathLabel || 'Оберіть марку, модель, покоління або комплектацію.'}</p>
          {generationDetails ? (
            <div className="admin-selection-summary">
              <strong>{generationDetails.brand.name}</strong>
              <span>{generationDetails.model.name}</span>
              <span>{generationDetails.displayName}</span>
            </div>
          ) : null}
        </section>

        {message && <p className="status-pill admin-status-pill">{message}</p>}
        {error && <p className="error-text">{error}</p>}
      </main>

      <SiteFooter />
    </div>
  )
}
