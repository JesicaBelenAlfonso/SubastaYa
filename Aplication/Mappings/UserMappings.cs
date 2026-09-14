using Domain.Entities;
using SubastaYa.Application.DTOs;

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
    }
}