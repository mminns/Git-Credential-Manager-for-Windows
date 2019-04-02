using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Atlassian.Bitbucket.Authentication.Avalonia.Controls
{
    public class OAuthControl : UserControl
    {
        public OAuthControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
