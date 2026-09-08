using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Transactions.Queries;


using SubastaYa.Domain;


namespace SubastaYa.Application.UseCases.Transactions.Handlers
{
    public class GetWalletTransactionsQueryHandler
    {
        private readonly ITransactionRepository _transactions;

        public GetWalletTransactionsQueryHandler(ITransactionRepository transactions)
        {
            _transactions = transactions;
        }

        public async Task<IEnumerable<TransactionResponseDto>> Handle(GetWalletTransactionsQuery query)
        {
            var items = await _transactions.GetByWalletIdAsync(query.WalletId);
            return items.Select(t => t.ToDto());
        }
    }
}