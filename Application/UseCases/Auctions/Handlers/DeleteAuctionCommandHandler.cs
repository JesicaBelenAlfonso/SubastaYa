using SubastaYa.Application.Interfaces;
using SubastaYa.Application.UseCases.Auctions.Commands;
using SubastaYa.Domain.Exceptions;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Auctions.Handlers
{
    public class DeleteAuctionCommandHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;

        public DeleteAuctionCommandHandler(IAuctionRepository auctions, IUnitOfWork uow, IAuditService audit)
        {
            _auctions = auctions;
            _uow = uow;
            _audit = audit;
        }

        public async Task Handle(DeleteAuctionCommand cmd)
        {
            var auction = await _auctions.GetByIdAsync(cmd.Id);

            if (auction is null)
                throw new DomainException($"No existe una subasta con Id {cmd.Id}");

            await _audit.LogAsync("Auction", auction.Id, "DELETE", auction.SellerId, new
            {
                auction.Title
            });

            _auctions.Delete(auction);
            await _uow.SaveChangesAsync();
        }
    }
}
