using System.Windows.Input;
using GitHub.Shared.Helpers;
using GitHub.Shared.ViewModels;
using GitHub.Shared.ViewModels.Validation;

namespace Basic.Authentication.Avalonia.ViewModels
{
    public class BasicPromptViewModel : DialogViewModel
    {
        public BasicPromptViewModel() : this(string.Empty, string.Empty)
        {
            // without this default constructor get nullreferenceexceptions during binding i guess
            // 'cos the view is built before the 'official' viewmodel and hence generates it own
            // viewmodel while building?
        }

        public BasicPromptViewModel(string username, string message)
        {
            LoginCommand = new ActionCommand(_ =>
            {
                Result = AuthenticationDialogResult.Ok;
            });
            CancelCommand = new ActionCommand(_ =>
            {

                Result = AuthenticationDialogResult.Cancel;
            });

            LoginValidator = PropertyValidator.For(this, x => x.Login).Required("Login Required");

            PasswordValidator = PropertyValidator.For(this, x => x.Password).Required("Password Required");

            ModelValidator = new ModelValidator(LoginValidator, PasswordValidator);
            ModelValidator.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(ModelValidator.IsValid))
                {
                    IsValid = ModelValidator.IsValid;
                }
            };

            // set last to allow validator to run
            if (!string.IsNullOrWhiteSpace(username))
            {
                Login = username;
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                _message = message;
            }
        }
        private string _login;

        /// <summary>
        /// Bitbucket login which is either the user name or email address.
        /// </summary>
        public string Login
        {
            get { return _login; }
            set
            {
                _login = value;
                RaisePropertyChangedEvent(nameof(Login));
            }
        }

        private string _message;

        /// <summary>
        /// Message
        /// </summary>
        public string Message
        {
            get { return _message; }
        }

        public PropertyValidator<string> LoginValidator { get; }

        private string _password;

        /// <summary>
        /// Bitbucket login which is either the user name or email address.
        /// </summary>
        public string Password
        {
            get { return _password; }
            set
            {
                // Hack: Because we're binding one way to source, we need to skip the initial value
                // that's sent when the binding is setup by the XAML
                if (_password == null && value == null)
                {
                    return;
                }

                _password = value;
                RaisePropertyChangedEvent(nameof(Password));
            }
        }

        public PropertyValidator<string> PasswordValidator { get; }

        public ModelValidator ModelValidator { get; }

        /// <summary>
        /// Start the process to validate the username/password
        /// </summary>
        public ICommand LoginCommand { get; }

        /// <summary>
        /// Cancel the authentication attempt.
        /// </summary>
        public ICommand CancelCommand { get; }

    }
}