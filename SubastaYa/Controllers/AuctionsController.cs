using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Auctions.Commands;
using SubastaYa.Application.UseCases.Auctions.Handlers;
using SubastaYa.Application.UseCases.Auctions.Queries;

namespace SubastaYa.Api.Controllers
{
    /// Subastas del catálogo. Estados posibles: ACTIVA, PROXIMA, FINALIZADA, DESIERTA.
    [ApiController]
    [Route("api/v1/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly CreateAuctionCommandHandler _createHandler;
        private readonly GetAuctionByIdQueryHandler _getByIdHandler;
        private readonly GetAllAuctionsQueryHandler _getAllHandler;
        private readonly DeleteAuctionCommandHandler _deleteHandler;
        private readonly UpdateAuctionCommandHandler _updateHandler;
        private readonly GetAuctionBidsQueryHandler _getBidsHandler;

        public AuctionsController(
            CreateAuctionCommandHandler createHandler,
            GetAuctionByIdQueryHandler getByIdHandler,
            GetAllAuctionsQueryHandler getAllHandler,
            DeleteAuctionCommandHandler deleteHandler,
            UpdateAuctionCommandHandler updateHandler,
            GetAuctionBidsQueryHandler getBidsHandler)
        {
            _createHandler = createHandler;
            _getByIdHandler = getByIdHandler;
            _getAllHandler = getAllHandler;
            _deleteHandler = deleteHandler;
            _updateHandler = updateHandler;
            _getBidsHandler = getBidsHandler;
        }

 
        /// Lista todas las subastas con oferta actual y cantidad de pujas.
        /// Ejemplo de respuesta (201 OK):
        /// [
        ///   {
        ///     "id": 1,
        ///     "title": "Notebook Gamer RTX 16GB",
        ///     "basePrice": 40000,
        ///     "status": "ACTIVA",
        ///     "ofertaActual": 45000,
        ///     "cantidadPujas": 2
        ///   }
        /// ]
        /// <response code="200">Listado de subastas.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<AuctionResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var auctions = await _getAllHandler.Handle(new GetAllAuctionsQuery());
            return Ok(auctions);
        }
       /// Crea una subasta. Nace ACTIVA si ya empezó o PROXIMA si aún no comenzó.
        /// Ejemplo de request:
        /// {
        ///   "sellerId": 1,
        ///   "categoryId": 1,
        ///   "title": "Notebook Gamer RTX 16GB",
        ///   "descripcion": "Notebook usada con garantía.",
        ///   "urlImagen": "https://picsum.photos/seed/notebook/600/400",
        ///   "basePrice": 40000,
        ///   "minIncrement": 5000,
        ///   "startDate": "2026-09-16T10:00:00Z",
        ///   "endDate": "2026-09-18T10:00:00Z"
        /// }
        /// <response code="201">Subasta creada.</response>
        /// <response code="400">Datos inválidos (por ejemplo, fin antes del inicio).</response>
        [HttpPost]
        [ProducesResponseType(typeof(AuctionResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateAuctionCommand cmd)
        {
            var result = await _createHandler.Handle(cmd);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }


        /// Devuelve una subasta por su id.
        /// <response code="200">Subasta encontrada.</response>
        /// <response code="404">No existe la subasta.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(AuctionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            var auction = await _getByIdHandler.Handle(new GetAuctionByIdQuery(id));

            return auction is null
                ? NotFound()
                : Ok(auction);
        }

   
        /// Historial de pujas de una subasta, anonimizado y ordenado de más reciente a más antigua.
        /// Cada pujador aparece con un rótulo neutro ("Pujador 1", "Pujador 2"...); no se expone
        /// su identidad real. La puja vigente (líder) llega con <c>isLeader: true</c>. Si se pasa
        /// <c>userId</c>, las pujas propias se marcan con <c>isMine: true</c> (para mostrar
        /// "Liderando/Superado" en la sala de pujas).
        /// <response code="200">Historial de pujas (vacío si todavía no hubo ninguna).</response>
        /// <response code="404">No existe la subasta.</response>
        [HttpGet("{id}/bids")]
        [ProducesResponseType(typeof(IEnumerable<BidHistoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBids(int id, int? userId)
        {
            var bids = await _getBidsHandler.Handle(new AuctionBidsQuery(id, userId));
            return Ok(bids);
        }

        /// Actualiza los datos de una subasta.
        /// <response code="200">Subasta actualizada.</response>
        /// <response code="400">Datos inválidos.</response>
        /// <response code="404">Subasta inexistente.</response>
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(AuctionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, UpdateAuctionCommand cmd)
        {
            cmd.Id = id;

            var result = await _updateHandler.Handle(cmd);
            return Ok(result);
        }

  
        /// Elimina una subasta.
        /// <response code="204">Subasta eliminada.</response>
        /// <response code="404">Subasta inexistente.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            await _deleteHandler.Handle(new DeleteAuctionCommand(id));
            return NoContent();
        }
    }
}