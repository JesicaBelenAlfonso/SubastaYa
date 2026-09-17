using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using SubastaYa.Application;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Services;
using SubastaYa.Application.UseCases.Auctions.Handlers;
using SubastaYa.Application.UseCases.Audits.Handlers;
using SubastaYa.Application.UseCases.Auth.Handlers;
using SubastaYa.Application.UseCases.Bids.Handlers;
using SubastaYa.Application.UseCases.Categories.Handlers;
using SubastaYa.Application.UseCases.Transactions.Handlers;
using SubastaYa.Application.UseCases.Users.Handlers;
using SubastaYa.Application.UseCases.Wallets.Handlers;
using SubastaYa.Api.Workers;
using SubastaYa.Infrastructure.Persistence;
using SubastaYa.Infrastructure.Persistence.Repositories;
using SubastaYa.Infrastructure.Segurity;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SubastaYa API",
        Version = "v1",
        Description = "API de la plataforma de subastas en línea. " +
                      "Estados de una subasta: ACTIVA, PROXIMA, FINALIZADA o DESIERTA."
    });

    // Incluye los comentarios XML de los controllers/DTOs en el esquema OpenAPI.
    var xmlFiles = new[]
    {
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml", // SubastaYa (controllers)
        "SubastaYa.Application.xml"                               // DTOs con <example>
    };
    foreach (var xmlFile in xmlFiles)
    {
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    }
});

// CORS: en desarrollo se permite cualquier origen (Live Server usa puertos variables).
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Inyección de dependencias.
var conexionString=builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(conexionString));
builder.Services.AddScoped<RegisterUserCommandHandler>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<IWalletRepository, WalletRepository>();
builder.Services.AddScoped<GetUserByIdQueryHandler>();
builder.Services.AddScoped<GetUserActivitiesQueryHandler>();
builder.Services.AddScoped<DeleteUserCommandHandler>();
builder.Services.AddScoped<UpdateUserCommandHandler>();
builder.Services.AddScoped<GetWalletByUserIdQueryHandler>();
builder.Services.AddScoped<ITransactionRepository,TransactionRepository>();
builder.Services.AddScoped<GetWalletTransactionsQueryHandler>();
builder.Services.AddScoped<GetTransactionByIdQueryHandler>();
builder.Services.AddScoped<CreateTransactionCommandHandler>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<IAuctionRepository, AuctionRepository>();
builder.Services.AddScoped<CreateAuctionCommandHandler>();
builder.Services.AddScoped<GetAuctionByIdQueryHandler>();
builder.Services.AddScoped<GetAllAuctionsQueryHandler>();
builder.Services.AddScoped<GetAuctionBidsQueryHandler>();
builder.Services.AddScoped<UpdateAuctionCommandHandler>();
builder.Services.AddScoped<DeleteAuctionCommandHandler>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<CreateCategoryCommandHandler>();
builder.Services.AddScoped<GetCategoriesQueryHandler>();
builder.Services.AddScoped<GetCategoryByIdQueryHandler>();
builder.Services.AddScoped<IBidRepository, BidRepository>();
builder.Services.AddScoped<CreateBidCommandHandler>();
builder.Services.AddScoped<IAuditRepository, AuditRepository>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<GetAuditsQueryHandler>();
builder.Services.AddSingleton(
builder.Configuration.GetSection("Auction").Get<AuctionOptions>() ?? new AuctionOptions());
builder.Services.AddScoped<IAuctionFinalizationService, AuctionFinalizationService>();
builder.Services.AddHostedService<AuctionStatusWorker>();

var app = builder.Build();
app.UseMiddleware<ExceptionMiddleware>();   // primero, para atrapar todo lo que viene después

// Aplica migraciones pendientes y siembra los datos de prueba si la base está vacía.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    await DbSeeder.SeedAsync(db, hasher);
}

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
