using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions;

public class TagFixer : IMigrationStep
{
    private readonly IGitService _gitService;

    private readonly IConsoleWriter _console;


    public TagFixer(IGitService gitService, IConsoleWriter console)
    {
        _gitService = gitService;
        _console = console;
    }

    public void Run(SharedData sharedData)
    {
        _console.WriteLine("*** Fixing tags...");
        var current = new Dictionary<string, string>();
        try
        {
            current["user.name"] = _gitService.GetUserName();
            current["user.email"] = _gitService.GetUserEmail();
            //var tags = _gitService.GetRemoteBranches()
            //    .FindAll(b => Regex.IsMatch(b.Trim(), "^svn\\/tags\\/")).ToList();
            foreach (var tag in sharedData.Tags)
            {
                var trimmedTag = tag.Trim();
                var id = trimmedTag.Replace("svn/tags/", "").Trim();
                //var subject =
                //    StringUtilities.ReverseString(_gitService.GetCommitSubject(trimmedTag));
                //var date = StringUtilities.ReverseString(_gitService.GetCommitDate(trimmedTag));
                //var author =
                //    StringUtilities.ReverseString(_gitService.GetCommitAuthorName(trimmedTag));
                //var email =
                //    StringUtilities.ReverseString(_gitService.GetCommitAuthorEmail(trimmedTag));
                var subject = _gitService.GetCommitSubject(trimmedTag);
                var date = _gitService.GetCommitDate(trimmedTag);
                var author = _gitService.GetCommitAuthorName(trimmedTag);
                var email = _gitService.GetCommitAuthorEmail(trimmedTag);
                _gitService.SetUserName(author);
                _gitService.SetUserEmail(email);
                var originalGitCommitterDate =
                    Environment.GetEnvironmentVariable("GIT_COMMITTER_DATE");
                Environment.SetEnvironmentVariable(
                    "GIT_COMMITTER_DATE",
                    StringUtilities.EscapeQuotes(date));
                _gitService.CreateTag(id, trimmedTag, subject);

                Environment.SetEnvironmentVariable(
                    "GIT_COMMITTER_DATE",
                    originalGitCommitterDate);
                _gitService.DeleteBranch(trimmedTag, true);
            }
        }
        finally
        {
            if (sharedData.Tags.Count > 0)
            {
                foreach (var pair in current)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Value))
                    {
                        _gitService.SetConfigValue(pair.Key, pair.Value.Trim());
                    }
                    else
                    {
                        _gitService.UnsetConfigValue(pair.Key);
                    }
                }
            }
        }
    }
}