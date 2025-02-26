using System.Text.RegularExpressions;

using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions
{
    public class LegacyBranchFixer : IMigrationStep
    {
        private const string WarningMessage =
            @"********************************************************************
svn2git warning: Tracking remote SVN branches is deprecated.
In a future release local branches will be created without tracking.
If you must resync your branches, run: svn2git --rebase
********************************************************************";

        private readonly IConsoleWriter _console;

        private readonly IGitService _gitService;

        public LegacyBranchFixer(IGitService gitService, IConsoleWriter console)
        {
            _gitService = gitService;
            _console = console;
        }

        public void Run(SharedData sharedData)
        {
            _console.WriteLine("*** Fixing legacy tracking information...");

            var svnBranches = sharedData.RemoteBranches
                .Where(b => b.StartsWith("svn/") && !sharedData.Tags.Contains(b))
                .ToList();

            bool cannotSetupTrackingInformation = false;
            bool legacySvnBranchTrackingMessageDisplayed = false;

            foreach (var branch in svnBranches)
            {
                var trimmedBranch = branch.Replace("svn/", "").Trim();
                bool containsTrimmedBranch = sharedData.LocalBranches.Contains(trimmedBranch);

                if (containsTrimmedBranch || trimmedBranch == "trunk")
                {
                    continue;
                }

                if (cannotSetupTrackingInformation)
                {
                    _gitService.CheckoutAndCreateBranchFromSvn(trimmedBranch);
                }
                else
                {
                    var status = _gitService.GetBranchTrackInfo(trimmedBranch);
                    if (Regex.IsMatch(
                            status,
                            "Cannot setup tracking information",
                            RegexOptions.Multiline))
                    {
                        cannotSetupTrackingInformation = true;
                        _gitService.CheckoutAndCreateBranchFromSvn(trimmedBranch);
                    }
                    else
                    {
                        if (!legacySvnBranchTrackingMessageDisplayed)
                        {
                            _console.WriteLine(WarningMessage);
                            legacySvnBranchTrackingMessageDisplayed = true;
                        }

                        _gitService.Checkout(trimmedBranch, false);
                    }
                }
            }
        }
    }
}
