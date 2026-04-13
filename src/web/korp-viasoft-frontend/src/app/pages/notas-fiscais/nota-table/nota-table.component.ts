import { Component, input, output } from '@angular/core';
import { NotaFiscal } from '../../../core/models/nota-fiscal.model';

@Component({
  selector: 'app-nota-table',
  standalone: true,
  templateUrl: './nota-table.component.html',
  styleUrl: '../notas-fiscais.component.css'
})
export class NotaTableComponent {
  notas = input.required<NotaFiscal[]>();
  detalheId = input<string | null>(null);
  notaDetalhe = input<NotaFiscal | null>(null);
  carregandoDetalhe = input(false);
  imprimindoId = input<string | null>(null);
  
  onToggleDetalhe = output<NotaFiscal>();
  onFecharDetalhe = output<void>();
  onImprimir = output<NotaFiscal>();

  statusBadge(s: string) { return s === 'Aberta' ? 'badge badge-open' : 'badge badge-closed'; }
  
  fmtData(d: string) {
    return d;
  }
}