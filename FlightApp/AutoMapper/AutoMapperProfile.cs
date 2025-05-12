using AutoMapper;
using FlightApp.Domains.EntitiesDB;
using FlightApp.Domains.EntityAPI;
using FlightApp.Models;
using FlightApp.ViewModels;

namespace FlightApp.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Flight, FlightVM>().ForMember(dest => dest.DepartureCity, 
                opts => opts.MapFrom(
                    src => src.DepartureCityNavigation.CityName))
                .ForMember(dest => dest.ArrivalCity,
                opts => opts.MapFrom(
                    src => src.ArrivalCityNavigation.CityName))
                .ForMember(dest => dest.Tickets,
                opts => opts.MapFrom(
                    src => src.Tickets));

            CreateMap<Booking, BookingVM>()
                .ForMember(dest => dest.UserName,
                    opts => opts.MapFrom(
                        src => src.User.UserName))
                .ForMember(dest => dest.RouteId,
                    opts => opts.MapFrom(
                        src => src.Route.RouteId))
                .ForMember(dest => dest.BookingTime,
                opts => opts.MapFrom(
                    src => DateOnly.FromDateTime(src.BookingTime)))
                .ForMember(dest => dest.Passengers,
                opts => opts.MapFrom(
                    src => src.Passengers));

            CreateMap<City, CityVM>();

            CreateMap<Domains.EntitiesDB.Route, RouteVM>().ForMember(dest => dest.DepartureCity,
                opts => opts.MapFrom(
                    src => src.DepartureCity.CityName))
                .ForMember(dest => dest.ArrivalCity,
                opts => opts.MapFrom(
                    src => src.ArrivalCity.CityName))
                .ForMember(dest => dest.Flights,
                opts => opts.MapFrom(
                    src => src.Flights));

            CreateMap<RouteFlightBridge, RouteFlightBridgeVM>()
                .ForMember(dest => dest.DepartureCity,
                    opts => opts.MapFrom(
                        src => src.RouteNav.DepartureCity.CityName))
                .ForMember(dest => dest.ArrivalCity,
                opts => opts.MapFrom(
                    src => src.RouteNav.ArrivalCity.CityName))
                .ForMember(dest => dest.DepartureTime,
                opts => opts.MapFrom(
                    src =>src.RouteNav.DepartureTime));

            CreateMap<BookingHistory, BookingHistoryVM>()
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.BookingId))
                .ForMember(dest => dest.PaymentStatus, opt => opt.MapFrom(src => src.Booking.PaymentStatus))
                .ForMember(dest => dest.BookingTime, opt => opt.MapFrom(src => src.Booking.BookingTime))
                .ForMember(dest => dest.RouteId, opt => opt.MapFrom(src => src.Booking.RouteId))
                .ForMember(dest => dest.FlightId, opt => opt.MapFrom(src => src.Booking.FlightId))
                .ForMember(dest => dest.DepartureCity, opt => opt.MapFrom(src =>
                    src.Booking.RouteId.HasValue && src.Booking.Route != null ?
                    src.Booking.Route.DepartureCity.CityName :
                    (src.Booking.FlightId.HasValue && src.Booking.Flight != null ?
                    src.Booking.Flight.DepartureCityNavigation.CityName :
                    "Unknown")))
                .ForMember(dest => dest.ArrivalCity, opt => opt.MapFrom(src =>
                    src.Booking.RouteId.HasValue && src.Booking.Route != null ?
                    src.Booking.Route.ArrivalCity.CityName :
                    (src.Booking.FlightId.HasValue && src.Booking.Flight != null ?
                    src.Booking.Flight.ArrivalCityNavigation.CityName :
                    "Unknown")))
                .ForMember(dest => dest.ArrivalCityData, opt => opt.MapFrom(src =>
                    src.Booking.RouteId.HasValue && src.Booking.Route != null ?
                    src.Booking.Route.ArrivalCity :
                    (src.Booking.FlightId.HasValue && src.Booking.Flight != null ?
                    src.Booking.Flight.ArrivalCityNavigation :
                    null)))
                .ForMember(dest => dest.DepartureDate,
                opts => opts.MapFrom(
                    src =>
                    src.Booking.RouteId.HasValue && src.Booking.Route != null ?
                    src.Booking.Route.DepartureTime :
                    (src.Booking.FlightId.HasValue && src.Booking.Flight != null ?
                    src.Booking.Flight.DepartureTime :
                    null)));

            CreateMap<MealChoice, MealChoiceVM>();

            CreateMap<BookingClass, BookingClassVM>();

            CreateMap<Ticket, TicketVM>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TicketId))
                .ForMember(dest => dest.PassengerName, opt => opt.MapFrom(src => $"{src.Passenger.FirstName} {src.Passenger.LastName}"))
                .ForMember(dest => dest.FlightDeparture, opt => opt.MapFrom(src => src.Flight.DepartureCityNavigation.CityName))
                .ForMember(dest => dest.FlightArrival, opt => opt.MapFrom(src => src.Flight.ArrivalCityNavigation.CityName))
                .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom(src => src.Flight.DepartureTime))
                .ForMember(dest => dest.ArrivalTime, opt => opt.MapFrom(src => src.Flight.ArrivalTime))
                .ForMember(dest => dest.BookingClassName, opt => opt.MapFrom(src => src.BookingClass.Description))
                .ForMember(dest => dest.MealChoiceType, opt => opt.MapFrom(src => src.MealChoice.Type));

            CreateMap<Hotel, HotelVM>()
                .ForMember(dest => dest.Price,
                opts => opts.MapFrom(
                    src => src.composite_price_breakdown.all_inclusive_amount.value))
                .ForMember(dest => dest.PriceString,
                opts => opts.MapFrom(
                    src => src.composite_price_breakdown.all_inclusive_amount.amount_rounded))
                .ForMember(dest => dest.PhotoUrls,
                opts => opts.MapFrom(
                    src => src.rawData.photoUrls))
                .ForMember(dest => dest.ReviewScore,
                opts => opts.MapFrom(
                    src => src.rawData.reviewScore));

            CreateMap<AspNetUser, ASPNetUserVM>();
        }
    }
}
