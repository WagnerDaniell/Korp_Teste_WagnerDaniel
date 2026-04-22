using Korp.Faturamento.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Korp.Faturamento.Infrastructure.Context;

public class FaturamentoDbContext : DbContext
{
    public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options) : base(options) { }

    public DbSet<NotaFiscal> NotasFiscais { get; set; }
    public DbSet<ItemNotaFiscal> ItensNotaFiscal { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasSequence<int>("notas_fiscais_numero_seq")
            .StartsAt(1)
            .IncrementsBy(1);

        modelBuilder.Entity<NotaFiscal>(entity =>
        {
            entity.ToTable("notas_fiscais");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Numero)
                .IsRequired()
                .HasDefaultValueSql("nextval('notas_fiscais_numero_seq')")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(20);

            entity.HasIndex(e => e.ImpressaoId)
                .IsUnique();

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("now()");

            entity.HasMany(e => e.Itens)
                .WithOne()
                .HasForeignKey(i => i.NotaFiscalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemNotaFiscal>(entity =>
        {
            entity.ToTable("itens_nota_fiscal");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.NotaFiscalId)
                .IsRequired();

            entity.Property(e => e.CodigoProduto)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Quantidade)
                .IsRequired();
        });
    }
}