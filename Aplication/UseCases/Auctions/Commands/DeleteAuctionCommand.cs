namespace SubastaYa.Application.UseCases.Auctions.Commands
{
    public class DeleteAuctionCommand
    {
        public int Id { get; set; }

        public DeleteAuctionCommand(int id)
        {
            Id = id;
        }
    }
}
