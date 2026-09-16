using SubastaYa.Application.Interfaces;

namespace SubastaYa.Application.Services
{
    public class AuctionFinalizationService : IAuctionFinalizationService
    {
        private readonly IAuctionRepository _auctions;
        private readonly IAuditService _audit;
        private readonly IUnitOfWork _uow;

        public AuctionFinalizationService(IAuctionRepository auctions, IAuditService audit, IUnitOfWork uow)
        {
            _auctions = auctions;
            _audit = audit;
            _uow = uow;
        }

        public async Task<int> FinalizeExpiredAuctionsAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var expired = (await _auctions.GetExpiredActiveAsync(now)).ToList();

            foreach (var auction in expired)
            {
                auction.Status = "FINALIZADA";

                await _audit.LogAsync("Auction", auction.Id, AuditActions.AUCTION_STATUS_CHANGED, 0, new
                {
                    from = "ACTIVA",
                    to = "FINALIZADA",
                    reason = "worker"
                });
            }

            if (expired.Count == 0)
                return 0;

            return await _uow.SaveChangesAsync(ct);
        }
    }
}