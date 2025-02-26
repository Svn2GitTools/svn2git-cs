using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions
{
    public class RebaseBranchFixer : IMigrationStep
    {
        private readonly IGitService _gitService;

        private readonly IConsoleWriter _console;

        public RebaseBranchFixer(IGitService gitService, IConsoleWriter console)
        {
            _gitService = gitService;
            _console = console;
        }

        public void Run(SharedData sharedData)
        {
            _console.WriteLine("*** Fixing rebase tracking information...");
            var svnBranches = sharedData.RemoteBranches
                .Where(b => b.StartsWith("svn/") && !sharedData.Tags.Contains(b))
                .ToList();

            _gitService.Fetch();

            foreach (var branch in svnBranches)
            {
                var trimmedBranch = branch.Replace("svn/", "").Trim();
                bool containsTrimmedBranch = sharedData.LocalBranches.Contains(trimmedBranch);

                if (containsTrimmedBranch || trimmedBranch == "trunk")
                {
                    var localBranch = trimmedBranch == "trunk" ? "master" : trimmedBranch;
                    _gitService.Checkout(localBranch, true);
                    _gitService.RebaseBranch(trimmedBranch);
                }
            }
        }
    }
}
