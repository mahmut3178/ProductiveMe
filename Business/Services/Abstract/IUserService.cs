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
        public Task<IDataResult<Guid>> Register(UserRegisterDto userDto);
    }
}
