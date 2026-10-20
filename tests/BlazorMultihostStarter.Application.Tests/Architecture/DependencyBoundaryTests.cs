using BlazorMultihostStarter.Application.Modules.Tasks.CreateTask;

namespace BlazorMultihostStarter.Application.Tests.Architecture;

public sealed class DependencyBoundaryTests
{
    [Fact]
    public void Application_does_not_reference_adapters_or_ui_hosts()
    {
        var references = typeof(CreateTaskHandler).Assembly.GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .ToArray();

        Assert.DoesNotContain(references, name => name == "BlazorMultihostStarter.Infrastructure");
        Assert.DoesNotContain(references, name => name?.StartsWith("BlazorMultihostStarter.Maui", StringComparison.Ordinal) == true);
        Assert.DoesNotContain(references, name => name?.StartsWith("BlazorMultihostStarter.Web", StringComparison.Ordinal) == true);
    }
}
