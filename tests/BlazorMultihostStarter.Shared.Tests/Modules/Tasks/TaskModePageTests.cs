using BlazorMultihostStarter.Client.Authentication;
using BlazorMultihostStarter.Client.Tasks;
using BlazorMultihostStarter.Contracts.Tasks;
using BlazorMultihostStarter.Shared.Modules.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorMultihostStarter.Shared.Tests.Modules.Tasks;

public sealed class TaskModePageTests
{
    [Fact]
    public void Client_local_page_loads_and_creates_without_an_authentication_session()
    {
        using var context = new BunitContext();
        var localTasks = new FakeLocalTasksClient();
        context.Services.AddSingleton<ILocalTasksClient>(localTasks);

        var cut = context.Render<LocalTasksPage>();
        cut.WaitForAssertion(() => Assert.Contains("No local tasks yet", cut.Markup));

        cut.Find("#local-task-title").Input("Keep on device");
        cut.Find(".mode-form").Submit();

        cut.WaitForAssertion(() => Assert.Contains("Keep on device", cut.Markup));
        Assert.Single(localTasks.Tasks);
    }

    [Fact]
    public void Offline_page_keeps_pending_data_visible_until_sign_in()
    {
        using var context = new BunitContext();
        var localTask = new TaskResponse(
            Guid.NewGuid(),
            "Queued while offline",
            false,
            DateTimeOffset.UtcNow,
            null,
            0);
        context.Services.AddSingleton<IHybridTasksClient>(
            new FakeHybridTasksClient(new LocalTaskRecord(localTask, 0, LocalTaskSyncState.Pending)));
        context.Services.AddSingleton<IUserAuthenticationSession>(new FakeAuthenticationSession());

        var cut = context.Render<OfflineTasksPage>();

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("Queued while offline", cut.Markup);
            Assert.Contains("Sign in to sync", cut.Markup);
        });
        Assert.True(cut.Find(".mode-list-heading button").HasAttribute("disabled"));
    }

    private sealed class FakeLocalTasksClient : ILocalTasksClient
    {
        public List<LocalTaskRecord> Tasks { get; } = [];

        public Task<IReadOnlyList<LocalTaskRecord>> ListAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<LocalTaskRecord>>(Tasks.ToArray());

        public Task<TaskResponse> CreateAsync(
            string? title,
            CancellationToken cancellationToken = default)
        {
            var task = new TaskResponse(Guid.NewGuid(), title ?? string.Empty, false, DateTimeOffset.UtcNow, null, 0);
            Tasks.Add(new LocalTaskRecord(task, 0, LocalTaskSyncState.LocalOnly));
            return Task.FromResult(task);
        }

        public Task<TaskResponse> CompleteAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TaskResponse> RenameAsync(
            Guid id,
            string? title,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
    }

    private sealed class FakeHybridTasksClient(LocalTaskRecord record) : IHybridTasksClient
    {
        public Task<IReadOnlyList<LocalTaskRecord>> ListAsync(
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<LocalTaskRecord>>([record]);

        public Task<TaskResponse> CreateAsync(
            string? title,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TaskResponse> CompleteAsync(
            Guid id,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<TaskResponse> RenameAsync(
            Guid id,
            string? title,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();

        public Task<HybridSyncReport> SyncPendingAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(new HybridSyncReport(0, 0, 0));

        public Task ResolveConflictAsync(Guid id, bool keepLocal, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task RemoveMissingAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class FakeAuthenticationSession : IUserAuthenticationSession
    {
        public Task<bool> IsSignedInAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(false);

        public Task SignInAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task SignOutAsync(CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
