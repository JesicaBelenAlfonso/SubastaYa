using SubastaYa.Domain.Entities;

namespace SubastaYa.Application.DTOs
{

    /// Subasta junto con los agregados de sus pujas (oferta actual y cantidad).

    public class AuctionCatalogItem
    {
        public Auction Auction { get; set; } = null!;
        public decimal? OfertaActual { get; set; }
        public int CantidadPujas { get; set; }
    }
}
