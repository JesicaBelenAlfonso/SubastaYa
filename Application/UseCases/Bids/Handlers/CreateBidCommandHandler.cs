using SubastaYa.Application.DTOs;
using SubastaYa.Application.Interfaces;
using SubastaYa.Application.Mappings;
using SubastaYa.Application.UseCases.Bids.Commands;
using SubastaYa.Domain.Exceptions;
using System.Threading.Tasks;

namespace SubastaYa.Application.UseCases.Bids.Handlers
{
    public class CreateBidCommandHandler
    {
        private readonly IAuctionRepository _auctions;
        private readonly IBidRepository _bids;
        private readonly IWalletRepository _wallets;     
        private readonly IUnitOfWork _uow;
        private readonly IAuditService _audit;

        public CreateBidCommandHandler(
            IAuctionRepository auctions,
            IBidRepository bids,
            IWalletRepository wallets,      
            IUnitOfWork uow,
            IAuditService audit)
        {
            _auctions = auctions;
            _bids = bids;
            _wallets = wallets;            
            _uow = uow;
            _audit = audit;
        }

        public async Task<BidResponseDto> Handle(CreateBidCommand cmd)
        {
            var auction = await _auctions.GetByIdAsync(cmd.AuctionId) //Valida su existe subasta
                ?? throw new DomainException($"No existe una subasta con Id {cmd.AuctionId}");

            //Calculo del monto min requerido
            // Obtenemos la oferta actual más alta por esta subasta
            var currentOffer = await _bids.GetHighestAmountByAuctionIdAsync(cmd.AuctionId);
            // El mínimo es el actual o el precio base si no hay ofertas
            var minimum = currentOffer ?? auction.BasePrice;

            
            // La puja debe ser al menos igual a (oferta actual + incremento mínimo oficial)
            var requiredAmount = minimum + auction.MinIncrement;
            if (cmd.Amount < requiredAmount)
            {
                var reference = currentOffer.HasValue ? "la oferta actual" : "el precio base";
                throw new DomainException($"La puja debe ser mayor o igual a {reference} ({minimum}) + incremento mínimo ({auction.MinIncrement}) = {requiredAmount}");
            }
            

            
            // Verificamos que el usuario tenga una wallet asociada
            var wallet = await _wallets.GetByUserIdAsync(cmd.BuyerId)
                ?? throw new DomainException($"El usuario id {cmd.BuyerId} no tiene una billetera asociada");

            // Validamos que el usuario tenga suficiente saldo disponible
            if (wallet.AvailableBalance < cmd.Amount)
            {
                throw new DomainException($"Saldo disponible insuficiente para tu puja. Disponible: {wallet.AvailableBalance}, Requerido: {cmd.Amount}");
            }

            
            // Bloqueamos el monto para que el usuario no pueda usarlo mientras su puja esté activa
            wallet.HeldBalance += cmd.Amount;
            

            
            var bid = cmd.ToEntity(cmd.BuyerId); // Creamos la entidad Bid 
            

            
            // Después de crear la puja, actualizamos el precio base de la subasta
            // con el nuevo monto de la puja ganadora
            auction.BasePrice = cmd.Amount;
            

            
            await _bids.AddAsync(bid);
            await _uow.SaveChangesAsync(); // Guardamos: Bid + Wallet + Auction de forma atómica
            

            
            await _audit.LogAsync("Bid", bid.Id, "CREATE", cmd.BuyerId, new //Registro de Auditoría
            {
                bid.AuctionId,
                bid.Amount
            });
            
            return bid.ToDto();
        }
    }
}