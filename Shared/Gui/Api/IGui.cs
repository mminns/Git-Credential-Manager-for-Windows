using GitHub.Shared.Controls;
using GitHub.Shared.ViewModels;
using Microsoft.Alm.Authentication;
using System;
using System.Collections.Generic;
using System.Text;

namespace GitHub.Shared.Api
{
    public interface IGui : IRuntimeService
    {
        /// <summary>
        /// Presents the user with `<paramref name="windowCreator"/>` with the `<paramref name="viewModel"/>`.
        /// <para/>
        /// Returns `<see langword="true"/>` if the user completed the dialog; otherwise `<see langword="false"/>` if the user canceled or abandoned the dialog.
        /// </summary>
        /// <param name="viewModel">The view model passed to the presented window.</param>
        /// <param name="windowCreator">Creates the window `<paramref name="viewModel"/>` is passed to.</param>
        bool ShowViewModel(DialogViewModel viewModel, Func<AuthenticationDialogWindow> windowCreator);
    }
}
