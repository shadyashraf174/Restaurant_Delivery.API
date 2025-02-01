using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.IdentityModel.Tokens;
using Restaurant_Delivery.API.DataAccessLayer.Data;
using Restaurant_Delivery.API.DataAccessLayer.Models.DTOs;
using Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain;
using Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Restaurant_Delivery.API.BusinessLogicLayer.Services
{
    public class UserService : IUserService
    {
        private readonly DeliveryAuthDBContext _context;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly ILogger<UserService> _logger;

        public UserService(
            DeliveryAuthDBContext context,
            IConfiguration configuration,
            IMapper mapper,
            IDistributedCache cache,
            ILogger<UserService> logger)
        {
            _context = context;
            _passwordHasher = new PasswordHasher<User>();
            _configuration = configuration;
            _mapper = mapper;
            _cache = cache;
            _logger = logger;
        }

        public async Task<TokenResponse> RegisterAsync(UserRegisterModel request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Invalid request data.");
            }

            // Check if the email is already registered
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email is already registered.");
            }

            // Create a new user
            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = request.FullName,
                Email = request.Email,
                Password = _passwordHasher.HashPassword(null, request.Password), // Hash the password
                Address = request.Address,
                BirthDate = request.BirthDate,
                Gender = request.Gender,
                PhoneNumber = request.PhoneNumber
            };

            // Add the user to the database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Generate a JWT token
            var token = GenerateJwtToken(user);

            return new TokenResponse { Token = token };
        }

        public async Task<TokenResponse> LoginAsync(LoginCredentials request)
        {
            if (request == null || string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                throw new ArgumentException("Invalid request data.");
            }

            // Find the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            // Verify the password
            var passwordResult = _passwordHasher.VerifyHashedPassword(user, user.Password, request.Password);
            if (passwordResult == PasswordVerificationResult.Failed)
            {
                throw new InvalidOperationException("Invalid email or password.");
            }

            // Generate a JWT token
            var token = GenerateJwtToken(user);

            return new TokenResponse { Token = token };
        }

        public async Task LogoutAsync(string token)
        {
            if (string.IsNullOrEmpty(token) || IsTokenExpired(token))
            {
                throw new UnauthorizedAccessException("Invalid or expired token.");
            }

            // Store the token in the cache with an expiration time
            var tokenExpiration = DateTimeOffset.UtcNow.AddMinutes(30); // Match token expiration
            await _cache.SetStringAsync(token, "revoked", new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = tokenExpiration
            });

            _logger.LogInformation("User logged out successfully.");
        }

        public async Task<UserDto> GetProfileAsync(Guid userId)
        {
            // Find the user by ID
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // Map User to UserDto using AutoMapper
            return _mapper.Map<UserDto>(user);
        }

        public async Task UpdateProfileAsync(Guid userId, UserEditModel request)
        {
            if (request == null)
            {
                throw new ArgumentException("Invalid request data.");
            }

            // Find the user by ID
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            // Update user properties
            user.FullName = request.FullName ?? user.FullName;
            user.BirthDate = request.BirthDate ?? user.BirthDate;
            user.Gender = request.Gender ?? user.Gender;
            user.Address = request.Address ?? user.Address;
            user.PhoneNumber = request.PhoneNumber ?? user.PhoneNumber;

            // Save changes to the database
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool IsTokenExpired(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
    }
}
