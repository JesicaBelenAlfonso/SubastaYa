using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.UseCases.Audits.Queries;

namespace SubastaYa.Application.UseCases.Audits.Handlers
{
    public class GetAuditsQueryHandler
    {
        private readonly IAuditRepository _audits;

        public GetAuditsQueryHandler(IAuditRepository audits)
        {
            _audits = audits;
        }

        public async Task<List<AuditResponseDto>> Handle(GetAuditsQuery query)
        {
            var audits = await _audits.GetAllAsync(query.Entity, query.Action);

            return audits.Select(a => new AuditResponseDto
            {
                Id = a.Id,
                Entity = a.Entity,
                EntityId = a.EntityId,
                Action = a.Action,
                UserId = a.UserId,
                DetalleJson = a.DetalleJson,
                Date = a.Date
            }).ToList();
        }
    }
}