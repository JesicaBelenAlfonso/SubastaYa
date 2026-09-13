using System;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.UseCases.Wallets.Commands;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Wallets.Handlers
{
    public class DeleteWalletCommandHandler
    {
        private readonly IWalletRepository _wallets;
        private readonly ITransactionRepository _transactions;
        private readonly IUnitOfWork _uow;

        public DeleteWalletCommandHandler(IWalletRepository wallets, ITransactionRepository transactions, IUnitOfWork uow)
        {
            _wallets = wallets;
            _transactions = transactions;
            _uow = uow;
        }

        public async Task Handle(DeleteWalletCommand cmd)
        {
            var wallet = await _wallets.GetByUserIdAsync(cmd.UserId)
                ?? throw new DomainException("El usuario no tiene una billetera asociada");

            // El ledger es inmutable: si la billetera tiene movimientos,
            // no se puede borrar. El Restrict de la FK lo forzaría igual,
            // pero acá lo validamos en dominio para devolver un mensaje claro.
            if (await _transactions.ExistsByWalletIdAsync(wallet.Id))
                throw new DomainException("No se puede eliminar una billetera con movimientos");

            _wallets.Delete(wallet);
            await _uow.SaveChangesAsync();
        }
    }
}