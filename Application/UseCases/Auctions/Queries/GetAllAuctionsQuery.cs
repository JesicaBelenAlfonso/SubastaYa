namespace SubastaYa.Application.UseCases.Auctions.Queries
{
    public class GetAllAuctionsQuery
    {
        public string? Estado { get; set; }
        public int? CategoriaId { get; set; }
        public decimal? MinPrecio { get; set; }
        public decimal? MaxPrecio { get; set; }
        public string Orden { get; set; } = "recientes";
        public int Pagina { get; set; } = 1;
        public int Tamano { get; set; } = 12;
    }
}
