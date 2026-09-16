using SubastaYa.Application.Interfaces;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Services
{
    public class AuctionFinalizationService : IAuctionFinalizationService
    {
        private readonly IAuctionRepository _auctions;
        private readonly IBidRepository _bids;
        private readonly IWalletRepository _wallets;
        private readonly ITransactionRepository _transactions;
        private readonly IAuditService _audit;
        private readonly IUnitOfWork _uow;

        public AuctionFinalizationService(
            IAuctionRepository auctions,
            IBidRepository bids,
            IWalletRepository wallets,
            ITransactionRepository transactions,
            IAuditService audit,
            IUnitOfWork uow)
        {
            _auctions = auctions;
            _bids = bids;
            _wallets = wallets;
            _transactions = transactions;
            _audit = audit;
            _uow = uow;
        }

        public async Task<int> FinalizeExpiredAuctionsAsync(CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;

            // Idempotencia: solo trae subastas ACTIVA ya vencidas.
            var expired = (await _auctions.GetExpiredActiveAsync(now)).ToList();
            if (expired.Count == 0)
                return 0;

            foreach (var auction in expired)
            {
                var winner = await _bids.GetHighestBidByAuctionIdAsync(auction.Id);

                if (winner is null)
                {
                    auction.Status = AuctionStatus.Desierta;
                    await _audit.LogAsync("Auction", auction.Id, AuditAction.AUCTION_STATUS_CHANGED, 0, new
                    {
                        from = AuctionStatus.Activa.ToString().ToUpperInvariant(),
                        to = AuctionStatus.Desierta.ToString().ToUpperInvariant(),
                        reason = "worker: sin pujas"
                    });
                    continue;
                }

                var buyerWallet = await _wallets.GetByUserIdAsync(winner.BuyerId);
                var sellerWallet = await _wallets.GetByUserIdAsync(auction.SellerId);

                if (buyerWallet is null || sellerWallet is null)
                {
                    // Sin billeteras no se puede liquidar.
                    continue;
                }

                // Solo se liquida si el ganador tiene el monto retenido.
                if (buyerWallet.HeldBalance >= winner.Amount)
                {
                    buyerWallet.HeldBalance -= winner.Amount;   // libera la retención
                    buyerWallet.TotalBalance -= winner.Amount;  // pago del comprador
                    sellerWallet.TotalBalance += winner.Amount; // cobro del vendedor

                    await _transactions.AddAsync(new Transaction
                    {
                        WalletId = buyerWallet.Id,
                        Type = TransactionType.Pago,
                        Amount = winner.Amount,
                        AuctionId = auction.Id,
                        Date = now
                    });

                    await _transactions.AddAsync(new Transaction
                    {
                        WalletId = sellerWallet.Id,
                        Type = TransactionType.Cobro,
                        Amount = winner.Amount,
                        AuctionId = auction.Id,
                        Date = now
                    });
                }

                auction.Status = AuctionStatus.Finalizada;

                await _audit.LogAsync("Auction", auction.Id, AuditAction.AUCTION_STATUS_CHANGED, 0, new
                {
                    from = AuctionStatus.Activa.ToString().ToUpperInvariant(),
                    to = AuctionStatus.Finalizada.ToString().ToUpperInvariant(),
                    winnerId = winner.BuyerId,
                    winningAmount = winner.Amount,
                    reason = "worker: liquidación"
                });
            }

            // Un único SaveChanges: la liquidación del lote es atómica.
            return await _uow.SaveChangesAsync(ct);
        }
    }
}
