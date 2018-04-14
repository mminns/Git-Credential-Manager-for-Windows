using System;
using Microsoft.Alm.Authentication;

namespace GitHub.Authentication
{
    public class BaseAuthenticationPrompts: Base
    {
        public BaseAuthenticationPrompts(RuntimeContext context)
            : base(context)
        { }

        public bool CredentialModalPrompt(TargetUri targetUri, out string username, out string password)
        {
            System.Console.Write("username:");
            username = System.Console.ReadLine();
            System.Console.Write("password:");
            password = System.Console.ReadLine();

            return true;

        }

        public bool AuthenticationCodeModalPrompt(TargetUri targetUri, GitHubAuthenticationResultType resultType, string username, out string authenticationCode)
        {
            throw new NotSupportedException();
        }
    }
}
