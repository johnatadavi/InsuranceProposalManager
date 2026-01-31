using ContratacaoService.Domain.Entities;
using ContratacaoService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContratacaoService.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Contract entity.
/// </summary>
public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("Contracts");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.ContractNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(c => c.ContractNumber)
            .IsUnique();

        builder.Property(c => c.ProposalId)
            .IsRequired();

        builder.HasIndex(c => c.ProposalId)
            .IsUnique();

        builder.Property(c => c.ProposalNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.HolderCpf)
            .HasMaxLength(11)
            .IsRequired();

        builder.HasIndex(c => c.HolderCpf);

        builder.Property(c => c.HolderName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.InsuranceType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.CoverageAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.PremiumAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(c => c.Currency)
            .HasMaxLength(3)
            .IsRequired();

        builder.Property(c => c.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(c => c.CancellationReason)
            .HasMaxLength(1000);

        builder.Property(c => c.ContractedAt)
            .IsRequired();

        // Ignore domain events - they are transient
        builder.Ignore(c => c.DomainEvents);
    }
}
