using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Bids.Commands;
using SubastaYa.Application.UseCases.Bids.Handlers;

namespace SubastaYa.Api.Controllers
{
    /// <summary>
    /// Pujas: el corazÃ³n de la subasta (escrow + anti-sniping).
    /// </summary>
    [ApiController]
    [Route("api/v1/bids")]
    public class BidsController : ControllerBase
    {
        private readonly CreateBidCommandHandler _createHandler;

        public BidsController(CreateBidCommandHandler createHandler)
        {
            _createHandler = createHandler;
        }

        /// <summary>
        /// Registra una puja sobre una subasta ACTIVA.
        /// </summary>
        /// <response code="201">Puja registrada.</response>
        /// <response code="400">Monto invÃ¡lido.</response>
        /// <response code="404">Subasta inexistente.</response>
        /// <response code="409">Conflicto de estado o de concurrencia.</response>
        [HttpPost]
        [ProducesResponseType(typeof(BidResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Create(CreateBidCommand cmd)
        {
            var result = await _createHandler.Handle(cmd);

            return CreatedAtAction(nameof(AuctionsController.GetById), "Auctions", new { id = result.AuctionId }, result);
        }
    }
}
