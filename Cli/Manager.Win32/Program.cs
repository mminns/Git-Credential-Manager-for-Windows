using System;
using Basic.Authentication;
using Microsoft.Alm.Authentication.Win32;
using Microsoft.Alm.Cli;

namespace Microsoft.Alm.Cli
{
    public partial class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            var context = Win32RuntimeContext.Default;
            var logger = new Win32Logger();
            var azurePrompts = new AzureDevOps.Authentication.AuthenticationPrompts(context);
            var gitHubPrompts = new GitHub.Authentication.AuthenticationPrompts(context);
            var bitbucketPrompts = new Atlassian.Bitbucket.Authentication.AuthenticationPrompts(context);
            var basicPrompts = new BasicAuthenticationPrompts();
            ;
            var program = new Program(context, logger, azurePrompts, gitHubPrompts, bitbucketPrompts, basicPrompts);

            program.Run(args, new Installer(program));
        }
    }
}
