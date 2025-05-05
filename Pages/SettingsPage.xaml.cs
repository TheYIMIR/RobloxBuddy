using RobloxBuddy.Models;
using RobloxBuddy.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using MessageBox = System.Windows.MessageBox;

namespace RobloxBuddy.Pages
{
    public partial class SettingsPage : Page
    {
        private UserSettings _userSettings;

        public SettingsPage()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            try
            {
                _userSettings = SettingsManager.LoadSettings();

                // Update UI with current settings
                chkFriendNotifications.IsChecked = _userSettings.EnableFriendNotifications;
                chkGameNotifications.IsChecked = _userSettings.EnableGameNotifications;
                chkMinimizeToTray.IsChecked = _userSettings.MinimizeToTray;
                chkStartWithWindows.IsChecked = _userSettings.StartWithWindows;
                sldNotificationDuration.Value = _userSettings.NotificationDuration;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnSaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Update settings object
                _userSettings.EnableFriendNotifications = chkFriendNotifications.IsChecked ?? true;
                _userSettings.EnableGameNotifications = chkGameNotifications.IsChecked ?? true;
                _userSettings.MinimizeToTray = chkMinimizeToTray.IsChecked ?? true;
                _userSettings.StartWithWindows = chkStartWithWindows.IsChecked ?? false;
                _userSettings.NotificationDuration = (int)sldNotificationDuration.Value;

                // Save settings
                SettingsManager.SaveSettings(_userSettings);

                // Configure autostart if needed
                if (_userSettings.StartWithWindows)
                {
                    SetStartWithWindows(true);
                }
                else
                {
                    SetStartWithWindows(false);
                }

                MessageBox.Show("Settings saved successfully!", "Settings", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetStartWithWindows(bool enable)
        {
            try
            {
                // In a real implementation, this would add or remove the app from Windows startup
                // For demo purposes, this is just a placeholder
                string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                if (enable)
                {
                    // Add to startup (placeholder)
                    // Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).SetValue("RobloxBuddy", appPath);
                }
                else
                {
                    // Remove from startup (placeholder)
                    // Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).DeleteValue("RobloxBuddy", false);
                }
            }
            catch (Exception)
            {
                // Handle exception
            }
        }
    }
}