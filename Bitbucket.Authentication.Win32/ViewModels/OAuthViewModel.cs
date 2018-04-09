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

using System.Windows.Input;
using GitHub.Shared.Helpers;
using GitHub.Shared.ViewModels;

namespace Atlassian.Bitbucket.Authentication.ViewModels
{
    /// <summary>
    /// The ViewModel behind the OAuth UI prompt
    /// </summary>
    public class OAuthViewModel : DialogViewModel
    {
        private bool _resultType;

        public OAuthViewModel() : this(false)
        {
        }

        public OAuthViewModel(bool resultType)
        {
            _resultType = resultType;

            OkCommand = new ActionCommand(_ => Result = AuthenticationDialogResult.Ok);
            CancelCommand = new ActionCommand(_ => Result = AuthenticationDialogResult.Cancel);

            // just a notification dialog so its always valid.
            IsValid = true;
        }

        /// <summary>
        /// Provides a link to Bitbucket OAuth documentation
        /// </summary>
        public ICommand LearnMoreCommand { get; } = new HyperLinkCommand();

        /// <summary>
        /// Hyperlink to the Bitbucket forgotten password process.
        /// </summary>
        public ICommand ForgotPasswordCommand { get; } = new HyperLinkCommand();

        /// <summary>
        /// Hyperlink to the Bitbucket sign up process.
        /// </summary>
        public ICommand SignUpCommand { get; } = new HyperLinkCommand();

        /// <summary>
        /// Run the OAuth dance.
        /// </summary>
        public ICommand OkCommand { get; }

        /// <summary>
        /// Cancel the authentication attempt.
        /// </summary>
        public ICommand CancelCommand { get; }
    }
}
