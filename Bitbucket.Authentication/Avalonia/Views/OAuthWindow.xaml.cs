using System;
using System.Threading.Tasks;
using Atlassian.Bitbucket.Authentication.Avalonia.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using GitHub.Shared.Controls;
using Microsoft.Alm.Authentication;

namespace Atlassian.Bitbucket.Authentication.Avalonia.Views
{
    public class OAuthWindow : AuthenticationDialogWindow
    {
        public OAuthWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
