using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Auth.Commands;
using SubastaYa.Application.UseCases.Auth.Handlers;

namespace SubastaYa.Api.Controllers
{
    /// Autenticación de usuarios.
 
    [ApiController]
    [Route("api/v1/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LoginCommandHandler _loginHandler;

        public AuthController(LoginCommandHandler loginHandler)
        {
            _loginHandler = loginHandler;
        }

        /// Inicia sesión (crea una "sesión") con email y contraseña.
        /// 201 Created: Devuelve un objeto UserResponseDto con la información del usuario autenticado.
        /// 401 Unauthorized: Credenciales inválidas.
 
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
