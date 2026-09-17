using System.Text.Json;
using SubastaYa.Domain.Entities;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _audits;
        private readonly IUnitOfWork _uow;

        public AuditService(IAuditRepository audits, IUnitOfWork uow)
        {
            _audits = audits;
            _uow = uow;
        }

        public async Task LogAsync(string entity, int entityId, string action, int userId, object? detail = null)
        {
            await _audits.AddAsync(new Audit
            {
                Entity = entity,
                EntityId = entityId,
                Action = action,
                UserId = userId,
                DetalleJson = detail is null
                    ? "{}"
                    : JsonSerializer.Serialize(detail),
                Date = DateTime.UtcNow
            });

            await _uow.SaveChangesAsync();
        }
    }
}
