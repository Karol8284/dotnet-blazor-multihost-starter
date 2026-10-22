namespace BlazorMultihostStarter.Client.Authentication;

public sealed class AuthenticationRequiredException()
    : Exception("Sign in is required to call the Tasks API.")
{
}
