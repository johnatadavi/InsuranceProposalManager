using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PropostaService.Domain.Entities;
using PropostaService.Domain.Enums;
using PropostaService.Domain.ValueObjects;

namespace PropostaService.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for Proposal entity.
/// Maps domain model to database schema.
/// </summary>
public sealed class ProposalConfiguration : IEntityTypeConfiguration<Proposal>
{
    public void Configure(EntityTypeBuilder<Proposal> builder)
    {
        builder.ToTable("Proposals");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.ProposalNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(p => p.ProposalNumber)
            .IsUnique();

        // Value Object: CPF
        builder.OwnsOne(p => p.HolderCpf, cpfBuilder =>
        {
            cpfBuilder.Property(c => c.Value)
                .HasColumnName("HolderCpf")
                .HasMaxLength(11)
                .IsRequired();

            cpfBuilder.HasIndex(c => c.Value);
        });

        builder.Property(p => p.HolderName)
            .HasMaxLength(200)
            .IsRequired();

        // Value Object: Email
        builder.OwnsOne(p => p.HolderEmail, emailBuilder =>
        {
            emailBuilder.Property(e => e.Value)
                .HasColumnName("HolderEmail")
                .HasMaxLength(256)
                .IsRequired();
        });

        builder.Property(p => p.InsuranceType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Value Object: Money (Coverage)
        builder.OwnsOne(p => p.CoverageAmount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("CoverageAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("CoverageCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Value Object: Money (Premium)
        builder.OwnsOne(p => p.PremiumAmount, moneyBuilder =>
        {
            moneyBuilder.Property(m => m.Amount)
                .HasColumnName("PremiumAmount")
                .HasPrecision(18, 2)
                .IsRequired();

            moneyBuilder.Property(m => m.Currency)
                .HasColumnName("PremiumCurrency")
                .HasMaxLength(3)
                .IsRequired();
        });

        // Value Object: CoveragePeriod
        builder.OwnsOne(p => p.CoveragePeriod, periodBuilder =>
        {
            periodBuilder.Property(per => per.StartDate)
                .HasColumnName("CoverageStartDate")
                .IsRequired();

            periodBuilder.Property(per => per.EndDate)
                .HasColumnName("CoverageEndDate")
                .IsRequired();
        });

        builder.Property(p => p.Description)
            .HasMaxLength(2000);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(p => p.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        // Ignore domain events - they are transient
        builder.Ignore(p => p.DomainEvents);
    }
}
