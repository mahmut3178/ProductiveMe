using Business.Services.Abstract;
using Core.UnitOfWork;
using Core.UnitOfWork.Repositories;
using Core.Utilities.ResultsHelper;
using Core.Utilities.Security;
using DataAccess.Dtos.Auth;
using DataAccess.Entities;

namespace Business.Services.Concrete.EntityFramework
{
    public class EfUserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<User> _repo;
        public EfUserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _repo = _unitOfWork.GetEntityRepository<User>();
        }
        public async Task<IDataResult<Guid>> Register(UserRegisterDto userDto)
        {
            bool doesUserNameExist = _repo.Get(user => user.UserName == userDto.UserName) != null;
            bool doesEmailExist = _repo.Get(user => user.Email == userDto.Email) != null;

            if (doesUserNameExist)
                return new ErrorDataResult<Guid>("The given Username already exists!");

            if (doesEmailExist)
                return new ErrorDataResult<Guid>("The given Email already exists!");

            byte[] passwordHash;
            byte[] passwordSalt;

            HashingHelper.CreatePasswordHash(userDto.Password, out passwordHash, out passwordSalt);

            User newUser = new User
            {
                Id = Guid.NewGuid(),
                UserName = userDto.UserName,
                Email = userDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                UserProfiles = new List<UserProfile>{
                    new UserProfile
                    {
                        FirstName = userDto.FirstName,
                        LastName = userDto.LastName,
                        ProfileEmail = userDto.Email
                    }
                }
            };

            using (var uow = _unitOfWork)
            {
                uow.GetEntityRepository<User>().Create(newUser);
                await uow.SaveAsync();
            }

            return new SuccessDataResult<Guid>(newUser.Id, "Account successfully registered!");
        }
    }
}
