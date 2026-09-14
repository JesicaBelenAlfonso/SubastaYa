using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Users.Queries;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Users.Handlers
{
    public class GetUserByIdQueryHandler
    {
        private readonly IUserRepository _users;

        public GetUserByIdQueryHandler(IUserRepository users)
        {
            _users = users;
        }

        public async Task<UserResponseDto?> Handle(GetUserByIdQuery query)
        {
            var user = await _users.GetByIdAsync(query.Id);

            if (user is null)
                return null;               // el controller decide qué hacer con esto

            return user.ToDto();           // reusamos el mapping que ya armamos
        }
    }
}