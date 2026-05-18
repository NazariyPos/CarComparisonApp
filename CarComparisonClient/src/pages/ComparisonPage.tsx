import { useEffect, useMemo, useState } from 'react'
import { SiteFooter } from '../components/SiteFooter'
import { SiteHeader } from '../components/SiteHeader'
import {
  compareTrims,
  getBrands,
  getGenerationVariantsByModel,
  getModelsByBrand,
  getTrimsByGenerationVariant,
  getTrimFullDetails,
  type BrandBasicDto,
  type GenerationVariantOptionDto,
  type ModelDto,
  type TrimDto,
} from '../services/carApi.ts'

type RawDto = Record<string, unknown>

interface TechnicalDetailsView {
  maxSpeed?: number
  acceleration0To100?: number
  engineCode?: string
  cylindersCount?: number
  valvesCount?: number
  compressionRatio?: number
  maxPowerAtRPM?: number
  maxTorqueAtRPM?: number
  engineDisplacement?: number
  fuelConsumptionMixed?: number
  fuelConsumptionCity?: number
  fuelConsumptionHighway?: number
  electricRange?: number
  length?: number
  width?: number
  height?: number
  wheelbase?: number
  frontTrack?: number
  rearTrack?: number
  curbWeight?: number
  grossWeight?: number
  fuelTankCapacity?: number
  turningCircle?: number
  frontBrakes?: string
  rearBrakes?: string
  frontSuspension?: string
  rearSuspension?: string
  torque?: number
  power?: number
  engineType?: string
  fuelType?: string
  driveType?: string
}

interface ComparisonTrimView {
  id: number
  name: string
  transmissionType?: string
  doorsCount?: number
  seatsCount?: number
  brandName?: string
  modelName?: string
  generationName?: string
  technicalDetails: TechnicalDetailsView
}

interface ComparisonResultView {
  trims: ComparisonTrimView[]
  highlights: Record<string, number[]>
}

type MetricTone = 'best' | 'worst'
type BetterDirection = 'higher' | 'lower'

interface ComparisonParameter {
  key: string
  label: string
  metricKey?: string
  betterDirection?: BetterDirection
  accessor: (trim: ComparisonTrimView) => unknown
  formatter: (value: unknown) => string
}

const numberFormatter = new Intl.NumberFormat('uk-UA', {
  maximumFractionDigits: 1,
})

const integerFormatter = new Intl.NumberFormat('uk-UA', {
  maximumFractionDigits: 0,
})

const isRecord = (value: unknown): value is RawDto => {
  return typeof value === 'object' && value !== null
}

const readValue = (raw: RawDto, camelKey: string, pascalKey: string): unknown => {
  return raw[camelKey] ?? raw[pascalKey]
}

const readString = (raw: RawDto, camelKey: string, pascalKey: string): string => {
  const value = readValue(raw, camelKey, pascalKey)
  return typeof value === 'string' ? value : ''
}

const readOptionalString = (
  raw: RawDto,
  camelKey: string,
  pascalKey: string,
): string | undefined => {
  const value = readValue(raw, camelKey, pascalKey)
  return typeof value === 'string' ? value : undefined
}

const readOptionalNumber = (
  raw: RawDto,
  camelKey: string,
  pascalKey: string,
): number | undefined => {
  const value = readValue(raw, camelKey, pascalKey)

  if (typeof value === 'number' && Number.isFinite(value)) {
    return value
  }

  if (typeof value === 'string') {
    const parsed = Number.parseFloat(value)
    if (Number.isFinite(parsed)) {
      return parsed
    }
  }

  return undefined
}

const readOptionalRecord = (
  raw: RawDto,
  camelKey: string,
  pascalKey: string,
): RawDto | undefined => {
  const value = readValue(raw, camelKey, pascalKey)
  return isRecord(value) ? value : undefined
}

const asRawDtoArray = (value: unknown): RawDto[] => {
  if (!Array.isArray(value)) {
    return []
  }

  return value.filter((item): item is RawDto => isRecord(item))
}

const asNumberArray = (value: unknown): number[] => {
  if (!Array.isArray(value)) {
    return []
  }

  return value.filter((item): item is number => typeof item === 'number')
}

const formatNumber = (value: unknown, unit: string, integer = false): string => {
  if (typeof value !== 'number' || !Number.isFinite(value)) {
    return '—'
  }

  const formatter = integer ? integerFormatter : numberFormatter
  const formatted = formatter.format(value)
  return unit ? `${formatted} ${unit}` : formatted
}

const formatText = (value: unknown): string => {
  return typeof value === 'string' && value.trim().length > 0 ? value : '—'
}

const comparisonParameters: ComparisonParameter[] = [
  {
    key: 'engineCode',
    label: 'Код двигуна',
    accessor: (trim) => trim.technicalDetails.engineCode,
    formatter: formatText,
  },
  {
    key: 'engineType',
    label: 'Тип двигуна',
    accessor: (trim) => trim.technicalDetails.engineType,
    formatter: formatText,
  },
  {
    key: 'cylindersCount',
    label: 'Кількість циліндрів',
    accessor: (trim) => trim.technicalDetails.cylindersCount,
    formatter: (value) => formatNumber(value, '', true),
  },
  {
    key: 'valvesCount',
    label: 'Кількість клапанів',
    accessor: (trim) => trim.technicalDetails.valvesCount,
    formatter: (value) => formatNumber(value, '', true),
  },
  {
    key: 'compressionRatio',
    label: 'Ступінь стиснення',
    accessor: (trim) => trim.technicalDetails.compressionRatio,
    formatter: (value) => formatNumber(value, ''),
  },
  {
    key: 'fuelType',
    label: 'Тип пального',
    accessor: (trim) => trim.technicalDetails.fuelType,
    formatter: formatText,
  },
  {
    key: 'power',
    label: 'Потужність',
    betterDirection: 'higher',
    accessor: (trim) => trim.technicalDetails.power,
    formatter: (value) => formatNumber(value, 'к.с.', true),
  },
  {
    key: 'torque',
    label: 'Крутний момент',
    metricKey: 'Torque',
    betterDirection: 'higher',
    accessor: (trim) => trim.technicalDetails.torque,
    formatter: (value) => formatNumber(value, 'Нм', true),
  },
  {
    key: 'maxPowerAtRPM',
    label: 'Максимальна потужність при об/хв',
    accessor: (trim) => trim.technicalDetails.maxPowerAtRPM,
    formatter: (value) => formatNumber(value, 'об/хв', true),
  },
  {
    key: 'maxTorqueAtRPM',
    label: 'Максимальний крутний момент при об/хв',
    accessor: (trim) => trim.technicalDetails.maxTorqueAtRPM,
    formatter: (value) => formatNumber(value, 'об/хв', true),
  },
  {
    key: 'engineDisplacement',
    label: "Об'єм двигуна",
    accessor: (trim) => trim.technicalDetails.engineDisplacement,
    formatter: (value) => formatNumber(value, 'л'),
  },
  {
    key: 'driveType',
    label: 'Привід',
    accessor: (trim) => trim.technicalDetails.driveType,
    formatter: formatText,
  },
  {
    key: 'transmissionType',
    label: 'Трансмісія',
    accessor: (trim) => trim.transmissionType,
    formatter: formatText,
  },
  {
    key: 'doorsCount',
    label: 'Кількість дверей',
    accessor: (trim) => trim.doorsCount,
    formatter: (value) => formatNumber(value, 'шт.', true),
  },
  {
    key: 'seatsCount',
    label: 'Кількість місць',
    accessor: (trim) => trim.seatsCount,
    formatter: (value) => formatNumber(value, 'шт.', true),
  },
  {
    key: 'maxSpeed',
    label: 'Максимальна швидкість',
    metricKey: 'MaxSpeed',
    betterDirection: 'higher',
    accessor: (trim) => trim.technicalDetails.maxSpeed,
    formatter: (value) => formatNumber(value, 'км/год', true),
  },
  {
    key: 'acceleration0To100',
    label: 'Розгін від 0 до 100 км/год',
    metricKey: 'Acceleration0To100',
    betterDirection: 'lower',
    accessor: (trim) => trim.technicalDetails.acceleration0To100,
    formatter: (value) => formatNumber(value, 'с'),
  },
  {
    key: 'fuelConsumptionMixed',
    label: 'Середній розхід палива',
    metricKey: 'FuelConsumption',
    betterDirection: 'lower',
    accessor: (trim) => trim.technicalDetails.fuelConsumptionMixed,
    formatter: (value) => formatNumber(value, 'л/100 км'),
  },
  {
    key: 'fuelConsumptionCity',
    label: 'Розхід палива (міський режим)',
    betterDirection: 'lower',
    accessor: (trim) => trim.technicalDetails.fuelConsumptionCity,
    formatter: (value) => formatNumber(value, 'л/100 км'),
  },
  {
    key: 'fuelConsumptionHighway',
    label: 'Розхід палива (заміський режим)',
    betterDirection: 'lower',
    accessor: (trim) => trim.technicalDetails.fuelConsumptionHighway,
    formatter: (value) => formatNumber(value, 'л/100 км'),
  },
  {
    key: 'electricRange',
    label: 'Запас ходу на електротязі',
    accessor: (trim) => trim.technicalDetails.electricRange,
    formatter: (value) => formatNumber(value, 'км'),
  },
  {
    key: 'length',
    label: 'Довжина',
    accessor: (trim) => trim.technicalDetails.length,
    formatter: (value) => formatNumber(value, 'мм', true),
  },
  {
    key: 'width',
    label: 'Ширина',
    accessor: (trim) => trim.technicalDetails.width,
    formatter: (value) => formatNumber(value, 'мм', true),
  },
  {
    key: 'height',
    label: 'Висота',
    accessor: (trim) => trim.technicalDetails.height,
    formatter: (value) => formatNumber(value, 'мм', true),
  },
  {
    key: 'wheelbase',
    label: 'Колісна база',
    accessor: (trim) => trim.technicalDetails.wheelbase,
    formatter: (value) => formatNumber(value, 'мм', true),
  },
  {
    key: 'frontTrack',
    label: 'Передня колія',
    accessor: (trim) => trim.technicalDetails.frontTrack,
    formatter: (value) => formatNumber(value, 'мм', true),
  },
  {
    key: 'rearTrack',
    label: 'Задня колія',
    accessor: (trim) => trim.technicalDetails.rearTrack,
    formatter: (value) => formatNumber(value, 'мм', true),
  },
  {
    key: 'curbWeight',
    label: 'Споряджена маса',
    accessor: (trim) => trim.technicalDetails.curbWeight,
    formatter: (value) => formatNumber(value, 'кг', true),
  },
  {
    key: 'grossWeight',
    label: 'Повна маса',
    accessor: (trim) => trim.technicalDetails.grossWeight,
    formatter: (value) => formatNumber(value, 'кг', true),
  },
  {
    key: 'fuelTankCapacity',
    label: 'Обʼєм паливного баку',
    accessor: (trim) => trim.technicalDetails.fuelTankCapacity,
    formatter: (value) => formatNumber(value, 'л'),
  },
  {
    key: 'turningCircle',
    label: 'Радіус розвороту',
    betterDirection: 'lower',
    accessor: (trim) => trim.technicalDetails.turningCircle,
    formatter: (value) => formatNumber(value, 'м'),
  },
  {
    key: 'frontBrakes',
    label: 'Передні гальма',
    accessor: (trim) => trim.technicalDetails.frontBrakes,
    formatter: formatText,
  },
  {
    key: 'rearBrakes',
    label: 'Задні гальма',
    accessor: (trim) => trim.technicalDetails.rearBrakes,
    formatter: formatText,
  },
  {
    key: 'frontSuspension',
    label: 'Передня підвіска',
    accessor: (trim) => trim.technicalDetails.frontSuspension,
    formatter: formatText,
  },
  {
    key: 'rearSuspension',
    label: 'Задня підвіска',
    accessor: (trim) => trim.technicalDetails.rearSuspension,
    formatter: formatText,
  },
]

const mapComparisonTrim = (rawTrim: RawDto): ComparisonTrimView => {
  const technicalDetailsRaw =
    readOptionalRecord(rawTrim, 'technicalDetails', 'TechnicalDetails') ?? {}

  const generationRaw = readOptionalRecord(rawTrim, 'generation', 'Generation')
  const modelRaw = generationRaw
    ? readOptionalRecord(generationRaw, 'model', 'Model')
    : undefined
  const brandRaw = modelRaw ? readOptionalRecord(modelRaw, 'brand', 'Brand') : undefined

  return {
    id: readOptionalNumber(rawTrim, 'id', 'Id') ?? 0,
    name: readString(rawTrim, 'name', 'Name'),
    transmissionType: readOptionalString(
      rawTrim,
      'transmissionType',
      'TransmissionType',
    ),
    doorsCount: readOptionalNumber(rawTrim, 'doorsCount', 'DoorsCount'),
    seatsCount: readOptionalNumber(rawTrim, 'seatsCount', 'SeatsCount'),
    brandName: brandRaw ? readOptionalString(brandRaw, 'name', 'Name') : undefined,
    modelName: modelRaw ? readOptionalString(modelRaw, 'name', 'Name') : undefined,
    generationName: generationRaw
      ? readOptionalString(generationRaw, 'name', 'Name')
      : undefined,
    technicalDetails: {
      maxSpeed: readOptionalNumber(technicalDetailsRaw, 'maxSpeed', 'MaxSpeed'),
      acceleration0To100: readOptionalNumber(
        technicalDetailsRaw,
        'acceleration0To100',
        'Acceleration0To100',
      ),
      engineCode: readOptionalString(technicalDetailsRaw, 'engineCode', 'EngineCode'),
      cylindersCount: readOptionalNumber(
        technicalDetailsRaw,
        'cylindersCount',
        'CylindersCount',
      ),
      valvesCount: readOptionalNumber(
        technicalDetailsRaw,
        'valvesCount',
        'ValvesCount',
      ),
      compressionRatio: readOptionalNumber(
        technicalDetailsRaw,
        'compressionRatio',
        'CompressionRatio',
      ),
      fuelConsumptionMixed: readOptionalNumber(
        technicalDetailsRaw,
        'fuelConsumptionMixed',
        'FuelConsumptionMixed',
      ),
      fuelConsumptionCity: readOptionalNumber(
        technicalDetailsRaw,
        'fuelConsumptionCity',
        'FuelConsumptionCity',
      ),
      fuelConsumptionHighway: readOptionalNumber(
        technicalDetailsRaw,
        'fuelConsumptionHighway',
        'FuelConsumptionHighway',
      ),
      electricRange: readOptionalNumber(
        technicalDetailsRaw,
        'electricRange',
        'ElectricRange',
      ),
      maxPowerAtRPM: readOptionalNumber(
        technicalDetailsRaw,
        'maxPowerAtRPM',
        'MaxPowerAtRPM',
      ),
      maxTorqueAtRPM: readOptionalNumber(
        technicalDetailsRaw,
        'maxTorqueAtRPM',
        'MaxTorqueAtRPM',
      ),
      engineDisplacement: readOptionalNumber(
        technicalDetailsRaw,
        'engineDisplacement',
        'EngineDisplacement',
      ),
      length: readOptionalNumber(technicalDetailsRaw, 'length', 'Length'),
      width: readOptionalNumber(technicalDetailsRaw, 'width', 'Width'),
      height: readOptionalNumber(technicalDetailsRaw, 'height', 'Height'),
      wheelbase: readOptionalNumber(technicalDetailsRaw, 'wheelbase', 'Wheelbase'),
      frontTrack: readOptionalNumber(technicalDetailsRaw, 'frontTrack', 'FrontTrack'),
      rearTrack: readOptionalNumber(technicalDetailsRaw, 'rearTrack', 'RearTrack'),
      curbWeight: readOptionalNumber(technicalDetailsRaw, 'curbWeight', 'CurbWeight'),
      grossWeight: readOptionalNumber(technicalDetailsRaw, 'grossWeight', 'GrossWeight'),
      fuelTankCapacity: readOptionalNumber(
        technicalDetailsRaw,
        'fuelTankCapacity',
        'FuelTankCapacity',
      ),
      turningCircle: readOptionalNumber(
        technicalDetailsRaw,
        'turningCircle',
        'TurningCircle',
      ),
      frontBrakes: readOptionalString(technicalDetailsRaw, 'frontBrakes', 'FrontBrakes'),
      rearBrakes: readOptionalString(technicalDetailsRaw, 'rearBrakes', 'RearBrakes'),
      frontSuspension: readOptionalString(
        technicalDetailsRaw,
        'frontSuspension',
        'FrontSuspension',
      ),
      rearSuspension: readOptionalString(
        technicalDetailsRaw,
        'rearSuspension',
        'RearSuspension',
      ),
      torque: readOptionalNumber(technicalDetailsRaw, 'torque', 'Torque'),
      power: readOptionalNumber(technicalDetailsRaw, 'power', 'Power'),
      fuelType: readOptionalString(technicalDetailsRaw, 'fuelType', 'FuelType'),
      engineType: readOptionalString(technicalDetailsRaw, 'engineType', 'EngineType'),
      driveType: readOptionalString(technicalDetailsRaw, 'driveType', 'DriveType'),
    },
  }
}

const parseComparisonResult = (raw: unknown): ComparisonResultView => {
  if (!isRecord(raw)) {
    return {
      trims: [],
      highlights: {},
    }
  }

  const trimsRaw = readValue(raw, 'trims', 'Trims')
  const highlightsRaw = readValue(raw, 'highlights', 'Highlights')

  const trims = asRawDtoArray(trimsRaw).map(mapComparisonTrim)
  const highlightsSource = isRecord(highlightsRaw) ? highlightsRaw : {}
  const highlights: Record<string, number[]> = {}

  Object.entries(highlightsSource).forEach(([key, value]) => {
    highlights[key] = asNumberArray(value)
  })

  return {
    trims,
    highlights,
  }
}

const sortTrimsByUserOrder = (
  trims: ComparisonTrimView[],
  selectedTrimIds: number[],
): ComparisonTrimView[] => {
  const orderMap = new Map<number, number>()

  selectedTrimIds.forEach((trimId, index) => {
    if (!orderMap.has(trimId)) {
      orderMap.set(trimId, index)
    }
  })

  return [...trims].sort((left, right) => {
    const leftIndex = orderMap.get(left.id)
    const rightIndex = orderMap.get(right.id)

    if (leftIndex === undefined && rightIndex === undefined) {
      return 0
    }

    if (leftIndex === undefined) {
      return 1
    }

    if (rightIndex === undefined) {
      return -1
    }

    return leftIndex - rightIndex
  })
}

const getMetricTone = (
  parameter: ComparisonParameter,
  trims: ComparisonTrimView[],
  trimIndex: number,
): MetricTone | undefined => {
  if (!parameter.betterDirection) {
    return undefined
  }

  const numericValues = trims
    .map((trim, index) => ({
      index,
      value: parameter.accessor(trim),
    }))
    .filter((entry): entry is { index: number; value: number } => {
      return typeof entry.value === 'number' && Number.isFinite(entry.value)
    })

  if (numericValues.length < 2) {
    return undefined
  }

  const values = numericValues.map((entry) => entry.value)
  const bestValue =
    parameter.betterDirection === 'higher' ? Math.max(...values) : Math.min(...values)
  const worstValue =
    parameter.betterDirection === 'higher' ? Math.min(...values) : Math.max(...values)

  const bestIndices = numericValues
    .filter((entry) => entry.value === bestValue)
    .map((entry) => entry.index)
  const worstIndices = numericValues
    .filter((entry) => entry.value === worstValue)
    .map((entry) => entry.index)

  const isBest = bestIndices.includes(trimIndex)
  const isWorst = worstIndices.includes(trimIndex)

  if (isBest && !isWorst) {
    return 'best'
  }

  if (isWorst && !isBest) {
    return 'worst'
  }

  return undefined
}

const formatTrimTitle = (trim: ComparisonTrimView): string => {
  const parts = [trim.brandName, trim.modelName, trim.generationName, trim.name].filter(
    (part): part is string => typeof part === 'string' && part.trim().length > 0,
  )

  return parts.length > 0 ? parts.join(' / ') : 'Невідома комплектація'
}

interface CompareSlotState {
  brandId: string
  modelId: string
  variantId: string
  trimId: string
  models: ModelDto[]
  variants: GenerationVariantOptionDto[]
  trims: TrimDto[]
}

const createEmptySlot = (): CompareSlotState => ({
  brandId: '',
  modelId: '',
  variantId: '',
  trimId: '',
  models: [],
  variants: [],
  trims: [],
})

export function ComparisonPage() {
  const [brands, setBrands] = useState<BrandBasicDto[]>([])
  const [slots, setSlots] = useState<CompareSlotState[]>(() =>
    Array.from({ length: 4 }, () => createEmptySlot()),
  )

  const [comparisonResult, setComparisonResult] =
    useState<ComparisonResultView | null>(null)
  const [comparisonPhotos, setComparisonPhotos] = useState<string[]>([])
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function loadBrands() {
      try {
        const data = await getBrands()
        setBrands(data)
      } catch {
        setBrands([])
      }
    }

    void loadBrands()
  }, [])

  const selectedTrimIds = useMemo(
    () =>
      slots
        .map((slot) => Number.parseInt(slot.trimId, 10))
        .filter((value) => Number.isFinite(value)),
    [slots],
  )

  const updateSlot = (index: number, updater: (slot: CompareSlotState) => CompareSlotState) => {
    setSlots((current) =>
      current.map((slot, slotIndex) =>
        slotIndex === index ? updater(slot) : slot,
      ),
    )
  }

  const handleBrandChange = async (index: number, value: string) => {
    updateSlot(index, () => ({
      ...createEmptySlot(),
      brandId: value,
    }))

    if (!value) {
      return
    }

    try {
      const models = await getModelsByBrand(Number.parseInt(value, 10))
      updateSlot(index, (slot) => ({ ...slot, models }))
    } catch {
      updateSlot(index, (slot) => ({ ...slot, models: [] }))
    }
  }

  const handleModelChange = async (index: number, value: string) => {
    updateSlot(index, (slot) => ({
      ...slot,
      modelId: value,
      variantId: '',
      trimId: '',
      variants: [],
      trims: [],
    }))

    if (!value) {
      return
    }

    try {
      const variants = await getGenerationVariantsByModel(Number.parseInt(value, 10))
      updateSlot(index, (slot) => ({ ...slot, variants }))
    } catch {
      updateSlot(index, (slot) => ({ ...slot, variants: [] }))
    }
  }

  const handleVariantChange = async (index: number, value: string) => {
    updateSlot(index, (slot) => ({
      ...slot,
      variantId: value,
      trimId: '',
      trims: [],
    }))

    if (!value) {
      return
    }

    try {
      const trims = await getTrimsByGenerationVariant(Number.parseInt(value, 10))
      updateSlot(index, (slot) => ({ ...slot, trims }))
    } catch {
      updateSlot(index, (slot) => ({ ...slot, trims: [] }))
    }
  }

  const handleCompare = async () => {
    if (selectedTrimIds.length < 2) {
      setError('Оберіть щонайменше 2 комплектації для порівняння.')
      setComparisonResult(null)
      return
    }

    setIsLoading(true)
    setError(null)

    try {
      const data = await compareTrims(selectedTrimIds)
      const parsedResult = parseComparisonResult(data)
      setComparisonResult({
        ...parsedResult,
        trims: sortTrimsByUserOrder(parsedResult.trims, selectedTrimIds),
      })
      // load photos for each selected trim
      try {
        const photoPromises = selectedTrimIds.map(async (trimId) => {
          try {
            const details = await getTrimFullDetails(trimId)
            // prefer generation photo if available
            return (details?.generation?.photoUrl as string) || ''
          } catch {
            return ''
          }
        })

        const photos = await Promise.all(photoPromises)
        setComparisonPhotos(photos)
      } catch {
        setComparisonPhotos([])
      }
    } catch {
      setError('Порівняння не виконано. Перевірте обрані дані або доступність API.')
      setComparisonResult(null)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="catalog-page">
      <div className="top-band" />
      <SiteHeader />

      <main className="catalog-main">
        <section className="comparison-panel">
          <h2>Порівняти авто</h2>
          <p className="muted-note">
            Оберіть до чотирьох авто і натисніть порівняти, щоб отримати результат.
          </p>

          <div className="comparison-grid">
            {slots.map((slot, index) => (
              <div key={`slot-${index}`} className="compare-slot">
                <select
                  value={slot.brandId}
                  onChange={(event) => void handleBrandChange(index, event.target.value)}
                >
                  <option value="">Марка</option>
                  {brands.map((brand) => (
                    <option key={brand.id} value={brand.id}>
                      {brand.name}
                    </option>
                  ))}
                </select>

                <select
                  value={slot.modelId}
                  onChange={(event) => void handleModelChange(index, event.target.value)}
                  disabled={!slot.brandId}
                >
                  <option value="">Модель</option>
                  {slot.models.map((model) => (
                    <option key={model.id} value={model.id}>
                      {model.name}
                    </option>
                  ))}
                </select>

                <select
                  value={slot.variantId}
                  onChange={(event) =>
                    void handleVariantChange(index, event.target.value)
                  }
                  disabled={!slot.modelId}
                >
                  <option value="">Варіант покоління</option>
                  {slot.variants.map((variant) => (
                    <option key={variant.id} value={variant.id}>
                      {variant.name}
                    </option>
                  ))}
                </select>

                <select
                  value={slot.trimId}
                  onChange={(event) =>
                    updateSlot(index, (current) => ({
                      ...current,
                      trimId: event.target.value,
                    }))
                  }
                  disabled={!slot.variantId}
                >
                  <option value="">Модифікація</option>
                  {slot.trims.map((trim) => (
                    <option key={trim.id} value={trim.id}>
                      {trim.name}
                    </option>
                  ))}
                </select>
              </div>
            ))}
          </div>

          <button
            type="button"
            className="primary-cta"
            disabled={isLoading || selectedTrimIds.length < 2}
            onClick={handleCompare}
          >
            {isLoading ? 'Порівнюємо...' : 'Порівняти'}
          </button>

          {error && <p className="error-text">{error}</p>}

          {comparisonResult && comparisonResult.trims.length > 0 && (
            <section className="results-panel comparison-results-panel">
              <h3>Порівняння комплектацій</h3>
              <div className="comparison-result-table-wrap">
                <table className="comparison-result-table">
                  <thead>
                    <tr>
                      <th scope="col">Характеристика</th>
                      {comparisonResult.trims.map((trim, trimIndex) => (
                        <th key={trim.id > 0 ? trim.id : `head-trim-${trimIndex}`} scope="col">
                          {formatTrimTitle(trim)}
                        </th>
                      ))}
                    </tr>
                  </thead>
                  {/* Photos row */}
                  <tbody>
                    <tr>
                      <th className="comparison-parameter-name">Фото</th>
                      {comparisonResult.trims.map((trim, trimIndex) => (
                        <td key={`photo-${trim.id}-${trimIndex}`} className="comparison-photo-cell">
                          {comparisonPhotos[trimIndex] ? (
                            <img src={comparisonPhotos[trimIndex]} alt={formatTrimTitle(trim)} className="comparison-photo-img" />
                          ) : (
                            <div className="comparison-photo-placeholder">Фото</div>
                          )}
                        </td>
                      ))}
                    </tr>
                  </tbody>
                  <tbody>
                    {comparisonParameters.map((parameter) => (
                      <tr key={parameter.key}>
                        <th scope="row" className="comparison-parameter-name">
                          {parameter.label}
                        </th>

                        {comparisonResult.trims.map((trim, trimIndex) => {
                          const rawValue = parameter.accessor(trim)
                          const tone = getMetricTone(
                            parameter,
                            comparisonResult.trims,
                            trimIndex,
                          )

                          return (
                            <td
                              key={`${parameter.key}-${trim.id}-${trimIndex}`}
                              className={
                                tone
                                  ? `comparison-value-cell comparison-value-cell--${tone}`
                                  : 'comparison-value-cell'
                              }
                            >
                              {parameter.formatter(rawValue)}
                            </td>
                          )
                        })}
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </section>
          )}
        </section>
      </main>

      <SiteFooter />
    </div>
  )
}
