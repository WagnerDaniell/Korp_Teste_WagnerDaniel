import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { NotaFiscal, CreateNotaFiscalDto, ImprimirNotaDto } from '../models/nota-fiscal.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class NotaFiscalService {
  private http = inject(HttpClient);
  private base = `${environment.faturamentoApiUrl}/notas`;

  listar(): Observable<NotaFiscal[]> {
    return this.http.get<NotaFiscal[]>(this.base);
  }

  buscarPorId(id: string): Observable<NotaFiscal> {
    return this.http.get<NotaFiscal>(`${this.base}/${id}`);
  }

  criar(dto: CreateNotaFiscalDto): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(this.base, dto);
  }

  imprimir(id: string, dto: ImprimirNotaDto): Observable<NotaFiscal> {
    return this.http.post<NotaFiscal>(`${this.base}/${id}/imprimir`, dto);
  }
}