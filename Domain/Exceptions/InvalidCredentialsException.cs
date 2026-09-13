namespace SubastaYa.Domain.Exceptions
{
    public class InvalidCredentialsException : DomainException
    {
        public InvalidCredentialsException(string message) : base(message) { }
    }
}