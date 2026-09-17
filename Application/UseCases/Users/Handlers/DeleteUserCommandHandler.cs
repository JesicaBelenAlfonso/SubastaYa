using SubastaYa.Application.Interfaces;
using SubastaYa.Application.UseCases.Users.Commands;
using SubastaYa.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Users.Handlers
{
    public class DeleteUserCommandHandler
    {
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _uow;

        public DeleteUserCommandHandler(IUserRepository users, IUnitOfWork uow)
        {
            _users = users;
            _uow = uow;
        }

        public async Task Handle(DeleteUserCommand cmd)
        {
            var user = await _users.GetByIdAsync(cmd.Id);

            if (user is null)
                throw new DomainException($"No existe un usuario con Id {cmd.Id}");

            _users.Delete(user);
            await _uow.SaveChangesAsync();
        }
    }

}
