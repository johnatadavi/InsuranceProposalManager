using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests;

/// <summary>
/// Tests to ensure Domain entities do not have database-specific attributes.
/// Domain should be persistence-ignorant.
/// </summary>
public class DomainPersistenceIgnoranceTests
{
    private static readonly Assembly PropostaDomainAssembly = typeof(PropostaService.Domain.Entities.Proposal).Assembly;
    private static readonly Assembly ContratacaoDomainAssembly = typeof(ContratacaoService.Domain.Entities.Contract).Assembly;

    // Database-related attribute namespaces to check against
    private static readonly string[] DatabaseAttributeNamespaces = new[]
    {
        "System.ComponentModel.DataAnnotations.Schema",  // [Table], [Column], [ForeignKey]
        "System.ComponentModel.DataAnnotations",          // [Key], [Required], [MaxLength]
        "Microsoft.EntityFrameworkCore"                   // EF Core attributes
    };

    #region PropostaService Domain Tests

    [Fact]
    public void PropostaDomain_Entities_ShouldNotHave_TableAttribute()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaDomainAssembly)
            .That()
            .ResideInNamespace("PropostaService.Domain.Entities")
            .ShouldNot()
            .HaveDependencyOn("System.ComponentModel.DataAnnotations.Schema")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain entities should not have [Table], [Column], or other database schema attributes. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PropostaDomain_Entities_ShouldNotHave_KeyAttribute()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaDomainAssembly)
            .That()
            .ResideInNamespace("PropostaService.Domain.Entities")
            .ShouldNot()
            .HaveDependencyOn("System.ComponentModel.DataAnnotations")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain entities should not use [Key], [Required], or other data annotation attributes. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PropostaDomain_ValueObjects_ShouldNotHave_DatabaseAttributes()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaDomainAssembly)
            .That()
            .ResideInNamespace("PropostaService.Domain.ValueObjects")
            .ShouldNot()
            .HaveDependencyOn("System.ComponentModel.DataAnnotations.Schema")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain value objects should not have database attributes. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region ContratacaoService Domain Tests

    [Fact]
    public void ContratacaoDomain_Entities_ShouldNotHave_TableAttribute()
    {
        // Arrange & Act
        var result = Types.InAssembly(ContratacaoDomainAssembly)
            .That()
            .ResideInNamespace("ContratacaoService.Domain.Entities")
            .ShouldNot()
            .HaveDependencyOn("System.ComponentModel.DataAnnotations.Schema")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain entities should not have [Table], [Column], or other database schema attributes. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ContratacaoDomain_Entities_ShouldNotHave_KeyAttribute()
    {
        // Arrange & Act
        var result = Types.InAssembly(ContratacaoDomainAssembly)
            .That()
            .ResideInNamespace("ContratacaoService.Domain.Entities")
            .ShouldNot()
            .HaveDependencyOn("System.ComponentModel.DataAnnotations")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Domain entities should not use [Key], [Required], or other data annotation attributes. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion
}
