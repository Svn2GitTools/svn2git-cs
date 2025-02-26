using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole
{
    public class StartupVerificationService
    {
        private readonly IArgumentParser _argumentParser;

        private readonly IConsoleWriter _consoleWriter;

        private readonly MigrationCommands _commands;

        private readonly IGitService _gitService;

        private MigrationOptions Options { get; set; }

        public StartupVerificationService(
            IGitService gitService,
            IArgumentParser argumentParser,
            IConsoleWriter consoleWriter,
            MigrationOptions options, MigrationCommands commands)
        {
            _gitService = gitService;
            _argumentParser = argumentParser;
            _consoleWriter = consoleWriter;
            _commands = commands;

            Options = options;
        }

        public void Check(string[] args)
        {
            if (_commands.Rebase)
            {
                //if (args.Length > 0)
                //{
                //    ShowHelpMessage("Too many arguments");
                //}

                VerifyWorkingTreeIsClean();
            }
            else if (Options.RebaseBranch != null)
            {
                //if (args.Length > 0)
                //{
                //    ShowHelpMessage("Too many arguments");
                //}

                VerifyWorkingTreeIsClean();
            }
            else
            {
                if (args.Length == 0)
                {
                    ShowHelpMessage("Missing SVN_URL parameter");
                }
            }
        }

        private void ShowHelpMessage(string msg)
        {
            if (!string.IsNullOrEmpty(msg))
            {
                _consoleWriter.WriteLine($"Error starting script: {msg}\n");
            }
            else
            {
                _consoleWriter.WriteLine(_argumentParser.GetHelpMessage);
            }

            throw new InvalidOperationException(msg); // Throw exception instead of exiting
        }

        private void VerifyWorkingTreeIsClean()
        {
            var status = _gitService.GetWorkingDirectoryStatus();
            if (!string.IsNullOrWhiteSpace(status))
            {
                throw new InvalidOperationException(
                    "You have local pending changes. The working tree must be clean in order to continue.");
            }
        }
    }
}
