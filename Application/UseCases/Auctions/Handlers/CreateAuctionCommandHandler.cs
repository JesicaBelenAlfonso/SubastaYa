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
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;

        public CreateAuctionCommandHandler(IAuctionRepository auctions, IUnitOfWork uow, IAuditService audit)
        {
            _auctions = auctions;
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

            var auction = cmd.ToEntity(cmd.SellerId);

            await _auctions.AddAsync(auction);

            await _audit.LogAsync("Auction", auction.Id, AuditActions.CREATE, cmd.SellerId, new
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
