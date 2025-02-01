using System.Text.Json.Serialization;

namespace Restaurant_Delivery.API.DataAccessLayer.Models.Enums
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum DishSorting
    {
        NameAsc,
        NameDesc,
        PriceAsc,
        PriceDesc,
        RatingAsc,
        RatingDesc
    }
}
