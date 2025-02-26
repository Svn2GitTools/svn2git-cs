namespace Svn2GitConsole
{
    public class SharedData
    {
        public MigrationOptions Options { get; set; }

        public List<string> LocalBranches { get; set; } = new();

        public List<string> RemoteBranches { get; set; } = new();

        public List<string> Tags { get; set; } = new();
    }
}
