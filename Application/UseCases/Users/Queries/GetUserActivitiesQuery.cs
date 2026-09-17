namespace SubastaYa.Application.UseCases.Users.Queries
{
    public class GetUserActivitiesQuery
    {
        public int UserId { get; set; }

        public GetUserActivitiesQuery(int userId)
        {
            UserId = userId;
        }
    }
}