using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole
{
    public class GitService : IGitService
    {
        private readonly IConsoleWriter _consoleWriter;

        private readonly MigrationOptions _options;

        private readonly IProcessRunner _processRunner;

        private string _gitConfigCommand;

        public GitService(
            IProcessRunner processRunner,
            IConsoleWriter consoleWriter,
            MigrationOptions options)
        {
            _processRunner = processRunner;
            _consoleWriter = consoleWriter;
            _options = options;
        }

        /// <summary>
        /// Checkouts the specified branch. Switches to an existing branch
        /// </summary>
        /// <param name="branch">The branch.</param>
        /// <param name="force">if set to <c>true</c> [force].</param>
        public void Checkout(string branch, bool force)
        {
            string command = force ? $"git checkout -f \"{branch}\"" : $"git checkout \"{branch}\"";
            RunCommand(command);
        }

        /// <summary>
        /// Creates a new branch and switches to it.
        /// </summary>
        /// <param name="branch">The branch.</param>
        public void CheckoutAndCreateBranch(string branch)
        {
            RunCommand($"git checkout -f -b {branch}");
        }

        /// <summary>
        /// Creates a new branch from a specific SVN remote and switches to it.
        /// </summary>
        /// <param name="branch">The branch.</param>
        public void CheckoutAndCreateBranchFromSvn(string branch)
        {
            RunCommand($"git checkout -b \"{branch}\" \"remotes/svn/{branch}\"");
        }

        /// <summary>
        /// Applies a commit from another branch to the current branch using git cherry-pick.
        /// This effectively copies the changes introduced by the specified commit into the current working branch.
        /// </summary>
        /// <param name="commitHash">The SHA-1 hash of the commit to cherry-pick.  This should be a 40-character hexadecimal string.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="commitHash"/> is null or empty.</exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="commitHash"/> is not a valid SHA-1 hash (not 40 hexadecimal characters).</exception>
        /// <exception cref="InvalidOperationException">Thrown if the cherry-pick operation fails (e.g., due to conflicts).  Check the output of the git command for details.</exception>
        public void CherryPick(string commitHash)
        {
            if (string.IsNullOrEmpty(commitHash))
            {
                throw new ArgumentNullException(
                    nameof(commitHash),
                    "Commit hash cannot be null or empty.");
            }

            if (!IsValidSha1Hash(commitHash)) // Helper function to validate the hash
            {
                throw new ArgumentException(
                    "Invalid commit hash.  Must be a 40-character hexadecimal string.",
                    nameof(commitHash));
            }

            try
            {
                RunCommand($"git cherry-pick {commitHash}");
            }
            catch (Exception ex) // Catch potential exceptions from RunCommand
            {
                throw new InvalidOperationException(
                    $"Cherry-pick failed for commit {commitHash}. See inner exception for details.",
                    ex); // Wrap in a more informative exception
            }
        }

        public void CherryPickMerge(string commit, int parentNumber = 1)
        {
            RunCommand($"git cherry-pick -m {parentNumber} {commit}");
        }

        public void CreateBareRepo()
        {
            RunCommand("git init --bare");
        }

        public void CreateTag(string tagName, string revision, string message)
        {
            RunCommand(
                $"git tag -a -m \"{StringUtilities.EscapeQuotes(message)}\" \"{StringUtilities.EscapeQuotes(tagName)}\" \"{StringUtilities.EscapeQuotes(revision)}\"");
        }

        public void CreateTagFromSvnRemote(string tagName, string? message)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentNullException(
                    nameof(tagName),
                    "Tag name cannot be null or empty.");
            }

            if (message == null) // Message can be empty, but not null
            {
                message = ""; // or a default message: "Converted from SVN tag"
            }

            try
            {
                // Construct the git tag command.  Escape quotes for safety.
                string command =
                    $"git tag -a \"{StringUtilities.EscapeQuotes(tagName)}\" -m \"{StringUtilities.EscapeQuotes(message)}\" $(git rev-list -n 1 \"refs/remotes/{StringUtilities.EscapeQuotes(tagName)}\")";

                RunCommand(command);
            }
            catch (InvalidOperationException ex) // Catch potential exceptions from RunCommand
            {
                throw new InvalidOperationException(
                    $"Creating tag '{tagName}' from SVN remote failed. See inner exception for details.",
                    ex); // Wrap in a more informative exception
            }
            catch (Exception ex) // Catch any other unexpected exceptions
            {
                throw new InvalidOperationException(
                    $"An unexpected error occurred while creating tag '{tagName}' from SVN remote.",
                    ex);
            }
        }

        public void CreateTagWithCommit(string tagName, string commit, string message)
        {
            RunCommand(
                $"git tag -a \"{StringUtilities.EscapeQuotes(tagName)}\" {StringUtilities.EscapeQuotes(commit)} -m \"{StringUtilities.EscapeQuotes(message)}\"");
        }

        public void DeleteBranch(string branch, bool remote)
        {
            RunCommand($"git branch -D {(remote ? "-r" : "")} \"{branch}\"");
        }

        public void Fetch()
        {
            RunCommand("git svn fetch");
        }

        public string FindEquivalentCommitOnMaster(string commit)
        {
            // Use git merge-base to find the best common ancestor between the commit and master
            string mergeBase = RunCommand($"git merge-base {commit} master");

            if (!string.IsNullOrEmpty(mergeBase))
            {
                return mergeBase.Trim(); // Return the commit hash
            }

            return null; // No equivalent commit found
        }

        public string FindMergeBase(string commit1, string commit2)
        {
            return RunCommand($"git merge-base {commit1} {commit2}").Trim();
        }

        public void GarbageCollect()
        {
            RunCommand("git gc");
        }

        public string GetBranchTrackInfo(string branch)
        {
            return RunCommand(
                $"git branch --track \"{branch}\" \"remotes/svn/{branch}\"",
                false);
        }

        public string GetCommitAuthorEmail(string revision)
        {
            return RunCommand(
                    $"git log -1 --pretty=format:'%ae' \"{StringUtilities.EscapeQuotes(revision)}\"")
                .Trim('\'');
        }

        public string GetCommitAuthorName(string revision)
        {
            return RunCommand(
                    $"git log -1 --pretty=format:'%an' \"{StringUtilities.EscapeQuotes(revision)}\"")
                .Trim('\'');
        }

        public string GetCommitDate(string revision)
        {
            return RunCommand(
                    $"git log -1 --pretty=format:'%ci' \"{StringUtilities.EscapeQuotes(revision)}\"")
                .Trim('\'');
        }

        public string GetCommitSubject(string revision)
        {
            return RunCommand(
                    $"git log -1 --pretty=format:'%s' \"{StringUtilities.EscapeQuotes(revision)}\"")
                .Trim('\'');
        }

        public string GetConfigValue(string key)
        {
            return RunCommand($"{GetGitConfigCommand()} --get {key}", false);
        }

        public string GetGitConfigCommand()
        {
            if (string.IsNullOrEmpty(_gitConfigCommand))
            {
                var status = RunCommand("git config --local --get user.name", false);
                _gitConfigCommand = status.Contains("unknown option")
                                        ? "git config"
                                        : "git config --local";
            }

            return _gitConfigCommand;
        }

        public List<string> GetLocalBranches()
        {
            var runGitCommandResult = RunCommand("git branch -l --no-color");
            return runGitCommandResult
                .Split("\n")
                .Select(b => b.Replace("*", "").Trim())
                .Where(b => !string.IsNullOrEmpty(b))
                .ToList();
        }

        public List<string> GetRemoteBranches()
        {
            var runGitCommandResult = RunCommand("git branch -r --no-color");
            return runGitCommandResult
                .Split("\n")
                .Select(b => b.Replace("*", "").Trim())
                .Where(b => !string.IsNullOrEmpty(b))
                .ToList();
        }

        public List<string> GetTagsListWithHashes(string tagsPath)
        {
            string tagRefs = RunCommand(
                $"git for-each-ref --format=\"%(refname:short) %(objectname)\" {tagsPath}",
                true,
                true);
            return tagRefs
                .Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries)
                .ToList();
        }

        public string GetUserEmail()
        {
            return GetConfigValue("user.email");
        }

        public string GetUserName()
        {
            return GetConfigValue("user.name");
        }

        public string GetWorkingDirectoryStatus()
        {
            return RunCommand("git status --porcelain --untracked-files=no");
        }

        public void Log(string msg)
        {
            if (_options.Verbose)
            {
                _consoleWriter.Write(msg);
            }
        }

        public void RebaseBranch(string remoteBranch)
        {
            RunCommand($"git rebase \"remotes/svn/{remoteBranch}\"");
        }

        public string RunCommand(
            string command,
            bool exitOnError = true,
            bool printoutOutput = false)
        {
            Log($"Running command: {command}\n");
            return _processRunner.Run(command, exitOnError, printoutOutput);
        }

        public void SetAuthorsFile(string authorsFile)
        {
            RunCommand($"{GetGitConfigCommand()} svn.authorsfile {authorsFile}");
        }

        public void SetConfigValue(string key, string value)
        {
            RunCommand(
                $"{GetGitConfigCommand()} {key} \"{StringUtilities.EscapeQuotes(value)}\"");
        }

        public void SetUserEmail(string userEmail)
        {
            RunCommand(
                $"{GetGitConfigCommand()} user.email \"{StringUtilities.EscapeQuotes(userEmail)}\"");
        }

        public void SetUserName(string userName)
        {
            RunCommand(
                $"{GetGitConfigCommand()} user.name \"{StringUtilities.EscapeQuotes(userName)}\"");
        }

        public void UnsetConfigValue(string key)
        {
            RunCommand($"{GetGitConfigCommand()} --unset {key}");
        }

        public bool VerifyTag(string tagName, string expectedCommit)
        {
            string actualCommit = RunCommand($"git rev-parse {tagName}").Trim();
            return actualCommit == expectedCommit;
        }

        private static bool IsHexadecimal(string hash)
        {
            foreach (char c in hash)
            {
                if (!((c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')))
                {
                    return false;
                }
            }

            return true;
        }

        // Helper function to validate SHA-1 hash (you might already have something similar)
        private static bool IsValidSha1Hash(string hash)
        {
            return !string.IsNullOrEmpty(hash) && hash.Length == 40 && IsHexadecimal(hash);
        }
    }
}
