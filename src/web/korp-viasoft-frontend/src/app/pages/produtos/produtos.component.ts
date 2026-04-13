import { Component, inject, OnInit, signal } from '@angular/core';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { ProdutoService } from '../../core/services/produto.service';
import { Produto } from '../../core/models/produto.model';
import { ProdutoTableComponent } from './produto-table/produto-table.component';
import { ProdutoModalComponent } from './produto-modal/produto-modal.component';
import { SidebarComponent } from '../../shared/components/sidebar.component';

@Component({
  selector: 'app-produtos',
  standalone: true,
  imports: [ReactiveFormsModule, ProdutoTableComponent, ProdutoModalComponent, SidebarComponent ],
  templateUrl: './produtos.component.html',
  styleUrl: './produtos.component.css',
})
export class ProdutosComponent implements OnInit {
  private service = inject(ProdutoService);
  private fb = inject(FormBuilder);

  produtos = signal<Produto[]>([]);
  carregando = signal(false);
  erro = signal('');
  erroModal = signal('');
  salvando = signal(false);
  removendoId = signal<string | null>(null);
  modalAberto = signal(false);
  editando = signal<Produto | null>(null);
  modalRemocao = signal(false);
  produtoParaRemover = signal<Produto | null>(null);

  form = this.fb.group({
    codigo:    ['', Validators.required],
    descricao: ['', Validators.required],
    saldo:     [0, [Validators.required, Validators.min(0)]],
  });

  ngOnInit() { this.carregar(); }

  carregar() {
    this.carregando.set(true);
    this.erro.set('');
    this.service.listar().subscribe({
      next:  d => { this.produtos.set(d); this.carregando.set(false); },
      error: e => { this.erro.set(e.mensagem); this.carregando.set(false); }
    });
  }

  saldoBadge(s: number): string {
    if (s === 0) return 'badge badge-zero';
    if (s <= 5)  return 'badge badge-low';
    return 'badge badge-ok';
  }

  abrirCriar() {
    this.editando.set(null);
    this.erroModal.set('');
    this.form.reset({ saldo: 0 });
    this.form.enable();
    this.modalAberto.set(true);
  }

  abrirEditar(p: Produto) {
    this.editando.set(p);
    this.erroModal.set('');
    this.form.patchValue({ descricao: p.descricao });
    this.form.get('codigo')?.disable();
    this.form.get('saldo')?.disable();
    this.modalAberto.set(true);
  }

  fecharModal() { this.modalAberto.set(false); }

  salvar() {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.salvando.set(true);
    this.erroModal.set('');
    const ed = this.editando();

    if (ed) {
      this.service.atualizar(ed.id, { descricao: this.form.value.descricao! }).subscribe({
        next:  () => { this.salvando.set(false); this.fecharModal(); this.carregar(); },
        error: e  => { this.salvando.set(false); this.erroModal.set(e.mensagem); }
      });
    } else {
      this.service.criar({
        codigo:    this.form.getRawValue().codigo!,
        descricao: this.form.value.descricao!,
        saldo:     this.form.getRawValue().saldo!,
      }).subscribe({
        next:  () => { this.salvando.set(false); this.fecharModal(); this.carregar(); },
        error: e  => { this.salvando.set(false); this.erroModal.set(e.mensagem); }
      });
    }
  }

  confirmarRemover(p: Produto) {
    this.produtoParaRemover.set(p);
    this.modalRemocao.set(true);
  }

  fecharRemocao() {
    this.modalRemocao.set(false);
    this.produtoParaRemover.set(null);
  }

  remover() {
    const p = this.produtoParaRemover();
    if (!p) return;
    this.removendoId.set(p.id);
    this.service.remover(p.id).subscribe({
      next:  () => { this.removendoId.set(null); this.fecharRemocao(); this.carregar(); },
      error: e  => { this.removendoId.set(null); this.erro.set(e.mensagem); this.fecharRemocao(); }
    });
  }
}