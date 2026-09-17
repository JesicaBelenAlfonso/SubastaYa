using SubastaYa.Application.Interfaces;

namespace SubastaYa.Api.Workers
{
    public class AuctionStatusWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AuctionStatusWorker> _logger;

        public AuctionStatusWorker(IServiceScopeFactory scopeFactory, ILogger<AuctionStatusWorker> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IAuctionFinalizationService>();

                    var finalized = await service.FinalizeExpiredAuctionsAsync(stoppingToken);
                    if (finalized > 0)
                        _logger.LogInformation("Worker finalizó {Count} subastas", finalized);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al finalizar subastas vencidas");
                }

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}