using Business.Services.Abstract;
using Business.Services.Concrete.EntityFramework;
using Business.UnitTests.Contexts;
using Business.UnitTests.Helpers;
using Core.Entities.Concrete;
using Core.UnitOfWork;
using Core.UnitOfWork.ORMS;
using Core.UnitOfWork.Repositories;
using Core.Utilities.Security.Jwt;
using DataAccess.Concrete;
using DataAccess.Dtos.Auth;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Business.UnitTests.Tests.Services
{
    public class UserServiceTests
    {
        private IUserService _userService;
        private Mock<IUnitOfWork> _mockUow = new Mock<IUnitOfWork>();
        private Mock<ITokenHelper> _tokenHelper = new Mock<ITokenHelper>();
        private Mock<IRepository<User>> _mockUserRepository = new Mock<IRepository<User>>();
        private Mock<IRepository<Role>> _mockRoleRepository = new Mock<IRepository<Role>>();
        private List<Role> _roleList = new List<Role> { new Role { Id = Guid.NewGuid() } };

        [SetUp]
        public void SetUp()
        {
            _mockUow.Setup(x => x.GetEntityRepository<User>()).Returns(_mockUserRepository.Object);
            _mockUow.Setup(x => x.GetEntityRepository<Role>()).Returns(_mockRoleRepository.Object);
            _userService = new UserService(_mockUow.Object, _tokenHelper.Object);
        }

        [Test]
        public void Register_ShouldBeSuccessful_WhenUsernameAndEmailIsUnique()
        {
            //arrange
            var newUser = new UserRegisterDto
            {
                Email = "john_doe@gmail.com",
                FirstName = "John",
                LastName = "Doe",
                UserName = "john317",
                Password = "doe123"
            };

            _mockUserRepository.Setup(x => x.Get(It.IsAny<Expression<Func<User, bool>>>())).Returns<User>(null);
            _mockRoleRepository.Setup(x => x.GetMany(It.IsAny<Expression<Func<Role, bool>>>())).Returns(_roleList);
            _tokenHelper.Setup(x => x.CreateToken(It.IsAny<User>(), It.IsAny<List<Role>>())).Returns(new AccessToken { Token = "JwtToken"});

            //act
            var result = _userService.Register(newUser);

            //assert
            _mockUserRepository.Verify(x => x.Create(It.IsAny<User>()), Times.Once);
            Assert.True(result.Result.Success);
            
        }
    }
}
