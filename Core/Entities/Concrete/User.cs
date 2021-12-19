using Core.Entities.Abstract;

namespace Core.Entities.Concrete
{
    public class User : Entity
    {
        public string UserName { get; set; }
        public byte[] PasswordSalt { get; set; }
        public byte[] PasswordHash { get; set; }
        public string Email { get; set; }
        public ICollection<UserProfile> UserProfiles { get; set; }
        public ICollection<UserRole> UserRoles { get; set; }
    }
}
