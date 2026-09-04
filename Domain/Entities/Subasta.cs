using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Subasta
    {
        public int Id { get; set; }
        public int VendedorId { get; set; }

        public int CategoriaId { get; set; }
        public String Titulo { get; set; }

        public String Descripcion   { get; set; }
        public String UrlImagen { get; set; }

        public decimal PrecioBase { get; set; }

        public decimal IncrementoMinimo { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        public string Estado { get; set; } 
        public int Version  { get; set; }
        public Subasta() { 


        }




    }
}
