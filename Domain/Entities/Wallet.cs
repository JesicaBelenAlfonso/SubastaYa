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

        // Navigation property: permite que EF Core resuelva la FK durante
        // un SaveChanges único (insert de User + Wallet atómico).
        public User User { get; set; } = null!;

        public decimal TotalBalance { get; set; }
        public decimal HeldBalance { get; set; }

        // Esto asegura que el sistema calcule el saldo
        // utilizable en tiempo real restando el dinero congelado en garantía
        [NotMapped]
        public decimal AvailableBalance => TotalBalance - HeldBalance;

        // Esto le indica a Entity Framework Core y SQL Server que
        // utilicen este campo para el control de concurrencia optimista
        // (Optimistic Locking)
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public Wallet() { }
    }

}
