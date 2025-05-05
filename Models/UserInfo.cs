namespace RobloxBuddy.Models
{
    public class UserInfo
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public string ProfileUrl { get; set; }
        public string AvatarUrl { get; set; }
        public int RobuxBalance { get; set; }
        public int FriendsCount { get; set; }
        public int FollowersCount { get; set; }
        public int FollowingCount { get; set; }
    }
}