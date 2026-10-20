using BlazorMultihostStarter.Domain.Modules.Tasks;

namespace BlazorMultihostStarter.Domain.Tests.Architecture;

public sealed class DependencyBoundaryTests
{
    [Fact]
    public void Domain_does_not_reference_host_or_persistence_frameworks()
    {
        var references = typeof(TaskItem).Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.DoesNotContain(references, name => name?.StartsWith("Microsoft.EntityFrameworkCore", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(references, name => name?.StartsWith("Microsoft.AspNetCore", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(references, name => name?.StartsWith("Microsoft.Maui", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(references, name => name?.Contains("Infrastructure", StringComparison.Ordinal) == true);
    }
}
