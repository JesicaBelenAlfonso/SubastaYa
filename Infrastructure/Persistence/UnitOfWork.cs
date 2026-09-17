using SubastaYa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
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
        private dynamic _transaction;

        public UnitOfWork(AppDbContext ctx)
        {
            _ctx = ctx;
            _transaction = null;
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _ctx.SaveChangesAsync(cancellationToken);
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _ctx.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                _transaction = null;
            }
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                _transaction = null;
            }
        }

        public void SetOriginalValue(object entity, string propertyName, object? value)
        {
            _ctx.Entry(entity).Property(propertyName).OriginalValue = value;
        }

        public void DetachAll()
        {
            foreach (var entry in _ctx.ChangeTracker.Entries())
            {
                entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
        }
    }
}