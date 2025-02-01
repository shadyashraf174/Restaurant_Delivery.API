using System.Text.Json.Serialization;

namespace Restaurant_Delivery.API.DataAccessLayer.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DishCategory
    {
        Wok,
        Pizza,
        Soup,
        Dessert,
        Drink
    }
}
