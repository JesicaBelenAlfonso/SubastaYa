using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Auctions.Commands;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Auctions.Handlers
{
    public class UpdateAuctionCommandHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly IUnitOfWork _uow;

        public UpdateAuctionCommandHandler(IAuctionRepository auctions, IUnitOfWork uow)
        {
            _auctions = auctions;
            _uow = uow;
        }

        public async Task<AuctionResponseDto> Handle(UpdateAuctionCommand cmd)
        {
            var auction = await _auctions.GetByIdAsync(cmd.Id);

            if (auction is null)
                throw new DomainException($"No existe una subasta con Id {cmd.Id}");

            if (cmd.EndDate <= cmd.StartDate)
                throw new DomainException("La fecha de fin debe ser posterior a la fecha de inicio");

            auction.CategoryId = cmd.CategoryId;
            auction.Title = cmd.Title;
            auction.Descripcion = cmd.Descripcion;
            auction.UrlImagen = cmd.UrlImagen;
            auction.BasePrice = cmd.BasePrice;
            auction.MinIncrement = cmd.MinIncrement;
            auction.StartDate = cmd.StartDate;
            auction.EndDate = cmd.EndDate;

            await _uow.SaveChangesAsync();

            return auction.ToDto();
        }
    }
}
