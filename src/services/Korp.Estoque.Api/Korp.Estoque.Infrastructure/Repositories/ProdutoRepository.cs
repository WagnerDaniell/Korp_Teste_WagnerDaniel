using Korp.Estoque.Domain.Entities;
using Korp.Estoque.Domain.Repositories;
using Korp.Estoque.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Korp.Estoque.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly EstoqueDbContext _context;

    public ProdutoRepository(EstoqueDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(Produto produto)
    {
        await _context.Produtos.AddAsync(produto);
        await _context.SaveChangesAsync();
    }

    public async Task<List<string>> GetCodigosExistentesAsync(List<string> codigos)
    {
        return await _context.Produtos
            .Where(p => codigos.Contains(p.Codigo))
            .Select(p => p.Codigo)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Produto?> GetByIdAsync(Guid id)
    {
        return await _context.Produtos.FindAsync(id);
    }

    public async Task<List<Produto>> GetAllAsync()
    {
        return await _context.Produtos
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Produto?> GetByCodigoAsync(string codigo)
    {
        return await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Codigo == codigo);
    }

    public async Task<List<Produto>> GetByCodigosForUpdateAsync(List<string> codigos)
    {
        var sql = """SELECT * FROM produtos WHERE "Codigo" = ANY(@codigos) ORDER BY "Codigo" FOR UPDATE""";
        var parameter = new NpgsqlParameter("codigos", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = codigos.ToArray()
        };

        return await _context.Produtos
            .FromSqlRaw(sql, parameter)
            .ToListAsync();
    }

    public async Task UpdateAsync(Produto produto)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto != null)
        {
            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync();
        }
    }
}