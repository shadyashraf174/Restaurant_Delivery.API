using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Services
{
    public interface IBasketService
    {
        Task<List<DishBasketDto>> GetBasketAsync(Guid userId);
        Task AddDishToBasketAsync(Guid userId, Guid dishId);
        Task RemoveOrDecreaseDishAsync(Guid userId, Guid dishId, bool increase);
    }
}
