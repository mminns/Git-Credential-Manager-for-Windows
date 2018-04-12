using System;
using Xamarin.Forms.Platform.WPF;

namespace Bitbucket.Authentication.Tester.Core.Wpf
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            Xamarin.Forms.Forms.Init();
            var page = new FormsApplicationPage();
            Xamarin.FormsMaps.Init("");
            page.LoadApplication(new Controls.App());
        }
    }
}
