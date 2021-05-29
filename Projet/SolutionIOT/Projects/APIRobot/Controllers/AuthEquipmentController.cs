using APIRobot.Models;
using APIRobot.Models.Auth;
using APIRobot.Services;
using Authentification.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace APIRobot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthEquipmentController : ControllerBase
    {

        private readonly JwtService Service;

        public AuthEquipmentController(JwtService Service)
        {
            this.Service = Service;
        }

        [DisableCors]
        [AllowAnonymous]
        [HttpPost("auth")]
        public ActionResult<JwtToken> Login([FromBody] EquipmentAuthView equipment)
        {

            JwtToken token = Service.GetJwtToken(equipment);

            if (token is null)
                return BadRequest("{ \"reason\": \"Bad credencial\" }");

            return Ok(token);
        }
    }
}
