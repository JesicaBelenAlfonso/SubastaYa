namespace SubastaYa.Application.DTOs
{
    public class TransactionResponseDto
    {
        public int Id { get; set; }
        public int WalletId { get; set; }
        public string Type { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public System.DateTime Date { get; set; }
        public int? AuctionId { get; set; }
    }
}