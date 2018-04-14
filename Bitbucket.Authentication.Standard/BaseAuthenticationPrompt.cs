using System;
using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication
{
    public class BaseAuthenticationPrompts: Base
    {
        public BaseAuthenticationPrompts(RuntimeContext context)
            : base(context)
        { }

        /// <summary>
        /// Opens a Modal UI prompting the user for Basic Auth credentials.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="targetUri">contains the URL etc of the Authority</param>
        /// <param name="username">the username entered by the user</param>
        /// <param name="password">the password entered by the user</param>
        /// <returns>
        /// returns true if the user provides credentials which are then successfully validated,
        /// false otherwise
        /// </returns>
        public bool CredentialModalPrompt(string title, TargetUri targetUri, out string username, out string password)
        {
            System.Console.Write("username:");
            username = System.Console.ReadLine();
            System.Console.Write("password:");
            password = System.Console.ReadLine();

            return true;
        }

        /// <summary>
        /// Opens a modal UI prompting the user to run the OAuth dance.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="targetUri">contains the URL etc of the Authority</param>
        /// <param name="resultType"></param>
        /// <param name="username"></param>
        /// <returns>
        /// returns true if the user successfully completes the OAuth dance and the returned
        /// access_token is validated, false otherwise
        /// </returns>
        public bool AuthenticationOAuthModalPrompt(string title, TargetUri targetUri, AuthenticationResultType resultType, string username)
        {
            throw new NotSupportedException();
        }
    }
}
