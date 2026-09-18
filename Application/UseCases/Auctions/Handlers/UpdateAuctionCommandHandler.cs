using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Auctions.Commands;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Auctions.Handlers
{
    public class UpdateAuctionCommandHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly ICategoryRepository _categories;
        private readonly IBidRepository _bids;
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;

        public UpdateAuctionCommandHandler(
            IAuctionRepository auctions,
            ICategoryRepository categories,
            IBidRepository bids,
            IUnitOfWork uow,
            IAuditService audit)
        {
            _auctions = auctions;
            _categories = categories;
            _bids = bids;
            _uow = uow;
            _audit = audit;
        }

        public async Task<AuctionResponseDto> Handle(UpdateAuctionCommand cmd)
        {
            var auction = await _auctions.GetByIdAsync(cmd.Id);

            if (auction is null)
                throw new NotFoundException($"No existe una subasta con Id {cmd.Id}");

            // Refleja la activación automática antes de decidir si es editable.
            auction.RefreshStatus(DateTime.UtcNow);

            // Las subastas cerradas no se editan.
            if (auction.IsTerminal)
                throw new DomainConflictException("No se puede modificar una subasta finalizada o desierta");

            // Una subasta activa que ya recibió pujas está en curso: no se toca.
            if (auction.Status == AuctionStatus.Activa && await _bids.GetCountByAuctionIdAsync(auction.Id) > 0)
                throw new DomainConflictException("No se puede modificar una subasta activa que ya tiene pujas");

            // Mismas validaciones que el alta de subasta.
            if (cmd.EndDate <= cmd.StartDate)
                throw new DomainException("La fecha de fin debe ser posterior a la fecha de inicio");

            if (cmd.BasePrice <= 0)
                throw new DomainException("El precio base debe ser mayor a 0");

            if (cmd.MinIncrement <= 0)
                throw new DomainException("El incremento mínimo debe ser mayor a 0");

            if (cmd.MinIncrement > cmd.BasePrice)
                throw new DomainException("El incremento mínimo no puede ser mayor al precio base");

            // Evita el 500 por FK: la categoría debe existir.
            if (await _categories.GetByIdAsync(cmd.CategoryId) is null)
                throw new DomainException($"No existe una categoría con Id {cmd.CategoryId}");

            auction.CategoryId = cmd.CategoryId;
            auction.Title = cmd.Title;
            auction.Descripcion = cmd.Descripcion;
            auction.UrlImagen = cmd.UrlImagen;
            auction.BasePrice = cmd.BasePrice;
            auction.MinIncrement = cmd.MinIncrement;
            auction.StartDate = cmd.StartDate;
            auction.EndDate = cmd.EndDate;

            if (cmd.RowVersion is not null)
                _uow.SetOriginalValue(auction, "RowVersion", cmd.RowVersion);

            await _audit.LogAsync("Auction", auction.Id, AuditAction.UPDATE, auction.SellerId, new
            {
                auction.Title,
                auction.BasePrice,
                auction.MinIncrement,
                auction.CategoryId,
                auction.StartDate,
                auction.EndDate
            });

            await _uow.SaveChangesAsync();

            return auction.ToDto();
        }
    }
}
