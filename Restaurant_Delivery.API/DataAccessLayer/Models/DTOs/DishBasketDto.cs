namespace Restaurant_Delivery.API.DataAccessLayer.Models.DTOs
{

    public class DishBasketDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
        public int Amount { get; set; }
        public string Image { get; set; }
    }
}
