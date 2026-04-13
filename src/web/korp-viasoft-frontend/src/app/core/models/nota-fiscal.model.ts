export interface ItemNotaFiscal {
  id: string;
  codigoProduto: string;
  quantidade: number;
}

export interface NotaFiscal {
  id: string;
  numero: number;
  status: 'Aberta' | 'Fechada';
  createdAt: string;
  itens: ItemNotaFiscal[];
}

export interface CreateItemNotaDto {
  codigoProduto: string;
  quantidade: number;
}

export interface CreateNotaFiscalDto {
  itens: CreateItemNotaDto[];
}

export interface ImprimirNotaDto {
  impressaoId: string;
}