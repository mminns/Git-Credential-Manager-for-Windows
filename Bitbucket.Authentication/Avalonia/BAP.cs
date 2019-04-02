using System;
using Atlassian.Bitbucket.Authentication.Avalonia.Views;
using GitHub.Shared.Controls;
using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication.Avalonia
{
    public class BAP : AbstactAuthenticationPrompts
    {
        public BAP(RuntimeContext context, IntPtr parentHwnd) : base(context, parentHwnd, new BitbucketGui(context))
        {
        }

        public BAP(RuntimeContext context) : this(context, IntPtr.Zero)
        {
        }

        public override Func<IAuthenticationDialogWindow> GetCredentialWindowCreator()
        {
            return () => new CredentialsWindow();
        }

        public override Func<IAuthenticationDialogWindow> GetOAuthWindowCreator()
        {
            return () => new OAuthWindow();
        }
    }
}