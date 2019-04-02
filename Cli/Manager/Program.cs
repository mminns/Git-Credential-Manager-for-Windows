using System;
using System.Threading;
using Atlassian.Bitbucket.Authentication.Avalonia;
using Avalonia;
using Avalonia.Logging.Serilog;
using AzureDevOps.Authentication.Avalonia;
using Bitbucket.Authentication.Avalonia;
using GitHub.Authentication.Avalonia;
using Microsoft.Alm.Authentication;
using Microsoft.Alm.Cli;
using Prototype;

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
                c => { return new Authentication.Git.Trace(c); },
                c => { return new PrototypeUtilities(c); },
                c => { return new PrototypeWhere(c); }
            );
            var logger = new Logger();
            var azurePrompts = new AzureDevOpsAuthenticationPrompts(context);
            var gitHubPrompts = new GitHubAuthenticationPrompts(context);
            using (var bitbucketPrompts = new BitbucketAuthenticationPrompts(context, IntPtr.Zero))
            {
                var program = new Program(context, logger, azurePrompts, gitHubPrompts, bitbucketPrompts);
                program.Run(args, new Installer(program));
            }
        }

    }
}
