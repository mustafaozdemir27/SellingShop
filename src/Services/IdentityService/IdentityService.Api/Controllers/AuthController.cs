using IdentityService.Api.Application.Models;
using IdentityService.Api.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace IdentityService.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityManagementService _identityService;

        public AuthController(IIdentityManagementService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost]
        [Route("login")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
        {
            var result = await _identityService.Login(model);

            return Ok(result);
        }
    }
}
