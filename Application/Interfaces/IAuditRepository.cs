using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface IAuditRepository
    {
        Task AddAsync(Audit audit);
        Task<IEnumerable<Audit>> GetAllAsync(string? entity = null, string? action = null);
    }
}
