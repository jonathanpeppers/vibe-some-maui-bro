using System.Text.Json;
using Microsoft.Extensions.Logging;
using VibeSomeMauiBro.Models;

namespace VibeSomeMauiBro.Services;

public interface ICatService
{
    Task<List<Cat>> GetCatsAsync(int count = 10);
    Task<List<Cat>> GetLikedCatsAsync();
    Task LikeCatAsync(Cat cat);
    Task DislikeCatAsync(Cat cat);
}

public class CatService : ICatService
{
    private readonly HttpClient _httpClient;
    private readonly HashSet<Cat> _likedCats = new();
    private readonly HashSet<string> _seenCatIds = new(StringComparer.Ordinal);
    private readonly string _likedCatsFilePath;
    private readonly ILogger<CatService> _logger;

    public CatService(HttpClient httpClient, ILogger<CatService> logger, string? customFilePath = null)
    {
        _httpClient = httpClient;
        _logger = logger;
        
        // Configure API key if available
        var apiKey = AppContext.GetData("CAT_API_KEY") as string;
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }

        // Set up file path for liked cats storage
        if (customFilePath != null)
        {
            _likedCatsFilePath = customFilePath;
        }
        else
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _likedCatsFilePath = Path.Combine(appDataPath, "VibeSomeMauiBro", "liked_cats.json");
        }
        
        // Ensure directory exists
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_likedCatsFilePath)!);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create directory for liked cats storage: {DirectoryPath}", Path.GetDirectoryName(_likedCatsFilePath));
        }
        
        // Load existing liked cats
        LoadLikedCatsAsync().Wait();
    }

    public async Task<List<Cat>> GetCatsAsync(int count = 10)
    {
        try
        {
            // Using The Cat API - free tier allows 10 requests per minute
            var response = await _httpClient.GetStringAsync($"https://api.thecatapi.com/v1/images/search?limit={count}&has_breeds=1");
            var catApiResponses = JsonSerializer.Deserialize(response, CatApiJsonContext.Default.CatApiResponseArray);

            if (catApiResponses == null) return new List<Cat>();

            var cats = catApiResponses
                .Where(c => !_seenCatIds.Contains(c.Id))
                .Select(c => new Cat
                {
                    Id = c.Id,
                    ImageUrl = c.Url,
                    Breed = c.Breeds?.FirstOrDefault()?.Name,
                    Description = c.Breeds?.FirstOrDefault()?.Description
                })
                .ToList();

            // Mark as seen
            foreach (var cat in cats)
            {
                _seenCatIds.Add(cat.Id);
            }

            return cats;
        }
        catch (Exception)
        {
            // Return fallback data if API fails
            return GetFallbackCats(count);
        }
    }

    public Task<List<Cat>> GetLikedCatsAsync()
    {
        return Task.FromResult(_likedCats.ToList());
    }

    public async Task LikeCatAsync(Cat cat)
    {
        cat.IsLiked = true;
        cat.LikedAt = DateTime.Now;
        
        // HashSet will handle duplicates automatically based on Cat.Id
        _likedCats.Add(cat);
        
        await SaveLikedCatsAsync();
    }

    public async Task DislikeCatAsync(Cat cat)
    {
        cat.IsLiked = false;
        cat.LikedAt = null;
        
        _likedCats.RemoveWhere(c => c.Id == cat.Id);
        
        await SaveLikedCatsAsync();
    }

    private async Task LoadLikedCatsAsync()
    {
        try
        {
            if (File.Exists(_likedCatsFilePath))
            {
                using var fileStream = File.OpenRead(_likedCatsFilePath);
                var likedCats = await JsonSerializer.DeserializeAsync(fileStream, CatApiJsonContext.Default.ListCat);
                if (likedCats != null)
                {
                    _likedCats.Clear();
                    foreach (var cat in likedCats)
                    {
                        _likedCats.Add(cat);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load liked cats from file: {FilePath}", _likedCatsFilePath);
            _likedCats.Clear();
        }
    }

    private async Task SaveLikedCatsAsync()
    {
        try
        {
            using var fileStream = File.Create(_likedCatsFilePath);
            await JsonSerializer.SerializeAsync(fileStream, _likedCats.ToList(), CatApiJsonContext.Default.ListCat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save liked cats to file: {FilePath}", _likedCatsFilePath);
        }
    }

    private static List<Cat> GetFallbackCats(int count)
    {
        // Fallback cat images in case API is unavailable
        var fallbackUrls = new[]
        {
            "https://cdn2.thecatapi.com/images/0XYvRd7oD.jpg",
            "https://cdn2.thecatapi.com/images/1p0.jpg",
            "https://cdn2.thecatapi.com/images/3eg.jpg",
            "https://cdn2.thecatapi.com/images/4fk.jpg",
            "https://cdn2.thecatapi.com/images/5i3.jpg",
        };

        return Enumerable.Range(0, Math.Min(count, fallbackUrls.Length))
            .Select(i => new Cat
            {
                Id = $"fallback_{i}",
                ImageUrl = fallbackUrls[i],
                Breed = "Mixed",
                Description = "A beautiful cat"
            })
            .ToList();
    }
}