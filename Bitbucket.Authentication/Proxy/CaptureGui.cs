using Atlassian.Bitbucket.Authentication.ViewModels;
using GitHub.Shared.Api;
using GitHub.Shared.Controls;
using GitHub.Shared.ViewModels;
using Microsoft.Alm.Authentication;
using Microsoft.Alm.Authentication.Test;
using System;
using System.Collections.Generic;

namespace Atlassian.Bitbucket.Authentication.Test
{
    public class CaptureGui : IGui, ICaptureService<CapturedGuiData>
    {
        internal const string FauxPassword = "is!realPassword?";
        internal const string FauxUsername = "tester";

        internal CaptureGui(RuntimeContext context, IGui gui)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));
            if (gui is null)
                throw new ArgumentNullException(nameof(gui));

            _captured = new Queue<CapturedGuiOperation>();
            _context = context;
            _gui = gui;
            _syncpoint = new object();
        }

        private readonly Queue<CapturedGuiOperation> _captured;
        private readonly RuntimeContext _context;
        private readonly IGui _gui;
        private readonly object _syncpoint;

        public string ServiceName
            => "Gui";

        public Type ServiceType
            => typeof(IGui);

        public bool ShowViewModel(DialogViewModel viewModel, Func<AuthenticationDialogWindow> windowCreator)
        {
            _context.Trace.WriteLine($"capture {nameof(ShowViewModel)}.");

            var success = _gui.ShowViewModel(viewModel, windowCreator);

            Capture(success, viewModel);

            return success;
        }

        private void Capture(bool success, DialogViewModel viewModel)
        {
            var capture = default(CapturedGuiOperation);

            switch (viewModel)
            {
                case CredentialsViewModel credentialsViewModel:
                    {
                        capture = new CapturedGuiOperation
                        {
                            Output = new CapturedGuiOutput
                            {
                                Login = credentialsViewModel.Login,
                                IsValid = viewModel.IsValid,
                                Password = credentialsViewModel.Password,
                                Result = (int)viewModel.Result,
                                Success = success,
                            },
                            DialogType = credentialsViewModel.GetType().FullName,
                        };
                    }
                    break;

                case OAuthViewModel oauthViewModel:
                    {
                        capture = new CapturedGuiOperation
                        {
                            Output = new CapturedGuiOutput
                            {
                                UsesOAuth = true,
                                IsValid = viewModel.IsValid,
                                Result = (int)viewModel.Result,
                                Success = success,
                            },
                            DialogType = oauthViewModel.GetType().FullName,
                        };
                    }
                    break;

                default:
                    throw new ReplayDataException($"Unknown type `{viewModel.GetType().FullName}`");
            }

            lock (_syncpoint)
            {
                _captured.Enqueue(capture);
            }
        }


        internal bool GetCapturedData(ICapturedDataFilter filter, out CapturedGuiData capturedData)
        {
            if (filter is null)
                throw new ArgumentNullException(nameof(filter));

            lock (_syncpoint)
            {
                capturedData = new CapturedGuiData
                {
                    Operations = new List<CapturedGuiOperation>(_captured.Count),
                };

                foreach (var item in _captured)
                {
                    var operation = new CapturedGuiOperation
                    {
                        Output = new CapturedGuiOutput
                        {
                            UsesOAuth = item.Output.UsesOAuth,
                            IsValid = item.Output.IsValid,
                            Login = item.Output.Login != null
                                ? FauxUsername
                                : null,
                            Password = item.Output.Password != null
                                ? FauxPassword
                                : null,
                            Result = item.Output.Result,
                            Success = item.Output.Success,
                        },
                        DialogType = item.DialogType,
                    };

                    capturedData.Operations.Add(operation);
                }
            }

            return true;
        }

        bool ICaptureService<CapturedGuiData>.GetCapturedData(ICapturedDataFilter filter, out CapturedGuiData capturedData)
            => GetCapturedData(filter, out capturedData);

        bool ICaptureService.GetCapturedData(ICapturedDataFilter filter, out object capturedData)
        {
            if (GetCapturedData(filter, out CapturedGuiData guiData))
            {
                capturedData = guiData;
                return true;
            }

            capturedData = null;
            return false;
        }
    }
}
