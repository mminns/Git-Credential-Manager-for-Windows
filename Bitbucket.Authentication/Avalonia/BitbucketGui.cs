using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using GitHub.Shared.Controls;
using GitHub.Shared.ViewModels;
using Microsoft.Alm.Authentication;
using Rheic;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Alm.Authentication.Git;

namespace Atlassian.Bitbucket.Authentication.Avalonia
{
    public class BitbucketGui : IGui, IDisposable
    {
        private RuntimeContext context;
        private AvaloniaGateway _avaloniaGateway = new AvaloniaGateway();
        public BitbucketGui(RuntimeContext context)
        {
            this.context = context;

        }

        public Type ServiceType
            => typeof(IGui);

        public bool ShowViewModel(DialogViewModel viewModel, Func<IAuthenticationDialogWindow> windowCreator)
        {
            System.Console.WriteLine("ShowViewModel start");
            StartSTATask(() =>
                {
                    System.Console.WriteLine("Task start");
                    OptionallyStartGui();
                    _avaloniaGateway.Show(viewModel, () => { return windowCreator() as Window;});
                })
                .Wait();
            
            return viewModel.Result == AuthenticationDialogResult.Ok
                   && viewModel.IsValid;
        }

        private void OptionallyStartGui()
        {
            System.Console.WriteLine("OptionallyStartGui start");
            if (!_avaloniaGateway.Running)
            {
                _avaloniaGateway.Open(() =>
                {
                    System.Console.WriteLine("Open Task start");
                    BuildAvaloniaApp().SetExitMode(ExitMode.OnExplicitExit).SetupWithoutStarting();
                    System.Console.WriteLine("Open Task end");
                });
            }
        }

        private static Task StartSTATask(Action action)
        {
            System.Console.WriteLine("StartSTATask start");
            var completionSource = new TaskCompletionSource<object>();

            var thread = new Thread(() =>
            {
                try
                {
                    action();
                    completionSource.SetResult(null);
                }
                catch (Exception e)
                {
                    completionSource.SetException(e);
                }
            });
            System.Console.WriteLine("StartSTATask set state");
            if(System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows))
            {
                thread.SetApartmentState(ApartmentState.STA);
            }
            System.Console.WriteLine("StartSTATask start");
            thread.Start();

            return completionSource.Task;
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();

        public void Dispose()
        {
            _avaloniaGateway.Close();
        }
    }
}