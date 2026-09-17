namespace SubastaYa.Application.DTOs
{

    /// Actividad de un usuario sobre una subasta: la creó como vendedor o participó
    /// como pujador (con indicador de si hoy está liderando).

    public class UserActivityResponseDto
    {
        /// VENDEDOR (la creó) o PUJADOR (participó)
        public string Role { get; set; } = string.Empty;

        /// true si el usuario es pujador y su puja es la vigente (líder)
        public bool Liderando { get; set; }


        /// Resultado de la puja para el usuario: GANADA (finalizó y era el líder),
        /// SUPERADA (finalizó y no era el líder), EN_CURSO (aún no terminó) o DESIERTA (sin pujas).

        public string ResultadoPuja { get; set; } = string.Empty;

        public AuctionResponseDto Auction { get; set; } = null!;
    }
}