using System.Threading;
using System.Threading.Tasks;
using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication.OAuth
{
    public interface IOAuthAuthenticator
    {
        Task<AuthenticationResult> RefreshAuthAsync(TargetUri targetUri, string refreshToken, CancellationToken cancellationToken);
        Task<AuthenticationResult> GetAuthAsync(TargetUri targetUri, TokenScope scope, CancellationToken cancellationToken);
        Task<AuthenticationResult> Authenticate(string restRootUrl, TargetUri targetUri, Credential credentials, TokenScope scope);
    }
}