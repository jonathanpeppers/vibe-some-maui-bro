namespace VibeSomeMauiBro.Models;

public class Cat
{
    public string Id { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? Breed { get; set; }
    public string? Description { get; set; }
    public bool IsLiked { get; set; }
    public DateTime? LikedAt { get; set; }
}

public class CatApiResponse
{
    public string Id { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public Breed[]? Breeds { get; set; }
}

public class Breed
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}