using Xunit;

namespace IntegrationTests;

/// <summary>
/// Trait to mark tests that require Docker to be running.
/// Use: [Trait("Category", "RequiresDocker")]
/// Skip: dotnet test --filter "Category!=RequiresDocker"
/// </summary>
public static class TestCategories
{
    public const string RequiresDocker = "RequiresDocker";
    public const string Integration = "Integration";
    public const string Contract = "Contract";
    public const string Architecture = "Architecture";
}
