using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task AddAsync(Category category);
        Task<bool> ExistsByNameAsync(string name);
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
    }
}
