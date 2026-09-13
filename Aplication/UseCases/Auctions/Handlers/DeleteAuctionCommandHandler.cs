using SubastaYa.Application.Interfaces;
using SubastaYa.Application.UseCases.Auctions.Commands;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Auctions.Handlers
{
    public class DeleteAuctionCommandHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly IUnitOfWork _uow;

        public DeleteAuctionCommandHandler(IAuctionRepository auctions, IUnitOfWork uow)
        {
            _auctions = auctions;
            _uow = uow;
        }

        public async Task Handle(DeleteAuctionCommand cmd)
        {
            var auction = await _auctions.GetByIdAsync(cmd.Id);

            if (auction is null)
                throw new DomainException($"No existe una subasta con Id {cmd.Id}");

            _auctions.Delete(auction);
            await _uow.SaveChangesAsync();
        }
    }
}
