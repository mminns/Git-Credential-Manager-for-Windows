using System;
using Atlassian.Bitbucket.Authentication.Views;
using GitHub.Shared.Controls;
using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication
{
    public class AuthenticationPrompts : AbstactAuthenticationPrompts
    {
        public AuthenticationPrompts(RuntimeContext context, IntPtr parentHwnd) : base(context, parentHwnd, new Gui(context))
        {
        }

        public AuthenticationPrompts(RuntimeContext context) : this(context, IntPtr.Zero)
        {
        }

        public override Func<IAuthenticationDialogWindow> GetCredentialWindowCreator()
        {
            return () => new CredentialsWindow(Context, _parentHwnd);
        }

        public override Func<IAuthenticationDialogWindow> GetOAuthWindowCreator()
        {
            return () => new OAuthWindow(Context, _parentHwnd);
        }
    }
}