using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Atlassian.Bitbucket.Authentication;
using Atlassian.Bitbucket.Authentication.Avalonia.Views;
using Atlassian.Bitbucket.Authentication.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Logging.Serilog;
using Avalonia.Threading;
using Bitbucket.Authentication.Avalonia;
using Bitbucket.Authentication.Avalonia.Views;
using Microsoft.Alm.Authentication;
using Microsoft.Alm.Authentication.Git;
using Base = Atlassian.Bitbucket.Authentication.Base;

namespace Atlassian.Bitbucket.Authentication.Avalonia
{
    public class BitbucketAuthenticationPrompts : Base, Atlassian.Bitbucket.Authentication.IAuthenticationPrompts
    {
        private RuntimeContext context;
        private IntPtr _parentHwnd;
        //private CancellationTokenSource taskSchedulerCancellationTokenSource = new CancellationTokenSource();
        //private SingleThreadTaskScheduler taskScheduler;
        /// <summary>
        /// Utility method used to extract a username from a URL of the form http(s)://username@domain/
        /// </summary>
        /// <param name="targetUri"></param>
        /// <returns></returns>
        public static string GetUserFromTargetUri(TargetUri targetUri)
        {
            var url = targetUri.QueryUri.AbsoluteUri;
            if (!url.Contains("@"))
            {
                return null;
            }

            var match = Regex.Match(url, @"\/\/(.+)@");
            if (!match.Success)
            {
                return null;
            }

            return match.Groups[1].Value;
        }

        private AppBuilder appBuilder;

        public BitbucketAuthenticationPrompts(RuntimeContext context)
            : this(context, IntPtr.Zero, null)
        {
        }

        public BitbucketAuthenticationPrompts(RuntimeContext context, IntPtr parentHwnd, AppBuilder appBuilder)
            : base(context)
        {
            this.context = context;
            this.appBuilder = appBuilder;

            var gui = GetService<IGui>();

            if (gui is null)
            {
                // Since there's no pre-existing Gui service registered with the current
                // context, we'll need to allocate and add one to it.
                gui = new BitbucketGui(Context);

                SetService(gui);
            }

            _parentHwnd = parentHwnd;

            ////https://stackoverflow.com/a/30726903
            //taskScheduler = new SingleThreadTaskScheduler(taskSchedulerCancellationTokenSource.Token);
            //taskScheduler.Schedule(() =>
            //{
            //    BuildAvaloniaApp().SetExitMode(ExitMode.OnExplicitExit).SetupWithoutStarting();
            //});
            //taskScheduler.Start();

        }

        public bool CredentialModalPrompt(string titleMessage, TargetUri targetUri, out string username, out string password)
        {

            // if there is a user in the remote URL then prepopulate the UI with it.
            var credentialViewModel = new CredentialsViewModel(GetUserFromTargetUri(targetUri));

            Trace.WriteLine("prompting user for credentials.");

            //StartSTATask(async () => Gui.ShowViewModel(credentialViewModel, () => new CredentialsWindow())).Wait();
            var result = Gui.ShowViewModel(credentialViewModel, () => new CredentialsWindow());
            
            username = credentialViewModel.Login;
            password = credentialViewModel.Password;

            return result;
        }

        public bool AuthenticationOAuthModalPrompt(string title, TargetUri targeturi, AuthenticationResultType resulttype,
            string username)
        {
            var oauthViewModel = new OAuthViewModel(resulttype == AuthenticationResultType.TwoFactor);

            Trace.WriteLine("prompting user for authentication code.");

            //StartSTATask(async () => Gui.ShowViewModel(oauthViewModel, () => new OAuthWindow())).Wait();
            var result = Gui.ShowViewModel(oauthViewModel, () => new OAuthWindow());

            return result;
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


    public sealed class SingleThreadTaskScheduler : TaskScheduler
    {
        [ThreadStatic]
        private static bool isExecuting;
        private readonly CancellationToken cancellationToken;

        private readonly BlockingCollection<Task> taskQueue;

        public SingleThreadTaskScheduler(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            this.taskQueue = new BlockingCollection<Task>();
        }

        public void Start()
        {
            new Thread(RunOnCurrentThread) { Name = "STTS Thread" }.Start();
        }

        // Just a helper for the sample code
        public Task Schedule(Action action)
        {
            return
                Task.Factory.StartNew
                    (
                        action,
                        CancellationToken.None,
                        TaskCreationOptions.None,
                        this
                    );
        }

        // You can have this public if you want - just make sure to hide it
        private void RunOnCurrentThread()
        {
            isExecuting = true;

            try
            {
                foreach (var task in taskQueue.GetConsumingEnumerable(cancellationToken))
                {
                    TryExecuteTask(task);
                }
            }
            catch (OperationCanceledException)
            { }
            finally
            {
                isExecuting = false;
            }
        }

        // Signaling this allows the task scheduler to finish after all tasks complete
        public void Complete() { taskQueue.CompleteAdding(); }
        protected override IEnumerable<Task> GetScheduledTasks() { return null; }

        protected override void QueueTask(Task task)
        {
            try
            {
                taskQueue.Add(task, cancellationToken);
            }
            catch (OperationCanceledException)
            { }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            // We'd need to remove the task from queue if it was already queued. 
            // That would be too hard.
            if (taskWasPreviouslyQueued) return false;

            return isExecuting && TryExecuteTask(task);
        }
    }
}