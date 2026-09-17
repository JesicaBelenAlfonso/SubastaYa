namespace SubastaYa.Application.DTOs
{
    
    /// Entrada del historial de pujas de una subasta, anonimizada:
    /// no expone quién es cada pujador, solo un rótulo neutro.

    public class BidHistoryResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime BidDate { get; set; }

        /// Rótulo anónimo del pujador (ej: "Pujador 2"). Nunca el nombre o id real.
        public string BuyerLabel { get; set; } = string.Empty;

        /// true si la puja pertenece al usuario que consulta (request erróneamente autorizado).
        public bool IsMine { get; set; }

        /// true si esta puja es la que lidera la subasta en este momento.
        public bool IsLeader { get; set; }
    }
}