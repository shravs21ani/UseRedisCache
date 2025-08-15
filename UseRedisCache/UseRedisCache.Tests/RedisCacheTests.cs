using System;
using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using StackExchange.Redis;
using Xunit;

public class RedisCacheTests
{
    private readonly string _userId = "12345";
    private readonly string _cacheKey;

    public RedisCacheTests()
    {
        _cacheKey = $"user:preferences:{_userId}";
    }

    [Fact]
    public async Task GetUserPreferences_ShouldReturnFromDatabase_WhenCacheMiss()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(x => x.StringGetAsync(_cacheKey, It.IsAny<CommandFlags>()))
              .ReturnsAsync(RedisValue.Null);

        mockDb.Setup(x => x.StringSetAsync(
                _cacheKey,
                It.IsAny<RedisValue>(),
                It.IsAny<TimeSpan?>(),
                It.IsAny<When>(),
                It.IsAny<CommandFlags>()))
                .ReturnsAsync(true);


        var expected = FetchFromDatabase(_userId);

        // Act
        var result = await GetUserPreferences(mockDb.Object, _userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.UserId, result.UserId);
        Assert.Equal(expected.CommunicationLanguage, result.CommunicationLanguage);
        Assert.Equal(expected.CommunicationMode, result.CommunicationMode);
    }

    [Fact]
    public async Task GetUserPreferences_ShouldReturnFromCache_WhenCacheHit()
    {
        // Arrange
        var expected = new UserPreference
        {
            UserId = _userId,
            CommunicationLanguage = "English",
            CommunicationMode = "Email"
        };
        string cachedJson = JsonSerializer.Serialize(expected);

        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(x => x.StringGetAsync(_cacheKey, It.IsAny<CommandFlags>()))
              .ReturnsAsync(cachedJson);

        mockDb.Setup(x => x.KeyExpireAsync(_cacheKey, It.IsAny<TimeSpan>(), It.IsAny<CommandFlags>()))
              .ReturnsAsync(true);

        // Act
        var result = await GetUserPreferences(mockDb.Object, _userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expected.UserId, result.UserId);
        Assert.Equal(expected.CommunicationLanguage, result.CommunicationLanguage);
        Assert.Equal(expected.CommunicationMode, result.CommunicationMode);
    }

    [Fact]
    public async Task InvalidateCache_ShouldDeleteKey()
    {
        // Arrange
        var mockDb = new Mock<IDatabase>();
        mockDb.Setup(x => x.KeyDeleteAsync(_cacheKey, It.IsAny<CommandFlags>()))
              .ReturnsAsync(true)
              .Verifiable();

        // Act
        await InvalidateCache(mockDb.Object, _userId);

        // Assert
        mockDb.Verify();
    }

    // Extracted logic for testing
    private static async Task<UserPreference> GetUserPreferences(IDatabase db, string userId)
    {
        string cacheKey = $"user:preferences:{userId}";
        var slidingExpiration = TimeSpan.FromMinutes(10);

        var cachedValue = await db.StringGetAsync(cacheKey);
        if (cachedValue.HasValue)
        {
            await db.KeyExpireAsync(cacheKey, slidingExpiration);
            return JsonSerializer.Deserialize<UserPreference>(cachedValue!)!;
        }

        var preferencesFromDb = FetchFromDatabase(userId);
        await db.StringSetAsync(cacheKey, JsonSerializer.Serialize(preferencesFromDb), slidingExpiration);

        return preferencesFromDb;
    }

    private static async Task InvalidateCache(IDatabase db, string userId)
    {
        string cacheKey = $"user:preferences:{userId}";
        await db.KeyDeleteAsync(cacheKey);
    }

    private static UserPreference FetchFromDatabase(string userId)
    {
        return new UserPreference
        {
            UserId = userId,
            CommunicationLanguage = "English",
            CommunicationMode = "Email"
        };
    }
}
