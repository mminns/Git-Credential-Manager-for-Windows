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
            StartSTATask(() =>
                {
                    OptionallyStartGui();
                    _avaloniaGateway.Show(viewModel, () => { return windowCreator() as Window;});
                })
                .Wait();
            
            return viewModel.Result == AuthenticationDialogResult.Ok
                   && viewModel.IsValid;
        }

        private void OptionallyStartGui()
        {
            if (!_avaloniaGateway.Running)
            {
                _avaloniaGateway.Open(() =>
                {
                    BuildAvaloniaApp().SetExitMode(ExitMode.OnExplicitExit).SetupWithoutStarting();
                });
            }
        }

        private static Task StartSTATask(Action action)
        {
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

            thread.SetApartmentState(ApartmentState.STA);

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