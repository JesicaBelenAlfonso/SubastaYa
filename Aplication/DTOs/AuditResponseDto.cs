using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Application.DTOs
{
    public class AuditResponseDto
    {
        public int Id { get; set; }
        public string Entity { get; set; } = null!;        // Ejemplo: "Auction", "Bid", etc.
        public int EntityId { get; set; }       // ID del registro afectado
        public string Action { get; set; } = null!;        // Ejemplo: "CREATE", "UPDATE", "DELETE"
        public int UserId { get; set; }         // Quién hizo la acción
        public string DetalleJson { get; set; } = null!;   // El detalle en formato JSON
        public DateTime Date { get; set; }        // Cuándo ocurrió
    }
}
