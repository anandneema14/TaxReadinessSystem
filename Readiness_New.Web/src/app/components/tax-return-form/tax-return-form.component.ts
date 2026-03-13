import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { TaxReturnService } from '../../services/tax-return.service';
import { TaxReturn, ValidationResult } from '../../models/tax-return.model';

@Component({
  selector: 'app-tax-return-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './tax-return-form.component.html',
  styleUrls: ['./tax-return-form.component.scss']
})
export class TaxReturnFormComponent {
  taxForm: FormGroup;
  validationResult: ValidationResult | null = null;
  loading = false;
  error: string | null = null;
  activeTab = 'personal';
  inputMode: 'form' | 'json' = 'form';
  jsonContent: string = '';
  jsonError: string | null = null;

  constructor(private fb: FormBuilder, private taxService: TaxReturnService) {
    this.taxForm = this.fb.group({
      taxpayerId: ['', Validators.required],
      taxYear: [2024, [Validators.required, Validators.min(2000), Validators.max(2025)]],
      filingStatus: ['Single', Validators.required],
      totalIncome: [0, Validators.required],
      totalDeductions: [0],
      taxableIncome: [0],
      totalTax: [0],
      withholdingAmount: [0],
      hasDependents: [false],
      numberOfDependents: [0],
      form1040: this.fb.group({
        // Personal Information
        firstName: ['', Validators.required],
        lastName: ['', Validators.required],
        ssn: ['', Validators.required],
        spouseFirstName: [''],
        spouseLastName: [''],
        spouseSSN: [''],
        address: ['', Validators.required],
        city: ['', Validators.required],
        state: ['', Validators.required],
        zipCode: ['', Validators.required],
        foreignCountry: [''],
        foreignProvince: [''],
        foreignPostalCode: [''],
        
        // Filing Status Indicators
        isPresidentialElectionCampaign: [false],
        isSpousePresidentialElectionCampaign: [false],
        canBeClaimedAsDependent: [false],
        isSpouseItemizingDeductions: [false],
        
        // Dependents
        dependents: this.fb.array([]),
        hasMoreThanFourDependents: [false],
        
        // Income Section (Lines 1-9)
        line1_Wages: [0],
        line2a_TaxExemptInterest: [0],
        line2b_TaxableInterest: [0],
        line3a_QualifiedDividends: [0],
        line3b_OrdinaryDividends: [0],
        line4a_IRADistributions: [0],
        line4b_TaxableIRADistributions: [0],
        line5a_PensionsAndAnnuities: [0],
        line5b_TaxablePensionsAndAnnuities: [0],
        line6a_SocialSecurityBenefits: [0],
        line6b_TaxableSocialSecurityBenefits: [0],
        line7_CapitalGainOrLoss: [0],
        line8_OtherIncome: [0],
        line9_TotalIncome: [0],
        
        // Adjustments to Income (Lines 10-11)
        line10a_AdjustmentsToIncome: [0],
        line10b_CharitableContributions: [0],
        line10c_TotalAdjustments: [0],
        line11_AdjustedGrossIncome: [0],
        
        // Standard/Itemized Deduction (Lines 12-14)
        line12_StandardOrItemizedDeduction: [0],
        line12a_StandardDeduction: [0],
        isItemizingDeductions: [false],
        line13_QualifiedBusinessIncomeDeduction: [0],
        line14_TotalDeductions: [0],
        
        // Taxable Income (Line 15)
        line15_TaxableIncome: [0],
        
        // Tax and Credits (Lines 16-23)
        line16_Tax: [0],
        line17_ScheduleTwo_AdditionalTax: [0],
        line18_TotalTax: [0],
        line19_ChildTaxCredit: [0],
        line20_ScheduleThree_AdditionalCredits: [0],
        line21_TotalCredits: [0],
        line22_TaxAfterCredits: [0],
        line23_OtherTaxes: [0],
        
        // Payments (Lines 24-32)
        line24_TotalTax: [0],
        line25a_FederalIncomeTaxWithheld: [0],
        line25b_EarnedIncomeCredit: [0],
        line25c_AdditionalChildTaxCredit: [0],
        line25d_AmericanOpportunityCredit: [0],
        line26_RecoveryRebateCredit: [0],
        line27_OtherPaymentsAndCredits: [0],
        line28_TotalPayments: [0],
        
        // Refund or Amount Owed (Lines 29-35)
        line29_Overpayment: [0],
        line30_AmountToBeRefunded: [0],
        line31_AmountAppliedToNextYear: [0],
        line32_AmountYouOwe: [0],
        line33_EstimatedTaxPenalty: [0],
        
        // Bank Account Information
        routingNumber: [''],
        accountNumber: [''],
        accountType: ['Checking'],
        
        // Third Party Designee
        hasThirdPartyDesignee: [false],
        thirdPartyDesigneeName: [''],
        thirdPartyDesigneePhone: [''],
        thirdPartyDesigneePIN: [''],
        
        // Signature Information
        taxpayerOccupation: [''],
        spouseOccupation: [''],
        taxpayerPhoneNumber: [''],
        taxpayerEmail: [''],
        
        // Preparer Information
        isSelfPrepared: [true],
        preparerName: [''],
        preparerPTIN: [''],
        preparerFirmName: [''],
        preparerFirmEIN: [''],
        preparerAddress: [''],
        preparerPhone: ['']
      }),
      scheduleA: this.fb.group({
        medicalExpenses: [0],
        stateAndLocalTaxes: [0],
        mortgageInterest: [0],
        charitableContributions: [0],
        totalItemizedDeductions: [0]
      }),
      scheduleC: this.fb.group({
        businessName: [''],
        grossReceipts: [0],
        totalExpenses: [0],
        netProfit: [0]
      }),
      w2Forms: this.fb.array([])
    });
  }

  get w2Forms() {
    return this.taxForm.get('w2Forms') as FormArray;
  }

  get dependents() {
    return this.taxForm.get('form1040.dependents') as FormArray;
  }

  setInputMode(mode: 'form' | 'json') {
    this.inputMode = mode;
    this.error = null;
    this.jsonError = null;
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      // Validate file type
      if (file.type !== 'application/json') {
        this.jsonError = 'Please upload a valid JSON file.';
        return;
      }

      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        this.jsonError = 'File size exceeds 5MB limit.';
        return;
      }

      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        try {
          const content = e.target?.result as string;
          this.jsonContent = content;
          
          // Validate and parse JSON
          const taxReturn = JSON.parse(content);
          this.populateFormFromJson(taxReturn);
          this.jsonError = null;
        } catch (error) {
          this.jsonError = 'Invalid JSON format. Please check your file.';
          console.error('JSON parsing error:', error);
        }
      };
      reader.onerror = () => {
        this.jsonError = 'Error reading file. Please try again.';
      };
      reader.readAsText(file);
    }
  }

  populateFormFromJson(taxReturn: TaxReturn) {
    try {
      // Clear existing arrays
      while (this.dependents.length) {
        this.dependents.removeAt(0);
      }
      while (this.w2Forms.length) {
        this.w2Forms.removeAt(0);
      }

      // Populate dependents if they exist
      if (taxReturn.form1040?.dependents && Array.isArray(taxReturn.form1040.dependents)) {
        taxReturn.form1040.dependents.forEach((dep: any) => {
          const dependent = this.fb.group({
            firstName: [dep.firstName || '', Validators.required],
            lastName: [dep.lastName || '', Validators.required],
            ssn: [dep.ssn || '', Validators.required],
            relationship: [dep.relationship || '', Validators.required],
            isQualifyingChildForChildTaxCredit: [dep.isQualifyingChildForChildTaxCredit || false],
            isQualifyingChildForOtherCredit: [dep.isQualifyingChildForOtherCredit || false]
          });
          this.dependents.push(dependent);
        });
      }

      // Populate W2 forms if they exist
      if (taxReturn.w2Forms && Array.isArray(taxReturn.w2Forms)) {
        taxReturn.w2Forms.forEach((w2: any) => {
          const w2Form = this.fb.group({
            employerName: [w2.employerName || '', Validators.required],
            employerEIN: [w2.employerEIN || '', Validators.required],
            wages: [w2.wages || 0, Validators.required],
            federalTaxWithheld: [w2.federalTaxWithheld || 0],
            socialSecurityWages: [w2.socialSecurityWages || 0]
          });
          this.w2Forms.push(w2Form);
        });
      }

      // Patch the form with the values
      this.taxForm.patchValue(taxReturn);
      
      this.jsonError = null;
      this.error = null;
    } catch (error) {
      this.jsonError = 'Error populating form from JSON. Please check the structure.';
      console.error('Form population error:', error);
    }
  }

  loadJsonFromTextarea() {
    try {
      if (!this.jsonContent.trim()) {
        this.jsonError = 'Please enter JSON content.';
        return;
      }

      const taxReturn = JSON.parse(this.jsonContent);
      this.populateFormFromJson(taxReturn);
      this.jsonError = null;
      
      // Switch to form view to see the populated data
      this.inputMode = 'form';
    } catch (error) {
      this.jsonError = 'Invalid JSON format. Please check your syntax.';
      console.error('JSON parsing error:', error);
    }
  }

  exportFormToJson() {
    const taxReturn: TaxReturn = this.taxForm.value;
    const jsonString = JSON.stringify(taxReturn, null, 2);
    
    // Create blob and download
    const blob = new Blob([jsonString], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `tax-return-${taxReturn.taxYear || 'export'}.json`;
    link.click();
    window.URL.revokeObjectURL(url);
  }

  downloadSampleJson() {
    const sample: TaxReturn = {
      taxpayerId: "123-45-6789",
      taxYear: 2024,
      filingStatus: "Single",
      totalIncome: 75000,
      totalDeductions: 13850,
      taxableIncome: 61150,
      totalTax: 8500,
      withholdingAmount: 9000,
      hasDependents: false,
      numberOfDependents: 0,
      form1040: {
        firstName: "John",
        lastName: "Doe",
        ssn: "123-45-6789",
        spouseFirstName: "",
        spouseLastName: "",
        spouseSSN: "",
        address: "123 Main Street",
        city: "Springfield",
        state: "IL",
        zipCode: "62701",
        foreignCountry: "",
        foreignProvince: "",
        foreignPostalCode: "",
        isPresidentialElectionCampaign: false,
        isSpousePresidentialElectionCampaign: false,
        canBeClaimedAsDependent: false,
        isSpouseItemizingDeductions: false,
        dependents: [],
        hasMoreThanFourDependents: false,
        line1_Wages: 75000,
        line2a_TaxExemptInterest: 0,
        line2b_TaxableInterest: 250,
        line3a_QualifiedDividends: 0,
        line3b_OrdinaryDividends: 0,
        line4a_IRADistributions: 0,
        line4b_TaxableIRADistributions: 0,
        line5a_PensionsAndAnnuities: 0,
        line5b_TaxablePensionsAndAnnuities: 0,
        line6a_SocialSecurityBenefits: 0,
        line6b_TaxableSocialSecurityBenefits: 0,
        line7_CapitalGainOrLoss: 0,
        line8_OtherIncome: 0,
        line9_TotalIncome: 75250,
        line10a_AdjustmentsToIncome: 0,
        line10b_CharitableContributions: 300,
        line10c_TotalAdjustments: 300,
        line11_AdjustedGrossIncome: 74950,
        line12_StandardOrItemizedDeduction: 13850,
        line12a_StandardDeduction: 13850,
        isItemizingDeductions: false,
        line13_QualifiedBusinessIncomeDeduction: 0,
        line14_TotalDeductions: 13850,
        line15_TaxableIncome: 61100,
        line16_Tax: 8500,
        line17_ScheduleTwo_AdditionalTax: 0,
        line18_TotalTax: 8500,
        line19_ChildTaxCredit: 0,
        line20_ScheduleThree_AdditionalCredits: 0,
        line21_TotalCredits: 0,
        line22_TaxAfterCredits: 8500,
        line23_OtherTaxes: 0,
        line24_TotalTax: 8500,
        line25a_FederalIncomeTaxWithheld: 9000,
        line25b_EarnedIncomeCredit: 0,
        line25c_AdditionalChildTaxCredit: 0,
        line25d_AmericanOpportunityCredit: 0,
        line26_RecoveryRebateCredit: 0,
        line27_OtherPaymentsAndCredits: 0,
        line28_TotalPayments: 9000,
        line29_Overpayment: 500,
        line30_AmountToBeRefunded: 500,
        line31_AmountAppliedToNextYear: 0,
        line32_AmountYouOwe: 0,
        line33_EstimatedTaxPenalty: 0,
        routingNumber: "",
        accountNumber: "",
        accountType: "Checking",
        hasThirdPartyDesignee: false,
        thirdPartyDesigneeName: "",
        thirdPartyDesigneePhone: "",
        thirdPartyDesigneePIN: "",
        taxpayerOccupation: "Software Engineer",
        spouseOccupation: "",
        taxpayerPhoneNumber: "555-1234",
        taxpayerEmail: "john.doe@example.com",
        isSelfPrepared: true,
        preparerName: "",
        preparerPTIN: "",
        preparerFirmName: "",
        preparerFirmEIN: "",
        preparerAddress: "",
        preparerPhone: ""
      },
      scheduleA: {
        medicalExpenses: 0,
        stateAndLocalTaxes: 0,
        mortgageInterest: 0,
        charitableContributions: 0,
        totalItemizedDeductions: 0
      },
      scheduleC: {
        businessName: "",
        grossReceipts: 0,
        totalExpenses: 0,
        netProfit: 0
      },
      w2Forms: [
        {
          employerName: "ABC Corporation",
          employerEIN: "12-3456789",
          wages: 75000,
          federalTaxWithheld: 9000,
          socialSecurityWages: 75000
        }
      ]
    };

    const jsonString = JSON.stringify(sample, null, 2);
    const blob = new Blob([jsonString], { type: 'application/json' });
    const url = window.URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = 'sample-tax-return.json';
    link.click();
    window.URL.revokeObjectURL(url);
  }

  addDependent() {
    const dependent = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      ssn: ['', Validators.required],
      relationship: ['', Validators.required],
      isQualifyingChildForChildTaxCredit: [false],
      isQualifyingChildForOtherCredit: [false]
    });
    this.dependents.push(dependent);
  }

  removeDependent(index: number) {
    this.dependents.removeAt(index);
  }

  addW2Form() {
    const w2Form = this.fb.group({
      employerName: ['', Validators.required],
      employerEIN: ['', Validators.required],
      wages: [0, Validators.required],
      federalTaxWithheld: [0],
      socialSecurityWages: [0]
    });
    this.w2Forms.push(w2Form);
  }

  removeW2Form(index: number) {
    this.w2Forms.removeAt(index);
  }

  setActiveTab(tab: string) {
    this.activeTab = tab;
  }

  onSubmit() {
    if (this.taxForm.valid) {
      this.loading = true;
      this.error = null;
      this.validationResult = null;

      const taxReturn: TaxReturn = this.taxForm.value;
      
      this.taxService.validateTaxReturn(taxReturn).subscribe({
        next: (result) => {
          this.validationResult = result;
          this.loading = false;
        },
        error: (err) => {
          this.error = 'An error occurred while validating the tax return.';
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      this.taxForm.markAllAsTouched();
      this.error = 'Please fill in all required fields.';
    }
  }
}
