using Newtonsoft.Json;
using RestSharp;
using RobloxBuddy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RobloxBuddy.Services
{
    public class RobloxApiService
    {
        private RestClient _client;
        private readonly UserSettings _userSettings;

        // Roblox API endpoints
        private const string ApiBaseUrl = "https://api.roblox.com";
        private const string UsersApiUrl = "https://users.roblox.com";
        private const string FriendsApiUrl = "https://friends.roblox.com";
        private const string PresenceApiUrl = "https://presence.roblox.com";
        private const string GamesApiUrl = "https://games.roblox.com";
        private const string ThumbnailsApiUrl = "https://thumbnails.roblox.com";

        public bool IsLoggedIn => !string.IsNullOrEmpty(_userSettings.RobloxToken);

        public RobloxApiService(UserSettings userSettings)
        {
            _userSettings = userSettings;
            _client = new RestClient();

            // Set default request headers if logged in
            if (IsLoggedIn)
            {
                _client.AddDefaultHeader("Cookie", $".ROBLOSECURITY={_userSettings.RobloxToken}");
            }
        }

        #region User Methods

        public async Task<UserInfo> GetCurrentUserInfoAsync()
        {
            if (!IsLoggedIn)
                throw new InvalidOperationException("User is not logged in");

            // Get authenticated user info
            var request = new RestRequest($"{UsersApiUrl}/v1/users/authenticated");
            var response = await _client.ExecuteGetAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"Failed to get user info: {response.ErrorMessage}");

            var userBasicInfo = JsonConvert.DeserializeObject<UserBasicInfo>(response.Content);

            // Now get more detailed info about the user
            return await GetUserInfoAsync(userBasicInfo.Id);
        }

        public async Task<UserInfo> GetUserInfoAsync(long userId)
        {
            // Get basic user info
            var request = new RestRequest($"{UsersApiUrl}/v1/users/{userId}");
            var response = await _client.ExecuteGetAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"Failed to get user info: {response.ErrorMessage}");

            var userDetail = JsonConvert.DeserializeObject<UserDetailInfo>(response.Content);

            // Get avatar info for thumbnail
            var thumbnailRequest = new RestRequest($"{ThumbnailsApiUrl}/v1/users/avatar-headshot");
            thumbnailRequest.AddQueryParameter("userIds", userId.ToString());
            thumbnailRequest.AddQueryParameter("size", "150x150");
            thumbnailRequest.AddQueryParameter("format", "Png");

            var thumbnailResponse = await _client.ExecuteGetAsync(thumbnailRequest);
            string avatarUrl = null;

            if (thumbnailResponse.IsSuccessful)
            {
                var thumbnailInfo = JsonConvert.DeserializeObject<ThumbnailResponse>(thumbnailResponse.Content);
                if (thumbnailInfo.Data.Any())
                {
                    avatarUrl = thumbnailInfo.Data[0].ImageUrl;
                }
            }

            return new UserInfo
            {
                UserId = userId,
                Username = userDetail.Name,
                DisplayName = userDetail.DisplayName,
                ProfileUrl = $"https://www.roblox.com/users/{userId}/profile",
                AvatarUrl = avatarUrl,
                // Additional data would require more API calls in a real implementation
                FriendsCount = await GetFriendsCountAsync(userId),
                FollowersCount = await GetFollowersCountAsync(userId)
            };
        }

        private async Task<int> GetFriendsCountAsync(long userId)
        {
            var request = new RestRequest($"{FriendsApiUrl}/v1/users/{userId}/friends/count");
            var response = await _client.ExecuteGetAsync(request);

            if (!response.IsSuccessful)
                return 0;

            var countData = JsonConvert.DeserializeObject<CountResponse>(response.Content);
            return countData.Count;
        }

        private async Task<int> GetFollowersCountAsync(long userId)
        {
            var request = new RestRequest($"{UsersApiUrl}/v1/users/{userId}/followers/count");
            var response = await _client.ExecuteGetAsync(request);

            if (!response.IsSuccessful)
                return 0;

            var countData = JsonConvert.DeserializeObject<CountResponse>(response.Content);
            return countData.Count;
        }

        #endregion

        #region Friends Methods

        public async Task<List<Friend>> GetFriendsAsync(long userId)
        {
            // Get friends list
            var request = new RestRequest($"{FriendsApiUrl}/v1/users/{userId}/friends");
            var response = await _client.ExecuteGetAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"Failed to get friends: {response.ErrorMessage}");

            var friendsData = JsonConvert.DeserializeObject<FriendsResponse>(response.Content);
            var friends = new List<Friend>();

            if (friendsData.Data.Any())
            {
                // Get user IDs for presence API
                var userIds = friendsData.Data.Select(f => f.Id).ToList();

                // Get presence info for friends
                var presenceRequest = new RestRequest($"{PresenceApiUrl}/v1/presence/users");
                presenceRequest.AddHeader("Content-Type", "application/json");
                presenceRequest.AddBody(new { userIds });

                var presenceResponse = await _client.ExecutePostAsync(presenceRequest);
                var presenceData = JsonConvert.DeserializeObject<PresenceResponse>(presenceResponse.Content);

                // Get avatar thumbnails for friends
                var thumbnailRequest = new RestRequest($"{ThumbnailsApiUrl}/v1/users/avatar-headshot");
                thumbnailRequest.AddQueryParameter("userIds", string.Join(",", userIds));
                thumbnailRequest.AddQueryParameter("size", "150x150");
                thumbnailRequest.AddQueryParameter("format", "Png");

                var thumbnailResponse = await _client.ExecuteGetAsync(thumbnailRequest);
                var thumbnailData = JsonConvert.DeserializeObject<ThumbnailResponse>(thumbnailResponse.Content);

                foreach (var friendData in friendsData.Data)
                {
                    var friend = new Friend
                    {
                        UserId = friendData.Id,
                        Username = friendData.Name,
                        DisplayName = friendData.DisplayName,
                        // Default to offline
                        IsOnline = false,
                        LastLocation = "Offline",
                        IsFavorite = _userSettings.FavoriteFriends.Contains(friendData.Id)
                    };

                    // Add presence info if available
                    var presence = presenceData?.UserPresences?.FirstOrDefault(p => p.UserId == friendData.Id);
                    if (presence != null)
                    {
                        friend.IsOnline = presence.UserPresenceType != 0; // 0 = Offline
                        friend.LastLocation = presence.LastLocation;
                        friend.GameId = presence.PlaceId;
                        friend.GameName = presence.LastLocation;
                    }

                    // Add thumbnail if available
                    var thumbnail = thumbnailData?.Data?.FirstOrDefault(t => t.TargetId == friendData.Id);
                    if (thumbnail != null && thumbnail.State == "Completed")
                    {
                        friend.AvatarUrl = thumbnail.ImageUrl;
                    }

                    friends.Add(friend);
                }
            }

            return friends;
        }

        public async Task<List<Friend>> GetOnlineFriendsAsync(long userId)
        {
            var friends = await GetFriendsAsync(userId);
            return friends.Where(f => f.IsOnline).ToList();
        }

        #endregion

        #region Games Methods

        public List<Game> GetPopularGames()
        {
            try
            {
                // Get popular games using the games API
                var request = new RestRequest($"{GamesApiUrl}/v1/games/list");
                request.AddQueryParameter("model.sortToken", "");
                request.AddQueryParameter("model.gameFilter", "1"); // Default (Popular)
                request.AddQueryParameter("model.timeFilter", "0"); // Now
                request.AddQueryParameter("model.universeIds", "");
                request.AddQueryParameter("model.maxRows", "10");

                var response = _client.ExecuteGetAsync(request).Result;

                if (response.IsSuccessful)
                {
                    var gamesData = JsonConvert.DeserializeObject<GamesResponse>(response.Content);
                    var games = new List<Game>();

                    if (gamesData?.Games != null && gamesData.Games.Any())
                    {
                        // Get universe IDs for thumbnail API
                        var universeIds = gamesData.Games.Select(g => g.Id).ToList();

                        // Get thumbnails for games
                        var thumbnailRequest = new RestRequest($"{ThumbnailsApiUrl}/v1/games/multiget/thumbnails");
                        thumbnailRequest.AddQueryParameter("universeIds", string.Join(",", universeIds));
                        thumbnailRequest.AddQueryParameter("size", "768x432");
                        thumbnailRequest.AddQueryParameter("format", "Png");
                        thumbnailRequest.AddQueryParameter("isCircular", "false");

                        var thumbnailResponse = _client.ExecuteGetAsync(thumbnailRequest).Result;
                        var thumbnailData = JsonConvert.DeserializeObject<GameThumbnailResponse>(thumbnailResponse.Content);

                        foreach (var gameData in gamesData.Games)
                        {
                            var game = new Game
                            {
                                GameId = gameData.RootPlaceId,
                                Name = gameData.Name,
                                Description = gameData.Description,
                                ActivePlayers = gameData.PlayerCount,
                                TotalVisits = gameData.PlayerCount * 10, // Since VisitCount is not available
                                CreatorName = gameData.Creator?.Name,
                                IsFavorite = _userSettings.FavoriteGames.Contains(gameData.RootPlaceId.ToString())
                            };

                            // Add thumbnail if available
                            var thumbnailInfo = thumbnailData?.Data?.FirstOrDefault(t => t.UniverseId == gameData.Id);
                            if (thumbnailInfo != null && thumbnailInfo.Thumbnails.Any() && thumbnailInfo.Thumbnails[0].State == "Completed")
                            {
                                game.ThumbnailUrl = thumbnailInfo.Thumbnails[0].ImageUrl;
                            }

                            games.Add(game);
                        }
                    }

                    return games;
                }

                // Return empty list if API call failed
                return new List<Game>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPopularGames: {ex.Message}");
                return new List<Game>();
            }
        }

        public List<Game> GetRecentlyPlayedGames()
        {
            try
            {
                if (!IsLoggedIn)
                    return new List<Game>();

                // Get current user ID
                var currentUser = GetCurrentUserInfoAsync().Result;
                if (currentUser == null)
                    return new List<Game>();

                // Get recently played games for the authenticated user
                var request = new RestRequest($"{GamesApiUrl}/v2/users/{currentUser.UserId}/recently-played-games");
                request.AddQueryParameter("limit", "10");

                var response = _client.ExecuteGetAsync(request).Result;

                if (response.IsSuccessful)
                {
                    var recentGamesData = JsonConvert.DeserializeObject<RecentGamesResponse>(response.Content);
                    var games = new List<Game>();

                    if (recentGamesData?.Data != null && recentGamesData.Data.Any())
                    {
                        foreach (var gameData in recentGamesData.Data)
                        {
                            var game = new Game
                            {
                                GameId = gameData.RootPlaceId,
                                Name = gameData.Name,
                                ThumbnailUrl = gameData.ThumbnailUrl,
                                IsFavorite = _userSettings.FavoriteGames.Contains(gameData.RootPlaceId.ToString())
                            };

                            // Get additional details for this game
                            try
                            {
                                var detailsRequest = new RestRequest($"{GamesApiUrl}/v1/games/multiget-place-details");
                                detailsRequest.AddQueryParameter("placeIds", gameData.RootPlaceId.ToString());

                                var detailsResponse = _client.ExecuteGetAsync(detailsRequest).Result;
                                if (detailsResponse.IsSuccessful)
                                {
                                    var placeDetails = JsonConvert.DeserializeObject<List<PlaceDetailInfo>>(detailsResponse.Content);
                                    if (placeDetails?.Any() == true)
                                    {
                                        var detail = placeDetails.First();
                                        game.Description = detail.Description;
                                        game.CreatorName = detail.Builder;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error getting game details: {ex.Message}");
                                // Continue with limited game info
                            }

                            games.Add(game);
                        }
                    }

                    return games;
                }

                // Return empty list if API call failed
                return new List<Game>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetRecentlyPlayedGames: {ex.Message}");
                return new List<Game>();
            }
        }

        #endregion

        #region Auth Methods

        public async Task<string> LoginAsync(string username, string password)
        {
            // NOTE: Roblox doesn't provide a direct username/password login API
            // In a real app, you would use browser-based authentication
            // This is a simplified mock implementation

            // Simulate successful login and get token
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                // Just for demo - this would actually happen via web auth flow
                var token = Guid.NewGuid().ToString();

                // Add the token to default headers for future requests
                _client.AddDefaultHeader("Cookie", $".ROBLOSECURITY={token}");

                return token;
            }

            throw new Exception("Invalid username or password");
        }

        public void Logout()
        {
            // Create a new client instead of trying to remove the header
            _client = new RestClient();
        }

        #endregion
    }

    #region API Response Models

    // Simple models for deserializing API responses

    public class UserBasicInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }

    public class UserDetailInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("created")]
        public DateTime Created { get; set; }

        [JsonProperty("isBanned")]
        public bool IsBanned { get; set; }
    }

    public class CountResponse
    {
        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class FriendsResponse
    {
        [JsonProperty("data")]
        public List<UserBasicInfo> Data { get; set; }
    }

    public class PresenceResponse
    {
        [JsonProperty("userPresences")]
        public List<UserPresenceInfo> UserPresences { get; set; }
    }

    public class UserPresenceInfo
    {
        [JsonProperty("userId")]
        public long UserId { get; set; }

        [JsonProperty("userPresenceType")]
        public int UserPresenceType { get; set; }

        [JsonProperty("lastLocation")]
        public string LastLocation { get; set; }

        [JsonProperty("placeId")]
        public long? PlaceId { get; set; }

        [JsonProperty("universeId")]
        public long? UniverseId { get; set; }

        [JsonProperty("gameId")]
        public string GameId { get; set; }
    }

    public class ThumbnailResponse
    {
        [JsonProperty("data")]
        public List<ThumbnailInfo> Data { get; set; }
    }

    public class ThumbnailInfo
    {
        [JsonProperty("targetId")]
        public long TargetId { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class GamesResponse
    {
        [JsonProperty("games")]
        public List<GameInfo> Games { get; set; }
    }

    public class GameInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("creator")]
        public CreatorInfo Creator { get; set; }

        [JsonProperty("rootPlaceId")]
        public long RootPlaceId { get; set; }

        [JsonProperty("playerCount")]
        public int PlayerCount { get; set; }

        [JsonProperty("visitCount")]
        public long VisitCount { get; set; }
    }

    public class CreatorInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public class GameThumbnailResponse
    {
        [JsonProperty("data")]
        public List<GameThumbnailData> Data { get; set; }
    }

    public class GameThumbnailData
    {
        [JsonProperty("universeId")]
        public long UniverseId { get; set; }

        [JsonProperty("thumbnails")]
        public List<ThumbnailDetail> Thumbnails { get; set; }
    }

    public class ThumbnailDetail
    {
        [JsonProperty("targetId")]
        public long TargetId { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("imageUrl")]
        public string ImageUrl { get; set; }
    }

    public class RecentGamesResponse
    {
        [JsonProperty("data")]
        public List<RecentGameInfo> Data { get; set; }
    }

    public class RecentGameInfo
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("rootPlaceId")]
        public long RootPlaceId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("thumbnailUrl")]
        public string ThumbnailUrl { get; set; }
    }

    public class PlaceDetailInfo
    {
        [JsonProperty("placeId")]
        public long PlaceId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("builder")]
        public string Builder { get; set; }

        [JsonProperty("builderId")]
        public long BuilderId { get; set; }
    }

    #endregion
}