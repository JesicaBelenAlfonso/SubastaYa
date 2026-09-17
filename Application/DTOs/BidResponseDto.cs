using System;

namespace SubastaYa.Application.DTOs
{
    public class BidResponseDto
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }
        public int AuctionId { get; set; }
        public decimal Amount { get; set; }
        public DateTime BidDate { get; set; }

        /// True si la puja disparó la extensión anti-sniping del cierre.
        public bool SeExtendio { get; set; }

        /// Nueva fecha de cierre cuando la puja extendió la subasta; null si no hubo extensión.
        public DateTime? NuevaFechaFin { get; set; }
    }
}
