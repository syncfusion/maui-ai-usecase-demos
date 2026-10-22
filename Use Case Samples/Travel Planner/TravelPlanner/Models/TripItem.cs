namespace TravelPlanner.Models;

public class TripInput
{
    public string Destination { get; set; } = "";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string TripType { get; set; } = "";
    public List<string> Interests { get; set; } = new();
}

public class PlaceCard
{
    public string ImageUrl { get; set; } = "";
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public double Rating { get; set; }

    // Optional coordinates; used to drop pins on map when available
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}

public class TripRecommendation
{
    public string DestinationSummary { get; set; } = "";
    public List<PlaceCard> TopPlaces { get; set; } = new();
    public List<PlaceCard> Hotels { get; set; } = new();
    public List<PlaceCard> FoodSpots { get; set; } = new();
}
