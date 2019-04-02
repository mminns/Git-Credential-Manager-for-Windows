using Avalonia;
using Avalonia.Markup.Xaml;

namespace Atlassian.Bitbucket.Authentication.Avalonia
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
