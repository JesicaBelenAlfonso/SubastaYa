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
        public TransactionType Type { get; set; }
        public decimal Amount { get; set; }
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // AuctionId es opcional (ej.: recargas de saldo).
        public int? AuctionId { get; set; }

        public Transaction() { }
    }
}
