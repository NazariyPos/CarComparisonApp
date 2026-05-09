import { useEffect, useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { SiteFooter } from '../components/SiteFooter'
import { SiteHeader } from '../components/SiteHeader'
import {
  getBrands,
  getGenerationVariantsByModel,
  getSearchFacets,
  getModelsByBrand,
  searchGenerations,
  getCarImagesFromExternal,
  type BodyStyleOptionDto,
  type BrandBasicDto,
  type GenerationVariantDto,
  type GenerationCardDto,
  type ModelDto,
} from '../services/carApi.ts'

export function BrandsPage() {
  const [brandId, setBrandId] = useState('')
  const [modelId, setModelId] = useState('')
  const [generationVariantId, setGenerationVariantId] = useState('')
  const [transmission, setTransmission] = useState('')
  const [bodyStyleId, setBodyStyleId] = useState('')
  const [fuelType, setFuelType] = useState('')
  const [minYear, setMinYear] = useState('')
  const [maxYear, setMaxYear] = useState('')

  const [brands, setBrands] = useState<BrandBasicDto[]>([])
  const [models, setModels] = useState<ModelDto[]>([])
  const [generationVariants, setGenerationVariants] = useState<GenerationVariantDto[]>([])
  const [results, setResults] = useState<GenerationCardDto[]>([])
  const [bodyStyleOptions, setBodyStyleOptions] = useState<BodyStyleOptionDto[]>([])
  const [transmissionOptions, setTransmissionOptions] = useState<string[]>([])
  const [fuelTypeOptions, setFuelTypeOptions] = useState<string[]>([])
  const [isFiltersLoading, setIsFiltersLoading] = useState(false)
  const [externalImages, setExternalImages] = useState<Map<string, string>>(new Map())

  const [hasSearch, setHasSearch] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function loadBrands() {
      setIsLoading(true)
      setError(null)

      try {
        const data = await getBrands()
        setBrands(data)
      } catch {
        setError('Failed to load brands from CarComparisonApi')
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
        setModelId('')
        setGenerationVariantId('')
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
    async function loadGenerations() {
      if (!modelId) {
        setGenerationVariantId('')
        setGenerationVariants([])
        return
      }

      try {
        const data = await getGenerationVariantsByModel(Number.parseInt(modelId, 10))
        setGenerationVariants(data)
      } catch {
        setGenerationVariants([])
      }
    }

    void loadGenerations()
  }, [modelId])

  useEffect(() => {
    async function loadFilterOptions() {
      if (brands.length === 0) {
        return
      }

      setIsFiltersLoading(true)

      try {
        // For facets, we need to pass generationId if a variant is selected
        const generationIdForFacets = generationVariantId
          ? generationVariants.find((v) => String(v.id) === generationVariantId)?.generationId || undefined
          : undefined

        const facets = await getSearchFacets({
          brandId: brandId ? Number.parseInt(brandId, 10) : undefined,
          modelId: modelId ? Number.parseInt(modelId, 10) : undefined,
          generationId: generationIdForFacets,
          minYear: minYear ? Number.parseInt(minYear, 10) : undefined,
          maxYear: maxYear ? Number.parseInt(maxYear, 10) : undefined,
        })

        setBodyStyleOptions(facets.bodyStyles)
        setTransmissionOptions(facets.transmissionTypes)
        setFuelTypeOptions(facets.fuelTypes)
      } finally {
        setIsFiltersLoading(false)
      }
    }

    void loadFilterOptions()
  }, [
    brands,
    brandId,
    modelId,
    models,
    generationVariantId,
    generationVariants,
    minYear,
    maxYear,
  ])

  useEffect(() => {
    if (bodyStyleId && !bodyStyleOptions.some((option) => String(option.id) === bodyStyleId)) {
      setBodyStyleId('')
    }

    if (transmission && !transmissionOptions.includes(transmission)) {
      setTransmission('')
    }

    if (fuelType && !fuelTypeOptions.includes(fuelType)) {
      setFuelType('')
    }
  }, [bodyStyleId, transmission, fuelType, bodyStyleOptions, transmissionOptions, fuelTypeOptions])

  const popularCards = useMemo(() => {
    if (results.length > 0) {
      return results.slice(0, 4)
    }

    return [
      {
        generationId: 0,
        generationName: 'Дані скоро',
        yearFrom: 0,
        yearTo: 0,
        modelId: 0,
        modelName: 'Популярні моделі',
        bodyType: 'Седан',
        brandId: 0,
        brandName: 'CarDD',
        trimCount: 0,
      },
      {
        generationId: 1,
        generationName: 'Дані скоро',
        yearFrom: 0,
        yearTo: 0,
        modelId: 1,
        modelName: 'Популярні моделі',
        bodyType: 'Хетчбек',
        brandId: 1,
        brandName: 'CarDD',
        trimCount: 0,
      },
      {
        generationId: 2,
        generationName: 'Дані скоро',
        yearFrom: 0,
        yearTo: 0,
        modelId: 2,
        modelName: 'Популярні моделі',
        bodyType: 'Купе',
        brandId: 2,
        brandName: 'CarDD',
        trimCount: 0,
      },
      {
        generationId: 3,
        generationName: 'Дані скоро',
        yearFrom: 0,
        yearTo: 0,
        modelId: 3,
        modelName: 'Популярні моделі',
        bodyType: 'Кросовер',
        brandId: 3,
        brandName: 'CarDD',
        trimCount: 0,
      },
    ]
  }, [results])

  useEffect(() => {
    if (results.length === 0) {
      setExternalImages(new Map())
      return
    }

    const loadExternalImages = async () => {
      const imageMap = new Map<string, string>()

      // Get unique brand+model combinations
      const uniquePairs = new Map<string, { brand: string; model: string; year?: number }>()
      results.forEach((item) => {
        const year = item.yearFrom > 0 ? item.yearFrom : undefined
        const key = `${item.brandName}-${item.modelName}-${year ?? 'any'}`
        if (!uniquePairs.has(key)) {
          uniquePairs.set(key, { brand: item.brandName, model: item.modelName, year })
        }
      })

      // Load images for each unique pair
      for (const { brand, model, year } of uniquePairs.values()) {
        try {
          const images = await getCarImagesFromExternal(brand, model, year)
          if (images.length > 0) {
            const key = `${brand}-${model}-${year ?? 'any'}`
            imageMap.set(key, images[0].url)
          }
        } catch {
          // Gracefully handle API errors
        }
      }

      setExternalImages(imageMap)
    }

    void loadExternalImages()
  }, [results])

  const handleSearch = async () => {
    setIsLoading(true)
    setHasSearch(true)
    setError(null)

    try {
      const data = await searchGenerations({
        brandId: brandId ? Number.parseInt(brandId, 10) : undefined,
        modelId: modelId ? Number.parseInt(modelId, 10) : undefined,
        generationVariantId: generationVariantId
          ? Number.parseInt(generationVariantId, 10)
          : undefined,
        transmission: transmission || undefined,
        bodyStyleId: bodyStyleId ? Number.parseInt(bodyStyleId, 10) : undefined,
        fuelType: fuelType || undefined,
        minYear: minYear ? Number.parseInt(minYear, 10) : undefined,
        maxYear: maxYear ? Number.parseInt(maxYear, 10) : undefined,
      })
      setResults(data)
    } catch {
      setResults([])
      setError('Пошук не дав результату або сервер тимчасово недоступний.')
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="catalog-page">
      <div className="top-band" />
      <SiteHeader />

      <main className="catalog-main">
        <section className="search-panel">
          <h2>Пошук авто</h2>

          <div className="search-grid">
            <select
              value={brandId}
              onChange={(event) => {
                setBrandId(event.target.value)
                setModelId('')
                setGenerationVariantId('')
              }}
            >
              <option value="">Марка</option>
              {brands.map((brand) => (
                <option key={brand.id} value={brand.id}>
                  {brand.name}
                </option>
              ))}
            </select>

            <select
              value={modelId}
              onChange={(event) => {
                setModelId(event.target.value)
                setGenerationVariantId('')
              }}
              disabled={!brandId}
            >
              <option value="">Модель</option>
              {models.map((model) => (
                <option key={model.id} value={model.id}>
                  {model.name}
                </option>
              ))}
            </select>

            <select
              value={generationVariantId}
              onChange={(event) => setGenerationVariantId(event.target.value)}
              disabled={!modelId || generationVariants.length === 0}
            >
              <option value="">Покоління / Рестайлінг / Кузов</option>
              {generationVariants.map((variant) => (
                <option key={variant.id} value={variant.id}>
                  {variant.name} — {variant.yearFrom}–{variant.yearTo}, {variant.bodyStyleName}
                </option>
              ))}
            </select>

            <select
              value={transmission}
              onChange={(event) => setTransmission(event.target.value)}
              disabled={isFiltersLoading || transmissionOptions.length === 0}
            >
              <option value="">Коробка передач</option>
              {transmissionOptions.map((value) => (
                <option key={value} value={value}>
                  {value}
                </option>
              ))}
            </select>

            <input
              value={minYear}
              onChange={(event) => setMinYear(event.target.value)}
              placeholder="Рік випуску від"
            />

            <select
              value={bodyStyleId}
              onChange={(event) => setBodyStyleId(event.target.value)}
              disabled={isFiltersLoading || bodyStyleOptions.length === 0}
            >
              <option value="">Тип кузова (доп.фільтр)</option>
              {bodyStyleOptions.map((option) => (
                <option key={option.id} value={option.id}>
                  {option.name}
                </option>
              ))}
            </select>

            <select
              value={fuelType}
              onChange={(event) => setFuelType(event.target.value)}
              disabled={isFiltersLoading || fuelTypeOptions.length === 0}
            >
              <option value="">Пальне</option>
              {fuelTypeOptions.map((value) => (
                <option key={value} value={value}>
                  {value}
                </option>
              ))}
            </select>

            <input
              value={maxYear}
              onChange={(event) => setMaxYear(event.target.value)}
              placeholder="Рік випуску до"
            />
          </div>

          <button type="button" className="primary-cta" onClick={handleSearch}>
            {isLoading ? 'Шукаємо...' : 'Обрати авто'}
          </button>

          {isFiltersLoading && (
            <p className="muted-note">Оновлення доступних параметрів з бази...</p>
          )}

          {error && <p className="error-text">{error}</p>}
        </section>

        {hasSearch && (
          <section className="results-panel">
            <h3>Результати пошуку</h3>
            {results.length === 0 && !isLoading && (
              <p className="muted-note">Нічого не знайдено за вибраними параметрами.</p>
            )}

            {results.length > 0 && (
              <ul className="result-grid">
                {results.map((item) => {
                  const imageKey = `${item.brandName}-${item.modelName}-${item.yearFrom > 0 ? item.yearFrom : 'any'}`
                  const externalImageUrl = externalImages.get(
                    imageKey
                  )
                  const photoUrl = externalImageUrl || item.photoUrl

                  return (
                    <li
                      key={`${item.generationVariantId ?? item.generationId}-${item.modelId}`}
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
                      {item.displayGenerationName}
                      {' '}
                      ({item.yearFrom}-{item.yearTo})
                    </span>
                    <small>
                      {item.bodyType} • {item.trimCount} комплектацій
                    </small>
                    <Link
                      to={
                        item.generationVariantId
                          ? `/cars/variants/${item.generationVariantId}`
                          : `/cars/${item.generationId}`
                      }
                      className="result-card-link"
                    >
                      Переглянути сторінку авто
                    </Link>
                  </li>
                  )
                })}
              </ul>
            )}
          </section>
        )}

        <section className="results-panel">
          <h3>Найпопулярніші запити</h3>
          <p className="muted-note">
            Тимчасово відображаються демо-картки або перші результати пошуку.
          </p>

          <ul className="result-grid">
            {popularCards.map((item) => (
              <li key={`${item.brandId}-${item.modelId}-${item.generationId}`} className="result-card">
                <div className="placeholder-photo">Без фото</div>
                <strong>
                  {item.brandName} {item.modelName}
                </strong>
                <span>{item.generationName}</span>
                <small>{item.bodyType}</small>
              </li>
            ))}
          </ul>
        </section>
      </main>

      <SiteFooter />
    </div>
  )
}
