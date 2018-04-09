﻿/**** Git Credential Manager for Windows ****
 *
 * Copyright (c) GitHub Corporation
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
using GitHub.Authentication.Properties;
using GitHub.Shared.Helpers;
using GitHub.Shared.ViewModels;
using GitHub.Shared.ViewModels.Validation;

namespace GitHub.Authentication.ViewModels
{
    public class CredentialsViewModel : DialogViewModel
    {
        public CredentialsViewModel()
        {
            LoginCommand = new ActionCommand(_ => Result = AuthenticationDialogResult.Ok);
            CancelCommand = new ActionCommand(_ => Result = AuthenticationDialogResult.Cancel);

            LoginValidator = PropertyValidator.For(this, x => x.Login)
                .Required(Resources.LoginRequired);

            PasswordValidator = PropertyValidator.For(this, x => x.Password)
                .Required(Resources.PasswordRequired);

            ModelValidator = new ModelValidator(LoginValidator, PasswordValidator);
            ModelValidator.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ModelValidator.IsValid))
                {
                    IsValid = ModelValidator.IsValid;
                }
            };
        }

        private string _login;

        /// <summary>
        /// GitHub login which is either the user name or email address.
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

        public PropertyValidator<string> LoginValidator { get; }

        private string _password;

        /// <summary>
        /// GitHub login which is either the user name or email address.
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                // Hack: Because we're binding one way to source, we need to skip the initial value
                // that's sent when the binding is setup by the XAML
                if (_password == null && value == null) return;
                _password = value;
                RaisePropertyChangedEvent(nameof(Password));
            }
        }

        public PropertyValidator<string> PasswordValidator { get; }

        public ModelValidator ModelValidator { get; }

        public ICommand LoginCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand HyperLinkCommand { get; } = new HyperLinkCommand();
        public ICommand ForgotPasswordCommand { get; } = new HyperLinkCommand();
    }
}
