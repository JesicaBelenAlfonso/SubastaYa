using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Categories.Queries;
using System.Threading.Tasks;


namespace SubastaYa.Application.UseCases.Categories.Handlers
{
    public class GetCategoriesQueryHandler
    {
        private readonly ICategoryRepository _categories;

        public GetCategoriesQueryHandler(ICategoryRepository categories)
        {
            _categories = categories;
        }

        public async Task<IEnumerable<CategoryResponseDto>> Handle(GetCategoriesQuery query)
        {
            var categories = await _categories.GetAllAsync();
            return categories.Select(c => c.ToDto());
        }
    }
}
