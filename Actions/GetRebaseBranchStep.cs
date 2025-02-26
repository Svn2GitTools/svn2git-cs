using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions
{
    public class GetRebaseBranchStep : IMigrationStep
    {
        private readonly IConsoleWriter _console;

        private readonly IGitService _gitService;

        private readonly MigrationOptions _options;

        public GetRebaseBranchStep(
            IGitService gitService,
            IConsoleWriter console,
            MigrationOptions options)
        {
            _gitService = gitService;
            _console = console;
            _options = options;
        }


        public void Run(SharedData sharedData)
        {
            _console.WriteLine("*** Getting rebase branches");
            var localBranchesFiltered =
                sharedData.LocalBranches.FindAll(l => l == _options.RebaseBranch);
            var remoteBranchesFiltered =
                sharedData.RemoteBranches.FindAll(r => r.Contains(_options.RebaseBranch));
            if (localBranchesFiltered.Count > 1)
            {
                var msg =
                    $"To many matching branches found ({string.Join(", ", localBranchesFiltered)}).";
                _console.WriteLine(msg);
                throw new InvalidOperationException(msg);
            }

            if (localBranchesFiltered.Count == 0)
            {
                var msg = $"No local branch named \"{_options.RebaseBranch}\" found.";
                _console.WriteLine(msg);
                throw new InvalidOperationException(msg);
            }

            if (remoteBranchesFiltered.Count > 2)
            {
                var msg =
                    $"To many matching remotes found ({string.Join(", ", remoteBranchesFiltered)}).";
                _console.WriteLine(msg);
                throw new InvalidOperationException(msg);
            }

            if (remoteBranchesFiltered.Count == 0)
            {
                var msg = $"No remote branch named \"{_options.RebaseBranch}\" found.";
                _console.WriteLine(msg);
                throw new InvalidOperationException(msg);
            }

            _console.WriteLine(
                $"Local branches \"{string.Join(", ", localBranchesFiltered)}\" found");
            _console.WriteLine(
                $"Remote branches \"{string.Join(", ", remoteBranchesFiltered)}\" found");

            sharedData.LocalBranches = localBranchesFiltered;
            sharedData.RemoteBranches = remoteBranchesFiltered;

            sharedData.Tags.Clear(); // We only rebase the specified branch
        }
    }
}
