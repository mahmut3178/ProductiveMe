using Core.Entities.Abstract;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities.Concrete
{
    public class UserProfile : Entity
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfileEmail { get; set; }
        public User User { get; set; }
    }
}
