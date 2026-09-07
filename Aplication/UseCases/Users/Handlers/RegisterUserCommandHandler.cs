using Domain.Entities;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Users.Commands;

using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Users.Handlers
{
    public class RegisterUserCommandHandler
    {
        private readonly IUserRepository _users;
        private readonly IWalletRepository _wallets;   // ← nueva dependencia
        private readonly IPasswordHasher _hasher;
        private readonly IUnitOfWork _uow;

        public RegisterUserCommandHandler(
            IUserRepository users,
            IWalletRepository wallets,              // ← se inyecta acá también
            IPasswordHasher hasher,
            IUnitOfWork uow)
        {
            _users = users;
            _wallets = wallets;
            _hasher = hasher;
            _uow = uow;
        }

        public async Task<UserResponseDto> Handle(RegisterUserCommand cmd)
        {
            var existente = await _users.GetByEmailAsync(cmd.Email);
            if (existente is not null)
                throw new DomainException("Ya existe un usuario con ese email");

            var passwordHash = _hasher.Hash(cmd.Password);

            var user = new User
            {
                Email = cmd.Email,
                Name = cmd.Name,
                PasswordHash = passwordHash,
                RegisteredAt = DateTime.UtcNow
            };

            await _users.AddAsync(user);

            // OJO ACÁ — esto es lo más importante de este paso.
            // Guardamos primero al User, para que la base le asigne un Id
            // (recién ahí existe user.Id, antes es 0).
            await _uow.SaveChangesAsync();

            // Recién con el Id ya generado, podemos crear el Wallet
            // apuntando al User correcto.
            var wallet = new Wallet
            {
                UserId = user.Id,
                TotalBalance = 0,
                HeldBalance = 0
            };

            await _wallets.AddAsync(wallet);
            await _uow.SaveChangesAsync();   // segunda confirmación, para el Wallet

            return user.ToDto();
        }
    }
}