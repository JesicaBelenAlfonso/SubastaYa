using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Categories.Commands;
using SubastaYa.Application.UseCases.Categories.Handlers;
using SubastaYa.Application.UseCases.Categories.Queries;

namespace SubastaYa.Api.Controllers
{
    
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

        /// Crea una categoría.
        /// 201 Created: Categoría creada.
        /// 400 Bad Request: Nombre inválido o duplicado.
      
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateCategoryCommand cmd)
        {
            var result = await _createHandler.Handle(cmd);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }


        /// Lista todas las categorías.
        ///200 OK: Listado de categorías.
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _getAllHandler.Handle(new GetCategoriesQuery());
            return Ok(categories);
        }

        /// Devuelve una categoría por su id.
        /// 200 OK: Categoría encontrada.
        /// 404 Not Found: No existe la categoría.
    
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