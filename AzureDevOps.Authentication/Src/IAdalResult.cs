namespace AzureDevOps.Authentication
{
    /// <summary>
    /// Contains the results of one token acquisition operation.
    /// </summary>
    public interface IAdalResult
    {
        /// <summary>
        /// Gets the Access Token requested.
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Gets the type of the Access Token returned.
        /// </summary>
        string AccessTokenType { get; }

        /// <summary>
        /// Gets the authority that has issued the token.
        /// </summary>
        string Authority { get; }

        /// <summary>
        /// Gets an identifier for the tenant the token was acquired from.
        /// <para/>
        /// This property will be null if tenant information is not returned by the service.
        /// </summary>
        string TenantId { get; }
    }
}