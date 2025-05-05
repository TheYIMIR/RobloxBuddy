using RobloxBuddy.Models;
using RobloxBuddy.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Application = System.Windows.Application;
using Button = System.Windows.Controls.Button;
using ListView = System.Windows.Controls.ListView;
using MessageBox = System.Windows.MessageBox;

namespace RobloxBuddy.Pages
{
    public partial class FriendsPage : Page
    {
        private RobloxApiService _robloxApiService;
        private NotificationService _notificationService;
        private UserSettings _userSettings;
        private List<Friend> _allFriends;
        private bool _isLoading = false;

        public FriendsPage()
        {
            InitializeComponent();

            _robloxApiService = ServiceLocator.Get<RobloxApiService>();
            _notificationService = ServiceLocator.Get<NotificationService>();
            _userSettings = ServiceLocator.Get<UserSettings>();

            // Show loading indicator
            ShowLoadingState(true);

            // Load friends async
            LoadFriendsAsync();

            // Setup search functionality
            txtSearch.TextChanged += TxtSearch_TextChanged;
        }

        private async void LoadFriendsAsync()
        {
            try
            {
                _isLoading = true;
                ShowLoadingState(true);

                // Check if logged in
                if (_robloxApiService.IsLoggedIn)
                {
                    // Get current user ID
                    var currentUser = await _robloxApiService.GetCurrentUserInfoAsync();

                    if (currentUser != null)
                    {
                        _allFriends = await _robloxApiService.GetFriendsAsync(currentUser.UserId);
                    }
                    else
                    {
                        _allFriends = new List<Friend>();
                        MessageBox.Show("Could not determine current user. Please log in again.",
                            "Authentication Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    // Not logged in, show demo data
                    _allFriends = GetDemoFriends();
                    ShowLoginMessage(true);
                }

                // Update UI
                UpdateFriendsLists();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading friends: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _allFriends = new List<Friend>();
            }
            finally
            {
                _isLoading = false;
                ShowLoadingState(false);
            }
        }

        private void UpdateFriendsLists()
        {
            // Apply search filter if needed
            var filteredFriends = ApplySearchFilter(_allFriends);

            // Update each tab's content
            lvAllFriends.ItemsSource = filteredFriends;
            lvOnlineFriends.ItemsSource = filteredFriends.Where(f => f.IsOnline).ToList();
            lvFavoriteFriends.ItemsSource = filteredFriends.Where(f => f.IsFavorite).ToList();

            // Update empty state visibility
            UpdateEmptyStates(filteredFriends);
        }

        private List<Friend> ApplySearchFilter(List<Friend> friends)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                return friends;
            }

            string searchTerm = txtSearch.Text.ToLower();
            return friends.Where(f =>
                f.Username.ToLower().Contains(searchTerm) ||
                f.DisplayName.ToLower().Contains(searchTerm) ||
                (f.GameName != null && f.GameName.ToLower().Contains(searchTerm))
            ).ToList();
        }

        private void UpdateEmptyStates(List<Friend> filteredFriends)
        {
            txtEmptyAllFriends.Visibility = filteredFriends.Any() ? Visibility.Collapsed : Visibility.Visible;
            txtEmptyOnlineFriends.Visibility = filteredFriends.Any(f => f.IsOnline) ? Visibility.Collapsed : Visibility.Visible;
            txtEmptyFavoriteFriends.Visibility = filteredFriends.Any(f => f.IsFavorite) ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ShowLoadingState(bool isLoading)
        {
            progressLoading.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            tabControl.IsEnabled = !isLoading;
        }

        private void ShowLoginMessage(bool show)
        {
            loginPrompt.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }

        private List<Friend> GetDemoFriends()
        {
            // Return some demo friends for when not logged in
            return new List<Friend>
            {
                new Friend {
                    UserId = 11111111,
                    Username = "CoolFriend1",
                    DisplayName = "Cool Friend",
                    IsOnline = true,
                    GameName = "Adopt Me!",
                    GameId = 920587237,
                    LastLocation = "Playing",
                    AvatarUrl = "https://tr.rbxcdn.com/e4ecd709b1b4426bbd8099422f267944/150/150/AvatarHeadshot/Png",
                    IsFavorite = true
                },
                new Friend {
                    UserId = 22222222,
                    Username = "SuperGamer42",
                    DisplayName = "Super Gamer",
                    IsOnline = true,
                    GameName = "Brookhaven RP",
                    GameId = 4924922222,
                    LastLocation = "Playing",
                    AvatarUrl = "https://tr.rbxcdn.com/1a1a1a1a1b4426bbd8099422f267944/150/150/AvatarHeadshot/Png",
                    IsFavorite = true
                },
                new Friend {
                    UserId = 33333333,
                    Username = "BuildMaster99",
                    DisplayName = "Master Builder",
                    IsOnline = false,
                    GameName = null,
                    GameId = null,
                    LastLocation = "Offline",
                    AvatarUrl = "https://tr.rbxcdn.com/2b2b2b2b1b4426bbd8099422f267944/150/150/AvatarHeadshot/Png",
                    IsFavorite = false
                }
            };
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isLoading)
            {
                UpdateFriendsLists();
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            UpdateFriendsLists();
        }

        private void lvFriends_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            Friend selectedFriend = listView.SelectedItem as Friend;

            if (selectedFriend != null)
            {
                // Open friend details
                ShowFriendDetails(selectedFriend);

                listView.SelectedItem = null; // Reset selection
            }
        }

        private void ShowFriendDetails(Friend friend)
        {
            // In a real app, this would open a detailed view
            // For demo, show a message box
            MessageBox.Show($"Selected friend: {friend.DisplayName} ({friend.Username})\n" +
                            $"Status: {(friend.IsOnline ? "Online" : "Offline")}\n" +
                            $"Location: {friend.LastLocation}",
                            "Friend Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void btnJoinFriend_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Friend friend = button.DataContext as Friend;

            if (friend != null && friend.GameId.HasValue)
            {
                // Attempt to join the friend's game
                JoinGame(friend.GameId.Value, $"Joining {friend.DisplayName}");
            }
            else
            {
                MessageBox.Show("Cannot join friend - not in a joinable game.",
                    "Join Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void btnToggleFavorite_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Friend friend = button.DataContext as Friend;

            if (friend != null)
            {
                // Toggle favorite status
                friend.IsFavorite = !friend.IsFavorite;

                // Update settings
                if (friend.IsFavorite && !_userSettings.FavoriteFriends.Contains(friend.UserId))
                {
                    _userSettings.FavoriteFriends.Add(friend.UserId);
                }
                else if (!friend.IsFavorite && _userSettings.FavoriteFriends.Contains(friend.UserId))
                {
                    _userSettings.FavoriteFriends.Remove(friend.UserId);
                }

                // Save settings
                SettingsManager.SaveSettings(_userSettings);

                // Update UI
                UpdateFriendsLists();
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadFriendsAsync();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Open login window from main window
            if (Application.Current.MainWindow is MainWindow mainWindow)
            {
                LoginWindow loginWindow = new LoginWindow();
                if (loginWindow.ShowDialog() == true)
                {
                    // Refresh the page
                    LoadFriendsAsync();
                    ShowLoginMessage(false);
                }
            }
        }

        private void JoinGame(long placeId, string action)
        {
            try
            {
                // Launch Roblox with the place ID
                Process.Start(new ProcessStartInfo
                {
                    FileName = $"roblox://placeId={placeId}",
                    UseShellExecute = true
                });

                // Show a notification
                _notificationService.ShowCustomNotification(
                    "Joining Game",
                    action,
                    "gameJoined");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to join game: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}