using System;
using Basic.Authentication.Functions;
using Microsoft.Alm.Authentication;
using Microsoft.Alm.Authentication.Git;

namespace Basic.Authentication
{
    public class BasicAuthenticationPrompts : IAuthenticationPrompts
    {
        //internal delegate bool ModalPromptDisplayDialogDelegate(Program program,
        //    ref NativeMethods.CredentialUiInfo credUiInfo,
        //    ref NativeMethods.CredentialPackFlags authPackage,
        //    IntPtr packedAuthBufferPtr,
        //    uint packedAuthBufferSize,
        //    IntPtr inBufferPtr,
        //    int inBufferSize,
        //    bool saveCredentials,
        //    NativeMethods.CredentialUiWindowsFlags flags,
        //    out string username,
        //    out string password);

        //internal delegate Credential ModalPromptForCredentialsDelegate(Program program, TargetUri targetUri, string message);

        //internal delegate Credential ModalPromptForPasswordDelegate(Program program, TargetUri targetUri, string message, string username);

        //internal ModalPromptDisplayDialogDelegate _modalPromptDisplayDialog = DialogFunctions.DisplayModal;
        //internal ModalPromptForCredentialsDelegate _modalPromptForCredentials = DialogFunctions.CredentialPrompt;
        //internal ModalPromptForPasswordDelegate _modalPromptForPassword = DialogFunctions.PasswordPrompt;

        public Credential ModalPromptForCredentials(ITrace trace, string programTitle, IntPtr parentHwnd, TargetUri targetUri, string message) =>
            DialogFunctions.CredentialPrompt(trace, programTitle, parentHwnd, targetUri, message);

        public Credential ModalPromptForPassword(ITrace trace, string programTitle, IntPtr parentHwnd, TargetUri targetUri, string message, string username) =>
            DialogFunctions.PasswordPrompt(trace, programTitle, parentHwnd, targetUri, message, username);









    }
}
