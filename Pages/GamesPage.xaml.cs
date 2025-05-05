using RobloxBuddy.Models;
using RobloxBuddy.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RobloxBuddy.Pages
{
    public partial class GamesPage : Page
    {
        private RobloxApiService _robloxApiService;
        private UserSettings _userSettings;
        private List<Game> _allGames;

        public GamesPage()
        {
            InitializeComponent();

            _userSettings = SettingsManager.LoadSettings();
            _robloxApiService = new RobloxApiService(_userSettings);

            LoadGames();
        }

        private void LoadGames()
        {
            try
            {
                // In a real implementation, this would fetch real data from the API
                _allGames = _robloxApiService.GetPopularGames();

                // Update UI
                lvPopularGames.ItemsSource = _allGames;
                lvRecentGames.ItemsSource = _robloxApiService.GetRecentlyPlayedGames();
                lvFavoriteGames.ItemsSource = _allGames.Where(g => g.IsFavorite).ToList();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading games: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void lvGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView listView = sender as ListView;
            Game selectedGame = listView.SelectedItem as Game;

            if (selectedGame != null)
            {
                // In a real implementation, this would open the game details
                // For demo purposes, just show basic info
                System.Windows.MessageBox.Show($"Selected game: {selectedGame.Name}\n{selectedGame.Description}",
                                "Game Selected", MessageBoxButton.OK, MessageBoxImage.Information);

                listView.SelectedItem = null; // Reset selection
            }
        }

        private void btnPlayGame_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            Game game = button.DataContext as Game;

            if (game != null)
            {
                // In a real implementation, this would launch the game via Roblox protocol
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = $"roblox://placeID={game.GameId}",
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to launch game: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}