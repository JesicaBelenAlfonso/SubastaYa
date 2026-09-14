using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Application.DTOs
{
    public class AuctionResponseDto
    {
        public int Id { get; set; }
        public int SellerId { get; set; }
        public int CategoryId { get; set; }
        public string Title { get; set; }
        public string Descripcion { get; set; }
        public string UrlImagen { get; set; }
        public decimal BasePrice { get; set; }
        public decimal MinIncrement { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }

       public byte[]? RowVersion { get; set; }
    }
}
