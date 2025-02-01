using Microsoft.EntityFrameworkCore;
using Restaurant_Delivery.API.DataAccessLayer.Data;
using Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Repositories
{
    public class DishRepository : IDishRepository
    {
        private readonly DeliveryDbContext _context;

        public DishRepository(DeliveryDbContext context)
        {
            _context = context;
        }

        public IQueryable<Dish> GetAll()
        {
            return _context.Dishes.AsQueryable();
        }

        public async Task<Dish> GetByIdAsync(Guid id)
        {
            return await _context.Dishes.FindAsync(id);
        }

        public async Task UpdateAsync(Dish dish)
        {
            _context.Dishes.Update(dish);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasUserOrderedDishAsync(Guid userId, Guid dishId)
        {
            return await _context.Orders
                .AnyAsync(o => o.Id == userId && o.Dishes.Any(d => d.Id == dishId));
        }
    }
}
