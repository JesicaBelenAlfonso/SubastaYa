using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SubastaYa.Application.DTOs
{
    public class WalletResponseDto
    {
        public int Id { get; set; }
        public decimal TotalBalance { get; set; }
        public decimal HeldBalance { get; set; }
        public decimal AvailableBalance { get; set; }
    }
}