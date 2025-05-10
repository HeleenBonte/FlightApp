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
            //Flight
            CreateMap<Flight, FlightVM>().ForMember(dest => dest.DepartureCity, 
                opts => opts.MapFrom(
                    src => src.DepartureCityNavigation.CityName))
                .ForMember(dest => dest.ArrivalCity,
                opts => opts.MapFrom(
                    src => src.ArrivalCityNavigation.CityName))
                .ForMember(dest => dest.Tickets,
                opts => opts.MapFrom(
                    src => src.Tickets));



            //Booking
            CreateMap<Booking, BookingVM>()
                .ForMember(dest => dest.UserName,
                    opts => opts.MapFrom(
                        src => src.User.UserName))
                .ForMember(dest => dest.RouteId,
                    opts => opts.MapFrom(
                        src => src.Route.RouteId));


            //City
            CreateMap<City, CityVM>();

            //Route
            CreateMap<Domains.EntitiesDB.Route, RouteVM>().ForMember(dest => dest.DepartureCity,
                opts => opts.MapFrom(
                    src => src.DepartureCity.CityName))
                .ForMember(dest => dest.ArrivalCity,
                opts => opts.MapFrom(
                    src => src.ArrivalCity.CityName))
                .ForMember(dest => dest.Flights,
                opts => opts.MapFrom(
                    src => src.Flights));


            //RouteFlightBridge
            CreateMap<RouteFlightBridge, RouteFlightBridgeVM>()
                .ForMember(dest => dest.DepartureCity,
                    opts => opts.MapFrom(
                        src => src.RouteNav.DepartureCity.CityName))
                .ForMember(dest => dest.ArrivalCity,
                opts => opts.MapFrom(
                    src => src.RouteNav.ArrivalCity.CityName))
                .ForMember(dest => dest.DepartureTime,
                opts => opts.MapFrom(
                    src =>src.RouteNav.DepartureTime))
                ;

            //BookingHistory
            CreateMap<BookingHistory, BookingHistoryVM>()
                .ForMember(dest => dest.DepartureCity,
                    opts => opts.MapFrom(
                        src => src.Booking.Route.DepartureCity.CityName))
                .ForMember(dest => dest.ArrivalCity,
                opts => opts.MapFrom(
                    src => src.Booking.Route.ArrivalCity.CityName))
                .ForMember(dest => dest.Passengers,
                opts => opts.MapFrom(
                    src => src.Booking.Passengers));

            //MealChoice
            CreateMap<MealChoice, MealChoiceVM>();

            //BookingClass
            CreateMap<BookingClass, BookingClassVM>();

            //Tickets
            CreateMap<Ticket, TicketVM>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TicketId))
                .ForMember(dest => dest.PassengerName, opt => opt.MapFrom(src => $"{src.Passenger.FirstName} {src.Passenger.LastName}"))
                .ForMember(dest => dest.FlightDeparture, opt => opt.MapFrom(src => src.Flight.DepartureCityNavigation.CityName))
                .ForMember(dest => dest.FlightArrival, opt => opt.MapFrom(src => src.Flight.ArrivalCityNavigation.CityName))
                .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom(src => src.Flight.DepartureTime))
                .ForMember(dest => dest.ArrivalTime, opt => opt.MapFrom(src => src.Flight.ArrivalTime))
                .ForMember(dest => dest.BookingClassName, opt => opt.MapFrom(src => src.BookingClass.Description))
                .ForMember(dest => dest.MealChoiceType, opt => opt.MapFrom(src => src.MealChoice.Type));

            //Hotel
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

            // CreateMap<Source, Destination>();
            // CreateMap<Destination, Source>();
            // CreateMap<Source, Destination>().ReverseMap();
            // CreateMap<Source, Destination>().ForMember(dest => dest.Property, opt => opt.MapFrom(src => src.Property));
        }
    }
    
}
