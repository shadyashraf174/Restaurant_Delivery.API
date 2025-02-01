namespace Restaurant_Delivery.API.DataAccessLayer.Models.DTOs
{
    public class OrderCreateDto
    {
        public DateTime DeliveryTime { get; set; }
        public string Address { get; set; }
    }
}
