﻿/**** Git Credential Manager for Windows ****
 *
 * Copyright (c) Microsoft Corporation
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
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Microsoft.Alm.Authentication
{
    internal class WwwAuthenticateHelper
    {
        public static readonly Credential Credentials = new Credential(string.Empty, string.Empty);
        public static readonly AuthenticationHeaderValue NtlmHeader = new AuthenticationHeaderValue("NTLM");
        public static readonly AuthenticationHeaderValue NegotiateHeader = new AuthenticationHeaderValue("Negotiate");

        private static readonly AuthenticationHeaderValue[] NullResult = new AuthenticationHeaderValue[0];

        public static async Task<AuthenticationHeaderValue[]> GetHeaderValues(RuntimeContext context, TargetUri targetUri)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (targetUri is null)
                throw new ArgumentNullException(nameof(targetUri));

            if (targetUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.Ordinal)
                || targetUri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.Ordinal))
            {
                try
                {
                    // Configure the HTTP request to not choose an authentication strategy for us
                    // because we want to deliver the complete payload to the caller.
                    var options = new NetworkRequestOptions(false, context)
                    {
                        Flags = NetworkRequestOptionFlags.UseProxy,
                        Timeout = TimeSpan.FromMilliseconds(Global.RequestTimeout),
                    };
                    var requestUri = targetUri;

                    // Since we're more likely to get an accurate response from the actual-URL,
                    // use it if available, otherwise use `targetUri` as supplied; but only if
                    // the actual-URL differs from the query-URL.
                    if (targetUri.ActualUri != null
                        && targetUri.ActualUri != targetUri.QueryUri)
                    {
                        requestUri = targetUri.CreateWith(queryUri: targetUri.ActualUri);
                    }

                    // Make the request and return the response.
                    using (var result = await context.Network.HttpHeadAsync(requestUri, options))
                    {
                        return result.Headers?.WwwAuthenticate?.ToArray()
                            ?? NullResult;
                    }
                }
                catch (Exception exception)
                {
                    context.Trace.WriteLine("error testing targetUri for NTLM.");
                    context.Trace.WriteException(exception);
                }
            }

            return NullResult;
        }

        public static bool IsNtlm(AuthenticationHeaderValue value)
        {
            return value?.Scheme != null
                && value.Scheme.Equals(NtlmHeader.Scheme, StringComparison.OrdinalIgnoreCase);
        }
    }
}
