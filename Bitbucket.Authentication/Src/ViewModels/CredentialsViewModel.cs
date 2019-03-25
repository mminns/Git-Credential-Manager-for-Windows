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
using System.Windows;
using System.Windows.Input;
using Atlassian.Bitbucket.Authentication.Properties;
using GitHub.Shared.Helpers;
using GitHub.Shared.ViewModels;
using GitHub.Shared.ViewModels.Validation;

namespace Atlassian.Bitbucket.Authentication.ViewModels
{
    /// <summary>
    /// The ViewModel behind the Basic Auth username/password UI prompt.
    /// </summary>
    public class CredentialsViewModel : DialogViewModel
    {
        public CredentialsViewModel() : this(string.Empty, String.Empty, false)
        {
            // without this default constructor get nullreferenceexceptions during binding i guess
            // 'cos the view is built before the 'official' viewmodel and hence generates it own
            // viewmodel while building?
        }

        public CredentialsViewModel(string username, string host, bool supportsOAuth)
        {
            LoginCommand = new ActionCommand(_ => Result = AuthenticationDialogResult.Ok);
            SkipToOAuthCommand = new ActionCommand(_ =>
            {
                SkippedToOAuth = true;
                IsValid = true;
                Result = AuthenticationDialogResult.Ok;
            });
            CancelCommand = new ActionCommand(_ => Result = AuthenticationDialogResult.Cancel);

            LoginValidator = PropertyValidator.For(this, x => x.Login).Required(Resources.LoginRequired);

            PasswordValidator = PropertyValidator.For(this, x => x.Password).Required(Resources.PasswordRequired);

            ModelValidator = new ModelValidator(LoginValidator, PasswordValidator);
            ModelValidator.PropertyChanged += (s, e) =>
                                              {
                                                 if (e.PropertyName == nameof(ModelValidator.IsValid))
                                                  {
                                                      IsValid = ModelValidator.IsValid;
                                                  }
                                              };

            IsSkippableToOAuth = supportsOAuth;

            if (!string.IsNullOrWhiteSpace(host))
            {
                Host = host;
            }

            // set last to allow validator to run
            if (!string.IsNullOrWhiteSpace(username))
            {
                Login = username;
            }
        }

        private string _login;

        /// <summary>
        /// Bitbucket login which is either the user name or email address.
        /// </summary>
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                RaisePropertyChangedEvent(nameof(Login));
            }
        }

        private string _host;

        /// <summary>
        /// Bitbucket Host
        /// </summary>
        public string Host
        {
            get { return _host; }
            set
            {
                _host = value;
                RaisePropertyChangedEvent(nameof(Host));
            }
        }

        public PropertyValidator<string> LoginValidator { get; }

        private string _password;

        /// <summary>
        /// Bitbucket login which is either the user name or email address.
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                // Hack: Because we're binding one way to source, we need to skip the initial value
                // that's sent when the binding is setup by the XAML
                if (_password == null && value == null)
                {
                    return;
                }

                _password = value;
                RaisePropertyChangedEvent(nameof(Password));
            }
        }

        public PropertyValidator<string> PasswordValidator { get; }

        public ModelValidator ModelValidator { get; }

        /// <summary>
        /// Start the process to validate the username/password
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// Skip straight to OAuth
        /// </summary>
        public ICommand SkipToOAuthCommand { get; }

        /// <summary>
        /// Cancel the authentication attempt.
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Hyperlink to Bitbucket documentation.
        /// </summary>
        public ICommand HyperLinkCommand { get; } = new HyperLinkCommand();

        /// <summary>
        /// Hyperlink to the Bitbucket forgotten password process.
        /// </summary>
        public ICommand ForgotPasswordCommand { get; } = new HyperLinkCommand();

        /// <summary>
        /// Hyperlink to the Bitbucket sign up process.
        /// </summary>
        public ICommand SignUpCommand { get; } = new HyperLinkCommand();

        private bool _isSkippableToOAuth;

        public bool IsSkippableToOAuth
        {
            get { return _isSkippableToOAuth; }
            set
            {
                _isSkippableToOAuth = value;
                IsSkipHidden = _isSkippableToOAuth ? Visibility.Visible : Visibility.Collapsed;
                RaisePropertyChangedEvent(nameof(IsSkippableToOAuth));
            }
        }

        private Visibility _isSkipHidden = Visibility.Collapsed;

        public Visibility IsSkipHidden
        {
            get { return _isSkipHidden; }
            set
            {
                _isSkipHidden = value;
                RaisePropertyChangedEvent(nameof(IsSkipHidden));
            }
        }


        public bool SkippedToOAuth { get; private set; }
    }
}
