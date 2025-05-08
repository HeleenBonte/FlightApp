namespace FlightApp.ViewModels
{
    public class MealChoiceVM
    {
        public int MealChoiceId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int? CityId { get; set; }

        // Helper method to determine if this meal is available for a specific flight
        public bool IsAvailableForFlight(int departureCityId, int arrivalCityId)
        {
            // If no city restriction (null CityId), the meal is available on all flights
            if (!CityId.HasValue)
                return true;

            // Otherwise, the meal is only available if it matches either the departure or arrival city
            return CityId == departureCityId || CityId == arrivalCityId;
        }
    }
}