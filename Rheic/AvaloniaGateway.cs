using System;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

/// <summary>
/// Rheic is the sea around the continent of Avalonia, hence the 'gateway' to Avalonia
/// </summary>
namespace Rheic
{
    public class AvaloniaGateway
    {
        private CancellationTokenSource taskSchedulerCancellationTokenSource = new CancellationTokenSource();
        private SingleThreadTaskScheduler taskScheduler;
        public bool Running { get; private set; } = false;

        public void Open(Action initializationAction)
        {
            //https://stackoverflow.com/a/30726903
            taskScheduler = new SingleThreadTaskScheduler(taskSchedulerCancellationTokenSource.Token);
            taskScheduler.Schedule(initializationAction);
            taskScheduler.Start();

            Running = true;
        }

        public void Close()
        {
            if (taskScheduler == null)
            {
                // never opened
                return;
            }

             var task = taskScheduler.Schedule(() => {
                Application.Current.Exit();
            });

            // Wait for all of them to complete...
            task.GetAwaiter().GetResult();

            taskScheduler.Complete();

            Running = false;
        }

        /// <summary>
        /// Show an Avalonia window, the window has to be created and then run on the same thread as the app was initialized.
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="windowCreator"></param>
        public void Show(object viewModel, Func<Window> windowCreator)
        {
            var task = taskScheduler.Schedule(() => {
                var cts = new CancellationTokenSource();
                var window = windowCreator() as Window;
                if (window != null)
                {
                    window.DataContext = viewModel;
                    Show(window, cts);
                }
            });

            // Wait for all of them to complete...
            task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// Runs the application's main loop until some condition occurs that is specified by ExitMode.
        /// </summary>
        /// <param name="mainWindow">The main window</param>
        public void Show(Window window, CancellationTokenSource cancellationTokenSource)
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
