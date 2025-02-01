using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Restaurant_Delivery.API.DataAccessLayer.Data;
using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Services
{
    public class BasketService : IBasketService
    {
        private readonly DeliveryDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<BasketService> _logger;

        public BasketService(DeliveryDbContext context, IMapper mapper, ILogger<BasketService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<DishBasketDto>> GetBasketAsync(Guid userId)
        {
            var basket = await _context.DishBaskets
                .Where(d => d.UserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var basketDtos = _mapper.Map<List<DishBasketDto>>(basket);
            _logger.LogInformation("Successfully retrieved basket for user {UserId}.", userId);
            return basketDtos;
        }

        public async Task AddDishToBasketAsync(Guid userId, Guid dishId)
        {
            var dish = await _context.Dishes.FindAsync(dishId);
            if (dish == null)
            {
                _logger.LogWarning("Dish {DishId} not found.", dishId);
                throw new KeyNotFoundException("Dish not found.");
            }

            // Check if the dish is already in the user's basket
            var existingDishInBasket = await _context.DishBaskets
                .FirstOrDefaultAsync(d => d.Name == dish.Name && d.UserId == userId);

            if (existingDishInBasket != null)
            {
                // If the dish is already in the basket, increment the amount
                existingDishInBasket.Amount++;
                existingDishInBasket.TotalPrice = existingDishInBasket.Amount * dish.Price;
                _logger.LogInformation("Increased amount of dish {DishId} in basket for user {UserId}.", dishId, userId);
            }
            else
            {
                // If the dish is not in the basket, add it
                var dishBasket = new DishBasket
                {
                    Id = Guid.NewGuid(),
                    Name = dish.Name,
                    Price = dish.Price,
                    TotalPrice = dish.Price,
                    Amount = 1,
                    Image = dish.Image,
                    UserId = userId
                };
                _context.DishBaskets.Add(dishBasket);
                _logger.LogInformation("Added dish {DishId} to basket for user {UserId}.", dishId, userId);
            }

            await _context.SaveChangesAsync();
        }

        public async Task RemoveOrDecreaseDishAsync(Guid userId, Guid dishId, bool increase)
        {
            var dishInBasket = await _context.DishBaskets
                .FirstOrDefaultAsync(d => d.Id == dishId && d.UserId == userId);

            if (dishInBasket == null)
            {
                _logger.LogWarning("Dish {DishId} not found in basket for user {UserId}.", dishId, userId);
                throw new KeyNotFoundException("Dish not found in the basket.");
            }

            if (increase)
            {
                if (dishInBasket.Amount > 1)
                {
                    dishInBasket.Amount--;
                    dishInBasket.TotalPrice = dishInBasket.Amount * dishInBasket.Price;
                }
                else
                {
                    _context.DishBaskets.Remove(dishInBasket);
                }
            }
            else
            {
                _context.DishBaskets.Remove(dishInBasket);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Dish {DishId} updated in basket for user {UserId}.", dishId, userId);
        }
    }
}
