using Restaurant_Delivery.API.DataAccessLayer.Models.Enums;

namespace Restaurant_Delivery.API.DataAccessLayer.Models.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public DateTime DeliveryTime { get; set; }
        public DateTime OrderTime { get; set; }
        public OrderStatus Status { get; set; }
        public double Price { get; set; }
        public List<DishBasketDto> Dishes { get; set; }
        public string Address { get; set; }
    }
}
