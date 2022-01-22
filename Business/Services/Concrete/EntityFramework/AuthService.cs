using Business.Services.Abstract;
using Core.Entities.Concrete;
using Core.UnitOfWork;
using Core.UnitOfWork.Repositories;
using Core.Utilities.ResultsHelper;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.Jwt;
using DataAccess.Dtos.Auth;
using Microsoft.EntityFrameworkCore;

namespace Business.Services.Concrete.EntityFramework
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User> _repo;
        private readonly ITokenHelper _tokenHelper;
        public AuthService(IUnitOfWork unitOfWork, ITokenHelper tokenHelper)
        {
            _unitOfWork = unitOfWork;
            _repo = _unitOfWork.GetEntityRepository<User>();
            _tokenHelper = tokenHelper;
        }
        public IDataResult<UserTokenDto> Login(UserLoginDto userDto)
        {
            var user = _unitOfWork.GetQuery<User>()
                .Include(inc => inc.UserProfiles)
                .Include(inc => inc.UserRoles)
                .ThenInclude(inc => inc.Role)
                .AsNoTracking()
                .FirstOrDefault(u => u.UserName == userDto.Username || u.Email == userDto.Username);

            if (user == null)
                return new ErrorDataResult<UserTokenDto>("User does not exist!");

            var validCredentials = HashingHelper.VerifyPasswordHash(userDto.Password, user.PasswordHash, user.PasswordSalt);

            if (!validCredentials)
                return new ErrorDataResult<UserTokenDto>("Invalid credentials!");

            var roles = user.UserRoles?.Select(ur => ur.Role).ToList();

            if (roles == null)
                return new ErrorDataResult<UserTokenDto>("User does not have authorization");

            var token = _tokenHelper.CreateToken(user, roles);
            var userTokenDto = new UserTokenDto
            {
                UserId = user.Id,
                Token = token.Token
            };

            _unitOfWork.Save();
            return new SuccessDataResult<UserTokenDto>(userTokenDto, "Login successful");
        }

        public async Task<IDataResult<UserTokenDto>> Register(UserRegisterDto userDto)
        {
            var result = _repo.Get(user => user.UserName == userDto.UserName);
            bool doesUserNameExist = _repo.Get(user => user.UserName == userDto.UserName) != null;
            bool doesEmailExist = _repo.Get(user => user.Email == userDto.Email) != null;

            if (doesUserNameExist)
                return new ErrorDataResult<UserTokenDto>("The given Username already exists!");

            if (doesEmailExist)
                return new ErrorDataResult<UserTokenDto>("The given Email already exists!");

            byte[] passwordHash;
            byte[] passwordSalt;

            HashingHelper.CreatePasswordHash(userDto.Password, out passwordHash, out passwordSalt);

            var guid = Guid.NewGuid();
            var roles = _unitOfWork.GetEntityRepository<Role>().GetMany(r => r.Name == "Standard").ToList();
            User newUser = new User
            {
                Id = guid,
                UserName = userDto.UserName,
                Email = userDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                UserProfiles = new List<UserProfile> {
                   new UserProfile
                   {
                       FirstName = userDto.FirstName,
                       LastName = userDto.LastName,
                       ProfileEmail = userDto.Email
                   }
               },
                UserRoles = roles.Select(r => new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = guid,
                    RoleId = r.Id
                }).ToList()
            };

            using (var uow = _unitOfWork)
            {
                uow.GetEntityRepository<User>().Create(newUser);
                await uow.SaveAsync();
            }

            if (roles.Count == 0)
                return new ErrorDataResult<UserTokenDto>("User does not have authorization");

            var token = _tokenHelper.CreateToken(newUser, roles);

            var userTokenDto = new UserTokenDto
            {
                UserId = newUser.Id,
                Token = token.Token
            };

            return new SuccessDataResult<UserTokenDto>(null, "Account successfully registered!");
        }
    }
}
