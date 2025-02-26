using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions
{
    public class RepoOptimizer : IMigrationStep
    {
        private readonly IGitService _gitService;

        private readonly IConsoleWriter _console;

        public RepoOptimizer(IGitService gitService, IConsoleWriter console)
        {
            _gitService = gitService;
            _console = console;
        }
        public void Run(SharedData sharedData)
        {
            _console.WriteLine("*** Optimizing repository...");
            _gitService.GarbageCollect();
        }
    }
}
