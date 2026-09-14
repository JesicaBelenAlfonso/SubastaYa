namespace SubastaYa.Application.UseCases.Auctions.Queries
{
    public class GetAuctionByIdQuery
    {
        public int Id { get; set; }

        public GetAuctionByIdQuery(int id)
        {
            Id = id;
        }
    }
}
