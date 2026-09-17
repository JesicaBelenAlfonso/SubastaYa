using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Auth.Commands;
using SubastaYa.Application.UseCases.Auth.Handlers;

namespace SubastaYa.Api.Controllers
{
    /// <summary>
    /// AutenticaciÃ³n de usuarios.
    /// </summary>
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LoginCommandHandler _loginHandler;

        public AuthController(LoginCommandHandler loginHandler)
        {
            _loginHandler = loginHandler;
        }

        /// <summary>
        /// Inicia sesiÃ³n (crea una "sesiÃ³n") con email y contraseÃ±a.
        /// </summary>
        /// <response code="201">SesiÃ³n creada (usuario autenticado).</response>
        /// <response code="401">Credenciales invÃ¡lidas.</response>
        [HttpPost("sessions")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(LoginCommand cmd)
        {
            var user = await _loginHandler.Handle(cmd);

            return StatusCode(StatusCodes.Status201Created, user);
        }
    }
}
