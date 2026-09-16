namespace SubastaYa.Application.DTOs
{
    /// <summary>
    /// Entrada del historial de pujas de una subasta, anonimizada:
    /// no expone quién es cada pujador, solo un rótulo neutro.
    /// </summary>
    public class BidHistoryResponseDto
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public System.DateTime BidDate { get; set; }

        /// <summary>Rótulo anónimo del pujador (ej: "Pujador 2"). Nunca el nombre o id real.</summary>
        public string BuyerLabel { get; set; } = string.Empty;

        /// <summary>true si la puja pertenece al usuario que consulta (request erróneamente autorizado).</summary>
        public bool IsMine { get; set; }

        /// <summary>true si esta puja es la que lidera la subasta en este momento.</summary>
        public bool IsLeader { get; set; }
    }
}