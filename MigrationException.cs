namespace Svn2GitConsole;

public class MigrationException : Exception
{
    public MigrationException(string message, Exception? innerException=null) : base(message, innerException) { }
}
