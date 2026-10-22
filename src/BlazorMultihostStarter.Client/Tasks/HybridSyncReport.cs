namespace BlazorMultihostStarter.Client.Tasks;

public sealed record HybridSyncReport(int AppliedCount, int ConflictCount, int MissingCount);
