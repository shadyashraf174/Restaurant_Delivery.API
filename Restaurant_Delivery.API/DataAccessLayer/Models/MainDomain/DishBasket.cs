namespace Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain
{
    public class DishBasket
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
        public double TotalPrice { get; set; }
        public int Amount { get; set; }
        public string Image { get; set; }

        // Foreign key to associate with an Order
        public Guid? OrderId { get; set; }
        public Order Order { get; set; }

        public Guid UserId { get; set; }
    }
}
