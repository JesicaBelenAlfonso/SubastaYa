using System.Collections.Generic;

namespace SubastaYa.Application.DTOs
{

    /// Envelope del catálogo de subastas con su metadata de paginación.

    public class PagedAuctionsResponseDto
    {
        public List<AuctionResponseDto> Items { get; set; } = new();
        public int Pagina { get; set; }
        public int Tamano { get; set; }
        public int Total { get; set; }
        public int TotalPaginas { get; set; }
    }
}
