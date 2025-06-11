using System.Text.Json.Serialization;

namespace VibeSomeMauiBro.Models;

public class Cat
{
    public string Id { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string? Breed { get; set; }
    public string? Description { get; set; }
    public bool IsLiked { get; set; }
    public DateTime? LikedAt { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is Cat cat && Id == cat.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode(StringComparison.Ordinal);
    }
}

public class CatApiResponse
{
    public string Id { get; set; } = "";
    public string Url { get; set; } = "";
    public Breed[]? Breeds { get; set; }
}

public class Breed
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

[JsonSerializable(typeof(CatApiResponse[]))]
[JsonSerializable(typeof(List<Cat>))]
[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
public partial class CatApiJsonContext : JsonSerializerContext
{
}
