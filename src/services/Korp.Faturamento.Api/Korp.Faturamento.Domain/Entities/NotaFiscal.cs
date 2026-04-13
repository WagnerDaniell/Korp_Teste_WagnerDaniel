using Korp.Faturamento.Domain.Exceptions;

namespace Korp.Faturamento.Domain.Entities
{
    public enum StatusNota { Aberta, Fechada }

    public class NotaFiscal
    {
        protected NotaFiscal() { }

        public Guid Id { get; private set; }
        public int Numero { get; private set; }
        public StatusNota Status { get; private set; }
        public Guid? ImpressaoId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public IReadOnlyCollection<ItemNotaFiscal> Itens { get; private set; }

        public NotaFiscal(Guid id, List<ItemNotaFiscal> itens)
        {
            if (itens == null || itens.Count == 0)
                throw new ConflictException("A nota deve conter ao menos um item");

            Id = id;
            Numero = 0; // preenchido pelo sequence do banco sequence
            Status = StatusNota.Aberta;
            CreatedAt = DateTime.UtcNow;
            Itens = itens;
        }

        public void Fechar(Guid impressaoId)
        {
            if (Status != StatusNota.Aberta)
                throw new ConflictException("Apenas notas com status Aberta podem ser impressas");

            Status = StatusNota.Fechada;
            ImpressaoId = impressaoId;
        }

        public bool JaProcessada(Guid impressaoId) => ImpressaoId == impressaoId;

    }
}