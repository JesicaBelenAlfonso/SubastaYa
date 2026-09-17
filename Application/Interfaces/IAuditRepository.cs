using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface IAuditRepository
    {
        Task AddAsync(Audit audit);
    }
}
