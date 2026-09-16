using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Wallets.Handlers;
using SubastaYa.Application.UseCases.Wallets.Queries;

namespace SubastaYa.Api.Controllers
{
    /// <summary>
    /// Billetera del usuario (saldo disponible, retenido y total).
    /// </summary>
    [ApiController]
    [Route("api/v1/users/{userId}/wallets")]   // recurso anidado: la billetera pertenece a un usuario
    public class WalletsController : ControllerBase
    {
        private readonly GetWalletByUserIdQueryHandler _getWallet;

        public WalletsController(GetWalletByUserIdQueryHandler getWallet)
        {
            _getWallet = getWallet;
        }

        /// <summary>
        /// Devuelve la billetera del usuario.
        /// </summary>
        /// <response code="200">Billetera encontrada.</response>
        /// <response code="404">El usuario no tiene billetera.</response>
        [HttpGet]
        [ProducesResponseType(typeof(WalletResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(int userId)
        {
            var dto = await _getWallet.Handle(new GetWalletByUserIdQuery(userId));
            return Ok(dto);
        }

    }
}
