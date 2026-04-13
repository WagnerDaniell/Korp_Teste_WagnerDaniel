import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Produto, CreateProdutoDto, UpdateProdutoDto } from '../models/produto.model';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ProdutoService {
  private http = inject(HttpClient);
  private base = `${environment.estoqueApiUrl}/produtos`;

  listar(): Observable<Produto[]> {
    return this.http.get<Produto[]>(this.base);
  }

  buscarPorCodigo(codigo: string): Observable<Produto> {
    return this.http.get<Produto>(`${this.base}/${codigo}`);
  }

  criar(dto: CreateProdutoDto): Observable<Produto> {
    return this.http.post<Produto>(this.base, dto);
  }

  atualizar(id: string, dto: UpdateProdutoDto): Observable<Produto> {
    return this.http.put<Produto>(`${this.base}/${id}`, dto);
  }

  remover(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}