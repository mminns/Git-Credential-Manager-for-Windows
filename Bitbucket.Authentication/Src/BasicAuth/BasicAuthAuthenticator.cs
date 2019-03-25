﻿using System;
using System.Text;
using System.Threading.Tasks;
using Atlassian.Bitbucket.Authentication.Rest;
using Microsoft.Alm.Authentication;
using Microsoft.Alm.Authentication.Git;

namespace Atlassian.Bitbucket.Authentication.BasicAuth
{
    /// <summary>
    ///     Provides the functionality for validating basic auth credentials with Bitbucket.org
    /// </summary>
    public class BasicAuthAuthenticator : Base
    {
        public BasicAuthAuthenticator(RuntimeContext context)
            : base(context)
        { }

        public async Task<AuthenticationResult> GetAuthAsync(TargetUri targetUri, TokenScope scope, int requestTimeout,
            Uri restRootUrl, Credential credentials)
        {
            if (Rest.Cloud.RestClient.IsAcceptableUri(targetUri))
            {
                return await GetCloudAuthAsync(targetUri, scope, requestTimeout, restRootUrl, credentials);
            }
            else
            {
                return await GetServerAuthAsync(targetUri, scope, requestTimeout, restRootUrl, credentials);
            }

        }

        private async Task<AuthenticationResult> GetServerAuthAsync(TargetUri targetUri, TokenScope scope, int requestTimeout, Uri restRootUrl, Credential credentials)
        {
            // Use the provided username and password and attempt a basic authentication request to a known REST API resource.
            var result = await (new Rest.Server.RestClient(Context)).TryGetUser(targetUri, requestTimeout, restRootUrl, credentials);

            if (result.Type.Equals(AuthenticationResultType.Success))
            {
                // Success with username/password indicates 2FA is not on so the 'token' is actually
                // the password if we had a successful call then the password is good.
                var token = new Token(credentials.Password, TokenType.Personal);
                if (!string.IsNullOrWhiteSpace(result.RemoteUsername) && !credentials.Username.Equals(result.RemoteUsername))
                {
                    Trace.WriteLine($"remote username [{result.RemoteUsername}] != [{credentials.Username}] supplied username");
                    return new AuthenticationResult(AuthenticationResultType.Success, token, result.RemoteUsername);
                }

                return new AuthenticationResult(AuthenticationResultType.Success, token);
            }

            Trace.WriteLine("authentication failed");
            return result;
        }

        public async Task<AuthenticationResult> GetCloudAuthAsync(TargetUri targetUri, TokenScope scope, int requestTimeout,
            Uri restRootUrl, Credential credentials)
        {
            // Use the provided username and password and attempt a basic authentication request to a known REST API resource.
            var result = await ( new Rest.Cloud.RestClient(Context)).TryGetUser(targetUri, requestTimeout, restRootUrl, credentials);

            if (result.Type.Equals(AuthenticationResultType.Success))
            {
                // Success with username/password indicates 2FA is not on so the 'token' is actually
                // the password if we had a successful call then the password is good.
                var token = new Token(credentials.Password, TokenType.Personal);
                if (!string.IsNullOrWhiteSpace(result.RemoteUsername) && !credentials.Username.Equals(result.RemoteUsername))
                {
                    Trace.WriteLine($"remote username [{result.RemoteUsername}] != [{credentials.Username}] supplied username");
                    return new AuthenticationResult(AuthenticationResultType.Success, token, result.RemoteUsername);
                }

                return new AuthenticationResult(AuthenticationResultType.Success, token);
            }

            Trace.WriteLine("authentication failed");
            return result;
        }

        public async Task<AuthenticationResult> Authenticate(string restRootUrl, TargetUri targetUri, Credential credentials, TokenScope scope, int RequestTimeout)
        {
            var restRootUri = new Uri(restRootUrl);
            return await GetAuthAsync(targetUri, scope, RequestTimeout, restRootUri, credentials);
        }
    }
}
