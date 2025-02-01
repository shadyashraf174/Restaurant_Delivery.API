using Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain;

namespace Restaurant_Delivery.API.DataAccessLayer.Models.DTOs
{
    public class DishPagedListDto
    {
        public List<DishDto> Dishes { get; set; }
        public PageInfoModel Pagination { get; set; }
    }
}
