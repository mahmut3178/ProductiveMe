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
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Standard")]
        public void Test()
        {
            Console.WriteLine("The purpose of this action is to test the systems Authorization.");
        }

        [HttpPost]
        public async Task<ActionResult<UserTokenDto>> Register(UserRegisterDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest("Empty user data");
            }

            var result = await _userService.Register(userDto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }


        [HttpPost("session")]
        public ActionResult<UserTokenDto> Login(UserLoginDto userDto)
        {
            if (userDto == null)
            {
                return BadRequest("Empty user data");
            }

            var result = _userService.Login(userDto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}
