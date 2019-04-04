using Avalonia;
using Avalonia.Markup.Xaml;

namespace Basic.Authentication.Avalonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
