using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Users.Commands
{
    public class DeleteUserCommand
    {
        public int Id { get; set; }

        public DeleteUserCommand(int id)
        {
            Id = id;
        }
    }
}
