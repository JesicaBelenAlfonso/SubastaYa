using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Audits.Handlers;
using SubastaYa.Application.UseCases.Audits.Queries;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/audits")]
    public class AuditsController : ControllerBase
    {
        private readonly GetAuditsQueryHandler _handler;

        public AuditsController(GetAuditsQueryHandler handler)
        {
            _handler = handler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? entity, [FromQuery] string? action)
        {
            var audits = await _handler.Handle(new GetAuditsQuery(entity, action));
            return Ok(audits);
        }
    }
}