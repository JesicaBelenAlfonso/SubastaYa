using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace SubastaYa.Domain.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // FK resuelta por EF Core en un SaveChanges único.
        public User User { get; set; } = null!;

        public decimal TotalBalance { get; set; }
        public decimal HeldBalance { get; set; }

        // Saldo utilizable = total menos lo congelado en garantía.
        [NotMapped]
        public decimal AvailableBalance => TotalBalance - HeldBalance;

        // Control de concurrencia optimista.
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public Wallet() { }
    }

}
