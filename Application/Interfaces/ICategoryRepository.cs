using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(int id);
    }
}
