import { Component, inject, OnInit, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, FormArray, Validators } from '@angular/forms';
import { NotaFiscalService } from '../../core/services/nota-fiscal.service';
import { ProdutoService } from '../../core/services/produto.service';
import { NotaFiscal } from '../../core/models/nota-fiscal.model';
import { Produto } from '../../core/models/produto.model';
import { v4 as uuidv4 } from 'uuid';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import { NotaTableComponent } from './nota-table/nota-table.component';
import { NotaModalComponent } from './nota-modal/nota-modal.component';
import { SidebarComponent } from '../../shared/components/sidebar.component';

@Component({
  selector: 'app-notas-fiscais',
  standalone: true,
  imports: [ReactiveFormsModule, NotaTableComponent, NotaModalComponent, SidebarComponent],
  templateUrl: './notas-fiscais.component.html',
  styleUrl: './notas-fiscais.component.css',
})
export class NotasFiscaisComponent implements OnInit {
  private notaService    = inject(NotaFiscalService);
  private produtoService = inject(ProdutoService);
  private fb             = inject(FormBuilder);

  notas    = signal<NotaFiscal[]>([]);
  produtos = signal<Produto[]>([]);

  carregando         = signal(false);
  carregandoProdutos = signal(false);
  carregandoDetalhe  = signal(false);
  salvando           = signal(false);
  imprimindoId       = signal<string | null>(null);

  erro      = signal('');
  erroModal = signal('');

  modalAberto = signal(false);
  feedbackOk  = signal(false);
  detalheId   = signal<string | null>(null);
  notaDetalhe = signal<NotaFiscal | null>(null);

  form = this.fb.group({ itens: this.fb.array([this.novoItem()]) });
  get itensArr() { return this.form.get('itens') as FormArray; }

  novoItem() {
    return this.fb.group({
      codigoProduto: ['', Validators.required],
      quantidade:    [1, [Validators.required, Validators.min(1)]],
    });
  }

  ngOnInit() { this.carregar(); }

  carregar() {
    this.carregando.set(true);
    this.erro.set('');
    this.notaService.listar().subscribe({
      next:  d => { this.notas.set(d); this.carregando.set(false); },
      error: e => { this.erro.set(e.mensagem); this.carregando.set(false); }
    });
  }

  abrirCriar() {
    this.erroModal.set('');
    this.form.setControl('itens', this.fb.array([this.novoItem()]));
    this.modalAberto.set(true);
    this.carregandoProdutos.set(true);
    this.produtoService.listar().subscribe({
      next:  d => { this.produtos.set(d); this.carregandoProdutos.set(false); },
      error: () => this.carregandoProdutos.set(false)
    });
  }

  fecharModal() { this.modalAberto.set(false); }

  addItem()             { this.itensArr.push(this.novoItem()); }
  removeItem(i: number) { this.itensArr.removeAt(i); }

  criar() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.salvando.set(true);
    this.erroModal.set('');
    const itens = this.itensArr.value.map((i: any) => ({
      codigoProduto: i.codigoProduto,
      quantidade:    i.quantidade,
    }));
    this.notaService.criar({ itens }).subscribe({
      next:  () => { this.salvando.set(false); this.fecharModal(); this.carregar(); },
      error: e  => { this.salvando.set(false); this.erroModal.set(e.mensagem); }
    });
  }

  imprimir(nota: NotaFiscal) {
    this.imprimindoId.set(nota.id);
    this.erro.set('');

    const idUnicoParaIdempotencia = uuidv4();

    this.notaService.imprimir(nota.id, { impressaoId: idUnicoParaIdempotencia }).subscribe({
      next:  () => { this.imprimindoId.set(null); this.feedbackOk.set(true); this.carregar(); },
      error: e  => { this.imprimindoId.set(null); this.erro.set(e.mensagem); }
    });
  }

  toggleDetalhe(nota: NotaFiscal) {
    if (this.detalheId() === nota.id) { this.detalheId.set(null); return; }
    this.detalheId.set(nota.id);
    this.carregandoDetalhe.set(true);
    this.notaService.buscarPorId(nota.id).subscribe({
      next:  d => { this.notaDetalhe.set(d); this.carregandoDetalhe.set(false); },
      error: () => this.carregandoDetalhe.set(false)
    });
  }

  fecharDetalhe()  { this.detalheId.set(null); }
  fecharFeedback() { this.feedbackOk.set(false); }

  statusBadge(s: string) { return s === 'Aberta' ? 'badge badge-open' : 'badge badge-closed'; }

  fmtData(d: string) {
    if (!d) return '-';
    return format(new Date(d), "dd/MM/yyyy 'às' HH:mm", { locale: ptBR });
  }
}