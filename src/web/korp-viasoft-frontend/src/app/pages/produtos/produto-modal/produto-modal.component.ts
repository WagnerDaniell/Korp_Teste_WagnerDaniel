import { Component, input, output, inject } from '@angular/core';
import { ReactiveFormsModule, FormGroup } from '@angular/forms';

@Component({
  selector: 'app-produto-modal',
  standalone: true,
  imports: [ReactiveFormsModule],
  template: `
    <div class="overlay" (click)="onClose.emit()">
      <div class="modal" (click)="$event.stopPropagation()">
        <div class="modal-head">
          <span class="modal-title">{{ editando() ? 'Editar Produto' : 'Novo Produto' }}</span>
          <button class="close-btn" (click)="onClose.emit()">×</button>
        </div>
        <form [formGroup]="form()" (ngSubmit)="onSave.emit()">
          <div class="modal-body">
            @if (erroModal()) { <div class="alert alert-error">⚠ {{ erroModal() }}</div> }
            
            @if (!editando()) {
              <div class="field">
                <label>Código *</label>
                <input formControlName="codigo" placeholder="Ex: PROD-001"
                       [class.invalid]="form().get('codigo')?.invalid && form().get('codigo')?.touched" />
              </div>
            }

            <div class="field">
              <label>Descrição *</label>
              <input formControlName="descricao" placeholder="Nome do produto"
                     [class.invalid]="form().get('descricao')?.invalid && form().get('descricao')?.touched" />
            </div>

            @if (!editando()) {
              <div class="field">
                <label>Saldo inicial *</label>
                <input type="number" formControlName="saldo" />
              </div>
            }
          </div>
          <div class="modal-foot">
            <button type="button" class="btn btn-ghost" (click)="onClose.emit()">Cancelar</button>
            <button type="submit" class="btn btn-primary" [disabled]="salvando() || form().invalid">
              {{ salvando() ? 'Salvando...' : 'Confirmar' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styleUrl: '../produtos.component.css'
})
export class ProdutoModalComponent {
  form = input.required<FormGroup>();
  editando = input<any>(null);
  salvando = input(false);
  erroModal = input('');

  onClose = output<void>();
  onSave = output<void>();
}