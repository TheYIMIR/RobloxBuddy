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

        public BackgroundMonitorService(RobloxApiService robloxApi, NotificationService notificationService, UserSettings settings)
        {
            _robloxApi = robloxApi;
            _notificationService = notificationService;
            _settings = settings;
            _lastKnownFriends = new List<Friend>();
        }

        public void StartMonitoring(long userId)
        {
            if (_isMonitoring)
                return;

            _isMonitoring = true;
            _cancellationToken = new CancellationTokenSource();

            // Initialize with current state
            Task.Run(async () =>
            {
                try
                {
                    _lastKnownFriends = await _robloxApi.GetFriendsAsync(userId);
                    // Start regular monitoring
                    _monitorTimer = new System.Threading.Timer(async (state) => await CheckForChangesAsync(userId),
                        null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting monitoring: {ex.Message}");
                    _isMonitoring = false;
                }
            });
        }

        public void StopMonitoring()
        {
            if (!_isMonitoring)
                return;

            _cancellationToken?.Cancel();
            _monitorTimer?.Dispose();
            _monitorTimer = null;
            _isMonitoring = false;
        }

        private async Task CheckForChangesAsync(long userId)
        {
            try
            {
                if (_cancellationToken.IsCancellationRequested)
                    return;

                // Get current friends state
                var currentFriends = await _robloxApi.GetFriendsAsync(userId);

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