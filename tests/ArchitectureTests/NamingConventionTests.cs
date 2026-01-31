using System.Reflection;
using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace ArchitectureTests;

/// <summary>
/// Tests to ensure proper naming conventions are followed throughout the codebase.
/// </summary>
public class NamingConventionTests
{
    private static readonly Assembly PropostaApplicationAssembly = typeof(PropostaService.Application.DependencyInjection).Assembly;
    private static readonly Assembly ContratacaoApplicationAssembly = typeof(ContratacaoService.Application.DependencyInjection).Assembly;
    private static readonly Assembly PropostaInfrastructureAssembly = typeof(PropostaService.Infrastructure.DependencyInjection).Assembly;
    private static readonly Assembly ContratacaoInfrastructureAssembly = typeof(ContratacaoService.Infrastructure.DependencyInjection).Assembly;

    #region Command and Query Naming Tests

    [Fact]
    public void PropostaCommands_ShouldEndWith_Command()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaApplicationAssembly)
            .That()
            .ResideInNamespaceContaining("Commands")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Command")
            .Or()
            .HaveNameEndingWith("CommandHandler")
            .Or()
            .HaveNameEndingWith("CommandValidator")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All command types should follow naming convention (Command, CommandHandler, CommandValidator). " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PropostaQueries_ShouldEndWith_Query()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaApplicationAssembly)
            .That()
            .ResideInNamespaceContaining("Queries")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Query")
            .Or()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All query types should follow naming convention (Query, QueryHandler). " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ContratacaoCommands_ShouldEndWith_Command()
    {
        // Arrange & Act
        var result = Types.InAssembly(ContratacaoApplicationAssembly)
            .That()
            .ResideInNamespaceContaining("Commands")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Command")
            .Or()
            .HaveNameEndingWith("CommandHandler")
            .Or()
            .HaveNameEndingWith("CommandValidator")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All command types should follow naming convention (Command, CommandHandler, CommandValidator). " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region Repository Naming Tests

    [Fact]
    public void PropostaRepositories_ShouldEndWith_Repository()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaInfrastructureAssembly)
            .That()
            .ResideInNamespaceContaining("Repositories")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All repository implementations should end with 'Repository'. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void ContratacaoRepositories_ShouldEndWith_Repository()
    {
        // Arrange & Act
        var result = Types.InAssembly(ContratacaoInfrastructureAssembly)
            .That()
            .ResideInNamespaceContaining("Repositories")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All repository implementations should end with 'Repository'. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion

    #region Handler Implementation Tests

    [Fact]
    public void PropostaCommandHandlers_ShouldImplement_IRequestHandler()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaApplicationAssembly)
            .That()
            .HaveNameEndingWith("CommandHandler")
            .Should()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All command handlers should implement IRequestHandler<,>. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    [Fact]
    public void PropostaQueryHandlers_ShouldImplement_IRequestHandler()
    {
        // Arrange & Act
        var result = Types.InAssembly(PropostaApplicationAssembly)
            .That()
            .HaveNameEndingWith("QueryHandler")
            .Should()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .GetResult();

        // Assert
        result.IsSuccessful.Should().BeTrue(
            "All query handlers should implement IRequestHandler<,>. " +
            $"Failing types: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
    }

    #endregion
}
