using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Users.Commands;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Users.Handlers
{
    public class RegisterUserCommandHandler
    {
        private readonly IUserRepository _users;
        private readonly IWalletRepository _wallets;
        private readonly IPasswordHasher _hasher;
        private readonly IUnitOfWork _uow;

        public RegisterUserCommandHandler(
            IUserRepository users,
            IWalletRepository wallets,
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
                throw new DomainConflictException("Ya existe un usuario con ese email");
            var passwordHash = _hasher.Hash(cmd.Password);

            var user = new User
            {
                Email = cmd.Email,
                Name = cmd.Name,
                PasswordHash = passwordHash,
                RegisteredAt = DateTime.UtcNow
            };

            await _users.AddAsync(user);

            var wallet = new Wallet
            {
                TotalBalance = 0,
                HeldBalance = 0,
                User = user
            };

            await _wallets.AddAsync(wallet);

            await _uow.SaveChangesAsync();

            return user.ToDto();
        }
    }
}