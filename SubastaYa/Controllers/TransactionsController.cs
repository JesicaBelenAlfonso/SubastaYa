using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Transactions.Commands;
using SubastaYa.Application.UseCases.Transactions.Handlers;
using SubastaYa.Application.UseCases.Transactions.Queries;
using SubastaYa.Application.UseCases.Wallets.Handlers;
using SubastaYa.Application.UseCases.Wallets.Queries;

namespace SubastaYa.Api.Controllers
{
    /// <summary>
    /// Movimientos de la billetera (ledger append-only): DEPOSITO, RETIRO y consulta.
    /// </summary>
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

        /// <summary>
        /// Historial de movimientos (ledger) de la billetera del usuario.
        /// </summary>
        /// <response code="200">Listado del ledger.</response>
        /// <response code="404">El usuario no tiene billetera.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TransactionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int userId)
        {
            var wallet = await _getWallet.Handle(new GetWalletByUserIdQuery(userId));
            var dto = await _getTransactions.Handle(new GetWalletTransactionsQuery { WalletId = wallet.Id });
            return Ok(dto);
        }

        /// <summary>
        /// Devuelve un movimiento del ledger por su id.
        /// </summary>
        /// <response code="200">Movimiento encontrado.</response>
        /// <response code="404">Movimiento inexistente.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TransactionResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int userId, int id)
        {
            var dto = await _getTransactionById.Handle(new GetTransactionByIdQuery(userId, id));
            return Ok(dto);
        }

        /// <summary>
        /// Registra un movimiento manual (DEPOSITO / RETIRO). La retención y liberación
        /// solo las genera el sistema al procesar una puja: aunque el cliente envíe un
        /// AuctionId, estos tipos son rechazados desde esta API abierta.
        /// </summary>
        /// <response code="201">Movimiento registrado.</response>
        /// <response code="400">Tipo inválido o retiro mayor al saldo disponible.</response>
        /// <response code="404">El usuario no tiene billetera.</response>
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
