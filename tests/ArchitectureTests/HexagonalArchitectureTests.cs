using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests;

/// <summary>
/// Architecture tests to ensure proper layer separation in Hexagonal Architecture.
/// These tests validate that dependencies flow in the correct direction:
/// Domain <- Application <- Infrastructure <- API
/// </summary>
public class HexagonalArchitectureTests
{
    // Assembly references for PropostaService
    private static readonly Assembly PropostaDomainAssembly = typeof(PropostaService.Domain.Entities.Proposal).Assembly;
    private static readonly Assembly PropostaApplicationAssembly = typeof(PropostaService.Application.DependencyInjection).Assembly;
    private static readonly Assembly PropostaInfrastructureAssembly = typeof(PropostaService.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly PropostaApiAssembly = typeof(PropostaService.Api.Program).Assembly;

    // Assembly references for ContratacaoService
    private static readonly Assembly ContratacaoDomainAssembly = typeof(ContratacaoService.Domain.Entities.Contract).Assembly;
    private static readonly Assembly ContratacaoApplicationAssembly = typeof(ContratacaoService.Application.DependencyInjection).Assembly;
    private static readonly Assembly ContratacaoInfrastructureAssembly = typeof(ContratacaoService.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly ContratacaoApiAssembly = typeof(ContratacaoService.Api.Program).Assembly;

    // Namespace constants
    private const string PropostaDomainNamespace = "PropostaService.Domain";
    private const string PropostaApplicationNamespace = "PropostaService.Application";
    private const string PropostaInfrastructureNamespace = "PropostaService.Infrastructure";
    private const string PropostaApiNamespace = "PropostaService.Api";

    private const string ContratacaoDomainNamespace = "ContratacaoService.Domain";
    private const string ContratacaoApplicationNamespace = "ContratacaoService.Application";
    private const string ContratacaoInfrastructureNamespace = "ContratacaoService.Infrastructure";
    private const string ContratacaoApiNamespace = "ContratacaoService.Api";

    #region PropostaService Domain Layer Tests

    [Fact]
    public void PropostaDomain_ShouldNotDependOn_ApplicationLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(PropostaApplicationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on Application layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PropostaDomain_ShouldNotDependOn_InfrastructureLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(PropostaInfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on Infrastructure layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PropostaDomain_ShouldNotDependOn_ApiLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(PropostaApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on API layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PropostaDomain_ShouldNotDependOn_EntityFramework()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on Entity Framework. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region PropostaService Application Layer Tests

    [Fact]
    public void PropostaApplication_ShouldNotDependOn_InfrastructureLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(PropostaInfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer should not depend on Infrastructure layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PropostaApplication_ShouldNotDependOn_ApiLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(PropostaApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer should not depend on API layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PropostaApplication_ShouldDependOn_DomainLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaApplicationAssembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .HaveDependencyOn(PropostaDomainNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "Application handlers should depend on Domain layer.");
    }

    #endregion

    #region ContratacaoService Domain Layer Tests

    [Fact]
    public void ContratacaoDomain_ShouldNotDependOn_ApplicationLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(ContratacaoDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ContratacaoApplicationNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on Application layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ContratacaoDomain_ShouldNotDependOn_InfrastructureLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(ContratacaoDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ContratacaoInfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on Infrastructure layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ContratacaoDomain_ShouldNotDependOn_ApiLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(ContratacaoDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(ContratacaoApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on API layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ContratacaoDomain_ShouldNotDependOn_EntityFramework()
    {
        // Arrange & Act
        var result = Types.InAssembly(ContratacaoDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Domain layer should not depend on Entity Framework. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region ContratacaoService Application Layer Tests

    [Fact]
    public void ContratacaoApplication_ShouldNotDependOn_InfrastructureLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(ContratacaoApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ContratacaoInfrastructureNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer should not depend on Infrastructure layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ContratacaoApplication_ShouldNotDependOn_ApiLayer()
    {
        // Arrange & Act
        var result = Types.InAssembly(ContratacaoApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(ContratacaoApiNamespace)
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            $"Application layer should not depend on API layer. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion
}
