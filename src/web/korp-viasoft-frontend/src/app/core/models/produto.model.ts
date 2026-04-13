export interface Produto {
  id: string;
  codigo: string;
  descricao: string;
  saldo: number;
  createdAt: string;
}

export interface CreateProdutoDto {
  codigo: string;
  descricao: string;
  saldo: number;
}

export interface UpdateProdutoDto {
  descricao: string;
}