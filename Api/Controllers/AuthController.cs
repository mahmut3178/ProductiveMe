using Business.Services.Abstract;
using Core.Entities.Concrete;
using Core.Utilities.ResultsHelper;
using DataAccess.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        [Authorize(Roles = "Standard, Admin")]
        public void Test()
        {
            Console.WriteLine("The purpose of this action is to test the systems Authorization.");
        }

        [HttpPost]
        public async Task<ActionResult<UserTokenDto>> Register(UserRegisterDto userDto)
        {
            if (userDto == null)
            {
                return ValidationProblem("Empty user data");
            }

            var result = await _authService.Register(userDto);

            if (!result.Success)
                return ValidationProblem(result.Message);

            return Ok(result.Data);
        }


        [HttpPost("session")]
        public ActionResult<UserTokenDto> Login(UserLoginDto userDto)
        {
            if (userDto == null)
            {
                return ValidationProblem("Empty user data");
            }

            var result = _authService.Login(userDto);

            if (!result.Success)
                return ValidationProblem(result.Message);

            return Ok(result.Data);
        }
    }
}
