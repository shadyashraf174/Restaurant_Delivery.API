using System.ComponentModel.DataAnnotations;

namespace Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain
{
    public class LoginCredentials
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}