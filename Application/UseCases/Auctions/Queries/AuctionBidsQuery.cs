namespace SubastaYa.Application.UseCases.Auctions.Queries
{
    public class AuctionBidsQuery
    {
        public int AuctionId { get; set; }

        /// <summary>Id del usuario que consulta (opcional). Marca sus propias pujas con IsMine.</summary>
        public int? UserId { get; set; }

        public AuctionBidsQuery(int auctionId, int? userId)
        {
            AuctionId = auctionId;
            UserId = userId;
        }
    }
}