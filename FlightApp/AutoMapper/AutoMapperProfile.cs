using AutoMapper;
using FlightApp.Domains.EntitiesDB;
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
                    src => src.ArrivalCityNavigation.CityName));



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



            // CreateMap<Source, Destination>();
            // CreateMap<Destination, Source>();
            // CreateMap<Source, Destination>().ReverseMap();
            // CreateMap<Source, Destination>().ForMember(dest => dest.Property, opt => opt.MapFrom(src => src.Property));
        }
    }
    
}
