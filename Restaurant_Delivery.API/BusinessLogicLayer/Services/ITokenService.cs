using System.Security.Claims;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Services
{
    public interface ITokenService
    {
        Task<Guid> GetCurrentUserIdAsync(HttpRequest request, ClaimsPrincipal user);
        Task<bool> IsTokenRevokedAsync(string token);
    }
}
