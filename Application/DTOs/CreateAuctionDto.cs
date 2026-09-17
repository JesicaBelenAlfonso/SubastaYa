using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Application.DTOs
{
    public class CreateAuctionDto
    {
        public int CategoryId { get; set; }
        public string Title { get; set; } = null!;
        public string Descripcion { get; set; } = null!;
        public string UrlImagen { get; set; } = null!;   
        public decimal BasePrice { get; set; }
        public decimal MinIncrement { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
}
