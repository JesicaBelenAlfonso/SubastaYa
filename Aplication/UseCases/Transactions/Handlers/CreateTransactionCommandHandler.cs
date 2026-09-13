using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Transactions.Commands;
using Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Transactions.Handlers
{
    public class CreateTransactionCommandHandler
    {
        private readonly IWalletRepository _wallets;
        private readonly ITransactionRepository _transactions;
        private readonly IUnitOfWork _uow;

        public CreateTransactionCommandHandler(
            IWalletRepository wallets,
            ITransactionRepository transactions,
            IUnitOfWork uow)
        {
            _wallets = wallets;
            _transactions = transactions;
            _uow = uow;
        }

        public async Task<WalletResponseDto> Handle(CreateTransactionCommand cmd)
        {
            var wallet = await _wallets.GetByUserIdAsync(cmd.UserId)
                ?? throw new DomainException("El usuario no tiene una billetera asociada");

            ApplyMovement(wallet, cmd);

            // El ledger es append-only: cada movimiento se registra como una
            // Transaction nueva, nunca se modifica ni se borra.
            await _transactions.AddAsync(new Transaction
            {
                WalletId = wallet.Id,
                Type = cmd.Type,
                Amount = cmd.Amount,
                AuctionId = cmd.AuctionId,
                Date = DateTime.UtcNow
            });

            // Un solo SaveChanges: o se aplica el movimiento y se registra
            // la Transaction, o no pasa nada (atómico).
            await _uow.SaveChangesAsync();

            return wallet.ToDto();
        }

        private static void ApplyMovement(Wallet wallet, CreateTransactionCommand cmd)
        {
            switch (cmd.Type)
            {
                case "DEPOSITO":
                    wallet.TotalBalance += cmd.Amount;
                    break;

                case "RETIRO":
                    if (cmd.Amount > wallet.AvailableBalance)
                        throw new DomainException("Saldo disponible insuficiente para retirar");
                    wallet.TotalBalance -= cmd.Amount;
                    break;

                case "RETENCION":
                    if (cmd.Amount > wallet.AvailableBalance)
                        throw new DomainException("Saldo disponible insuficiente para congelar");
                    wallet.HeldBalance += cmd.Amount;
                    break;

                case "LIBERACION":
                    if (cmd.Amount > wallet.HeldBalance)
                        throw new DomainException("El monto congelado no alcanza para liberar");
                    wallet.HeldBalance -= cmd.Amount;
                    break;

                default:
                    throw new DomainException("Tipo de movimiento no válido");
            }
        }
    }
}