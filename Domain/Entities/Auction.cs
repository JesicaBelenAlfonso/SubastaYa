using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
namespace SubastaYa.Domain.Entities
{
    public class Auction
    {
        public int Id { get; set; }
        public int SellerId { get; set; }

        public int CategoryId { get; set; }
        public String Title { get; set; } = null!;

        public String Descripcion   { get; set; } = null!;
        public String UrlImagen { get; set; } = null!;

        public decimal BasePrice { get; set; }

        public decimal MinIncrement { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Toca la subasta en cada puja (marca de actividad + RowVersion).
        public DateTime? LastBidAt { get; set; }

        public AuctionStatus Status { get; set; }
        [Timestamp] public byte[] RowVersion { get; set; } = null!;

        // Nace ACTIVA si ya empezó; los estados terminales no se revierten.
        public void RefreshStatus(DateTime now)
        {
            if (Status == AuctionStatus.Proxima && StartDate <= now && EndDate > now)
                Status = AuctionStatus.Activa;
        }

        public bool IsTerminal => Status.IsTerminal();

        public Auction() { 


        }




    }
}
