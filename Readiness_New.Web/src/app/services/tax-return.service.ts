import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { TaxReturn, ValidationResult } from '../models/tax-return.model';

@Injectable({
  providedIn: 'root'
})
export class TaxReturnService {
  private apiUrl = `${environment.apiUrl}/TaxReturnValidation`;

  constructor(private http: HttpClient) { }

  validateTaxReturn(taxReturn: TaxReturn): Observable<ValidationResult> {
    return this.http.post<ValidationResult>(`${this.apiUrl}/validate`, taxReturn);
  }

  getRules(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/rules`);
  }
}
