using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions;

public class TrunkFixerRebase : IMigrationStep
{
    private readonly IGitService _gitService;

    private readonly IConsoleWriter _console;

    public TrunkFixerRebase(IGitService gitService, IConsoleWriter console)
    {
        _gitService = gitService;
        _console = console;
    }

    public void Run(SharedData sharedData)
    {
        _console.WriteLine("*** Trunk fixer for rebase mode...");
        _gitService.Checkout("master", true);
    }
}
