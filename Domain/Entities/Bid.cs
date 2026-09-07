using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Domain.Entities
{
    public class Bid//puja
    {
        public int Id { get; set; }
        public int BuyerId { get; set; }//comprador
        public int AuctionId { get; set; } //subasta
        public decimal Amount { get; set; } //monto de la puja
        public DateTime BidDate { get; set; } //fecha de la puja

        public Bid() { }
    }

}
