namespace SubastaYa.Application.DTOs
{
    /// <summary>
    /// Actividad de un usuario sobre una subasta: la creó como vendedor o participó
    /// como pujador (con indicador de si hoy está liderando).
    /// </summary>
    public class UserActivityResponseDto
    {
        /// <summary>VENDEDOR (la creó) o PUJADOR (participó).</summary>
        public string Role { get; set; } = string.Empty;

        /// <summary>true si el usuario es pujador y su puja es la vigente (líder).</summary>
        public bool Liderando { get; set; }

        public AuctionResponseDto Auction { get; set; } = null!;
    }
}