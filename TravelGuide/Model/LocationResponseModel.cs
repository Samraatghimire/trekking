namespace TravelGuide.Model
{
    public class LocationResponseModel
    {
        public LocationDetailsModel Address { get; set; }
        public List<AttractionModel> NearbyAttractions { get; set; }
    }
}
