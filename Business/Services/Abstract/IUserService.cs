using Core.Entities.Concrete;
using Core.UnitOfWork.Repositories;
using Core.Utilities.ResultsHelper;
using DataAccess.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Services.Abstract
{
    public interface IUserService
    {
        public Task<IDataResult<UserTokenDto>> Register(UserRegisterDto userDto);
        public IDataResult<UserTokenDto> Login(UserLoginDto userDto);
    }
}
