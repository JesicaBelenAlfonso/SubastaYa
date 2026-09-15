using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.DTOs
{
    public class CreateBidDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "La subasta es obligatoria")]
        public int AuctionId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }
    }
}
