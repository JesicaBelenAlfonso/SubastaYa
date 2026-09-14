using Domain.Entities;
using SubastaYa.Application.DTOs;

namespace SubastaYa.Application.Mappings
{
    public static class WalletMappings
    {
        public static WalletResponseDto ToDto(this Wallet wallet)
        {
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