using Atlassian.Bitbucket.Authentication.Avalonia;
using AzureDevOps.Authentication.Avalonia;
using Basic.Authentication;
using GitHub.Authentication.Avalonia;
using Microsoft.Alm.Authentication;
using Prototype;
using System;
using Basic.Authentication.Avalonia;

namespace Microsoft.Alm.Cli
{
    public partial class Program
    {
        private static void Main(string[] args)
        {
            var context = new Prototype.GenericRuntimeContext(
                c => { return new Network(c);},
                c => { return new Settings(c);},
                c => { return new SimpleFileStorage(c); },
                c => { return new Microsoft.Alm.Authentication.Git.Trace(c); },
                c => { return new PrototypeUtilities(c); },
                c => { return new PrototypeWhere(c); }
            );
            var logger = new Logger();
            var azurePrompts = new AzureDevOpsAuthenticationPrompts(context);
            var gitHubPrompts = new GitHubAuthenticationPrompts(context);
            using (var basicPrompts = new BasicAuthenticationPrompts(context, IntPtr.Zero))
            {
                using (var bitbucketPrompts = new BitbucketAuthenticationPrompts(context, IntPtr.Zero))
                {
                    var program = new Program(context, logger, azurePrompts, gitHubPrompts, bitbucketPrompts, basicPrompts);
                    program.Run(args, new Installer(program));
                }
            }


        }

    }
}
