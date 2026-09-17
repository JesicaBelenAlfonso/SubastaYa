using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.UseCases.Wallets.Handlers;
using SubastaYa.Application.UseCases.Wallets.Queries;

namespace SubastaYa.Api.Controllers
{
    
    /// Billetera del usuario (saldo disponible, retenido y total).
    [ApiController]
    [Route("api/v1/users/{userId}/wallets")]   // recurso anidado: la billetera pertenece a un usuario
    public class WalletsController : ControllerBase
    {
        private readonly GetWalletByUserIdQueryHandler _getWallet;

        public WalletsController(GetWalletByUserIdQueryHandler getWallet)
        {
            _getWallet = getWallet;
        }


        /// Devuelve la billetera del usuario.
        /// 200 OK: Devuelve la billetera del usuario.
        /// 404 Not Found: El usuario no tiene una billetera asociada.
     
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
