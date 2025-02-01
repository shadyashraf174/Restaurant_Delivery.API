using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Restaurant_Delivery.API.DataAccessLayer.Data;
using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.Enums;
using Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Services
{
    public class OrderService : IOrderService
    {
        private readonly DeliveryDbContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<OrderService> _logger;

        public OrderService(DeliveryDbContext context, IMapper mapper, ILogger<OrderService> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<OrderDto> GetOrderAsync(Guid orderId, Guid userId)
        {
            var order = await _context.Orders
                .Include(o => o.Dishes)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Dishes.Any(d => d.UserId == userId));

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for user {UserId}.", orderId, userId);
                throw new KeyNotFoundException("Order not found.");
            }

            return _mapper.Map<OrderDto>(order);
        }

        public async Task<List<OrderInfoDto>> GetOrdersAsync(Guid userId)
        {
            var orders = await _context.Orders
                .Where(o => o.Dishes.Any(d => d.UserId == userId))
                .AsNoTracking()
                .ToListAsync();

            _logger.LogInformation("Successfully retrieved {Count} orders for user {UserId}.", orders.Count, userId);
            return _mapper.Map<List<OrderInfoDto>>(orders);
        }

        public async Task<Guid> CreateOrderAsync(Guid userId, OrderCreateDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.Address) || request.DeliveryTime == default)
            {
                _logger.LogWarning("Invalid request data for creating an order.");
                throw new ArgumentException("Invalid request data.");
            }

            // Get dishes from the user's basket
            var dishes = await _context.DishBaskets
                .Where(d => d.UserId == userId && d.OrderId == null)
                .ToListAsync();

            if (!dishes.Any())
            {
                _logger.LogWarning("Attempt to create an order with an empty basket for user {UserId}.", userId);
                throw new InvalidOperationException("Basket is empty.");
            }

            // Create the order
            var order = new Order
            {
                Id = Guid.NewGuid(),
                DeliveryTime = request.DeliveryTime,
                OrderTime = DateTime.UtcNow,
                Status = OrderStatus.InProcess,
                Price = dishes.Sum(d => d.TotalPrice),
                Address = request.Address,
                Dishes = dishes
            };

            // Add the order to the database
            _context.Orders.Add(order);

            // Update the DishBasket items with the OrderId
            foreach (var dish in dishes)
            {
                dish.OrderId = order.Id;
                _context.DishBaskets.Update(dish);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} created successfully for user {UserId}.", order.Id, userId);
            return order.Id;
        }

        public async Task ConfirmDeliveryAsync(Guid orderId, Guid userId)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId && o.Dishes.Any(d => d.UserId == userId));

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for user {UserId}.", orderId, userId);
                throw new KeyNotFoundException("Order not found.");
            }

            if (order.Status == OrderStatus.Delivered)
            {
                _logger.LogWarning("Order {OrderId} is already delivered.", orderId);
                throw new InvalidOperationException("Order is already delivered.");
            }

            order.Status = OrderStatus.Delivered;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Order {OrderId} marked as delivered for user {UserId}.", orderId, userId);
        }
    }
}
