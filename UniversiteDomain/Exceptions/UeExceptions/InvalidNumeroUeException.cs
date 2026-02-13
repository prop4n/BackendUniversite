namespace UniversiteDomain.Exceptions.UeExceptions;

public class InvalidNumeroUeException : Exception
{
    public InvalidNumeroUeException() : base() { }
    public InvalidNumeroUeException(string message) : base(message) { }
    public InvalidNumeroUeException(string message, Exception inner) : base(message, inner) { }
}