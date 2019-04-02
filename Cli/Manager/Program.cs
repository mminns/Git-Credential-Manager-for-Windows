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
            var context = new Prototype.PrototypeRuntimeContext(
                c => { return new Network(c);},
                c => { return new Settings(c);},
                c => { return new SimpleFileStorage(c); },
                c => { return new Authentication.Git.Trace(c); },
                c => { return new PrototypeUtilities(c); },
                c => { return new PrototypeWhere(c); }
            );
            var logger = new CoreLogger();
            var azurePrompts = new AzureDevOpsAuthenticationPrompts(context);
            var gitHubPrompts = new GitHubAuthenticationPrompts(context);
            //BuildAvaloniaApp().SetupWithoutStarting();
            var bitbucketPrompts = new BAP(context, IntPtr.Zero);
            var program = new Program(context, logger, azurePrompts, gitHubPrompts, bitbucketPrompts);

            program.Run(args, new CoreInstaller(program));
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();
    }

    internal class CoreInstaller : IInstaller
    {
        private Program _program;

        public CoreInstaller(Program program)
        {
            _program = program;
        }

        public ResultValue Result => throw new NotImplementedException();

        public int ExitCode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void DeployConsole()
        {
            throw new NotImplementedException();
        }

        public void RemoveConsole()
        {
            throw new NotImplementedException();
        }
    }

    internal class CoreLogger : ILogger
    {
        public void LogEvent(Program program, string message, string eventTypeName)
        {
            // TODO MMINNS do nothing for now
            // use Msft.Ext.Logging
            // throw new NotImplementedException();
        }
    }
}
