using Korp.Faturamento.Domain.Exceptions;

namespace Korp.Faturamento.Domain.Entities
{
    public class ItemNotaFiscal
    {
        protected ItemNotaFiscal() { }

        public Guid Id { get; private set; }
        public Guid NotaFiscalId { get; private set; }
        public string CodigoProduto { get; private set; }
        public int Quantidade { get; private set; }

        public ItemNotaFiscal(Guid id, string codigoProduto, int quantidade)
        {
            if (string.IsNullOrWhiteSpace(codigoProduto))
                throw new ConflictException("Código do produto inválido");

            if (quantidade <= 0)
                throw new ConflictException("Quantidade deve ser maior que zero");

            Id = id;
            CodigoProduto = codigoProduto;
            Quantidade = quantidade;
        }
    }
}