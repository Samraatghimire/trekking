namespace TravelGuide.Model
{
    public class AttractionModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public double? EntryFee { get; set; }
        public string OpenHours { get; set; }
        public double Rating { get; set; }
        public double DistanceInKm { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
