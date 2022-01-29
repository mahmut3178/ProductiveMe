using Business.Services.Abstract;
using Business.Services.Concrete.EntityFramework;
using Core.Entities.Concrete;
using Core.UnitOfWork;
using Core.UnitOfWork.Repositories;
using Core.Utilities.Security.Hashing;
using Core.Utilities.Security.Jwt;
using DataAccess.Dtos.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Business.UnitTests.Tests.Services
{
    public class AuthServiceTests
    {
        private IAuthService _authService;
        private Mock<IUnitOfWork> _mockUow;
        private Mock<ITokenHelper> _mockTokenHelper;
        private Mock<IHashingHelper> _mockHashingHelper;
        private Mock<IRepository<User>> _mockUserRepository;
        private Mock<IRepository<Role>> _mockRoleRepository;
        private Mock<TokenValidationParameters> _mockTokenValidationParameters;
        private List<Role> _roleList = new List<Role> { new Role { Id = Guid.NewGuid() } };

        UserRegisterDto newUser = new UserRegisterDto
        {
            Email = "john_doe@gmail.com",
            FirstName = "John",
            LastName = "Doe",
            UserName = "john317",
            Password = "doe123"
        };

        UserLoginDto userLogin = new UserLoginDto
        {
            Username = "john317",
            Password = "doe123"
        };

        User mockDatabaseUser = new User
        {
            UserName = "john317",
            Email = "john_doe@gmail.com",
            UserRoles = new List<UserRole> { new UserRole { Role = new Role { Id = Guid.NewGuid() } } }
        };


        [SetUp]
        public void SetUp()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockTokenHelper = new Mock<ITokenHelper>();

            _mockUserRepository = new Mock<IRepository<User>>();
            _mockRoleRepository = new Mock<IRepository<Role>>();
            _mockHashingHelper = new Mock<IHashingHelper>();
            _mockTokenValidationParameters = new Mock<TokenValidationParameters>();
            _mockUow.Setup(x => x.GetEntityRepository<User>()).Returns(_mockUserRepository.Object);
            _mockUow.Setup(x => x.GetEntityRepository<Role>()).Returns(_mockRoleRepository.Object);
            _authService = new AuthService(_mockUow.Object, _mockTokenHelper.Object, _mockHashingHelper.Object, _mockTokenValidationParameters.Object);

            _mockRoleRepository.Setup(x => x.GetMany(It.IsAny<Expression<Func<Role, bool>>>()))
                .Returns(_roleList);

            _mockTokenHelper.Setup(x => x.CreateToken(It.IsAny<User>(), It.IsAny<IRepository<RefreshToken>>()))
                .Returns(new AccessToken { Token = "JwtToken" });
        }

        [Test]
        public async Task Register_ShouldBeSuccessful_WhenUsernameAndEmailIsUnique()
        {

            _mockUserRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>())).Returns<User>(null);


            var result = await _authService.Register(newUser);

            //assert
            _mockUserRepository.Verify(x => x.Create(It.IsAny<User>()), Times.Once);
            Assert.True(result.Success);
        }

        [Test]
        public async Task Register_ShouldBeUnSuccessful_WhenUsernameOrEmailExists()
        {
            _mockUserRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>())).Returns(new Mock<User>().Object);


            var result = await _authService.Register(newUser);


            _mockUserRepository.Verify(x => x.Create(It.IsAny<User>()), Times.Never);
            Assert.False(result.Success);
        }


        [Test]
        public void Login_ShouldBeSuccessful_WhenCredentialsAreValid()
        {
            _mockUow.Setup(x => x.GetQuery<User>()).Returns(new List<User> { mockDatabaseUser }.AsQueryable());
            _mockHashingHelper.Setup(x => x.VerifyPasswordHash(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(true);
            var result = _authService.Login(userLogin);

            Assert.True(result.Success);
            Assert.AreEqual(result.Message, "Login successful!");
        }

        [Test]
        public void Login_ShouldBeUnSuccessful_WhenUsernameOrEmailInvalid()
        {
            _mockUow.Setup(x => x.GetQuery<User>()).Returns(new List<User> { mockDatabaseUser }.AsQueryable());
            _mockHashingHelper.Setup(x => x.VerifyPasswordHash(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(true);

            UserLoginDto invalidUserLogin = new UserLoginDto
            {
                Username = "john",
                Password = "doe123"
            };

            var result = _authService.Login(invalidUserLogin);

            Assert.False(result.Success);
            Assert.AreEqual(result.Message, "User does not exist!");
        }

        [Test]
        public void Login_ShouldBeUnSuccessful_WhenPasswordInvalid()
        {
            _mockUow.Setup(x => x.GetQuery<User>()).Returns(new List<User> { mockDatabaseUser }.AsQueryable());
            _mockHashingHelper.Setup(x => x.VerifyPasswordHash(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<byte[]>())).Returns(false);

            var result = _authService.Login(userLogin);

            Assert.False(result.Success);
            Assert.AreEqual(result.Message, "Invalid credentials!");
        }
    }
}
