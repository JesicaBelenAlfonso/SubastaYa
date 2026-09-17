using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.DTOs
{
    public class CreateCategoryDto
    {
        [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
        [MaxLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "El icono de la categoría es obligatorio")]
        [MaxLength(500, ErrorMessage = "La URL del icono no puede superar los 500 caracteres")]
        public string UrlIcono { get; set; } = null!;
    }
}