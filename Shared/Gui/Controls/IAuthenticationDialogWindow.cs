using System;
using System.Collections.Generic;
using System.Text;
using GitHub.Shared.ViewModels;

namespace GitHub.Shared.Controls
{
    public interface IAuthenticationDialogWindow
    {
        object DataContext { get; set; }
        bool? ShowDialog();
    }
}
