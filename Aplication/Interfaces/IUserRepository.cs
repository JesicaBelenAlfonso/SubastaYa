using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Domain;
namespace SubastaYa.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task<User?> GetByIdAsync(int id);
        void Delete(User user);          

    }
}
