using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Auctions.Commands;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Auctions.Handlers
{
    public class CreateAuctionCommandHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly ICategoryRepository _categories;
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;

        public CreateAuctionCommandHandler(
            IAuctionRepository auctions,
            ICategoryRepository categories,
            IUnitOfWork uow,
            IAuditService audit)
        {
            _auctions = auctions;
            _categories = categories;
            _uow = uow;
            _audit = audit;
        }

        public async Task<AuctionResponseDto> Handle(CreateAuctionCommand cmd)
        {
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

            var auction = cmd.ToEntity(cmd.SellerId);

            await _auctions.AddAsync(auction);

            await _audit.LogAsync("Auction", auction.Id, AuditAction.CREATE, cmd.SellerId, new
            {
                auction.Title,
                auction.BasePrice,
                auction.CategoryId
            });

            await _uow.SaveChangesAsync();

            return auction.ToDto();
        }
    }
}
