using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using PDV.Domain.Interfaces;
using PDV.UI.WinUI3.Services;
using System;
using System.Threading.Tasks;

namespace PDV.UI.WinUI3.Views
{
    public sealed partial class LoginPage : Page
    {
        private IAuthenticationService _authService;
        private bool _isLoggingIn = false;

        public LoginPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Start the animation
            LoadAnimation.Begin();

            // Get the authentication service
            var app = Microsoft.UI.Xaml.Application.Current as App;
            var services = app?.Services;
            if (services != null)
            {
                _authService = services.GetService<IAuthenticationService>();
            }

            // Set focus to username field
            UsernameTextBox.Focus(FocusState.Programmatic);

            // Check if we have saved credentials
            CheckForSavedCredentials();
        }

        private void CheckForSavedCredentials()
        {
            // In a real implementation, you would load these from secure storage
            // For now, we'll just leave it empty
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await AttemptLoginAsync();
        }

        private void InputField_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Don't try to log in again if we're already processing
                if (!_isLoggingIn)
                {
                    _ = AttemptLoginAsync();
                }
            }
        }

        private async Task AttemptLoginAsync()
        {
            // Avoid multiple login attempts simultaneously
            if (_isLoggingIn)
                return;

            _isLoggingIn = true;

            // Reset error state
            ErrorInfoBar.IsOpen = false;

            // Get the credentials
            string username = UsernameTextBox.Text?.Trim();
            string password = PasswordBox.Password;

            // Validate input
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorInfoBar.Title = "Campos Vazios";
                ErrorInfoBar.Message = "Por favor, preencha todos os campos.";
                ErrorInfoBar.IsOpen = true;
                _isLoggingIn = false;
                return;
            }

            try
            {
                // Show loading state (in a real app, consider adding a loading indicator)
                this.IsEnabled = false;

                // Attempt authentication
                bool isValid = await _authService.ValidateCredentialsAsync(username, password);

                if (isValid)
                {
                    // Get the user details
                    var employee = await _authService.GetEmployeeByUsernameAsync(username);

                    // Store in session
                    SessionService.Instance.CurrentUser = employee;

                    // Save credentials if selected
                    if (RememberMeCheckBox.IsChecked == true)
                    {
                        SaveCredentials(username);
                    }

                    // Navigate to main page
                    NavigateToMainApp();
                }
                else
                {
                    // Show error message
                    ErrorInfoBar.Title = "Login Falhou";
                    ErrorInfoBar.Message = "Nome de usuário ou senha incorretos.";
                    ErrorInfoBar.IsOpen = true;

                    // Clear password field
                    PasswordBox.Password = string.Empty;
                    PasswordBox.Focus(FocusState.Programmatic);
                }
            }
            catch (Exception ex)
            {
                // Show error message
                ErrorInfoBar.Title = "Erro";
                ErrorInfoBar.Message = $"Ocorreu um erro ao tentar fazer login: {ex.Message}";
                ErrorInfoBar.IsOpen = true;
            }
            finally
            {
                this.IsEnabled = true;
                _isLoggingIn = false;
            }
        }

        private void NavigateToMainApp()
        {
            // In a real implementation, this would navigate to the main app window
            App.MainWindow.DispatcherQueue.TryEnqueue(() =>
            {
                App.MainWindow.Activate();

                // Find the Frame control in MainWindow
                var mainWindow = App.MainWindow as MainWindow;
                mainWindow?.NavigateToHomePage();
            });
        }

        private void SaveCredentials(string username)
        {
            // In a real implementation, you would save credentials securely
            // For now, we'll just leave it as a placeholder
        }

        private void ForgotPassword_Click(object sender, RoutedEventArgs e)
        {
            // Show password recovery dialog
            ShowPasswordRecoveryDialog();
        }

        private async void ShowPasswordRecoveryDialog()
        {
            ContentDialog dialog = new ContentDialog
            {
                Title = "Recuperação de Senha",
                Content = "Entre em contato com o administrador do sistema para redefinir sua senha.",
                PrimaryButtonText = "OK",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
