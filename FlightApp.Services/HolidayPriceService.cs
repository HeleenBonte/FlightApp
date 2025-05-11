// FlightApp.Services/HolidayPriceService.cs
using FlightApp.Domains.EntitiesDB;
using FlightApp.Repositories.Interface;
using FlightApp.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FlightApp.Services
{
    public class HolidayPriceService : IHolidayPriceService
    {
        private readonly IDAO<Holiday> _holidayDAO;
        private readonly ILogger<HolidayPriceService> _logger;

        public HolidayPriceService(IDAO<Holiday> holidayDAO, ILogger<HolidayPriceService> logger)
        {
            _holidayDAO = holidayDAO;
            _logger = logger;
        }

        public async Task<double> GetHolidayPriceFactor(int cityId, DateTime departureTime)
        {
            try
            {
                // Convert to DateOnly for comparison with Holiday model
                DateOnly departureDate = DateOnly.FromDateTime(departureTime);

                // Get all holidays
                var holidays = await _holidayDAO.GetAllAsync();

                if (holidays == null)
                    return 1.0; // Default factor if no holidays found

                // Find any holiday that matches the city and date
                var matchingHoliday = holidays
                    .FirstOrDefault(h => h.CityId == cityId &&
                                        departureDate >= h.StartDate &&
                                        departureDate <= h.EndDate);

                // Return the price factor or default to 1.0 (no adjustment)
                return matchingHoliday?.PriceFactor ?? 1.0;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calculating holiday price factor: {ex.Message}");
                return 1.0; // Default factor in case of errors
            }
        }
    }
}