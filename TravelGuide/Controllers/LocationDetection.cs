using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using TravelGuide.Model;
using System.Net.Http;

namespace TravelGuide.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationDetectionController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public LocationDetectionController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> PostLocation([FromBody] LocationModel location)
        {
            var locationDetails = await ReverseGeocode(location.Latitude, location.Longitude);

            return Ok(new
            {
                message = "Location detected successfully",
                coordinates = location,
                address = locationDetails
            });
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
