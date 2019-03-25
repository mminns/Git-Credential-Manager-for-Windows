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
using System.Threading.Tasks;
using Microsoft.Alm.Authentication;
using static System.StringComparer;

namespace Atlassian.Bitbucket.Authentication
{
    /// <summary>
    /// Extension of <see cref="BaseAuthentication"/> implementing Bitbucket's
    /// <see cref="IAuthentication"/> and providing functionality to manage credentials for Bitbucket
    /// hosting service.
    /// </summary>
    public class Authentication : BaseAuthentication, IAuthentication
    {
        public const string BitbucketBaseUrlHost = "bitbucket.org";
        private const string RefreshTokenSuffix = "/refresh_token";

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="targetUri"></param>
        /// <param name="personalAccessTokenStore">where to store validated credentials</param>
        /// <param name="acquireCredentialsCallback">
        /// what to call to promot the user for Basic Auth credentials
        /// </param>
        /// <param name="acquireAuthenticationOAuthCallback">
        /// what to call to prompt the user to run the OAuth process
        /// </param>
        /// <param name="authority">a predefined instance of <see cref="IAuthority"/>. Primarily used for Mock testing</param>
        public Authentication(
            RuntimeContext context,
            TargetUri targetUri,
            ICredentialStore personalAccessTokenStore,
            AcquireCredentialsDelegate acquireCredentialsCallback,
            AcquireAuthenticationOAuthDelegate acquireAuthenticationOAuthCallback,
            string bbsConsumerKey,
            string bbsConsumerSecret,
            IAuthority authority = null)
            : base(context)
        {
            if (personalAccessTokenStore == null)
                throw new ArgumentNullException(nameof(personalAccessTokenStore), $"The parameter `{nameof(personalAccessTokenStore)}` is null or invalid.");

            PersonalAccessTokenStore = personalAccessTokenStore;

            BitbucketAuthority = authority ?? new Authority(context, targetUri);
            TokenScope = TokenScope.SnippetWrite | TokenScope.RepositoryWrite;

            BbSConsumerKey = bbsConsumerKey;
            BbSConsumerSecret = bbsConsumerSecret;

            AcquireCredentialsCallback = acquireCredentialsCallback;
            AcquireAuthenticationOAuthCallback = acquireAuthenticationOAuthCallback;
        }

        /// <summary>
        /// The desired scope of the authentication token to be requested.
        /// </summary>
        public readonly TokenScope TokenScope;

        public ICredentialStore PersonalAccessTokenStore { get; }

        internal AcquireCredentialsDelegate AcquireCredentialsCallback { get; set; }

        internal AcquireAuthenticationOAuthDelegate AcquireAuthenticationOAuthCallback { get; set; }

        internal AuthenticationResultDelegate AuthenticationResultCallback { get; set; }

        /// <summary>
        /// Deletes a `<see cref="Credential"/>` from the storage used by the authentication object.
        /// </summary>
        /// <param name="targetUri">The uniform resource indicator used to uniquely identify the credentials.</param>
        public override Task<bool> DeleteCredentials(TargetUri targetUri)
            => DeleteCredentials(targetUri, null);

        /// <inheritdoc/>
        public override async Task<bool> DeleteCredentials(TargetUri targetUri, string username)
        {
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));

            Trace.WriteLine($"Deleting Bitbucket Credentials for {targetUri.QueryUri}");

            Credential credentials = null;
            if ((credentials = await PersonalAccessTokenStore.ReadCredentials(targetUri)) != null)
            {
                // Try to delete the credentials for the explicit target uri first.
                await PersonalAccessTokenStore.DeleteCredentials(targetUri);
                Trace.WriteLine($"host credentials deleted for {targetUri.QueryUri}");
            }

            // Tidy up and delete any related refresh tokens.
            var refreshTargetUri = GetRefreshTokenTargetUri(targetUri);
            if ((credentials = await PersonalAccessTokenStore.ReadCredentials(refreshTargetUri)) != null)
            {
                // Try to delete the credentials for the explicit target uri first.
                await PersonalAccessTokenStore.DeleteCredentials(refreshTargetUri);
                Trace.WriteLine($"host refresh credentials deleted for {refreshTargetUri.QueryUri}");
            }

            // If we deleted per user then we should try and delete the host level credentials too if
            // they match the username.
            if (targetUri.ContainsUserInfo)
            {
                var hostTargetUri = new TargetUri(targetUri.ToString(false, true, true));
                var hostCredentials = await GetCredentials(hostTargetUri);
                var encodedUsername = Uri.EscapeDataString(targetUri.UserInfo);
                if (encodedUsername != username)
                {
                    Trace.WriteLine($"username {username} != targetUri userInfo {encodedUsername}");
                }

                if (hostCredentials != null && hostCredentials.Username.Equals(encodedUsername))
                {
                    await DeleteCredentials(hostTargetUri, username);
                }
            }

            return true;
        }

        /// <summary>
        /// Generate a new <see cref="TargetUri"/> to be used as the key when storing the
        /// refresh_tokne alongside the access_token.
        /// </summary>
        /// <param name="targetUri">contains Authority URL etc used for storing the sibling access_token</param>
        /// <returns></returns>
        private static TargetUri GetRefreshTokenTargetUri(TargetUri targetUri)
        {
            var uri = new Uri(targetUri.QueryUri, RefreshTokenSuffix);
            return new TargetUri(uri);
        }

        /// <inheritdoc/>
        public async Task<Credential> GetCredentials(TargetUri targetUri, string username)
        {
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));

            if (string.IsNullOrWhiteSpace(username) || targetUri.ContainsUserInfo)
                return await GetCredentials(targetUri);

            return await GetCredentials(targetUri.GetPerUserTargetUri(username));
        }

        /// <summary>
        /// Gets a <see cref="Credential"/> from the storage used by the authentication object.
        /// <para/>
        /// Returns a `<see cref="Credential"/>` if successful; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="targetUri">The uniform resource indicator used to uniquely identify the credentials.</param>
        public override async Task<Credential> GetCredentials(TargetUri targetUri)
        {
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));

            Credential credentials = null;

            if ((credentials = await PersonalAccessTokenStore.ReadCredentials(targetUri)) != null)
            {
                Trace.WriteLine("successfully retrieved stored credentials, updating credential cache");
                return credentials;
            }

            // Try for a refresh token.
            var refreshCredentials = await PersonalAccessTokenStore.ReadCredentials(GetRefreshTokenTargetUri(targetUri));
            if (refreshCredentials is null)
                // No refresh token return null.
                return credentials;

            Credential refreshedCredentials = await RefreshCredentials(targetUri, refreshCredentials.Password, null, BbSConsumerKey, BbSConsumerSecret);

            if (refreshedCredentials is null)
                // Refresh failed return null.
                return credentials;

            credentials = refreshedCredentials;

            return credentials;
        }

        /// <inheritdoc/>
        public override async Task<bool> SetCredentials(TargetUri targetUri, Credential credentials)
        {
            // This is only called from the `Store()` method so only applies to default host entries
            // calling this from elsewhere may have unintended consequences, use
            // `SetCredentials(targetUri, credentials, username)` instead.

            // Only store the credentials as received if they match the uri and user of the existing
            // default entry.
            var currentCredentials = await GetCredentials(targetUri);
            if (currentCredentials != null
                && currentCredentials.Username != null
                && !Ordinal.Equals(currentCredentials.Username, credentials.Username))
            {
                // Do nothing as the default is for another username and we don't want to overwrite it.
                Trace.WriteLine($"skipping for {targetUri.QueryUri} new username {currentCredentials.Username} != {credentials.Username}");
                return false;
            }

            await SetCredentials(targetUri, credentials, null);

            // `Store()` will not call with a username Url.
            if (targetUri.ContainsUserInfo)
                return false;

            // See if there is a matching personal refresh token.
            var username = credentials.Username;
            var userSpecificTargetUri = targetUri.GetPerUserTargetUri(username);
            var userCredentials = await GetCredentials(userSpecificTargetUri, username);

            if (userCredentials != null && userCredentials.Password.Equals(credentials.Password))
            {
                var userRefreshCredentials = await GetCredentials(GetRefreshTokenTargetUri(userSpecificTargetUri), username);
                if (userRefreshCredentials != null)
                {
                    Trace.WriteLine("OAuth RefreshToken");
                    var hostRefreshCredentials = new Credential(credentials.Username, userRefreshCredentials.Password);
                    await SetCredentials(GetRefreshTokenTargetUri(targetUri), hostRefreshCredentials, null);
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> SetCredentials(TargetUri targetUri, Credential credentials, string username)
        {
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));
            if (credentials is null)
                throw new ArgumentNullException(nameof(credentials));

            Trace.WriteLine($"{credentials.Username} at {targetUri.QueryUri.AbsoluteUri}");

            // If the Url doesn't contain a username then save with an explicit username.
            if (!targetUri.ContainsUserInfo && (!string.IsNullOrWhiteSpace(username)
                || !string.IsNullOrWhiteSpace(credentials.Username)))
            {

                var realUsername = GetRealUsername(credentials, username);
                var tempCredentials = new Credential(realUsername, credentials.Password);

                if (tempCredentials.Username.Length > BaseSecureStore.UsernameMaxLength)
                    throw new ArgumentOutOfRangeException(nameof(tempCredentials.Username));
                if (tempCredentials.Password.Length > BaseSecureStore.PasswordMaxLength)
                    throw new ArgumentOutOfRangeException(nameof(tempCredentials.Password));

                await SetCredentials(targetUri.GetPerUserTargetUri(realUsername), tempCredentials, null);
            }

            return await PersonalAccessTokenStore.WriteCredentials(targetUri, credentials);
        }

        private static string GetRealUsername(Credential credentials, string username)
        {
            return GetRealUsername(credentials.Username, username);
        }

        private static string GetRealUsername(string remoteUsername, string username)
        {
            // If there is no credentials username, use the provided one.
            if (string.IsNullOrWhiteSpace(remoteUsername))
                return username;

            // Otherwise.
            return remoteUsername;
        }

        /// <summary>
        /// Identify the Hosting service from the the targetUri.
        /// <para/>
        /// Returns a `<see cref="BaseAuthentication"/>` instance if the `<paramref name="targetUri"/>` represents Bitbucket; otherwise `<see langword=""="null"/>`.
        /// </summary>
        /// <param name="targetUri"></param>
        public static BaseAuthentication GetAuthentication(
            RuntimeContext context,
            TargetUri targetUri,
            ICredentialStore personalAccessTokenStore,
            AcquireCredentialsDelegate acquireCredentialsCallback,
            AcquireAuthenticationOAuthDelegate acquireAuthenticationOAuthCallback,
            string bbsConsumerKey,
            string bbsConsumerSecret)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));

            BaseAuthentication authentication = null;

            if (personalAccessTokenStore is null)
                throw new ArgumentNullException(nameof(personalAccessTokenStore), $"The `{nameof(personalAccessTokenStore)}` is null or invalid.");

            if (targetUri.QueryUri.DnsSafeHost.EndsWith(BitbucketBaseUrlHost, StringComparison.OrdinalIgnoreCase))
            {
                authentication = new Authentication(context, targetUri, personalAccessTokenStore, acquireCredentialsCallback, acquireAuthenticationOAuthCallback, bbsConsumerKey, bbsConsumerSecret);
                context.Trace.WriteLine("authentication for Bitbucket created");
            }
            else
            {
                authentication = null;
            }

            return authentication;
        }

        /// <summary>
        /// Prompt the user for authentication credentials.
        /// </summary>
        /// <param name="targetUri"></param>
        /// <param name="username"></param>
        /// <returns>a valid instance of <see cref="Credential"/> or null</returns>
        public async Task<Credential> InteractiveLogon(TargetUri targetUri, string username)
        {
            if (string.IsNullOrWhiteSpace(username) || targetUri.ContainsUserInfo)
                return await InteractiveLogon(targetUri);

            return await InteractiveLogon(targetUri.GetPerUserTargetUri(username));
        }

        /// <inheritdoc/>
        public async Task<Credential> InteractiveLogon(TargetUri targetUri)
        {
            Credential credentials = null;
            string username;
            string password;

            // Ask the user for basic authentication credentials
            if (AcquireCredentialsCallback("Please enter your Bitbucket credentials for ", targetUri, BbSConsumerKey, BbSConsumerSecret, out username, out password))
            {
                AuthenticationResult result = new AuthenticationResult(AuthenticationResultType.None);
                credentials = new Credential(username, password);
                bool skipToOAuth = false;
                if ("skiptooauth".Equals(username)
                    && "skiptooauth".Equals(password))
                {
                    result = new AuthenticationResult(AuthenticationResultType.TwoFactor);
                    skipToOAuth = true;
                    username = null;
                    password = null;
                }

                if (!skipToOAuth && 
                    (result = await BitbucketAuthority.AcquireToken(targetUri, credentials, AuthenticationResultType.None, TokenScope, BbSConsumerKey, BbSConsumerSecret)))
                {
                    Trace.WriteLine("token acquisition succeeded");

                    credentials = GenerateCredentials(targetUri, username, ref result);
                    await SetCredentials(targetUri, credentials, username);

                    // If a result callback was registered, call it.
                    AuthenticationResultCallback?.Invoke(targetUri, result);

                    return credentials;
                }
                else if (result == AuthenticationResultType.TwoFactor)
                {
                    // Basic authentication attempt returned a result indicating the user has 2FA on so prompt
                    // the user to run the OAuth dance.
                    if (AcquireAuthenticationOAuthCallback("", targetUri, result, username))
                    {
                        if (result = await BitbucketAuthority.AcquireToken(targetUri, credentials, AuthenticationResultType.TwoFactor, TokenScope, BbSConsumerKey, BbSConsumerSecret))
                        {
                            Trace.WriteLine("token acquisition succeeded");

                            credentials = GenerateCredentials(targetUri, username, ref result);

                            await SetCredentials(targetUri, credentials, username);
                            if(result.RefreshToken != null)
                            {
                                await SetCredentials(GetRefreshTokenTargetUri(targetUri), 
                                                 new Credential(result.RefreshToken.Type.ToString(),
                                                                result.RefreshToken.Value),
                                                 username);
                            }

                            // If a result callback was registered, call it.
                            AuthenticationResultCallback?.Invoke(targetUri, result);

                            return credentials;
                        }
                    }
                }
            }

            Trace.WriteLine("interactive logon failed");
            return credentials;
        }

        /// <summary>
        /// Generate the final credentials for storing.
        /// <para>
        /// Bitbucket always wants the username as well as the password/token so if the username
        /// isn't explicit in the remote URL then we need to ensure the credentials are stored with a
        /// real username rather than 'Personal Access Token' etc
        /// </para>
        /// </summary>
        /// <param name="targetUri"></param>
        /// <param name="username"></param>
        /// <param name="result"></param>
        /// <returns>the final <see cref="Credential"/> instance.</returns>
        private Credential GenerateCredentials(TargetUri targetUri, string username,
            ref AuthenticationResult result)
        {
            var credentials = (Credential)result.Token;

            // No user info in Uri, or it's a basic login so we need to personalize the credentials.
            if (!targetUri.ContainsUserInfo || result.Token.Type == TokenType.Personal)
            {
                // No user info in Uri so personalize the credentials.
                var realUsername = GetRealUsername(result.RemoteUsername, username);
                credentials = new Credential(realUsername, credentials.Password);
            }

            return credentials;
        }

        /// <summary>
        /// Generate the final refresh token credentials for storing.
        /// <para>
        /// Bitbucket always wants the username as well as the password/token so if the username
        /// isn't explicit in the remote URL then we need to ensure the credentials are stored with a
        /// real username rather than 'Personal Access Token' etc. This applies to the refesh token
        /// as well.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="targetUri"></param>
        /// <param name="username"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private Credential GenerateRefreshCredentials(TargetUri targetUri, string username,
            ref AuthenticationResult result)
        {
            var credentials = (Credential)result.Token;

            // If `targetUri` contains user information, override the credential username with it;
            // otherwise keep the username provided by the result token.
            credentials = targetUri.ContainsUserInfo
                ? new Credential(credentials.Username, result.RefreshToken.Value)
                : new Credential(username, result.RefreshToken.Value);

            return credentials;
        }

        public async Task<Credential> ValidateCredentials(TargetUri targetUri, string username, Credential credentials)
        {
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));
            if (credentials is null)
                throw new ArgumentNullException(nameof(credentials));

            TargetUri userSpecificTargetUri = targetUri.ContainsUserInfo
                ? targetUri
                : targetUri.GetPerUserTargetUri(username);

            if (await BitbucketAuthority.ValidateCredentials(userSpecificTargetUri, username, credentials))
                return credentials;

            var userSpecificRefreshCredentials = await GetCredentials(GetRefreshTokenTargetUri(userSpecificTargetUri), username);

            // If there are refresh credentials it suggests it might be OAuth so we can try and
            // refresh the access_token and try again.
            if (userSpecificRefreshCredentials == null)
                return null;

            Credential refreshedCredentials;

            if ((refreshedCredentials = await RefreshCredentials(userSpecificTargetUri, userSpecificRefreshCredentials.Password, username ?? credentials.Username,
                    BbSConsumerKey, BbSConsumerSecret)) != null)
                return refreshedCredentials;

            return null;
        }

        /// <summary>
        /// Use locally stored refresh_token to attempt to retrieve a new access_token.
        /// </summary>
        /// <param name="targetUri"></param>
        /// <param name="refreshToken"></param>
        /// <param name="username"></param>
        /// <returns>
        /// A <see cref="Credential"/> containing the new access_token if successful, null otherwise
        /// </returns>
        private async Task<Credential> RefreshCredentials(TargetUri targetUri, string refreshToken, string username, string bbsConsumerKey, string bbsConsumerSecret)
        {
            Credential credentials = null;
            AuthenticationResult result;

            if ((result = await BitbucketAuthority.RefreshToken(targetUri, refreshToken, bbsConsumerKey, bbsConsumerSecret)) == true)
            {
                Trace.WriteLine("token refresh succeeded");

                var tempCredentials = GenerateCredentials(targetUri, username, ref result);
                if (!await BitbucketAuthority.ValidateCredentials(targetUri, username, tempCredentials))
                    // Oddly our new access_token failed to work, maybe we've been revoked in the
                    // last millisecond?
                    return credentials;

                // The new access_token is good, so store it and store the refresh_token used to get it.
                await SetCredentials(targetUri, tempCredentials, null);

                var newRefreshCredentials = GenerateRefreshCredentials(targetUri, username, ref result);

                await SetCredentials(GetRefreshTokenTargetUri(targetUri), newRefreshCredentials, username);

                credentials = tempCredentials;
            }

            return credentials;
        }

        private IAuthority BitbucketAuthority { get; }
        private string BbSConsumerKey { get; }
        private string BbSConsumerSecret { get; }

        /// <summary>
        /// Delegate for Basic Auth credential acquisition from the UX.
        /// </summary>
        /// <param name="titleMessage">the title to display to the user.</param>
        /// <param name="targetUri">
        /// The uniform resource indicator used to uniquely identify the credentials.
        /// </param>
        /// <param name="username">The username supplied by the user.</param>
        /// <param name="password">The password supplied by the user.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public delegate bool AcquireCredentialsDelegate(string titleMessage, TargetUri targetUri, string bbsConsumerKey, string bbsConsumerSecret, out string username, out string password);

        /// <summary>
        /// Delegate for OAuth token acquisition from the UX.
        /// </summary>
        /// <param name="targetUri">
        /// The uniform resource indicator used to uniquely identify the credentials.
        /// </param>
        /// <param name="resultType">
        /// <para>The result of initial logon attempt, using the results of <see cref="AcquireCredentialsDelegate"/>.</para>
        /// <para>Should be <see cref="AuthenticationResultType.OAuth"/>.</para>
        /// </param>
        /// <param name="authenticationCode">The authentication code provided by the user.</param>
        /// <returns>True if successful; otherwise false.</returns>
        public delegate bool AcquireAuthenticationOAuthDelegate(string title, TargetUri targetUri, AuthenticationResultType resultType, string username);

        /// <summary>
        /// Delegate for reporting the success, or not, of an authentication attempt.
        /// </summary>
        /// <param name="targetUri">
        /// The uniform resource indicator used to uniquely identify the credentials.
        /// </param>
        /// <param name="result">The result of the interactive authentication attempt.</param>
        public delegate void AuthenticationResultDelegate(TargetUri targetUri, AuthenticationResultType result);
    }
}
