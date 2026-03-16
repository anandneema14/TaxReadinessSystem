export interface TaxReturn {
  taxpayerId: string;
  taxYear: number;
  filingStatus: string;
  totalIncome: number;
  totalDeductions: number;
  taxableIncome: number;
  totalTax: number;
  withholdingAmount: number;
  hasDependents: boolean;
  numberOfDependents: number;
  form1040: Form1040;
  scheduleA: ScheduleA;
  scheduleC: ScheduleC;
  w2Forms: W2Form[];
  filingDate?: string;
}

export interface Form1040 {
  name: string;
  ssn: string;
  address: string;
  line1_Wages: number;
  line2_Interest: number;
  line3_Dividends: number;
  line8_TotalIncome: number;
  line9_StandardDeduction: number;
  line11_TaxableIncome: number;
  line16_TotalTax: number;
  line24_TotalPayments: number;
  line34_Refund: number;
  line37_AmountYouOwe: number;
}

export interface ScheduleA {
  medicalExpenses: number;
  stateAndLocalTaxes: number;
  mortgageInterest: number;
  charitableContributions: number;
  totalItemizedDeductions: number;
}

export interface ScheduleC {
  businessName: string;
  grossReceipts: number;
  totalExpenses: number;
  netProfit: number;
}

export interface W2Form {
  employerName: string;
  employerEIN: string;
  wages: number;
  federalTaxWithheld: number;
  socialSecurityWages: number;
}

export interface ReadinessScore {
  score: number;
  level: string;
  summary: string;
  categoryScores: CategoryScore[];
  validationResult: ValidationResult;
  isApproved: boolean;
  isAutoApproved: boolean;
  approvedBy?: string;
  approvedAt?: string;
}

export interface CategoryScore {
  category: string;
  score: number;
}

export interface ValidationResult {
  isValid: boolean;
  violations: Violation[];
  violationCount: number;
}

export interface Violation {
  ruleId: string;
  message: string;
  severity: string;
  category: string;
}
