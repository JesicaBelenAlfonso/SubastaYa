using Domain.Entities;
using Infraestructure.Persistence;
using SubastaYa.Application.Interfaces;

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
    }
}
