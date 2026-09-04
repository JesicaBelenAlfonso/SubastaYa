using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Categoria
    {
        public int id { get; set; }
        public string nombre { get; set; }
        public string UrlIcono { get; set; }

        public Categoria() { }

    }
}
