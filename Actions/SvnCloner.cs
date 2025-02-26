using Svn2GitConsole.Interfaces;

namespace Svn2GitConsole.Actions
{
    public class SvnCloner : IMigrationStep
    {
        private readonly IGitService _gitService;
        private readonly MigrationOptions _options;

        public SvnCloner(IGitService gitService, MigrationOptions options)
        {
            _gitService = gitService;
            _options = options;
        }
        public void Run(SharedData sharedData)
        {
            string trunk = _options.Trunk;
            List<string> branches = _options.Branches;
            List<string> tags = _options.Tags;
            bool rootIsTrunk = _options.RootIsTrunk;
            string authors = _options.Authors;

            if (false)
            {
                _gitService.CreateBareRepo();
            }

            string cmd;
            cmd = "git svn init --prefix=svn/ ";
            cmd += _options.Username != null ? $"--username='{_options.Username}' " : "";
            cmd += _options.Metadata ? "" : "--no-metadata ";
            cmd += _options.NoMinimizeUrl ? "--no-minimize-url " : "";

            if (rootIsTrunk)
            {
                cmd += $"--trunk='{_options.SvnRepoUrl}'";
                //cmd += $"-T {_options.Url}";
                _gitService.RunCommand(cmd, true, true);
            }
            else
            {
                cmd += trunk != null ? $"--trunk {trunk} " : "";
                //cmd += trunk != null ? $"-T {trunk} " : "";
                if (tags != null)
                {
                    if (tags.Count == 0)
                    {
                        tags = new List<string> { "tags" };
                    }

                    foreach (var tag in tags)
                    {
                        cmd += $"-t {tag} ";
                    }
                }

                if (branches != null)
                {
                    if (branches.Count == 0)
                    {
                        branches = new List<string> { "branches" };
                    }

                    foreach (var branch in branches)
                    {
                        cmd += $"-b {branch} ";
                    }
                }

                cmd += _options.SvnRepoUrl;
                _gitService.RunCommand(cmd, true, true);
            }

            if (!string.IsNullOrEmpty(authors))
            {
                _gitService.SetAuthorsFile(authors);
            }

            cmd = "git svn fetch ";
            if (_options.Verbose)
            {
                //cmd += "--log-window-size=50 ";
                //Environment.SetEnvironmentVariable("GIT_TRACE", "1");
            }
            if (!string.IsNullOrEmpty(_options.Revision))
            {
                var range = _options.Revision.Split(":");
                if (range.Length < 2)
                {
                    range = new[] { range[0], "HEAD" };
                }

                cmd += $"-r {range[0]}:{range[1]} ";
            }

            if (_options.Exclude.Count > 0)
            {
                List<string> regex = new List<string>();
                if (!rootIsTrunk)
                {
                    if (trunk != null)
                    {
                        regex.Add($"{trunk}[/]");
                    }

                    if (tags != null)
                    {
                        foreach (var tag in tags)
                        {
                            regex.Add($"{tag}[/][^/]+[/]");
                        }
                    }

                    if (branches != null)
                    {
                        foreach (var branch in branches)
                        {
                            regex.Add($"{branch}[/][^/]+[/]");
                        }
                    }
                }

                var combinedRegex = "^(?:" + string.Join("|", regex) + ")(?:"
                                    + string.Join("|", _options.Exclude) + ")";
                cmd += $"--ignore-paths='{combinedRegex}' ";
            }

            _gitService.RunCommand(cmd, true, true);
            //if (_options.Verbose)
            //{
            //    Environment.SetEnvironmentVariable("GIT_TRACE", null);
            //}

            //_gitService.GetLocalBranches();
        }
    }
}