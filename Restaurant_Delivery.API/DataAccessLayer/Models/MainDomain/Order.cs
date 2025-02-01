using Restaurant_Delivery.API.DataAccessLayer.Models.Enums;

namespace Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime DeliveryTime { get; set; }
        public DateTime OrderTime { get; set; }
        public OrderStatus Status { get; set; }
        public double Price { get; set; }
        public string Address { get; set; }

        // Navigation property for the dishes in the order
        public List<DishBasket> Dishes { get; set; } = new List<DishBasket>();

    }
}


