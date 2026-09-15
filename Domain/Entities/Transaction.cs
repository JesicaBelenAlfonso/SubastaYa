using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public string Type { get; set; } = string.Empty; // Ej: DEPOSITO, RETENCION, LIBERACION, PAGO, COBRO
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        //no todas las transacciones están atadas directamente a una subasta
        //(por ejemplo, las recargas de saldo iniciales en la billetera).

        public int? AuctionId { get; set; }

        public Transaction() { }
    }
}
