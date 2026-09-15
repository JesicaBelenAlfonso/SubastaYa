using SubastaYa.Domain.Entities;
using SubastaYa.Application.DTOs;

namespace SubastaYa.Application.Mappings
{
    public static class TransactionMappings
    {
        public static TransactionResponseDto ToDto(this Transaction t)
        {
            return new TransactionResponseDto
            {
                Id = t.Id,
                WalletId = t.WalletId,
                Type = t.Type,
                Amount = t.Amount,
                Date = t.Date,
                AuctionId = t.AuctionId
            };
        }
    }
}