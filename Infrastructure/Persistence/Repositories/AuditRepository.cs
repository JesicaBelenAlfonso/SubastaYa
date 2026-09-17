using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Persistence;
using SubastaYa.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace SubastaYa.Infrastructure.Persistence.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly AppDbContext _ctx;

        public AuditRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(Audit audit)
        {
            await _ctx.Audits.AddAsync(audit);
        }

        public async Task<IEnumerable<Audit>> GetAllAsync(string? entity = null, string? action = null)
        {
            var query = _ctx.Audits.AsQueryable();

            if (!string.IsNullOrWhiteSpace(entity))
                query = query.Where(a => a.Entity == entity);

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.Action == action);

            return await query.OrderByDescending(a => a.Date).ToListAsync();
        }
    }
}
