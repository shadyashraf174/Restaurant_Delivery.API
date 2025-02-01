using Restaurant_Delivery.API.DataAccessLayer.Models.Enums;

namespace Restaurant_Delivery.API.DataAccessLayer.Models.MainDomain
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender Gender { get; set; }
        public string PhoneNumber { get; set; }
    }
}
