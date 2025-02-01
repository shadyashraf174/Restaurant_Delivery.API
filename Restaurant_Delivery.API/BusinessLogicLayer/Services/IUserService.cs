using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Services
{
    public interface IUserService
    {
        Task<TokenResponse> RegisterAsync(UserRegisterModel request);
        Task<TokenResponse> LoginAsync(LoginCredentials request);
        Task LogoutAsync(string token);
        Task<UserDto> GetProfileAsync(Guid userId);
        Task UpdateProfileAsync(Guid userId, UserEditModel request);
    }
}
