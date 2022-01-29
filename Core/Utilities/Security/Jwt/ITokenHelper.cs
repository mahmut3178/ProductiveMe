using Core.Entities.Concrete;
using Core.UnitOfWork.Repositories;

namespace Core.Utilities.Security.Jwt
{
    public interface ITokenHelper
    {
        AccessToken CreateToken(User user, IRepository<RefreshToken> refreshTokenRepository);
    }
}
