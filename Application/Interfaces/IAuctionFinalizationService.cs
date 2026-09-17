namespace SubastaYa.Application.Interfaces
{
    public interface IAuctionFinalizationService
    {
        Task<int> FinalizeExpiredAuctionsAsync(CancellationToken ct = default);
    }
}