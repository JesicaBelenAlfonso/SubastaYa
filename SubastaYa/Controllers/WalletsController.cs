using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Wallets.Handlers;
using SubastaYa.Application.UseCases.Wallets.Queries;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/wallet")]   // recurso anidado: la billetera pertenece a un usuario
    public class WalletsController : ControllerBase
    {
        private readonly GetWalletByUserIdQueryHandler _getWallet;

        public WalletsController(GetWalletByUserIdQueryHandler getWallet)
        {
            _getWallet = getWallet;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int userId)
        {
            var dto = await _getWallet.Handle(new GetWalletByUserIdQuery(userId));
            return Ok(dto);
        }
    }
}