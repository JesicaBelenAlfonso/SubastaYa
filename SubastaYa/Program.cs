using Infraestructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.UseCases.Auth.Handlers;
using SubastaYa.Application.UseCases.Transactions.Handlers;
using SubastaYa.Application.UseCases.Users.Handlers;
using SubastaYa.Application.UseCases.Wallets.Handlers;
using SubastaYa.Infrastructure.Persistence;
using SubastaYa.Infrastructure.Persistence.Repositories;
using SubastaYa.Infrastructure.Segurity;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS: en desarrollo se permite cualquier origen (Live Server usa puertos variables).
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

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
builder.Services.AddScoped<GetWalletByUserIdQueryHandler>();
builder.Services.AddScoped<ITransactionRepository,TransactionRepository>();
builder.Services.AddScoped<GetWalletTransactionsQueryHandler>();
builder.Services.AddScoped<GetTransactionByIdQueryHandler>();
builder.Services.AddScoped<CreateTransactionCommandHandler>();
builder.Services.AddScoped<LoginCommandHandler>();

var app = builder.Build(); 
app.UseMiddleware<ExceptionMiddleware>();   // ← primero, para atrapar todo lo que viene después

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("FrontendDev");

app.UseAuthorization();

app.MapControllers();

app.Run();