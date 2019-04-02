using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication
{
    public interface IAuthenticationPrompts
    {
        bool CredentialModalPrompt(string titleMessage, TargetUri targetUri, out string username, out string password);
        bool AuthenticationOAuthModalPrompt(string title, TargetUri targeturi, AuthenticationResultType resulttype, string username);
    }
}