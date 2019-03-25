/**** Git Credential Manager for Windows ****
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Atlassian.Bitbucket.Authentication.Security;
using DotNetAuth.OAuth1a;
using Microsoft.Alm.Authentication;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Deserializers;

namespace Atlassian.Bitbucket.Authentication.OAuth.v1
{
    /// <summary>
    /// </summary>
    public class OAuthAuthenticator : Base, IOAuthAuthenticator
    {
        /// <summary>
        /// The maximum wait time for a network request before timing out
        /// </summary>
        public const int RequestTimeout = 15 * 1000; // 15 second limit

        internal static readonly Regex RefreshTokenRegex = new Regex(@"\s*""refresh_token""\s*:\s*""([^""]+)""\s*", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
        internal static readonly Regex AccessTokenTokenRegex = new Regex(@"\s*""access_token""\s*:\s*""([^""]+)""\s*", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);


        private BbSOauth1AProvider _bbSOAuth1AProvider;
        private Dictionary<string, string> _oAuthSession = new Dictionary<string, string>();
        private OAuth10aStateManager _oAuth10AStateManager;
        private ApplicationCredentials _oAuth10ACredentials;

        public OAuthAuthenticator(RuntimeContext context, string consumerKey, string consumerSecret)
            : base(context)
        {
            ConsumerKey = consumerKey;
            ConsumerSecret = consumerSecret;
            _oAuth10AStateManager = new OAuth10aStateManager((k, v) => _oAuthSession[k] = v, k => (string)_oAuthSession[k]);
            _oAuth10ACredentials = new ApplicationCredentials
            {
                ConsumerKey = ConsumerKey,
                ConsumerSecret = GetConsumerSecret()
            };
        }

        public string AuthorizeUrl { get { return "plugins/servlet/oauth/authorize"; } }

        public string CallbackUrl { get { return "http://localhost:34106/"; } }

        public string ConsumerKey { get; }

        public string ConsumerSecret { get; }

        public string TokenUrl { get { return "plugins/servlet/oauth/request-token"; } }

        /// <summary>
        /// Gets the OAuth access token
        /// </summary>
        /// <returns>The access token</returns>
        /// <exception cref="SourceTree.Exceptions.OAuthException">
        /// Thrown when OAuth fails for whatever reason
        /// </exception>
        public async Task<AuthenticationResult> GetAuthAsync(TargetUri targetUri, TokenScope scope, CancellationToken cancellationToken)
        {
            var userSlug = await Authorize(targetUri, scope, cancellationToken);

            return await GetAccessToken(targetUri, userSlug);
        }

        public async Task<AuthenticationResult> Authenticate(string restRootUrl, TargetUri targetUri, Credential credentials, TokenScope scope)
        {
            var result = await GetAuthAsync(targetUri, scope, CancellationToken.None);

            if (!result.IsSuccess)
            {
                Trace.WriteLine($"oauth authentication failed");
                return new AuthenticationResult(AuthenticationResultType.Failure);
            }

            // HACK HACk HACK
            return new AuthenticationResult(AuthenticationResultType.Success, result.Token,
                result.RemoteUsername);


            //// We got a token but lets check to see the usernames match.
            //var restRootUri = new Uri(restRootUrl);
            //var userResult =
            //    await (new Rest.Cloud.RestClient(Context)).TryGetUser(targetUri, RequestTimeout, restRootUri, result.Token);

            //if (!userResult.IsSuccess)
            //{
            //    Trace.WriteLine($"oauth user check failed");
            //    return new AuthenticationResult(AuthenticationResultType.Failure);
            //}

            //if (!string.IsNullOrWhiteSpace(userResult.RemoteUsername) &&
            //    !credentials.Username.Equals(userResult.RemoteUsername))
            //{
            //    Trace.WriteLine($"Remote username [{userResult.RemoteUsername}] != [{credentials.Username}] supplied username");
            //    // Make sure the 'real' username is returned.
            //    return new AuthenticationResult(AuthenticationResultType.Success, result.Token, result.RefreshToken,
            //        userResult.RemoteUsername);
            //}

            //// Everything is hunky dory.
            //return result;
        }

        /// <summary>
        /// Uses a refresh_token to get a new access_token
        /// </summary>
        /// <param name="targetUri"></param>
        /// <param name="refreshToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<AuthenticationResult> RefreshAuthAsync(TargetUri targetUri, string refreshToken, CancellationToken cancellationToken)
        {
            return await RefreshAccessToken(targetUri, refreshToken);
        }

        /// <summary>
        /// Run the OAuth dance to get a new request_token
        /// </summary>
        /// <param name="targetUri"></param>
        /// <param name="scope"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private async Task<string> Authorize(TargetUri targetUri, TokenScope scope, CancellationToken cancellationToken)
        {
            var authorizationUri = GetAuthorizationUri(targetUri, scope);


            // Open the browser to prompt the user to authorize the token request
            Process.Start(authorizationUri.AbsoluteUri);

            Uri uri;
            try
            {
                // Start a temporary server to handle the callback request and await for the reply.
                uri = await SimpleServer.WaitForURLAsync(CallbackUrl, cancellationToken);
            }
            catch (Exception ex)
            {
                string message;
                if (ex.InnerException != null && ex.InnerException.GetType().IsAssignableFrom(typeof(TimeoutException)))
                {
                    message = "Timeout awaiting response from Host service.";
                }
                else
                {
                    message = "Unable to receive callback from OAuth service provider";
                }

                throw new Exception(message, ex);
            }

            try
            {
                var processUserResponse = OAuth1aProcess.ProcessUserResponse(_bbSOAuth1AProvider, _oAuth10ACredentials,
                    uri, _oAuth10AStateManager);
                processUserResponse.Wait();
                _oAuthSession["access_token"] = processUserResponse.Result.AllParameters["oauth_token"];
                _oAuthSession["accessTokenSecret"] = processUserResponse.Result.AllParameters["oauth_token_secret"];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            // TDOD mke any REST request and then get the X-AUSERNAME header from the response
            // then use that as the usrslug in the token request
            var provider = _bbSOAuth1AProvider;
            var jiraCredentials = _oAuth10ACredentials;
            var accessToken = _oAuthSession["access_token"] as string;
            var accessTokenSecret = _oAuthSession["accessTokenSecret"] as string;


            var http = new Http { Url = new Uri(targetUri.ActualUri.AbsoluteUri + @"/rest/api/1.0/application-properties") };

            http.ApplyAccessTokenToHeader(provider, jiraCredentials, accessToken, accessTokenSecret, "GET");
            var response = http.Get();


            //var processUserResponse2 = OAuth1aProcess.ProcessUserResponse(_bbSOAuth1AProvider, _oAuth10ACredentials,
            //    uri, _oAuth10AStateManager);
            //processUserResponse2.Wait();
            //_oAuthSession["access_token"] = processUserResponse2.Result.AllParameters["oauth_token"];
            //_oAuthSession["accessTokenSecret"] = processUserResponse2.Result.AllParameters["oauth_token_secret"];

            var userslug = response.Headers.FirstOrDefault(h =>
                h.Name.Equals("X-AUSERNAME", StringComparison.InvariantCultureIgnoreCase));

            //var patRequestBody = $"{{\"name\": \"Git-Credential-Manager-{DateTime.Today.ToShortDateString()}\",\"permissions\": [\"REPO_ADMIN\",\"PROJECT_READ\"]}}";
            //var http2 = new Http { Url = new Uri(targetUri.ToString() + $"/rest/access-tokens/1.0/users/{userslug.Value}"), RequestBody = patRequestBody, RequestContentType = "application/json"};

            //http2.ApplyAccessTokenToHeader(provider, jiraCredentials, accessToken, accessTokenSecret, "PUT");
            //var response2 = http2.Put();

            //var json = JObject.Parse(response2.Content);
            //var pat = json["token"];
            ////Parse the callback url
            //Dictionary<string, string> qs = GetQueryParameters(uri.Query);

            //// look for a request_token code in the parameters
            //string authCode = GetAuthenticationCode(qs);

            //if (string.IsNullOrWhiteSpace(authCode))
            //{
            //    var error_desc = GetErrorDescription(qs);
            //    throw new Exception("Request for an OAuth request_token was denied" + error_desc);
            //}

            return userslug.Value;
        }

        private string GetAuthenticationCode(Dictionary<string, string> qs)
        {
            if (qs is null)
                return null;

            return qs.Keys.Where(k => k.EndsWith("code", StringComparison.OrdinalIgnoreCase))
                          .Select(k => qs[k])
                          .FirstOrDefault();
        }

        private string GetErrorDescription(Dictionary<string, string> qs)
        {
            if (qs is null)
                return null;

            return qs["error_description"];
        }

        /// <summary>
        /// Use a request_token to get an access_token
        /// </summary>
        /// <param name="targetUri"></param>
        /// <param name="authCode"></param>
        /// <returns></returns>
        private async Task<AuthenticationResult> GetAccessToken(TargetUri targetUri, string userSlug)
        {
            //if (targetUri is null)
            //    throw new ArgumentNullException(nameof(targetUri));
            //if (authCode is null)
            //    throw new ArgumentNullException(nameof(authCode));

            //var options = new NetworkRequestOptions(true)
            //{
            //    Timeout = TimeSpan.FromMilliseconds(RequestTimeout),
            //};
            //var grantUri = GetGrantUrl(targetUri, authCode);
            //var requestUri = targetUri.CreateWith(grantUri);
            //var content = GetGrantRequestContent(authCode);

            //using (var response = await Network.HttpPostAsync(requestUri, content, options))
            //{
            //    Trace.WriteLine($"server responded with {response.StatusCode}.");

            //    switch (response.StatusCode)
            //    {
            //        case HttpStatusCode.OK:
            //        case HttpStatusCode.Created:
            //            {
            //                // The request was successful, look for the tokens in the response.
            //                string responseText = response.Content.AsString;
            //                var token = FindAccessToken(responseText);
            //                var refreshToken = FindRefreshToken(responseText);
            //                return GetAuthenticationResult(token, refreshToken);
            //            }

            //        case HttpStatusCode.Unauthorized:
            //            {
            //                // Do something.
            //                return new AuthenticationResult(AuthenticationResultType.Failure);
            //            }

            //        default:
            //            Trace.WriteLine("authentication failed");
            //            var error = response.Content.AsString;
            //            return new AuthenticationResult(AuthenticationResultType.Failure);
            //    }
            //}

            var accessToken = _oAuthSession["access_token"] as string;
            var accessTokenSecret = _oAuthSession["accessTokenSecret"] as string;

            var patRequestBody = $"{{\"name\": \"Git-Credential-Manager-{DateTime.Today.ToShortDateString()}\",\"permissions\": [\"REPO_ADMIN\",\"PROJECT_READ\"]}}";

            // HACK why is this QueryUri when the previous call was actualuri?
            var http2 = new Http { Url = new Uri(targetUri.ActualUri.AbsoluteUri + $"/rest/access-tokens/1.0/users/{userSlug}"), RequestBody = patRequestBody, RequestContentType = "application/json" };

            http2.ApplyAccessTokenToHeader(_bbSOAuth1AProvider, _oAuth10ACredentials, accessToken, accessTokenSecret, "PUT");
            var response2 = http2.Put();

            var json = JObject.Parse(response2.Content);
            var pat = json["token"];

            return new AuthenticationResult(AuthenticationResultType.Success, new Token(pat.Value<string>(), TokenType.BitbucketAccess), userSlug);
        }

        /// <summary>
        /// Use a refresh_token to get a new access_token
        /// </summary>
        /// <param name="targetUri"></param>
        /// <param name="currentRefreshToken"></param>
        /// <returns></returns>
        private async Task<AuthenticationResult> RefreshAccessToken(TargetUri targetUri, string currentRefreshToken)
        {
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));
            if (currentRefreshToken is null)
                throw new ArgumentNullException(nameof(currentRefreshToken));

            var refreshUri = GetRefreshUri(targetUri);
            var requestUri = targetUri.CreateWith(refreshUri);
            var options = new NetworkRequestOptions(true)
            {
                Timeout = TimeSpan.FromMilliseconds(RequestTimeout),
            };
            var content = GetRefreshRequestContent(currentRefreshToken);

            using (var response = await Network.HttpPostAsync(requestUri, content, options))
            {
                Trace.WriteLine($"server responded with {response.StatusCode}.");

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                    case HttpStatusCode.Created:
                        {
                            // The request was successful, look for the tokens in the response.
                            string responseText = response.Content.AsString;
                            var token = FindAccessToken(responseText);
                            var refreshToken = FindRefreshToken(responseText);
                            return GetAuthenticationResult(token, refreshToken);
                        }

                    case HttpStatusCode.Unauthorized:
                        {
                            // Do something.
                            return new AuthenticationResult(AuthenticationResultType.Failure);
                        }

                    default:
                        Trace.WriteLine("authentication failed");
                        var error = response.Content.AsString;
                        return new AuthenticationResult(AuthenticationResultType.Failure);
                }
            }
        }

        private Uri GetAuthorizationUri(TargetUri targetUri, TokenScope scope)
        {

            _bbSOAuth1AProvider = new BbSOauth1AProvider(targetUri.ActualUri.AbsoluteUri);
            var authorizationUri = OAuth1aProcess.GetAuthorizationUri(_bbSOAuth1AProvider, _oAuth10ACredentials,
                CallbackUrl, _oAuth10AStateManager);
            authorizationUri.Wait();
            return authorizationUri.Result;

            /*


            var xxxx = GetAuthorizationUrl("POST", new Uri(targetUri, AuthorizeUrl).AbsoluteUri, null);

            const string AuthorizationUrl = "{0}?response_type=code&client_id={1}&state=authenticated&scope={2}&redirect_uri={3}";

            var authorityUrl = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                             AuthorizationUrl,
                                             AuthorizeUrl,
                                             ConsumerKey,
                                             scope.ToString(),
                                             CallbackUrl);

            return new Uri(targetUri, authorityUrl);
            */
        }

        private string GetConsumerSecret()
        {
            if (File.Exists(ConsumerSecret.Replace(@"\\\\", @"\")))
            {
                StreamReader sr = File.OpenText(ConsumerSecret);
                var bbsPrivateKey = sr.ReadToEnd().Trim();
                sr.Close();

                var consumerSecret = bbsPrivateKey.Replace("-----BEGIN PRIVATE KEY-----", "").Replace("-----END PRIVATE KEY-----", "").Replace("\r\n", "").Replace("\n", "");

                RSACryptoServiceProvider keyInfo = opensslkey.DecodePrivateKeyInfo(Convert.FromBase64String(consumerSecret));
                return keyInfo.ToXmlString(true);
            }
            else
            {
                return ConsumerSecret;
            }
        }

        private Uri GetRefreshUri(TargetUri targetUri)
        {
            return new Uri(targetUri, TokenUrl);
        }

        private Uri GetGrantUrl(TargetUri targetUri, string authCode)
        {
            var tokenUrl = $"{TokenUrl}?grant_type=authorization_code&code={authCode}&client_id={ConsumerKey}&client_secret={ConsumerSecret}&state=authenticated";
            return new Uri(new Uri(targetUri.ToString()), tokenUrl);
        }

        private MultipartFormDataContent GetGrantRequestContent(string authCode)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent("authorization_code"), "grant_type" },
                { new StringContent(authCode), "code" },
                { new StringContent(ConsumerKey), "client_id" },
                { new StringContent(ConsumerSecret), "client_secret" },
                { new StringContent("authenticated"), "state" },
                { new StringContent(CallbackUrl), "redirect_uri" }
            };
            return content;
        }

        private Dictionary<string, string> GetQueryParameters(string rawUrlData)
        {
            return rawUrlData.Replace("/?", string.Empty).Split('&')
                             .ToDictionary(c => c.Split('=')[0],
                                           c => Uri.UnescapeDataString(c.Split('=')[1]));
        }

        private MultipartFormDataContent GetRefreshRequestContent(string currentRefreshToken)
        {
            var content = new MultipartFormDataContent
            {
                { new StringContent("refresh_token"), "grant_type" },
                { new StringContent(currentRefreshToken), "refresh_token" },
                { new StringContent(ConsumerKey), "client_id" },
                { new StringContent(ConsumerSecret), "client_secret" }
            };
            return content;
        }

        private Token FindAccessToken(string responseText)
        {
            Match tokenMatch;
            if ((tokenMatch = AccessTokenTokenRegex.Match(responseText)).Success
                && tokenMatch.Groups.Count > 1)
            {
                string tokenText = tokenMatch.Groups[1].Value;
                return new Token(tokenText, TokenType.BitbucketAccess);
            }

            return null;
        }

        private Token FindRefreshToken(string responseText)
        {
            Match refreshTokenMatch;
            if ((refreshTokenMatch = RefreshTokenRegex.Match(responseText)).Success
                && refreshTokenMatch.Groups.Count > 1)
            {
                string refreshTokenText = refreshTokenMatch.Groups[1].Value;
                return new Token(refreshTokenText, TokenType.BitbucketRefresh);
            }

            return null;
        }

        private AuthenticationResult GetAuthenticationResult(Token token, Token refreshToken)
        {
            // Bitbucket should always return both.
            if (token == null || refreshToken == null)
            {
                Trace.WriteLine("authentication failure");
                return new AuthenticationResult(AuthenticationResultType.Failure);
            }
            else
            {
                Trace.WriteLine("authentication success: new personal access token created.");
                return new AuthenticationResult(AuthenticationResultType.Success, token, refreshToken);
            }
        }


    }
}
