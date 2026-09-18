using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Users.Queries;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Users.Handlers
{
    /// Actividades del usuario: subastas que publicó (VENDEDOR) y subastas donde pujó (PUJADOR,
    /// marcando si su puja lidera y el resultado de su puja). Los estados se reflejan al instante
    /// igual que en el catálogo.
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

            // Una sola consulta: subastas del vendedor o donde el usuario pujó (sin recorrer el catálogo).
            var auctions = await _auctions.GetAuctionsForUserAsync(query.UserId);
            var categorias = (await _categories.GetAllAsync()).ToDictionary(c => c.Id, c => c.Name);
            var now = DateTime.UtcNow;

            var result = new List<UserActivityResponseDto>();
            foreach (var auction in auctions)
            {
                auction.RefreshStatus(now);

                var esVendedor = auction.SellerId == query.UserId;
                var lider = await _bids.GetHighestBidByAuctionIdAsync(auction.Id);
                var resultado = ComputeResultadoPuja(auction, query.UserId, esVendedor, lider);

                var dto = auction.ToDto(
                    categorias.TryGetValue(auction.CategoryId, out var nombre) ? nombre : null,
                    lider?.Amount,
                    await _bids.GetCountByAuctionIdAsync(auction.Id));

                if (esVendedor)
                {
                    result.Add(new UserActivityResponseDto
                    {
                        Role = "VENDEDOR",
                        Liderando = false,
                        ResultadoPuja = resultado,
                        Auction = dto
                    });
                }
                else
                {
                    result.Add(new UserActivityResponseDto
                    {
                        Role = "PUJADOR",
                        Liderando = lider is not null && lider.BuyerId == query.UserId,
                        ResultadoPuja = resultado,
                        Auction = dto
                    });
                }
            }

            return result.OrderByDescending(a => a.Auction.StartDate).ToList();
        }

        // GANADA: terminó y el usuario se quedó con la subasta. SUPERADA: terminó y no la ganó.
        // EN_CURSO: todavía no terminó. DESIERTA: terminó sin pujas.
        // El estado efectivo compensa al worker (que corre cada ~30 s): si la subasta
        // ya venció pero sigue ACTIVA en la base, se trata como terminada.
        private static string ComputeResultadoPuja(Auction auction, int userId, bool esVendedor, Bid? lider)
        {
            var vencida = auction.Status == AuctionStatus.Activa && auction.EndDate <= DateTime.UtcNow;

            if (auction.Status == AuctionStatus.Desierta)
                return "DESIERTA";

            if (auction.Status == AuctionStatus.Finalizada || vencida)
            {
                if (lider is null)
                    return "DESIERTA";

                var gano = esVendedor || lider.BuyerId == userId;
                return gano ? "GANADA" : "SUPERADA";
            }

            return "EN_CURSO";
        }
    }
}
