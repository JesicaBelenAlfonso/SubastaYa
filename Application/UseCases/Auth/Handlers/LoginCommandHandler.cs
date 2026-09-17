using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Auth.Commands;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Auth.Handlers
{
    public class LoginCommandHandler
    {
        private readonly IUserRepository _users;
        private readonly IPasswordHasher _hasher;

        public LoginCommandHandler(IUserRepository users, IPasswordHasher hasher)
        {
            _users = users;
            _hasher = hasher;
        }

        public async Task<UserResponseDto> Handle(LoginCommand cmd)
        {
            var user = await _users.GetByEmailAsync(cmd.Email);

            // Mismo mensaje para no filtrar qué emails están registrados.
            if (user is null || !_hasher.Verify(cmd.Password, user.PasswordHash))
                throw new InvalidCredentialsException("Credenciales inválidas");

            return user.ToDto();
        }
    }
}