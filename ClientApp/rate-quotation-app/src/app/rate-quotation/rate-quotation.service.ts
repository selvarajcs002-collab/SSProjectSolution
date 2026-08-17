import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RateQuotationService {
  private apiUrl = 'http://localhost:5219'; // Adjust this to match environment API URL if needed

  constructor(private http: HttpClient) {}

  createQuotation(data: any): Observable<any> {
    return this.http.post(`${this.apiUrl}/api/RateQuotation/create`, data);
  }

  downloadPdf(quotationId: number): Observable<Blob> {
    return this.http.post(
      `${this.apiUrl}/api/RateQuotation/download-pdf`,
      { quotationId: quotationId }, 
      { 
        responseType: 'blob',
        headers: {
          'Content-Type': 'application/json-patch+json',
          'accept': '*/*'
        }
      }
    );
  }
}
