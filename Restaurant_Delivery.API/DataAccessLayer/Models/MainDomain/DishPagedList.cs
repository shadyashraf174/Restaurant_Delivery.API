using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain;

namespace Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain
{
    public class DishPagedList
    {
        public List<DishDto> Dishes { get; set; }
        public PageInfoModel Pagination { get; set; }
    }
}
