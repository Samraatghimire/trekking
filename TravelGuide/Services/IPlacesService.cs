using TravelGuide.Model;

namespace TravelGuide.Services
{
    public interface IPlacesService
    {
        Task<List<AttractionModel>> GetNearbyAttractionsAsync(double latitude, double longitude,
        double radiusInKm, string category = null, double? minRating = null, int? maxDistance = null);
    
    }
}
