using System;
using System.Threading.Tasks;

namespace AzureDevOps.Authentication
{
    public interface IAdal : Microsoft.Alm.Authentication.IRuntimeService
    {
        /// <summary>
        /// Initiates an interactive authentication experience.
        /// <para/>
        /// Returns an authentication result which contains an access token and other relevant information.
        /// </summary>
        /// <param name="authorityHostUrl">Address of the authority to issue token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        /// <param name="redirectUri">Address to return to upon receiving a response from the authority.</param>
        /// <param name="extraQueryParameters">
        /// This parameter will be appended as is to the query string in the HTTP authentication request to the authority.
        /// <para/>
        /// The parameter can be null.
        /// </param>
        Task<IAdalResult> AcquireTokenAsync(string authorityHostUrl, string resource, string clientId, Uri redirectUri, string extraQueryParameters);

        /// <summary>
        /// Initiates a non-interactive authentication experience.
        /// <para/>
        /// Returns an authentication result which contains an access token and other relevant information.
        /// </summary>
        /// <param name="authorityHostUrl">Address of the authority to issue token.</param>
        /// <param name="resource">Identifier of the target resource that is the recipient of the requested token.</param>
        /// <param name="clientId">Identifier of the client requesting the token.</param>
        Task<IAdalResult> AcquireTokenAsync(string authorityHostUrl, string resource, string client);
    }
}