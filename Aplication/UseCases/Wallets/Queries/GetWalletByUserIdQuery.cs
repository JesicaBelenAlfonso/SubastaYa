namespace SubastaYa.Application.UseCases.Wallets.Queries
{
    public class GetWalletByUserIdQuery
    {
        public int UserId { get; set; }

        public GetWalletByUserIdQuery() { }

        public GetWalletByUserIdQuery(int userId)
        {
            UserId = userId;
        }
    }
}