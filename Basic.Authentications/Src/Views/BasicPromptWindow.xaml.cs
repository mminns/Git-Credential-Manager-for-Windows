using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GitHub.Shared.ViewModels;

namespace Basic.Authentication.Avalonia.Views
{
    public class BasicPromptWindow : Window
    {
        public BasicPromptWindow()
        {
            this.InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DataContextChanged += (s, e) =>
            {
                var window = s as Window;
                if (window.DataContext is ViewModel viewModel)
                {
                    viewModel.PropertyChanged += HandleDialogResult;
                }
                //var oldViewModel = e.OldValue as ViewModel;
                //if (oldViewModel != null)
                //{
                //    oldViewModel.PropertyChanged -= HandleDialogResult;
                //}
                //DataContext = e.NewValue;
                //if (DataContext != null)
                //{
                //    ((ViewModel)DataContext).PropertyChanged += HandleDialogResult;
                //}

                int i = 0;
            };

            //new WindowInteropHelper(this).Owner = parentHwnd;
        }

        //protected AuthenticationDialogWindow(RuntimeContext context)
        //    : this(context, IntPtr.Zero)
        //{ }

        private void HandleDialogResult(object sender, PropertyChangedEventArgs e)
        {
            var viewModel = sender as DialogViewModel;
            if (viewModel == null) return;
            if (e.PropertyName == nameof(DialogViewModel.Result))
            {
                if (viewModel.Result != AuthenticationDialogResult.None)
                {
                    Close();
                }
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
