using RobloxBuddy.Models;
using RobloxBuddy.Services;
using System;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace RobloxBuddy
{
    public partial class LoginWindow : Window
    {
        private RobloxApiService _robloxApiService;

        public string Username { get; private set; }
        public string Token { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();

            try
            {
                // Get the API service from ServiceLocator
                _robloxApiService = ServiceLocator.Get<RobloxApiService>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing login window: {ex.Message}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Get reference to the button that triggered this event
            var loginButton = sender as System.Windows.Controls.Button;

            try
            {
                // Validate input fields exist
                if (txtUsername == null || txtPassword == null)
                {
                    MessageBox.Show("UI controls not properly initialized.",
                        "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string username = txtUsername.Text;
                string password = txtPassword.Password;

                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter both username and password.",
                        "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Show loading indicator using the sender button
                if (loginButton != null)
                {
                    loginButton.IsEnabled = false;
                    loginButton.Content = "Logging in...";
                }

                // Validate API service
                if (_robloxApiService == null)
                {
                    _robloxApiService = ServiceLocator.Get<RobloxApiService>();
                    if (_robloxApiService == null)
                    {
                        throw new InvalidOperationException("Could not get the Roblox API service.");
                    }
                }

                // Attempt to login with the API
                Token = await _robloxApiService.LoginAsync(username, password);

                if (!string.IsNullOrEmpty(Token))
                {
                    // Save credentials in settings
                    try
                    {
                        UserSettings settings = ServiceLocator.Get<UserSettings>();
                        if (settings != null)
                        {
                            settings.RobloxToken = Token;
                            SettingsManager.SaveSettings(settings);
                        }

                        // Set the properties for the main window to use
                        this.Username = username;

                        this.DialogResult = true;
                        this.Close();
                    }
                    catch (Exception settingsEx)
                    {
                        MessageBox.Show($"Login successful, but there was an error saving your settings: {settingsEx.Message}",
                            "Settings Error", MessageBoxButton.OK, MessageBoxImage.Warning);

                        // Still consider this a successful login
                        this.Username = username;
                        this.DialogResult = true;
                        this.Close();
                    }
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
                // Reset UI state using the sender button
                if (loginButton != null)
                {
                    loginButton.IsEnabled = true;
                    loginButton.Content = "Login";
                }
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}