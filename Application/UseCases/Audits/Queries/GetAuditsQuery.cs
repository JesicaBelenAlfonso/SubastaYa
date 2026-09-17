namespace SubastaYa.Application.UseCases.Audits.Queries
{
    public class GetAuditsQuery
    {
        public string? Entity { get; set; }
        public string? Action { get; set; }

        public GetAuditsQuery() { }

        public GetAuditsQuery(string? entity, string? action)
        {
            Entity = entity;
            Action = action;
        }
    }
}