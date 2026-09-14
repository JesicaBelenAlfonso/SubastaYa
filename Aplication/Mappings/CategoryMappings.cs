using Domain.Entities;
using SubastaYa.Application.DTOs;

namespace SubastaYa.Application.Mappings
{
    public static class CategoryMappings
    {
        public static CategoryResponseDto ToDto(this Category category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                UrlIcono = category.UrlIcono
            };
        }
    }
}
