using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Services
{
    public interface IOrderService
    {
        Task<OrderDto> GetOrderAsync(Guid orderId, Guid userId);
        Task<List<OrderInfoDto>> GetOrdersAsync(Guid userId);
        Task<Guid> CreateOrderAsync(Guid userId, OrderCreateDto request);
        Task ConfirmDeliveryAsync(Guid orderId, Guid userId);
    }
}
