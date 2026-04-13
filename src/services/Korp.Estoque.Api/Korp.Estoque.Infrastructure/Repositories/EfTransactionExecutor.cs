using Korp.Estoque.Domain.Repositories;
using Korp.Estoque.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Korp.Estoque.Infrastructure.Repositories;

public class EfTransactionExecutor : ITransactionExecutor
{
    private readonly EstoqueDbContext _context;

    public EfTransactionExecutor(EstoqueDbContext context) => _context = context;

    public async Task ExecuteAsync(Func<Task> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                await operation();
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
}