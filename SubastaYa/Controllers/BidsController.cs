using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Bids.Commands;
using SubastaYa.Application.UseCases.Bids.Handlers;

namespace SubastaYa.Api.Controllers
{

    /// Pujas: el corazon de la subasta (escrow + anti-sniping).

    [ApiController]
    [Route("api/v1/bids")]
    public class BidsController : ControllerBase
    {
        private readonly CreateBidCommandHandler _createHandler;

        public BidsController(CreateBidCommandHandler createHandler)
        {
            _createHandler = createHandler;
        }


        /// Registra una puja sobre una subasta ACTIVA.
        /// 201 Created: La puja fue registrada correctamente.
        /// 400 Bad Request: El monto de la puja es inválido (menor al mínimo o menor a la puja actual).
        /// 404 Not Found: La subasta no existe.
        /// 409 Conflict: La subasta no está en estado ACTIVA o la puja no pudo registrarse por un conflicto de concurrencia.
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
