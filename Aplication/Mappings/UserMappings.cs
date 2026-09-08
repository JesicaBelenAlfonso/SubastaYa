using Domain.Entities;
using SubastaYa.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubastaYa.Application.Mappings
{
    public static class UserMappings
    {
        public static UserResponseDto ToDto(this User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                RegisteredAt = user.RegisteredAt
            };
        }

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
