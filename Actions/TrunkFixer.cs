using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions
{
    public class TrunkFixer : IMigrationStep
    {
        private readonly IGitService _gitService;

        private readonly IConsoleWriter _console;

        public TrunkFixer(IGitService gitService, IConsoleWriter console)
        {
            _gitService = gitService;
            _console = console;
        }

        public void Run(SharedData sharedData)
        {
            _console.WriteLine("*** Trying to fix trunk name...");
            var trunk = sharedData.RemoteBranches.FirstOrDefault(b => b.Trim() == "trunk");
            if (trunk != null)
            {
                _gitService.Checkout("svn/trunk", false);
                _gitService.DeleteBranch("master", false);
                _gitService.CheckoutAndCreateBranch("master");
            }
            else
            {
                _gitService.Checkout("master", true);
            }
        }
    }
}
