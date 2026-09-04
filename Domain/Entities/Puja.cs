using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Puja
    {
        public int Id { get; set; }
        public int CompradorId { get; set; }
        public int SubastaId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPuja { get; set; }

        public Puja() { }
    }

}
