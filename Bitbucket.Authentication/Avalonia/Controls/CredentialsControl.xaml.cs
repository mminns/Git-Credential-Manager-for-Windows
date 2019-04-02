using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Atlassian.Bitbucket.Authentication.Avalonia.Controls
{
    public class CredentialsControl : UserControl
    {
        public CredentialsControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
