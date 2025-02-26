using System.Text.RegularExpressions;

using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions
{
    public class GetBranchesStep : IMigrationStep
    {
        private readonly IGitService _gitService;

        private readonly IConsoleWriter _console;

        public GetBranchesStep(IGitService gitService, IConsoleWriter console)
        {
            _gitService = gitService;
            _console = console;
        }
        public void Run(SharedData sharedData)
        {
            _console.WriteLine("*** Getting branches");
            sharedData.LocalBranches  = _gitService.GetLocalBranches();
            sharedData.RemoteBranches = _gitService.GetRemoteBranches();
            //sharedData.Tags = sharedData.RemoteBranches.FindAll(b => Regex.IsMatch(b.Trim(), "^svn\\/tags\\/")).ToList();
            sharedData.Tags = sharedData.RemoteBranches
                .Where(b => b.StartsWith("svn/tags/"))
                .ToList(); ;
        }
    }
}
/*
 * private void GetBranches()
        {
            _local = RunCommand("git branch -l --no-color")
                .Split('\n')
                .Select(b => b.Replace("*", "").Trim())
                .Where(b => !string.IsNullOrEmpty(b))
                .ToList();

            _remote = RunCommand("git branch -r --no-color")
                .Split('\n')
                .Select(b => b.Replace("*", "").Trim())
                .Where(b => !string.IsNullOrEmpty(b))
                .ToList();

            _tags = _remote
                .Where(b => b.StartsWith("svn/tags/"))
                .ToList();
        }
 */