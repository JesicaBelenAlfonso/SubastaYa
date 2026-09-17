using SubastaYa.Infrastructure.Persistence;
using SubastaYa.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _ctx;

        public UnitOfWork(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            return await _ctx.SaveChangesAsync(ct);
        }

        public void SetOriginalValue(object entity, string propertyName, object? value)
        {
            _ctx.Entry(entity).Property(propertyName).OriginalValue = value;
        }

        public void DetachAll()
        {
            _ctx.ChangeTracker.Clear();
        }
    }
}
