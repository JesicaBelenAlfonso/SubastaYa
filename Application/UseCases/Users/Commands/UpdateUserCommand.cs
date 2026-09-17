using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Users.Commands
{
    public class UpdateUserCommand
    {
        public int Id { get; set; }             

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        public string Name { get; set; } = string.Empty;
    }
}
