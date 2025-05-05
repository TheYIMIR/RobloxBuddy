using RobloxBuddy.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using XmlDocument = Windows.Data.Xml.Dom.XmlDocument;

namespace RobloxBuddy.Services
{
    public class NotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly UserSettings _settings;
        private const string APP_ID = "RobloxBuddy";

        public NotificationService()
        {
            _httpClient = new HttpClient();
            _settings = ServiceLocator.Get<UserSettings>();

            // Ensure we have an App ID for notifications
            if (string.IsNullOrEmpty(APP_ID))
            {
                throw new InvalidOperationException("Application ID is required for notifications");
            }
        }

        public void ShowFriendOnlineNotification(string friendName, string activity, string avatarUrl = null)
        {
            if (!_settings.EnableFriendNotifications)
                return;

            try
            {
                // Create a simple toast XML template
                string xml = $@"<toast activationType='foreground' launch='action=viewFriend&amp;friendName={friendName}'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>Friend Online</text>
                            <text>{friendName} is now online</text>
                            <text>{activity}</text>
                        </binding>
                    </visual>
                </toast>";

                // Create the toast notification
                var toastXml = new XmlDocument();
                toastXml.LoadXml(xml);

                // Create and show the toast
                var toast = new ToastNotification(toastXml);

                // Show the toast
                ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing friend notification: {ex.Message}");
            }
        }

        public void ShowGameUpdateNotification(string gameName, string updateInfo, string thumbnailUrl = null)
        {
            if (!_settings.EnableGameNotifications)
                return;

            try
            {
                // Create a simple toast XML template
                string xml = $@"<toast activationType='foreground' launch='action=viewGame&amp;gameName={gameName}'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>Game Update</text>
                            <text>{gameName}</text>
                            <text>{updateInfo}</text>
                        </binding>
                    </visual>
                </toast>";

                // Create the toast notification
                var toastXml = new XmlDocument();
                toastXml.LoadXml(xml);

                // Create and show the toast
                var toast = new ToastNotification(toastXml);

                // Show the toast
                ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing game notification: {ex.Message}");
            }
        }

        public void ShowCustomNotification(string title, string message, string action = null, string imageUrl = null)
        {
            try
            {
                // Add action parameter if provided
                string launchParam = "";
                if (!string.IsNullOrEmpty(action))
                {
                    launchParam = $"action={action}";
                }

                // Create a simple toast XML template
                string xml = $@"<toast activationType='foreground' launch='{launchParam}'>
                    <visual>
                        <binding template='ToastGeneric'>
                            <text>{title}</text>
                            <text>{message}</text>
                        </binding>
                    </visual>
                </toast>";

                // Create the toast notification
                var toastXml = new XmlDocument();
                toastXml.LoadXml(xml);

                // Create and show the toast
                var toast = new ToastNotification(toastXml);

                // Show the toast
                ToastNotificationManager.CreateToastNotifier(APP_ID).Show(toast);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error showing custom notification: {ex.Message}");
            }
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading image: {ex.Message}");
                return null;
            }
        }

        // Static method to initialize the notification system
        public static void InitializeNotifications()
        {
            try
            {
                // Register the current process to handle toast activations
                // No initialization needed for direct ToastNotificationManager use
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing notifications: {ex.Message}");
            }
        }
    }
}