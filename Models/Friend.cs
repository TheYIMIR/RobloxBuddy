namespace RobloxBuddy.Models
{
    public class Friend
    {
        public long UserId { get; set; }
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public bool IsOnline { get; set; }
        public string LastLocation { get; set; }
        public string GameName { get; set; }
        public long? GameId { get; set; }
        public string AvatarUrl { get; set; }
        public bool IsFavorite { get; set; }
    }
}