using Avalonia;
using Avalonia.Markup.Xaml;

namespace PrototypeAvalonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
