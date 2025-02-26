using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole
{
    public class ArgumentParser : IArgumentParser
    {
        public const string DefaultAuthorsFile = "authors.txt";

        private readonly IConsoleWriter _consoleWriter;

        private const string HelpMessage = @"Usage: svn2git-cs SVN_URL [OUTPUT_DIR] [options]

Specific options:
  --rebase                        Instead of cloning a new project, rebase an existing one against SVN
  --createauthors EMAIL_DOMAIN    Create authors file
  --username NAME                 Username for transports that needs it (http(s), svn)
                                  Password for transports that need it (http(s), svn), must be set over client credential
  --trunk TRUNK_PATH              Subpath to trunk from repository URL (default: trunk)
  --branches BRANCHES_PATH        Subpath to branches from repository URL (default: branches); can be used multiple times
  --tags TAGS_PATH                Subpath to tags from repository URL (default: tags); can be used multiple times
  --rootistrunk                   Use this if the root level of the repo is equivalent to the trunk and there are no tags or branches
  --notrunk                       Do not import anything from trunk
  --nobranches                    Do not try to import any branches
  --notags                        Do not try to import any tags
  --no-minimize-url               Accept URLs as-is without attempting to connect to a higher level directory
  --revision START_REV[:END_REV]  Start importing from SVN revision START_REV; optionally end at END_REV
  -m, --metadata                  Include metadata in git logs (git-svn-id)
  --authors AUTHORS_FILE          Path to file containing svn-to-git authors mapping (default: authors.txt)
  --exclude REGEX                 Specify a Perl regular expression to filter paths when fetching; can be used multiple times
 -v, --verbose                   Be verbose in logging -- useful for debugging issues
  --rebasebranch REBASEBRANCH    Rebase specified branch.

  -h, --help                      Show this message";

        public ArgumentParser(IConsoleWriter consoleWriter)
        {
            _consoleWriter = consoleWriter;
        }

        public (MigrationOptions options, MigrationCommands commands) Parse(string[] args)
        {
            var options = new MigrationOptions
                              {
                                  Verbose = false,
                                  Metadata = false,
                                  NoMinimizeUrl = false,
                                  RootIsTrunk = false,
                                  Trunk = "trunk",
                                  Branches = new List<string>(),
                                  Tags = new List<string>(),
                                  Exclude = new List<string>(),
                                  Revision = null,
                                  Username = null,
                                  RebaseBranch = null
            };
            var commands = new MigrationCommands
                               {
                                   Rebase = false, CreateAutorsFile = false, RebaseBranch = false
                               };

            var path = Environment.ExpandEnvironmentVariables(DefaultAuthorsFile);
            if (File.Exists(path))
            {
                options.Authors = Path.Combine(AppContext.BaseDirectory, ArgumentParser.DefaultAuthorsFile);
            }

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--rebase":
                        commands.Rebase = true;
                        break;
                    case "--createauthors":
                        commands.CreateAutorsFile = true;
                        if (i + 1 < args.Length)
                        {
                            options.EmailDomain = args[i + 1];
                        }

                        i++;
                        break;
                    case "--username":
                        if (i + 1 < args.Length)
                        {
                            options.Username = args[i + 1];
                        }

                        i++;
                        break;
                    case "--trunk":
                        if (i + 1 < args.Length)
                        {
                            options.Trunk = args[i + 1];
                        }

                        i++;
                        break;
                    case "--branches":
                        if (i + 1 < args.Length)
                        {
                            options.Branches.Add(args[i + 1]);
                        }

                        i++;
                        break;
                    case "--tags":
                        if (i + 1 < args.Length)
                        {
                            options.Tags.Add(args[i + 1]);
                        }

                        i++;
                        break;
                    case "--rootistrunk":
                        options.RootIsTrunk = true;
                        options.Trunk = null;
                        options.Branches = null;
                        options.Tags = null;
                        break;
                    case "--notrunk":
                        options.Trunk = null;
                        break;
                    case "--nobranches":
                        options.Branches = null;
                        break;
                    case "--notags":
                        options.Tags = null;
                        break;
                    case "--no-minimize-url":
                        options.NoMinimizeUrl = true;
                        break;
                    case "--revision":
                        if (i + 1 < args.Length)
                        {
                            options.Revision = args[i + 1];
                        }

                        i++;
                        break;
                    case "-m":
                    case "--metadata":
                        options.Metadata = true;
                        break;
                    case "--authors":
                        if (i + 1 < args.Length)
                        {
                            options.Authors = args[i + 1];
                        }

                        i++;
                        break;
                    case "--exclude":
                        if (i + 1 < args.Length)
                        {
                            options.Exclude.Add(args[i + 1]);
                        }

                        i++;
                        break;
                    case "-v":
                    case "--verbose":
                        options.Verbose = true;
                        break;
                    case "--rebasebranch":
                        if (i + 1 < args.Length)
                        {
                            options.RebaseBranch = args[i + 1];
                        }

                        i++;
                        commands.RebaseBranch = options.RebaseBranch != null;
                        break;
                    case "-h":
                    case "--help":
                        _consoleWriter.WriteLine(HelpMessage);
                        break;
                    default:
                        if (args[i].StartsWith("--"))
                        {
                            var unknownOptionMessage =
                                $"Error starting script: Unknown option: {args[i]}\n";
                            _consoleWriter.WriteLine(unknownOptionMessage);
                        }

                        break;
                }
            }

            if (args.Length > 0)
            {
                options.SvnRepoUrl = args[0].Replace(" ", "\\ ");
            }
            if (args.Length > 1)
            {
                string outputDirectory = args[1];
                if (!outputDirectory.StartsWith("-"))
                {
                    try
                    {
                        if (!Directory.Exists(outputDirectory))
                        {
                            Directory.CreateDirectory(outputDirectory);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log the error or display a message to the user
                        Console.WriteLine($"Error creating directory: {ex.Message}");
                    }
                    options.OutputDirectory = outputDirectory;
                }
            }
            return (options, commands);
        }

        public bool ShouldExitEarly(MigrationOptions options, MigrationCommands commands)
        {
            return string.IsNullOrEmpty(options.Trunk)
                   && string.IsNullOrEmpty(options.RebaseBranch) 
                   && !commands.Rebase
                   && string.IsNullOrEmpty(options.Username);
        }

        public string GetHelpMessage => HelpMessage;
    }
}
