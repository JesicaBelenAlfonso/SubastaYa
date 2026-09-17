using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Interfaces;
using SubastaYa.Domain.Entities;

namespace SubastaYa.Infrastructure.Persistence
{
    public static class DbSeeder
    {
        public const string DefaultPassword = "Password123";

        // Siembra los datos de prueba SOLO si la base estÃ¡ vacÃ­a (idempotente).
        public static async Task SeedAsync(AppDbContext ctx, IPasswordHasher hasher)
        {
            if (await ctx.Users.AnyAsync())
                return;

            var now = DateTime.UtcNow;

            // ---------- 4 categorÃ­as ----------
            var categorias = new[]
            {
                new Category { Name = "ElectrÃ³nica", UrlIcono = "bi-phone" },
                new Category { Name = "VehÃ­culos", UrlIcono = "bi-bicycle" },
                new Category { Name = "Coleccionables", UrlIcono = "bi-stars" },
                new Category { Name = "Hogar", UrlIcono = "bi-house-heart" },
            };
            await ctx.Categories.AddRangeAsync(categorias);
            await ctx.SaveChangesAsync();

            var catId = await ctx.Categories.ToDictionaryAsync(c => c.Name, c => c.Id);

            // ---------- Usuarios + wallets ----------
            var vendedor   = NewUser("vendedor@subastaya.com",    "Vendedor",      hasher);
            var comprador1 = NewUser("comprador1@subastaya.com",  "Comprador Uno", hasher);
            var comprador2 = NewUser("comprador2@subastaya.com",  "Comprador Dos", hasher);
            var sinfondos  = NewUser("sinfondos@subastaya.com",   "Sin Fondos",    hasher);

            var walletVendedor   = new Wallet { User = vendedor,   TotalBalance = 0m };
            var walletComprador1 = new Wallet { User = comprador1, TotalBalance = 200_000m };
            // Comprador Dos es lÃ­der de A1 (retenciÃ³n 45.000) y ganador de A4 (retenciÃ³n 60.000).
            var walletComprador2 = new Wallet { User = comprador2, TotalBalance = 200_000m, HeldBalance = 105_000m };
            var walletSinFondos  = new Wallet { User = sinfondos,  TotalBalance = 500m };

            ctx.Users.AddRange(vendedor, comprador1, comprador2, sinfondos);
            ctx.Wallets.AddRange(walletVendedor, walletComprador1, walletComprador2, walletSinFondos);
            await ctx.SaveChangesAsync();

            // ---------- 5 subastas ----------
            var a1 = new Auction
            {
                SellerId = vendedor.Id,
                CategoryId = catId["ElectrÃ³nica"],
                Title = "Notebook Gamer RTX 16GB",
                Descripcion = "Notebook usada con garantÃ­a, impecable.",
                UrlImagen = "https://picsum.photos/seed/notebook/600/400",
                BasePrice = 40_000m,
                MinIncrement = 5_000m,
                StartDate = now.AddHours(-2),
                EndDate = now.AddDays(2),
                Status = AuctionStatus.Activa
            };

            var a2 = new Auction
            {
                SellerId = vendedor.Id,
                CategoryId = catId["VehÃ­culos"],
                Title = "Bicicleta Mountain Bike 29",
                Descripcion = "Ideal para probar el anti-sniping: termina en menos de 2 minutos.",
                UrlImagen = "https://picsum.photos/seed/bici/600/400",
                BasePrice = 30_000m,
                MinIncrement = 2_000m,
                StartDate = now.AddHours(-1),
                EndDate = now.AddSeconds(90),
                Status = AuctionStatus.Activa
            };

            var a3 = new Auction
            {
                SellerId = vendedor.Id,
                CategoryId = catId["Coleccionables"],
                Title = "Figura de colecciÃ³n ediciÃ³n limitada",
                Descripcion = "Comienza maÃ±ana: visible como PRÃ“XIMA.",
                UrlImagen = "https://picsum.photos/seed/figura/600/400",
                BasePrice = 15_000m,
                MinIncrement = 1_000m,
                StartDate = now.AddHours(24),
                EndDate = now.AddDays(3),
                Status = AuctionStatus.Proxima
            };

            var a4 = new Auction
            {
                SellerId = vendedor.Id,
                CategoryId = catId["Hogar"],
                Title = "Juego de living de roble",
                Descripcion = "Vencida con ganador: la liquida el worker al arrancar.",
                UrlImagen = "https://picsum.photos/seed/living/600/400",
                BasePrice = 50_000m,
                MinIncrement = 5_000m,
                StartDate = now.AddDays(-5),
                EndDate = now.AddHours(-1),
                Status = AuctionStatus.Activa
            };

            var a5 = new Auction
            {
                SellerId = vendedor.Id,
                CategoryId = catId["ElectrÃ³nica"],
                Title = "Monitor 27'' 144Hz",
                Descripcion = "Vencida sin pujas: pasa a DESIERTA con el worker.",
                UrlImagen = "https://picsum.photos/seed/monitor/600/400",
                BasePrice = 80_000m,
                MinIncrement = 5_000m,
                StartDate = now.AddDays(-5),
                EndDate = now.AddHours(-2),
                Status = AuctionStatus.Activa
            };

            ctx.Auctions.AddRange(a1, a2, a3, a4, a5);
            await ctx.SaveChangesAsync();

            // ---------- Pujas ----------
            // A1 (estÃ¡ndar): 2 pujas, lÃ­der Comprador Dos con 45.000
            ctx.Bids.AddRange(
                new Bid { BuyerId = comprador1.Id, AuctionId = a1.Id, Amount = 40_000m, BidDate = now.AddMinutes(-90) },
                new Bid { BuyerId = comprador2.Id, AuctionId = a1.Id, Amount = 45_000m, BidDate = now.AddMinutes(-60) });

            // A4 (vencida con ganador): gana Comprador Dos con 60.000
            ctx.Bids.Add(new Bid { BuyerId = comprador2.Id, AuctionId = a4.Id, Amount = 60_000m, BidDate = now.AddDays(-2) });

            await ctx.SaveChangesAsync();

            // ---------- Ledger coherente con las wallets ----------
            ctx.Transactions.AddRange(
                // DepÃ³sitos iniciales
                new Transaction { WalletId = walletComprador1.Id, Type = TransactionType.Deposito, Amount = 200_000m, AuctionId = null, Date = now.AddDays(-10) },
                new Transaction { WalletId = walletComprador2.Id, Type = TransactionType.Deposito, Amount = 200_000m, AuctionId = null, Date = now.AddDays(-10) },
                new Transaction { WalletId = walletSinFondos.Id, Type = TransactionType.Deposito, Amount = 500m,      AuctionId = null, Date = now.AddDays(-10) },

                // A1: Comprador Uno puja 40.000 (retenciÃ³n) y es superado (liberaciÃ³n)
                new Transaction { WalletId = walletComprador1.Id, Type = TransactionType.Retencion,  Amount = 40_000m, AuctionId = a1.Id, Date = now.AddMinutes(-90) },
                new Transaction { WalletId = walletComprador1.Id, Type = TransactionType.Liberacion, Amount = 40_000m, AuctionId = a1.Id, Date = now.AddMinutes(-60) },

                // A1: Comprador Dos queda lÃ­der con 45.000 (retenciÃ³n vigente)
                new Transaction { WalletId = walletComprador2.Id, Type = TransactionType.Retencion, Amount = 45_000m, AuctionId = a1.Id, Date = now.AddMinutes(-60) },

                // A4: Comprador Dos gana con 60.000 (retenciÃ³n vigente, el worker la liquida)
                new Transaction { WalletId = walletComprador2.Id, Type = TransactionType.Retencion, Amount = 60_000m, AuctionId = a4.Id, Date = now.AddDays(-2) });

            await ctx.SaveChangesAsync();
        }

        private static User NewUser(string email, string name, IPasswordHasher hasher)
        {
            return new User
            {
                Email = email,
                Name = name,
                PasswordHash = hasher.Hash(DefaultPassword),
                RegisteredAt = DateTime.UtcNow.AddDays(-7)
            };
        }
    }
}
