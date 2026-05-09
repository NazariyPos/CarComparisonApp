import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { SiteFooter } from '../components/SiteFooter'
import { SiteHeader } from '../components/SiteHeader'
import { useAuth } from '../context/AuthContext'
import {
  type BodyStyleOptionDto,
  type BrandBasicDto,
  type GenerationWithTrimsDto,
  type GenerationVariantDto,
  type ModelDto,
  createBrand,
  createGenerationVariant,
  createModel,
  createTechnicalDetails,
  createTrim,
  getBrands,
  getGenerationVariantWithTrims,
  getGenerationVariantsByModel,
  getAdminBodyStyles,
  getModelsByBrand,
} from '../services/carApi'

// Дозволені значення для контролю якості даних
const BODY_TYPES = ['Седан', 'Купе', 'Універсал', 'Хетчбек', 'Позашляховик', 'Мінівен', 'Кабріолет'] as const
const FUEL_TYPES = ['Бензин', 'Дизель', 'Гібрид', 'Електро', 'LPG'] as const
const DRIVE_TYPES = ['FWD', 'RWD', 'AWD', '4WD'] as const
const VARIANT_TYPES = [
  { value: 'Standard', label: 'Standard' },
  { value: 'PreFacelift', label: 'PreFacelift' },
  { value: 'Facelift', label: 'Facelift' },
] as const

const VARIANT_PHASE_SUFFIXES: Record<string, string> = {
  PreFacelift: ' (дорестайлінг)',
  Facelift: ' (рестайлінг)',
}

export function AdminCatalogPage() {
  const { currentUser, isAuthenticated, isAuthReady } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const [brands, setBrands] = useState<BrandBasicDto[]>([])
  const [bodyStyles, setBodyStyles] = useState<BodyStyleOptionDto[]>([])
  const [models, setModels] = useState<ModelDto[]>([])
  const [generationVariants, setGenerationVariants] = useState<GenerationVariantDto[]>([])
  const [generationDetails, setGenerationDetails] = useState<GenerationWithTrimsDto | null>(null)

  const [brandId, setBrandId] = useState('')
  const [modelId, setModelId] = useState('')
  const [variantId, setVariantId] = useState('')
  const [trimId, setTrimId] = useState('')

  const [brandName, setBrandName] = useState('')
  const [modelName, setModelName] = useState('')
  const [modelBodyType, setModelBodyType] = useState('')
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
  const [showTechnicalForm, setShowTechnicalForm] = useState(false)
  const [maxSpeed, setMaxSpeed] = useState('')
  const [acceleration0To100, setAcceleration0To100] = useState('')
  const [engineCode, setEngineCode] = useState('')
  const [engineType, setEngineType] = useState('')
  const [cylindersCount, setCylindersCount] = useState('')
  const [valvesCount, setValvesCount] = useState('')
  const [compressionRatio, setCompressionRatio] = useState('')
  const [torque, setTorque] = useState('')
  const [maxPowerAtRPM, setMaxPowerAtRPM] = useState('')
  const [maxTorqueAtRPM, setMaxTorqueAtRPM] = useState('')
  const [engineDisplacement, setEngineDisplacement] = useState('')
  const [fuelConsumptionCity, setFuelConsumptionCity] = useState('')
  const [fuelConsumptionMixed, setFuelConsumptionMixed] = useState('')
  const [fuelConsumptionHighway, setFuelConsumptionHighway] = useState('')
  const [electricRange, setElectricRange] = useState('')
  const [length, setLength] = useState('')
  const [width, setWidth] = useState('')
  const [height, setHeight] = useState('')
  const [wheelbase, setWheelbase] = useState('')
  const [frontTrack, setFrontTrack] = useState('')
  const [rearTrack, setRearTrack] = useState('')
  const [curbWeight, setCurbWeight] = useState('')
  const [grossWeight, setGrossWeight] = useState('')
  const [fuelTankCapacity, setFuelTankCapacity] = useState('')
  const [turningCircle, setTurningCircle] = useState('')
  const [frontBrakes, setFrontBrakes] = useState('')
  const [rearBrakes, setRearBrakes] = useState('')
  const [frontSuspension, setFrontSuspension] = useState('')
  const [rearSuspension, setRearSuspension] = useState('')

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
        const styles = await getAdminBodyStyles()
        setBodyStyles(styles)
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
        setGenerationVariants([])
        setGenerationDetails(null)
        setVariantId('')
        setTrimId('')
        return
      }

      const variantData = await getGenerationVariantsByModel(Number.parseInt(modelId, 10))
      setGenerationVariants(variantData)
    }

    void loadModelScope()
  }, [modelId])

  useEffect(() => {
    async function loadGenerationScope() {
      if (!variantId) {
        setGenerationDetails(null)
        setTrimId('')
        return
      }

      const details = await getGenerationVariantWithTrims(Number.parseInt(variantId, 10))
      setGenerationDetails(details)

      setTrimId('')
    }

    void loadGenerationScope()
  }, [variantId])

  const selectedBrand = useMemo(() => brands.find((item) => String(item.id) === brandId), [brandId, brands])
  const selectedModel = useMemo(() => models.find((item) => String(item.id) === modelId), [modelId, models])
  const selectedVariant = useMemo(
    () => generationVariants.find((item) => String(item.id) === variantId) ?? null,
    [generationVariants, variantId],
  )
  const selectedTrims = useMemo(() => {
    if (!generationDetails) return []
    return generationDetails.trims
  }, [generationDetails])
  const selectedTrim = useMemo(
    () => selectedTrims.find((item) => String(item.id) === trimId) ?? null,
    [selectedTrims, trimId],
  )

  const activePathLabel = useMemo(
    () => [selectedBrand?.name, selectedModel?.name, selectedVariant?.name]
      .filter(Boolean)
      .join(' / '),
    [selectedBrand?.name, selectedModel?.name, selectedVariant?.name],
  )

  const resetModelScope = () => {
    setModelId('')
    setVariantId('')
    setTrimId('')
    setModels([])
    setGenerationVariants([])
    setGenerationDetails(null)
  }

  const resetVariantScope = () => {
    setVariantId('')
    setTrimId('')
    setGenerationVariants([])
    setGenerationDetails(null)
  }

  const normalizeVariantName = (name: string, phase: string) => {
    const trimmedName = name.trim()
    const suffix = VARIANT_PHASE_SUFFIXES[phase]

    if (!trimmedName) {
      return ''
    }

    if (!suffix) {
      return trimmedName
    }

    const lowerName = trimmedName.toLowerCase()
    if (lowerName.includes('дорестайлінг') || lowerName.includes('рестайлінг')) {
      return trimmedName
    }

    return `${trimmedName}${suffix}`
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

  const handleCreateVariant = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage(null)
    setError(null)

    if (!modelId) {
      setError('Оберіть марку і модель')
      return
    }

    const normalizedVariantName = normalizeVariantName(variantName, variantType)

    const bodyStyleId = Number.parseInt(variantBodyStyleId, 10)
    const created = await createGenerationVariant(Number.parseInt(modelId, 10), {
      name: normalizedVariantName,
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

    if (created.error) {
      setError(created.error)
      return
    }

    setVariantName('')
    setVariantType('Standard')
    setVariantBodyStyleId('')
    setVariantDoorsCount('')
    setVariantYearFrom('')
    setVariantYearTo('')
    setMessage(`Модифікацію створено: ${created.name}`)

    const variants = await getGenerationVariantsByModel(Number.parseInt(modelId, 10))
    setGenerationVariants(variants)
  }

  const handleCreateTrim = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage(null)
    setError(null)

    if (!variantId) {
      setError('Оберіть марку, модель і варіант')
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

    const details = await getGenerationVariantWithTrims(Number.parseInt(variantId, 10))
    setGenerationDetails(details)
  }

  const handleCreateTechnical = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault()
    setMessage(null)
    setError(null)

    if (!trimId) {
      setError('Оберіть марку, модель, варіант і комплектацію')
      return
    }

    const created = await createTechnicalDetails(Number.parseInt(trimId, 10), {
      maxSpeed: maxSpeed ? Number.parseInt(maxSpeed, 10) : undefined,
      acceleration0To100: acceleration0To100 ? Number.parseFloat(acceleration0To100) : undefined,
      engineCode: engineCode || undefined,
      engineType: engineType || undefined,
      cylindersCount: cylindersCount ? Number.parseInt(cylindersCount, 10) : undefined,
      valvesCount: valvesCount ? Number.parseInt(valvesCount, 10) : undefined,
      compressionRatio: compressionRatio ? Number.parseFloat(compressionRatio) : undefined,
      fuelType: fuelType || undefined,
      power: power ? Number.parseInt(power, 10) : undefined,
      torque: torque ? Number.parseInt(torque, 10) : undefined,
      maxPowerAtRPM: maxPowerAtRPM ? Number.parseInt(maxPowerAtRPM, 10) : undefined,
      maxTorqueAtRPM: maxTorqueAtRPM ? Number.parseInt(maxTorqueAtRPM, 10) : undefined,
      engineDisplacement: engineDisplacement ? Number.parseFloat(engineDisplacement) : undefined,
      driveType: driveType || undefined,
      fuelConsumptionCity: fuelConsumptionCity ? Number.parseFloat(fuelConsumptionCity) : undefined,
      fuelConsumptionMixed: fuelConsumptionMixed ? Number.parseFloat(fuelConsumptionMixed) : undefined,
      fuelConsumptionHighway: fuelConsumptionHighway ? Number.parseFloat(fuelConsumptionHighway) : undefined,
      electricRange: electricRange ? Number.parseFloat(electricRange) : undefined,
      length: length ? Number.parseFloat(length) : undefined,
      width: width ? Number.parseFloat(width) : undefined,
      height: height ? Number.parseFloat(height) : undefined,
      wheelbase: wheelbase ? Number.parseFloat(wheelbase) : undefined,
      frontTrack: frontTrack ? Number.parseFloat(frontTrack) : undefined,
      rearTrack: rearTrack ? Number.parseFloat(rearTrack) : undefined,
      curbWeight: curbWeight ? Number.parseFloat(curbWeight) : undefined,
      grossWeight: grossWeight ? Number.parseFloat(grossWeight) : undefined,
      fuelTankCapacity: fuelTankCapacity ? Number.parseFloat(fuelTankCapacity) : undefined,
      turningCircle: turningCircle ? Number.parseFloat(turningCircle) : undefined,
      frontBrakes: frontBrakes || undefined,
      rearBrakes: rearBrakes || undefined,
      frontSuspension: frontSuspension || undefined,
      rearSuspension: rearSuspension || undefined,
    })

    if (!created) {
      setError('Не вдалося додати або оновити технічні характеристики')
      return
    }

    setFuelType('')
    setPower('')
    setDriveType('')
  setMaxSpeed('')
  setAcceleration0To100('')
  setEngineCode('')
  setEngineType('')
  setCylindersCount('')
  setValvesCount('')
  setCompressionRatio('')
  setTorque('')
  setMaxPowerAtRPM('')
  setMaxTorqueAtRPM('')
  setEngineDisplacement('')
  setFuelConsumptionCity('')
  setFuelConsumptionMixed('')
  setFuelConsumptionHighway('')
  setElectricRange('')
  setLength('')
  setWidth('')
  setHeight('')
  setWheelbase('')
  setFrontTrack('')
  setRearTrack('')
  setCurbWeight('')
  setGrossWeight('')
  setFuelTankCapacity('')
  setTurningCircle('')
  setFrontBrakes('')
  setRearBrakes('')
  setFrontSuspension('')
  setRearSuspension('')
    setMessage('Технічні характеристики збережено')

    const details = await getGenerationVariantWithTrims(Number.parseInt(variantId, 10))
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
              <select value={modelBodyType} onChange={(event) => setModelBodyType(event.target.value)}>
                <option value="">Оберіть тип кузова</option>
                {BODY_TYPES.map((type) => (
                  <option key={type} value={type}>{type}</option>
                ))}
              </select>
            </label>
            <button type="submit" className="primary-cta" disabled={!brandId}>Додати модель</button>
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
              <select value={modelId} onChange={(event) => { setModelId(event.target.value); resetVariantScope(); }} disabled={!brandId}>
                <option value="">Оберіть модель</option>
                {models.map((model) => (
                  <option key={model.id} value={model.id}>{model.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Назва варіанта</span>
              <input value={variantName} onChange={(event) => setVariantName(event.target.value)} />
            </label>
            <label className="admin-field">
              <span>Фаза варіанту</span>
              <select
                value={variantType}
                onChange={(event) => {
                  const nextVariantType = event.target.value
                  setVariantType(nextVariantType)
                  setVariantName((currentName) => normalizeVariantName(currentName, nextVariantType))
                }}
              >
                {VARIANT_TYPES.map((type) => (
                  <option key={type.value} value={type.value}>{type.label}</option>
                ))}
              </select>
              <small className="muted-note">Standard уже вибраний за замовчуванням. Якщо немає рестайлінгу, лишайте це значення.</small>
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
            <button type="submit" className="primary-cta" disabled={!modelId || bodyStyles.length === 0}>Додати варіант</button>
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
              <select value={modelId} onChange={(event) => { setModelId(event.target.value); resetVariantScope(); }} disabled={!brandId}>
                <option value="">Оберіть модель</option>
                {models.map((model) => (
                  <option key={model.id} value={model.id}>{model.name}</option>
                ))}
              </select>
            </label>
            <label className="admin-field">
              <span>Оберіть варіант</span>
              <select value={variantId} onChange={(event) => { setVariantId(event.target.value); setTrimId(''); }} disabled={!modelId || generationVariants.length === 0}>
                <option value="">Оберіть варіант</option>
                {generationVariants.map((variant) => (
                  <option key={variant.id} value={variant.id}>{variant.name} ({variant.yearFrom}-{variant.yearTo})</option>
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
          <div className="admin-form-stack">
            <p className="muted-note">Натисніть, щоб розгорнути форму вниз і заповнити потрібні характеристики.</p>
            <button
              type="button"
              className="primary-cta admin-expander-toggle"
              onClick={() => setShowTechnicalForm((current) => !current)}
              aria-expanded={showTechnicalForm}
              aria-controls="technical-details-form"
            >
              {showTechnicalForm ? 'Сховати форму технічних характеристик' : 'Відкрити форму технічних характеристик'}
            </button>
          </div>

          <div className={`admin-expander-panel ${showTechnicalForm ? 'admin-expander-panel-open' : ''}`}>
            <div className="admin-expander-panel-inner" id="technical-details-form">
              <form onSubmit={handleCreateTechnical} className="admin-form-stack">
                <div className="admin-form-grid">
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
                    <select value={modelId} onChange={(event) => { setModelId(event.target.value); resetVariantScope(); }} disabled={!brandId}>
                      <option value="">Оберіть модель</option>
                      {models.map((model) => (
                        <option key={model.id} value={model.id}>{model.name}</option>
                      ))}
                    </select>
                  </label>
                  <label className="admin-field">
                    <span>Оберіть варіант</span>
                    <select value={variantId} onChange={(event) => { setVariantId(event.target.value); setTrimId(''); }} disabled={!modelId || generationVariants.length === 0}>
                      <option value="">Оберіть варіант</option>
                      {generationVariants.map((variant) => (
                        <option key={variant.id} value={variant.id}>{variant.name} ({variant.yearFrom}-{variant.yearTo})</option>
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
                </div>

                <div className="admin-form-divider">
                  <h3>Дані двигуна</h3>
                </div>
                <div className="admin-form-grid">
                  <label className="admin-field"><span>Максимальна швидкість</span><input type="number" value={maxSpeed} onChange={(event) => setMaxSpeed(event.target.value)} /></label>
                  <label className="admin-field"><span>Розгін 0-100</span><input type="number" step="0.1" value={acceleration0To100} onChange={(event) => setAcceleration0To100(event.target.value)} /></label>
                  <label className="admin-field"><span>Код двигуна</span><input value={engineCode} onChange={(event) => setEngineCode(event.target.value)} /></label>
                  <label className="admin-field"><span>Тип двигуна</span><input value={engineType} onChange={(event) => setEngineType(event.target.value)} /></label>
                  <label className="admin-field"><span>Кількість циліндрів</span><input type="number" value={cylindersCount} onChange={(event) => setCylindersCount(event.target.value)} /></label>
                  <label className="admin-field"><span>Кількість клапанів</span><input type="number" value={valvesCount} onChange={(event) => setValvesCount(event.target.value)} /></label>
                  <label className="admin-field"><span>Ступінь стиснення</span><input type="number" step="0.1" value={compressionRatio} onChange={(event) => setCompressionRatio(event.target.value)} /></label>
                  <label className="admin-field">
                    <span>Тип палива</span>
                    <select value={fuelType} onChange={(event) => setFuelType(event.target.value)}>
                      <option value="">Оберіть тип палива</option>
                      {FUEL_TYPES.map((type) => (
                        <option key={type} value={type}>{type}</option>
                      ))}
                    </select>
                  </label>
                  <label className="admin-field"><span>Потужність (к.с.)</span><input type="number" value={power} onChange={(event) => setPower(event.target.value)} /></label>
                  <label className="admin-field"><span>Крутний момент</span><input type="number" value={torque} onChange={(event) => setTorque(event.target.value)} /></label>
                  <label className="admin-field"><span>Макс. потужність при RPM</span><input type="number" value={maxPowerAtRPM} onChange={(event) => setMaxPowerAtRPM(event.target.value)} /></label>
                  <label className="admin-field"><span>Макс. момент при RPM</span><input type="number" value={maxTorqueAtRPM} onChange={(event) => setMaxTorqueAtRPM(event.target.value)} /></label>
                  <label className="admin-field"><span>Робочий об'єм двигуна</span><input type="number" step="0.1" value={engineDisplacement} onChange={(event) => setEngineDisplacement(event.target.value)} /></label>
                  <label className="admin-field">
                    <span>Привід</span>
                    <select value={driveType} onChange={(event) => setDriveType(event.target.value)}>
                      <option value="">Оберіть привід</option>
                      {DRIVE_TYPES.map((type) => (
                        <option key={type} value={type}>{type}</option>
                      ))}
                    </select>
                  </label>
                </div>

                <div className="admin-form-divider">
                  <h3>Витрата та запас ходу</h3>
                </div>
                <div className="admin-form-grid">
                  <label className="admin-field"><span>Витрата в місті</span><input type="number" step="0.1" value={fuelConsumptionCity} onChange={(event) => setFuelConsumptionCity(event.target.value)} /></label>
                  <label className="admin-field"><span>Витрата змішана</span><input type="number" step="0.1" value={fuelConsumptionMixed} onChange={(event) => setFuelConsumptionMixed(event.target.value)} /></label>
                  <label className="admin-field"><span>Витрата на трасі</span><input type="number" step="0.1" value={fuelConsumptionHighway} onChange={(event) => setFuelConsumptionHighway(event.target.value)} /></label>
                  <label className="admin-field"><span>Електричний запас ходу</span><input type="number" value={electricRange} onChange={(event) => setElectricRange(event.target.value)} /></label>
                </div>

                <div className="admin-form-divider">
                  <h3>Габарити та маса</h3>
                </div>
                <div className="admin-form-grid">
                  <label className="admin-field"><span>Довжина</span><input type="number" value={length} onChange={(event) => setLength(event.target.value)} /></label>
                  <label className="admin-field"><span>Ширина</span><input type="number" value={width} onChange={(event) => setWidth(event.target.value)} /></label>
                  <label className="admin-field"><span>Висота</span><input type="number" value={height} onChange={(event) => setHeight(event.target.value)} /></label>
                  <label className="admin-field"><span>Колісна база</span><input type="number" value={wheelbase} onChange={(event) => setWheelbase(event.target.value)} /></label>
                  <label className="admin-field"><span>Передня колія</span><input type="number" value={frontTrack} onChange={(event) => setFrontTrack(event.target.value)} /></label>
                  <label className="admin-field"><span>Задня колія</span><input type="number" value={rearTrack} onChange={(event) => setRearTrack(event.target.value)} /></label>
                  <label className="admin-field"><span>Споряджена маса</span><input type="number" value={curbWeight} onChange={(event) => setCurbWeight(event.target.value)} /></label>
                  <label className="admin-field"><span>Повна маса</span><input type="number" value={grossWeight} onChange={(event) => setGrossWeight(event.target.value)} /></label>
                  <label className="admin-field"><span>Паливний бак</span><input type="number" value={fuelTankCapacity} onChange={(event) => setFuelTankCapacity(event.target.value)} /></label>
                  <label className="admin-field"><span>Радіус розвороту</span><input type="number" step="0.1" value={turningCircle} onChange={(event) => setTurningCircle(event.target.value)} /></label>
                </div>

                <div className="admin-form-divider">
                  <h3>Гальма та підвіска</h3>
                </div>
                <div className="admin-form-grid">
                  <label className="admin-field"><span>Передні гальма</span><input value={frontBrakes} onChange={(event) => setFrontBrakes(event.target.value)} /></label>
                  <label className="admin-field"><span>Задні гальма</span><input value={rearBrakes} onChange={(event) => setRearBrakes(event.target.value)} /></label>
                  <label className="admin-field"><span>Передня підвіска</span><input value={frontSuspension} onChange={(event) => setFrontSuspension(event.target.value)} /></label>
                  <label className="admin-field"><span>Задня підвіска</span><input value={rearSuspension} onChange={(event) => setRearSuspension(event.target.value)} /></label>
                </div>

                <button type="submit" className="primary-cta" disabled={!trimId}>Додати технічні характеристики</button>
              </form>
            </div>
          </div>
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
