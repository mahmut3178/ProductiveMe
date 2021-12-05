using Core.Entities.Abstract;

namespace DataAccess.Entities
{
    public class UserProfile : Entity
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfileEmail { get; set; }
        public User User { get; set; }
    }
}
