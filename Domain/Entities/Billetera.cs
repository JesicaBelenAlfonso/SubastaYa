using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Billetera
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public decimal SaldoTotal { get; set; }
        public decimal SaldoRetenido { get; set; }
        public decimal SaldoDisponible { get; set; }
        public int Version { get; set; } 

        public Billetera() { }

    }
}
