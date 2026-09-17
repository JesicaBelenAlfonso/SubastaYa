using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Auctions.Commands;
using SubastaYa.Application.UseCases.Auctions.Handlers;
using SubastaYa.Application.UseCases.Auctions.Queries;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly CreateAuctionCommandHandler _createHandler;
        private readonly GetAuctionByIdQueryHandler _getByIdHandler;
        private readonly GetAllAuctionsQueryHandler _getAllHandler;
        private readonly DeleteAuctionCommandHandler _deleteHandler;
        private readonly UpdateAuctionCommandHandler _updateHandler;

        public AuctionsController(
            CreateAuctionCommandHandler createHandler,
            GetAuctionByIdQueryHandler getByIdHandler,
            GetAllAuctionsQueryHandler getAllHandler,
            DeleteAuctionCommandHandler deleteHandler,
            UpdateAuctionCommandHandler updateHandler)
        {
            _createHandler = createHandler;
            _getByIdHandler = getByIdHandler;
            _getAllHandler = getAllHandler;
            _deleteHandler = deleteHandler;
            _updateHandler = updateHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var auctions = await _getAllHandler.Handle(new GetAllAuctionsQuery());
            return Ok(auctions);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateAuctionCommand cmd)
        {
            var result = await _createHandler.Handle(cmd);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var auction = await _getByIdHandler.Handle(new GetAuctionByIdQuery(id));

            return auction is null
                ? NotFound()
                : Ok(auction);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Update(int id, UpdateAuctionCommand cmd)
        {
            cmd.Id = id;

            var result = await _updateHandler.Handle(cmd);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _deleteHandler.Handle(new DeleteAuctionCommand(id));
            return NoContent();
        }
    }
}
