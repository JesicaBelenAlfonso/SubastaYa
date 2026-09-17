using SubastaYa.Application.DTOs;

namespace SubastaYa.Application.UseCases.Auctions.Commands
{
    public class UpdateAuctionCommand : CreateAuctionDto
    {
        public int Id { get; set; }
        public byte[]? RowVersion { get; set; }
    }
}
