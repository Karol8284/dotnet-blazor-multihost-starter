namespace BlazorMultihostStarter.Client.Authentication;

public sealed class AuthenticationOperationException(string message, Exception innerException)
    : Exception(message, innerException)
{
}
