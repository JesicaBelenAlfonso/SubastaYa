namespace SubastaYa.Application.UseCases.Transactions.Queries
{
    public class GetTransactionByIdQuery
    {
        public int UserId { get; set; }
        public int TransactionId { get; set; }

        public GetTransactionByIdQuery(int userId, int transactionId)
        {
            UserId = userId;
            TransactionId = transactionId;
        }
    }
}