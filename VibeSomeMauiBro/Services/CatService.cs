using System.Text.Json;
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
    private static Random _random = new();

    public CatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        // Configure API key if available
        var apiKey = AppContext.GetData("CAT_API_KEY") as string;
        if (!string.IsNullOrEmpty(apiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("x-api-key", apiKey);
        }
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

            // 1 in 1000 chance for epic cat (only if we haven't seen it before)
            if (_random.Next(1000) == 0 && !_seenCatIds.Contains("epic_cat_legendary"))
            {
                var epicCat = CreateEpicCat();
                cats.Insert(0, epicCat); // Add at the beginning for immediate visibility
                _seenCatIds.Add(epicCat.Id);
            }

            return cats;
        }
        catch (Exception)
        {
            // Return fallback data if API fails
            var fallbackCats = GetFallbackCats(count);
            
            // 1 in 1000 chance for epic cat (only if we haven't seen it before)
            if (_random.Next(1000) == 0 && !_seenCatIds.Contains("epic_cat_legendary"))
            {
                var epicCat = CreateEpicCat();
                fallbackCats.Insert(0, epicCat); // Add at the beginning for immediate visibility
                _seenCatIds.Add(epicCat.Id);
            }
            
            return fallbackCats;
        }
    }

    public Task<List<Cat>> GetLikedCatsAsync()
    {
        return Task.FromResult(_likedCats.ToList());
    }

    public Task LikeCatAsync(Cat cat)
    {
        cat.IsLiked = true;
        cat.LikedAt = DateTime.Now;
        _likedCats.Add(cat);
        return Task.CompletedTask;
    }

    public Task DislikeCatAsync(Cat cat)
    {
        cat.IsLiked = false;
        cat.LikedAt = null;
        _likedCats.Remove(cat);
        return Task.CompletedTask;
    }

    private static Cat CreateEpicCat()
    {
        return new Cat
        {
            Id = "epic_cat_legendary",
            ImageUrl = "epic_cat.png", // Embedded image
            Breed = "Legendary Epic Cat",
            Description = "🌟 LEGENDARY EPIC CAT! 🌟 Ultra rare and magnificent! You are incredibly lucky to see this majestic creature!"
        };
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