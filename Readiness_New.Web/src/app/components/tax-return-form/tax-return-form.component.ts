import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormArray } from '@angular/forms';
import { TaxReturnService } from '../../services/tax-return.service';
import { AuthService } from '../../services/auth.service';
import { TaxReturn, ValidationResult, ReadinessScore } from '../../models/tax-return.model';

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
  readinessScore: ReadinessScore | null = null;
  loading = false;
  error: string | null = null;
  uploadMode = false;
  selectedFile: File | null = null;

  constructor(
    private fb: FormBuilder, 
    private taxService: TaxReturnService,
    public authService: AuthService
  ) {
    this.taxForm = this.fb.group({
      taxpayerId: ['', Validators.required],
      taxYear: [2023, [Validators.required, Validators.min(2000), Validators.max(2025)]],
      filingStatus: ['Single', Validators.required],
      totalIncome: [0, Validators.required],
      totalDeductions: [0],
      taxableIncome: [0],
      totalTax: [0],
      withholdingAmount: [0],
      hasDependents: [false],
      numberOfDependents: [0],
      form1040: this.fb.group({
        name: ['', Validators.required],
        ssn: ['', Validators.required],
        address: [''],
        line1_Wages: [0],
        line2_Interest: [0],
        line3_Dividends: [0],
        line8_TotalIncome: [0],
        line9_StandardDeduction: [0],
        line11_TaxableIncome: [0],
        line16_TotalTax: [0],
        line24_TotalPayments: [0],
        line34_Refund: [0],
        line37_AmountYouOwe: [0]
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

  toggleMode() {
    this.uploadMode = !this.uploadMode;
    this.error = null;
    this.validationResult = null;
    this.readinessScore = null;
    this.selectedFile = null;
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file && file.type === 'application/json') {
      this.selectedFile = file;
      this.error = null;
    } else {
      this.selectedFile = null;
      this.error = 'Please select a valid JSON file.';
    }
  }

  processTaxReturn(taxReturn: TaxReturn) {
    this.loading = true;
    this.error = null;
    this.validationResult = null;
    this.readinessScore = null;

    this.taxService.validateTaxReturn(taxReturn).subscribe({
      next: (result) => {
        this.validationResult = result;
        if (result.isValid) {
          this.taxService.calculateReadinessScore(taxReturn).subscribe({
            next: (score) => {
              this.readinessScore = score;
              this.loading = false;
            },
            error: (err) => {
              this.error = 'An error occurred while calculating the readiness score.';
              this.loading = false;
            }
          });
        } else {
          this.loading = false;
        }
      },
      error: (err) => {
        this.error = 'An error occurred while validating the tax return.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  approveScore() {
    if (this.readinessScore) {
      this.loading = true;
      this.taxService.approveScore(this.readinessScore).subscribe({
        next: (updatedScore) => {
          this.readinessScore = updatedScore;
          this.loading = false;
        },
        error: (err) => {
          this.error = err.error || 'An error occurred while approving the score.';
          this.loading = false;
        }
      });
    }
  }

  onFileUpload() {
    if (!this.selectedFile) {
      this.error = 'Please select a file first.';
      return;
    }

    const reader = new FileReader();
    reader.onload = (e: any) => {
      try {
        const taxReturn: TaxReturn = JSON.parse(e.target.result);
        this.processTaxReturn(taxReturn);
      } catch (err) {
        this.error = 'Invalid JSON file format.';
        this.loading = false;
      }
    };
    reader.onerror = () => {
      this.error = 'Failed to read the file.';
      this.loading = false;
    };
    reader.readAsText(this.selectedFile);
  }

  onSubmit() {
    if (this.uploadMode) {
      this.onFileUpload();
      return;
    }

    if (this.taxForm.valid) {
      this.processTaxReturn(this.taxForm.value);
    } else {
      this.taxForm.markAllAsTouched();
    }
  }
}
