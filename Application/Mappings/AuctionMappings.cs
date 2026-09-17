using SubastaYa.Domain.Entities;
using SubastaYa.Application.DTOs;


namespace SubastaYa.Application.Mappings

{
    public static class AuctionMappings
    {
        public static Auction ToEntity(this CreateAuctionDto dto, int sellerId)
        {
            return new Auction
            {
                SellerId = sellerId,
                CategoryId = dto.CategoryId,
                Title = dto.Title,
                Descripcion = dto.Descripcion,
                UrlImagen = dto.UrlImagen,
                BasePrice = dto.BasePrice,
                MinIncrement = dto.MinIncrement,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                // La subasta arranca ACTIVA si ya empezó; si no, PROXIMA.
                Status = dto.StartDate <= DateTime.UtcNow
                    ? AuctionStatus.Activa
                    : AuctionStatus.Proxima,
            };
        }

        public static AuctionResponseDto ToDto(
            this Auction auction,
            string? categoria = null,
            decimal? ofertaActual = null,
            int cantidadPujas = 0)
        {
            return new AuctionResponseDto
            {
                Id = auction.Id,
                SellerId = auction.SellerId,
                CategoryId = auction.CategoryId,
                Title = auction.Title,
                Descripcion = auction.Descripcion,
                UrlImagen = auction.UrlImagen,
                BasePrice = auction.BasePrice,
                MinIncrement = auction.MinIncrement,
                StartDate = auction.StartDate,
                EndDate = auction.EndDate,
                Status = auction.Status.ToString().ToUpperInvariant(),
                Categoria = categoria ?? "General",
                OfertaActual = ofertaActual,
                CantidadPujas = cantidadPujas,
                RowVersion = auction.RowVersion
            };
        }
    }
}
