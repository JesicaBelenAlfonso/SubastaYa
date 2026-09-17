using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces;


namespace SubastaYa.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _ctx;

        public CategoryRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(Category category)
        {
            await _ctx.Categories.AddAsync(category);
        }

        public async Task<bool> ExistsByNameAsync(string name)
            => await _ctx.Categories.AnyAsync(c => c.Name == name);

        public async Task<IEnumerable<Category>> GetAllAsync()
            => await _ctx.Categories.OrderBy(c => c.Name).ToListAsync();

        public async Task<Category?> GetByIdAsync(int id)
            => await _ctx.Categories.FirstOrDefaultAsync(c => c.Id == id);
    }
}
