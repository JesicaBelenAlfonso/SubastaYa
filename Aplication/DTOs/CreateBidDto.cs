using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Application.DTOs
{
    public class CreateBidDto
    {
        public int AuctionId { get; set; }
        public decimal Amount { get; set; }
    }
}
