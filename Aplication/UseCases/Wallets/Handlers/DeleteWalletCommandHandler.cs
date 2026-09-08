using System;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.UseCases.Wallets.Commands;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Wallets.Handlers
{
    public class DeleteWalletCommandHandler
    {
        private readonly IWalletRepository _wallets;
        private readonly IUnitOfWork _uow;

        public DeleteWalletCommandHandler(IWalletRepository wallets, IUnitOfWork uow)
        {
            _wallets = wallets;
            _uow = uow;
        }

        public async Task Handle(DeleteWalletCommand cmd)
        {
            var wallet = await _wallets.GetByUserIdAsync(cmd.UserId)
                ?? throw new DomainException("El usuario no tiene una billetera asociada");

            _wallets.Delete(wallet);
            await _uow.SaveChangesAsync();
        }
    }
}