﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Dtos.Auth
{
    public class RefreshTokenRequestDto
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}
