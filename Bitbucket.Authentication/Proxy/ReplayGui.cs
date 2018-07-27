using Atlassian.Bitbucket.Authentication.ViewModels;
using GitHub.Shared.Api;
using GitHub.Shared.Controls;
using GitHub.Shared.ViewModels;
using Microsoft.Alm.Authentication;
using Microsoft.Alm.Authentication.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.StringComparer;

namespace Atlassian.Bitbucket.Authentication.Test
{
    public class ReplayGui : IGui, IReplayService<CapturedGuiData>
    {
        internal ReplayGui(RuntimeContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            _captured = new Queue<CapturedGuiOperation>();
            _context = context;
            _replayed = new Stack<CapturedGuiOperation>();
            _syncpoint = new object();
        }

        private readonly Queue<CapturedGuiOperation> _captured;
        private readonly RuntimeContext _context;
        private readonly Stack<CapturedGuiOperation> _replayed;
        private readonly object _syncpoint;

        public string ServiceName
            => nameof(Gui);

        public Type ServiceType
            => typeof(IGui);

        public bool ShowViewModel(DialogViewModel viewModel, Func<AuthenticationDialogWindow> windowCreator)
        {
            if (!TryGetNext(out CapturedGuiOperation operation))
                throw new ReplayNotFoundException($"Failed to find next `{nameof(CapturedGuiOperation)}`.");
            if (!Ordinal.Equals(viewModel?.GetType().FullName, operation.DialogType))
                throw new ReplayInputTypeException($"Expected `{viewModel?.GetType().FullName}` vs. Actual `{operation.DialogType}`.");

            _context.Trace.WriteLine($"replay {nameof(ShowViewModel)}.");

            viewModel.IsValid = operation.Output.IsValid;
            viewModel.Result = (AuthenticationDialogResult)operation.Output.Result;

            switch (viewModel)
            {
                case CredentialsViewModel credentialsViewModel:
                    {
                        credentialsViewModel.Login = operation.Output.Login;
                        credentialsViewModel.Password = operation.Output.Password;
                    }
                    break;

                case OAuthViewModel oauthViewModel:
                    {
                    }
                    break;
            }

            return operation.Output.Success;
        }

        internal void SetReplayData(CapturedGuiData replayData)
        {
            lock (_syncpoint)
            {
                _captured.Clear();

                if (replayData.Operations != null)
                {
                    foreach (var operation in replayData.Operations)
                    {
                        _captured.Enqueue(operation);
                    }
                }

                _captured.TrimExcess();
            }
        }

        private bool TryGetNext(out CapturedGuiOperation operation)
        {
            lock (_syncpoint)
            {
                if (_captured.Count > 0)
                {
                    operation = _captured.Dequeue();
                    return true;
                }
            }

            operation = default(CapturedGuiOperation);
            return false;
        }

        void IReplayService<CapturedGuiData>.SetReplayData(CapturedGuiData replayData)
            => SetReplayData(replayData);

        void IReplayService.SetReplayData(object replayData)
        {
            if (!(replayData is CapturedGuiData guiData)
                && !CapturedGuiData.TryDeserialize(replayData, out guiData))
            {
                var inner = new System.IO.InvalidDataException($"Failed to deserialize data into `{nameof(CapturedGuiData)}`.");
                throw new ArgumentException(inner.Message, nameof(replayData), inner);
            }

            SetReplayData(guiData);
        }
    }
}
