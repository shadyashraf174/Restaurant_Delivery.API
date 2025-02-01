using Microsoft.Extensions.Caching.Distributed;
using System.Security.Claims;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Services
{
    public class TokenService : ITokenService
    {
        private readonly IDistributedCache _cache;

        public TokenService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<Guid> GetCurrentUserIdAsync(HttpRequest request, ClaimsPrincipal user)
        {
            var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            if (await IsTokenRevokedAsync(token))
            {
                return Guid.Empty;
            }

            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return userId;
            }
            return Guid.Empty;
        }

        public async Task<bool> IsTokenRevokedAsync(string token)
        {
            var revokedToken = await _cache.GetStringAsync(token);
            return revokedToken != null;
        }
    }
}
