using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Persistence;
using SubastaYa.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SubastaYa.Application.DTOs;


namespace SubastaYa.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _ctx;

        public UserRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _ctx.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(User user)
        {
            await _ctx.Users.AddAsync(user);
            // NO llamamos a SaveChangesAsync acá (lo hace el UnitOfWork).
        }
        public async Task<User?> GetByIdAsync(int id)                    
       => await _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        public void Delete(User user)
       => _ctx.Users.Remove(user);
    }
}
