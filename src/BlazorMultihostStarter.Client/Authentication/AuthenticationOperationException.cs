namespace BlazorMultihostStarter.Client.Authentication;

public sealed class AuthenticationOperationException(string message, Exception? innerException = null)
    : Exception(message, innerException)
{
}
