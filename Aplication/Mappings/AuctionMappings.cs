using Domain.Entities;
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
                Status = "Pending",
                Version = 0
            };
        }

        public static AuctionResponseDto ToDto(this Auction auction)
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
                Status = auction.Status
            };
        }
    }
}
