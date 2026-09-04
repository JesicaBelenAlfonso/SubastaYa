using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public string Tipo { get; set; } = string.Empty; // Ej: DEPOSITO, RETENCION, LIBERACION, PAGO, COBRO
        public decimal Monto { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        //no todas las transacciones están atadas directamente a una subasta
        //(por ejemplo, las recargas de saldo iniciales en la billetera).

        public int? SubastaId { get; set; }

        public Transaction() { }
    }
}
