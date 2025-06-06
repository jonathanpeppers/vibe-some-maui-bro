# The Cat API Integration

The CatSwipe application integrates with [The Cat API](https://thecatapi.com/) to provide a continuous stream of real cat photos with breed information.

## API Overview

**Base URL**: `https://api.thecatapi.com/v1/`

**Documentation**: https://thecatapi.com/

The Cat API is a free service that provides access to thousands of cat images, including breed information, size data, and high-quality photos perfect for mobile applications.

## Authentication

The Cat API offers both free and paid tiers:

- **Free Tier**: 10 requests per minute, no API key required
- **Paid Tiers**: Higher rate limits with API key authentication

CatSwipe currently uses the free tier without authentication, which is sufficient for typical user interactions.

## Endpoints Used

### Search Images

**Endpoint**: `GET /images/search`

**URL**: `https://api.thecatapi.com/v1/images/search`

**Parameters**:
- `limit` (integer): Number of images to return (1-10, default: 1)
- `has_breeds` (boolean): Only return images with breed information (0 or 1)
- `size` (string): Image size preference ("small", "med", "full")

**Example Request**:
```
GET https://api.thecatapi.com/v1/images/search?limit=10&has_breeds=1
```

**Example Response**:
```json
[
  {
    "id": "0XYvRd7oD",
    "url": "https://cdn2.thecatapi.com/images/0XYvRd7oD.jpg",
    "width": 1204,
    "height": 1445,
    "breeds": [
      {
        "weight": {
          "imperial": "7  -  10",
          "metric": "3 - 5"
        },
        "id": "abys",
        "name": "Abyssinian",
        "cfa_url": "http://cfa.org/Breeds/BreedsAB/Abyssinian.aspx",
        "vetstreet_url": "http://www.vetstreet.com/cats/abyssinian",
        "vcahospitals_url": "https://vcahospitals.com/know-your-pet/cat-breeds/abyssinian",
        "temperament": "Active, Energetic, Independent, Intelligent, Gentle",
        "origin": "Egypt",
        "country_codes": "EG",
        "country_code": "EG",
        "description": "The Abyssinian is easy to care for, and a joy to have in your home. They're affectionate cats and love both people and other animals.",
        "life_span": "14 - 15",
        "indoor": 0,
        "lap": 1,
        "alt_names": "",
        "adaptability": 5,
        "affection_level": 5,
        "child_friendly": 3,
        "dog_friendly": 4,
        "energy_level": 5,
        "grooming": 1,
        "health_issues": 2,
        "intelligence": 5,
        "shedding_level": 2,
        "social_needs": 5,
        "stranger_friendly": 5,
        "vocalisation": 1,
        "experimental": 0,
        "hairless": 0,
        "natural": 1,
        "rare": 0,
        "rex": 0,
        "suppressed_tail": 0,
        "short_legs": 0,
        "wikipedia_url": "https://en.wikipedia.org/wiki/Abyssinian_cat",
        "hypoallergenic": 0,
        "reference_image_id": "0XYvRd7oD"
      }
    ]
  }
]
```

## Implementation in CatSwipe

### Service Integration

The `CatService` class handles all API interactions:

```csharp
public async Task<List<Cat>> GetCatsAsync(int count = 10)
{
    var response = await _httpClient.GetStringAsync($"https://api.thecatapi.com/v1/images/search?limit={count}&has_breeds=1");
    var catApiResponses = JsonSerializer.Deserialize<CatApiResponse[]>(response, new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true
    });
    // ... processing logic
}
```

### Data Models

The application uses these models to handle API responses:

- `CatApiResponse`: Maps to the API response structure
- `Cat`: Internal model for application use
- `Breed`: Contains breed information from the API

### Error Handling

The service includes comprehensive error handling:

1. **Network Failures**: Falls back to local images if API is unavailable
2. **Rate Limiting**: Gracefully handles the 10 requests/minute limit
3. **Invalid Responses**: Validates JSON deserialization results

### Fallback Strategy

When The Cat API is unavailable, CatSwipe falls back to a curated list of static cat images from the same CDN, ensuring users always have content to view.

## Rate Limiting

**Free Tier Limit**: 10 requests per minute

**CatSwipe Strategy**:
- Requests batches of 10 images at once to minimize API calls
- Implements local caching of fetched images
- Tracks seen images to avoid duplicates
- Falls back to local images when rate limited

## Future Enhancements

Potential improvements for production use:

1. **API Key Integration**: Add support for paid tier with higher limits
2. **Image Caching**: Implement persistent image caching
3. **Favorites API**: Use The Cat API's favorites endpoint for cloud sync
4. **Breed Filtering**: Allow users to filter by specific breeds
5. **Vote Integration**: Implement The Cat API's voting system