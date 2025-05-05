namespace RobloxBuddy.Models
{
    public class Game
    {
        public long GameId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ThumbnailUrl { get; set; }
        public int ActivePlayers { get; set; }
        public int TotalVisits { get; set; }
        public double Rating { get; set; }
        public string CreatorName { get; set; }
        public bool IsFavorite { get; set; }
    }
}