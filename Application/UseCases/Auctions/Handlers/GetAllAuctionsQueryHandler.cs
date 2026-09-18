using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Auctions.Queries;
using SubastaYa.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Auctions.Handlers
{
    public class GetAllAuctionsQueryHandler
    {
        private const string OrdenPorDefecto = "recientes";
        private const int TamanoPorDefecto = 12;

        private readonly IAuctionRepository _auctions;
        private readonly ICategoryRepository _categories;

        public GetAllAuctionsQueryHandler(
            IAuctionRepository auctions,
            ICategoryRepository categories)
        {
            _auctions = auctions;
            _categories = categories;
        }

        public async Task<PagedAuctionsResponseDto> Handle(GetAllAuctionsQuery query)
        {
            var now = DateTime.UtcNow;
            var estado = ParseEstado(query.Estado);

            var orden = string.IsNullOrWhiteSpace(query.Orden) ? OrdenPorDefecto : query.Orden;
            var pagina = query.Pagina < 1 ? 1 : query.Pagina;
            var tamano = query.Tamano < 1 ? TamanoPorDefecto : query.Tamano;

            // Sin parámetros (ni filtros ni paginación/orden explícitos) se mantiene el
            // comportamiento histórico: listado completo para que el index arme sus destacadas.
            var sinParametros = estado is null
                && !query.CategoriaId.HasValue
                && !query.MinPrecio.HasValue
                && !query.MaxPrecio.HasValue
                && pagina == 1
                && tamano == TamanoPorDefecto
                && string.Equals(orden, OrdenPorDefecto, StringComparison.OrdinalIgnoreCase);

            // Consulta 1: total de resultados (para la metadata de paginación).
            var total = await _auctions.GetCatalogCountAsync(
                estado, query.CategoriaId, query.MinPrecio, query.MaxPrecio);

            if (sinParametros)
            {
                pagina = 1;
                tamano = total;
            }

            // Consulta 2: página pedida con OfertaActual y CantidadPujas agregadas.
            var items = total == 0
                ? new List<AuctionCatalogItem>()
                : (await _auctions.GetCatalogAsync(
                    estado, query.CategoriaId, query.MinPrecio, query.MaxPrecio,
                    orden, pagina, tamano)).ToList();

            // Consulta 3: nombres de categoría en un solo viaje.
            var categorias = (await _categories.GetAllAsync())
                .ToDictionary(c => c.Id, c => c.Name);

            var resultado = items.Select(item =>
            {
                // Refleja la activación automática en memoria, sin tocar la base.
                item.Auction.RefreshStatus(now);

                var categoria = categorias.TryGetValue(item.Auction.CategoryId, out var name)
                    ? name
                    : null;

                return item.Auction.ToDto(categoria, item.OfertaActual, item.CantidadPujas);
            }).ToList();

            return new PagedAuctionsResponseDto
            {
                Items = resultado,
                Pagina = pagina,
                Tamano = tamano,
                Total = total,
                TotalPaginas = tamano > 0
                    ? (int)Math.Ceiling(total / (double)tamano)
                    : 0
            };
        }

        private static AuctionStatus? ParseEstado(string? estado)
        {
            if (string.IsNullOrWhiteSpace(estado))
                return null;

            return Enum.TryParse<AuctionStatus>(estado, ignoreCase: true, out var parsed)
                ? parsed
                : null;
        }
    }
}
