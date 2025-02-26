namespace Svn2GitConsole.Interfaces;

public interface IArgumentParser
{
    string GetHelpMessage { get; }

    (MigrationOptions options, MigrationCommands commands) Parse(string[] args);

    bool ShouldExitEarly(MigrationOptions options, MigrationCommands commands);
}
