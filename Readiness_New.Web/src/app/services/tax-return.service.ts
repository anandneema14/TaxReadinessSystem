import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { TaxReturn, ValidationResult, ReadinessScore } from '../models/tax-return.model';

@Injectable({
  providedIn: 'root'
})
export class TaxReturnService {
  private apiUrl = `${environment.apiUrl}/TaxReturnValidation`;
  private readinessUrl = `${environment.apiUrl}/Readiness`;

  constructor(private http: HttpClient) { }

  validateTaxReturn(taxReturn: TaxReturn): Observable<ValidationResult> {
    return this.http.post<ValidationResult>(`${this.apiUrl}/validate`, taxReturn);
  }

  calculateReadinessScore(taxReturn: TaxReturn): Observable<ReadinessScore> {
    return this.http.post<ReadinessScore>(`${this.readinessUrl}/score`, taxReturn);
  }

  approveScore(score: ReadinessScore): Observable<ReadinessScore> {
    return this.http.post<ReadinessScore>(`${this.readinessUrl}/approve`, score);
  }

  getRules(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/rules`);
  }
}
