using System.ComponentModel.DataAnnotations;
using SubastaYa.Application.DTOs;

namespace SubastaYa.Application.UseCases.Bids.Commands
{
    public class CreateBidCommand : CreateBidDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "El comprador es obligatorio")]
        public int BuyerId { get; set; }
    }
}
