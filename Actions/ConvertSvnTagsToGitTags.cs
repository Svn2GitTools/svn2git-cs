using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions;

public class ConvertSvnTagsToGitTags : IMigrationStep
{
    private readonly IConsoleWriter _console;

    private readonly IGitService _gitService;

    public ConvertSvnTagsToGitTags(IGitService gitService, IConsoleWriter console)
    {
        _gitService = gitService;
        _console = console;
    }

    public void Run(SharedData sharedData)
    {
        _console.WriteLine("*** Convert Svn tags to Git tags...");

        var tagLines = _gitService.GetTagsListWithHashes("refs/remotes");
        Dictionary<string, int> tagCounts = new Dictionary<string, int>();

        foreach (string tagLine in tagLines)
        {
            string[] parts = tagLine.Split(new[] { ' ' }, 2);
            if (parts.Length == 2)
            {
                string originalTag = parts[0];
                string commit = parts[1];

                // Extract the actual tag name
                string tagName = originalTag.Split(new[] { '/' }).Last();

                // Remove @revision from tag name, but keep it for uniqueness
                string cleanTag = System.Text.RegularExpressions.Regex.Replace(
                    tagName,
                    @"@(\d+)$",
                    match => $"_r{match.Groups[1].Value}");

                if (tagCounts.ContainsKey(cleanTag))
                {
                    tagCounts[cleanTag]++;
                    cleanTag = $"{cleanTag}_{tagCounts[cleanTag]}";
                }
                else
                {
                    tagCounts[cleanTag] = 0;
                }

                // Create new annotated tag
                //RunGitCommand(
                //    $"tag -a \"{cleanTag}\" {commit} -m \"Converted from SVN: {originalTag}\"",
                //    repoPath);
                if (cleanTag != "trunk")
                {
                    _gitService.CreateTagWithCommit(
                        cleanTag,
                        commit,
                        $"Converted from SVN: {originalTag}");
                }

                // Delete old tag branch (remote-tracking branch)
                string branchToDelete = originalTag.StartsWith("remotes/")
                                            ? originalTag.Substring(8)
                                            : originalTag;
                //_gitService.DeleteBranch(branchToDelete, true);

                _console.WriteLine($"Converted tag: {originalTag} -> {cleanTag}");
            }
        }
    }
}
