using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Restaurant_Delivery.API.BusinessLogicLayer.Repositories;
using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.Enums;
using Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain;
using Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Services
{
    public class DishService : IDishService
    {
        private readonly IDishRepository _dishRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<DishService> _logger;

        public DishService(IDishRepository dishRepository, IMapper mapper, ILogger<DishService> logger)
        {
            _dishRepository = dishRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DishPagedListDto> GetDishesAsync(List<DishCategory> categories, bool? vegetarian, DishSorting? sorting, int page)
        {
            try
            {
                var dishesQuery = _dishRepository.GetAll();

                dishesQuery = ApplyFilters(dishesQuery, categories, vegetarian);
                dishesQuery = ApplySorting(dishesQuery, sorting);

                var pagedDishes = await ApplyPagination(dishesQuery, page);

                return pagedDishes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving dishes.");
                throw;
            }
        }

        public async Task<DishDto> GetDishAsync(Guid id)
        {
            var dish = await _dishRepository.GetByIdAsync(id);
            if (dish == null)
            {
                _logger.LogWarning("Dish {DishId} not found.", id);
                throw new KeyNotFoundException("Dish not found.");
            }

            return _mapper.Map<DishDto>(dish);
        }

        public async Task<bool> CanRateDishAsync(Guid dishId, Guid userId)
        {
            var dish = await _dishRepository.GetByIdAsync(dishId);
            if (dish == null)
            {
                _logger.LogWarning("Dish {DishId} not found.", dishId);
                throw new KeyNotFoundException("Dish not found.");
            }

            bool hasOrderedDish = await _dishRepository.HasUserOrderedDishAsync(userId, dishId);
            return hasOrderedDish;
        }

        public async Task RateDishAsync(Guid dishId, Guid userId, double ratingScore)
        {
            if (ratingScore < 1 || ratingScore > 10)
            {
                _logger.LogWarning("Invalid rating score {RatingScore} provided for dish {DishId}.", ratingScore, dishId);
                throw new ArgumentException("Rating score must be between 1 and 10.");
            }

            var dish = await _dishRepository.GetByIdAsync(dishId);
            if (dish == null)
            {
                _logger.LogWarning("Dish {DishId} not found.", dishId);
                throw new KeyNotFoundException("Dish not found.");
            }

            dish.Rating = ratingScore;
            await _dishRepository.UpdateAsync(dish);

            _logger.LogInformation("Rating for dish {DishId} updated to {RatingScore}.", dishId, ratingScore);
        }

        // Helper Methods
        private static IQueryable<Dish> ApplyFilters(IQueryable<Dish> dishes, List<DishCategory> categories, bool? vegetarian)
        {
            if (categories != null && categories.Any())
            {
                dishes = dishes.Where(d => categories.Contains(d.Category));
            }

            if (vegetarian.HasValue)
            {
                dishes = dishes.Where(d => d.Vegetarian == vegetarian.Value);
            }

            return dishes;
        }

        private static IQueryable<Dish> ApplySorting(IQueryable<Dish> dishes, DishSorting? sorting)
        {
            if (sorting.HasValue)
            {
                dishes = sorting.Value switch
                {
                    DishSorting.NameAsc => dishes.OrderBy(d => d.Name),
                    DishSorting.NameDesc => dishes.OrderByDescending(d => d.Name),
                    DishSorting.PriceAsc => dishes.OrderBy(d => d.Price),
                    DishSorting.PriceDesc => dishes.OrderByDescending(d => d.Price),
                    DishSorting.RatingAsc => dishes.OrderBy(d => d.Rating ?? 0),
                    DishSorting.RatingDesc => dishes.OrderByDescending(d => d.Rating ?? 0),
                    _ => throw new ArgumentException("Invalid sorting parameter.")
                };
            }

            return dishes;
        }

        private async Task<DishPagedListDto> ApplyPagination(IQueryable<Dish> dishes, int page)
        {
            const int pageSize = 10;
            var totalCount = await dishes.CountAsync();
            var pagedDishes = await dishes.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new DishPagedListDto
            {
                Dishes = _mapper.Map<List<DishDto>>(pagedDishes),
                Pagination = new PageInfoModel
                {
                    Size = pageSize,
                    Count = totalCount,
                    Current = page
                }
            };
        }
    }
}
