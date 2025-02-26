using System.Text.RegularExpressions;

using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions;

public class CreateAuthorsFile : IMigrationStep
{
    private readonly IConsoleWriter _console;

    private readonly IProcessRunner _processRunner;

    public CreateAuthorsFile(IProcessRunner processRunner, IConsoleWriter console)
    {
        _processRunner = processRunner;
        _console = console;
    }

    public void Run(SharedData sharedData)
    {
        _console.WriteLine("*** Getting authors from SVN repository...");

        if (string.IsNullOrEmpty(sharedData.Options.SvnRepoUrl))
        {
            throw new MigrationException("SVN repository URL is required.");
        }

        string tempLogFile = "svn_log_temp.txt";

        string authorsFile = GetAuthorsFile(sharedData);

        try
        {
            // Get SVN log and save to temporary file
            _processRunner.Run(
                $"svn log --quiet {sharedData.Options.SvnRepoUrl} > {tempLogFile}",
                printOutput: true);

            // Read the log file and extract authors
            var authors = File.ReadAllLines(tempLogFile)
                .Where(line => Regex.IsMatch(line, @"^r\d+"))
                .Select(line => line.Split('|')[1].Trim())
                .Distinct()
                .Where(author => !string.IsNullOrEmpty(author))
                .OrderBy(author => author);

            // Write authors to file
            using (StreamWriter writer = new(authorsFile))
            {
                foreach (var author in authors)
                {
                    writer.WriteLine($"{author} = {author} <{author}@{sharedData.Options.EmailDomain}>");
                }
            }

            _console.WriteLine($"Authors have been saved to {authorsFile}.");
        }
        catch (Exception ex)
        {
            throw new MigrationException("Failed to get authors from SVN repository.", ex);
        }
        finally
        {
            // Clean up temporary file
            if (File.Exists(tempLogFile))
            {
                File.Delete(tempLogFile);
            }
        }
    }

    private static string GetAuthorsFile(SharedData sharedData)
    {
        string authorsFile;
        if (sharedData.Options.Authors != null)
        {
            // Check if the provided path is absolute
            if (Path.IsPathRooted(sharedData.Options.Authors))
            {
                authorsFile = sharedData.Options.Authors;
            }
            else
            {
                // If it's a relative path, combine it with the base path
                authorsFile = Path.Combine(AppContext.BaseDirectory, sharedData.Options.Authors);
            }
        }
        else
        {
            // Use the default authors file if no custom path is provided
            authorsFile = Path.Combine(AppContext.BaseDirectory, ArgumentParser.DefaultAuthorsFile);
        }

        return authorsFile;
    }
}
