using Microsoft.Alm.Authentication;
using Microsoft.Alm.Authentication.Win32;
using System;
using System.Windows;

namespace GitHub.Authentication
{
    /// <summary>
    /// Interaction logic for Tester.xaml
    /// </summary>
    public partial class Tester : Window
    {
        public Tester()
        {
            InitializeComponent();
        }

        private void ShowCredentials(object sender, RoutedEventArgs e)
        {
            new CredentialsWindow(Win32RuntimeContext.Default, IntPtr.Zero).ShowDialog();
        }

        private void ShowAuthenticationCode(object sender, RoutedEventArgs e)
        {
            new TwoFactorWindow(Win32RuntimeContext.Default, IntPtr.Zero).ShowDialog();
        }
    }
}
