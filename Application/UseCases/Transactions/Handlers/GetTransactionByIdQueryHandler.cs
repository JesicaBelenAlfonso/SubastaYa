using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Transactions.Queries;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Transactions.Handlers
{
    public class GetTransactionByIdQueryHandler
    {
        private readonly IWalletRepository _wallets;
        private readonly ITransactionRepository _transactions;

        public GetTransactionByIdQueryHandler(
            IWalletRepository wallets,
            ITransactionRepository transactions)
        {
            _wallets = wallets;
            _transactions = transactions;
        }

        public async Task<TransactionResponseDto?> Handle(GetTransactionByIdQuery query)
        {
            var wallet = await _wallets.GetByUserIdAsync(query.UserId)
                ?? throw new NotFoundException("El usuario no tiene una billetera asociada");

            var transaction = await _transactions.GetByIdAsync(wallet.Id, query.TransactionId);

            if (transaction is null)
                throw new NotFoundException($"No existe un movimiento con Id {query.TransactionId}");

            return transaction.ToDto();
        }
    }
}