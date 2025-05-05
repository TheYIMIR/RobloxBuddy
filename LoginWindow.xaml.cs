using RobloxBuddy.Models;
using RobloxBuddy.Services;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace RobloxBuddy
{
    public partial class LoginWindow : Window
    {
        private readonly RobloxApiService _robloxApiService;

        public string Username { get; private set; }
        public string Token { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            _robloxApiService = ServiceLocator.Get<RobloxApiService>();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show loading indicator
            btnLogin.IsEnabled = false;
            btnLogin.Content = "Logging in...";

            try
            {
                // Attempt to login with the API
                Token = await _robloxApiService.LoginAsync(username, password);

                if (!string.IsNullOrEmpty(Token))
                {
                    // Save credentials in settings
                    UserSettings settings = ServiceLocator.Get<UserSettings>();
                    settings.RobloxToken = Token;
                    SettingsManager.SaveSettings(settings);

                    // Set the properties for the main window to use
                    this.Username = username;

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Login failed. Please check your credentials and try again.",
                        "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login failed: {ex.Message}",
                    "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Reset UI state
                btnLogin.IsEnabled = true;
                btnLogin.Content = "Login";
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}