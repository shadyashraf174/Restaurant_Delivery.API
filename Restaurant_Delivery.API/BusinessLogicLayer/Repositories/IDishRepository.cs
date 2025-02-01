using Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Repositories
{
    public interface IDishRepository
    {
        IQueryable<Dish> GetAll();
        Task<Dish> GetByIdAsync(Guid id);
        Task UpdateAsync(Dish dish);
        Task<bool> HasUserOrderedDishAsync(Guid userId, Guid dishId);
    }
}
