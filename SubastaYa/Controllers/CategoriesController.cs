using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Categories.Handlers;
using SubastaYa.Application.UseCases.Categories.Queries;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly GetCategoriesQueryHandler _getAllHandler;
        private readonly GetCategoryByIdQueryHandler _getByIdHandler;

        public CategoriesController(
            GetCategoriesQueryHandler getAllHandler,
            GetCategoryByIdQueryHandler getByIdHandler)
        {
            _getAllHandler = getAllHandler;
            _getByIdHandler = getByIdHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _getAllHandler.Handle(new GetCategoriesQuery());
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var category = await _getByIdHandler.Handle(new GetCategoryByIdQuery(id));

            return category is null
                ? NotFound()
                : Ok(category);
        }
    }
}
