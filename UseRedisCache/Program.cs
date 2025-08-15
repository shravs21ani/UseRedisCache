using StackExchange.Redis;
using System.Text.Json;

public class UserPreference
{
    public string UserId { get; set; }
    public string CommunicationLanguage { get; set; }
    public string CommunicationMode { get; set; }
}

class Program
{
    private static readonly string redisConnectionString = "localhost:6379"; // Change if using Azure Redis
    private static readonly TimeSpan slidingExpiration = TimeSpan.FromMinutes(10);
    private static IDatabase _cache;

    static async Task Main()
    {
        var redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
        _cache = redis.GetDatabase();

        string userId = "12345";

        // First call: loads from DB and caches
        var prefs = await GetPatientPreferences(userId);
        Console.WriteLine($"Loaded preferences: {JsonSerializer.Serialize(prefs)}");

        // Second call within sliding window: loads from cache
        await Task.Delay(2000);
        prefs = await GetPatientPreferences(userId);
        Console.WriteLine($"Loaded preferences again: {JsonSerializer.Serialize(prefs)}");

        // Invalidate cache
        await InvalidateCache(userId);
        Console.WriteLine("Cache invalidated.");

        // Third call: reloads from DB after invalidation
        prefs = await GetPatientPreferences(userId);
        Console.WriteLine($"Loaded preferences after invalidation: {JsonSerializer.Serialize(prefs)}");
    }

    private static async Task<UserPreference> GetPatientPreferences(string userId)
    {
        string cacheKey = $"patient:preferences:{userId}";

        // Try to get from Redis
        var cachedValue = await _cache.StringGetAsync(cacheKey);
        if (cachedValue.HasValue)
        {
            // Refresh sliding expiration
            await _cache.KeyExpireAsync(cacheKey, slidingExpiration);
            return JsonSerializer.Deserialize<UserPreference>(cachedValue);
        }

        // Simulate DB fetch (replace with Cosmos DB logic)
        var preferencesFromDb = FetchFromDatabase(userId);

        // Cache in Redis with sliding expiration
        await _cache.StringSetAsync(cacheKey, JsonSerializer.Serialize(preferencesFromDb), slidingExpiration);

        return preferencesFromDb;
    }

    private static async Task InvalidateCache(string userId)
    {
        string cacheKey = $"user:preferences:{userId}";
        await _cache.KeyDeleteAsync(cacheKey);
    }

    private static UserPreference FetchFromDatabase(string userId)
    {
        Console.WriteLine($"Fetching from DB for user: {userId}");
        return new UserPreference
        {
            UserId = userId,
            CommunicationLanguage = "English",
            CommunicationMode = "Email"
        };
    }
}

