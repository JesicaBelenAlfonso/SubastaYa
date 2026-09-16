using System.Text.Json;
using SubastaYa.Domain.Entities;
using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _audits;

        public AuditService(IAuditRepository audits)
        {
            _audits = audits;
        }

        public async Task LogAsync(string entity, int entityId, AuditAction action, int userId, object? detail = null)
        {
            await _audits.AddAsync(new Audit
            {
                Entity = entity,
                EntityId = entityId,
                Action = action.ToString(),
                UserId = userId,
                DetalleJson = detail is null
                    ? "{}"
                    : JsonSerializer.Serialize(detail),
                Date = DateTime.UtcNow
            });
        }
    }
}
