namespace SubastaYa.Application
{
    public class AuctionOptions
    {
        // Anti-sniping: una puja con 60 s o menos de margen extiende el cierre 2 min.
        public int AntiSnipingWindowSeconds { get; set; } = 60;
        public int AntiSnipingExtensionMinutes { get; set; } = 2;
    }
}
