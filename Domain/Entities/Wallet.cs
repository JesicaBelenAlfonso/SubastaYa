using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public decimal SaldoTotal { get; set; }
        public decimal SaldoRetenido { get; set; }
        //Esto asegura que el sistema calcule el saldo
        //utilizable en tiempo real restando el dinero congelado en garantía
        public decimal SaldoDisponible => SaldoTotal - SaldoRetenido;
        //Esto le indica a Entity Framework Core y SQL Server que
        //utilicen este campo para el control de concurrencia optimista
        //(Optimistic Locking)
        [Timestamp]
        public byte[] RowVersion { get; set; } = null!;

        public Wallet() { }
    }
}
