using Korp.Estoque.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Korp.Estoque.Infrastructure.Context;

public class EstoqueDbContext : DbContext
{
    public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("produtos");

            entity.HasKey(p => p.Id);

            entity.Property(p => p.Codigo)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Descricao)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Saldo)
                .IsRequired();

            // Garantir que o código do produto seja único no banco
            entity.HasIndex(p => p.Codigo).IsUnique();
        });
    }
}