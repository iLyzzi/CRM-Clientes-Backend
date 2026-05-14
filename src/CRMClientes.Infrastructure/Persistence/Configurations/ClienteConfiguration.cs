using CRMClientes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CRMClientes.Infrastructure.Persistence.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("clientes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("id");
        builder.Property(c => c.Nome).HasColumnName("nome").IsRequired().HasMaxLength(150);
        builder.Property(c => c.Email).HasColumnName("email").IsRequired().HasMaxLength(200);
        builder.Property(c => c.Telefone).HasColumnName("telefone").IsRequired().HasMaxLength(30);
        builder.Property(c => c.Documento).HasColumnName("documento").IsRequired().HasMaxLength(20);
        builder.Property(c => c.Endereco).HasColumnName("endereco").HasMaxLength(300);
        builder.Property(c => c.Observacoes).HasColumnName("observacoes").HasMaxLength(1000);
        builder.Property(c => c.Ativo).HasColumnName("ativo").IsRequired();
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(c => c.CreatedByUserId).HasColumnName("created_by_user_id").IsRequired();

        builder.HasIndex(c => c.Documento);
        builder.HasIndex(c => c.Nome);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
