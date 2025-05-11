// FlightApp.Services/Interfaces/IHolidayPriceService.cs
namespace FlightApp.Services.Interfaces
{
    public interface IHolidayPriceService
    {
        Task<double> GetHolidayPriceFactor(int cityId, DateTime departureTime);
    }
}