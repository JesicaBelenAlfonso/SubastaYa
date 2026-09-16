using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Users.Queries;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Users.Handlers
{
    /// <summary>
    /// Actividades del usuario: subastas que publicó (VENDEDOR) y subastas donde pujó (PUJADOR,
    /// marcando si su puja lidera). Los estados se reflejan al instante igual que en el catálogo.
    /// </summary>
    public class GetUserActivitiesQueryHandler
    {
        private readonly IUserRepository _users;
        private readonly IAuctionRepository _auctions;
        private readonly ICategoryRepository _categories;
        private readonly IBidRepository _bids;

        public GetUserActivitiesQueryHandler(
            IUserRepository users,
            IAuctionRepository auctions,
            ICategoryRepository categories,
            IBidRepository bids)
        {
            _users = users;
            _auctions = auctions;
            _categories = categories;
            _bids = bids;
        }

        public async Task<List<UserActivityResponseDto>> Handle(GetUserActivitiesQuery query)
        {
            if (await _users.GetByIdAsync(query.UserId) is null)
                throw new NotFoundException($"No existe un usuario con Id {query.UserId}");

            var auctions = await _auctions.GetAllAsync();
            var categorias = (await _categories.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
            var now = DateTime.UtcNow;

            var result = new List<UserActivityResponseDto>();
            foreach (var auction in auctions)
            {
                auction.RefreshStatus(now);

                var esVendedor = auction.SellerId == query.UserId;
                var participo = await _bids.HasBidAsync(auction.Id, query.UserId);
                if (!esVendedor && !participo)
                    continue;

                var lider = await _bids.GetHighestBidByAuctionIdAsync(auction.Id);

                var dto = auction.ToDto(
                    categorias.TryGetValue(auction.CategoryId, out var nombre) ? nombre : null,
                    await _bids.GetHighestAmountByAuctionIdAsync(auction.Id),
                    await _bids.GetCountByAuctionIdAsync(auction.Id));

                if (esVendedor)
                    result.Add(new UserActivityResponseDto { Role = "VENDEDOR", Liderando = false, Auction = dto });

                if (participo)
                    result.Add(new UserActivityResponseDto
                    {
                        Role = "PUJADOR",
                        Liderando = lider is not null && lider.BuyerId == query.UserId,
                        Auction = dto
                    });
            }

            return result.OrderByDescending(a => a.Auction.StartDate).ToList();
        }
    }
}