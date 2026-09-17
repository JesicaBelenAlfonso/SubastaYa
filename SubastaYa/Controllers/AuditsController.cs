using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Audits.Handlers;
using SubastaYa.Application.UseCases.Audits.Queries;

namespace SubastaYa.Api.Controllers
{
    
    /// Auditoria (AuditLog): traza de alta confianza de las operaciones del sistema.
    /// Proporciona un registro immutable de eventos del sistema para compliance y debugging.
   
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
        /// <response code="200">Se devolvió el listado de eventos.</response>
        /// <response code="400">Parámetros de filtro inválidos.</response>

        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AuditResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAll([FromQuery] string? entity, [FromQuery] string? action)
        {
            if (string.IsNullOrWhiteSpace(entity) && string.IsNullOrWhiteSpace(action))
            {
                return BadRequest("Se requiere al menos un parámetro de filtro (entity o action).");
            }

            var audits = await _handler.Handle(new GetAuditsQuery(entity, action));
            return Ok(audits);
        }
    }
}