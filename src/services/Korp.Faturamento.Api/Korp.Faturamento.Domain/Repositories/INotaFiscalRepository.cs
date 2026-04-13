using Korp.Faturamento.Domain.Entities;

namespace Korp.Faturamento.Domain.Repositories;

public interface INotaFiscalRepository
{
    Task<List<NotaFiscal>> GetAllAsync();
    Task<NotaFiscal?> GetByIdAsync(Guid id);
    Task CreateAsync(NotaFiscal nota);
    Task UpdateAsync(NotaFiscal nota);
}