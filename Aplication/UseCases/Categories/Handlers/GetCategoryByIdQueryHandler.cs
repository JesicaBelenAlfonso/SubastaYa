using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Categories.Queries;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Categories.Handlers
{
    public class GetCategoryByIdQueryHandler
    {
        private readonly ICategoryRepository _categories;

        public GetCategoryByIdQueryHandler(ICategoryRepository categories)
        {
            _categories = categories;
        }

        public async Task<CategoryResponseDto?> Handle(GetCategoryByIdQuery query)
        {
            var category = await _categories.GetByIdAsync(query.Id);

            if (category is null)
                return null;

            return category.ToDto();
        }
    }
}
