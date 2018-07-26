﻿/**** Git Credential Manager for Windows ****
 *
 * Copyright (c) Atlassian
 * All rights reserved.
 *
 * MIT License
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the """"Software""""), to deal
 * in the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN
 * AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE."
**/

using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Bitbucket.Authentication.BasicAuth;
using Atlassian.Bitbucket.Authentication.Rest;
using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication
{
    /// <summary>
    /// Implementation of <see cref="IAuthority"/> representing the Bitbucket APIs as the authority
    /// that can provide and validate credentials for Bitbucket.
    /// </summary>
    internal class Authority : Base, IAuthority
    {
        /// <summary>
        /// The root URL for Bitbucket REST API calls.
        /// </summary>
        public const string DefaultRestRoot = "https://api.bitbucket.org/";

        /// <summary>
        /// The maximum wait time for a network request before timing out
        /// </summary>
        public const int RequestTimeout = 15 * 1000; // 15 second limit

        /// <summary>
        /// Default constructor of the <see cref="Authority"/>. Allows the default Bitbucket REST URL
        /// to be overridden.
        /// </summary>
        /// <param name="restRootUrl">overriding root URL for REST API call.</param>
        public Authority(RuntimeContext context, string restRootUrl = null)
            : base(context)
        {
            _restRootUrl = restRootUrl ?? DefaultRestRoot;
        }

        private readonly string _restRootUrl;

        /// <inheritdoc/>
        public async Task<AuthenticationResult> AcquireToken(TargetUri targetUri, Credential credentials, AuthenticationResultType resultType, TokenScope scope)
        {
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));
            if (credentials is null)
                throw new ArgumentNullException(nameof(credentials));
            if (resultType < AuthenticationResultType.None || resultType > AuthenticationResultType.TwoFactor)
                throw new ArgumentOutOfRangeException(nameof(resultType));
            if (scope is null)
                throw new ArgumentNullException(nameof(scope));

            if (resultType == AuthenticationResultType.TwoFactor)
            {
                // A previous attempt to acquire a token failed in a way that suggests the user has
                // Bitbucket 2FA turned on. so attempt to run the OAuth dance...
                var oauth = new OAuth.OAuthAuthenticator(Context);
                try
                {
                    var result = await oauth.GetAuthAsync(targetUri, scope, CancellationToken.None);

                    if (!result.IsSuccess)
                    {
                        Trace.WriteLine($"oauth authentication failed");
                        return new AuthenticationResult(AuthenticationResultType.Failure);
                    }

                    // We got a toke but lets check to see the usernames match.
                    var restRootUri = new Uri(_restRootUrl);
                    var userResult = await (new RestClient(Context)).TryGetUser(targetUri, RequestTimeout, restRootUri, result.Token);

                    if (!userResult.IsSuccess)
                    {
                        Trace.WriteLine($"oauth user check failed");
                        return new AuthenticationResult(AuthenticationResultType.Failure);
                    }

                    if (!string.IsNullOrWhiteSpace(userResult.RemoteUsername) && !credentials.Username.Equals(userResult.RemoteUsername))
                    {
                        Trace.WriteLine($"Remote username [{userResult.RemoteUsername}] != [{credentials.Username}] supplied username");
                        // Make sure the 'real' username is returned.
                        return new AuthenticationResult(AuthenticationResultType.Success, result.Token, result.RefreshToken, userResult.RemoteUsername);
                    }

                    // Everything is hunky dory.
                    return result;
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"oauth authentication failed [{ex.Message}]");
                    return new AuthenticationResult(AuthenticationResultType.Failure);
                }
            }
            else
            {
                BasicAuthAuthenticator basicauth = new BasicAuthAuthenticator(Context);
                try
                {
                    var restRootUri = new Uri(_restRootUrl);
                    return await basicauth.GetAuthAsync(targetUri, scope, RequestTimeout, restRootUri, credentials);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"basic authentication failed [{ex.Message}]");
                    return new AuthenticationResult(AuthenticationResultType.Failure);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<AuthenticationResult> RefreshToken(TargetUri targetUri, string refreshToken)
        {
            // Refreshing is only an OAuth concept so use the OAuth tools
            OAuth.OAuthAuthenticator oauth = new OAuth.OAuthAuthenticator(Context);
            try
            {
                return await oauth.RefreshAuthAsync(targetUri, refreshToken, CancellationToken.None);
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"oauth refresh failed [{ex.Message}]");
                return new AuthenticationResult(AuthenticationResultType.Failure);
            }
        }

        /// <inheritdoc/>
        public async Task<bool> ValidateCredentials(TargetUri targetUri, string username, Credential credentials)
        {
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));
            if (credentials is null)
                throw new ArgumentNullException(nameof(credentials));

            // We don't know when the credentials arrive here if they are using OAuth or Basic Auth,
            // so we try both.

            // Try the simplest basic authentication first
            var authEncode = GetEncodedCredentials(username, credentials);
            if (await ValidateCredentials(targetUri, credentials))
            {
                return true;
            }

            // If the basic authentication test failed then try again as OAuth
            if (await ValidateCredentials(targetUri, new Token(credentials.Password, TokenType.BitbucketAccess)))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the HTTP encoded version of the Credentials secret
        /// </summary>
        private static string GetEncodedCredentials(string username, Credential credentials)
        {
            var user = string.IsNullOrWhiteSpace(username) ? credentials.Username : username;
            var password = credentials.Password;
            return GetEncodedCredentials(user, password);
        }

        /// <summary>
        /// Get the HTTP encoded version of the Credentials secret
        /// </summary>
        private static string GetEncodedCredentials(string user, string password)
        {
            string authString = string.Format("{0}:{1}", user, password);
            byte[] authBytes = Encoding.UTF8.GetBytes(authString);
            string authEncode = Convert.ToBase64String(authBytes);
            return authEncode;
        }

        /// <summary>
        /// Validate the provided credentials, made up of the username and the contents if the
        /// authHeader, by making a request to a known Bitbucket REST API resource. A 200/Success
        /// response indicates the credentials are valid. Any other response indicates they are not.
        /// </summary>
        /// <param name="targetUri">
        /// Contains the <see cref="HttpClientHandler"/> used when making the REST API request
        /// </param>
        /// <param name="authorization">
        /// The HTTP authentication header containing the password/access_token to validate
        /// </param>
        /// <returns>true if the credentials are valid, false otherwise.</returns>
        private async Task<bool> ValidateCredentials(TargetUri targetUri, Secret authorization)
        {
            BaseSecureStore.ValidateTargetUri(targetUri);

            Trace.WriteLine($"authentication type = '{authorization.GetType().Name}'.");

            var restRootUrl = new Uri(_restRootUrl);
            var result = await (new RestClient(Context)).TryGetUser(targetUri, RequestTimeout, restRootUrl, authorization);

            if (result.Type.Equals(AuthenticationResultType.Success))
            {
                Trace.WriteLine("credential validation succeeded");
                return true;
            }

            Trace.WriteLine("credential validation failed");
            return false;
        }
    }
}
