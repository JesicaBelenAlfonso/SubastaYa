using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Categories.Commands;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Categories.Handlers
{
    public class CreateCategoryCommandHandler
    {
        private readonly ICategoryRepository _categories;
        private readonly IUnitOfWork _uow;

        public CreateCategoryCommandHandler(ICategoryRepository categories, IUnitOfWork uow)
        {
            _categories = categories;
            _uow = uow;
        }

        public async Task<CategoryResponseDto> Handle(CreateCategoryCommand cmd)
        {
            if (await _categories.ExistsByNameAsync(cmd.Name))
                throw new DomainConflictException($"Ya existe una categoría con el nombre '{cmd.Name}'");

            var category = cmd.ToEntity();

            await _categories.AddAsync(category);

            await _uow.SaveChangesAsync();

            return category.ToDto();
        }
    }
}