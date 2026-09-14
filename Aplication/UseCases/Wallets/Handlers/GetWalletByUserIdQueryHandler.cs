using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.UseCases.Wallets.Queries;
using SubastaYa.Domain.Exceptions;

namespace SubastaYa.Application.UseCases.Wallets.Handlers
{
    public class GetWalletByUserIdQueryHandler
    {
        private readonly IWalletRepository _wallets;

        public GetWalletByUserIdQueryHandler(IWalletRepository wallets)
        {
            _wallets = wallets;
        }

        public async Task<WalletResponseDto> Handle(GetWalletByUserIdQuery query)
        {
            var wallet = await _wallets.GetByUserIdAsync(query.UserId);

            if (wallet is null)
                throw new NotFoundException("El usuario no tiene una billetera asociada");
            return new WalletResponseDto
            {
                Id = wallet.Id,
                TotalBalance = wallet.TotalBalance,
                HeldBalance = wallet.HeldBalance,
                AvailableBalance = wallet.AvailableBalance
            };
        }
    }
}