using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Users.Commands;
using SubastaYa.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SubastaYa.Application.UseCases.Users.Handlers
{
    public class UpdateUserCommandHandler
    {
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _uow;

        public UpdateUserCommandHandler(IUserRepository users, IUnitOfWork uow)
        {
            _users = users;
            _uow = uow;
        }

        public async Task<UserResponseDto> Handle(UpdateUserCommand cmd)
        {
            var user = await _users.GetByIdAsync(cmd.Id);

            if (user is null)
                throw new DomainException($"No existe un usuario con Id {cmd.Id}");

            user.Name = cmd.Name;             

            await _uow.SaveChangesAsync();    

            return user.ToDto();
        }
    }
 }
