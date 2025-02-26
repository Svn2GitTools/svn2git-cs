using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions;

public class GitSvnInstallationChecker : IMigrationStep
{
    private readonly IConsoleWriter _console;

    private readonly IProcessRunner _processRunner;

    public GitSvnInstallationChecker(IProcessRunner processRunner, IConsoleWriter console)
    {
        _processRunner = processRunner;
        _console = console;
    }

    public void Run(SharedData sharedData)
    {
        _console.WriteLine("*** Git and Svn Installation checker...");

        string gitVersion = _processRunner.Run("git --version", printOutput: true);
        string svnVersion = _processRunner.Run("svn --version --quiet", printOutput: true);

        if (string.IsNullOrEmpty(gitVersion) || string.IsNullOrEmpty(svnVersion))
        {
            throw new Exception("Git and/or SVN are not installed or not functioning properly.");
        }

        _console.WriteLine("Git and SVN are installed and functional.");
    }
}
