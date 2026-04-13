import { Component, input, output } from '@angular/core';
import { ReactiveFormsModule, FormGroup, FormArray } from '@angular/forms';
import { Produto } from '../../../core/models/produto.model';

@Component({
  selector: 'app-nota-modal',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './nota-modal.component.html',
  styleUrl: '../notas-fiscais.component.css'
})
export class NotaModalComponent {
  form = input.required<FormGroup>();
  produtos = input.required<Produto[]>();
  salvando = input(false);
  carregandoProdutos = input(false);
  erroModal = input('');

  onClose = output<void>();
  onSave = output<void>();
  onAddItem = output<void>();
  onRemoveItem = output<number>();

  get itensArr() { return this.form().get('itens') as FormArray; }
}