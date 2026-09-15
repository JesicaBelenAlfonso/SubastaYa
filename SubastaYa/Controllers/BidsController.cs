using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Bids.Commands;
using SubastaYa.Application.UseCases.Bids.Handlers;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/bids")]
    public class BidsController : ControllerBase
    {
        private readonly CreateBidCommandHandler _createHandler;

        public BidsController(CreateBidCommandHandler createHandler)
        {
            _createHandler = createHandler;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateBidCommand cmd)
        {
            var result = await _createHandler.Handle(cmd);
            return Created($"/api/bids/{result.Id}", result);
        }
    }
}
