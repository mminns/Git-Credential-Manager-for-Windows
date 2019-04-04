using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Basic.Authentication.Avalonia.Controls
{
    public class BasicPromptControl : UserControl
    {
        public BasicPromptControl()
        {
            this.InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
