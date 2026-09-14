using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Transactions.Commands;
using SubastaYa.Application.UseCases.Transactions.Handlers;
using SubastaYa.Application.UseCases.Transactions.Queries;
using SubastaYa.Application.UseCases.Wallets.Handlers;
using SubastaYa.Application.UseCases.Wallets.Queries;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/users/{userId}/wallets/transactions")]
    public class TransactionsController : ControllerBase
    {
        private readonly GetWalletByUserIdQueryHandler _getWallet;
        private readonly GetWalletTransactionsQueryHandler _getTransactions;
        private readonly GetTransactionByIdQueryHandler _getTransactionById;
        private readonly CreateTransactionCommandHandler _createTransaction;

        public TransactionsController(
            GetWalletByUserIdQueryHandler getWallet,
            GetWalletTransactionsQueryHandler getTransactions,
            GetTransactionByIdQueryHandler getTransactionById,
            CreateTransactionCommandHandler createTransaction)
        {
            _getWallet = getWallet;
            _getTransactions = getTransactions;
            _getTransactionById = getTransactionById;
            _createTransaction = createTransaction;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int userId)
        {
            var wallet = await _getWallet.Handle(new GetWalletByUserIdQuery(userId));
            var dto = await _getTransactions.Handle(new GetWalletTransactionsQuery { WalletId = wallet.Id });
            return Ok(dto);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int userId, int id)
        {
            var dto = await _getTransactionById.Handle(new GetTransactionByIdQuery(userId, id));
            return Ok(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Create(int userId, CreateTransactionCommand cmd)
        {
            cmd.UserId = userId;   // el id viene de la URL
            var dto = await _createTransaction.Handle(cmd);
            return CreatedAtAction(nameof(GetById), new { userId = userId, id = dto.Id }, dto);
        }
    }
}