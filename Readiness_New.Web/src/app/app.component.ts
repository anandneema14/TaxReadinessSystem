import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaxReturnFormComponent } from './components/tax-return-form/tax-return-form.component';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, TaxReturnFormComponent],
  template: `
    <nav class="navbar navbar-dark bg-dark">
      <div class="container-fluid">
        <span class="navbar-brand mb-0 h1">Tax Readiness Checker</span>
      </div>
    </nav>
    <main>
      <app-tax-return-form></app-tax-return-form>
    </main>
  `,
  styles: [`
    main {
      padding: 20px 0;
    }
  `]
})
export class AppComponent {}
