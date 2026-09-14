using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Auth.Commands;
using SubastaYa.Application.UseCases.Auth.Handlers;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LoginCommandHandler _loginHandler;

        public AuthController(LoginCommandHandler loginHandler)
        {
            _loginHandler = loginHandler;
        }

        // POST api/v1/auth/sessions — crea una sesión (login), sin verbos en la URL.
        [HttpPost("sessions")]
        public async Task<IActionResult> Login(LoginCommand cmd)
        {
            var user = await _loginHandler.Handle(cmd);

            // 201 Created: se creó el recurso "sesión".
            return StatusCode(StatusCodes.Status201Created, user);
        }
    }
}