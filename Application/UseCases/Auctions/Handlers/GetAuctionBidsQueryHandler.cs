using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.UseCases.Auctions.Queries;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Auctions.Handlers
{
    /// <summary>
    /// Historial de pujas de una subasta, anonimizado. El líder se calcula con el
    /// monto más alto (la última puja que manda). No se expone ninguna identidad real.
    /// </summary>
    public class GetAuctionBidsQueryHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly IBidRepository _bids;

        public GetAuctionBidsQueryHandler(IAuctionRepository auctions, IBidRepository bids)
        {
            _auctions = auctions;
            _bids = bids;
        }

        public async Task<List<BidHistoryResponseDto>> Handle(AuctionBidsQuery query)
        {
            var auction = await _auctions.GetByIdAsync(query.AuctionId);
            if (auction is null)
                throw new NotFoundException($"No existe una subasta con Id {query.AuctionId}");

            var bids = (await _bids.GetByAuctionIdAsync(query.AuctionId)).ToList();
            var leader = await _bids.GetHighestBidByAuctionIdAsync(query.AuctionId);

            // Rótulo anónimo: "Pujador N", asignado por orden de primera aparición.
            var labels = new Dictionary<int, string>();
            foreach (var bid in bids)
            {
                if (!labels.ContainsKey(bid.BuyerId))
                    labels[bid.BuyerId] = $"Pujador {labels.Count + 1}";
            }

            return bids
                .OrderByDescending(b => b.BidDate)
                .Select(b => new BidHistoryResponseDto
                {
                    Id = b.Id,
                    Amount = b.Amount,
                    BidDate = b.BidDate,
                    BuyerLabel = labels.TryGetValue(b.BuyerId, out var label) ? label : $"Pujador {b.Id}",
                    IsMine = query.UserId.HasValue && b.BuyerId == query.UserId.Value,
                    IsLeader = leader is not null && b.Id == leader.Id
                })
                .ToList();
        }
    }
}