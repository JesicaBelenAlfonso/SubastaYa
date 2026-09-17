namespace SubastaYa.Application.Interfaces
{
    public interface IAuditService
    {
        Task LogAsync(string entity, int entityId, AuditAction action, int userId, object? detail = null);
    }
}
