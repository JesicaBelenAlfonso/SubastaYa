using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Transaccion
    {
        public int Id { get; set; }
        public int BilleteraId { get; set; }
        public string Tipo { get; set; }
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; }
        public int SubastaId { get; set; }

        public Transaccion() { }

    }
}
