namespace FlightApp.ViewModels
{
    public class MealChoiceVM
    {
        public int MealChoiceId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int? CityId { get; set; }

        public bool IsAvailableForFlight(int departureCityId, int arrivalCityId)
        {
            if (!CityId.HasValue)
                return true;

            return CityId == departureCityId || CityId == arrivalCityId;
        }
    }
}