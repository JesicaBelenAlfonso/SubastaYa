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


        /// Lista el catálogo de subastas con oferta actual, cantidad de pujas y metadata de paginación.
        /// Filtros opcionales: <c>estado</c> (ACTIVA, PROXIMA, FINALIZADA, DESIERTA), <c>categoriaId</c>,
        /// <c>minPrecio</c>, <c>maxPrecio</c>; ordenamiento <c>orden</c> (recientes, precio-desc, precio-asc, final)
        /// y paginación <c>pagina</c>/<c>tamano</c>.
        /// Sin query string se preserva el contrato original (array plano, listado completo) para el index/catálogo.
        /// Con parámetros se devuelve el envelope { items, pagina, tamano, total, totalPaginas }.
        /// 200 OK.
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll([FromQuery] GetAllAuctionsQuery query)
        {
            var result = await _getAllHandler.Handle(query);

            // Backward-compatible: sin parámetros el index recibe el array plano de siempre.
            return Request.Query.Count == 0
                ? Ok(result.Items)
                : Ok(result);
        }

        /// Crea una subasta. Nace ACTIVA si ya empezó o PROXIMA si aún no comenzó.
        /// 201 Created: Devuelve la subasta creada.
        /// 400 Bad Request: Datos inválidos (por ejemplo, fin antes del inicio).
        [HttpPost]
        [ProducesResponseType(typeof(AuctionResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create(CreateAuctionCommand cmd)
        {
            var result = await _createHandler.Handle(cmd);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        /// Devuelve una subasta por su id.
        /// 200 OK: Devuelve la subasta.
        /// 404 Not Found: No existe la subasta.
      
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
        /// 200 OK: Devuelve el historial de pujas (vacío si todavía no hubo ninguna).
        /// 404 Not Found: No existe la subasta.
        [HttpGet("{id}/bids")]
        [ProducesResponseType(typeof(IEnumerable<BidHistoryResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBids(int id, int? userId)
        {
            var bids = await _getBidsHandler.Handle(new AuctionBidsQuery(id, userId));
            return Ok(bids);
        }

        /// Actualiza los datos de una subasta (solo si aún no está cerrada ni tiene pujas).
        /// 200 OK: Devuelve la subasta actualizada.
        /// 400 Bad Request: Datos inválidos (fin antes del inicio, precios no positivos, incremento mayor al precio base o categoría inexistente).
        /// 404 Not Found: No existe la subasta.
        /// 409 Conflict: La subasta está finalizada/desierta o activa con pujas.
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(AuctionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Update(int id, UpdateAuctionCommand cmd)
        {
            cmd.Id = id;

            var result = await _updateHandler.Handle(cmd);
            return Ok(result);
        }


        /// Elimina una subasta.
        ///204 No Content: Subasta eliminada.
        ///404 Not Found: No existe la subasta.
      
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