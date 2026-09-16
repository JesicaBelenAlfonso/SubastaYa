using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Audits.Handlers;
using SubastaYa.Application.UseCases.Audits.Queries;

namespace SubastaYa.Api.Controllers
{
    /// <summary>
    /// AuditorÃ­a (AuditLog): traza de alta confianza de las operaciones del sistema.
    /// </summary>
    [ApiController]
    [Route("api/v1/audits")]
    public class AuditsController : ControllerBase
    {
        private readonly GetAuditsQueryHandler _handler;

        public AuditsController(GetAuditsQueryHandler handler)
        {
            _handler = handler;
        }

        /// <summary>
        /// Lista los eventos de auditorÃ­a, filtrable por entidad y/o acciÃ³n.
        /// </summary>
        /// <response code="200">Listado de auditorÃ­a (mÃ¡s recientes primero).</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AuditResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] string? entity, [FromQuery] string? action)
        {
            var audits = await _handler.Handle(new GetAuditsQuery(entity, action));
            return Ok(audits);
        }
    }
}
