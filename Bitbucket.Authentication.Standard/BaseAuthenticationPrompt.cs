using System;
using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication
{
    public class BaseAuthenticationPrompts: Base
    {
        private Func<TargetUri, string, Credential> _prompt;
        public BaseAuthenticationPrompts(RuntimeContext context, Func<TargetUri, string, Credential> prompt)
            : base(context)
        {
            _prompt = prompt;
        }

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
            var credential = _prompt(targetUri, title);
            username = credential.Username;
            password = credential.Password;
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
            // TODO extract out of here and bring in as a Func<>
            Console.WriteLine("Your account requires Two-Factor Authentication.");
            Console.WriteLine("Use OAuth authorization to complete this process? [Y(default)/N]:");

            var response = Console.ReadLine();

            return response.Equals("Y", StringComparison.InvariantCultureIgnoreCase) || response.Equals(string.Empty, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
