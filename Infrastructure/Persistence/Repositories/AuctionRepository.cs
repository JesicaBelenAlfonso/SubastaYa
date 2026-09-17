using SubastaYa.Domain.Entities;
using SubastaYa.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SubastaYa.Infrastructure.Persistence.Repositories
{
    public class AuctionRepository : IAuctionRepository
    {
        private readonly AppDbContext _ctx;

        public AuctionRepository(AppDbContext ctx)
        {
            _ctx = ctx;
        }

        public async Task AddAsync(Auction auction)
        {
            await _ctx.Auctions.AddAsync(auction);
        }

        public async Task<IEnumerable<Auction>> GetAllAsync()
            => await _ctx.Auctions.OrderByDescending(a => a.StartDate).ToListAsync();

        public async Task<IEnumerable<Auction>> GetAuctionsForUserAsync(int userId)
            => await _ctx.Auctions
                .AsNoTracking()
                .Where(a => a.SellerId == userId
                    || _ctx.Bids.Any(b => b.AuctionId == a.Id && b.BuyerId == userId))
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

        public async Task<Auction?> GetByIdAsync(int id)
            => await _ctx.Auctions.FirstOrDefaultAsync(a => a.Id == id);

        public void Delete(Auction auction)
            => _ctx.Auctions.Remove(auction);

        public async Task<IEnumerable<Auction>> GetExpiredActiveAsync(DateTime now)
            => await _ctx.Auctions
                .Where(a => a.Status == AuctionStatus.Activa && a.EndDate <= now)
                .ToListAsync();

        public async Task<IEnumerable<AuctionCatalogItem>> GetCatalogAsync(
            AuctionStatus? estado,
            int? categoriaId,
            decimal? minPrecio,
            decimal? maxPrecio,
            string orden,
            int pagina,
            int tamano)
        {
            var now = DateTime.UtcNow;

            var subastas = ApplyCatalogFilters(
                _ctx.Auctions.AsNoTracking(), estado, categoriaId, minPrecio, maxPrecio, now);

            // Una sola query: las pujas se agregan por subasta (Max y Count), sin N+1.
            var catalogo =
                from a in subastas
                join b in _ctx.Bids.AsNoTracking() on a.Id equals b.AuctionId into pujas
                select new AuctionCatalogItem
                {
                    Auction = a,
                    OfertaActual = pujas.Max(p => (decimal?)p.Amount),
                    CantidadPujas = pujas.Count()
                };

            catalogo = ApplyCatalogOrder(catalogo, orden);

            var skip = (pagina - 1) * tamano;
            if (skip < 0)
                skip = 0;

            return await catalogo.Skip(skip).Take(tamano).ToListAsync();
        }

        public async Task<int> GetCatalogCountAsync(
            AuctionStatus? estado,
            int? categoriaId,
            decimal? minPrecio,
            decimal? maxPrecio)
        {
            var now = DateTime.UtcNow;

            var subastas = ApplyCatalogFilters(
                _ctx.Auctions.AsNoTracking(), estado, categoriaId, minPrecio, maxPrecio, now);

            return await subastas.CountAsync();
        }

        private IQueryable<Auction> ApplyCatalogFilters(
            IQueryable<Auction> subastas,
            AuctionStatus? estado,
            int? categoriaId,
            decimal? minPrecio,
            decimal? maxPrecio,
            DateTime now)
        {
            if (estado.HasValue)
            {
                var estadoBuscado = estado.Value;

                // Una PROXIMA que ya empezó se considera ACTIVA (auto-activación sin worker).
                subastas = estadoBuscado == AuctionStatus.Activa
                    ? subastas.Where(a => a.Status == AuctionStatus.Activa
                        || (a.Status == AuctionStatus.Proxima && a.StartDate <= now && a.EndDate > now))
                    : subastas.Where(a => a.Status == estadoBuscado);
            }

            if (categoriaId.HasValue)
            {
                var categoria = categoriaId.Value;
                subastas = subastas.Where(a => a.CategoryId == categoria);
            }

            // El precio filtrado es la oferta actual si existe; si no, el precio base.
            if (minPrecio.HasValue)
            {
                var minimo = minPrecio.Value;
                subastas = subastas.Where(a =>
                    (_ctx.Bids.Where(b => b.AuctionId == a.Id).Max(b => (decimal?)b.Amount) ?? a.BasePrice) >= minimo);
            }

            if (maxPrecio.HasValue)
            {
                var maximo = maxPrecio.Value;
                subastas = subastas.Where(a =>
                    (_ctx.Bids.Where(b => b.AuctionId == a.Id).Max(b => (decimal?)b.Amount) ?? a.BasePrice) <= maximo);
            }

            return subastas;
        }

        private static IQueryable<AuctionCatalogItem> ApplyCatalogOrder(
            IQueryable<AuctionCatalogItem> catalogo,
            string orden)
        {
            IOrderedQueryable<AuctionCatalogItem> ordenado = (orden ?? string.Empty).ToLowerInvariant() switch
            {
                "precio-desc" => catalogo.OrderByDescending(c => c.OfertaActual),
                "precio-asc" => catalogo.OrderBy(c => c.OfertaActual),
                "final" => catalogo.OrderBy(c => c.Auction.EndDate),
                _ => catalogo.OrderByDescending(c => c.Auction.StartDate)
            };

            return ordenado.ThenBy(c => c.Auction.Id);
        }
    }
}
