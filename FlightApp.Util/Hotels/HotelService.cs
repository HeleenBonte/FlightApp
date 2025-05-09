using FlightApp.Domains.EntityAPI;
using FlightApp.Util.Hotels.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightApp.Util.Hotels
{
    public class HotelService : IHotelService
    {
        private IConfiguration _configuration;
        private string apiBaseUrl;
        private string apiGetIdUrl;
        private string apiGetHotelUrl;
        private string? apiKey;
        public HotelService(IConfiguration configuration)
        {
            _configuration = configuration;
            apiBaseUrl = _configuration["BookingComAPI:BaseUrl"];
            apiGetIdUrl = _configuration["BookingComAPI:getHotelIdEndpoint"];
            apiGetHotelUrl = _configuration["BookingComAPI:getHotelDetailsEndpoint"];
            apiKey = _configuration["BookingComAPI:ApiKey"];
        }

        public async Task<Hotel?> GetHotelByIdAsync(int hotelApiId)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var uri = $"{apiBaseUrl}{apiGetHotelUrl}?hotel_id={hotelApiId}&arrival_date=2025-05-09&departure_date=2025-05-10&currency_code=EUR";
                    //$"{apiBaseUrl}{apiGetHotelUrl}?hotel_id={hotelApiId}&arrival_date={arrivalDate}&Departure_date={departureDate}&currency_code=EUR"
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(uri),
                        Headers =
                        {
                            { "x-rapidapi-key", apiKey },
                            { "x-rapidapi-host", "booking-com15.p.rapidapi.com" }
                        }
                    };
                    var response = await httpClient.SendAsync(request);
                    
                        try
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            var hotelApiResponse = System.Text.Json.JsonSerializer.Deserialize<HotelApiResponse>(responseData, new System.Text.Json.JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            if (hotelApiResponse.status)
                            {
                                return hotelApiResponse?.Data;
                            }
                            else
                            {
                            return null;
                            }
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(response.ReasonPhrase);
                        }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fout bij het verbinden: {ex.Message}");
                }
            }
        }
                

        public async Task<List<HotelId>?> GetHotelIdsAsync(string cityApiId)
        {
            
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var uri = $"{apiBaseUrl}{apiGetIdUrl}?dest_id={cityApiId}&search_type=city&arrival_date=2025-05-09&departure_date=2025-05-10";
                    // $"{apiBaseUrl}{apiGetIdUrl}?dest_id={cityApiId}&search_type=city&arrival_date={arrival_date}&departure_date={departure_date}";
                    var request = new HttpRequestMessage
                    {
                        Method = HttpMethod.Get,
                        RequestUri = new Uri(uri),
                        Headers =
                        {
                            { "x-rapidapi-key", apiKey },
                            { "x-rapidapi-host", "booking-com15.p.rapidapi.com" }
                        }
                    };
                        
                    var response = await httpClient.SendAsync(request);
                        try
                        {
                            var responseData = await response.Content.ReadAsStringAsync();
                            var hotelIdApiResponse = System.Text.Json.JsonSerializer.Deserialize<HotelIDApiResponse>(responseData, new System.Text.Json.JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                            if (hotelIdApiResponse.Status)
                            {
                                return hotelIdApiResponse?.Data.Hotels ?? new List<HotelId>();
                            }
                            else
                            {
                                throw new Exception("Problemen met de server, probeer opnieuw");
                            }
                        }
                        catch(Exception ex)
                        {
                            throw new Exception(response.ReasonPhrase);
                        }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Fout bij het verbinden: {ex.Message}");
                }
            }
        }
    }
}
