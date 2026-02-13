namespace UniversiteDomain.Exceptions.NoteExceptions;

public class InvalidNoteValueException : Exception
{
    public InvalidNoteValueException() : base() { }
    public InvalidNoteValueException(string message) : base(message) { }
    public InvalidNoteValueException(string message, Exception inner) : base(message, inner) { }
}