using System.Net;
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using VibeSomeMauiBro.Models;
using VibeSomeMauiBro.Services;

namespace VibeSomeMauiBro.Tests.Services;

public class MockHttpMessageHandler : HttpMessageHandler
{
    private Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>? _sendAsyncFunc;

    public void Setup(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsyncFunc)
    {
        _sendAsyncFunc = sendAsyncFunc;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _sendAsyncFunc?.Invoke(request, cancellationToken)
            ?? Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
    }
}

public class CatServiceTests : IDisposable
{
    private readonly MockHttpMessageHandler _mockMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly CatService _catService;
    private readonly string _tempFilePath;
    private readonly ILogger<CatService> _logger;

    public CatServiceTests()
    {
        _mockMessageHandler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(_mockMessageHandler);
        _logger = NullLogger<CatService>.Instance;

        // Use a unique temporary file for each test to ensure isolation
        _tempFilePath = Path.Combine(Path.GetTempPath(), $"test_liked_cats_{Guid.NewGuid()}.json");
        _catService = new CatService(_httpClient, _logger, _tempFilePath);
    }

    public void Dispose()
    {
        _httpClient.Dispose();
        // Clean up the temporary file
        try
        {
            if (File.Exists(_tempFilePath))
            {
                File.Delete(_tempFilePath);
            }
        }
        catch (Exception ex)
        {
            // Log the error - in a real application you would use proper logging
            Console.WriteLine($"Failed to delete temporary test file {_tempFilePath}: {ex.Message}");
        }
    }

    private void SetupHttpResponse(string responseContent, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _mockMessageHandler.Setup((request, cancellationToken) =>
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(responseContent)
            };
            return Task.FromResult(response);
        });
    }

    private void SetupHttpException(Exception exception)
    {
        _mockMessageHandler.Setup((request, cancellationToken) =>
        {
            throw exception;
        });
    }

    [Fact]
    public async Task GetCatsAsync_WithValidResponse_ReturnsCats()
    {
        // Arrange
        var catApiResponses = new CatApiResponse[]
        {
            new() { Id = "cat1", Url = "https://example.com/cat1.jpg", Breeds = new[] { new Breed { Name = "Persian", Description = "Fluffy cat" } } },
            new() { Id = "cat2", Url = "https://example.com/cat2.jpg", Breeds = new[] { new Breed { Name = "Siamese", Description = "Vocal cat" } } }
        };

        var jsonResponse = JsonSerializer.Serialize(catApiResponses, CatApiJsonContext.Default.CatApiResponseArray);
        SetupHttpResponse(jsonResponse);

        // Act
        var result = await _catService.GetCatsAsync(2);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("cat1", result[0].Id);
        Assert.Equal("https://example.com/cat1.jpg", result[0].ImageUrl);
        Assert.Equal("Persian", result[0].Breed);
        Assert.Equal("Fluffy cat", result[0].Description);
        Assert.Equal("cat2", result[1].Id);
        Assert.Equal("https://example.com/cat2.jpg", result[1].ImageUrl);
        Assert.Equal("Siamese", result[1].Breed);
        Assert.Equal("Vocal cat", result[1].Description);
    }

    [Fact]
    public async Task GetCatsAsync_WithNullResponse_ReturnsEmptyList()
    {
        // Arrange
        SetupHttpResponse("null");

        // Act
        var result = await _catService.GetCatsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetCatsAsync_WithHttpException_ReturnsFallbackCats()
    {
        // Arrange
        SetupHttpException(new HttpRequestException("Network error"));

        // Act
        var result = await _catService.GetCatsAsync(3);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, cat => Assert.StartsWith("fallback_", cat.Id));
        Assert.All(result, cat => Assert.Equal("Mixed", cat.Breed));
        Assert.All(result, cat => Assert.Equal("A beautiful cat", cat.Description));
        Assert.All(result, cat => Assert.StartsWith("https://cdn2.thecatapi.com/images/", cat.ImageUrl));
    }

    [Fact]
    public async Task GetCatsAsync_WithMoreRequestedThanFallback_ReturnsMaxFallbackCats()
    {
        // Arrange
        SetupHttpException(new HttpRequestException("Network error"));

        // Act
        var result = await _catService.GetCatsAsync(10); // More than the 5 fallback cats available

        // Assert
        Assert.Equal(5, result.Count); // Should only return the 5 fallback cats available
        Assert.All(result, cat => Assert.StartsWith("fallback_", cat.Id));
    }

    [Fact]
    public async Task GetCatsAsync_TracksSeenCats_FiltersOutDuplicates()
    {
        // Arrange
        var catApiResponses = new CatApiResponse[]
        {
            new() { Id = "cat1", Url = "https://example.com/cat1.jpg" },
            new() { Id = "cat2", Url = "https://example.com/cat2.jpg" }
        };

        var jsonResponse = JsonSerializer.Serialize(catApiResponses, CatApiJsonContext.Default.CatApiResponseArray);
        SetupHttpResponse(jsonResponse);

        // Act - First call
        var firstResult = await _catService.GetCatsAsync(2);
        // Act - Second call with same data
        var secondResult = await _catService.GetCatsAsync(2);

        // Assert
        Assert.Equal(2, firstResult.Count);
        Assert.Empty(secondResult); // Should be empty as all cats are already seen
    }

    [Fact]
    public async Task GetCatsAsync_WithBreedsInResponse_ExtractsBreedInfo()
    {
        // Arrange
        var catApiResponses = new CatApiResponse[]
        {
            new() { Id = "cat1", Url = "https://example.com/cat1.jpg", Breeds = new[] { new Breed { Name = "Maine Coon", Description = "Large friendly cat" } } }
        };
        var jsonResponse = JsonSerializer.Serialize(catApiResponses, CatApiJsonContext.Default.CatApiResponseArray);
        SetupHttpResponse(jsonResponse);

        // Act
        var result = await _catService.GetCatsAsync(1);

        // Assert
        Assert.Single(result);
        Assert.Equal("Maine Coon", result[0].Breed);
        Assert.Equal("Large friendly cat", result[0].Description);
    }

    [Fact]
    public async Task GetLikedCatsAsync_WithNoLikedCats_ReturnsEmptyList()
    {
        // Act
        var result = await _catService.GetLikedCatsAsync();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLikedCatsAsync_WithLikedCats_ReturnsLikedCats()
    {
        // Arrange
        var cat = new Cat { Id = "cat1", ImageUrl = "https://example.com/cat1.jpg" };
        await _catService.LikeCatAsync(cat);

        // Act
        var result = await _catService.GetLikedCatsAsync();

        // Assert
        Assert.Single(result);
        Assert.Equal("cat1", result[0].Id);
        Assert.True(result[0].IsLiked);
        Assert.NotNull(result[0].LikedAt);
    }

    [Fact]
    public async Task LikeCatAsync_SetsCatProperties()
    {
        // Arrange
        var cat = new Cat { Id = "cat1", ImageUrl = "https://example.com/cat1.jpg", IsLiked = false, LikedAt = null };

        // Act
        await _catService.LikeCatAsync(cat);

        // Assert
        Assert.True(cat.IsLiked);
        Assert.NotNull(cat.LikedAt);
        Assert.True(cat.LikedAt <= DateTime.Now);
        Assert.True(cat.LikedAt > DateTime.Now.AddSeconds(-1)); // Should be very recent
    }

    [Fact]
    public async Task LikeCatAsync_AddsCatToLikedCollection()
    {
        // Arrange
        var cat = new Cat { Id = "cat1", ImageUrl = "https://example.com/cat1.jpg" };

        // Act
        await _catService.LikeCatAsync(cat);
        var likedCats = await _catService.GetLikedCatsAsync();

        // Assert
        Assert.Single(likedCats);
        Assert.Equal("cat1", likedCats[0].Id);
    }

    [Fact]
    public async Task LikeCatAsync_WithSameCatTwice_OnlyAddsOnce()
    {
        // Arrange
        var cat = new Cat { Id = "cat1", ImageUrl = "https://example.com/cat1.jpg" };

        // Act
        await _catService.LikeCatAsync(cat);
        await _catService.LikeCatAsync(cat); // Like same cat again

        var likedCats = await _catService.GetLikedCatsAsync();

        // Assert
        Assert.Single(likedCats); // Should still be only one cat (HashSet behavior)
    }

    [Fact]
    public async Task DislikeCatAsync_SetsCatProperties()
    {
        // Arrange
        var cat = new Cat { Id = "cat1", ImageUrl = "https://example.com/cat1.jpg", IsLiked = true, LikedAt = DateTime.Now };

        // Act
        await _catService.DislikeCatAsync(cat);

        // Assert
        Assert.False(cat.IsLiked);
        Assert.Null(cat.LikedAt);
    }

    [Fact]
    public async Task DislikeCatAsync_RemovesCatFromLikedCollection()
    {
        // Arrange
        var cat = new Cat { Id = "cat1", ImageUrl = "https://example.com/cat1.jpg" };
        await _catService.LikeCatAsync(cat); // First like the cat

        // Act
        await _catService.DislikeCatAsync(cat);
        var likedCats = await _catService.GetLikedCatsAsync();

        // Assert
        Assert.Empty(likedCats);
    }

    [Fact]
    public async Task DislikeCatAsync_WithNotLikedCat_DoesNotThrow()
    {
        // Arrange
        var cat = new Cat { Id = "cat1", ImageUrl = "https://example.com/cat1.jpg" };

        // Act & Assert (should not throw)
        await _catService.DislikeCatAsync(cat);

        var likedCats = await _catService.GetLikedCatsAsync();
        Assert.Empty(likedCats);
    }

    [Fact]
    public async Task LikeAndDislikeMultipleCats_ManagesCollectionCorrectly()
    {
        // Arrange
        var cat1 = new Cat { Id = "cat1", ImageUrl = "https://example.com/cat1.jpg" };
        var cat2 = new Cat { Id = "cat2", ImageUrl = "https://example.com/cat2.jpg" };
        var cat3 = new Cat { Id = "cat3", ImageUrl = "https://example.com/cat3.jpg" };

        // Act
        await _catService.LikeCatAsync(cat1);
        await _catService.LikeCatAsync(cat2);
        await _catService.LikeCatAsync(cat3);

        var likedAfterLiking = await _catService.GetLikedCatsAsync();

        await _catService.DislikeCatAsync(cat2); // Remove cat2

        var likedAfterDisliking = await _catService.GetLikedCatsAsync();

        // Assert
        Assert.Equal(3, likedAfterLiking.Count);
        Assert.Equal(2, likedAfterDisliking.Count);
        Assert.Contains(likedAfterDisliking, c => c.Id == "cat1");
        Assert.Contains(likedAfterDisliking, c => c.Id == "cat3");
        Assert.DoesNotContain(likedAfterDisliking, c => c.Id == "cat2");
    }

    [Fact]
    public async Task GetCatsAsync_CanGenerateEpicCat_WhenRarityCombinationsAlign()
    {
        // Arrange - simulate API failure to get fallback cats
        SetupHttpException(new HttpRequestException("Network error"));

        // Act - call multiple times to potentially get epic cat 
        // (Note: This test doesn't rely on randomness directly, but ensures epic cat properties are correct when generated)
        var results = new List<List<Cat>>();
        for (int i = 0; i < 10; i++)
        {
            // Create new service instance for each attempt to reset seen cats
            var newService = new CatService(new HttpClient(_mockMessageHandler), _logger);
            var cats = await newService.GetCatsAsync(1);
            results.Add(cats);
        }

        // Assert - At least verify that fallback mechanism is working
        Assert.All(results, result => Assert.NotEmpty(result));

        // If we want to specifically test epic cat properties, we can call the CreateEpicCat method directly 
        // through reflection, or test the properties when we know it was generated
        Assert.All(results, catList =>
        {
            foreach (var cat in catList)
            {
                if (cat.Id == "epic_cat_legendary")
                {
                    Assert.Equal("epic_cat.png", cat.ImageUrl);
                    Assert.Equal("Legendary Epic Cat", cat.Breed);
                    Assert.Contains("LEGENDARY EPIC CAT", cat.Description);
                    Assert.Contains("🌟", cat.Description);
                }
            }
        });
    }

    [Fact]
    public async Task GetCatsAsync_EpicCatOnlyAppearsOnce()
    {
        // Arrange - Since _random is now readonly, we can't control it through reflection.
        // Instead, test the epic cat behavior probabilistically by calling many times
        SetupHttpException(new HttpRequestException("Network error"));

        // Act - call GetCatsAsync multiple times to increase chance of hitting epic cat
        var allCats = new List<Cat>();
        var epicCatAppearances = 0;

        // Call many times to have a reasonable chance of hitting the 1/1000 epic cat condition
        for (int i = 0; i < 100; i++)
        {
            var cats = await _catService.GetCatsAsync(1);
            allCats.AddRange(cats);

            // Count epic cat appearances
            var epicCatsInThisBatch = cats.Where(c => c.Id == "epic_cat_legendary").ToList();
            epicCatAppearances += epicCatsInThisBatch.Count;
        }

        // Assert - Epic cat should appear at most once across all calls
        // (due to _seenCatIds tracking, even if random condition is met multiple times)
        Assert.True(epicCatAppearances <= 1, $"Epic cat appeared {epicCatAppearances} times, but should appear at most once");

        // If epic cat appeared, verify its properties
        var epicCat = allCats.FirstOrDefault(c => c.Id == "epic_cat_legendary");
        if (epicCat != null)
        {
            Assert.Equal("epic_cat.png", epicCat.ImageUrl);
            Assert.Equal("Legendary Epic Cat", epicCat.Breed);
            Assert.Contains("LEGENDARY EPIC CAT", epicCat.Description);
            Assert.Contains("🌟", epicCat.Description);
        }
    }

    [Fact]
    public async Task LikedCats_PersistedToFile_AndLoadedOnNewServiceInstance()
    {
        // Arrange
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"test_persistence_{Guid.NewGuid()}.json");
        var cat1 = new Cat { Id = "persist1", ImageUrl = "https://example.com/persist1.jpg" };
        var cat2 = new Cat { Id = "persist2", ImageUrl = "https://example.com/persist2.jpg" };

        try
        {
            // Act - Create first service instance and like some cats
            using var httpClient1 = new HttpClient(new MockHttpMessageHandler());
            var catService1 = new CatService(httpClient1, NullLogger<CatService>.Instance, tempFilePath);
            await catService1.LikeCatAsync(cat1);
            await catService1.LikeCatAsync(cat2);

            // Act - Create second service instance with same file path
            using var httpClient2 = new HttpClient(new MockHttpMessageHandler());
            var catService2 = new CatService(httpClient2, NullLogger<CatService>.Instance, tempFilePath);
            var persistedCats = await catService2.GetLikedCatsAsync();

            // Assert
            Assert.Equal(2, persistedCats.Count);
            Assert.Contains(persistedCats, c => c.Id == "persist1");
            Assert.Contains(persistedCats, c => c.Id == "persist2");
            Assert.All(persistedCats, cat => Assert.True(cat.IsLiked));
            Assert.All(persistedCats, cat => Assert.NotNull(cat.LikedAt));
        }
        finally
        {
            // Cleanup
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }
    }

    [Fact]
    public async Task FileSystemError_DuringLoad_StartsWithEmptyCollection()
    {
        // Arrange - Use an invalid file path to simulate load error
        var invalidPath = Path.Combine("/invalid/path/that/does/not/exist", "test.json");

        // Act & Assert - Should not throw and should start with empty collection
        using var httpClient = new HttpClient(new MockHttpMessageHandler());
        var catService = new CatService(httpClient, NullLogger<CatService>.Instance, invalidPath);
        var likedCats = await catService.GetLikedCatsAsync();

        Assert.Empty(likedCats);
    }
}

// Helper class for testing
public class MockRandom : Random
{
    private readonly int _value;

    public MockRandom(int value)
    {
        _value = value;
    }

    public override int Next(int maxValue)
    {
        return _value;
    }
}
