﻿using System;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication.Rest.Server
{
    public class RestClient : Base, IRestClient
    {
        internal static readonly Regex UsernameRegex = new Regex(@"\s*""name""\s*:\s*""([^""]+)""\s*", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

        public RestClient(RuntimeContext context)
            : base(context)
        { }

        public static string UserUrl
        {
            get { return "/1.0/users"; }
        }

        public async Task<AuthenticationResult> TryGetUser(TargetUri targetUri, int requestTimeout, Uri restRootUrl, Secret authorization)
        {
            var options = new NetworkRequestOptions(true)
            {
                Authorization = authorization,
                Timeout = TimeSpan.FromMilliseconds(requestTimeout),
            };

            // TDOD slugify username
            var cred = authorization as Credential;
            var apiUrl = new Uri(restRootUrl + UserUrl + "/" + cred.Username);
            var requestUri = targetUri.CreateWith(apiUrl);
            try
            {
                using (var response = await Network.HttpGetAsync(requestUri, options))
                {
                    Trace.WriteLine($"server responded with {response.StatusCode}.");

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        case HttpStatusCode.Created:
                            {
                                Trace.WriteLine("authentication success: new password token created.");

                                // Get username to cross check against supplied one
                                var responseText = response.Content.AsString;
                                var username = FindUsername(responseText);
                                return new AuthenticationResult(AuthenticationResultType.Success, username);
                            }

                        case HttpStatusCode.Forbidden:
                            {
                                // A 403/Forbidden response indicates the username/password are
                                // recognized and good but 2FA is on in which case we want to
                                // indicate that with the TwoFactor result
                                Trace.WriteLine("two-factor app authentication code required");
                                return new AuthenticationResult(AuthenticationResultType.TwoFactor);
                            }
                        case HttpStatusCode.Unauthorized:
                            {
                                // username or password are wrong.
                                Trace.WriteLine("authentication unauthorized");
                                return new AuthenticationResult(AuthenticationResultType.Failure);
                            }

                        default:
                            // any unexpected result can be treated as a failure.
                            Trace.WriteLine("authentication failed");
                            return new AuthenticationResult(AuthenticationResultType.Failure);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string FindUsername(string responseText)
        {
            Match usernameMatch;
            if ((usernameMatch = UsernameRegex.Match(responseText)).Success
                && usernameMatch.Groups.Count > 1)
            {
                string usernameText = usernameMatch.Groups[1].Value;
                Trace.WriteLine($"Found username [{usernameText}]");
                return usernameText;
            }

            return null;
        }
    }
}
