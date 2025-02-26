namespace Svn2GitConsole.Interfaces
{
    public interface IProcessRunner
    {
        string Run(string command, bool exitOnError = true, bool printOutput = false);
    }
}