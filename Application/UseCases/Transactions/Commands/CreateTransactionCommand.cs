using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.UseCases.Transactions.Commands
{
    public class CreateTransactionCommand
    {
        /// <summary>Id del usuario titular de la billetera.</summary>
        public int UserId { get; set; }

        /// <summary>Tipo de movimiento: DEPOSITO o RETIRO (manuales). La retenciÃ³n/liberaciÃ³n las genera solo el sistema.</summary>
        [Required(ErrorMessage = "El tipo es obligatorio")]
        public string Type { get; set; } = string.Empty;

        /// <summary>Monto del movimiento.</summary>
        [Required(ErrorMessage = "El monto es obligatorio")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0")]
        public decimal Amount { get; set; }

        /// <summary>Id de la subasta asociada (solo informativo; usado por movimientos del sistema).</summary>
        public int? AuctionId { get; set; }
    }
}
