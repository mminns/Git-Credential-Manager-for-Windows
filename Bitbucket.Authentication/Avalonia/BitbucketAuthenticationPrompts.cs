using System;
using Atlassian.Bitbucket.Authentication.Avalonia.Views;
using GitHub.Shared.Controls;
using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication.Avalonia
{
    public class BitbucketAuthenticationPrompts : AbstactAuthenticationPrompts, IDisposable
    {
        public BitbucketAuthenticationPrompts(RuntimeContext context, IntPtr parentHwnd) : base(context, parentHwnd, new BitbucketGui(context))
        {
        }

        public BitbucketAuthenticationPrompts(RuntimeContext context) : this(context, IntPtr.Zero)
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

        public void Dispose()
        {
            if (Gui is BitbucketGui bbcGui)
            {
                bbcGui.Dispose();
            }
        }
    }
}