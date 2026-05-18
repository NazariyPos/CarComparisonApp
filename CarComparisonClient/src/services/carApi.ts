import { isAxiosError } from 'axios'
import { apiClient } from './apiClient'

export interface BrandBasicDto {
  id: number
  name: string
}

export interface ModelDto {
  id: number
  name: string
  bodyType: string
  brandId: number
}

export interface GenerationVariantOptionDto {
  id: number
  generationId: number
  name: string
  variantType: string
  bodyStyleId: number
  bodyStyleName: string
  doorsCount: number
  yearFrom: number
  yearTo: number
}

export interface GenerationDto {
  id: number
  name: string
  modelId: number
  yearFrom: number
  yearTo: number
  variants?: GenerationVariantOptionDto[]
}

export interface TrimDto {
  id: number
  name: string
  generationVariantId: number
  transmissionType?: string
}

export interface GenerationCardDto {
  generationId: number
  trimId?: number
  generationVariantId?: number
  generationVariantName?: string
  yearFrom: number
  yearTo: number
  photoUrl?: string
  modelId: number
  modelName: string
  bodyType: string
  brandId: number
  brandName: string
  trimCount: number
}

export interface GenerationImageDto {
  id: number
  generationVariantId: number
  url: string
  isPrimary: boolean
  sortOrder: number
  createdAt: string
}

export interface ExternalCarImageDto {
  url: string
}

export interface GenerationVariantDto {
  id: number
  generationId: number
  name: string
  variantType: string
  bodyStyleId: number
  bodyStyleName: string
  doorsCount: number
  yearFrom: number
  yearTo: number
  isDefault: boolean
  photoUrl?: string
  images: GenerationImageDto[]
}

export interface GenerationVariantBasicDto {
  id: number
  name: string
  variantType: string
  bodyStyleId: number
  bodyStyleName: string
  doorsCount: number
}

export interface TrimBasicDto {
  id: number
  generationVariantId: number
  name: string
  transmissionType: string
  doorsCount?: number
  seatsCount?: number
  variantType: string
  bodyStyleName: string
}

export interface GenerationWithTrimsDto {
  id: number
  generationVariantId: number
  legacyGenerationId?: number
  name: string
  displayName: string
  yearFrom: number
  yearTo: number
  photoUrl?: string
  variants: GenerationVariantDto[]
  brand: BrandBasicDto
  model: ModelDto
  trims: TrimBasicDto[]
}

export interface GenerationBasicDto {
  id: number
  name: string
  yearFrom: number
  yearTo: number
  photoUrl?: string
}

export interface ModelBasicDto {
  id: number
  name: string
}

export interface TechnicalDetailsFullDto {
  maxSpeed?: number
  acceleration0To100?: number
  engineCode?: string
  engineType?: string
  cylindersCount?: number
  valvesCount?: number
  compressionRatio?: number
  fuelType?: string
  power?: number
  torque?: number
  maxPowerAtRPM?: number
  maxTorqueAtRPM?: number
  engineDisplacement?: number
  driveType?: string
  fuelConsumptionCity?: number
  fuelConsumptionMixed?: number
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
}

export interface TrimFullDetailsDto {
  id: number
  name: string
  transmissionType: string
  doorsCount?: number
  seatsCount?: number
  generation: GenerationBasicDto
  generationVariant: GenerationVariantBasicDto
  model: ModelBasicDto
  brand: BrandBasicDto
  technicalDetails?: TechnicalDetailsFullDto
}

export interface ReviewDto {
  id: number
  userId: number
  trimId: number
  content: string
  rating: number
  createdAt: string
  updatedAt?: string
}

export interface ReviewMutationDto {
  id?: number
  userId: number
  trimId: number
  content: string
  rating: number
}

export interface ReviewWithDetailsDto {
  review: ReviewDto
  username: string
  model: string
  generation: string
  trim: string
  brand: string
}

export interface TechnicalDetailsDto {
  id: number
  trimId: number
  fuelType?: string
}

export interface SearchFacetsDto {
  bodyStyles: BodyStyleOptionDto[]
  variantTypes: string[]
  transmissionTypes: string[]
  fuelTypes: string[]
}

export interface BodyStyleOptionDto {
  id: number
  name: string
}

export interface SearchGenerationParams {
  brandId?: number
  modelId?: number
  generationId?: number
  generationVariantId?: number
  brand?: string
  model?: string
  generation?: string
  minYear?: number
  maxYear?: number
  bodyStyleId?: number
  variantType?: string
  transmission?: string
  fuelType?: string
}

type RawDto = Record<string, unknown>

function readNumber(raw: RawDto, camelKey: string, pascalKey: string): number {
  const value = raw[camelKey] ?? raw[pascalKey]
  return typeof value === 'number' ? value : 0
}

function readString(raw: RawDto, camelKey: string, pascalKey: string): string {
  const value = raw[camelKey] ?? raw[pascalKey]
  return typeof value === 'string' ? value : ''
}

function readOptionalString(
  raw: RawDto,
  camelKey: string,
  pascalKey: string,
): string | undefined {
  const value = raw[camelKey] ?? raw[pascalKey]
  return typeof value === 'string' ? value : undefined
}

function readOptionalNumber(
  raw: RawDto,
  camelKey: string,
  pascalKey: string,
): number | undefined {
  const value = raw[camelKey] ?? raw[pascalKey]

  if (typeof value === 'number' && Number.isFinite(value)) {
    return value
  }

  if (typeof value === 'string') {
    const parsed = Number.parseFloat(value)
    return Number.isFinite(parsed) ? parsed : undefined
  }

  return undefined
}

function readOptionalBoolean(
  raw: RawDto,
  camelKey: string,
  pascalKey: string,
): boolean | undefined {
  const value = raw[camelKey] ?? raw[pascalKey]
  return typeof value === 'boolean' ? value : undefined
}

function readOptionalRawDto(
  raw: RawDto,
  camelKey: string,
  pascalKey: string,
): RawDto | undefined {
  const value = raw[camelKey] ?? raw[pascalKey]
  return typeof value === 'object' && value !== null ? (value as RawDto) : undefined
}

function readRawDtoArray(
  raw: RawDto,
  camelKey: string,
  pascalKey: string,
): RawDto[] {
  const value = raw[camelKey] ?? raw[pascalKey]

  if (!Array.isArray(value)) {
    return []
  }

  return value.filter((item): item is RawDto => typeof item === 'object' && item !== null)
}

function mapBrandDto(raw: RawDto): BrandBasicDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    name: readString(raw, 'name', 'Name'),
  }
}

function mapModelDto(raw: RawDto): ModelDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    name: readString(raw, 'name', 'Name'),
    bodyType: readString(raw, 'bodyType', 'BodyType'),
    brandId: readNumber(raw, 'brandId', 'BrandId'),
  }
}

function mapGenerationVariantOptionDto(raw: RawDto): GenerationVariantOptionDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    generationId: readNumber(raw, 'generationId', 'GenerationId'),
    name: readString(raw, 'name', 'Name'),
    variantType: readString(raw, 'variantType', 'VariantType'),
    bodyStyleId: readNumber(raw, 'bodyStyleId', 'BodyStyleId'),
    bodyStyleName: readString(raw, 'bodyStyleName', 'BodyStyleName'),
    doorsCount: readNumber(raw, 'doorsCount', 'DoorsCount'),
    yearFrom: readNumber(raw, 'yearFrom', 'YearFrom'),
    yearTo: readNumber(raw, 'yearTo', 'YearTo'),
  }
}

function mapGenerationDto(raw: RawDto): GenerationDto {
  const variants = readRawDtoArray(raw, 'variants', 'Variants')
    .map(mapGenerationVariantOptionDto)
    .filter((v) => v.id > 0)

  return {
    id: readNumber(raw, 'id', 'Id'),
    name: readString(raw, 'name', 'Name'),
    modelId: readNumber(raw, 'modelId', 'ModelId'),
    yearFrom: readNumber(raw, 'yearFrom', 'YearFrom'),
    yearTo: readNumber(raw, 'yearTo', 'YearTo'),
    variants: variants.length > 0 ? variants : undefined,
  }
}

function mapTrimDto(raw: RawDto): TrimDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    name: readString(raw, 'name', 'Name'),
    generationVariantId: readNumber(raw, 'generationVariantId', 'GenerationVariantId'),
    transmissionType: readOptionalString(
      raw,
      'transmissionType',
      'TransmissionType',
    ),
  }
}

function mapGenerationCardDto(raw: RawDto): GenerationCardDto {
  const generationVariantName = readOptionalString(raw, 'generationVariantName', 'GenerationVariantName')

  return {
    trimId: readOptionalNumber(raw, 'trimId', 'TrimId'),
    generationId: readNumber(raw, 'generationId', 'GenerationId'),
    generationVariantId: readOptionalNumber(raw, 'generationVariantId', 'GenerationVariantId'),
    generationVariantName,
    yearFrom: readNumber(raw, 'yearFrom', 'YearFrom'),
    yearTo: readNumber(raw, 'yearTo', 'YearTo'),
    photoUrl: readOptionalString(raw, 'photoUrl', 'PhotoUrl'),
    modelId: readNumber(raw, 'modelId', 'ModelId'),
    modelName: readString(raw, 'modelName', 'ModelName'),
    bodyType: readString(raw, 'bodyType', 'BodyType'),
    brandId: readNumber(raw, 'brandId', 'BrandId'),
    brandName: readString(raw, 'brandName', 'BrandName'),
    trimCount: readNumber(raw, 'trimCount', 'TrimCount'),
  }
}

function mapTechnicalDetailsDto(raw: RawDto): TechnicalDetailsDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    trimId: readNumber(raw, 'trimId', 'TrimId'),
    fuelType: readOptionalString(raw, 'fuelType', 'FuelType'),
  }
}

function mapGenerationImageDto(raw: RawDto): GenerationImageDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    generationVariantId: readNumber(raw, 'generationVariantId', 'GenerationVariantId'),
    url: readString(raw, 'url', 'Url'),
    isPrimary: readOptionalBoolean(raw, 'isPrimary', 'IsPrimary') ?? false,
    sortOrder: readOptionalNumber(raw, 'sortOrder', 'SortOrder') ?? 0,
    createdAt: readString(raw, 'createdAt', 'CreatedAt'),
  }
}

function mapGenerationVariantDto(raw: RawDto): GenerationVariantDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    generationId: readNumber(raw, 'generationId', 'GenerationId'),
    name: readString(raw, 'name', 'Name'),
    variantType: readString(raw, 'variantType', 'VariantType'),
    bodyStyleId: readNumber(raw, 'bodyStyleId', 'BodyStyleId'),
    bodyStyleName: readString(raw, 'bodyStyleName', 'BodyStyleName'),
    doorsCount: readNumber(raw, 'doorsCount', 'DoorsCount'),
    yearFrom: readNumber(raw, 'yearFrom', 'YearFrom'),
    yearTo: readNumber(raw, 'yearTo', 'YearTo'),
    isDefault: readOptionalBoolean(raw, 'isDefault', 'IsDefault') ?? false,
    photoUrl: readOptionalString(raw, 'photoUrl', 'PhotoUrl'),
    images: asRawDtoArray(raw.images ?? raw.Images).map(mapGenerationImageDto),
  }
}

function mapGenerationVariantBasicDto(raw: RawDto): GenerationVariantBasicDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    name: readString(raw, 'name', 'Name'),
    variantType: readString(raw, 'variantType', 'VariantType'),
    bodyStyleId: readNumber(raw, 'bodyStyleId', 'BodyStyleId'),
    bodyStyleName: readString(raw, 'bodyStyleName', 'BodyStyleName'),
    doorsCount: readNumber(raw, 'doorsCount', 'DoorsCount'),
  }
}

function mapTrimBasicDto(raw: RawDto): TrimBasicDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    generationVariantId: readNumber(raw, 'generationVariantId', 'GenerationVariantId'),
    name: readString(raw, 'name', 'Name'),
    transmissionType: readString(raw, 'transmissionType', 'TransmissionType'),
    doorsCount: readOptionalNumber(raw, 'doorsCount', 'DoorsCount'),
    seatsCount: readOptionalNumber(raw, 'seatsCount', 'SeatsCount'),
    variantType: readString(raw, 'variantType', 'VariantType'),
    bodyStyleName: readString(raw, 'bodyStyleName', 'BodyStyleName'),
  }
}

function mapGenerationWithTrimsDto(raw: RawDto): GenerationWithTrimsDto {
  const brandRaw = readOptionalRawDto(raw, 'brand', 'Brand') ?? {}
  const modelRaw = readOptionalRawDto(raw, 'model', 'Model') ?? {}
  const name = readString(raw, 'name', 'Name')

  return {
    id: readNumber(raw, 'id', 'Id'),
    generationVariantId: readNumber(raw, 'generationVariantId', 'GenerationVariantId'),
    legacyGenerationId: readOptionalNumber(raw, 'legacyGenerationId', 'LegacyGenerationId'),
    name,
    displayName: readString(raw, 'displayName', 'DisplayName') || name,
    yearFrom: readNumber(raw, 'yearFrom', 'YearFrom'),
    yearTo: readNumber(raw, 'yearTo', 'YearTo'),
    photoUrl: readOptionalString(raw, 'photoUrl', 'PhotoUrl'),
    variants: asRawDtoArray(raw.variants ?? raw.Variants).map(mapGenerationVariantDto),
    brand: mapBrandDto(brandRaw),
    model: mapModelDto(modelRaw),
    trims: asRawDtoArray(raw.trims ?? raw.Trims).map(mapTrimBasicDto),
  }
}

function mapGenerationBasicDto(raw: RawDto): GenerationBasicDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    name: readString(raw, 'name', 'Name'),
    yearFrom: readNumber(raw, 'yearFrom', 'YearFrom'),
    yearTo: readNumber(raw, 'yearTo', 'YearTo'),
    photoUrl: readOptionalString(raw, 'photoUrl', 'PhotoUrl'),
  }
}

function mapModelBasicDto(raw: RawDto): ModelBasicDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    name: readString(raw, 'name', 'Name'),
  }
}

function mapTechnicalDetailsFullDto(raw: RawDto): TechnicalDetailsFullDto {
  return {
    maxSpeed: readOptionalNumber(raw, 'maxSpeed', 'MaxSpeed'),
    acceleration0To100: readOptionalNumber(raw, 'acceleration0To100', 'Acceleration0To100'),
    engineCode: readOptionalString(raw, 'engineCode', 'EngineCode'),
    engineType: readOptionalString(raw, 'engineType', 'EngineType'),
    cylindersCount: readOptionalNumber(raw, 'cylindersCount', 'CylindersCount'),
    valvesCount: readOptionalNumber(raw, 'valvesCount', 'ValvesCount'),
    compressionRatio: readOptionalNumber(raw, 'compressionRatio', 'CompressionRatio'),
    fuelType: readOptionalString(raw, 'fuelType', 'FuelType'),
    power: readOptionalNumber(raw, 'power', 'Power'),
    driveType: readOptionalString(raw, 'driveType', 'DriveType'),
    torque: readOptionalNumber(raw, 'torque', 'Torque'),
    maxPowerAtRPM: readOptionalNumber(raw, 'maxPowerAtRPM', 'MaxPowerAtRPM'),
    maxTorqueAtRPM: readOptionalNumber(raw, 'maxTorqueAtRPM', 'MaxTorqueAtRPM'),
    engineDisplacement: readOptionalNumber(raw, 'engineDisplacement', 'EngineDisplacement'),
    fuelConsumptionCity: readOptionalNumber(raw, 'fuelConsumptionCity', 'FuelConsumptionCity'),
    fuelConsumptionMixed: readOptionalNumber(raw, 'fuelConsumptionMixed', 'FuelConsumptionMixed'),
    fuelConsumptionHighway: readOptionalNumber(raw, 'fuelConsumptionHighway', 'FuelConsumptionHighway'),
    electricRange: readOptionalNumber(raw, 'electricRange', 'ElectricRange'),
    length: readOptionalNumber(raw, 'length', 'Length'),
    width: readOptionalNumber(raw, 'width', 'Width'),
    height: readOptionalNumber(raw, 'height', 'Height'),
    wheelbase: readOptionalNumber(raw, 'wheelbase', 'Wheelbase'),
    frontTrack: readOptionalNumber(raw, 'frontTrack', 'FrontTrack'),
    rearTrack: readOptionalNumber(raw, 'rearTrack', 'RearTrack'),
    curbWeight: readOptionalNumber(raw, 'curbWeight', 'CurbWeight'),
    grossWeight: readOptionalNumber(raw, 'grossWeight', 'GrossWeight'),
    fuelTankCapacity: readOptionalNumber(raw, 'fuelTankCapacity', 'FuelTankCapacity'),
    turningCircle: readOptionalNumber(raw, 'turningCircle', 'TurningCircle'),
    frontBrakes: readOptionalString(raw, 'frontBrakes', 'FrontBrakes'),
    rearBrakes: readOptionalString(raw, 'rearBrakes', 'RearBrakes'),
    frontSuspension: readOptionalString(raw, 'frontSuspension', 'FrontSuspension'),
    rearSuspension: readOptionalString(raw, 'rearSuspension', 'RearSuspension'),
  }
}

function mapTrimFullDetailsDto(raw: RawDto): TrimFullDetailsDto {
  const generationRaw = readOptionalRawDto(raw, 'generation', 'Generation') ?? {}
  const generationVariantRaw =
    readOptionalRawDto(raw, 'generationVariant', 'GenerationVariant') ?? {}
  const modelRaw = readOptionalRawDto(raw, 'model', 'Model') ?? {}
  const brandRaw = readOptionalRawDto(raw, 'brand', 'Brand') ?? {}
  const technicalDetailsRaw = readOptionalRawDto(raw, 'technicalDetails', 'TechnicalDetails')

  return {
    id: readNumber(raw, 'id', 'Id'),
    name: readString(raw, 'name', 'Name'),
    transmissionType: readString(raw, 'transmissionType', 'TransmissionType'),
    doorsCount: readOptionalNumber(raw, 'doorsCount', 'DoorsCount'),
    seatsCount: readOptionalNumber(raw, 'seatsCount', 'SeatsCount'),
    generation: mapGenerationBasicDto(generationRaw),
    generationVariant: mapGenerationVariantBasicDto(generationVariantRaw),
    model: mapModelBasicDto(modelRaw),
    brand: mapBrandDto(brandRaw),
    technicalDetails: technicalDetailsRaw
      ? mapTechnicalDetailsFullDto(technicalDetailsRaw)
      : undefined,
  }
}

function mapReviewDto(raw: RawDto): ReviewDto {
  return {
    id: readNumber(raw, 'id', 'Id'),
    userId: readNumber(raw, 'userId', 'UserId'),
    trimId: readNumber(raw, 'trimId', 'TrimId'),
    content: readString(raw, 'content', 'Content'),
    rating: readNumber(raw, 'rating', 'Rating'),
    createdAt: readString(raw, 'createdAt', 'CreatedAt'),
    updatedAt: readOptionalString(raw, 'updatedAt', 'UpdatedAt'),
  }
}

function mapReviewWithDetailsDto(raw: RawDto): ReviewWithDetailsDto {
  const reviewRaw = readOptionalRawDto(raw, 'review', 'Review') ?? {}

  return {
    review: mapReviewDto(reviewRaw),
    username: readString(raw, 'username', 'Username'),
    model: readString(raw, 'model', 'Model'),
    generation: readString(raw, 'generation', 'Generation'),
    trim: readString(raw, 'trim', 'Trim'),
    brand: readString(raw, 'brand', 'Brand'),
  }
}

function mapSearchFacetsDto(raw: RawDto): SearchFacetsDto {
  const readStringArray = (camelKey: string, pascalKey: string): string[] => {
    const value = raw[camelKey] ?? raw[pascalKey]

    if (!Array.isArray(value)) {
      return []
    }

    return value.filter((item): item is string => typeof item === 'string')
  }

  const readBodyStyles = (camelKey: string, pascalKey: string): BodyStyleOptionDto[] => {
    const value = raw[camelKey] ?? raw[pascalKey]
    if (!Array.isArray(value)) {
      return []
    }

    return value
      .filter((item): item is RawDto => typeof item === 'object' && item !== null)
      .map((item) => ({
        id: readNumber(item, 'id', 'Id'),
        name: readString(item, 'name', 'Name'),
      }))
      .filter((item) => item.id > 0 && item.name.length > 0)
  }

  return {
    bodyStyles: readBodyStyles('bodyStyles', 'BodyStyles'),
    variantTypes: readStringArray('variantTypes', 'VariantTypes'),
    transmissionTypes: readStringArray('transmissionTypes', 'TransmissionTypes'),
    fuelTypes: readStringArray('fuelTypes', 'FuelTypes'),
  }
}

function asRawDtoArray(value: unknown): RawDto[] {
  if (!Array.isArray(value)) {
    return []
  }

  return value.filter((item): item is RawDto => {
    return typeof item === 'object' && item !== null
  })
}

export interface TestResponse {
  message: string
  timestamp: string
}

export async function getBrands(): Promise<BrandBasicDto[]> {
  const { data } = await apiClient.get<unknown>('/Cars/brands')
  return asRawDtoArray(data).map(mapBrandDto)
}

export async function getModelsByBrand(brandId: number): Promise<ModelDto[]> {
  const { data } = await apiClient.get<unknown>(`/Cars/brands/${brandId}/models`)
  return asRawDtoArray(data).map(mapModelDto)
}

export async function getGenerationsByModel(
  modelId: number,
): Promise<GenerationDto[]> {
  const { data } = await apiClient.get<unknown>(`/Cars/models/${modelId}/generations`)
  return asRawDtoArray(data).map(mapGenerationDto)
}

export async function getGenerationVariantsByModel(
  modelId: number,
): Promise<GenerationVariantDto[]> {
  const { data } = await apiClient.get<unknown>(`/Cars/models/${modelId}/variants`)
  return asRawDtoArray(data).map(mapGenerationVariantDto)
}

export async function getTrimsByGeneration(
  generationId: number,
): Promise<TrimDto[]> {
  const { data } = await apiClient.get<unknown>(`/Cars/generations/${generationId}/trims`)
  return asRawDtoArray(data).map(mapTrimDto)
}

export async function getTrimsByGenerationVariant(
  variantId: number,
): Promise<TrimDto[]> {
  const { data } = await apiClient.get<unknown>(`/Cars/variants/${variantId}/trims`)
  return asRawDtoArray(data).map(mapTrimDto)
}

export async function getTechnicalDetailsByTrim(
  trimId: number,
): Promise<TechnicalDetailsDto | null> {
  try {
    const { data } = await apiClient.get<unknown>(
      `/Cars/trims/${trimId}/technical-details`,
    )

    if (!data || typeof data !== 'object') {
      return null
    }

    return mapTechnicalDetailsDto(data as RawDto)
  } catch {
    return null
  }
}

export async function getSearchFacets(
  params: Pick<
    SearchGenerationParams,
    | 'brandId'
    | 'modelId'
    | 'generationId'
    | 'brand'
    | 'model'
    | 'generation'
    | 'minYear'
    | 'maxYear'
  >,
): Promise<SearchFacetsDto> {
  const { data } = await apiClient.get<unknown>('/Cars/search-facets', {
    params,
  })

  if (!data || typeof data !== 'object') {
    return {
      bodyStyles: [],
      variantTypes: [],
      transmissionTypes: [],
      fuelTypes: [],
    }
  }

  return mapSearchFacetsDto(data as RawDto)
}

export async function searchGenerations(
  params: SearchGenerationParams,
): Promise<GenerationCardDto[]> {
  const { data } = await apiClient.get<unknown>('/Cars/search', {
    params,
  })
  return asRawDtoArray(data).map(mapGenerationCardDto)
}

export async function compareTrims(trimIds: number[]): Promise<unknown> {
  const { data } = await apiClient.get('/Comparison/compare', {
    params: { trimIds: trimIds.join(',') },
  })

  return data
}

export async function getGenerationWithTrims(
  generationId: number,
): Promise<GenerationWithTrimsDto | null> {
  try {
    const { data } = await apiClient.get<unknown>(
      `/Cars/generations/${generationId}/details`,
    )

    if (!data || typeof data !== 'object') {
      return null
    }

    return mapGenerationWithTrimsDto(data as RawDto)
  } catch {
    return null
  }
}

export async function getGenerationVariantWithTrims(
  generationVariantId: number,
): Promise<GenerationWithTrimsDto | null> {
  try {
    const { data } = await apiClient.get<unknown>(
      `/Cars/variants/${generationVariantId}/details`,
    )

    if (!data || typeof data !== 'object') {
      return null
    }

    return mapGenerationWithTrimsDto(data as RawDto)
  } catch {
    return null
  }
}

export async function getTrimFullDetails(
  trimId: number,
): Promise<TrimFullDetailsDto | null> {
  try {
    const { data } = await apiClient.get<unknown>(`/Cars/trims/${trimId}/details`)

    if (!data || typeof data !== 'object') {
      return null
    }

    return mapTrimFullDetailsDto(data as RawDto)
  } catch {
    return null
  }
}

export async function getReviewsByTrim(
  trimId: number,
): Promise<ReviewWithDetailsDto[]> {
  try {
    const { data } = await apiClient.get<unknown>(`/Reviews/trim/${trimId}`)
    return asRawDtoArray(data).map(mapReviewWithDetailsDto)
  } catch {
    return []
  }
}

export async function getReviewsByUser(
  userId: number,
): Promise<ReviewWithDetailsDto[]> {
  try {
    const { data } = await apiClient.get<unknown>(`/Reviews/user/${userId}`)
    return asRawDtoArray(data).map(mapReviewWithDetailsDto)
  } catch {
    return []
  }
}

export async function createReview(payload: { trimId: number; rating: number; content: string }): Promise<ReviewDto> {
  try {
    const { data } = await apiClient.post<unknown>('/Reviews', payload)

    if (!data || typeof data !== 'object') {
      throw new Error('Invalid response')
    }

    return mapReviewDto(data as RawDto)
  } catch (error) {
    throw error
  }
}

export async function updateReview(reviewId: number, payload: ReviewMutationDto): Promise<ReviewDto> {
  try {
    const { data } = await apiClient.put<unknown>(`/Reviews/${reviewId}`, payload)

    if (!data || typeof data !== 'object') {
      throw new Error('Invalid response')
    }

    return mapReviewDto(data as RawDto)
  } catch (error) {
    throw error
  }
}

export async function deleteReview(reviewId: number): Promise<boolean> {
  try {
    await apiClient.delete(`/Reviews/${reviewId}`)
    return true
  } catch {
    return false
  }
}

export async function getCarImagesFromExternal(
  brand: string,
  model: string,
  year?: number,
): Promise<ExternalCarImageDto[]> {
  try {
    const params: Record<string, string | number> = {
      brand,
      model,
    }

    if (typeof year === 'number' && Number.isFinite(year)) {
      params.year = year
    }

    const { data } = await apiClient.get<unknown>('/Cars/images/external', {
      params,
    })

    return asRawDtoArray(data)
      .map((item) => ({
        url: readString(item, 'url', 'Url'),
      }))
      .filter((item) => item.url.length > 0)
  } catch {
    return []
  }
}

export async function getGenerationVariantImages(
  generationId: number,
  variantId: number,
): Promise<GenerationImageDto[]> {
  try {
    const { data } = await apiClient.get<unknown>(
      `/generations/${generationId}/variants/${variantId}/images`,
    )

    return asRawDtoArray(data).map(mapGenerationImageDto)
  } catch {
    return []
  }
}

export async function uploadGenerationVariantImage(
  generationId: number,
  variantId: number,
  file: File,
  isPrimary: boolean,
  sortOrder?: number,
): Promise<GenerationImageDto | null> {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('isPrimary', String(isPrimary))

  if (typeof sortOrder === 'number' && Number.isFinite(sortOrder)) {
    formData.append('sortOrder', String(sortOrder))
  }

  try {
    const { data } = await apiClient.post<unknown>(
      `/generations/${generationId}/variants/${variantId}/images`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      },
    )

    if (!data || typeof data !== 'object') {
      return null
    }

    return mapGenerationImageDto(data as RawDto)
  } catch {
    return null
  }
}

export async function uploadGenerationImageByGeneration(
  generationId: number,
  file: File,
  isPrimary: boolean,
  sortOrder?: number,
): Promise<GenerationImageDto | null> {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('isPrimary', String(isPrimary))

  if (typeof sortOrder === 'number' && Number.isFinite(sortOrder)) {
    formData.append('sortOrder', String(sortOrder))
  }

  try {
    const { data } = await apiClient.post<unknown>(
      `/generations/${generationId}/images`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      },
    )

    if (!data || typeof data !== 'object') {
      return null
    }

    return mapGenerationImageDto(data as RawDto)
  } catch {
    return null
  }
}

export async function setGenerationVariantImagePrimary(
  generationId: number,
  variantId: number,
  imageId: number,
): Promise<GenerationImageDto | null> {
  try {
    const { data } = await apiClient.put<unknown>(
      `/generations/${generationId}/variants/${variantId}/images/${imageId}/primary`,
    )

    if (!data || typeof data !== 'object') {
      return null
    }

    return mapGenerationImageDto(data as RawDto)
  } catch {
    return null
  }
}

export async function deleteGenerationVariantImage(
  generationId: number,
  variantId: number,
  imageId: number,
): Promise<boolean> {
  try {
    await apiClient.delete(`/generations/${generationId}/variants/${variantId}/images/${imageId}`)
    return true
  } catch {
    return false
  }
}

// Admin write operations
export async function createBrand(name: string): Promise<{ id: number; name: string } | null> {
  try {
    const { data } = await apiClient.post('/admin/brands', { name })
    return data as { id: number; name: string }
  } catch {
    return null
  }
}

export async function createModel(brandId: number, name: string, bodyType?: string): Promise<{ id: number; name: string; brandId: number } | null> {
  try {
    const { data } = await apiClient.post(`/admin/brands/${brandId}/models`, { name, bodyType })
    return data as { id: number; name: string; brandId: number }
  } catch {
    return null
  }
}

export async function createGeneration(modelId: number, name: string, yearFrom: number, yearTo: number, photoUrl?: string): Promise<{ id: number; name: string; modelId: number } | null> {
  try {
    const { data } = await apiClient.post(`/admin/models/${modelId}/generations`, { name, yearFrom, yearTo, photoUrl })
    return data as { id: number; name: string; modelId: number }
  } catch {
    return null
  }
}

export async function getAdminBodyStyles(): Promise<BodyStyleOptionDto[]> {
  try {
    const { data } = await apiClient.get<unknown>('/admin/body-styles')
    return asRawDtoArray(data)
      .map((item) => ({
        id: readNumber(item, 'id', 'Id'),
        name: readString(item, 'name', 'Name'),
      }))
      .filter((item) => item.id > 0 && item.name.length > 0)
  } catch {
    return []
  }
}

export async function createGenerationVariant(modelId: number, payload: { name: string; variantType: string; bodyStyleId?: number; doorsCount: number; yearFrom: number; yearTo: number; isDefault: boolean; photoUrl?: string }): Promise<{ id: number; name: string; modelId: number; error?: string } | null> {
  try {
    const { data } = await apiClient.post(`/admin/models/${modelId}/variants`, payload)
    return data as { id: number; name: string; modelId: number }
  } catch (error: unknown) {
    if (isAxiosError(error) && error.response?.status === 409) {
      const responseData = error.response.data as Record<string, unknown>
      return {
        id: 0,
        name: '',
        modelId,
        error: (responseData?.message as string) || 'Варіант з такими параметрами вже існує'
      }
    }
    return null
  }
}

export async function createTrim(variantId: number, payload: { name: string; transmissionType?: string; doorsCount?: number; seatsCount?: number }): Promise<{ id: number; name: string; variantId: number } | null> {
  try {
    const { data } = await apiClient.post(`/admin/variants/${variantId}/trims`, payload)
    return data as { id: number; name: string; variantId: number }
  } catch {
    return null
  }
}

export async function createTechnicalDetails(trimId: number, payload: { 
  maxSpeed?: number
  acceleration0To100?: number
  engineCode?: string
  engineType?: string
  cylindersCount?: number
  valvesCount?: number
  compressionRatio?: number
  fuelType?: string
  power?: number
  torque?: number
  maxPowerAtRPM?: number
  maxTorqueAtRPM?: number
  engineDisplacement?: number
  driveType?: string
  fuelConsumptionCity?: number
  fuelConsumptionMixed?: number
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
}): Promise<{ id: number; trimId: number } | null> {
  try {
    const { data } = await apiClient.post(`/admin/trims/${trimId}/technical-details`, payload)
    return data as { id: number; trimId: number }
  } catch {
    return null
  }
}

export async function deleteBrand(id: number): Promise<boolean> {
  try {
    await apiClient.delete(`/admin/brands/${id}`)
    return true
  } catch {
    return false
  }
}

export async function deleteModel(id: number): Promise<boolean> {
  try {
    await apiClient.delete(`/admin/models/${id}`)
    return true
  } catch {
    return false
  }
}

export async function deleteGenerationVariant(id: number): Promise<boolean> {
  try {
    await apiClient.delete(`/admin/variants/${id}`)
    return true
  } catch {
    return false
  }
}

export async function deleteTrim(id: number): Promise<boolean> {
  try {
    await apiClient.delete(`/admin/trims/${id}`)
    return true
  } catch {
    return false
  }
}

export async function getFavorites(): Promise<GenerationCardDto[]> {
  try {
    const { data } = await apiClient.get<unknown>('/Favorites/me')
    return asRawDtoArray(data).map(mapGenerationCardDto)
  } catch {
    return []
  }
}

export async function removeFavorite(id: number): Promise<boolean> {
  try {
    await apiClient.delete(`/Favorites/trim/${id}`)
    return true
  } catch {
    return false
  }
}

export async function addFavorite(trimId: number): Promise<boolean> {
  try {
    const payload = { trimId }
    await apiClient.post('/Favorites', payload)
    return true
  } catch {
    return false
  }
}
