using Microsoft.AspNetCore.Components;

namespace TravelGuide.Model.Location
{
    public class LocationModel
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double RadiusInKm { get; set; }
        public string Category { get; set; } = "all";  // this will make the default category to be all
        public double? MinRating { get; set; } = null;  //this will make the default rating to be null
        public int? MaxDistance { get; set; } = null;  //this will make the default distance to be null

    }
}
