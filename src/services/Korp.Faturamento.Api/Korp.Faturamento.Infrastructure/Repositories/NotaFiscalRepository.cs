using Korp.Faturamento.Domain.Entities;
using Korp.Faturamento.Domain.Repositories;
using Korp.Faturamento.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Korp.Faturamento.Infrastructure.Repositories;

public class NotaFiscalRepository : INotaFiscalRepository
{
    private readonly FaturamentoDbContext _context;

    public NotaFiscalRepository(FaturamentoDbContext context) => _context = context;

    public async Task<List<NotaFiscal>> GetAllAsync()
        => await _context.NotasFiscais
            .AsNoTracking()
            .Include(n => n.Itens)
            .OrderByDescending(n => n.Numero)
            .ToListAsync();

    public async Task<NotaFiscal?> GetByIdAsync(Guid id)
        => await _context.NotasFiscais
            .Include(n => n.Itens)
            .FirstOrDefaultAsync(n => n.Id == id);

    public async Task CreateAsync(NotaFiscal nota)
    {
        await _context.NotasFiscais.AddAsync(nota);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(NotaFiscal nota)
        => await _context.SaveChangesAsync();
}