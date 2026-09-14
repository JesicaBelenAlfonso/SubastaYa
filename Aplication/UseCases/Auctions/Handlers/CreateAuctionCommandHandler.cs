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

        public CreateAuctionCommandHandler(IAuctionRepository auctions, IUnitOfWork uow)
        {
            _auctions = auctions;
            _uow = uow;
        }

        public async Task<AuctionResponseDto> Handle(CreateAuctionCommand cmd)
        {
            if (cmd.EndDate <= cmd.StartDate)
                throw new DomainException("La fecha de fin debe ser posterior a la fecha de inicio");

            if (cmd.BasePrice <= 0)
                throw new DomainException("El precio base debe ser mayor a 0");

            if (cmd.MinIncrement <= 0)
                throw new DomainException("El incremento mínimo debe ser mayor a 0");

            var auction = cmd.ToEntity(cmd.SellerId);

            await _auctions.AddAsync(auction);
            await _uow.SaveChangesAsync();

            return auction.ToDto();
        }
    }
}
