using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.UseCases.Wallets.Commands;
using SubastaYa.Application.UseCases.Wallets.Handlers;
using SubastaYa.Application.UseCases.Wallets.Queries;

namespace SubastaYa.Api.Controllers
{
    [ApiController]
    [Route("api/v1/users/{userId}/wallet")]   // recurso anidado: la billetera pertenece a un usuario
    public class WalletsController : ControllerBase
    {
        private readonly GetWalletByUserIdQueryHandler _getWallet;
        private readonly DeleteWalletCommandHandler _delete;

        public WalletsController(GetWalletByUserIdQueryHandler getWallet, DeleteWalletCommandHandler delete)
        {
            _getWallet = getWallet;
            _delete = delete;
        }

        [HttpGet]
        public async Task<IActionResult> Get(int userId)
        {
            var dto = await _getWallet.Handle(new GetWalletByUserIdQuery(userId));
            return Ok(dto);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int userId)
        {
            await _delete.Handle(new DeleteWalletCommand { UserId = userId });
            return NoContent();   // 204
        }

    }
}