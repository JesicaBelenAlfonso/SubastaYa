using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Categories.Commands;
using SubastaYa.Application.UseCases.Categories.Handlers;
using SubastaYa.Application.UseCases.Categories.Queries;

namespace SubastaYa.Api.Controllers
{
    /// <summary>
    /// CategorÃ­as del catÃ¡logo (ElectrÃ³nica, VehÃ­culos, Coleccionables, Hogar...).
    /// </summary>
    [ApiController]
    [Route("api/v1/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly CreateCategoryCommandHandler _createHandler;
        private readonly GetCategoriesQueryHandler _getAllHandler;
        private readonly GetCategoryByIdQueryHandler _getByIdHandler;

        public CategoriesController(
            CreateCategoryCommandHandler createHandler,
            GetCategoriesQueryHandler getAllHandler,
            GetCategoryByIdQueryHandler getByIdHandler)
        {
            _createHandler = createHandler;
            _getAllHandler = getAllHandler;
            _getByIdHandler = getByIdHandler;
        }

        /// <summary>
        /// Crea una categorÃ­a.
        /// </summary>
        /// <response code="201">CategorÃ­a creada.</response>
        /// <response code="400">Nombre invÃ¡lido o duplicado.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateCategoryCommand cmd)
        {
            var result = await _createHandler.Handle(cmd);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        /// <summary>
        /// Lista todas las categorÃ­as.
        /// </summary>
        /// <response code="200">Listado de categorÃ­as.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _getAllHandler.Handle(new GetCategoriesQuery());
            return Ok(categories);
        }

        /// <summary>
        /// Devuelve una categorÃ­a por su id.
        /// </summary>
        /// <response code="200">CategorÃ­a encontrada.</response>
        /// <response code="404">No existe la categorÃ­a.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _getByIdHandler.Handle(new GetCategoryByIdQuery(id));

            return category is null
                ? NotFound()
                : Ok(category);
        }
    }
}
