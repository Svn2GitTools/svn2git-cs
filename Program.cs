using Svn2GitConsole.Actions;
using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            
            IConsoleWriter consoleWriter = new ConsoleWriter();
            IArgumentParser argumentParser = new ArgumentParser(consoleWriter);

            try
            {
                var (options, commands) = argumentParser.Parse(args);
                if (argumentParser.ShouldExitEarly(options, commands))
                {
                    return 0;
                }
                SharedData sharedData = new() { Options = options };
                string? gitRepoPath = null;
                if (options.OutputDirectory != null)
                {
                    gitRepoPath = Path.Combine(options.OutputDirectory, options.GetSvnRepoName);
                    // Check if the directory exists
                    if (!Directory.Exists(gitRepoPath))
                    {
                        try
                        {
                            // Create the directory if it doesn't exist
                            Directory.CreateDirectory(gitRepoPath);
                            Console.WriteLine($"Created directory: {gitRepoPath}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error creating directory: {ex.Message}");
                            return 1;
                        }
                    }
                }

                IProcessRunner processRunner = new ProcessRunner(gitRepoPath);

                IGitService gitService = new GitService(processRunner, consoleWriter, options);

                IMigrationStep createAuthorsFile = new CreateAuthorsFile(processRunner, consoleWriter);

                if (commands.CreateAutorsFile)
                {
                    createAuthorsFile.Run(sharedData);
                    return 0;
                }
                IMigrationStep gitSvnInstallationChecker =
                    new GitSvnInstallationChecker(processRunner, consoleWriter);
                IMigrationStep svnCloner = new SvnCloner(gitService, options);
                IMigrationStep tagFixer = new TagFixer(gitService, consoleWriter);
                IMigrationStep convertSvnTagsToGitTags =
                    new ConvertSvnTagsToGitTags(gitService, consoleWriter);
                IMigrationStep trunkFixer = new TrunkFixer(gitService, consoleWriter);
                IMigrationStep trunkFixerRebase = new TrunkFixerRebase(gitService, consoleWriter);
                IMigrationStep repoOptimizer = new RepoOptimizer(gitService, consoleWriter);
                IMigrationStep rebaseBranchFixer = new RebaseBranchFixer(gitService, consoleWriter);
                IMigrationStep legacyBranchFixer = new LegacyBranchFixer(gitService, consoleWriter);

                IMigrationStep getBranchesStep = new GetBranchesStep(gitService, consoleWriter);
                IMigrationStep rebaseBranchStep =
                    new GetRebaseBranchStep(gitService, consoleWriter, options);

                List<IMigrationStep> migrationSteps = new();
                migrationSteps.Add(gitSvnInstallationChecker);
                if (commands.Rebase)
                {
                    migrationSteps.Add(getBranchesStep);
                    migrationSteps.Add(rebaseBranchFixer);
                    migrationSteps.Add(tagFixer);
                    migrationSteps.Add(trunkFixerRebase);
                }
                else if (commands.RebaseBranch)
                {
                    migrationSteps.Add(getBranchesStep);
                    migrationSteps.Add(rebaseBranchStep);
                    migrationSteps.Add(legacyBranchFixer);
                    migrationSteps.Add(tagFixer);
                    migrationSteps.Add(trunkFixer);
                }
                else
                {
                    //migrationSteps.Add(svnCloner);
                    //migrationSteps.Add(getBranchesStep);
                    //migrationSteps.Add(legacyBranchFixer);

                    //migrationSteps.Add(tagFixer);
                    migrationSteps.Add(convertSvnTagsToGitTags);
                    migrationSteps.Add(trunkFixer);
                }

                migrationSteps.Add(repoOptimizer);

                StartupVerificationService verificationService = new(
                    gitService,
                    argumentParser,
                    consoleWriter,
                    options,
                    commands);

                verificationService.Check(args);

              
                foreach (var step in migrationSteps)
                {
                    step.Run(sharedData);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }

            return 0;
        }
    }
}
