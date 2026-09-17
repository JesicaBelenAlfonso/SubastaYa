using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Audits.Handlers;
using SubastaYa.Application.UseCases.Audits.Queries;

namespace SubastaYa.Api.Controllers
{
    
    /// Auditoria (AuditLog): traza de alta confianza de las operaciones del sistema.
   
    [ApiController]
    [Route("api/v1/audits")]
    public class AuditsController : ControllerBase
    {
        private readonly GetAuditsQueryHandler _handler;

        public AuditsController(GetAuditsQueryHandler handler)
        {
            _handler = handler;
        }


        /// Lista los eventos de auditoría, filtrable por entidad y/o acción.
        /// 200 OK: Listado de auditoría (más recientes primero).

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AuditResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? entity, [FromQuery] string? action)
        {
            var audits = await _handler.Handle(new GetAuditsQuery(entity, action));
            return Ok(audits);
        }
    }
}
