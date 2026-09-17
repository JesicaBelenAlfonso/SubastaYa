using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Transactions.Commands;
using SubastaYa.Domain.Entities;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Transactions.Handlers
{
    public class CreateTransactionCommandHandler
    {
        private readonly IWalletRepository _wallets;
        private readonly ITransactionRepository _transactions;
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;

        public CreateTransactionCommandHandler(
            IWalletRepository wallets,
            ITransactionRepository transactions,
            IUnitOfWork uow,
            IAuditService audit)
        {
            _wallets = wallets;
            _transactions = transactions;
            _uow = uow;
            _audit = audit;
        }

        public async Task<TransactionResponseDto> Handle(CreateTransactionCommand cmd)
        {
            var wallet = await _wallets.GetByUserIdAsync(cmd.UserId)
                ?? throw new NotFoundException("El usuario no tiene una billetera asociada");

            if (!Enum.TryParse<TransactionType>(cmd.Type, true, out var tipo))
                throw new DomainException("Tipo de movimiento no válido");

            ApplyMovement(wallet, tipo, cmd.Amount);

            if (tipo == TransactionType.Deposito)
            {
                await _audit.LogAsync("Wallet", wallet.Id, AuditAction.WALLET_MANUAL_CREDIT, cmd.UserId, new
                {
                    walletId = wallet.Id,
                    amount = cmd.Amount,
                    newTotalBalance = wallet.TotalBalance
                });
            }
            else if (tipo == TransactionType.Retiro)
            {
                await _audit.LogAsync("Wallet", wallet.Id, AuditAction.WALLET_WITHDRAWAL, cmd.UserId, new
                {
                    walletId = wallet.Id,
                    amount = cmd.Amount,
                    newTotalBalance = wallet.TotalBalance
                });
            }

            // Ledger append-only: cada movimiento es una Transaction nueva.
            var transaction = new Transaction
            {
                WalletId = wallet.Id,
                Type = tipo,
                Amount = cmd.Amount,
                AuctionId = cmd.AuctionId,
                Date = DateTime.UtcNow
            };
            await _transactions.AddAsync(transaction);

            await _uow.SaveChangesAsync();

            return transaction.ToDto();
        }

        private static void ApplyMovement(Wallet wallet, TransactionType tipo, decimal amount)
        {
            // RETENCION/LIBERACION solo las ejecuta el sistema; la API no las acepta.
            if (tipo == TransactionType.Retencion || tipo == TransactionType.Liberacion)
                throw new DomainConflictException("La retención/liberación de saldo solo puede realizarla el sistema al procesar una puja");

            switch (tipo)
            {
                case TransactionType.Deposito:
                    wallet.TotalBalance += amount;
                    break;

                case TransactionType.Retiro:
                    if (amount > wallet.AvailableBalance)
                        throw new DomainConflictException("Saldo disponible insuficiente para retirar");
                    wallet.TotalBalance -= amount;
                    break;

                default:
                    throw new DomainException("Tipo de movimiento no válido");
            }
        }
    }
}