using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Users.Commands;
using SubastaYa.Application.UseCases.Users.Handlers;
using SubastaYa.Application.UseCases.Users.Queries;



namespace SubastaYa.Api.Controllers
{
    /// <summary>
    /// Usuarios: registro, consulta, actualización, baja y actividades.
    /// </summary>
    [ApiController]
    [Route("api/v1/users")]
    public class UsersController : ControllerBase
    {
        private readonly RegisterUserCommandHandler _registerHandler;
        private readonly GetUserByIdQueryHandler _getByIdHandler;
        private readonly DeleteUserCommandHandler _deleteHandler;
        private readonly UpdateUserCommandHandler _updateHandler;
        private readonly GetUserActivitiesQueryHandler _getActivitiesHandler;
        public UsersController(RegisterUserCommandHandler registerHandler, 
            GetUserByIdQueryHandler getByIdHandler, DeleteUserCommandHandler deleteHandler,
            UpdateUserCommandHandler updateHandler, GetUserActivitiesQueryHandler getActivitiesHandler)
        {
            _registerHandler = registerHandler;
            _getByIdHandler = getByIdHandler;
            _deleteHandler = deleteHandler;
            _updateHandler = updateHandler;
            _getActivitiesHandler = getActivitiesHandler;
        }

        /// <summary>
        /// Registra un nuevo usuario y le crea su billetera.
        /// </summary>
        /// <response code="201">Usuario registrado.</response>
        /// <response code="400">Datos inválidos o email duplicado.</response>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterUserCommand cmd)
        {
            var result = await _registerHandler.Handle(cmd);

            return CreatedAtAction(nameof(Register), new { id = result.Id }, result);

        }

        /// <summary>
        /// Devuelve un usuario por su id.
        /// </summary>
        /// <response code="200">Usuario encontrado.</response>
        /// <response code="404">No existe el usuario.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _getByIdHandler.Handle(new GetUserByIdQuery(id));

            return user is null
                ? NotFound()
                : Ok(user);
        }

        /// <summary>
        /// Actividades del usuario: subastas que publicó y subastas donde pujó.
        /// </summary>
        /// <response code="200">Listado de actividades.</response>
        /// <response code="404">No existe el usuario.</response>
        [HttpGet("{userId}/activities")]
        [ProducesResponseType(typeof(IEnumerable<UserActivityResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActivities(int userId)
        {
            var activities = await _getActivitiesHandler.Handle(new GetUserActivitiesQuery(userId));
            return Ok(activities);
        }

        /// <summary>
        /// Actualiza los datos de un usuario.
        /// </summary>
        /// <response code="200">Usuario actualizado.</response>
        /// <response code="404">No existe el usuario.</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, UpdateUserCommand cmd)
        {
            cmd.Id = id;    // el Id viene de la URL, se lo asignamos al Command acá

            var result = await _updateHandler.Handle(cmd);
            return Ok(result);
        }

        /// <summary>
        /// Elimina un usuario.
        /// </summary>
        /// <response code="204">Usuario eliminado.</response>
        /// <response code="404">No existe el usuario.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _deleteHandler.Handle(new DeleteUserCommand(id));
            return NoContent();   // 204: se hizo, pero no hay nada que devolver
        }

      
    }
}
