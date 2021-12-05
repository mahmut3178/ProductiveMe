using Core.Entities.Abstract;

namespace DataAccess.Entities
{
    public class User : Entity
    {
        public string UserName { get; set; }
        public byte[] PasswordSalt { get; set; }
        public byte[] PasswordHash { get; set; }
        public string Email { get; set; }
        public List<UserProfile> UserProfiles { get; set; }
    }
}
