using Microsoft.Toolkit.Uwp.Notifications;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using RobloxBuddy.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace RobloxBuddy.Services
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly UserSettings _settings;

        public NotificationService()
        {
            _httpClient = new HttpClient();
            _settings = ServiceLocator.Get<UserSettings>();
        }

        public void ShowFriendOnlineNotification(string friendName, string activity, string avatarUrl = null)
        {
            if (!_settings.EnableFriendNotifications)
                return;

            var builder = new ToastContentBuilder()
                .AddArgument("action", "viewFriend")
                .AddArgument("friendName", friendName)
                .AddText("Friend Online")
                .AddText($"{friendName} is now online")
                .AddText(activity);

            // Add avatar image if available
            if (!string.IsNullOrEmpty(avatarUrl))
            {
                try
                {
                    // In a real app, download and cache the image
                    string localPath = DownloadImageAsync(avatarUrl, $"{friendName}_avatar.png").Result;
                    if (File.Exists(localPath))
                    {
                        builder.AddAppLogoOverride(new Uri(localPath), ToastGenericAppLogoCrop.Circle);
                    }
                }
                catch
                {
                    // Ignore errors with image download
                }
            }

            // For .NET 8, set toast expiration
            var toastNotification = builder.GetToastNotification();
            toastNotification.ExpirationTime = DateTime.Now.AddSeconds(_settings.NotificationDuration);

            // Show toast
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        public void ShowGameUpdateNotification(string gameName, string updateInfo, string thumbnailUrl = null)
        {
            if (!_settings.EnableGameNotifications)
                return;

            var builder = new ToastContentBuilder()
                .AddArgument("action", "viewGame")
                .AddArgument("gameName", gameName)
                .AddText("Game Update")
                .AddText(gameName)
                .AddText(updateInfo);

            // Add game thumbnail if available
            if (!string.IsNullOrEmpty(thumbnailUrl))
            {
                try
                {
                    // In a real app, download and cache the image
                    string localPath = DownloadImageAsync(thumbnailUrl, $"{gameName}_thumbnail.png").Result;
                    if (File.Exists(localPath))
                    {
                        builder.AddHeroImage(new Uri(localPath));
                    }
                }
                catch
                {
                    // Ignore errors with image download
                }
            }

            // For .NET 8, set toast expiration
            var toastNotification = builder.GetToastNotification();
            toastNotification.ExpirationTime = DateTime.Now.AddSeconds(_settings.NotificationDuration);

            // Show toast
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        public void ShowCustomNotification(string title, string message, string action = null, string imageUrl = null)
        {
            var builder = new ToastContentBuilder()
                .AddText(title)
                .AddText(message);

            if (!string.IsNullOrEmpty(action))
            {
                builder.AddArgument("action", action);
            }

            // Add image if available
            if (!string.IsNullOrEmpty(imageUrl))
            {
                try
                {
                    // In a real app, download and cache the image
                    string localPath = DownloadImageAsync(imageUrl, $"notification_{Guid.NewGuid()}.png").Result;
                    if (File.Exists(localPath))
                    {
                        builder.AddInlineImage(new Uri(localPath));
                    }
                }
                catch
                {
                    // Ignore errors with image download
                }
            }

            // For .NET 8, set toast expiration
            var toastNotification = builder.GetToastNotification();
            toastNotification.ExpirationTime = DateTime.Now.AddSeconds(_settings.NotificationDuration);

            // Show toast
            ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
        }

        private async Task<string> DownloadImageAsync(string url, string filename)
        {
            try
            {
                string cacheFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "RobloxBuddy", "Cache");

                if (!Directory.Exists(cacheFolder))
                {
                    Directory.CreateDirectory(cacheFolder);
                }

                string localPath = Path.Combine(cacheFolder, filename);

                // Check if file already exists (cache)
                if (File.Exists(localPath))
                {
                    return localPath;
                }

                // Download the image
                var imageBytes = await _httpClient.GetByteArrayAsync(url);
                await File.WriteAllBytesAsync(localPath, imageBytes);

                return localPath;
            }
            catch
            {
                return null;
            }
        }

        // Add this method to initialize the toast notification activation
        public static void InitializeNotifications(string appId = "RobloxBuddy")
        {
            // Register COM server and activator
            ToastNotificationManagerCompat.OnActivated += toastArgs =>
            {
                // Get the arguments from the notification
                var args = toastArgs.Argument;

                // Dispatch to UI thread
                App.Current.Dispatcher.Invoke(() =>
                {
                    // Handle notification activation
                    if (App.Current.MainWindow is MainWindow mainWindow)
                    {
                        mainWindow.ShowWindow();

                        // Handle specific actions
                        switch (args)
                        {
                            case "viewFriend":
                                // Navigate to friends page
                                mainWindow.NavigateToFriendsPage();
                                break;
                            case "viewGame":
                                // Navigate to games page
                                mainWindow.NavigateToGamesPage();
                                break;
                        }
                    }
                });
            };
        }
    }
}