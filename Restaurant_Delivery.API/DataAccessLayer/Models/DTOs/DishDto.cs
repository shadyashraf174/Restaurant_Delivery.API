using Restaurant_Delivery.API.DataAccessLayer.Models.Enums;

namespace Restaurant_Delivery.API.DataAccessLayer.Models.DTOs
{
    public class DishDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public string Image { get; set; }
        public bool Vegetarian { get; set; }
        public double? Rating { get; set; }
        public DishCategory Category { get; set; }

    }
}
