using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Auction
    {
        public int Id { get; set; }
        public int SellerId { get; set; }

        public int CategoryId { get; set; }
        public String Title { get; set; }

        public String Descripcion   { get; set; }
        public String UrlImagen { get; set; }

        public decimal BasePrice { get; set; }

        public decimal MinIncrement { get; set; } //minimo incremento de subasta
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string Status { get; set; } 
        public int Version  { get; set; }
        public Auction() { 


        }




    }
}
