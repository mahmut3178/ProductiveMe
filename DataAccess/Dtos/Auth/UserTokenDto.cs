using Core.Entities.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Dtos.Auth
{
    public class UserTokenDto : Dto
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
    }
}
