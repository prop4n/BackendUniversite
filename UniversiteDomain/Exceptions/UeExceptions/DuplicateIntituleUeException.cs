namespace UniversiteDomain.Exceptions.UeExceptions;

public class DuplicateIntituleUeException : Exception
{
    public DuplicateIntituleUeException() : base() { }
    public DuplicateIntituleUeException(string message) : base(message) { }
    public DuplicateIntituleUeException(string message, Exception inner) : base(message, inner) { }
}