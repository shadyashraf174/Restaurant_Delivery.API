using Restaurant_Delivery.API.DataAccessLayer.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace Restaurant_Delivery.API.DataAccessLayer.Models.OthersDomain
{
    public class UserRegisterModel
    {
        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        public string Address { get; set; }

        public DateTime? BirthDate { get; set; }

        public Gender Gender { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
    }
}
