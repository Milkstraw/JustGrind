// coffee.model.ts
export interface Coffee {
  id: number;
  name: string;
  roaster?: string;
  originCountry?: string;
  originRegion?: string;
  elevationMasl?: number;
  processingMethod: ProcessingMethod;
  variety?: string;
  species: Species;
  roastLevel: number;
  roastDate?: string;
  isBlend: boolean;
  isActive: boolean;
  notes?: string;
  tastingNotes?: string;
  createdAt: string;
  grindLogs?: GrindLog[];
}

export type ProcessingMethod = 'Washed' | 'Natural' | 'Honey' | 'Anaerobic' | 'WetHulled' | 'Other';
export type Species = 'Arabica' | 'Robusta' | 'Liberica';

// grinder.model.ts
export interface Grinder {
  id: number;
  brand: string;
  model: string;
  grindType: GrindType;
  burrType: BurrType;
  scaleType: ScaleType;
  scaleSubDivisions?: number;
  scaleFormat?: string;
  scaleSubType?: string;
  burrSizeMm?: number;
  scaleMin: number;
  scaleMax: number;
  notes?: string;
  isVerified: boolean;
  calibrations?: GrinderCalibration[];
}

export type GrindType = 'Stepped' | 'Stepless';
export type BurrType = 'Flat' | 'Conical' | 'Blade';
export type ScaleType = 'Numeric' | 'Alpha' | 'AlphaNumeric';

export interface GrinderCalibration {
  id: number;
  grinderId: number;
  brewMethod: BrewMethod;
  nativeSetting: number;
  ngiValue: number;
  anchorLabel?: string;
}

// grind-log.model.ts
export interface GrindLog {
  id: number;
  coffeeId: number;
  grinderId: number;
  brewMethod: BrewMethod;
  nativeSetting: number;
  ngiNormalized: number;
  doseG?: number;
  yieldG?: number;
  extractionTimeS?: number;
  tdsPercent?: number;
  rating?: number;
  notes?: string;
  brewDate?: string;
  createdAt: string;
  recipeId?: number;
  coffee?: Coffee;
  grinder?: Grinder;
  recipe?: BrewRecipe;
}

export type BrewMethod =
  | 'Espresso' | 'MokaPot' | 'AeropressFine' | 'AeropressCoarse'
  | 'PourOver' | 'Chemex' | 'FrenchPress' | 'ColdBrew' | 'Siphon';

export const BREW_METHOD_LABELS: Record<BrewMethod, string> = {
  Espresso: 'Espresso',
  MokaPot: 'Moka Pot',
  AeropressFine: 'AeroPress (Fine)',
  AeropressCoarse: 'AeroPress (Coarse)',
  PourOver: 'Pour Over / V60',
  Chemex: 'Chemex',
  FrenchPress: 'French Press',
  ColdBrew: 'Cold Brew',
  Siphon: 'Siphon',
};

// brew-recipe.model.ts
export interface BrewRecipeStep {
  id: number;
  recipeId: number;
  stepOrder: number;
  instruction: string;
  durationS: number;
  pourWaterG?: number;
}

export interface BrewRecipe {
  id: number;
  name: string;
  coffeeId: number;
  grinderId: number;
  brewMethod: BrewMethod;
  nativeSetting?: number;
  doseG?: number;
  waterG?: number;
  waterTempC?: number;
  techniqueNotes?: string;
  rating?: number;
  isFavorite: boolean;
  steps: BrewRecipeStep[];
  createdAt: string;
  coffee?: Coffee;
  grinder?: Grinder;
}

export interface CreateBrewRecipeRequest {
  name: string;
  coffeeId: number;
  grinderId: number;
  brewMethod: BrewMethod;
  nativeSetting?: number;
  doseG?: number;
  waterG?: number;
  waterTempC?: number;
  techniqueNotes?: string;
  steps: {
    stepOrder: number;
    instruction: string;
    durationS: number;
    pourWaterG?: number;
  }[];
}

// estimate.model.ts
export interface EstimateRequest {
  coffeeId: number;
  targetGrinderId: number;
  brewMethod: BrewMethod;
}

export interface EstimateResponse {
  coffeeId: number;
  coffeeName: string;
  grinderId: number;
  grinderName: string;
  brewMethod: string;
  estimatedNativeSetting: number;
  estimatedNgi: number;
  confidenceScore: number;
  inferenceLayer: number;
  inferenceLayerLabel: string;
  sourceLogCount: number;
  avgSimilarityScore: number;
  explanation: string;
}

export interface AddGrindLogRequest {
  coffeeId: number;
  grinderId: number;
  brewMethod: BrewMethod;
  nativeSetting: number;
  doseG?: number;
  yieldG?: number;
  extractionTimeS?: number;
  rating?: number;
  notes?: string;
  recipeId?: number;
}

// auth
export interface AuthUser {
  email: string;
  displayName?: string | null;
  token: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName?: string;
}
