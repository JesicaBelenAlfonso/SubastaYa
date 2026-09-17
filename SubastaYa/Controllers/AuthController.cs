using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Auth.Commands;
using SubastaYa.Application.UseCases.Auth.Handlers;

namespace SubastaYa.Api.Controllers
{
    /// <summary>
    /// Autenticación de usuarios.
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
        /// Inicia sesión (crea una "sesión") con email y contraseña.
        /// </summary>
        /// <response code="200">Sesión iniciada correctamente.</response>
        /// <response code="401">Credenciales inválidas o cuenta no encontrada.</response>
        /// <response code="400">Datos de entrada inválidos.</response>
        [HttpPost("sessions")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(LoginCommand cmd)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _loginHandler.Handle(cmd);

            if (user is null)
            {
                return Unauthorized();
            }

            return Ok(user);
        }
    }
}