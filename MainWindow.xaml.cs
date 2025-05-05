using RobloxBuddy.Models;
using RobloxBuddy.Pages;
using RobloxBuddy.Services;
using System;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace RobloxBuddy
{
    public partial class MainWindow : Window
    {
        private UserSettings _userSettings;
        private BackgroundMonitorService _backgroundMonitor;
        private UserInfo _currentUser;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

                // Get settings from ServiceLocator
                _userSettings = ServiceLocator.Get<UserSettings>();

                // Initialize background monitor
                _backgroundMonitor = new BackgroundMonitorService(
                    ServiceLocator.Get<RobloxApiService>(),
                    ServiceLocator.Get<NotificationService>(),
                    _userSettings);

                // Start with friends page
                NavigateToFriendsPage();

                // Check for saved credentials and auto-login
                if (!string.IsNullOrEmpty(_userSettings.RobloxToken))
                {
                    // Auto-login logic here
                    try
                    {
                        AuthenticateAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Auto-login failed: {ex.Message}",
                            "Login Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}",
                    "Initialization Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void AuthenticateAsync()
        {
            try
            {
                var robloxApiService = ServiceLocator.Get<RobloxApiService>();

                // Load user info
                _currentUser = await robloxApiService.GetCurrentUserInfoAsync();

                if (_currentUser != null)
                {
                    // Update UI
                    UpdateLoginStatus(_currentUser.Username);

                    // Start monitoring for friend activity
                    StartFriendMonitoring(_currentUser.UserId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authentication error: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StartFriendMonitoring(long userId)
        {
            // Start background monitoring for friend status changes
            if (_userSettings.EnableFriendNotifications)
            {
                _backgroundMonitor.StartMonitoring(userId);
            }
        }

        // Navigation methods
        public void NavigateToPage(object page)
        {
            if (MainFrame != null)
            {
                MainFrame.Navigate(page);
            }
        }

        public void NavigateToFriendsPage()
        {
            NavigateToPage(new FriendsPage());
        }

        public void NavigateToGamesPage()
        {
            NavigateToPage(new GamesPage());
        }

        // UI Event handlers
        private void btnFriends_Click(object sender, RoutedEventArgs e)
        {
            NavigateToFriendsPage();
        }

        private void btnGames_Click(object sender, RoutedEventArgs e)
        {
            NavigateToGamesPage();
        }

        private void btnAvatar_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new AvatarPage());
        }

        private void btnInventory_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new InventoryPage());
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new SettingsPage());
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Owner = this; // Set owner for proper modal behavior
            if (loginWindow.ShowDialog() == true)
            {
                // Authenticate and update UI
                AuthenticateAsync();
            }
        }

        private void UpdateLoginStatus(string username)
        {
            if (txtUsername != null && btnLogin != null)
            {
                txtUsername.Text = username;
                btnLogin.Content = "Logout";

                // Remove old handler and add new one
                btnLogin.Click -= btnLogin_Click;
                btnLogin.Click += btnLogout_Click;
            }
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Stop background monitoring
            _backgroundMonitor.StopMonitoring();

            // Logout logic
            ServiceLocator.Get<RobloxApiService>().Logout();
            _userSettings.RobloxToken = null;
            SettingsManager.SaveSettings(_userSettings);

            // Update UI
            if (txtUsername != null)
                txtUsername.Text = "Not logged in";

            if (btnLogin != null)
            {
                btnLogin.Content = "Login";
                btnLogin.Click -= btnLogout_Click;
                btnLogin.Click += btnLogin_Click;
            }

            // Reset current user
            _currentUser = null;

            // Navigate to friends page
            NavigateToFriendsPage();
        }

        public void ShowWindow()
        {
            if (WindowState == WindowState.Minimized)
            {
                WindowState = WindowState.Normal;
            }

            Show();
            Activate();
        }

        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Stop background monitoring
                if (_backgroundMonitor != null)
                {
                    _backgroundMonitor.StopMonitoring();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping background monitor: {ex.Message}");
            }

            base.OnClosed(e);
        }
    }
}