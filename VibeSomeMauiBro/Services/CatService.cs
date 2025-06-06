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

    public CatService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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