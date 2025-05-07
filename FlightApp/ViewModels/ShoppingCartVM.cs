using FlightApp.Domains.EntitiesDB;

namespace FlightApp.ViewModels
{
    public class ShoppingCartVM
    {
        public List<CartItemVM> CartItems { get; set; }
        public double ComputeTotalValue() => CartItems.Sum(x => x.Passengers.Count * x.Price);
    }
    public class CartItemVM
    {
        public int FlightId { get; set; }
        public double Price { get; set; }
        public List<PassengerVM> Passengers { get; set; } = new List<PassengerVM>();
        public System.DateTime DateCreated { get; set; }
    }
}
