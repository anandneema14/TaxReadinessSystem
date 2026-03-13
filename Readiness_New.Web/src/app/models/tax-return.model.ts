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
  // Personal Information
  firstName: string;
  lastName: string;
  ssn: string;
  spouseFirstName?: string;
  spouseLastName?: string;
  spouseSSN?: string;
  address: string;
  city: string;
  state: string;
  zipCode: string;
  foreignCountry?: string;
  foreignProvince?: string;
  foreignPostalCode?: string;
  
  // Filing Status
  isPresidentialElectionCampaign: boolean;
  isSpousePresidentialElectionCampaign: boolean;
  canBeClaimedAsDependent: boolean;
  isSpouseItemizingDeductions: boolean;
  
  // Dependents
  dependents: Dependent[];
  hasMoreThanFourDependents: boolean;
  
  // Income Lines
  line1_Wages: number;
  line2a_TaxExemptInterest: number;
  line2b_TaxableInterest: number;
  line3a_QualifiedDividends: number;
  line3b_OrdinaryDividends: number;
  line4a_IRADistributions: number;
  line4b_TaxableIRADistributions: number;
  line5a_PensionsAndAnnuities: number;
  line5b_TaxablePensionsAndAnnuities: number;
  line6a_SocialSecurityBenefits: number;
  line6b_TaxableSocialSecurityBenefits: number;
  line7_CapitalGainOrLoss: number;
  line8_OtherIncome: number;
  line9_TotalIncome: number;
  
  // Adjustments
  line10a_AdjustmentsToIncome: number;
  line10b_CharitableContributions: number;
  line10c_TotalAdjustments: number;
  line11_AdjustedGrossIncome: number;
  
  // Deductions
  line12_StandardOrItemizedDeduction: number;
  line12a_StandardDeduction: number;
  isItemizingDeductions: boolean;
  line13_QualifiedBusinessIncomeDeduction: number;
  line14_TotalDeductions: number;
  line15_TaxableIncome: number;
  
  // Tax and Credits
  line16_Tax: number;
  line17_ScheduleTwo_AdditionalTax: number;
  line18_TotalTax: number;
  line19_ChildTaxCredit: number;
  line20_ScheduleThree_AdditionalCredits: number;
  line21_TotalCredits: number;
  line22_TaxAfterCredits: number;
  line23_OtherTaxes: number;
  
  // Payments
  line24_TotalTax: number;
  line25a_FederalIncomeTaxWithheld: number;
  line25b_EarnedIncomeCredit: number;
  line25c_AdditionalChildTaxCredit: number;
  line25d_AmericanOpportunityCredit: number;
  line26_RecoveryRebateCredit: number;
  line27_OtherPaymentsAndCredits: number;
  line28_TotalPayments: number;
  
  // Refund/Owed
  line29_Overpayment: number;
  line30_AmountToBeRefunded: number;
  line31_AmountAppliedToNextYear: number;
  line32_AmountYouOwe: number;
  line33_EstimatedTaxPenalty: number;
  
  // Banking
  routingNumber?: string;
  accountNumber?: string;
  accountType?: string;
  
  // Third Party
  hasThirdPartyDesignee: boolean;
  thirdPartyDesigneeName?: string;
  thirdPartyDesigneePhone?: string;
  thirdPartyDesigneePIN?: string;
  
  // Signature
  taxpayerOccupation?: string;
  spouseOccupation?: string;
  taxpayerPhoneNumber?: string;
  taxpayerEmail?: string;
  
  // Preparer
  isSelfPrepared: boolean;
  preparerName?: string;
  preparerPTIN?: string;
  preparerFirmName?: string;
  preparerFirmEIN?: string;
  preparerAddress?: string;
  preparerPhone?: string;
}

export interface Dependent {
  firstName: string;
  lastName: string;
  ssn: string;
  relationship: string;
  isQualifyingChildForChildTaxCredit: boolean;
  isQualifyingChildForOtherCredit: boolean;
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

export interface ValidationResult {
  isValid: boolean;
  violationCount: number;
  violations: Violation[];
}

export interface Violation {
  ruleId: string;
  message: string;
  category: string;
  severity: string;
}