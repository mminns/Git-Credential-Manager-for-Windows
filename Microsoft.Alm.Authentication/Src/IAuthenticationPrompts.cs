using System;
using Microsoft.Alm.Authentication.Git;

namespace Microsoft.Alm.Authentication
{
    public interface IAuthenticationPrompts
    {
        Credential ModalPromptForCredentials(ITrace trace, string programTitle, IntPtr parentparentHwnd, TargetUri targetUri, string message);
        Credential ModalPromptForPassword(ITrace trace, string programTitle, IntPtr parentparentHwnd, TargetUri targetUri, string message, string username);
    }
}