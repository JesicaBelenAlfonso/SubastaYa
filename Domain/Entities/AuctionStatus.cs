namespace SubastaYa.Domain.Entities
{
    public enum AuctionStatus
    {
        Proxima,
        Activa,
        Finalizada,
        Desierta
    }

    public static class AuctionStatusExtensions
    {
        public static bool IsTerminal(this AuctionStatus status)
            => status == AuctionStatus.Finalizada || status == AuctionStatus.Desierta;
    }
}