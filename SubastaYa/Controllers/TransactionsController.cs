using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Transactions.Commands;
using SubastaYa.Application.UseCases.Transactions.Handlers;
using SubastaYa.Application.UseCases.Transactions.Queries;
using SubastaYa.Application.UseCases.Wallets.Handlers;
using SubastaYa.Application.UseCases.Wallets.Queries;

namespace SubastaYa.Api.Controllers
{
   
    /// Movimientos de la billetera (ledger append-only): DEPOSITO, RETIRO y consulta.
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


        /// Historial de movimientos (ledger) de la billetera del usuario.
        /// 200 Listado del ledger.
        /// 404 El usuario no tiene billetera.
       
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TransactionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int userId)
        {
            var wallet = await _getWallet.Handle(new GetWalletByUserIdQuery(userId));
            var dto = await _getTransactions.Handle(new GetWalletTransactionsQuery { WalletId = wallet.Id });
            return Ok(dto);
        }


        /// Devuelve un movimiento del ledger por su id.
        /// 200 Movimiento encontrado.
        /// 404 Movimiento inexistente.
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int userId, int id)
        {
            var dto = await _getTransactionById.Handle(new GetTransactionByIdQuery(userId, id));
            return Ok(dto);
        }


        /// Registra un movimiento manual (DEPOSITO / RETIRO). La retención y liberación
        /// solo las genera el sistema al procesar una puja: aunque el cliente envíe un
        /// AuctionId, estos tipos son rechazados desde esta API abierta.
        /// 201 Movimiento registrado.
        /// 400 Tipo inválido o retiro mayor al saldo disponible.
        /// 404 El usuario no tiene billetera.
        [HttpPost]
        [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(int userId, CreateTransactionCommand cmd)
        {
            cmd.UserId = userId;   // el id viene de la URL
            var dto = await _createTransaction.Handle(cmd);
            return CreatedAtAction(nameof(GetById), new { userId = userId, id = dto.Id }, dto);
        }
    }
}
