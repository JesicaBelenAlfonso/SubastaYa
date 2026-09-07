using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Audit//Auditoria
    {
        public int Id { get; set; }
        public string Entity { get; set; }
        public int EntityId { get; set; }
        public string Action { get; set; }
        public int UserId { get; set; }
        public string DetalleJson  { get; set; }
        public DateTime Date { get; set; } 

        public Audit() { }

    }
}
