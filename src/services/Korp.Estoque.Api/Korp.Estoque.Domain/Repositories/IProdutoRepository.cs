using Korp.Estoque.Domain.Entities;

namespace Korp.Estoque.Domain.Repositories;

public interface IProdutoRepository
{
    Task CreateAsync(Produto produto);
    Task<Produto?> GetByIdAsync(Guid id);
    Task <List<Produto>> GetAllAsync();
    Task<Produto?> GetByCodigoAsync(string codigo);
    Task<List<string>> GetCodigosExistentesAsync(List<string> codigos); //No tracking
    Task<List<Produto>> GetByCodigosForUpdateAsync(List<string> codigos);
    Task UpdateAsync(Produto produto);
    Task DeleteAsync(Guid id);
}