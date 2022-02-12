using Business.Services.Abstract;
using Core.Entities.Concrete;
using Core.UnitOfWork;
using Core.UnitOfWork.Repositories;
using Core.Utilities.ResultsHelper;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.Jwt;
using DataAccess.Dtos.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Business.Services.Concrete.EntityFramework
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenHelper _tokenHelper;
        private readonly IHashingHelper _hashingHelper;
        private readonly IRepository<User> _repository;
        private readonly IRepository<RefreshToken> _refreshTokenRepository;
        private readonly TokenValidationParameters _tokenValidationParameters;
        public AuthService(IUnitOfWork unitOfWork, ITokenHelper tokenHelper, IHashingHelper hashingHelper, TokenValidationParameters tokenValidationParameters)
        {
            _unitOfWork = unitOfWork;
            _tokenHelper = tokenHelper;
            _hashingHelper = hashingHelper;
            _repository = _unitOfWork.GetEntityRepository<User>();
            _refreshTokenRepository = _unitOfWork.GetEntityRepository<RefreshToken>();
            _tokenValidationParameters = tokenValidationParameters;

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

            var validCredentials = _hashingHelper.VerifyPasswordHash(userDto.Password, user.PasswordHash, user.PasswordSalt);

            if (!validCredentials)
                return new ErrorDataResult<UserTokenDto>("Invalid credentials!");

            var token = _tokenHelper.CreateToken(user, _refreshTokenRepository);
            var userTokenDto = new UserTokenDto
            {
                UserId = user.Id,
                Token = token.Token,
                RefreshToken = token.RefreshToken
            };

            _unitOfWork.Save();
            return new SuccessDataResult<UserTokenDto>(userTokenDto, "Login successful!");
        }

        public async Task<IDataResult<UserTokenDto>> Register(UserRegisterDto userDto)
        {
            var result = _repository.Get(user => user.UserName == userDto.UserName);
            bool doesUserNameExist = _repository.Get(user => user.UserName == userDto.UserName) != null;
            bool doesEmailExist = _repository.Get(user => user.Email == userDto.Email) != null;

            if (doesUserNameExist)
                return new ErrorDataResult<UserTokenDto>("The given Username already exists!");

            if (doesEmailExist)
                return new ErrorDataResult<UserTokenDto>("The given Email already exists!");

            byte[] passwordHash;
            byte[] passwordSalt;

            _hashingHelper.CreatePasswordHash(userDto.Password, out passwordHash, out passwordSalt);

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

            var token = _tokenHelper.CreateToken(newUser, _refreshTokenRepository);

            var userTokenDto = new UserTokenDto
            {
                UserId = newUser.Id,
                Token = token.Token,
                RefreshToken = token.RefreshToken
            };

            return new SuccessDataResult<UserTokenDto>(userTokenDto, "Account successfully registered!");
        }

        public async Task<IDataResult<UserTokenDto>> RefreshTokenAsync(string token, string refreshToken)
        {
            var validatedToken = GetPrincipalFromToken(token);
            if (validatedToken == null)
            {
                return new ErrorDataResult<UserTokenDto>(message: "Invalid Token");
            }


            var expiryDateUnix = long.Parse(validatedToken.Claims.Single(c => c.Type == JwtRegisteredClaimNames.Exp).Value);

            var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);


            if (expiryDateTimeUtc > DateTime.UtcNow)
            {
                return new ErrorDataResult<UserTokenDto>("This token hasn't expired yet");
            }

            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;

            var storedRefreshToken = await _unitOfWork.GetQuery<RefreshToken>()
                .SingleOrDefaultAsync(rt => rt.Id ==  Guid.Parse(refreshToken));

            if (storedRefreshToken == null)
            {
                return new ErrorDataResult<UserTokenDto>("This refresh token does not exist");
            }

            if(DateTime.UtcNow > storedRefreshToken.ExpiryDate)
            {
                return new ErrorDataResult<UserTokenDto>("This refresh token has expired");
            }

            if(storedRefreshToken.Used)
            {
                return new ErrorDataResult<UserTokenDto>("This refresh token has been used");
            }

            if (storedRefreshToken.Invalidated)
            {
                return new ErrorDataResult<UserTokenDto>("This refresh token has been invalidated");
            }
            storedRefreshToken.Used = true;

            User user = _unitOfWork.GetQuery<User>()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .AsNoTracking()
                .FirstOrDefault(x => x.Id == storedRefreshToken.UserId);


            if(user == null)
            {
                return new ErrorDataResult<UserTokenDto>("Invalid Token");
            }

            var accessToken = _tokenHelper.CreateToken(user, _refreshTokenRepository);

            await _unitOfWork.SaveAsync();

            var userTokenDto = new UserTokenDto
            {
                UserId = user.Id,
                Token = accessToken.Token,
                RefreshToken = accessToken.RefreshToken
            };

            return new SuccessDataResult<UserTokenDto> (userTokenDto);
        }


        private ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, _tokenValidationParameters, out var validatedToken);

                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
        }
        
    }
}
