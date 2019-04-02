using Microsoft.Alm.Authentication;

namespace GitHub.Authentication
{
    public interface IAuthenticationPrompts
    {
        bool CredentialModalPrompt(TargetUri targeturi, out string username, out string password);
        bool AuthenticationCodeModalPrompt(TargetUri targeturi, GitHubAuthenticationResultType resulttype, string username, out string authenticationcode);
    }
}