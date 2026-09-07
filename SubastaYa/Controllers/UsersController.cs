using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Users.Commands;
using SubastaYa.Application.UseCases.Users.Handlers;
using SubastaYa.Application.UseCases.Users.Queries;



namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        // El controller no conoce IUserRepository, ni AppDbContext, ni EF Core.
        // Solo conoce el Handler — una línea de dependencia, no cinco.
        private readonly RegisterUserCommandHandler _registerHandler;
        private readonly GetUserByIdQueryHandler _getByIdHandler;
        private readonly DeleteUserCommandHandler _deleteHandler;
        private readonly UpdateUserCommandHandler _updateHandler;
        public UsersController(RegisterUserCommandHandler registerHandler, 
            GetUserByIdQueryHandler getByIdHandler, DeleteUserCommandHandler deleteHandler, UpdateUserCommandHandler updateHandler)
        {
            _registerHandler = registerHandler;
            _getByIdHandler = getByIdHandler;
            _deleteHandler = deleteHandler;
            _updateHandler = updateHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUserCommand cmd)
        {
            // Si el cmd no cumple los [Required]/[EmailAddress] del Command,
            // ASP.NET ya devolvió 400 automáticamente y este código
            // ni se llega a ejecutar (por el atributo [ApiController]).
            var result = await _registerHandler.Handle(cmd);

            // 201 Created es el código correcto para "se creó un recurso nuevo".
            return CreatedAtAction(nameof(Register), new { id = result.Id }, result);

        }
        [HttpGet("{id}")]                                              // ← nuevo
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _getByIdHandler.Handle(new GetUserByIdQuery(id));

            return user is null
                ? NotFound()
                : Ok(user);
        }  
        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, UpdateUserCommand cmd)
        {
            cmd.Id = id;    // el Id viene de la URL, se lo asignamos al Command acá

            var result = await _updateHandler.Handle(cmd);
            return Ok(result);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _deleteHandler.Handle(new DeleteUserCommand(id));
            return NoContent();   // 204: se hizo, pero no hay nada que devolver
        }

      
    }
}