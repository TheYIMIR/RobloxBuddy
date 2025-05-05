using RobloxBuddy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RobloxBuddy.Services
{
    public class BackgroundMonitorService
    {
        private readonly RobloxApiService _robloxApi;
        private readonly NotificationService _notificationService;
        private readonly UserSettings _settings;

        private System.Threading.Timer _monitorTimer;
        private List<Friend> _lastKnownFriends;
        private CancellationTokenSource _cancellationToken;
        private bool _isMonitoring;
        private readonly object _lockObject = new object();

        public BackgroundMonitorService(RobloxApiService robloxApi, NotificationService notificationService, UserSettings settings)
        {
            _robloxApi = robloxApi;
            _notificationService = notificationService;
            _settings = settings;
            _lastKnownFriends = new List<Friend>();
        }

        public void StartMonitoring(long userId)
        {
            lock (_lockObject)
            {
                if (_isMonitoring)
                    return;

                try
                {
                    _isMonitoring = true;
                    _cancellationToken = new CancellationTokenSource();

                    // Initialize with current state
                    Task.Run(async () =>
                    {
                        try
                        {
                            // Get initial friends list
                            _lastKnownFriends = await _robloxApi.GetFriendsAsync(userId);

                            // Start regular monitoring if we have a valid token and monitoring is enabled
                            if (_robloxApi.IsLoggedIn && _settings.EnableFriendNotifications)
                            {
                                _monitorTimer = new System.Threading.Timer(
                                    async (state) => await CheckForChangesAsync(userId),
                                    null,
                                    TimeSpan.FromSeconds(30), // Initial delay
                                    TimeSpan.FromMinutes(1)); // Regular interval
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error starting monitoring: {ex.Message}");
                            StopMonitoring(); // Clean up if initialization fails
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in StartMonitoring: {ex.Message}");
                    _isMonitoring = false;
                }
            }
        }

        public void StopMonitoring()
        {
            lock (_lockObject)
            {
                if (!_isMonitoring)
                    return;

                try
                {
                    // Cancel any pending operations
                    if (_cancellationToken != null && !_cancellationToken.IsCancellationRequested)
                    {
                        _cancellationToken.Cancel();
                    }

                    // Dispose the timer properly
                    if (_monitorTimer != null)
                    {
                        _monitorTimer.Change(Timeout.Infinite, Timeout.Infinite);
                        _monitorTimer.Dispose();
                        _monitorTimer = null;
                    }

                    // Clear resources
                    _cancellationToken?.Dispose();
                    _cancellationToken = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping monitoring: {ex.Message}");
                }
                finally
                {
                    _isMonitoring = false;
                }
            }
        }

        private async Task CheckForChangesAsync(long userId)
        {
            // Skip if cancellation was requested
            if (_cancellationToken?.IsCancellationRequested == true)
                return;

            try
            {
                // Check if user is still logged in
                if (!_robloxApi.IsLoggedIn)
                {
                    StopMonitoring();
                    return;
                }

                // Get current friends state
                var currentFriends = await _robloxApi.GetFriendsAsync(userId);

                // Skip further processing if we couldn't get friends list
                if (currentFriends == null || !currentFriends.Any())
                {
                    return;
                }

                // Only process changes if we have previous data to compare against
                if (_lastKnownFriends != null && _lastKnownFriends.Any())
                {
                    // Check for friends who just came online
                    foreach (var friend in currentFriends)
                    {
                        var lastKnownState = _lastKnownFriends.FirstOrDefault(f => f.UserId == friend.UserId);

                        // Friend just came online
                        if (friend.IsOnline && (lastKnownState == null || !lastKnownState.IsOnline))
                        {
                            _notificationService.ShowFriendOnlineNotification(
                                friend.DisplayName,
                                friend.LastLocation,
                                friend.AvatarUrl);
                        }

                        // Friend changed game
                        if (friend.IsOnline && lastKnownState != null && lastKnownState.IsOnline &&
                            friend.GameId != lastKnownState.GameId && friend.GameId.HasValue)
                        {
                            _notificationService.ShowCustomNotification(
                                $"{friend.DisplayName} changed game",
                                $"Now playing: {friend.GameName}",
                                "viewFriend",
                                friend.AvatarUrl);
                        }
                    }
                }

                // Save current state for next comparison
                _lastKnownFriends = currentFriends;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error monitoring friends: {ex.Message}");
            }
        }
    }
}