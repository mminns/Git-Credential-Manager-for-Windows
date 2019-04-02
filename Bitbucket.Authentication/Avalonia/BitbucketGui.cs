using System;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Bitbucket.Authentication;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.Threading;
using GitHub.Shared.Controls;
using GitHub.Shared.ViewModels;
using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication.Avalonia
{
    public class BitbucketGui : IGui
    {
        private RuntimeContext context;
        private CancellationTokenSource taskSchedulerCancellationTokenSource = new CancellationTokenSource();
        private SingleThreadTaskScheduler taskScheduler;

        public BitbucketGui(RuntimeContext context)
        {
            this.context = context;

            //https://stackoverflow.com/a/30726903
            taskScheduler = new SingleThreadTaskScheduler(taskSchedulerCancellationTokenSource.Token);
            taskScheduler.Schedule(() =>
            {
                BuildAvaloniaApp().SetExitMode(ExitMode.OnExplicitExit).SetupWithoutStarting();
            });
            taskScheduler.Start();
        }

        public Type ServiceType
            => typeof(IGui);
        public bool ShowViewModel(DialogViewModel viewModel, Func<IAuthenticationDialogWindow> windowCreator)
        {
            StartSTATask(() =>
                {
                    var task = taskScheduler.Schedule(() => {
                        var cts = new CancellationTokenSource();
                        var window = windowCreator() as Window;
                        if (window != null)
                        {
                            window.DataContext = viewModel;
                            RunAvalonia(window, cts);
                        }
                    });

                    // Wait for all of them to complete...
                    task.GetAwaiter().GetResult();
                })
                .Wait();
            
            return viewModel.Result == AuthenticationDialogResult.Ok
                   && viewModel.IsValid;
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

        /// <summary>
        /// Runs the application's main loop until some condition occurs that is specified by ExitMode.
        /// </summary>
        /// <param name="mainWindow">The main window</param>
        public void RunAvalonia(Window window, CancellationTokenSource cancellationTokenSource)
        {
            if (window == null)
            {
                throw new ArgumentNullException(nameof(window));
            }

            window.Closed += (sender, args) =>
            {
                cancellationTokenSource.Cancel(true);
            };

            if (!window.IsVisible)
            {
                window.Show();
            }

            Application.Current.MainWindow = window;

            Dispatcher.UIThread.MainLoop(cancellationTokenSource.Token);

        }
    }
}