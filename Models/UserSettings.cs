using System.Collections.Generic;

namespace RobloxBuddy.Models
{
    public class UserSettings
    {
        public string RobloxToken { get; set; }
        public List<string> FavoriteGames { get; set; } = new List<string>();
        public List<long> FavoriteFriends { get; set; } = new List<long>();
        public bool EnableFriendNotifications { get; set; } = true;
        public bool EnableGameNotifications { get; set; } = true;
        public bool MinimizeToTray { get; set; } = true;
        public bool StartWithWindows { get; set; } = false;
        public int NotificationDuration { get; set; } = 5;
    }
}