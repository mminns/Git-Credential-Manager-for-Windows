using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Basic.Authentication.Avalonia.ViewModels;
using Basic.Authentication.Avalonia.Views;
using Microsoft.Alm.Authentication;
using Microsoft.Alm.Authentication.Git;
using Rheic;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Basic.Authentication.Avalonia
{
    public class BasicAuthenticationPrompts : Base, IAuthenticationPrompts, IDisposable
    {
        protected IntPtr _parentHwnd;
        private AvaloniaGateway _avaloniaGateway = new AvaloniaGateway();

        public BasicAuthenticationPrompts(RuntimeContext context, IntPtr parentHwnd) : base(context)
        {
            _parentHwnd = parentHwnd;
        }

        public void Dispose()
        {
            _avaloniaGateway.Close();
        }

        public Credential ModalPromptForCredentials(ITrace trace, string programTitle, IntPtr parentparentHwnd, TargetUri targetUri, string message)
        {
            var viewModel = new BasicPromptViewModel(null, message);
            StartSTATask(() =>
            {
                OptionallyStartGui();
                _avaloniaGateway.Show(viewModel, () => { return new BasicPromptWindow() as Window; });
            }).Wait();

            return new Credential(viewModel.Login, viewModel.Password);
        }

        public Credential ModalPromptForPassword(ITrace trace, string programTitle, IntPtr parentparentHwnd, TargetUri targetUri, string message, string username)
        {
            var viewModel = new BasicPromptViewModel(username, message);
            StartSTATask(() =>
            {
                OptionallyStartGui();
                _avaloniaGateway.Show(viewModel, () => { return new BasicPromptWindow() as Window; });
            }).Wait();

            return new Credential(viewModel.Login, viewModel.Password);
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

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();
    }
}
