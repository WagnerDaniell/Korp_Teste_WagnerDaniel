using Korp.Estoque.Domain.Exceptions;

namespace Korp.Estoque.Domain.Entities
{
    public class Produto
    {
        public Produto() { }

        public Guid Id { get; private set; }
        public string Codigo { get; private set; } = string.Empty;
        public string Descricao { get; private set; } = string.Empty;
        public int Saldo { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public Produto(Guid id, string codigo, string descricao, int saldo)
        {
            if (string.IsNullOrWhiteSpace(codigo))
                throw new ConflictException("Código inválido");

            if (saldo < 0)
                throw new ConflictException("Saldo inválido");

            Id = id;
            Codigo = codigo;
            Descricao = descricao;
            Saldo = saldo;
            CreatedAt = DateTime.UtcNow;
        }

        public void BaixarEstoque(int quantidade)
        {
            if (quantidade <= 0)
                throw new ConflictException("Quantidade inválida");

            if (Saldo < quantidade)
                throw new ConflictException("Saldo insuficiente");

            Saldo -= quantidade;
        }

        public void Atualizar(string descricao)
        {
            if (string.IsNullOrEmpty(descricao))
                throw new ConflictException("Descrição inválida");

            Descricao = descricao;
        }
    }
}
