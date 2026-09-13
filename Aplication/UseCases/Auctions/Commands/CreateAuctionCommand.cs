using System.ComponentModel.DataAnnotations;
using SubastaYa.Application.DTOs;

namespace SubastaYa.Application.UseCases.Auctions.Commands
{
    public class CreateAuctionCommand : CreateAuctionDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El vendedor es obligatorio")]
        public int SellerId { get; set; }
    }
}
