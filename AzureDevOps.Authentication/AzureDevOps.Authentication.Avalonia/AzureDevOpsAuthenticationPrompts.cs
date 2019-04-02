using Microsoft.Alm.Authentication;

namespace AzureDevOps.Authentication.Avalonia
{
    public class AzureDevOpsAuthenticationPrompts : AzureDevOps.Authentication.IAuthenticationPrompts
    {
        private RuntimeContext context;

        public AzureDevOpsAuthenticationPrompts(RuntimeContext context)
        {
            this.context = context;
        }

        public bool CredentialModalPrompt(TargetUri targeturi, out string username, out string password)
        {
            throw new System.NotImplementedException();
        }

        public bool AuthenticationCodeModalPrompt(TargetUri targeturi, string username,
            out string authenticationcode)
        {
            throw new System.NotImplementedException();
        }
    }
}