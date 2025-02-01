using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.Enums;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Services
{
    public interface IDishService
    {
        Task<DishPagedListDto> GetDishesAsync(List<DishCategory> categories, bool? vegetarian, DishSorting? sorting, int page);
        Task<DishDto> GetDishAsync(Guid id);
        Task<bool> CanRateDishAsync(Guid dishId, Guid userId);
        Task RateDishAsync(Guid dishId, Guid userId, double ratingScore);
    }
}
