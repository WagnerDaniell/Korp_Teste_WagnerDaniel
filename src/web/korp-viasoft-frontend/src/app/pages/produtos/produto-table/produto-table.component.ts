import { Component, input, output } from '@angular/core';
import { Produto } from '../../../core/models/produto.model';

@Component({
  selector: 'app-produto-table',
  standalone: true,
  template: `
    <div class="table-wrap">
      <table>
        <thead>
          <tr>
            <th>Código</th>
            <th>Descrição</th>
            <th>Saldo</th>
            <th class="col-actions">Ações</th>
          </tr>
        </thead>
        <tbody>
          @for (p of produtos(); track p.id) {
            <tr>
              <td><span class="chip">{{ p.codigo }}</span></td>
              <td class="td-desc">{{ p.descricao }}</td>
              <td>
                <span class="badge" [class]="saldoBadge(p.saldo)">{{ p.saldo }}</span>
              </td>
              <td class="col-actions">
                <div class="actions">
                  <button class="icon-btn" title="Editar" (click)="onEdit.emit(p)">✎</button>
                  <button class="icon-btn danger" title="Remover" 
                          [disabled]="removendoId() === p.id" 
                          (click)="onDelete.emit(p)">
                    @if (removendoId() === p.id) { <span class="spin-sm"></span> } @else { ✕ }
                  </button>
                </div>
              </td>
            </tr>
          }
        </tbody>
      </table>
    </div>
  `,
  styleUrl: '../produtos.component.css'
})
export class ProdutoTableComponent {
  produtos = input.required<Produto[]>();
  removendoId = input<string | null>(null);

  onEdit = output<Produto>();
  onDelete = output<Produto>();

  saldoBadge(s: number): string {
    if (s === 0) return 'badge badge-zero';
    if (s <= 5)  return 'badge badge-low';
    return 'badge badge-ok';
  }
}