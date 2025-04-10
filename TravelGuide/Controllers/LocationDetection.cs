using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TravelGuide.Model;
using System.Net.Http;
using TravelGuide.Services;

namespace TravelGuide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationDetectionController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IPlacesService _placesService;

        public LocationDetectionController(HttpClient httpClient, IPlacesService placesService)
        {
            _httpClient = httpClient;
            _placesService = placesService;
        }

        [HttpPost]
        public async Task<IActionResult> PostLocation([FromBody] LocationModel location)
        {
            try
            {
                var locationDetails = await ReverseGeocode(location.Latitude, location.Longitude);

                var attractions = await _placesService.GetNearbyAttractionsAsync(
                    location.Latitude,
                    location.Longitude,
                    location.RadiusInKm,
                    location.Category,
                    location.MinRating,
                    location.MaxDistance);

                return Ok(new LocationResponseModel
                {
                    Address = locationDetails,
                    NearbyAttractions = attractions
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "An error occurred while processing your request.", error = ex.Message });
            }            
        }

        //method to get exact location details from the coordinates
        private async Task<LocationDetailsModel> ReverseGeocode(double latitude, double longitude)
        {
            string url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={latitude}&lon={longitude}&zoom=18&addressdetails=1";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("User-Agent", "TravelGuideApp/1.0 (contact: anjanlc5644@gmail.com)");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();            

            var content = await response.Content.ReadAsStringAsync();
            var geocodeResponse = JsonSerializer.Deserialize<NominatimResponseModel>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new LocationDetailsModel
            {
                FormattedAddress = geocodeResponse?.DisplayName,
                Country = geocodeResponse?.Address?.Country,
                State = geocodeResponse?.Address?.State,
                City = geocodeResponse?.Address?.City ?? geocodeResponse?.Address?.Town ?? geocodeResponse?.Address?.Village,
                PostalCode = geocodeResponse?.Address?.Postcode,
                Road = geocodeResponse?.Address?.Road,
                HouseNumber = geocodeResponse?.Address?.HouseNumber
            };
        }
    }
}
