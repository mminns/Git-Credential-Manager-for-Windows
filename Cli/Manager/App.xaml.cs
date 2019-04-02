using Avalonia;
using Avalonia.Markup.Xaml;

namespace Microsoft.Alm.Cli
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
