

namespace SubastaYa.Application.UseCases.Users.Queries
{
    public class GetUserByIdQuery
    {
        public int Id { get; set; }

        public GetUserByIdQuery(int id)
        {
            Id = id;
        }
    }
}