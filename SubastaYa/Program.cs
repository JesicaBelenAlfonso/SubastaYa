
using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.UseCases.Users.Handlers;
using SubastaYa.Infrastructure.Persistence;
using SubastaYa.Infrastructure.Persistence.Repositories;
using SubastaYa.Infrastructure.Segurity;
using SubastaYa.Application.UseCases.Wallets.Handlers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// inyectamos 
// Add services to the container.
var conexionString=builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(conexionString));
builder.Services.AddScoped<RegisterUserCommandHandler>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>(); 
builder.Services.AddScoped<GetUserByIdQueryHandler>();
builder.Services.AddScoped<DeleteUserCommandHandler>();
builder.Services.AddScoped<UpdateUserCommandHandler>();



var app = builder.Build(); 
app.UseMiddleware<ExceptionMiddleware>();   // ← primero, para atrapar todo lo que viene después

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
