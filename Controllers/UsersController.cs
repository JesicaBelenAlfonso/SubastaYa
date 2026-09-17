using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Users.Commands;
using SubastaYa.Application.UseCases.Users.Handlers;
using SubastaYa.Application.UseCases.Users.Queries;



namespace SubastaYa.Api.Controllers
{
    /// Usuarios: registro, consulta, actualización, baja y actividades.
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


        /// Registra un nuevo usuario y le crea su billetera.
        /// 201: Usuario registrado. 400: Datos inválidos o email duplicado.
   
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(RegisterUserCommand cmd)
        {
            var result = await _registerHandler.Handle(cmd);

            return CreatedAtAction(nameof(Register), new { id = result.Id }, result);

        }

        /// Devuelve un usuario por su id.
        /// 200: Usuario encontrado. 404: No existe el usuario.
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


        /// Actividades del usuario: subastas que publicó y subastas donde pujó.
        /// 200: Listado de actividades. 404: No existe el usuario.
        [HttpGet("{userId}/activities")]
        [ProducesResponseType(typeof(IEnumerable<UserActivityResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetActivities(int userId)
        {
            var activities = await _getActivitiesHandler.Handle(new GetUserActivitiesQuery(userId));
            return Ok(activities);
        }


        /// Actualiza los datos de un usuario.
        /// 200: Usuario actualizado. 404: No existe el usuario.
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, UpdateUserCommand cmd)
        {
            cmd.Id = id;    // el Id viene de la URL, se lo asignamos al Command acá

            var result = await _updateHandler.Handle(cmd);
            return Ok(result);
        }

        /// Elimina un usuario.
        /// 204: Usuario eliminado. 404: No existe el usuario.
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
