using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using RobloxBuddy.Models;
using RobloxBuddy.Pages;
using RobloxBuddy.Services;
using System;
using System.Windows;

namespace RobloxBuddy
{
    public partial class MainWindow : Window
    {
        private UserSettings _userSettings;
        private BackgroundMonitorService _backgroundMonitor;
        private UserInfo _currentUser;

        public MainWindow()
        {
            InitializeComponent();

            // Get settings from ServiceLocator
            _userSettings = ServiceLocator.Get<UserSettings>();

            // Initialize background monitor
            _backgroundMonitor = new BackgroundMonitorService(
                ServiceLocator.Get<RobloxApiService>(),
                ServiceLocator.Get<NotificationService>(),
                _userSettings);

            NavigateToPage(new FriendsPage());

            // Setup notification click handler
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Handle notification clicks
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowWindow();
                    // Handle specific navigation based on the toast arguments
                    switch (toastArgs.Arguments)
                    {
                        case "viewFriend":
                            btnFriends_Click(this, null);
                            break;
                        case "viewGame":
                            btnGames_Click(this, null);
                            break;
                        default:
                            break;
                    }
                });
            };

            // Check for saved credentials and auto-login
            if (!string.IsNullOrEmpty(_userSettings.RobloxToken))
            {
                // Auto-login logic here
                try
                {
                    var robloxApiService = ServiceLocator.Get<RobloxApiService>();
                    AuthenticateAsync();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Auto-login failed: {ex.Message}");
                }
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
                System.Windows.MessageBox.Show($"Authentication error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void NavigateToPage(object page)
        {
            MainFrame.Navigate(page);
        }

        private void btnFriends_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new FriendsPage());
        }

        private void btnGames_Click(object sender, RoutedEventArgs e)
        {
            NavigateToPage(new GamesPage());
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
            if (loginWindow.ShowDialog() == true)
            {
                // Authenticate and update UI
                AuthenticateAsync();
            }
        }

        private void UpdateLoginStatus(string username)
        {
            txtUsername.Text = username;
            btnLogin.Content = "Logout";
            btnLogin.Click -= btnLogin_Click;
            btnLogin.Click += btnLogout_Click;
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
            txtUsername.Text = "Not logged in";
            btnLogin.Content = "Login";
            btnLogin.Click -= btnLogout_Click;
            btnLogin.Click += btnLogin_Click;

            // Reset current user
            _currentUser = null;

            // Navigate to friends page
            NavigateToPage(new FriendsPage());
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
            // Stop background monitoring
            _backgroundMonitor?.StopMonitoring();

            base.OnClosed(e);
        }
    }
}