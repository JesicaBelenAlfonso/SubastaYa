using SubastaYa.Application.DTOs;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.Interfaces
{
    public interface IAuctionRepository
    {
        Task AddAsync(Auction auction);
        Task<IEnumerable<Auction>> GetAllAsync();
        Task<IEnumerable<Auction>> GetAuctionsForUserAsync(int userId);
        Task<Auction?> GetByIdAsync(int id);
        void Delete(Auction auction);
        Task<IEnumerable<Auction>> GetExpiredActiveAsync(DateTime now);

        Task<IEnumerable<AuctionCatalogItem>> GetCatalogAsync(
            AuctionStatus? estado,
            int? categoriaId,
            decimal? minPrecio,
            decimal? maxPrecio,
            string orden,
            int pagina,
            int tamano);

        Task<int> GetCatalogCountAsync(
            AuctionStatus? estado,
            int? categoriaId,
            decimal? minPrecio,
            decimal? maxPrecio);
    }
}
