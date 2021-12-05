using Business.Services.Abstract;
using Core.Utilities.ResultsHelper;
using DataAccess.Dtos.Auth;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost]
        public async Task<ActionResult<string>> Register(UserRegisterDto userDto)
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

    }
}
