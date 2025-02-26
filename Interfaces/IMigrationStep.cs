namespace Svn2GitConsole.Interfaces
{
    public interface IMigrationStep
    {
        void Run(SharedData sharedData);
    }
}
