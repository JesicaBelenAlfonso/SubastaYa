using System;
using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.DTOs
{
    public class CreateAuctionDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "La categoría es obligatoria")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        public string Title { get; set; } = null!;

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string Descripcion { get; set; } = null!;

        [Required(ErrorMessage = "La imagen es obligatoria")]
        public string UrlImagen { get; set; } = null!;

        public decimal BasePrice { get; set; }

        public decimal MinIncrement { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }
    }
}
