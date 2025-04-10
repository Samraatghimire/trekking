using System.Text.Json;
using TravelGuide.Model;
using TravelGuide.Services;
public class GeoapifyPlacesService : IPlacesService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeoapifyPlacesService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Geoapify:ApiKey"];
    }

    public async Task<List<AttractionModel>> GetNearbyAttractionsAsync(double latitude, double longitude,
        double radiusInKm, string category = null, double? minRating = null, int? maxDistance = null)
    {
        // Convert km to meters for Google Places API
        int radiusInMeters = (5000);


        // Map category to Google Places type
        string categories = MapCategoryToGeoapify(category);  // Get categories (comma-separated)

        // Build the request URL

        string url = $"https://api.geoapify.com/v2/places?" +
            $"categories={categories}&" +  // Multiple categories, comma-separated
            $"filter=circle:{longitude},{latitude},{radiusInMeters}&" +
            $"bias=proximity:{longitude},{latitude}&" +
            $"limit=20&" +  // Limit to 20 results (adjustable)
            $"apiKey={_apiKey}";


        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var geoResponse = JsonSerializer.Deserialize<GeoapifyResponse>(content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        var attractions = new List<AttractionModel>();

        maxDistance = maxDistance ?? 5000;  // Default to 5km if not specified

        foreach (var feature in geoResponse.Features)
        {
            double distanceInKm = feature.Properties.Distance / 1000.0;

            if (maxDistance.HasValue && distanceInKm > maxDistance.Value)
            {
                continue;
            }

            attractions.Add(new AttractionModel
            {
                Id = feature.Properties.PlaceId,
                Name = feature.Properties.Name,
                Category = MapGeoapifyCategoryToCustom(feature.Properties.Categories?.FirstOrDefault()),
                ImageUrl = null, // No photo API
                Description = feature.Properties.Description ?? "No description available",
                EntryFee = null,
                OpenHours = feature.Properties.OpeningHours ?? "Information not available",
                DistanceInKm = distanceInKm,
                Latitude = feature.Geometry.Coordinates[1],
                Longitude = feature.Geometry.Coordinates[0]
            });
        }
        return attractions;
    }

    private string MapCategoryToGeoapify(string category)
    {
        if (string.IsNullOrEmpty(category) || category == "all")
            return "tourism";

        List<string> categoriesList = new List<string>();

        if (category.Contains("historical")) categoriesList.Add("tourism.sights");
        if (category.Contains("nature")) categoriesList.Add("leisure.park");
        if (category.Contains("food")) categoriesList.Add("catering.restaurant");
        if (category.Contains("entertainment")) categoriesList.Add("entertainment");
        if (category.Contains("shopping")) categoriesList.Add("commercial.shopping_mall");

        // If none of the specific categories matched, default to tourism.
        if (categoriesList.Count == 0) categoriesList.Add("tourism");

        return string.Join(",", categoriesList);
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        // Haversine formula to calculate distance between two points
        const double R = 6371; // Earth radius in km

        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    private string MapGeoapifyCategoryToCustom(string geoapifyType)
    {
        if (string.IsNullOrEmpty(geoapifyType)) return "Other";

        return geoapifyType.ToLower() switch
        {
            string s when s.Contains("restaurant") => "Food",
            string s when s.Contains("park") => "Nature",
            string s when s.Contains("museum") => "Historical",
            string s when s.Contains("shopping") => "Shopping",
            _ => "Other"
        };
    }

    //private string FormatOpeningHours(GoogleOpeningHours openingHours)
    //    {
    //        if (openingHours?.WeekdayText == null)
    //            return "Information not available";

    //        return string.Join(", ", openingHours.WeekdayText);
    //    }
}

// Response models for Google Places API
public class GeoapifyResponse
{
    public List<GeoapifyFeature> Features { get; set; }
}

public class GeoapifyFeature
{
    public GeoapifyGeometry Geometry { get; set; }
    public GeoapifyProperties Properties { get; set; }
}

public class GeoapifyGeometry
{
    public string Type { get; set; }
    public List<double> Coordinates { get; set; } // [lon, lat]
}

public class GeoapifyProperties
{
    public string PlaceId { get; set; }
    public string Name { get; set; }
    public List<string> Categories { get; set; }
    public string Description { get; set; }
    public string OpeningHours { get; set; }
    public int Distance { get; set; } // in meters
}