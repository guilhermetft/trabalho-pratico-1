using Locadora.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Locadora.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Fabricante> Fabricantes => Set<Fabricante>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Aluguel> Alugueis => Set<Aluguel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Fabricante>(e =>
        {
            e.ToTable("Fabricantes");
            e.HasKey(f => f.Id);
            e.Property(f => f.Nome).IsRequired().HasMaxLength(100);
            e.Property(f => f.PaisOrigem).IsRequired().HasMaxLength(60);
            e.HasIndex(f => f.Nome).IsUnique();
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.ToTable("Categorias", t => t.HasCheckConstraint("CK_Categorias_ValorDiariaBase", "[ValorDiariaBase] > 0"));
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).IsRequired().HasMaxLength(60);
            e.Property(c => c.Descricao).HasMaxLength(255);
            e.Property(c => c.ValorDiariaBase).HasPrecision(10, 2);
            e.HasIndex(c => c.Nome).IsUnique();
        });

        modelBuilder.Entity<Veiculo>(e =>
        {
            e.ToTable("Veiculos", t =>
            {
                t.HasCheckConstraint("CK_Veiculos_AnoFabricacao", "[AnoFabricacao] BETWEEN 1900 AND 2100");
                t.HasCheckConstraint("CK_Veiculos_Quilometragem", "[Quilometragem] >= 0");
            });
            e.HasKey(v => v.Id);
            e.Property(v => v.Modelo).IsRequired().HasMaxLength(100);
            e.Property(v => v.Placa).IsRequired().HasMaxLength(8);
            e.Property(v => v.Status).HasConversion<int>();
            e.HasIndex(v => v.Placa).IsUnique();

            e.HasOne(v => v.Fabricante)
                .WithMany(f => f.Veiculos)
                .HasForeignKey(v => v.FabricanteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(v => v.Categoria)
                .WithMany(c => c.Veiculos)
                .HasForeignKey(v => v.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(c => c.Id);
            e.Property(c => c.Nome).IsRequired().HasMaxLength(150);
            e.Property(c => c.Cpf).IsRequired().HasMaxLength(11).IsFixedLength();
            e.Property(c => c.Email).IsRequired().HasMaxLength(150);
            e.Property(c => c.Telefone).HasMaxLength(20);
            e.Property(c => c.Cnh).HasMaxLength(11);
            e.HasIndex(c => c.Cpf).IsUnique();
            e.HasIndex(c => c.Email).IsUnique();
        });

        modelBuilder.Entity<Aluguel>(e =>
        {
            e.ToTable("Alugueis", t =>
            {
                t.HasCheckConstraint("CK_Alugueis_Periodo", "[DataPrevistaDevolucao] >= [DataRetirada]");
                t.HasCheckConstraint("CK_Alugueis_Devolucao", "[DataDevolucao] IS NULL OR [DataDevolucao] >= [DataRetirada]");
                t.HasCheckConstraint("CK_Alugueis_KmFinal", "[KmFinal] IS NULL OR [KmFinal] >= [KmInicial]");
                t.HasCheckConstraint("CK_Alugueis_ValorDiaria", "[ValorDiaria] > 0");
                t.HasCheckConstraint("CK_Alugueis_ValorTotal", "[ValorTotal] IS NULL OR [ValorTotal] >= 0");
            });
            e.HasKey(a => a.Id);
            e.Property(a => a.ValorDiaria).HasPrecision(10, 2);
            e.Property(a => a.ValorTotal).HasPrecision(12, 2);

            e.HasOne(a => a.Cliente)
                .WithMany(c => c.Alugueis)
                .HasForeignKey(a => a.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(a => a.Veiculo)
                .WithMany(v => v.Alugueis)
                .HasForeignKey(a => a.VeiculoId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(a => new { a.VeiculoId, a.DataRetirada });
        });
    }
}
