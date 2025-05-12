using System.Collections.Generic;
using System.Linq;
using FlightApp.Domains.EntitiesDB;
using Microsoft.AspNetCore.Mvc;

namespace FlightApp.ViewModels
{
    public class ShoppingCartVM
    {
        public List<CartItemVM> CartItems { get; set; } = new List<CartItemVM>();
        public List<RouteCartItemVM> RouteItems { get; set; } = new List<RouteCartItemVM>();
        public List<FlightCartItemVM> FlightItems { get; set; } = new List<FlightCartItemVM>();

        public double ComputeTotalValue()
        {
            double legacyFlightTotal = CartItems.Sum(x => x.Passengers.Count * x.Price);
            double routeTotal = RouteItems.Sum(routeItem => routeItem.GetTotalPrice());
            double flightTotal = FlightItems.Sum(flightItem => flightItem.GetTotalPrice());

            return legacyFlightTotal + routeTotal + flightTotal;
        }

        public double CompleteItemsTotal()
        {
            // Only include routes that are marked as complete
            double routeTotal = RouteItems
                .Where(r => r.IsComplete)
                .Sum(routeItem => routeItem.GetTotalPrice());

            // Only include flights that are marked as complete
            double flightTotal = FlightItems
                .Where(f => f.IsComplete)
                .Sum(flightItem => flightItem.GetTotalPrice());

            // Include legacy flight items (assumed complete)
            double legacyFlightTotal = CartItems.Sum(x => x.Passengers.Count * x.Price);

            return legacyFlightTotal + routeTotal + flightTotal;
        }
    }

    public class CartItemVM
    {
        public int FlightId { get; set; }
        public double Price { get; set; }
        public List<PassengerVM> Passengers { get; set; } = new List<PassengerVM>();
        public System.DateTime DateCreated { get; set; }
    }
}