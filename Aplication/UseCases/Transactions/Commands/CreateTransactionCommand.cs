using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.UseCases.Transactions.Commands
{
    public class CreateTransactionCommand
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "El tipo es obligatorio")]
        public string Type { get; set; } = string.Empty;

        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        public int? AuctionId { get; set; }
    }
}