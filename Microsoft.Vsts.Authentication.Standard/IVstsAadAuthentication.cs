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
using System.Threading.Tasks;

namespace Microsoft.Alm.Authentication
{
    public interface IVstsAadAuthentication
    {
        /// <summary>
        /// Creates an interactive logon session, using ADAL secure browser GUI, which enables users to authenticate with the Azure tenant and acquire the necessary access tokens to exchange for a VSTS personal access token.
        /// <para/>
        /// Tokens acquired are stored in the secure secret stores provided during initialization.
        /// <para/>
        /// Return a `<see cref="Credential"/>` for resource access if successful; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="targetUri">The URI of the VSTS resource.</param>
        Task<Credential> InteractiveLogon(TargetUri targetUri, PersonalAccessTokenOptions options);

        /// <summary>
        /// Creates an interactive logon session, using ADAL secure browser GUI, which enables users to authenticate with the Azure tenant and acquire the necessary access tokens to exchange for a VSTS personal access token.
        /// <para/>
        /// Tokens acquired are stored in the secure secret stores provided during initialization.
        /// <para/>
        /// Return a `<see cref="Credential"/>` for resource access if successful; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="targetUri">The URI of the VSTS resource.</param>
        /// <param name="requestCompactToken">
        /// Requests a compact format personal access token if `<see langword="true"/>`; otherwise requests a standard format personal access token.
        /// <para/>
        /// Compact tokens are necessary for clients which have restrictions on the size of the basic authentication header which they can create (example: Git).
        /// </param>
        [Obsolete("Please use Task<Credential> InteractiveLogon(TargetUri targetUri, PersonalAccessTokenOptions options) instead.", false)]
        Task<Credential> InteractiveLogon(TargetUri targetUri, bool requestCompactToken);

        /// <summary>
        /// Uses Active Directory Federation Services to authenticate with the Azure tenant non-interactively and acquire the necessary access tokens to exchange for a VSTS personal access token.
        /// <para/>
        /// Tokens acquired are stored in the secure secret stores provided during initialization.
        /// <para/>
        /// Return a `<see cref="Credential"/>` for resource access if successful; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="targetUri">The URL of the VSTS resource.</param>
        /// <param name="options">Options related to VSTS personal access creation.</param>
        Task<Credential> NoninteractiveLogon(TargetUri targetUri, PersonalAccessTokenOptions options);

        /// <summary>
        /// Uses Active Directory Federation Services to authenticate with the Azure tenant non-interactively and acquire the necessary access tokens to exchange for a VSTS personal access token.
        /// <para/>
        /// Tokens acquired are stored in the secure secret stores provided during initialization.
        /// <para/>
        /// Return a `<see cref="Credential"/>` for resource access if successful; otherwise `<see langword="null"/>`.
        /// </summary>
        /// <param name="targetUri">The URL of the VSTS resource.</param>
        /// <param name="requestCompactToken">
        /// Requests a compact format personal access token if `<see langword="true"/>`; otherwise requests a standard format personal access token.
        /// <para/>
        /// Compact tokens are necessary for clients which have restrictions on the size of the basic authentication header which they can create (example: Git).
        /// </param>
        [Obsolete("Please use Task<Credential> NoninteractiveLogon(TargetUri targetUri, PersonalAccessTokenOptions options) instead.", false)]
        Task <Credential> NoninteractiveLogon(TargetUri targetUri, bool requestCompactToken);
    }
}
